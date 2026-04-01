using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AI controller for a bot black hole.
/// Uses a state machine to decide between Patrol, Hunt, Capture, Flee, and BuffSeek.
/// </summary>
[RequireComponent(typeof(BlackHolePhysics))]
[RequireComponent(typeof(MassSystem))]
[RequireComponent(typeof(TerritoryTrail))]
[RequireComponent(typeof(BlackHoleController))]
public class BotAI : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig    config;
    [SerializeField] public  BotDifficulty difficulty;

    // ── Components ─────────────────────────────────────────────────────────────
    private BlackHolePhysics   _physics;
    private MassSystem         _massSystem;
    private BlackHoleController _controller;
    private BotStateBase        _currentState;

    // ── State ──────────────────────────────────────────────────────────────────
    private float _stateUpdateTimer;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        _physics    = GetComponent<BlackHolePhysics>();
        _massSystem = GetComponent<MassSystem>();
        _controller = GetComponent<BlackHoleController>();

        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");
    }

    private BotStateMachine _stateMachine;

    private void Start()
    {
        // Create and initialise the state machine, then start in Patrol
        _stateMachine = new BotStateMachine();
        _stateMachine.Init(this);
        ChangeState(new PatrolState(this, _stateMachine));
    }

    private void Update()
    {
        if (!_controller.IsAlive) return;

        _stateUpdateTimer -= Time.deltaTime;
        if (_stateUpdateTimer <= 0f)
        {
            _stateUpdateTimer = config != null ? config.botStateUpdateInterval : 0.5f;
            EvaluateStateTransition();
        }

        _currentState?.Execute();
    }

    // ── Public API (used by States) ────────────────────────────────────────────

    /// <summary>Sets the bot's movement direction toward a target position.</summary>
    public void MoveTowards(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        float speedMult = difficulty != null ? difficulty.speedMultiplier : 1f;
        _physics.InputDirection = dir * speedMult;
    }

    /// <summary>Stops the bot's movement.</summary>
    public void StopMovement() => _physics.InputDirection = Vector2.zero;

    /// <summary>Finds the nearest city object this bot can absorb.</summary>
    public CityObject FindNearestAbsorbableObject()
    {
        float detectionRadius = config != null ? config.botDetectionRadius : 15f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        CityObject nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            CityObject obj = hit.GetComponent<CityObject>();
            if (obj == null || obj.IsAbsorbed) continue;
            if (!_massSystem.CanAbsorb(obj.ObjectMass)) continue;

            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = obj;
            }
        }
        return nearest;
    }

    /// <summary>Finds a smaller black hole to hunt.</summary>
    public BlackHoleController FindHuntTarget()
    {
        float detectionRadius = config != null ? config.botDetectionRadius : 15f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        BlackHoleController bestTarget = null;
        float bestScore = float.MinValue;

        float threshold = difficulty != null ? difficulty.huntMassThreshold :
                          (config != null ? config.huntMassRatio : 0.8f);

        foreach (var hit in hits)
        {
            BlackHoleController target = hit.GetComponent<BlackHoleController>();
            if (target == null || target == _controller || !target.IsAlive) continue;

            MassSystem targetMass = target.GetComponent<MassSystem>();
            if (targetMass == null) continue;
            if (targetMass.Mass > _massSystem.Mass * threshold) continue;

            // Score: prefer closer, lighter targets
            float dist = Vector2.Distance(transform.position, target.transform.position);
            float score = -dist - targetMass.Mass;
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }
        return bestTarget;
    }

    /// <summary>Finds a threat (larger black hole nearby).</summary>
    public BlackHoleController FindThreat()
    {
        float detectionRadius = config != null ? config.botDetectionRadius : 15f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        float fleeThreshold = difficulty != null ? difficulty.fleeMassThreshold :
                              (config != null ? config.fleeMassRatio : 1.2f);

        foreach (var hit in hits)
        {
            BlackHoleController other = hit.GetComponent<BlackHoleController>();
            if (other == null || other == _controller || !other.IsAlive) continue;

            MassSystem otherMass = other.GetComponent<MassSystem>();
            if (otherMass == null) continue;
            if (otherMass.Mass >= _massSystem.Mass * fleeThreshold)
                return other;
        }
        return null;
    }

    /// <summary>Finds the nearest active buff pick-up.</summary>
    public BuffBase FindNearestBuff()
    {
        float detectionRadius = config != null ? config.botDetectionRadius : 15f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        BuffBase nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            BuffBase buff = hit.GetComponent<BuffBase>();
            if (buff == null || !buff.gameObject.activeSelf) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = buff;
            }
        }
        return nearest;
    }

    /// <summary>Changes the active state (called by states themselves).</summary>
    public void ChangeState(BotStateBase newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    // ── Private ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Evaluates whether the bot should switch to a different state
    /// based on its environment and difficulty profile.
    /// </summary>
    private void EvaluateStateTransition()
    {
        float aggression  = difficulty != null ? difficulty.aggressionFactor  : 0.5f;
        float territorial = difficulty != null ? difficulty.territorialFactor  : 0.5f;
        float coward      = difficulty != null ? difficulty.cowardFactor       : 0.3f;

        // Priority 1: Flee from threats
        BlackHoleController threat = FindThreat();
        if (threat != null && !(_currentState is FleeState))
        {
            if (Random.value < coward + 0.3f)
            {
                ChangeState(new FleeState(this, _stateMachine));
                return;
            }
        }

        // Priority 2: Collect nearby buff
        BuffBase buff = FindNearestBuff();
        if (buff != null && !(_currentState is BuffSeekState))
        {
            float dist = Vector2.Distance(transform.position, buff.transform.position);
            if (dist < 8f)
            {
                ChangeState(new BuffSeekState(this, _stateMachine));
                return;
            }
        }

        // Priority 3: Hunt smaller opponent
        BlackHoleController huntTarget = FindHuntTarget();
        if (huntTarget != null && !(_currentState is HuntState))
        {
            if (Random.value < aggression)
            {
                ChangeState(new HuntState(this, _stateMachine));
                return;
            }
        }

        // Priority 4: Capture territory
        if (!(_currentState is CaptureState) && !(_currentState is PatrolState))
        {
            if (Random.value < territorial * 0.3f)
            {
                ChangeState(new CaptureState(this, _stateMachine));
                return;
            }
        }

        // Default: Patrol
        if (!(_currentState is PatrolState) && !(_currentState is HuntState) &&
            !(_currentState is CaptureState))
        {
            ChangeState(new PatrolState(this, _stateMachine));
        }
    }
}
