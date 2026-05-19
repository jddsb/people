using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TestAWheelColor
{
    Green,
    Blue,
    Yellow
}

public sealed class TestAWheelRunnerGame : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private TestAWheelColor initialWheelColor = TestAWheelColor.Green;
    [SerializeField] private float forwardSpeed = 8.5f;
    [SerializeField] private float maxForwardSpeed = 22f;
    [SerializeField] private float speedRampDistance = 500f;
    [SerializeField] private float horizontalSpeed = 9f;
    [SerializeField] private float dragSensitivity = 0.018f;
    [SerializeField] private float initialWheelRadius = 0.72f;
    [SerializeField] private float radiusStep = 0.36f;
    [SerializeField] private float minWheelRadius = 0.28f;
    [SerializeField] private float maxWheelRadius = 12f;

    [Header("Art Materials")]
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material yellowMaterial;
    [SerializeField] private Material trackMaterial;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material skinMaterial;
    [SerializeField] private Material shirtMaterial;
    [SerializeField] private Material shortsMaterial;
    [SerializeField] private Material hairMaterial;
    [SerializeField] private Material whiteMaterial;
    [SerializeField] private Material darkMaterial;

    private const float TrackWidth = 7.2f;
    private const float TrackLoopStartZ = 2f;
    private const float TrackLoopLength = 718f;
    private const int LoopSegmentCount = 4;
    private const float LoopSegmentRecycleMargin = 80f;
    private const float TrackContentZScale = 2f;
    private const float TrackStripeSpacing = 9f;
    private const float PadHitExtraHalfWidth = 0.38f;

    private readonly List<ColorPad> colorPads = new List<ColorPad>();
    private readonly List<ColorBaffle> colorBaffles = new List<ColorBaffle>();
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly float[] loopSegmentStarts = new float[LoopSegmentCount];
    private readonly Transform[] loopSegmentRoots = new Transform[LoopSegmentCount];

    private Transform runnerRoot;
    private Transform wheel;
    private Transform characterRoot;
    private Camera mainCamera;
    private Text scoreText;
    private Text heightText;
    private Text messageText;
    private Slider progressSlider;

    private TestAWheelColor wheelColor;
    private float currentRadius;
    private float targetRadius;
    private float xPosition;
    private float zPosition = TrackLoopStartZ;
    private float totalDistanceTraveled;
    private int lapCount;
    private float lastPointerX;
    private int score;
    private bool isDragging;
    private float wheelRollAngle;

    private void Awake()
    {
        if (Mathf.Approximately(maxWheelRadius, 2.35f) || Mathf.Approximately(maxWheelRadius, 5f))
        {
            maxWheelRadius = 12f;
        }

        wheelColor = initialWheelColor;
        currentRadius = initialWheelRadius;
        targetRadius = initialWheelRadius;
        BuildFallbackMaterials();
        BuildWorld();
        UpdateWheelScale(true);
        UpdateUi();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWheelColor(TestAWheelColor.Green);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWheelColor(TestAWheelColor.Blue);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetWheelColor(TestAWheelColor.Yellow);
        }

        HandleInput();
        MoveRunner();
        CheckPads();
        CheckBaffles();
        UpdateUi();
    }

    private void LateUpdate()
    {
        if (mainCamera == null || runnerRoot == null)
        {
            return;
        }

        GetCameraRig(out Vector3 targetPosition, out Vector3 lookTarget);
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - targetPosition, Vector3.up);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * 6f);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * 7f);
    }

    private void GetCameraRig(out Vector3 targetPosition, out Vector3 lookTarget)
    {
        float wheelRadius = Mathf.Max(currentRadius, targetRadius);
        float characterTop = wheelRadius * 2f + 2.1f;
        float backDistance = 9.5f + wheelRadius * 0.62f;
        float cameraHeight = characterTop + 5.2f + wheelRadius * 0.42f;
        float lookAheadZ = 14f + wheelRadius * 1.35f;
        float lookHeight = 1.05f;

        targetPosition = runnerRoot.position + new Vector3(0f, cameraHeight, -backDistance);
        lookTarget = runnerRoot.position + new Vector3(0f, lookHeight, lookAheadZ);
    }

    private void BuildFallbackMaterials()
    {
        greenMaterial = greenMaterial != null ? greenMaterial : CreateMaterial("Runtime Green", new Color(0.02f, 0.78f, 0.11f));
        blueMaterial = blueMaterial != null ? blueMaterial : CreateMaterial("Runtime Blue", new Color(0.02f, 0.44f, 1f));
        yellowMaterial = yellowMaterial != null ? yellowMaterial : CreateMaterial("Runtime Yellow", new Color(1f, 0.78f, 0.02f));
        trackMaterial = trackMaterial != null ? trackMaterial : CreateMaterial("Runtime Track", new Color(0.87f, 0.78f, 0.93f));
        wallMaterial = wallMaterial != null ? wallMaterial : CreateMaterial("Runtime Wall", new Color(0.42f, 0.31f, 0.68f));
        skinMaterial = skinMaterial != null ? skinMaterial : CreateMaterial("Runtime Skin", new Color(0.94f, 0.56f, 0.32f));
        shirtMaterial = shirtMaterial != null ? shirtMaterial : CreateMaterial("Runtime Shirt", new Color(0.05f, 0.33f, 0.45f));
        shortsMaterial = shortsMaterial != null ? shortsMaterial : CreateMaterial("Runtime Shorts", new Color(0.82f, 0.2f, 0.08f));
        hairMaterial = hairMaterial != null ? hairMaterial : CreateMaterial("Runtime Hair", new Color(0.04f, 0.035f, 0.03f));
        whiteMaterial = whiteMaterial != null ? whiteMaterial : CreateMaterial("Runtime White", Color.white);
        darkMaterial = darkMaterial != null ? darkMaterial : CreateMaterial("Runtime Dark", new Color(0.07f, 0.06f, 0.1f));
    }

    private void BuildWorld()
    {
        ClearSpawnedObjects();
        BuildLightingAndCamera();
        BuildLoopSegments();
        BuildColorBaffles();
        BuildRunner();
        BuildUi();
    }

    private void ClearSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();
        colorPads.Clear();
        colorBaffles.Clear();
    }

    private void BuildLightingAndCamera()
    {
        Camera[] existingCameras = FindObjectsOfType<Camera>();
        for (int i = 0; i < existingCameras.Length; i++)
        {
            Destroy(existingCameras[i].gameObject);
        }

        Light[] existingLights = FindObjectsOfType<Light>();
        for (int i = 0; i < existingLights.Length; i++)
        {
            Destroy(existingLights[i].gameObject);
        }

        GameObject lightObject = new GameObject("Sun Light");
        Register(lightObject);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 12f);

        GameObject cameraObject = new GameObject("Main Camera");
        Register(cameraObject);
        mainCamera = cameraObject.AddComponent<Camera>();
        mainCamera.tag = "MainCamera";
        mainCamera.fieldOfView = 58f;
        mainCamera.backgroundColor = new Color(0.18f, 0.14f, 0.28f);
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0f, 5.2f, -8.2f);
        cameraObject.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
    }

    private void BuildLoopSegments()
    {
        for (int i = 0; i < LoopSegmentCount; i++)
        {
            float segmentStart = TrackLoopStartZ + i * TrackLoopLength;
            loopSegmentStarts[i] = segmentStart;

            GameObject segmentRoot = new GameObject("Loop Segment " + i);
            Register(segmentRoot);
            segmentRoot.transform.position = new Vector3(0f, 0f, segmentStart);
            loopSegmentRoots[i] = segmentRoot.transform;

            BuildTrackSegmentVisuals(segmentRoot.transform);
            BuildColorPadsForSegment(segmentRoot.transform, i == 0);
        }
    }

    private void BuildTrackSegmentVisuals(Transform parent)
    {
        GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(track);
        track.name = "Loop Pastel Track";
        track.transform.SetParent(parent, false);
        track.transform.localPosition = new Vector3(0f, -0.05f, TrackLoopLength * 0.5f);
        track.transform.localScale = new Vector3(TrackWidth, 0.1f, TrackLoopLength);
        SetMaterial(track, trackMaterial);

        int stripeCount = Mathf.FloorToInt((TrackLoopLength - 4f) / TrackStripeSpacing);
        for (int i = 0; i < stripeCount; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Register(stripe);
            stripe.name = "Track Soft Stripe";
            stripe.transform.SetParent(parent, false);
            stripe.transform.localPosition = new Vector3(0f, 0.01f, 3f + i * TrackStripeSpacing);
            stripe.transform.localScale = new Vector3(TrackWidth, 0.025f, 4.4f);
            SetMaterial(stripe, i % 2 == 0 ? trackMaterial : CreateMaterial("Runtime Track Stripe", new Color(0.91f, 0.84f, 0.96f)));
        }

        CreateWallChild(parent, "Left Purple Wall", new Vector3(-TrackWidth * 0.5f - 0.45f, 1.4f, TrackLoopLength * 0.5f), new Vector3(0.55f, 2.8f, TrackLoopLength));
        CreateWallChild(parent, "Right Purple Wall", new Vector3(TrackWidth * 0.5f + 0.45f, 1.4f, TrackLoopLength * 0.5f), new Vector3(0.55f, 2.8f, TrackLoopLength));
    }

    private void CreateWallChild(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(wall);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = localPosition;
        wall.transform.localScale = localScale;
        SetMaterial(wall, wallMaterial);
    }

    private void BuildColorPadsForSegment(Transform parent, bool registerGameplay)
    {
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 1", TestAWheelColor.Green, -1.45f, 17f, 5.7f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 1", TestAWheelColor.Blue, 1.45f, 17f, 5.7f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 1", TestAWheelColor.Yellow, 0.35f, 31f, 7f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 2", TestAWheelColor.Green, -1.35f, 43f, 6.4f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 2", TestAWheelColor.Blue, 1.35f, 43f, 6.4f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 3", TestAWheelColor.Green, 0.25f, 58f, 8.5f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 2", TestAWheelColor.Yellow, -1.55f, 73f, 6.2f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 3", TestAWheelColor.Blue, 1.45f, 86f, 7.4f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 4", TestAWheelColor.Green, -0.7f, 100f, 8.2f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 3", TestAWheelColor.Yellow, 1.25f, 116f, 7f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 5", TestAWheelColor.Green, 0f, 130f, 6.8f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 4", TestAWheelColor.Blue, 1.4f, 148f, 6.5f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 4", TestAWheelColor.Yellow, -0.9f, 163f, 7f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 6", TestAWheelColor.Green, -1.2f, 178f, 6.8f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 5", TestAWheelColor.Blue, 1.35f, 193f, 6.4f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 5", TestAWheelColor.Yellow, 0.5f, 208f, 7.2f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 7", TestAWheelColor.Green, -0.6f, 223f, 6.6f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 6", TestAWheelColor.Blue, 1.5f, 238f, 6.2f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 6", TestAWheelColor.Yellow, -0.4f, 253f, 6.8f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 8", TestAWheelColor.Green, -1.3f, 268f, 6.5f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 7", TestAWheelColor.Blue, 1.25f, 283f, 6.6f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 7", TestAWheelColor.Yellow, 0.8f, 298f, 7f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 9", TestAWheelColor.Green, -0.5f, 313f, 6.7f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 8", TestAWheelColor.Blue, 1.45f, 328f, 6.4f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 8", TestAWheelColor.Yellow, -1.1f, 343f, 6.9f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 10", TestAWheelColor.Green, 0.35f, 358f, 6.3f);
    }

    private void AddScaledPad(Transform parent, bool registerGameplay, string name, TestAWheelColor padColor, float x, float z, float length)
    {
        AddPad(parent, registerGameplay, name, padColor, x, z * TrackContentZScale, length);
    }

    private void AddPad(Transform parent, bool registerGameplay, string name, TestAWheelColor padColor, float x, float z, float length)
    {
        float localCenterZ = z - TrackLoopStartZ;
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(pad);
        pad.name = name;
        pad.transform.SetParent(parent, false);
        pad.transform.localPosition = new Vector3(x, 0.06f, localCenterZ);
        pad.transform.localScale = new Vector3(1.15f, 0.12f, length);
        SetMaterial(pad, GetMaterial(padColor));

        if (registerGameplay)
        {
            colorPads.Add(new ColorPad(padColor, x, z - length * 0.5f, z + length * 0.5f, 0.78f, pad));
        }
    }

    private void BuildColorBaffles()
    {
        //AddBaffle("Yellow Color Baffle", TestAWheelColor.Yellow, 30f);
        //AddBaffle("Blue Color Baffle", TestAWheelColor.Blue, 100f);
    }

    private void AddBaffle(string name, TestAWheelColor baffleColor, float z)
    {
        Material opaqueMaterial = GetMaterial(baffleColor);
        Material gateMaterial = CreateTransparentMaterial("Runtime Baffle " + baffleColor, GetColor(baffleColor));

        GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(gate);
        gate.name = name;
        gate.transform.position = new Vector3(0f, 1.85f, z);
        gate.transform.localScale = new Vector3(TrackWidth - 0.35f, 3.7f, 0.14f);
        SetMaterial(gate, gateMaterial);
        RemoveCollider(gate);

        GameObject baseStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(baseStrip);
        baseStrip.name = name + " Base";
        baseStrip.transform.position = new Vector3(0f, 0.12f, z);
        baseStrip.transform.localScale = new Vector3(TrackWidth - 0.15f, 0.2f, 0.38f);
        SetMaterial(baseStrip, opaqueMaterial);
        RemoveCollider(baseStrip);

        colorBaffles.Add(new ColorBaffle(baffleColor, z, 0.42f, gate));
    }

    private void BuildRunner()
    {
        GameObject root = new GameObject("Wheel Runner");
        Register(root);
        runnerRoot = root.transform;
        runnerRoot.position = new Vector3(xPosition, 0f, zPosition);

        GameObject wheelObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Register(wheelObject);
        wheelObject.name = "Rolling Color Wheel";
        wheelObject.transform.SetParent(runnerRoot, false);
        wheelObject.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        wheel = wheelObject.transform;
        SetMaterial(wheelObject, GetMaterial(wheelColor));

        GameObject character = new GameObject("Runner Character");
        Register(character);
        character.transform.SetParent(runnerRoot, false);
        characterRoot = character.transform;
        BuildCharacter(characterRoot);
    }

    private void BuildCharacter(Transform parent)
    {
        CreatePrimitiveChild(parent, "Body", PrimitiveType.Capsule, new Vector3(0f, 1.15f, -0.1f), new Vector3(0.42f, 0.58f, 0.42f), shirtMaterial);
        CreatePrimitiveChild(parent, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.93f, -0.1f), new Vector3(0.42f, 0.42f, 0.42f), skinMaterial);
        CreatePrimitiveChild(parent, "Hair", PrimitiveType.Sphere, new Vector3(0f, 2.08f, -0.13f), new Vector3(0.43f, 0.24f, 0.43f), hairMaterial);
        CreatePrimitiveChild(parent, "Shorts", PrimitiveType.Cube, new Vector3(0f, 0.68f, -0.1f), new Vector3(0.48f, 0.32f, 0.36f), shortsMaterial);
        CreateLimb(parent, "Left Arm", new Vector3(-0.37f, 1.16f, -0.1f), new Vector3(0.14f, 0.56f, 0.14f), Quaternion.Euler(0f, 0f, -14f), skinMaterial);
        CreateLimb(parent, "Right Arm", new Vector3(0.37f, 1.16f, -0.1f), new Vector3(0.14f, 0.56f, 0.14f), Quaternion.Euler(0f, 0f, 14f), skinMaterial);
        CreateLimb(parent, "Left Leg", new Vector3(-0.15f, 0.23f, -0.08f), new Vector3(0.15f, 0.52f, 0.15f), Quaternion.Euler(8f, 0f, 4f), skinMaterial);
        CreateLimb(parent, "Right Leg", new Vector3(0.15f, 0.23f, -0.08f), new Vector3(0.15f, 0.52f, 0.15f), Quaternion.Euler(-8f, 0f, -4f), skinMaterial);
        CreatePrimitiveChild(parent, "Left Shoe", PrimitiveType.Cube, new Vector3(-0.15f, -0.06f, -0.02f), new Vector3(0.2f, 0.1f, 0.36f), whiteMaterial);
        CreatePrimitiveChild(parent, "Right Shoe", PrimitiveType.Cube, new Vector3(0.15f, -0.06f, -0.02f), new Vector3(0.2f, 0.1f, 0.36f), whiteMaterial);
    }

    private void CreateLimb(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
    {
        GameObject limb = CreatePrimitiveChild(parent, name, PrimitiveType.Capsule, localPosition, localScale, material);
        limb.transform.localRotation = localRotation;
    }

    private void BuildUi()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        Register(eventSystem);
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        GameObject canvasObject = new GameObject("Game UI");
        Register(canvasObject);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        scoreText = CreateText(canvasObject.transform, "Score Text", "Score\n0", new Vector2(28f, -28f), TextAnchor.UpperLeft, 32, whiteMaterial.color);
        heightText = CreateText(canvasObject.transform, "Height Text", "Height 0.72", new Vector2(-28f, -28f), TextAnchor.UpperRight, 24, whiteMaterial.color);
        messageText = CreateText(canvasObject.transform, "Message Text", "拖动左右移动；跑道循环无尽，速度会越来越快，R 重开", new Vector2(0f, 72f), TextAnchor.LowerCenter, 20, whiteMaterial.color);

        GameObject sliderObject = new GameObject("Level Progress");
        Register(sliderObject);
        sliderObject.transform.SetParent(canvasObject.transform, false);
        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 1f);
        sliderRect.anchorMax = new Vector2(0.5f, 1f);
        sliderRect.pivot = new Vector2(0.5f, 1f);
        sliderRect.anchoredPosition = new Vector2(0f, -30f);
        sliderRect.sizeDelta = new Vector2(320f, 28f);
        progressSlider = sliderObject.AddComponent<Slider>();
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.interactable = false;

        GameObject background = CreateUiImage(sliderObject.transform, "Background", new Color(1f, 1f, 1f, 0.9f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        Register(fillArea);
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(4f, 4f);
        fillAreaRect.offsetMax = new Vector2(-4f, -4f);

        GameObject fill = CreateUiImage(fillArea.transform, "Fill", GetColor(wheelColor));
        progressSlider.fillRect = fill.GetComponent<RectTransform>();
        progressSlider.targetGraphic = background.GetComponent<Image>();
    }

    private Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition, TextAnchor alignment, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(name);
        Register(textObject);
        textObject.transform.SetParent(parent, false);
        Text uiText = textObject.AddComponent<Text>();
        uiText.text = text;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.fontSize = fontSize;
        uiText.fontStyle = FontStyle.Bold;
        uiText.alignment = alignment;
        uiText.color = color;
        uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        float anchorX = alignment == TextAnchor.UpperRight ? 1f : alignment == TextAnchor.LowerCenter ? 0.5f : 0f;
        float anchorY = alignment == TextAnchor.LowerCenter ? 0f : 1f;
        rectTransform.anchorMin = new Vector2(anchorX, anchorY);
        rectTransform.anchorMax = rectTransform.anchorMin;
        rectTransform.pivot = new Vector2(anchorX, anchorY);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(720f, 110f);
        return uiText;
    }

    private GameObject CreateUiImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name);
        Register(imageObject);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        return imageObject;
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") * horizontalSpeed * Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastPointerX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            float delta = Input.mousePosition.x - lastPointerX;
            horizontal += delta * dragSensitivity;
            lastPointerX = Input.mousePosition.x;
        }

        xPosition = Mathf.Clamp(xPosition + horizontal, -TrackWidth * 0.5f + 0.72f, TrackWidth * 0.5f - 0.72f);
    }

    private float GetCurrentForwardSpeed()
    {
        float ramp = 1f - Mathf.Exp(-totalDistanceTraveled / Mathf.Max(speedRampDistance, 1f));
        float lapBoost = lapCount * 0.06f;
        return Mathf.Lerp(forwardSpeed, maxForwardSpeed, Mathf.Clamp01(ramp + lapBoost));
    }

    private void MoveRunner()
    {
        float speed = GetCurrentForwardSpeed();
        float delta = speed * Time.deltaTime;
        zPosition += delta;
        totalDistanceTraveled += delta;
        UpdateLapProgress();
        UpdateLoopSegments();
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * 10f);
        runnerRoot.position = new Vector3(xPosition, 0f, zPosition);
        UpdateWheelScale(false);

        wheelRollAngle += delta / Mathf.Max(currentRadius, 0.05f) * Mathf.Rad2Deg;
        wheel.localRotation = Quaternion.Euler(wheelRollAngle, 0f, 90f);

        if (characterRoot != null)
        {
            float bob = Mathf.Sin(Time.time * 11f) * 0.035f;
            characterRoot.localPosition = new Vector3(0f, currentRadius * 2f + 0.04f + bob, -0.06f);
        }
    }

    private float GetLoopZ(float worldZ)
    {
        return TrackLoopStartZ + Mathf.Repeat(worldZ - TrackLoopStartZ, TrackLoopLength);
    }

    private void UpdateLapProgress()
    {
        int newLap = Mathf.FloorToInt((zPosition - TrackLoopStartZ) / TrackLoopLength);
        if (newLap <= lapCount)
        {
            return;
        }

        lapCount = newLap;
        ResetLoopInteractables();
        ShowLapMessage();
    }

    private float GetFrontSegmentStart()
    {
        float front = loopSegmentStarts[0];
        for (int i = 1; i < LoopSegmentCount; i++)
        {
            if (loopSegmentStarts[i] > front)
            {
                front = loopSegmentStarts[i];
            }
        }

        return front;
    }

    private void UpdateLoopSegments()
    {
        float recycleZ = zPosition - TrackLoopLength - LoopSegmentRecycleMargin;
        for (int i = 0; i < LoopSegmentCount; i++)
        {
            if (loopSegmentStarts[i] + TrackLoopLength >= recycleZ || loopSegmentRoots[i] == null)
            {
                continue;
            }

            float newStart = GetFrontSegmentStart() + TrackLoopLength;
            loopSegmentStarts[i] = newStart;
            loopSegmentRoots[i].position = new Vector3(0f, 0f, newStart);
        }
    }

    private void ResetLoopInteractables()
    {
        for (int i = 0; i < colorPads.Count; i++)
        {
            ColorPad pad = colorPads[i];
            pad.Consumed = false;
            colorPads[i] = pad;
            if (pad.Visual != null)
            {
                pad.Visual.transform.localScale = new Vector3(pad.Visual.transform.localScale.x, 0.12f, pad.Visual.transform.localScale.z);
            }
        }

        for (int i = 0; i < colorBaffles.Count; i++)
        {
            ColorBaffle baffle = colorBaffles[i];
            baffle.Consumed = false;
            colorBaffles[i] = baffle;
        }
    }

    private void ShowLapMessage()
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = "第 " + (lapCount + 1) + " 圈";
        messageText.color = whiteMaterial.color;
    }

    private void UpdateWheelScale(bool immediate)
    {
        if (immediate)
        {
            currentRadius = targetRadius;
        }

        if (wheel != null)
        {
            wheel.localPosition = new Vector3(0f, currentRadius, 0f);
            wheel.localScale = new Vector3(currentRadius * 2f, 0.34f, currentRadius * 2f);
        }

        if (characterRoot != null)
        {
            characterRoot.localPosition = new Vector3(0f, currentRadius * 2f + 0.04f, -0.06f);
        }
    }

    private void CheckPads()
    {
        float loopZ = GetLoopZ(zPosition);
        for (int i = 0; i < colorPads.Count; i++)
        {
            ColorPad pad = colorPads[i];
            if (pad.Consumed || loopZ < pad.StartZ || loopZ > pad.EndZ || Mathf.Abs(xPosition - pad.X) > pad.HalfWidth + PadHitExtraHalfWidth)
            {
                continue;
            }

            bool isMatch = pad.Color == wheelColor;
            float delta = isMatch ? radiusStep : -radiusStep;
            float previousTargetRadius = targetRadius;
            float newTargetRadius = Mathf.Clamp(targetRadius + delta, minWheelRadius, maxWheelRadius);
            bool radiusChanged = !Mathf.Approximately(newTargetRadius, previousTargetRadius);

            pad.Consumed = true;
            colorPads[i] = pad;
            targetRadius = newTargetRadius;
            score = Mathf.Max(0, score + (isMatch ? 1 : -1));
            PulsePad(pad.Visual, isMatch);
            ShowPadMessage(isMatch, pad.Color, radiusChanged);
        }
    }

    private void PulsePad(GameObject pad, bool isMatch)
    {
        if (pad == null)
        {
            return;
        }

        pad.transform.localScale = new Vector3(pad.transform.localScale.x, isMatch ? 0.22f : 0.05f, pad.transform.localScale.z);
    }

    private void ShowPadMessage(bool isMatch, TestAWheelColor padColor, bool radiusChanged)
    {
        if (messageText == null)
        {
            return;
        }

        if (isMatch && !radiusChanged)
        {
            messageText.text = "已达最大高度！";
        }
        else
        {
            messageText.text = isMatch ? "颜色相同：轮子升高！" : "颜色不同：轮子降低！";
        }

        messageText.color = isMatch ? GetColor(wheelColor) : GetColor(padColor);
    }

    private void CheckBaffles()
    {
        for (int i = 0; i < colorBaffles.Count; i++)
        {
            ColorBaffle baffle = colorBaffles[i];
            if (baffle.Consumed || Mathf.Abs(GetLoopZ(zPosition) - baffle.Z) > baffle.HalfThickness)
            {
                continue;
            }

            baffle.Consumed = true;
            colorBaffles[i] = baffle;
            SetWheelColor(baffle.Color);
            ShowBaffleMessage(baffle.Color);
            score += 1;
            PulseBaffle(baffle.Visual);
        }
    }

    private void PulseBaffle(GameObject gate)
    {
        if (gate == null)
        {
            return;
        }

        gate.transform.localScale = new Vector3(gate.transform.localScale.x * 1.06f, gate.transform.localScale.y, gate.transform.localScale.z * 1.35f);
    }

    private void ShowBaffleMessage(TestAWheelColor baffleColor)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = "Very Good!";
        messageText.color = GetColor(baffleColor);
    }

    private void UpdateUi()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score\n" + score + "\nLap " + (lapCount + 1);
        }

        if (heightText != null)
        {
            heightText.text = "Color " + wheelColor + "\nHeight " + targetRadius.ToString("0.00");
        }

        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Clamp01((GetLoopZ(zPosition) - TrackLoopStartZ) / TrackLoopLength);
            if (progressSlider.fillRect != null)
            {
                Image fillImage = progressSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = GetColor(wheelColor);
                }
            }
        }
    }

    private void SetWheelColor(TestAWheelColor color)
    {
        wheelColor = color;
        if (wheel != null)
        {
            Renderer renderer = wheel.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetMaterial(wheelColor);
            }
        }

        UpdateUi();
    }

    private void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(wall);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        SetMaterial(wall, material);
    }

    private GameObject CreatePrimitiveChild(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject child = GameObject.CreatePrimitive(primitiveType);
        Register(child);
        child.name = name;
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        child.transform.localScale = localScale;
        SetMaterial(child, material);
        return child;
    }

    private void Register(GameObject gameObject)
    {
        spawnedObjects.Add(gameObject);
    }

    private void SetMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
    }

    private Material GetMaterial(TestAWheelColor color)
    {
        switch (color)
        {
            case TestAWheelColor.Blue:
                return blueMaterial;
            case TestAWheelColor.Yellow:
                return yellowMaterial;
            default:
                return greenMaterial;
        }
    }

    private Color GetColor(TestAWheelColor color)
    {
        Material material = GetMaterial(color);
        return material != null ? material.color : Color.white;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = name;
        material.color = color;
        return material;
    }

    private static Material CreateTransparentMaterial(string name, Color color)
    {
        Material material = CreateMaterial(name, color);
        Color transparent = color;
        transparent.a = 0.42f;
        material.color = transparent;
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        return material;
    }

    private static void RemoveCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private struct ColorBaffle
    {
        public readonly TestAWheelColor Color;
        public readonly float Z;
        public readonly float HalfThickness;
        public readonly GameObject Visual;
        public bool Consumed;

        public ColorBaffle(TestAWheelColor color, float z, float halfThickness, GameObject visual)
        {
            Color = color;
            Z = z;
            HalfThickness = halfThickness;
            Visual = visual;
            Consumed = false;
        }
    }

    private struct ColorPad
    {
        public readonly TestAWheelColor Color;
        public readonly float X;
        public readonly float StartZ;
        public readonly float EndZ;
        public readonly float HalfWidth;
        public readonly GameObject Visual;
        public bool Consumed;

        public ColorPad(TestAWheelColor color, float x, float startZ, float endZ, float halfWidth, GameObject visual)
        {
            Color = color;
            X = x;
            StartZ = startZ;
            EndZ = endZ;
            HalfWidth = halfWidth;
            Visual = visual;
            Consumed = false;
        }
    }
}

