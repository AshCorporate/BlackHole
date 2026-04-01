using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause menu — shown when the player pauses the game.
///
/// Scene setup (inside Canvas):
///   PausePanel
///     ├── TitleText ("PAUSED")
///     ├── ResumeButton
///     ├── SettingsButton
///     └── MainMenuButton
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button     resumeButton;
    [SerializeField] private Button     settingsButton;
    [SerializeField] private Button     mainMenuButton;
    [SerializeField] private SettingsMenu settingsMenu;

    // ── State ──────────────────────────────────────────────────────────────────
    public bool IsPaused { get; private set; }

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        resumeButton?.onClick.AddListener(Resume);
        settingsButton?.onClick.AddListener(OpenSettings);
        mainMenuButton?.onClick.AddListener(GoToMainMenu);

        pausePanel?.SetActive(false);
    }

    private void Update()
    {
        // Android back button / keyboard escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        pausePanel?.SetActive(true);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        pausePanel?.SetActive(false);
        settingsMenu?.gameObject.SetActive(false);
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void OpenSettings()
    {
        pausePanel?.SetActive(false);
        settingsMenu?.gameObject.SetActive(true);
        settingsMenu?.Show(() =>
        {
            settingsMenu.gameObject.SetActive(false);
            pausePanel?.SetActive(true);
        });
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
