using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Main controller for the player-controlled black hole.
/// Bridges input (joystick), physics, mass, and territory systems.
/// </summary>
[RequireComponent(typeof(BlackHolePhysics))]
[RequireComponent(typeof(MassSystem))]
[RequireComponent(typeof(TerritoryTrail))]
public class BlackHoleController : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;
    [SerializeField] private Joystick joystick;   // assigned via GameManager at runtime

    // ── Cached Components ──────────────────────────────────────────────────────
    private BlackHolePhysics _physics;
    private MassSystem _massSystem;
    private TerritoryTrail _trail;

    // ── State ──────────────────────────────────────────────────────────────────
    public bool IsAlive { get; private set; } = true;
    public bool IsShielded { get; private set; }
    private float _shieldTimer;

    /// <summary>True while the player is outside their own territory (tail active).</summary>
    public bool IsCapturing => _trail != null && _trail.IsTailActive;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        _physics = GetComponent<BlackHolePhysics>();
        _massSystem = GetComponent<MassSystem>();
        _trail = GetComponent<TerritoryTrail>();

        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");
    }

    private void Update()
    {
        if (!IsAlive) return;

        // Feed joystick input into physics
        if (joystick != null)
            _physics.InputDirection = joystick.Direction;

        // Update shield timer
        if (IsShielded)
        {
            _shieldTimer -= Time.deltaTime;
            if (_shieldTimer <= 0f)
                IsShielded = false;
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Assigns the joystick reference at runtime (called by GameManager).</summary>
    public void SetJoystick(Joystick j) => joystick = j;

    /// <summary>Called when this black hole absorbs a city object.</summary>
    public void AbsorbObject(CityObject obj)
    {
        if (!IsAlive || obj == null) return;
        if (!_massSystem.CanAbsorb(obj.ObjectMass)) return;

        _massSystem.AddMass(obj.ObjectMass);
        obj.OnAbsorbed();
    }

    /// <summary>Called when this black hole absorbs another black hole.</summary>
    public void AbsorbBlackHole(BlackHoleController other)
    {
        if (!IsAlive || other == null || !other.IsAlive) return;
        if (!_massSystem.CanAbsorb(other._massSystem.Mass)) return;

        _massSystem.AddMass(other._massSystem.Mass * 0.5f); // gain half the opponent's mass
        other.Die();
    }

    /// <summary>Kills the black hole (trail cut or absorbed).</summary>
    public void Die()
    {
        if (!IsAlive) return;
        IsAlive = false;
        _trail.ClearTrail();

        Debug.Log($"[BlackHoleController] {gameObject.name} died, respawning in {(config != null ? config.respawnDelay : 3f)}s");

        // Disable rendering and collision but keep the GameObject active so coroutines work
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;

        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders) c.enabled = false;

        // Schedule respawn
        float delay = config != null ? config.respawnDelay : 3f;
        StartCoroutine(RespawnCoroutine(delay));
    }

    /// <summary>Activates the shield buff.</summary>
    public void ActivateShield()
    {
        IsShielded = true;
        _shieldTimer = config != null ? config.shieldDuration : 5f;
    }

    /// <summary>
    /// Applies a movement-blocking stun for the given duration (seconds).
    /// Delegates to StunSystem; duration overrides the StunSystem's default.
    /// </summary>
    public void ApplyStun(float duration)
    {
        StunSystem stunSystem = GetComponent<StunSystem>();
        if (stunSystem == null) return;
        stunSystem.StunDuration = duration;
        stunSystem.Stun();
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private IEnumerator RespawnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Respawn();
    }

    private void Respawn()
    {
        float fraction = config != null ? config.respawnMassFraction : 0.3f;
        _massSystem.SetMass(_massSystem.Mass * fraction);

        // Place at a random position inside the map (70% of half the map side)
        float mapHalf = (config != null ? config.mapSize : 100f) * 0.5f;
        transform.position = MathHelpers.RandomPointInCircle(mapHalf * 0.7f);

        IsAlive = true;

        // Re-enable rendering and collision
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = true;

        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders) c.enabled = true;

        _trail.ResetTrail();

        Debug.Log($"[BlackHoleController] {gameObject.name} respawned.");
    }

    // ── Trigger Detection ──────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive) return;

        // Check for city objects
        CityObject cityObj = other.GetComponent<CityObject>();
        if (cityObj != null)
        {
            AbsorbObject(cityObj);
            return;
        }

        // Check for enemy black holes
        BlackHoleController enemy = other.GetComponent<BlackHoleController>();
        if (enemy != null && enemy != this)
        {
            // Larger one absorbs the smaller
            if (_massSystem.Mass > enemy._massSystem.Mass * 1.1f)
                AbsorbBlackHole(enemy);
            return;
        }

        // Check for buffs
        BuffBase buff = other.GetComponent<BuffBase>();
        if (buff != null)
        {
            buff.Apply(this);
            return;
        }
    }
}
