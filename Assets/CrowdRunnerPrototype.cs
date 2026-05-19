using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CrowdRunnerBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
       /* if (FindObjectOfType<CrowdRunnerGame>() != null)
        {
            return;
        }

        new GameObject("Crowd Runner Game").AddComponent<CrowdRunnerGame>(); */
    }
}

public sealed class CrowdRunnerGame : MonoBehaviour
{
    private const float TrackLength = 120f;
    private const float TrackWidth = 8f;
    private const float FinishZ = 108f;

    [SerializeField] private int baseStartUnits = 4;
    [SerializeField] private float forwardSpeed = 48f;
    [SerializeField] private float horizontalLimit = 3.4f;
    [SerializeField] private float dragSensitivity = 0.018f;

    private readonly List<GameObject> levelObjects = new List<GameObject>();
    private readonly Dictionary<int, List<Gate>> gateGroups = new Dictionary<int, List<Gate>>();
    private readonly HashSet<int> consumedGateGroups = new HashSet<int>();
    private Material blueMaterial;
    private Material redMaterial;
    private Material gateMaterial;
    private Material trackMaterial;
    private Material obstacleMaterial;
    private Material goldMaterial;
    private Material whiteMaterial;
    private CrowdController crowd;
    private Camera mainCamera;
    private Canvas canvas;
    private Text countText;
    private Text goldText;
    private Text gemText;
    private Text stateText;
    private Text incomeText;
    private Text startUnitsText;
    private Text resultText;
    private Slider progressSlider;
    private GameObject readyPanel;
    private GameObject resultPanel;
    private int nextGateGroupId;
    private int gold;
    private int gems = 25;
    private int startUnitsLevel = 1;
    private int incomeLevel = 1;
    private bool isRunning;
    private bool isFinished;

    private int StartUnits => baseStartUnits + (startUnitsLevel - 1) * 3;
    private float IncomeMultiplier => 1f + (incomeLevel - 1) * 0.35f;

    private void Awake()
    {
        BuildMaterials();
        BuildCameraAndLight();
        BuildLevel();
        BuildUi();
        ResetRun();
    }

    private void Update()
    {
        if (!isRunning || isFinished || crowd == null)
        {
            return;
        }

        crowd.Move(forwardSpeed, horizontalLimit, dragSensitivity);
        UpdateHud();

        if (crowd.Count <= 0)
        {
            FinishRun(false);
        }
    }

    private void LateUpdate()
    {
        FollowCamera(false);
    }

    public void ApplyGate(GateOperation operation, int value)
    {
        if (!isRunning || isFinished || crowd == null)
        {
            return;
        }

        int before = crowd.Count;
        int target = operation == GateOperation.Add ? before + value : before * value;
        crowd.SetCount(Mathf.Clamp(target, 0, 180));
        SpawnPopText(crowd.transform.position + Vector3.up * 3.2f, (target - before) >= 0 ? "+" + (target - before) : (target - before).ToString(), Color.cyan);
        UpdateHud();
    }

    public bool TryConsumeGateGroup(int gateGroupId)
    {
        if (consumedGateGroups.Contains(gateGroupId))
        {
            return false;
        }

        consumedGateGroups.Add(gateGroupId);
        if (gateGroups.TryGetValue(gateGroupId, out List<Gate> gates))
        {
            for (int i = 0; i < gates.Count; i++)
            {
                gates[i].Hide();
            }
        }

        return true;
    }

    public void ResolveEnemy(EnemyMob enemy)
    {
        if (!isRunning || isFinished || crowd == null || enemy == null || enemy.Remaining <= 0)
        {
            return;
        }

        int losses = Mathf.Min(crowd.Count, enemy.Remaining);
        crowd.RemoveUnits(losses);
        enemy.RemoveUnits(losses);
        SpawnPopText(enemy.transform.position + Vector3.up * 2.8f, "-" + losses, Color.red);
        UpdateHud();
    }

    public void DamageCrowd(int amount, Vector3 worldPosition)
    {
        if (!isRunning || isFinished || crowd == null || amount <= 0)
        {
            return;
        }

        int losses = Mathf.Min(crowd.Count, amount);
        crowd.RemoveUnits(losses);
        SpawnPopText(worldPosition + Vector3.up * 2.6f, "-" + losses, new Color(0.85f, 0.1f, 1f));
        UpdateHud();
    }

    public void ReachFinish()
    {
        if (isFinished)
        {
            return;
        }

        FinishRun(true);
    }

    private void BuildMaterials()
    {
        blueMaterial = MakeMaterial(new Color(0.05f, 0.45f, 1f));
        redMaterial = MakeMaterial(new Color(1f, 0.08f, 0.05f));
        trackMaterial = MakeMaterial(new Color(0.55f, 0.58f, 0.62f));
        obstacleMaterial = MakeMaterial(new Color(0.55f, 0.12f, 0.85f));
        goldMaterial = MakeMaterial(new Color(1f, 0.72f, 0.08f));
        whiteMaterial = MakeMaterial(Color.white);
        gateMaterial = MakeMaterial(new Color(0.05f, 0.75f, 1f, 0.42f), true);
    }

    private static Material MakeMaterial(Color color, bool transparent = false)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;

        if (transparent)
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        return material;
    }

    private void BuildCameraAndLight()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = new GameObject("Main Camera").AddComponent<Camera>();
            mainCamera.tag = "MainCamera";
        }

        mainCamera.transform.position = new Vector3(0f, 11.5f, -13f);
        mainCamera.transform.rotation = Quaternion.Euler(52f, 0f, 0f);
        mainCamera.fieldOfView = 52f;
        mainCamera.clearFlags = CameraClearFlags.Skybox;

        if (FindObjectOfType<Light>() == null)
        {
            Light light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }

    private void FollowCamera(bool snap)
    {
        if (mainCamera == null || crowd == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(crowd.transform.position.x * 0.35f, 11.5f, crowd.transform.position.z - 16f);
        Quaternion targetRotation = Quaternion.Euler(55f, 0f, 0f);

        if (snap)
        {
            mainCamera.transform.position = targetPosition;
            mainCamera.transform.rotation = targetRotation;
            return;
        }

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * 6f);
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * 8f);
    }

    private void BuildLevel()
    {
        GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
        track.name = "Runner Track";
        track.transform.position = new Vector3(0f, -0.15f, TrackLength * 0.5f);
        track.transform.localScale = new Vector3(TrackWidth, 0.25f, TrackLength);
        track.GetComponent<Renderer>().material = trackMaterial;
        levelObjects.Add(track);

        CreateRail(-TrackWidth * 0.5f - 0.25f);
        CreateRail(TrackWidth * 0.5f + 0.25f);

        CreateGatePair(14f, new GateSpec(GateOperation.Add, 12), new GateSpec(GateOperation.Multiply, 3));
        CreateEnemyMob(new Vector3(0f, 0f, 28f), 12, 4);
        CreateGatePair(42f, new GateSpec(GateOperation.Add, 40), new GateSpec(GateOperation.Multiply, 2));
        CreateSpikeRoller(new Vector3(1.5f, 0.75f, 56f), true);
        CreateGatePair(70f, new GateSpec(GateOperation.Multiply, 3), new GateSpec(GateOperation.Add, 60));
        CreateEnemyMob(new Vector3(-1.5f, 0f, 84f), 36, 7);
        CreateSpikeRoller(new Vector3(-1.7f, 0.75f, 94f), false);
        CreateFinishLine();
        CreateMultiplierSteps();
    }

    private void CreateRail(float x)
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = "Track Rail";
        rail.transform.position = new Vector3(x, 0.35f, TrackLength * 0.5f);
        rail.transform.localScale = new Vector3(0.16f, 0.7f, TrackLength);
        rail.GetComponent<Renderer>().material = whiteMaterial;
        levelObjects.Add(rail);
    }

    private void CreateGatePair(float z, GateSpec left, GateSpec right)
    {
        int gateGroupId = nextGateGroupId++;
        CreateGate(new Vector3(-2f, 1.75f, z), left, gateGroupId);
        CreateGate(new Vector3(2f, 1.75f, z), right, gateGroupId);
    }

    private void CreateGate(Vector3 position, GateSpec spec, int gateGroupId)
    {
        GameObject gateObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gateObject.name = "Gate " + spec.Label;
        gateObject.transform.position = position;
        gateObject.transform.localScale = new Vector3(3.2f, 3.5f, 0.35f);
        gateObject.GetComponent<Renderer>().material = gateMaterial;
        gateObject.GetComponent<Collider>().isTrigger = true;

        Gate gate = gateObject.AddComponent<Gate>();
        gate.Configure(this, spec.Operation, spec.Value, gateGroupId);
        if (!gateGroups.TryGetValue(gateGroupId, out List<Gate> gates))
        {
            gates = new List<Gate>();
            gateGroups.Add(gateGroupId, gates);
        }

        gates.Add(gate);
        levelObjects.Add(gateObject);

        TextMesh label = CreateWorldText(spec.Label, 0.035f, Color.white);
        label.transform.SetParent(gateObject.transform, false);
        label.transform.localPosition = new Vector3(0f, 0.05f, -0.25f);
        label.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void CreateEnemyMob(Vector3 center, int amount, int columns)
    {
        GameObject enemyObject = new GameObject("Red Mob " + amount);
        enemyObject.transform.position = center;
        EnemyMob enemy = enemyObject.AddComponent<EnemyMob>();
        enemy.Configure(this, redMaterial, amount, columns);
        levelObjects.Add(enemyObject);
    }

    private void CreateSpikeRoller(Vector3 center, bool movesRightFirst)
    {
        GameObject roller = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roller.name = "Spike Roller";
        roller.transform.position = center;
        roller.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        roller.transform.localScale = new Vector3(0.72f, 2.6f, 0.72f);
        roller.GetComponent<Renderer>().material = obstacleMaterial;
        roller.GetComponent<Collider>().isTrigger = true;

        ObstacleHazard hazard = roller.AddComponent<ObstacleHazard>();
        hazard.Configure(this, movesRightFirst ? 1f : -1f);
        levelObjects.Add(roller);

        for (int i = 0; i < 8; i++)
        {
            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spike.name = "Roller Spike";
            spike.transform.SetParent(roller.transform, false);
            spike.transform.localPosition = Quaternion.Euler(0f, i * 45f, 0f) * new Vector3(0.55f, 0f, 0f);
            spike.transform.localRotation = Quaternion.Euler(0f, i * 45f, 45f);
            spike.transform.localScale = new Vector3(0.18f, 0.18f, 0.9f);
            spike.GetComponent<Renderer>().material = obstacleMaterial;
            Destroy(spike.GetComponent<Collider>());
        }
    }

    private void CreateFinishLine()
    {
        GameObject finish = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finish.name = "Finish Line";
        finish.transform.position = new Vector3(0f, 1f, FinishZ);
        finish.transform.localScale = new Vector3(TrackWidth, 2f, 0.35f);
        finish.GetComponent<Renderer>().material = goldMaterial;
        finish.GetComponent<Collider>().isTrigger = true;
        finish.AddComponent<FinishZone>().Configure(this);
        levelObjects.Add(finish);

        TextMesh label = CreateWorldText("FINISH", 0.03f, Color.black);
        label.transform.SetParent(finish.transform, false);
        label.transform.localPosition = new Vector3(0f, 0.85f, -0.25f);
    }

    private void CreateMultiplierSteps()
    {
        float z = FinishZ + 6f;
        for (int i = 0; i < 5; i++)
        {
            GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "Multiplier x" + (i + 1);
            step.transform.position = new Vector3(0f, 0.15f + i * 0.22f, z + i * 3.2f);
            step.transform.localScale = new Vector3(TrackWidth - i * 0.8f, 0.3f, 2.8f);
            step.GetComponent<Renderer>().material = MakeMaterial(Color.Lerp(trackMaterial.color, goldMaterial.color, i / 4f));
            levelObjects.Add(step);

            TextMesh label = CreateWorldText("x" + (i + 1) + ".0", 0.025f, Color.black);
            label.transform.SetParent(step.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        }
    }

    private void BuildUi()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        canvas = new GameObject("Crowd Runner UI").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        goldText = CreateUiText("Gold: 0", new Vector2(1f, 1f), new Vector2(-150f, -56f), 34, TextAnchor.MiddleRight, Color.yellow);
        gemText = CreateUiText("Gem: 25", new Vector2(1f, 1f), new Vector2(-150f, -106f), 28, TextAnchor.MiddleRight, Color.cyan);
        countText = CreateUiText("0", new Vector2(0.5f, 0.5f), Vector2.zero, 44, TextAnchor.MiddleCenter, Color.white);
        stateText = CreateUiText("Hold and move", new Vector2(0.5f, 1f), new Vector2(0f, -126f), 34, TextAnchor.MiddleCenter, Color.white);

        progressSlider = CreateSlider(new Vector2(0.5f, 1f), new Vector2(520f, 28f));
        progressSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -62f);
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;

        readyPanel = CreatePanel("Ready Panel", new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(780f, 230f));
        CreateButton(readyPanel.transform, "START RUN", new Vector2(0f, 68f), new Vector2(260f, 64f), StartRun);
        startUnitsText = CreateButton(readyPanel.transform, "Start Units", new Vector2(-205f, -50f), new Vector2(300f, 72f), UpgradeStartUnits);
        incomeText = CreateButton(readyPanel.transform, "Income", new Vector2(205f, -50f), new Vector2(300f, 72f), UpgradeIncome);

        resultPanel = CreatePanel("Result Panel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 320f));
        resultText = CreateUiText("Result", new Vector2(0.5f, 0.5f), new Vector2(0f, 56f), 36, TextAnchor.MiddleCenter, Color.white, resultPanel.transform);
        CreateButton(resultPanel.transform, "NEXT RUN", new Vector2(0f, -104f), new Vector2(260f, 70f), ResetRun);
    }

    private Text CreateUiText(string text, Vector2 anchor, Vector2 anchoredPosition, int size, TextAnchor alignment, Color color, Transform parent = null)
    {
        GameObject textObject = new GameObject(text);
        textObject.transform.SetParent(parent == null ? canvas.transform : parent, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(620f, 90f);
        rect.anchoredPosition = anchoredPosition;

        Text label = textObject.AddComponent<Text>();
        label.font = GetRuntimeFont();
        label.text = text;
        label.fontSize = size;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private Font GetRuntimeFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    private GameObject CreatePanel(string panelName, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(panelName);
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.35f);
        return panel;
    }

    private Text CreateButton(Transform parent, string text, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(text + " Button");
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.55f, 1f, 0.88f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        Text label = CreateUiText(text, new Vector2(0.5f, 0.5f), Vector2.zero, 28, TextAnchor.MiddleCenter, Color.white, buttonObject.transform);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        return label;
    }

    private Slider CreateSlider(Vector2 anchor, Vector2 size)
    {
        GameObject sliderObject = new GameObject("Progress Slider");
        sliderObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = sliderObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.interactable = false;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObject.transform, false);
        Image bg = background.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.22f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2f, 2f);
        fillAreaRect.offsetMax = new Vector2(-2f, -2f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.1f, 0.85f, 1f, 0.9f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        return slider;
    }

    private TextMesh CreateWorldText(string text, float size, Color color)
    {
        GameObject textObject = new GameObject("World Text " + text);
        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 48;
        textMesh.characterSize = size;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        return textMesh;
    }

    private void StartRun()
    {
        isRunning = true;
        isFinished = false;
        readyPanel.SetActive(false);
        resultPanel.SetActive(false);
        stateText.text = "Choose the best gate";
    }

    private void ResetRun()
    {
        if (crowd != null)
        {
            Destroy(crowd.gameObject);
        }

        consumedGateGroups.Clear();

        foreach (GameObject levelObject in levelObjects)
        {
            Gate gate = levelObject.GetComponent<Gate>();
            if (gate != null)
            {
                gate.ResetGate();
            }

            EnemyMob enemy = levelObject.GetComponent<EnemyMob>();
            if (enemy != null)
            {
                enemy.ResetMob();
            }
        }

        GameObject crowdObject = new GameObject("Blue Crowd");
        crowdObject.transform.position = new Vector3(0f, 0.05f, 3f);
        crowd = crowdObject.AddComponent<CrowdController>();
        crowd.Configure(blueMaterial, StartUnits);
        FollowCamera(true);

        isRunning = false;
        isFinished = false;
        readyPanel.SetActive(true);
        resultPanel.SetActive(false);
        stateText.text = "Upgrade, then start";
        UpdateHud();
    }

    private void UpgradeStartUnits()
    {
        int cost = 20 + startUnitsLevel * 15;
        if (gold < cost)
        {
            SpawnScreenHint("Need " + cost + " gold");
            return;
        }

        gold -= cost;
        startUnitsLevel++;
        ResetRun();
    }

    private void UpgradeIncome()
    {
        int cost = 25 + incomeLevel * 20;
        if (gold < cost)
        {
            SpawnScreenHint("Need " + cost + " gold");
            return;
        }

        gold -= cost;
        incomeLevel++;
        UpdateHud();
    }

    private void FinishRun(bool reachedFinish)
    {
        isFinished = true;
        isRunning = false;

        int survivors = crowd == null ? 0 : crowd.Count;
        float multiplier = reachedFinish ? Mathf.Clamp(1f + survivors / 30f, 1f, 5f) : 0f;
        int earned = Mathf.RoundToInt(survivors * 2f * multiplier * IncomeMultiplier);
        gold += earned;

        stateText.text = reachedFinish ? "Finish!" : "Crowd wiped out";
        resultText.text = reachedFinish
            ? "Survivors: " + survivors + "\nReward x" + multiplier.ToString("0.0") + "\nGold +" + earned
            : "All units lost\nGold +0";
        resultPanel.SetActive(true);
        UpdateHud();
    }

    private void UpdateHud()
    {
        int count = crowd == null ? 0 : crowd.Count;
        countText.text = count.ToString();
        goldText.text = "Gold: " + gold;
        gemText.text = "Gem: " + gems;
        startUnitsText.text = "Start Units Lv." + startUnitsLevel + "\n" + StartUnits + " units";
        incomeText.text = "Income Lv." + incomeLevel + "\nx" + IncomeMultiplier.ToString("0.00");

        if (progressSlider != null && crowd != null)
        {
            progressSlider.value = Mathf.InverseLerp(3f, FinishZ, crowd.transform.position.z);
        }

        if (countText != null && mainCamera != null && crowd != null)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(crowd.transform.position + Vector3.up * 2.4f);
            countText.enabled = screenPosition.z > 0f;
            countText.transform.position = screenPosition;
        }
    }

    private void SpawnPopText(Vector3 position, string text, Color color)
    {
        TextMesh textMesh = CreateWorldText(text, 0.028f, color);
        textMesh.transform.position = position;
        FloatingText floatingText = textMesh.gameObject.AddComponent<FloatingText>();
        floatingText.Configure(mainCamera);
    }

    private void SpawnScreenHint(string text)
    {
        Text hint = CreateUiText(text, new Vector2(0.5f, 0.56f), Vector2.zero, 24, TextAnchor.MiddleCenter, Color.yellow);
        Destroy(hint.gameObject, 1.1f);
    }

    private readonly struct GateSpec
    {
        public readonly GateOperation Operation;
        public readonly int Value;

        public GateSpec(GateOperation operation, int value)
        {
            Operation = operation;
            Value = value;
        }

        public string Label => Operation == GateOperation.Add ? "+" + Value : "x" + Value;
    }
}

public enum GateOperation
{
    Add,
    Multiply
}

public sealed class CrowdController : MonoBehaviour
{
    private readonly List<Transform> units = new List<Transform>();
    private Material unitMaterial;
    private Rigidbody body;
    private BoxCollider trigger;
    private Vector3 lastPointerPosition;
    private bool dragging;

    public int Count => units.Count;

    public void Configure(Material material, int initialCount)
    {
        unitMaterial = material;
        body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        SetCount(initialCount);
    }

    public void Move(float forwardSpeed, float horizontalLimit, float dragSensitivity)
    {
        Vector3 position = transform.position;
        position.z += forwardSpeed * Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            dragging = !IsPointerOverUi();
            lastPointerPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging)
        {
            if (TryGetMouseTrackX(out float targetX))
            {
                position.x = Mathf.Lerp(position.x, targetX, Time.deltaTime * 18f);
            }
            else
            {
                Vector3 pointerDelta = Input.mousePosition - lastPointerPosition;
                position.x += pointerDelta.x * dragSensitivity;
            }

            lastPointerPosition = Input.mousePosition;
        }

        position.x = Mathf.Clamp(position.x, -horizontalLimit, horizontalLimit);
        body.MovePosition(position);
    }

    private static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private static bool TryGetMouseTrackX(out float x)
    {
        x = 0f;
        Camera camera = Camera.main;
        if (camera == null)
        {
            return false;
        }

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Plane trackPlane = new Plane(Vector3.up, Vector3.zero);
        if (!trackPlane.Raycast(ray, out float enter))
        {
            return false;
        }

        x = ray.GetPoint(enter).x;
        return true;
    }

    public void SetCount(int targetCount)
    {
        targetCount = Mathf.Max(0, targetCount);

        while (units.Count < targetCount)
        {
            Transform unit = CreateUnit(units.Count).transform;
            units.Add(unit);
        }

        while (units.Count > targetCount)
        {
            Transform unit = units[units.Count - 1];
            units.RemoveAt(units.Count - 1);
            Destroy(unit.gameObject);
        }

        ArrangeUnits();
    }

    public void RemoveUnits(int amount)
    {
        SetCount(Count - amount);
    }

    private GameObject CreateUnit(int index)
    {
        GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        unit.name = "Blue Unit " + index;
        unit.transform.SetParent(transform, false);
        unit.transform.localScale = new Vector3(0.38f, 0.58f, 0.38f);
        unit.GetComponent<Renderer>().material = unitMaterial;
        Destroy(unit.GetComponent<Collider>());
        return unit;
    }

    private void ArrangeUnits()
    {
        int columns = Mathf.Clamp(Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(1, Count))), 2, 14);
        float spacing = 0.55f;

        for (int i = 0; i < units.Count; i++)
        {
            int row = i / columns;
            int column = i % columns;
            float x = (column - (columns - 1) * 0.5f) * spacing;
            float z = -(row * spacing);
            units[i].localPosition = new Vector3(x, 0.55f, z);
        }

        int rows = Mathf.Max(1, Mathf.CeilToInt(Count / (float)columns));
        trigger.center = new Vector3(0f, 0.65f, -Mathf.Max(0f, rows - 1) * spacing * 0.5f);
        trigger.size = new Vector3(Mathf.Max(1f, columns * spacing), 1.5f, Mathf.Max(1f, rows * spacing));
    }
}

public sealed class Gate : MonoBehaviour
{
    private CrowdRunnerGame game;
    private GateOperation operation;
    private int value;
    private int gateGroupId;
    private bool consumed;

    public void Configure(CrowdRunnerGame owner, GateOperation gateOperation, int gateValue, int groupId)
    {
        game = owner;
        operation = gateOperation;
        value = gateValue;
        gateGroupId = groupId;
    }

    public void ResetGate()
    {
        consumed = false;
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = true;
        }
    }

    public void Hide()
    {
        consumed = true;
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (consumed || other.GetComponentInParent<CrowdController>() == null)
        {
            return;
        }

        if (!game.TryConsumeGateGroup(gateGroupId))
        {
            return;
        }

        game.ApplyGate(operation, value);
    }
}

public sealed class EnemyMob : MonoBehaviour
{
    private readonly List<GameObject> enemies = new List<GameObject>();
    private CrowdRunnerGame game;
    private Material material;
    private int initialAmount;
    private int columns;
    private BoxCollider trigger;

    public int Remaining => enemies.Count;

    public void Configure(CrowdRunnerGame owner, Material enemyMaterial, int amount, int columnCount)
    {
        game = owner;
        material = enemyMaterial;
        initialAmount = amount;
        columns = Mathf.Max(1, columnCount);
        trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        ResetMob();
    }

    public void ResetMob()
    {
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        enemies.Clear();

        for (int i = 0; i < initialAmount; i++)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Red Unit " + i;
            enemy.transform.SetParent(transform, false);
            enemy.transform.localScale = new Vector3(0.42f, 0.6f, 0.42f);
            enemy.GetComponent<Renderer>().material = material;
            Destroy(enemy.GetComponent<Collider>());
            enemies.Add(enemy);
        }

        Arrange();
    }

    public void RemoveUnits(int amount)
    {
        for (int i = 0; i < amount && enemies.Count > 0; i++)
        {
            GameObject enemy = enemies[enemies.Count - 1];
            enemies.RemoveAt(enemies.Count - 1);
            Destroy(enemy);
        }

        Arrange();
    }

    private void Arrange()
    {
        float spacing = 0.58f;
        for (int i = 0; i < enemies.Count; i++)
        {
            int row = i / columns;
            int column = i % columns;
            enemies[i].transform.localPosition = new Vector3((column - (columns - 1) * 0.5f) * spacing, 0.6f, -row * spacing);
        }

        int rows = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(1, enemies.Count) / (float)columns));
        trigger.center = new Vector3(0f, 0.65f, -Mathf.Max(0f, rows - 1) * spacing * 0.5f);
        trigger.size = new Vector3(Mathf.Max(1f, columns * spacing), 1.5f, Mathf.Max(1f, rows * spacing));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CrowdController>() == null)
        {
            return;
        }

        game.ResolveEnemy(this);
    }
}

public sealed class ObstacleHazard : MonoBehaviour
{
    private CrowdRunnerGame game;
    private float direction = 1f;
    private float nextDamageTime;
    private Vector3 startPosition;

    public void Configure(CrowdRunnerGame owner, float initialDirection)
    {
        game = owner;
        direction = initialDirection;
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, 420f * Time.deltaTime, Space.Self);
        Vector3 position = startPosition;
        position.x += Mathf.Sin(Time.time * 1.6f * direction) * 2.1f;
        transform.position = position;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < nextDamageTime || other.GetComponentInParent<CrowdController>() == null)
        {
            return;
        }

        nextDamageTime = Time.time + 0.18f;
        game.DamageCrowd(2, transform.position);
    }
}

public sealed class FinishZone : MonoBehaviour
{
    private CrowdRunnerGame game;

    public void Configure(CrowdRunnerGame owner)
    {
        game = owner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CrowdController>() == null)
        {
            return;
        }

        game.ReachFinish();
    }
}

public sealed class FloatingText : MonoBehaviour
{
    private Camera targetCamera;
    private float life = 0.8f;

    public void Configure(Camera cameraToFace)
    {
        targetCamera = cameraToFace;
    }

    private void Update()
    {
        life -= Time.deltaTime;
        transform.position += Vector3.up * (1.8f * Time.deltaTime);

        if (targetCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
        }

        if (life <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
