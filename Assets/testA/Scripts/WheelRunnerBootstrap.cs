using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed partial class WheelRunnerBootstrap : MonoBehaviour
{
    public enum WheelRunnerColor
    {
        Green,
        Blue,
        Yellow
    }

    internal struct WheelRunnerColorBaffle
    {
        public readonly WheelRunnerColor Color;
        public readonly float Z;
        public readonly float HalfThickness;
        public readonly GameObject Visual;
        public bool Consumed;

        public WheelRunnerColorBaffle(WheelRunnerColor color, float z, float halfThickness, GameObject visual)
        {
            Color = color;
            Z = z;
            HalfThickness = halfThickness;
            Visual = visual;
            Consumed = false;
        }
    }

    internal struct WheelRunnerColorPad
    {
        public readonly WheelRunnerColor Color;
        public readonly float X;
        public readonly float StartZ;
        public readonly float EndZ;
        public readonly float HalfWidth;
        public readonly GameObject Visual;
        public bool Consumed;

        public WheelRunnerColorPad(WheelRunnerColor color, float x, float startZ, float endZ, float halfWidth, GameObject visual)
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

    internal struct WheelRunnerSpikeTrap
    {
        public readonly float X;
        public readonly float StartZ;
        public readonly float EndZ;
        public readonly float HalfWidth;
        public readonly GameObject Visual;
        public bool Consumed;

        public WheelRunnerSpikeTrap(float x, float startZ, float endZ, float halfWidth, GameObject visual)
        {
            X = x;
            StartZ = startZ;
            EndZ = endZ;
            HalfWidth = halfWidth;
            Visual = visual;
            Consumed = false;
        }
    }

    internal struct WheelRunnerFallingBall
    {
        public WheelRunnerColor Color;
        public float X;
        public float Z;
        public float Y;
        public float RollAngle;
        public GameObject Visual;
        public bool HasLanded;
        public bool Consumed;

        public WheelRunnerFallingBall(WheelRunnerColor color, float x, float z, float y, GameObject visual)
        {
            Color = color;
            X = x;
            Z = z;
            Y = y;
            RollAngle = 0f;
            Visual = visual;
            HasLanded = false;
            Consumed = false;
        }
    }

    [Header("Gameplay")]
    [SerializeField] private WheelRunnerColor initialWheelColor = WheelRunnerColor.Green;
    [SerializeField] private float forwardSpeed = 8.5f;
    [SerializeField] private float maxForwardSpeed = 44f;
    [SerializeField] private float speedRampDistance = 320f;
    [SerializeField] private float mismatchSlowdownMultiplier = 0.42f;
    [SerializeField] private float postSlowdownAcceleration = 9f;
    [SerializeField] private float horizontalSpeed = 9f;
    [SerializeField] private float dragSensitivity = 0.018f;
    [SerializeField] private float initialWheelRadius = 0.72f;
    [SerializeField] private float radiusStep = 0.36f;
    [SerializeField] private float minWheelRadius = 0.28f;
    [SerializeField] private float maxWheelRadius = 12f;
    [SerializeField] private float spikeRadiusMultiplier = 0.42f;
    [SerializeField] private float spikeSlowdownMultiplier = 0.18f;
    [SerializeField] private float fallingBallRadius = 0.62f;
    [SerializeField] private float fallingBallSpawnDistance = 42f;
    [SerializeField] private float fallingBallSpawnInterval = 3.2f;
    [SerializeField] private float fallingBallFallSpeed = 10f;
    [SerializeField] private float fallingBallRollSpeed = 13f;
    [SerializeField] private float fallingBallRadiusMultiplier = 0.34f;
    [SerializeField] private float fallingBallSlowdownMultiplier = 0.16f;

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
    [SerializeField] private Material spikeMaterial;

    private readonly List<WheelRunnerColorPad> colorPads = new List<WheelRunnerColorPad>();
    private readonly List<WheelRunnerColorBaffle> colorBaffles = new List<WheelRunnerColorBaffle>();
    private readonly List<WheelRunnerSpikeTrap> spikeTraps = new List<WheelRunnerSpikeTrap>();
    private readonly List<WheelRunnerFallingBall> fallingBalls = new List<WheelRunnerFallingBall>();
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
    private RectTransform tutorialRoot;
    private RectTransform tutorialHand;
    private GameObject retryButtonObject;

    private WheelRunnerColor wheelColor;
    private float currentRadius;
    private float targetRadius;
    private float xPosition;
    private float zPosition = TrackLoopStartZ;
    private float totalDistanceTraveled;
    private int lapCount;
    private float lastPointerX;
    private int score;
    private bool isDragging;
    private bool gameStarted;
    private bool isDead;
    private float deathElapsed;
    private Vector3 deathCharacterStartLocalPosition;
    private float wheelRollAngle;
    private float currentForwardSpeed;
    private float nextFallingBallSpawnTime;

    private void Awake()
    {
        if (Mathf.Approximately(maxWheelRadius, 2.35f) || Mathf.Approximately(maxWheelRadius, 5f))
        {
            maxWheelRadius = 12f;
        }

        wheelColor = initialWheelColor;
        currentRadius = initialWheelRadius;
        targetRadius = initialWheelRadius;
        currentForwardSpeed = forwardSpeed;
        BuildFallbackMaterials();
        BuildWorld();
        UpdateWheelScale(true);
        UpdateUi();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
            return;
        }

        if (isDead)
        {
            UpdateDeathAnimation();
            UpdateUi();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWheelColor(WheelRunnerColor.Green);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWheelColor(WheelRunnerColor.Blue);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetWheelColor(WheelRunnerColor.Yellow);
        }

        if (!gameStarted)
        {
            UpdateTutorialGuide();
            bool startTriggered = Input.GetMouseButtonDown(0) || Input.touchCount > 0;
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Input.touchCount > 0)
            {
                startTriggered = startTriggered || Input.GetTouch(0).phase == TouchPhase.Began;
            }
#endif
            if (startTriggered)
            {
                StartGame();
            }

            UpdateUi();
            return;
        }

        HandleInput();
        MoveRunner();
        UpdateFallingBalls();
        CheckPads();
        CheckSpikeTraps();
        CheckFallingBalls();
        CheckBaffles();
        CheckDeathCondition();
        UpdateUi();
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CheckDeathCondition()
    {
        if (isDead || targetRadius >= initialWheelRadius)
        {
            return;
        }

        TriggerDeath();
    }

    private void TriggerDeath()
    {
        isDead = true;
        isDragging = false;
        currentForwardSpeed = 0f;
        deathElapsed = 0f;

        if (characterRoot != null)
        {
            deathCharacterStartLocalPosition = characterRoot.localPosition;
        }

        ShowRetryButton();
    }

    private void UpdateDeathAnimation()
    {
        if (characterRoot == null)
        {
            return;
        }

        deathElapsed += Time.deltaTime;
        float fallTime = Mathf.Clamp01(deathElapsed / 0.9f);
        float eased = 1f - Mathf.Pow(1f - fallTime, 2f);
        Vector3 fallOffset = new Vector3(1.15f * eased, -2.25f * eased, -0.75f * eased);
        characterRoot.localPosition = deathCharacterStartLocalPosition + fallOffset;
        characterRoot.localRotation = Quaternion.Euler(0f, 0f, -115f * eased);
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
}
