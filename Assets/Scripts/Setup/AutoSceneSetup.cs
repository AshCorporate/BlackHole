using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Reflection;

/// <summary>
/// Automatically builds the complete game scene at runtime using
/// [RuntimeInitializeOnLoadMethod]. No manual scene setup is required —
/// press Play in any empty scene to get a fully playable game.
/// </summary>
public static class AutoSceneSetup
{
    // ── Player accent colours (index 0 = player, 1-6 = bots) ─────────────────
    private static readonly Color[] PlayerColors = new Color[]
    {
        new Color(0.58f, 0.2f, 0.9f),   // violet  (player)
        new Color(1f,    0.2f, 0.2f),   // red
        new Color(0.2f,  0.6f, 1f),     // blue
        new Color(0.1f,  0.85f,0.3f),   // green
        new Color(1f,    0.75f,0.05f),  // yellow
        new Color(1f,    0.45f,0.0f),   // orange
        new Color(1f,    0.3f, 0.8f),   // pink
    };

    // ── Entry point ───────────────────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // Skip if a GameManager is already present (e.g. a hand-crafted scene)
        if (Object.FindObjectOfType<GameManager>() != null) return;

        // Show main menu if GameBootstrapper requests it; otherwise go straight
        // to the game (default for an empty scene).
        if (GameBootstrapper.ShowMenuOnBoot)
            MainMenuSetup.Build();
        else
            BuildGameScene();
    }

    // ── Public surface ────────────────────────────────────────────────────────

    /// <summary>Builds and wires the entire game scene programmatically.</summary>
    public static void BuildGameScene()
    {
        Debug.Log("[AutoSceneSetup] Building game scene…");

        // Order matters: systems first, then UI, then manager.
        GameObject cameraGO = CreateCamera();
        GameObject mapGO    = CreateMap();
        CreateUI(out Joystick joystick,
                 out GameHUD  hud,
                 out PauseMenu pauseMenu,
                 out GameOverScreen gameOverScreen);
        MatchTimer      matchTimer   = CreateSystem<MatchTimer>("MatchTimer");
        ScoreManager    scoreManager = CreateSystem<ScoreManager>("ScoreManager");
        TerritorySystem territory    = CreateSystem<TerritorySystem>("TerritorySystem");
        // BuffSpawner.Start() self-initialises — no additional wiring needed
        CreateSystem<BuffSpawner>("BuffSpawner");
        ObjectSpawner objSpawner = mapGO.GetComponentInChildren<ObjectSpawner>();

        // Create prefabs for GameManager to Instantiate
        GameObject playerPrefabGO = BuildBlackHolePrefab<BlackHoleController>(PlayerColors[0], "PlayerPrefab");
        GameObject botPrefabGO    = BuildBlackHolePrefab<BotAI>(PlayerColors[1], "BotPrefab");

        // Create city-object prefab for ObjectSpawner
        GameObject cityPrefabGO = BuildCityObjectPrefab();
        SetField(objSpawner, "cityObjectPrefab", cityPrefabGO.GetComponent<CityObject>());

        // Create and wire the GameManager
        GameManager gm = CreateGameManager(
            joystick, hud, pauseMenu, gameOverScreen,
            matchTimer, scoreManager, territory,
            playerPrefabGO.GetComponent<BlackHoleController>(),
            botPrefabGO.GetComponent<BotAI>());

        // Point the camera's follow at the player (set after GM spawns it)
        CameraFollow camFollow = cameraGO.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            // GameManager spawns the player in a coroutine — we hook in after
            gm.StartCoroutine(WireCamera(camFollow));
        }

        Debug.Log("[AutoSceneSetup] Game scene ready.");
    }

    // ── Camera ────────────────────────────────────────────────────────────────

    private static GameObject CreateCamera()
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";

        Camera cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.03f, 0.04f, 0.1f); // deep space navy
        cam.orthographic = true;
        cam.orthographicSize = 18f;
        cam.nearClipPlane = -10f;
        cam.farClipPlane = 100f;
        go.transform.position = new Vector3(0f, 0f, -10f);

        go.AddComponent<AudioListener>();
        go.AddComponent<CameraFollow>();

        return go;
    }

    // ── Map ───────────────────────────────────────────────────────────────────

    private static GameObject CreateMap()
    {
        GameObject mapRoot = new GameObject("Map");

        // MapGenerator.Start() will procedurally build ground, border and roads.
        // ObjectSpawner.Start() will spawn city objects once cityObjectPrefab is set.
        mapRoot.AddComponent<MapGenerator>();
        mapRoot.AddComponent<ObjectSpawner>();

        return mapRoot;
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    private static GameObject CreateUI(
        out Joystick       joystick,
        out GameHUD        hud,
        out PauseMenu      pauseMenu,
        out GameOverScreen gameOverScreen)
    {
        // EventSystem (required for UI interaction)
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Root canvas — Screen Space Overlay
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // ─── Joystick (bottom-left) ───────────────────────────────────────────
        joystick = CreateJoystick(canvasGO);

        // ─── HUD ─────────────────────────────────────────────────────────────
        hud = CreateHUD(canvasGO);

        // ─── Pause button (top-left) ──────────────────────────────────────────
        CreatePauseButton(canvasGO);

        // ─── Pause menu overlay ───────────────────────────────────────────────
        pauseMenu = CreatePauseMenu(canvasGO);

        // ─── Game-over screen ─────────────────────────────────────────────────
        gameOverScreen = CreateGameOverScreen(canvasGO);

        return canvasGO;
    }

    private static Joystick CreateJoystick(GameObject canvasGO)
    {
        // Background circle — the Joystick MonoBehaviour lives here.
        // In Awake, Joystick will assign `background = this.GetComponent<RectTransform>()`
        // (correct!) and call SetVisible(false) which deactivates this GO.
        // We reactivate it after setting the handle field.
        GameObject bg = new GameObject("JoystickBackground");
        bg.transform.SetParent(canvasGO.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = bgRect.anchorMax = Vector2.zero;
        bgRect.pivot     = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = new Vector2(120f, 120f);
        bgRect.sizeDelta = new Vector2(160f, 160f);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.sprite = ProceduralGraphics.CreateJoystickBackgroundSprite();
        bgImg.color  = Color.white;
        bgImg.raycastTarget = true;

        // AddComponent triggers Awake immediately:
        //   background = bg.GetComponent<RectTransform>() — correct
        //   SetVisible(false) → bg.SetActive(false)
        Joystick js = bg.AddComponent<Joystick>();

        // Handle circle (child of bg; added while bg is inactive — that is fine)
        GameObject handleGO = new GameObject("JoystickHandle");
        handleGO.transform.SetParent(bg.transform, false);
        RectTransform handleRect = handleGO.AddComponent<RectTransform>();
        handleRect.anchorMin = handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot     = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(70f, 70f);
        handleRect.anchoredPosition = Vector2.zero;
        Image handleImg = handleGO.AddComponent<Image>();
        handleImg.sprite = ProceduralGraphics.CreateJoystickKnobSprite();
        handleImg.color  = Color.white;

        // Wire the handle field and restore visibility
        SetField(js, "handle", handleRect);
        bg.SetActive(true); // reactivate after Joystick.Awake deactivated it

        return js;
    }

    private static GameHUD CreateHUD(GameObject canvasGO)
    {
        GameObject hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(canvasGO.transform, false);
        RectTransform hudRect = hudGO.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = hudRect.offsetMax = Vector2.zero;

        GameHUD hud = hudGO.AddComponent<GameHUD>();

        // Timer text (top-centre)
        TextMeshProUGUI timerText = CreateTMPText(hudGO, "TimerText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -50f), new Vector2(250f, 60f),
            "10:00", 36, Color.white, TextAlignmentOptions.Center);

        // Leaderboard panel (top-right)
        GameObject lbPanel = new GameObject("LeaderboardPanel");
        lbPanel.transform.SetParent(hudGO.transform, false);
        RectTransform lbRect = lbPanel.AddComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(1f, 1f);
        lbRect.anchorMax = new Vector2(1f, 1f);
        lbRect.pivot     = new Vector2(1f, 1f);
        lbRect.anchoredPosition = new Vector2(-10f, -10f);
        lbRect.sizeDelta = new Vector2(200f, 180f);
        Image lbBg = lbPanel.AddComponent<Image>();
        lbBg.sprite = ProceduralGraphics.CreatePanelSprite();
        lbBg.color  = Color.white;
        lbBg.type   = Image.Type.Simple;

        TextMeshProUGUI lbText = CreateTMPText(lbPanel, "LeaderboardText",
            Vector2.zero, Vector2.one,
            Vector2.zero, new Vector2(-16f, -16f),
            "TOP PLAYERS", 14, Color.white, TextAlignmentOptions.TopLeft);

        // Minimap panel (bottom-right) — background Image + child RawImage for render texture
        GameObject mmPanel = new GameObject("MinimapPanel");
        mmPanel.transform.SetParent(hudGO.transform, false);
        RectTransform mmRect = mmPanel.AddComponent<RectTransform>();
        mmRect.anchorMin = new Vector2(1f, 0f);
        mmRect.anchorMax = new Vector2(1f, 0f);
        mmRect.pivot     = new Vector2(1f, 0f);
        mmRect.anchoredPosition = new Vector2(-10f, 10f);
        mmRect.sizeDelta = new Vector2(120f, 120f);
        Image mmBg = mmPanel.AddComponent<Image>();
        mmBg.color = new Color(0f, 0f, 0f, 0.55f);

        // RawImage on a child so we don't have two Graphic components on one GO
        GameObject mmContent = new GameObject("MinimapContent");
        mmContent.transform.SetParent(mmPanel.transform, false);
        RectTransform mmContentRect = mmContent.AddComponent<RectTransform>();
        mmContentRect.anchorMin = Vector2.zero;
        mmContentRect.anchorMax = Vector2.one;
        mmContentRect.offsetMin = mmContentRect.offsetMax = Vector2.zero;
        RawImage minimapImage = mmContent.AddComponent<RawImage>();
        minimapImage.color = new Color(1f, 1f, 1f, 0f); // transparent until a render-texture is assigned

        // Wire private fields on GameHUD
        SetField(hud, "timerText",      timerText);
        SetField(hud, "leaderboardText", lbText);
        SetField(hud, "minimapImage",    minimapImage);

        return hud;
    }

    private static void CreatePauseButton(GameObject canvasGO)
    {
        GameObject btnGO = new GameObject("PauseButton");
        btnGO.transform.SetParent(canvasGO.transform, false);
        RectTransform btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0f, 1f);
        btnRect.anchorMax = new Vector2(0f, 1f);
        btnRect.pivot = new Vector2(0f, 1f);
        btnRect.anchoredPosition = new Vector2(10f, -10f);
        btnRect.sizeDelta = new Vector2(80f, 50f);

        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0f, 0f, 0f, 0.55f);

        Button btn = btnGO.AddComponent<Button>();

        TextMeshProUGUI label = CreateTMPText(btnGO, "PauseLabel",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "II", 24, Color.white, TextAlignmentOptions.Center);

        btn.onClick.AddListener(() =>
        {
            GameManager gm = Object.FindObjectOfType<GameManager>();
            gm?.TogglePause();
        });
    }

    private static PauseMenu CreatePauseMenu(GameObject canvasGO)
    {
        GameObject panel = new GameObject("PausePanel");
        panel.transform.SetParent(canvasGO.transform, false);
        RectTransform pr = panel.AddComponent<RectTransform>();
        pr.anchorMin = Vector2.zero;
        pr.anchorMax = Vector2.one;
        pr.offsetMin = pr.offsetMax = Vector2.zero;
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.75f);
        panel.SetActive(false);

        PauseMenu pm = panel.AddComponent<PauseMenu>();

        // Title
        CreateTMPText(panel, "TitleText",
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f),
            Vector2.zero, new Vector2(300f, 60f),
            "PAUSED", 48, Color.white, TextAlignmentOptions.Center);

        // Resume button
        Button resumeBtn = CreateUIButton(panel, "ResumeButton",
            new Vector2(0.5f, 0.55f), new Vector2(240f, 55f), "RESUME");

        // Settings button
        Button settingsBtn = CreateUIButton(panel, "SettingsButton",
            new Vector2(0.5f, 0.45f), new Vector2(240f, 55f), "SETTINGS");

        // Main Menu button
        Button menuBtn = CreateUIButton(panel, "MainMenuButton",
            new Vector2(0.5f, 0.35f), new Vector2(240f, 55f), "MAIN MENU");

        // Wire private fields
        SetField(pm, "pausePanel",   panel);
        SetField(pm, "resumeButton", resumeBtn);
        SetField(pm, "settingsButton", settingsBtn);
        SetField(pm, "mainMenuButton", menuBtn);

        return pm;
    }

    private static GameOverScreen CreateGameOverScreen(GameObject canvasGO)
    {
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        RectTransform pr = panel.AddComponent<RectTransform>();
        pr.anchorMin = Vector2.zero;
        pr.anchorMax = Vector2.one;
        pr.offsetMin = pr.offsetMax = Vector2.zero;
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);
        panel.SetActive(false);

        GameOverScreen gos = panel.AddComponent<GameOverScreen>();

        TextMeshProUGUI titleText = CreateTMPText(panel, "TitleText",
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f),
            Vector2.zero, new Vector2(400f, 80f),
            "GAME OVER", 52, Color.white, TextAlignmentOptions.Center);

        TextMeshProUGUI resultsText = CreateTMPText(panel, "ResultsText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400f, 200f),
            "", 20, Color.white, TextAlignmentOptions.Center);

        Button replayBtn = CreateUIButton(panel, "ReplayButton",
            new Vector2(0.5f, 0.3f), new Vector2(200f, 55f), "PLAY AGAIN");

        Button menuBtn = CreateUIButton(panel, "MenuButton",
            new Vector2(0.5f, 0.2f), new Vector2(200f, 55f), "MAIN MENU");

        SetField(gos, "resultText",    titleText);
        SetField(gos, "rankingText",   resultsText);
        SetField(gos, "playAgainButton", replayBtn);
        SetField(gos, "mainMenuButton",  menuBtn);
        SetField(gos, "gameOverPanel",   panel);

        return gos;
    }

    // ── Systems ───────────────────────────────────────────────────────────────

    private static T CreateSystem<T>(string name) where T : Component
    {
        GameObject go = new GameObject(name);
        return go.AddComponent<T>();
    }

    // ── GameManager ───────────────────────────────────────────────────────────

    private static GameManager CreateGameManager(
        Joystick joystick, GameHUD hud, PauseMenu pauseMenu, GameOverScreen gameOverScreen,
        MatchTimer matchTimer, ScoreManager scoreManager, TerritorySystem territory,
        BlackHoleController playerPrefab, BotAI botPrefab)
    {
        GameObject go = new GameObject("GameManager");
        GameManager gm = go.AddComponent<GameManager>();

        SetField(gm, "joystick",         joystick);
        SetField(gm, "hud",              hud);
        SetField(gm, "pauseMenu",        pauseMenu);
        SetField(gm, "gameOverScreen",   gameOverScreen);
        SetField(gm, "matchTimer",       matchTimer);
        SetField(gm, "scoreManager",     scoreManager);
        SetField(gm, "territorySystem",  territory);
        SetField(gm, "playerPrefab",     playerPrefab);
        SetField(gm, "botPrefab",        botPrefab);

        return gm;
    }

    // ── Prefab builders ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates an inactive "prefab" GameObject with all required black-hole
    /// components and a procedurally generated sprite.
    /// </summary>
    private static GameObject BuildBlackHolePrefab<T>(Color color, string goName)
        where T : Component
    {
        GameObject go = new GameObject(goName);
        go.SetActive(false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralGraphics.CreateBlackHoleSprite(color);
        sr.sortingOrder = 2;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        go.AddComponent<MassSystem>();
        go.AddComponent<BlackHolePhysics>();
        go.AddComponent<TerritoryTrail>();
        // BlackHoleController is always required — BotAI has [RequireComponent(typeof(BlackHoleController))]
        go.AddComponent<BlackHoleController>();

        if (typeof(T) == typeof(BotAI))
            go.AddComponent<BotAI>();

        return go;
    }

    /// <summary>
    /// Creates an inactive CityObject "prefab" with a white-pixel sprite
    /// (color is set by CityObject.Initialize later).
    /// </summary>
    private static GameObject BuildCityObjectPrefab()
    {
        GameObject go = new GameObject("CityObject_Prefab");
        go.SetActive(false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralGraphics.CreateWhitePixelSprite();
        sr.sortingOrder = 0;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.drag = 5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        go.AddComponent<CityObject>();

        return go;
    }

    // ── Camera wiring coroutine ───────────────────────────────────────────────

    private static System.Collections.IEnumerator WireCamera(CameraFollow camFollow)
    {
        // Wait for GameManager.InitialiseMatch coroutine (runs after 1 frame)
        yield return null;
        yield return null;

        // Camera auto-finds the player on its own; explicit wire kept for safety
        BlackHoleController player = Object.FindObjectOfType<BlackHoleController>();
        if (player != null)
            SetField(camFollow, "target", player.transform);

        // Apply distinct procedural sprites to each bot using its assigned trail colour
        BotAI[] bots = Object.FindObjectsOfType<BotAI>();
        foreach (BotAI bot in bots)
        {
            TerritoryTrail trail = bot.GetComponent<TerritoryTrail>();
            SpriteRenderer  sr   = bot.GetComponent<SpriteRenderer>();
            if (trail != null && sr != null)
                sr.sprite = ProceduralGraphics.CreateBlackHoleSprite(trail.PlayerColor);
        }
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    private static TextMeshProUGUI CreateTMPText(
        GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 sizeDelta,
        string text, float fontSize, Color color,
        TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta        = sizeDelta;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = alignment;

        return tmp;
    }

    private static Button CreateUIButton(
        GameObject parent, string name,
        Vector2 anchoredCenter, Vector2 size, string label)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchoredCenter;
        rt.anchorMax = anchoredCenter;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.18f, 0.92f);

        Button btn = go.AddComponent<Button>();

        CreateTMPText(go, "Label",
            Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero,
            label, 22, Color.white, TextAlignmentOptions.Center);

        return btn;
    }

    // ── Reflection helper ─────────────────────────────────────────────────────

    /// <summary>
    /// Sets a private or public instance field by name using reflection.
    /// Walks the inheritance chain so fields declared in base classes are found.
    /// </summary>
    internal static void SetField(object target, string fieldName, object value)
    {
        if (target == null) return;
        System.Type type = target.GetType();
        while (type != null)
        {
            FieldInfo fi = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fi != null)
            {
                fi.SetValue(target, value);
                return;
            }
            type = type.BaseType;
        }
        Debug.LogWarning($"[AutoSceneSetup] Field '{fieldName}' not found on {target.GetType().Name}");
    }
}
