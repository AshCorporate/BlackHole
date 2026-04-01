using UnityEngine;

/// <summary>
/// Bot wanders the map and absorbs small objects.
/// </summary>
public class PatrolState : BotStateBase
{
    private Vector2 _target;
    private float   _targetTimer;
    private const float TARGET_LIFETIME = 4f;

    public PatrolState(BotAI bot, BotStateMachine machine) : base(bot, machine) { }

    public override void Enter()
    {
        PickNewTarget();
    }

    public override void Execute()
    {
        _targetTimer -= Time.deltaTime;
        if (_targetTimer <= 0f || ReachedTarget())
            PickNewTarget();

        Bot.MoveTowards(_target);
    }

    private void PickNewTarget()
    {
        // Prefer a position that has objects nearby
        CityObject nearest = Bot.FindNearestAbsorbableObject();
        if (nearest != null)
            _target = nearest.transform.position;
        else
        {
            // Random point inside the map
            GameConfig cfg = Resources.Load<GameConfig>("GameConfig");
            float r = cfg != null ? cfg.mapRadius * 0.85f : 42f;
            _target = MathHelpers.RandomPointInCircle(r);
        }
        _targetTimer = TARGET_LIFETIME;
    }

    private bool ReachedTarget()
    {
        return Vector2.Distance(Bot.transform.position, _target) < 1.5f;
    }
}
