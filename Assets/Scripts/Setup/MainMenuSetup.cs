using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Builds the Main Menu scene entirely at runtime — no scene file or external
/// assets required.  Call <see cref="Build"/> to construct the menu.
/// The menu is also constructed automatically when
/// <see cref="GameBootstrapper.ShowMenuOnBoot"/> is set before scene load.
/// </summary>
public static class MainMenuSetup
{
    // ── Public surface ────────────────────────────────────────────────────────

    /// <summary>Creates the complete main-menu hierarchy in the active scene.</summary>
    public static void Build()
    {
        Debug.Log("[MainMenuSetup] Building main menu…");

        EnsureEventSystem();

        Camera cam = CreateCamera();

        GameObject canvasGO = CreateCanvas();
        CreateBackground(canvasGO);
        CreateAnimatedBlackHole(canvasGO);
        CreateStarField(canvasGO);
        CreateTitle(canvasGO);
        CreateButtons(canvasGO);

        Debug.Log("[MainMenuSetup] Main menu ready.");
    }

    // ── Camera ────────────────────────────────────────────────────────────────

    private static Camera CreateCamera()
    {
        if (Camera.main != null) return Camera.main;

        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        Camera cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f);
        cam.orthographic = true;
        cam.orthographicSize = 10f;
        go.transform.position = new Vector3(0f, 0f, -10f);
        go.AddComponent<AudioListener>();
        return cam;
    }

    // ── EventSystem ───────────────────────────────────────────────────────────

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    // ── Canvas ────────────────────────────────────────────────────────────────

    private static GameObject CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        return canvasGO;
    }

    // ── Background ────────────────────────────────────────────────────────────

    private static void CreateBackground(GameObject canvasGO)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = bg.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = bg.AddComponent<Image>();
        img.color = new Color(0.02f, 0.02f, 0.06f);
    }

    // ── Animated black hole (background decoration) ───────────────────────────

    private static void CreateAnimatedBlackHole(GameObject canvasGO)
    {
        GameObject go = new GameObject("AnimatedBlackHole");
        go.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(300f, -80f);
        rt.sizeDelta = new Vector2(400f, 400f);

        Image img = go.AddComponent<Image>();
        img.sprite = ProceduralGraphics.CreateBlackHoleSprite(new Color(0.4f, 0.1f, 0.7f));
        img.color  = new Color(1f, 1f, 1f, 0.35f);

        go.AddComponent<RotateForever>();
    }

    // ── Star field ────────────────────────────────────────────────────────────

    private static void CreateStarField(GameObject canvasGO)
    {
        // Fixed seed produces the same star layout every time (reproducible look)
        System.Random rng = new System.Random(12345);
        for (int i = 0; i < 80; i++)
        {
            GameObject star = new GameObject($"Star_{i}");
            star.transform.SetParent(canvasGO.transform, false);
            RectTransform rt = star.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(
                (float)rng.NextDouble(),
                (float)rng.NextDouble());
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            float sz = (float)(rng.NextDouble() * 4f + 1f);
            rt.sizeDelta = new Vector2(sz, sz);

            Image img = star.AddComponent<Image>();
            float brightness = (float)(rng.NextDouble() * 0.5f + 0.5f);
            img.color = new Color(brightness, brightness, brightness, 0.8f);
        }
    }

    // ── Title ─────────────────────────────────────────────────────────────────

    private static void CreateTitle(GameObject canvasGO)
    {
        // Shadow layer
        CreateTitleText(canvasGO, "TitleShadow",
            new Vector2(0.5f, 0.78f), new Vector2(4f, -4f),
            new Color(0.4f, 0f, 0.6f, 0.6f));

        // Main title
        CreateTitleText(canvasGO, "Title",
            new Vector2(0.5f, 0.78f), Vector2.zero,
            Color.white);

        // Sub-title tagline
        GameObject sub = new GameObject("Subtitle");
        sub.transform.SetParent(canvasGO.transform, false);
        RectTransform subRT = sub.AddComponent<RectTransform>();
        subRT.anchorMin = subRT.anchorMax = new Vector2(0.5f, 0.68f);
        subRT.pivot     = new Vector2(0.5f, 0.5f);
        subRT.anchoredPosition = Vector2.zero;
        subRT.sizeDelta = new Vector2(600f, 40f);
        TextMeshProUGUI tmp = sub.AddComponent<TextMeshProUGUI>();
        tmp.text      = "Consume. Capture. Dominate.";
        tmp.fontSize  = 24f;
        tmp.color     = new Color(0.8f, 0.7f, 1f, 0.85f);
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private static void CreateTitleText(GameObject canvasGO, string name,
        Vector2 anchor, Vector2 offset, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(700f, 100f);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = "BLACK HOLE";
        tmp.fontSize   = 80f;
        tmp.fontStyle  = FontStyles.Bold;
        tmp.color      = color;
        tmp.alignment  = TextAlignmentOptions.Center;
    }

    // ── Buttons ───────────────────────────────────────────────────────────────

    private static void CreateButtons(GameObject canvasGO)
    {
        // PLAY
        Button playBtn = CreateMenuButton(canvasGO, "PlayButton",
            new Vector2(0.5f, 0.53f), "PLAY",
            new Color(0.3f, 0.1f, 0.7f, 0.92f));
        playBtn.onClick.AddListener(OnPlayClicked);

        // SETTINGS (placeholder — opens nothing in prototype)
        Button settingsBtn = CreateMenuButton(canvasGO, "SettingsButton",
            new Vector2(0.5f, 0.42f), "SETTINGS",
            new Color(0.15f, 0.15f, 0.2f, 0.85f));
        settingsBtn.onClick.AddListener(OnSettingsClicked);

        // QUIT
        Button quitBtn = CreateMenuButton(canvasGO, "QuitButton",
            new Vector2(0.5f, 0.31f), "QUIT",
            new Color(0.15f, 0.15f, 0.2f, 0.85f));
        quitBtn.onClick.AddListener(OnQuitClicked);

        // Version label
        GameObject ver = new GameObject("Version");
        ver.transform.SetParent(canvasGO.transform, false);
        RectTransform vrt = ver.AddComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.zero;
        vrt.pivot = Vector2.zero;
        vrt.anchoredPosition = new Vector2(10f, 10f);
        vrt.sizeDelta = new Vector2(200f, 30f);
        TextMeshProUGUI vtmp = ver.AddComponent<TextMeshProUGUI>();
        vtmp.text = "v1.0 (prototype)";
        vtmp.fontSize = 14f;
        vtmp.color = new Color(1f, 1f, 1f, 0.4f);
    }

    private static Button CreateMenuButton(
        GameObject canvasGO, string name, Vector2 anchor, string label, Color bgColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(280f, 65f);

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();

        // Label
        GameObject lbl = new GameObject("Label");
        lbl.transform.SetParent(go.transform, false);
        RectTransform lrt = lbl.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 28f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private static void OnPlayClicked()
    {
        // Destroy menu objects and build game scene inline (no scene file needed)
        // Use GameBootstrapper's helper so the persistent manager is preserved.
        if (GameBootstrapper.Instance != null)
        {
            GameBootstrapper.Instance.RestartGame();
        }
        else
        {
            // Fallback: destroy everything and rebuild
            foreach (var root in UnityEngine.SceneManagement.SceneManager
                                            .GetActiveScene().GetRootGameObjects())
            {
                Object.Destroy(root);
            }
            GameBootstrapper.ShowMenuOnBoot = false;
            AutoSceneSetup.BuildGameScene();
        }
    }

    private static void OnSettingsClicked()
    {
        Debug.Log("[MainMenuSetup] Settings not yet implemented.");
    }

    private static void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

// ── Helper MonoBehaviour ───────────────────────────────────────────────────────

/// <summary>Rotates a UI element forever (used for the background black hole).</summary>
public class RotateForever : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 20f;

    private void Update()
    {
        transform.Rotate(0f, 0f, -degreesPerSecond * Time.deltaTime);
    }
}
