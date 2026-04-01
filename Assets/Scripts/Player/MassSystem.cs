using UnityEngine;

/// <summary>
/// Tracks and manages the mass of a black hole.
/// Mass drives size, absorption capability, and movement speed.
/// </summary>
public class MassSystem : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;

    // ── Public State ───────────────────────────────────────────────────────────
    /// <summary>Current mass of this black hole.</summary>
    public float Mass { get; private set; }

    /// <summary>Current radius derived from mass and config scale factors.</summary>
    public float Radius { get; private set; }

    // ── Events ─────────────────────────────────────────────────────────────────
    public System.Action<float> OnMassChanged;   // fired with new mass value

    // ── Private ────────────────────────────────────────────────────────────────
    private float _doubleMassTimer;
    private bool _doubleMassActive;

    // ──────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");

        Mass = config != null ? config.startMass : 5f;
        UpdateRadius();
    }

    private void Update()
    {
        // Countdown for double-mass buff
        if (_doubleMassActive)
        {
            _doubleMassTimer -= Time.deltaTime;
            if (_doubleMassTimer <= 0f)
                _doubleMassActive = false;
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds mass to the black hole (respects double-mass buff).
    /// </summary>
    public void AddMass(float amount)
    {
        if (amount <= 0f) return;
        float multiplier = _doubleMassActive ? 2f : 1f;
        Mass += amount * multiplier;
        UpdateRadius();
        OnMassChanged?.Invoke(Mass);
    }

    /// <summary>
    /// Removes mass (e.g., after respawn penalty).
    /// </summary>
    public void RemoveMass(float amount)
    {
        Mass = Mathf.Max(config != null ? config.startMass : 5f, Mass - amount);
        UpdateRadius();
        OnMassChanged?.Invoke(Mass);
    }

    /// <summary>
    /// Sets mass directly (used for respawning).
    /// </summary>
    public void SetMass(float value)
    {
        Mass = Mathf.Max(0.1f, value);
        UpdateRadius();
        OnMassChanged?.Invoke(Mass);
    }

    /// <summary>
    /// Returns true if this hole can absorb an object of the given mass.
    /// </summary>
    public bool CanAbsorb(float targetMass)
    {
        return Mass >= targetMass;
    }

    /// <summary>
    /// Activates the Double Mass buff for the configured duration.
    /// </summary>
    public void ActivateDoubleMass()
    {
        _doubleMassActive = true;
        _doubleMassTimer = config != null ? config.doubleMassDuration : 8f;
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void UpdateRadius()
    {
        float minScale = config != null ? config.minScale : 0.4f;
        float scaleFactor = config != null ? config.scaleFactor : 0.02f;
        float scale = minScale + Mass * scaleFactor;
        Radius = scale / 2f;

        // Apply scale to the GameObject
        transform.localScale = Vector3.one * scale;
    }
}
