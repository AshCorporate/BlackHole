using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings screen controller.
/// Exposes controls for sound toggle, joystick sensitivity, and player name.
///
/// Scene setup (inside Canvas):
///   SettingsPanel
///     ├── TitleText (TextMeshProUGUI – "SETTINGS")
///     ├── SoundToggle (Toggle)
///     ├── SensitivitySlider (Slider, range 0.1–2.0)
///     ├── PlayerNameInput (TMP_InputField)
///     └── BackButton (Button)
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private Toggle        soundToggle;
    [SerializeField] private Slider        sensitivitySlider;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button         backButton;

    // ── PlayerPrefs keys ───────────────────────────────────────────────────────
    private const string KEY_SOUND       = "sound_enabled";
    private const string KEY_SENSITIVITY = "joystick_sensitivity";
    private const string KEY_NAME        = "player_name";

    // ── Private ────────────────────────────────────────────────────────────────
    private System.Action _onBack;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        backButton?.onClick.AddListener(OnBackClicked);
        soundToggle?.onValueChanged.AddListener(OnSoundChanged);
        sensitivitySlider?.onValueChanged.AddListener(OnSensitivityChanged);
        playerNameInput?.onEndEdit.AddListener(OnNameChanged);
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Shows the settings panel and calls onBack when the back button is pressed.</summary>
    public void Show(System.Action onBack)
    {
        _onBack = onBack;
        LoadSettings();
    }

    // ── Static Helpers ─────────────────────────────────────────────────────────

    public static bool   SoundEnabled      => PlayerPrefs.GetInt(KEY_SOUND, 1) == 1;
    public static float  JoystickSensitivity => PlayerPrefs.GetFloat(KEY_SENSITIVITY, 1f);
    public static string PlayerName        => PlayerPrefs.GetString(KEY_NAME, "Player");

    // ── Private ────────────────────────────────────────────────────────────────

    private void LoadSettings()
    {
        if (soundToggle != null)
            soundToggle.isOn = SoundEnabled;

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.1f;
            sensitivitySlider.maxValue = 2f;
            sensitivitySlider.value = JoystickSensitivity;
        }

        if (playerNameInput != null)
            playerNameInput.text = PlayerName;
    }

    private void OnSoundChanged(bool value)
    {
        PlayerPrefs.SetInt(KEY_SOUND, value ? 1 : 0);
        AudioListener.volume = value ? 1f : 0f;
    }

    private void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, value);
    }

    private void OnNameChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            PlayerPrefs.SetString(KEY_NAME, value.Trim());
    }

    private void OnBackClicked()
    {
        PlayerPrefs.Save();
        _onBack?.Invoke();
    }
}
