using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Auto-setup bootstrapper that builds the entire BlackHole game scene at runtime.
/// Activated via [RuntimeInitializeOnLoadMethod(AfterSceneLoad)] so it fires
/// automatically when pressing ▶ Play on ANY scene — even an empty one.
///
/// Execution order inside Unity 6:
///   1. AfterSceneLoad fires (all existing Awake calls done, Start not yet called)
///   2. Bootstrap() checks for an existing GameManager → skips if already set up
///   3. Creates Camera, Map visuals, Game Systems, and full UI
///   4. Start() fires for every object:
///        - GameManager auto-discovers scene refs and spawns Player + Bots
///        - ObjectSpawner spawns City Objects using its built-in prefab fallback
///        - BuffSpawner spawns initial Buffs immediately
///        - CameraFollow tracks the Player automatically
/// </summary>
public static class GameBootstrapper
{
    // Map radius — mirrors GameConfig.mapRadius default value
    private const float MapRadius = 50f;

    // Cached text references wired between helper methods
    private static TextMeshProUGUI s_timerText;
    private static TextMeshProUGUI s_leaderboardText;

    // ── Entry Point ────────────────────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        // Skip if the scene already has a GameManager (properly set-up scene)
        if (Object.FindFirstObjectByType<GameManager>() != null) return;

        Debug.Log("[GameBootstrapper] Auto-building BlackHole game scene (Unity 6)...");

        RemoveDirectionalLight();
        SetupCamera();
        CreateMap();
        CreateGameSystems();
        CreateUI();

        Debug.Log("[GameBootstrapper] Scene ready — press Play to start!");
    }

    // ── Step 1 — Camera ────────────────────────────────────────────────────────

    static void SetupCamera()
    {
        Camera mainCam = Camera.main;

        if (mainCam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCam = camGO.AddComponent<Camera>();
            // Add AudioListener only if none exists in the scene
            if (Object.FindFirstObjectByType<AudioListener>() == null)
                camGO.AddComponent<AudioListener>();
        }

        mainCam.orthographic = true;
        mainCam.orthographicSize = 8f;
        mainCam.backgroundColor = new Color(0.05f, 0.02f, 0.15f);  // dark navy/space
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.transform.position = new Vector3(0f, 0f, -10f);

        if (mainCam.GetComponent<CameraFollow>() == null)
            mainCam.gameObject.AddComponent<CameraFollow>();
    }

    static void RemoveDirectionalLight()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in lights)
        {
            if (l != null && l.type == LightType.Directional)
                Object.Destroy(l.gameObject);
        }
    }

    // ── Step 2 — Map ───────────────────────────────────────────────────────────

    static void CreateMap()
    {
        // MapGenerator handles the full map setup (ground, border, roads) in its Start()
        new GameObject("Map").AddComponent<MapGenerator>();
    }

    // ── Step 3 — Game Systems ──────────────────────────────────────────────────

    static void CreateGameSystems()
    {
        // Order matters: systems that others depend on come first
        new GameObject("TerritorySystem").AddComponent<TerritorySystem>();
        new GameObject("ScoreManager").AddComponent<ScoreManager>();
        new GameObject("MatchTimer").AddComponent<MatchTimer>();
        new GameObject("ObjectSpawner").AddComponent<ObjectSpawner>();
        new GameObject("BuffSpawner").AddComponent<BuffSpawner>();

        // GameManager last — its Start() auto-discovers the objects above and spawns players
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    // ── Step 4 — UI ────────────────────────────────────────────────────────────

    static void CreateUI()
    {
        // EventSystem — required for all UI interaction
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        // Root Canvas — Screen Space Overlay, resolution-independent scaling
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Build all HUD widgets ──────────────────────────────────────────────
        s_timerText        = BuildTimerWidget(canvasGO);
        s_leaderboardText  = BuildLeaderboardWidget(canvasGO);
        BuildJoystick(canvasGO);
        BuildPauseButton(canvasGO);
        BuildPauseMenuPanel(canvasGO);
        BuildGameOverPanel(canvasGO);

        // GameHUD ties the timer and leaderboard text to live game data
        // (auto-discovers MatchTimer and ScoreManager in its own Start())
        GameHUD hud = canvasGO.AddComponent<GameHUD>();
        SetField(hud, "timerText", s_timerText);
        SetField(hud, "leaderboardText", s_leaderboardText);
    }

    // ── UI Widget: Timer (top-center) ──────────────────────────────────────────

    static TextMeshProUGUI BuildTimerWidget(GameObject canvas)
    {
        GameObject go = new GameObject("TimerText");
        go.transform.SetParent(canvas.transform, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = "10:00";
        tmp.fontSize = 42f;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -20f);
        rt.sizeDelta        = new Vector2(300f, 60f);

        return tmp;
    }

    // ── UI Widget: Leaderboard (top-right) ────────────────────────────────────

    static TextMeshProUGUI BuildLeaderboardWidget(GameObject canvas)
    {
        // Semi-transparent panel
        GameObject panelGO = new GameObject("LeaderboardPanel");
        panelGO.transform.SetParent(canvas.transform, false);

        Image bg = panelGO.AddComponent<Image>();
        bg.color  = new Color(0f, 0f, 0f, 0.65f);
        bg.sprite = ProceduralSprites.CreateWhiteSquare();

        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1f, 1f);
        panelRT.anchorMax = new Vector2(1f, 1f);
        panelRT.pivot     = new Vector2(1f, 1f);
        panelRT.anchoredPosition = new Vector2(-20f, -20f);
        panelRT.sizeDelta        = new Vector2(260f, 220f);

        // Content text
        GameObject textGO = new GameObject("LeaderboardText");
        textGO.transform.SetParent(panelGO.transform, false);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "🏆 Leaderboard";
        tmp.fontSize  = 18f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10f, 8f);
        textRT.offsetMax = new Vector2(-8f, -8f);

        // Wire Leaderboard component
        Leaderboard lb = panelGO.AddComponent<Leaderboard>();
        SetField(lb, "contentText", tmp);

        return tmp;
    }

    // ── UI Widget: Joystick (bottom-left) ─────────────────────────────────────
    //
    // Architecture (floating joystick):
    //   Canvas
    //   ├── JoystickArea  — transparent panel, bottom-left, holds Joystick component
    //   ├── JoystickBackground — visual circle, direct child of Canvas
    //   │     └── JoystickHandle — inner knob
    //
    // The Joystick component is on JoystickArea so the area stays active (receives
    // pointer events) while the visual background hides/shows as needed.
    // Having JoystickBackground as a direct child of Canvas ensures that
    // background.anchoredPosition = canvas-local-click-pos works correctly.
    //
    // Because Joystick.Awake() auto-assigns background = self.RectTransform when
    // background is null, and we cannot pre-set it before AddComponent, the fix is:
    //   • Joystick.cs now defers SetVisible(false) to Start() instead of Awake().
    //   • The bootstrapper overrides background/handle via reflection after Awake.
    //   • Start() then calls SetVisible(false) with the correctly set background.

    static void BuildJoystick(GameObject canvas)
    {
        // Visual background circle — DIRECT child of Canvas for correct canvas-space positioning
        GameObject bgGO = new GameObject("JoystickBackground");
        bgGO.transform.SetParent(canvas.transform, false);

        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color         = new Color(0.15f, 0.15f, 0.15f, 0.75f);
        bgImg.sprite        = ProceduralSprites.CreateCircleSprite(128, Color.white);
        bgImg.raycastTarget = false;   // only the area panel handles raycasts

        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0f);
        bgRT.anchorMax = new Vector2(0f, 0f);
        bgRT.pivot     = new Vector2(0.5f, 0.5f);
        bgRT.anchoredPosition = new Vector2(160f, 160f);   // default position hint
        bgRT.sizeDelta        = new Vector2(200f, 200f);

        // Visual handle knob — child of background
        GameObject handleGO = new GameObject("JoystickHandle");
        handleGO.transform.SetParent(bgGO.transform, false);

        Image handleImg = handleGO.AddComponent<Image>();
        handleImg.color         = new Color(0.7f, 0.7f, 0.7f, 0.85f);
        handleImg.sprite        = ProceduralSprites.CreateCircleSprite(64, Color.white);
        handleImg.raycastTarget = false;

        RectTransform handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0.5f, 0.5f);
        handleRT.anchorMax = new Vector2(0.5f, 0.5f);
        handleRT.pivot     = new Vector2(0.5f, 0.5f);
        handleRT.anchoredPosition = Vector2.zero;
        handleRT.sizeDelta        = new Vector2(80f, 80f);

        // Transparent input area — bottom-left quarter of screen, always active
        GameObject areaGO = new GameObject("JoystickArea");
        areaGO.transform.SetParent(canvas.transform, false);

        Image areaImg = areaGO.AddComponent<Image>();
        areaImg.color = Color.clear;   // invisible but receives pointer events

        RectTransform areaRT = areaGO.GetComponent<RectTransform>();
        areaRT.anchorMin = Vector2.zero;
        areaRT.anchorMax = new Vector2(0.45f, 0.45f);
        areaRT.offsetMin = Vector2.zero;
        areaRT.offsetMax = Vector2.zero;

        // Add Joystick component — Awake() will temporarily set background = areaRT,
        // but we override it immediately after via reflection.
        // Joystick.Start() (not Awake) calls SetVisible(false), so bgGO is hidden then.
        Joystick joystick = areaGO.AddComponent<Joystick>();
        SetField(joystick, "background", bgRT);
        SetField(joystick, "handle",     handleRT);
    }

    // ── UI Widget: Pause Button (top-left) ────────────────────────────────────

    static void BuildPauseButton(GameObject canvas)
    {
        GameObject btnGO = new GameObject("PauseButton");
        btnGO.transform.SetParent(canvas.transform, false);

        Image img = btnGO.AddComponent<Image>();
        img.color  = new Color(0.1f, 0.1f, 0.1f, 0.75f);
        img.sprite = ProceduralSprites.CreateWhiteSquare();

        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            gm?.TogglePause();
        });

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);
        rt.sizeDelta        = new Vector2(70f, 55f);

        // Pause icon label
        GameObject lblGO = new GameObject("PauseIcon");
        lblGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text          = "⏸";
        lbl.fontSize      = 28f;
        lbl.color         = Color.white;
        lbl.alignment     = TextAlignmentOptions.Center;
        lbl.raycastTarget = false;
        RectTransform lblRT = lblGO.GetComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
    }

    // ── UI Widget: Pause Menu Panel (hidden by default) ───────────────────────

    static void BuildPauseMenuPanel(GameObject canvas)
    {
        GameObject panelGO = new GameObject("PausePanel");
        panelGO.transform.SetParent(canvas.transform, false);

        Image overlay = panelGO.AddComponent<Image>();
        overlay.color  = new Color(0f, 0f, 0f, 0.80f);
        overlay.sprite = ProceduralSprites.CreateWhiteSquare();

        RectTransform rt = panelGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        CreateTMPLabel(panelGO, "PAUSED", 48f, Color.white, FontStyles.Bold,
            new Vector2(0.5f, 0.72f), new Vector2(400f, 70f));

        Button resumeBtn   = CreateMenuButton(panelGO, "RESUME",    new Vector2(0.5f, 0.55f));
        Button mainMenuBtn = CreateMenuButton(panelGO, "MAIN MENU", new Vector2(0.5f, 0.42f));

        // PauseMenu component — Awake already ran with null fields, so wire manually
        PauseMenu pauseMenu = panelGO.AddComponent<PauseMenu>();
        SetField(pauseMenu, "pausePanel",    panelGO);
        SetField(pauseMenu, "resumeButton",  resumeBtn);
        SetField(pauseMenu, "mainMenuButton", mainMenuBtn);

        // Wire button listeners (PauseMenu.Awake ran before fields were set)
        resumeBtn.onClick.AddListener(pauseMenu.Resume);
        mainMenuBtn.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        });

        panelGO.SetActive(false);
    }

    // ── UI Widget: Game Over Panel (hidden by default) ────────────────────────

    static void BuildGameOverPanel(GameObject canvas)
    {
        GameObject panelGO = new GameObject("GameOverPanel");
        panelGO.transform.SetParent(canvas.transform, false);

        Image overlay = panelGO.AddComponent<Image>();
        overlay.color  = new Color(0f, 0f, 0f, 0.85f);
        overlay.sprite = ProceduralSprites.CreateWhiteSquare();

        RectTransform rt = panelGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        TextMeshProUGUI resultTMP  = CreateTMPLabel(panelGO, "GAME OVER", 56f, Color.red,
            FontStyles.Bold, new Vector2(0.5f, 0.72f), new Vector2(600f, 80f));

        TextMeshProUGUI rankingTMP = CreateTMPLabel(panelGO, "", 20f, Color.white,
            FontStyles.Normal, new Vector2(0.5f, 0.52f), new Vector2(500f, 200f));
        rankingTMP.alignment = TextAlignmentOptions.Top;

        Button playAgainBtn = CreateMenuButton(panelGO, "PLAY AGAIN", new Vector2(0.5f, 0.30f));
        Button mainMenuBtn  = CreateMenuButton(panelGO, "MAIN MENU",  new Vector2(0.5f, 0.18f));

        // GameOverScreen component
        GameOverScreen gameOver = panelGO.AddComponent<GameOverScreen>();
        SetField(gameOver, "gameOverPanel",    panelGO);
        SetField(gameOver, "resultText",       resultTMP);
        SetField(gameOver, "rankingText",      rankingTMP);
        SetField(gameOver, "playAgainButton",  playAgainBtn);
        SetField(gameOver, "mainMenuButton",   mainMenuBtn);

        // Wire button listeners
        // "Play Again" reloads the current scene (works from any scene name)
        playAgainBtn.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        // "Main Menu" matches the scene name used across the rest of the project
        mainMenuBtn.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        });

        panelGO.SetActive(false);
    }

    // ── UI Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Creates a TextMeshProUGUI label anchored at a normalised position.</summary>
    static TextMeshProUGUI CreateTMPLabel(GameObject parent, string text,
        float fontSize, Color color, FontStyles style,
        Vector2 anchorCenter, Vector2 size)
    {
        GameObject go = new GameObject("Label_" + text.Replace(" ", "_"));
        go.transform.SetParent(parent.transform, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorCenter;
        rt.anchorMax = anchorCenter;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = size;

        return tmp;
    }

    /// <summary>Creates a dark-background button with a text label at a normalised anchor.</summary>
    static Button CreateMenuButton(GameObject parent, string label, Vector2 anchorCenter)
    {
        GameObject btnGO = new GameObject("Btn_" + label.Replace(" ", "_"));
        btnGO.transform.SetParent(parent.transform, false);

        Image bg = btnGO.AddComponent<Image>();
        bg.color  = new Color(0.15f, 0.10f, 0.30f, 0.95f);
        bg.sprite = ProceduralSprites.CreateWhiteSquare();

        Button btn = btnGO.AddComponent<Button>();

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = anchorCenter;
        rt.anchorMax = anchorCenter;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = new Vector2(280f, 55f);

        // Text label
        GameObject lblGO = new GameObject("Label");
        lblGO.transform.SetParent(btnGO.transform, false);

        TextMeshProUGUI tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text          = label;
        tmp.fontSize      = 26f;
        tmp.color         = Color.white;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        RectTransform lblRT = lblGO.GetComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;

        return btn;
    }

    // ── Reflection Helper ──────────────────────────────────────────────────────

    /// <summary>
    /// Sets a serialized (private or protected) field on a MonoBehaviour via reflection.
    /// Logs a warning if the field name is not found, but never throws.
    /// </summary>
    static void SetField(Component target, string fieldName, object value)
    {
        if (target == null) return;

        FieldInfo field = target.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        if (field != null)
            field.SetValue(target, value);
        else
            Debug.LogWarning(
                $"[GameBootstrapper] Field '{fieldName}' not found on {target.GetType().Name}.");
    }
}
