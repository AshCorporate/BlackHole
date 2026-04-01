using UnityEngine;

/// <summary>
/// Bot runs away from a larger black hole that threatens it.
/// </summary>
public class FleeState : BotStateBase
{
    private BlackHoleController _threat;
    private float _stateTimer;
    private const float FLEE_DURATION = 5f;

    public FleeState(BotAI bot, BotStateMachine machine) : base(bot, machine) { }

    public override void Enter()
    {
        _stateTimer = FLEE_DURATION;
        _threat = Bot.FindThreat();
    }

    public override void Execute()
    {
        _stateTimer -= Time.deltaTime;

        // Update threat
        _threat = Bot.FindThreat();

        if (_threat == null || _stateTimer <= 0f)
        {
            StateMachine.ChangeState(new PatrolState(Bot, StateMachine));
            return;
        }

        // Move in the opposite direction from the threat
        Vector2 away = ((Vector2)Bot.transform.position - (Vector2)_threat.transform.position).normalized;
        GameConfig cfg = Resources.Load<GameConfig>("GameConfig");
        float r = cfg != null ? cfg.mapRadius * 0.85f : 42f;
        Vector2 fleeTarget = (Vector2)Bot.transform.position + away * 10f;

        // Clamp inside map
        if (fleeTarget.magnitude > r)
            fleeTarget = fleeTarget.normalized * r;

        Bot.MoveTowards(fleeTarget);
    }
}
