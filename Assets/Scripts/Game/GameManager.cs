using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Central game manager — bootstraps the match, spawns players and bots,
/// tracks win conditions, and wires up all major systems.
///
/// Scene setup (Game.unity):
///   ┌─ GameManager (this component)
///   ├─ Map (MapGenerator)
///   ├─ ObjectSpawner
///   ├─ TerritorySystem
///   ├─ BuffSpawner
///   ├─ ScoreManager
///   ├─ MatchTimer
///   └─ Canvas
///        ├─ GameHUD
///        ├─ Joystick (background + handle children)
///        ├─ PauseMenu
///        ├─ GameOverScreen
///        └─ SettingsMenu
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Configuration")]
    [SerializeField] private GameConfig config;

    [Header("Prefabs")]
    [SerializeField] private BlackHoleController playerPrefab;
    [SerializeField] private BotAI               botPrefab;
    [SerializeField] private BotDifficulty[]     botDifficulties;

    [Header("Scene References")]
    [SerializeField] private Joystick       joystick;
    [SerializeField] private GameHUD        hud;
    [SerializeField] private PauseMenu      pauseMenu;
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private MatchTimer     matchTimer;
    [SerializeField] private ScoreManager   scoreManager;
    [SerializeField] private TerritorySystem territorySystem;
    [SerializeField] private MinimapController minimapController;

    // ── Private ────────────────────────────────────────────────────────────────
    private BlackHoleController              _playerController;
    private readonly List<BlackHoleController> _allControllers = new List<BlackHoleController>();
    private bool  _gameEnded;
    private float _matchStartTime;

    // Player colour palette
    private static readonly Color[] PlayerColors = new Color[]
    {
        Color.cyan,
        Color.red,
        Color.green,
        new Color(1f, 0.5f, 0f),   // orange
        Color.magenta,
        Color.yellow,
        new Color(0.5f, 0f, 1f),   // purple
        Color.white
    };

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");
    }

    private void Start()
    {
        // Auto-discover scene references if not assigned in the Inspector
        if (joystick        == null) joystick        = FindFirstObjectByType<Joystick>();
        if (hud             == null) hud             = FindFirstObjectByType<GameHUD>();
        if (pauseMenu       == null) pauseMenu       = FindFirstObjectByType<PauseMenu>();
        if (gameOverScreen  == null) gameOverScreen  = FindFirstObjectByType<GameOverScreen>();
        if (matchTimer      == null) matchTimer      = FindFirstObjectByType<MatchTimer>();
        if (scoreManager    == null) scoreManager    = FindFirstObjectByType<ScoreManager>();
        if (territorySystem == null) territorySystem = FindFirstObjectByType<TerritorySystem>();
        if (minimapController == null) minimapController = FindFirstObjectByType<MinimapController>();

        // Short delay to let all systems initialise
        StartCoroutine(InitialiseMatch());
    }

    private void Update()
    {
        if (_gameEnded) return;

        UpdateScores();
        CheckWinConditions();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Called by the pause button in the HUD.</summary>
    public void TogglePause()
    {
        if (pauseMenu == null) return;
        if (pauseMenu.IsPaused)
            pauseMenu.Resume();
        else
            pauseMenu.Pause();
    }

    // ── Private — Initialisation ───────────────────────────────────────────────

    private IEnumerator InitialiseMatch()
    {
        yield return null; // wait one frame

        // Re-discover references in case they were created after Start()
        if (matchTimer      == null) matchTimer      = FindFirstObjectByType<MatchTimer>();
        if (scoreManager    == null) scoreManager    = FindFirstObjectByType<ScoreManager>();
        if (territorySystem == null) territorySystem = FindFirstObjectByType<TerritorySystem>();
        if (joystick        == null) joystick        = FindFirstObjectByType<Joystick>();
        if (hud             == null) hud             = FindFirstObjectByType<GameHUD>();
        if (pauseMenu       == null) pauseMenu       = FindFirstObjectByType<PauseMenu>();
        if (gameOverScreen  == null) gameOverScreen  = FindFirstObjectByType<GameOverScreen>();
        if (minimapController == null) minimapController = FindFirstObjectByType<MinimapController>();
        if (minimapController == null)
            minimapController = gameObject.AddComponent<MinimapController>();

        SpawnPlayer();
        SpawnBots();

        _matchStartTime = Time.time;

        // Null-safe timer start
        if (matchTimer != null)
        {
            if (matchTimer.OnTimeUp == null)
                matchTimer.OnTimeUp = new UnityEngine.Events.UnityEvent();
            matchTimer.OnTimeUp.AddListener(OnTimeUp);
            matchTimer.StartTimer();
        }
        else
        {
            Debug.LogWarning("[GameManager] MatchTimer not found — creating one.");
            GameObject timerObj = new GameObject("MatchTimer");
            matchTimer = timerObj.AddComponent<MatchTimer>();
            if (matchTimer.OnTimeUp == null)
                matchTimer.OnTimeUp = new UnityEngine.Events.UnityEvent();
            matchTimer.OnTimeUp.AddListener(OnTimeUp);
            matchTimer.StartTimer();
        }

        Debug.Log("[GameManager] Match started!");
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("[GameManager] Player prefab not assigned — creating runtime placeholder.");
            playerPrefab = CreateBlackHolePrefab<BlackHoleController>(PlayerColors[0]);
        }

        // Spawn the human player at the map centre for guaranteed first-frame visibility
        Vector3 pos = Vector3.zero;

        BlackHoleController player = Instantiate(playerPrefab, pos, Quaternion.identity);
        player.gameObject.SetActive(true);
        player.gameObject.tag  = "Player";
        player.gameObject.name = "Player";
        player.SetJoystick(joystick);

        // Ensure PlayerVisual is attached so the player circle is always visible
        PlayerVisual visual = player.gameObject.GetComponent<PlayerVisual>();
        if (visual == null)
            visual = player.gameObject.AddComponent<PlayerVisual>();
        visual.SetColor(PlayerColors[0]);

        // Wire camera
        CameraController cam = Camera.main?.GetComponent<CameraController>();
        if (cam == null && Camera.main != null)
            cam = Camera.main.gameObject.AddComponent<CameraController>();
        cam?.SetTarget(player);

        // Set trail colour
        TerritoryTrail trail = player.GetComponent<TerritoryTrail>();
        trail?.SetColor(PlayerColors[0]);

        // Wire minimap — player trail
        minimapController?.SetPlayerTrail(trail);
        minimapController?.SetPlayerTransform(player.transform);
        minimapController?.RegisterTrail(trail);

        // Register in territory and score systems
        territorySystem?.RegisterPlayer(trail, pos, 4f);
        scoreManager?.RegisterPlayer(SettingsMenu.PlayerName, true);

        _playerController = player;
        _allControllers.Add(player);
    }

    private void SpawnBots()
    {
        int botCount = config != null ? config.botCount : 6;

        for (int i = 0; i < botCount; i++)
        {
            // Use the assigned prefab, or create a unique runtime placeholder for each bot
            BotAI prefabToUse = botPrefab;
            if (prefabToUse == null)
            {
                Debug.LogWarning("[GameManager] Bot prefab not assigned — creating runtime placeholder.");
                prefabToUse = CreateBlackHolePrefab<BotAI>(PlayerColors[1 + i % (PlayerColors.Length - 1)]);
            }

            float mapHalf = (config != null ? config.mapSize : 100f) * 0.3f;
            Vector3 pos = MathHelpers.RandomPointInCircle(mapHalf);

            BotAI bot = Instantiate(prefabToUse, pos, Quaternion.identity);
            bot.gameObject.SetActive(true);
            bot.gameObject.name = $"Bot_{i + 1}";

            // Assign difficulty profile
            if (botDifficulties != null && botDifficulties.Length > 0)
                bot.difficulty = botDifficulties[i % botDifficulties.Length];

            Color botColor = PlayerColors[(i + 1) % PlayerColors.Length];

            // Ensure bots are also visually represented
            PlayerVisual botVisual = bot.gameObject.GetComponent<PlayerVisual>();
            if (botVisual == null)
                botVisual = bot.gameObject.AddComponent<PlayerVisual>();
            botVisual.SetColor(botColor);

            TerritoryTrail trail = bot.GetComponent<TerritoryTrail>();
            trail?.SetColor(botColor);
            minimapController?.RegisterTrail(trail);

            territorySystem?.RegisterPlayer(trail, pos, 4f);
            scoreManager?.RegisterPlayer($"Bot {i + 1}", false);

            BlackHoleController ctrl = bot.GetComponent<BlackHoleController>();
            if (ctrl != null) _allControllers.Add(ctrl);
        }
    }

    // ── Private — Score Update ─────────────────────────────────────────────────

    private void UpdateScores()
    {
        foreach (var ctrl in _allControllers)
        {
            if (ctrl == null) continue;
            MassSystem ms = ctrl.GetComponent<MassSystem>();
            TerritoryTrail trail = ctrl.GetComponent<TerritoryTrail>();
            if (ms == null) continue;

            float area = territorySystem != null ? territorySystem.GetCapturedArea(trail) : 0f;
            scoreManager?.UpdateScore(ctrl.gameObject.name, ms.Mass, area, ctrl.IsAlive);
        }
    }

    // ── Private — Win Conditions ───────────────────────────────────────────────

    private void CheckWinConditions()
    {
        if (_gameEnded) return;

        // Grace period: don't end the game in the first 5 seconds
        if (Time.time - _matchStartTime < 5f) return;

        // Count alive players
        int aliveCount = 0;
        bool playerAlive = _playerController != null && _playerController.IsAlive;

        foreach (var ctrl in _allControllers)
        {
            if (ctrl != null && ctrl.IsAlive) aliveCount++;
        }

        // Win: player is the last one standing
        if (aliveCount == 1 && playerAlive)
        {
            EndGame(true);
            return;
        }

        // Lose: player is dead
        if (!playerAlive && aliveCount <= 1)
        {
            EndGame(false);
            return;
        }

        // Check territory domination (>80% of total map area)
        if (territorySystem != null && _playerController != null)
        {
            TerritoryTrail playerTrail = _playerController.GetComponent<TerritoryTrail>();
            float playerArea = territorySystem.GetCapturedArea(playerTrail);
            float mapSide    = config != null ? config.mapSize : 100f;
            float mapArea    = mapSide * mapSide;
            if (playerArea / mapArea > 0.8f)
            {
                EndGame(true);
            }
        }
    }

    private void OnTimeUp()
    {
        if (_gameEnded) return;

        // Determine winner by score
        ScoreManager.ScoreEntry humanEntry = scoreManager?.GetHumanEntry();
        var allEntries = scoreManager?.GetAllEntries();
        bool playerWon = allEntries != null &&
                         allEntries.Count > 0 &&
                         allEntries[0].IsHuman;

        EndGame(playerWon);
    }

    private void EndGame(bool playerWon)
    {
        _gameEnded = true;
        matchTimer?.PauseTimer();

        var allEntries = scoreManager?.GetAllEntries() ?? new List<ScoreManager.ScoreEntry>();
        gameOverScreen?.Show(playerWon, allEntries);

        Debug.Log($"[GameManager] Game ended. Player won: {playerWon}");
    }

    // ── Runtime Prefab Creation ────────────────────────────────────────────────

    /// <summary>
    /// Creates a minimal black-hole prefab at runtime when no prefab is assigned.
    /// Replace with real prefabs in the Inspector for production.
    /// </summary>
    private T CreateBlackHolePrefab<T>(Color color) where T : Component
    {
        GameObject go = new GameObject("BlackHolePrefab_" + typeof(T).Name);
        go.SetActive(false);
        go.transform.localScale = Vector3.one * 0.5f;

        // Visual
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.color = color;
        sr.sortingOrder = 2;

        // Physics
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        // Components
        go.AddComponent<MassSystem>();
        go.AddComponent<BlackHolePhysics>();
        go.AddComponent<TerritoryTrail>();
        go.AddComponent<BlackHoleController>();

        if (typeof(T) == typeof(BotAI))
            go.AddComponent<BotAI>();

        T comp = go.GetComponent<T>();
        return comp;
    }
}
