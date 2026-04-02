using UnityEngine;

/// <summary>
/// Handles the physical movement of the black hole and object-absorption physics.
/// Works with MassSystem to scale pull force and speed.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MassSystem))]
public class BlackHolePhysics : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;

    // ── Public State ───────────────────────────────────────────────────────────
    /// <summary>Normalised input direction set each frame by the controller.</summary>
    public Vector2 InputDirection { get; set; }

    /// <summary>Current movement speed (affected by mass and buffs).</summary>
    public float CurrentSpeed { get; private set; }

    /// <summary>When true movement input is ignored (stun state).</summary>
    public bool IsStunned { get; set; }

    // ── Private ────────────────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private MassSystem _massSystem;

    private float _externalSpeedModifier = 0f;
    private float _speedBoostMultiplier = 1f;
    private float _speedBoostTimer;
    private bool _magnetActive;
    private float _magnetTimer;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _massSystem = GetComponent<MassSystem>();

        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");

        // Black holes are not affected by gravity
        _rb.gravityScale = 0f;
        _rb.linearDamping = 2f;
        _rb.angularDamping = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        UpdateBuffTimers();
        ApplyMovement();
        KeepInsideMap();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Adds a speed modifier (e.g. -0.3f for -30%). Can be called multiple times; they stack additively.</summary>
    public void AddSpeedModifier(float delta)
    {
        _externalSpeedModifier += delta;
    }

    /// <summary>Activates the speed boost buff.</summary>
    public void ActivateSpeedBoost()
    {
        _speedBoostMultiplier = config != null ? config.speedBoostMultiplier : 2f;
        _speedBoostTimer = config != null ? config.speedBoostDuration : 5f;
    }

    /// <summary>Activates the magnet buff that pulls nearby objects.</summary>
    public void ActivateMagnet()
    {
        _magnetActive = true;
        _magnetTimer = config != null ? config.magnetDuration : 6f;
    }

    /// <summary>
    /// Attracts a Rigidbody2D toward this black hole (called by CityObject when in range).
    /// </summary>
    public void PullObject(Rigidbody2D targetRb)
    {
        if (targetRb == null) return;
        Vector2 direction = ((Vector2)transform.position - targetRb.position).normalized;
        float force = (config != null ? config.absorptionForce : 8f) * (_magnetActive ? 2f : 1f);
        targetRb.AddForce(direction * force, ForceMode2D.Force);
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void ApplyMovement()
    {
        if (IsStunned)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float baseSpeed = config != null ? config.baseSpeed : 6f;
        float penalty   = config != null ? config.speedMassPenalty : 0.03f;
        float minSpeed  = config != null ? config.minSpeed : 2f;

        float rawSpeed = (baseSpeed - _massSystem.Mass * penalty) * _speedBoostMultiplier;
        rawSpeed *= (1f + _externalSpeedModifier);               // apply stacked modifiers
        CurrentSpeed = Mathf.Max(minSpeed, rawSpeed);

        _rb.linearVelocity = InputDirection.normalized * CurrentSpeed;
    }

    private void KeepInsideMap()
    {
        float mapSize = config != null ? config.mapSize : 100f;
        float half    = mapSize * 0.5f;
        float r       = _massSystem.Radius;
        Vector2 pos   = _rb.position;
        bool changed  = false;

        if (pos.x < -half + r) { pos.x = -half + r; changed = true; }
        if (pos.x >  half - r) { pos.x =  half - r; changed = true; }
        if (pos.y < -half + r) { pos.y = -half + r; changed = true; }
        if (pos.y >  half - r) { pos.y =  half - r; changed = true; }

        if (changed) _rb.MovePosition(pos);
    }

    private void UpdateBuffTimers()
    {
        if (_speedBoostTimer > 0f)
        {
            _speedBoostTimer -= Time.fixedDeltaTime;
            if (_speedBoostTimer <= 0f)
                _speedBoostMultiplier = 1f;
        }

        if (_magnetTimer > 0f)
        {
            _magnetTimer -= Time.fixedDeltaTime;
            if (_magnetTimer <= 0f)
                _magnetActive = false;
        }

        // Magnet effect: attract all rigidbodies in range
        if (_magnetActive)
        {
            float radius = config != null ? config.magnetRadius : 8f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var col in hits)
            {
                if (col.gameObject == gameObject) continue;
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb != null)
                    PullObject(rb);
            }
        }
    }
}
