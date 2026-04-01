using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Main Menu screen controller.
/// Handles Play, Settings, and Quit buttons.
///
/// Scene setup (MainMenu.unity):
///   Canvas
///     ├── Background (Image – black/space)
///     ├── Title (TextMeshProUGUI – "BLACK HOLE")
///     ├── PlayButton (Button)
///     ├── SettingsButton (Button)
///     └── QuitButton (Button)
/// </summary>
public class MainMenu : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private SettingsMenu settingsPanel;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Start()
    {
        // Wire up buttons
        playButton?.onClick.AddListener(OnPlayClicked);
        settingsButton?.onClick.AddListener(OnSettingsClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        ShowMainPanel();
    }

    // ── Button Handlers ────────────────────────────────────────────────────────

    private void OnPlayClicked()
    {
        SceneManager.LoadScene("Game");
    }

    private void OnSettingsClicked()
    {
        mainPanel?.SetActive(false);
        settingsPanel?.gameObject.SetActive(true);
        settingsPanel?.Show(OnSettingsClosed);
    }

    private void OnSettingsClosed()
    {
        ShowMainPanel();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void ShowMainPanel()
    {
        mainPanel?.SetActive(true);
        settingsPanel?.gameObject.SetActive(false);
    }
}
