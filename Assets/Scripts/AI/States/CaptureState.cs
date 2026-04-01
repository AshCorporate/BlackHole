using UnityEngine;

/// <summary>
/// Bot tries to close a territory loop (Paper.io mechanic).
/// </summary>
public class CaptureState : BotStateBase
{
    private Vector2 _captureTarget;
    private float   _stateTimer;
    private const float MAX_CAPTURE_DURATION = 8f;

    public CaptureState(BotAI bot, BotStateMachine machine) : base(bot, machine) { }

    public override void Enter()
    {
        _stateTimer = MAX_CAPTURE_DURATION;
        PickCaptureTarget();
    }

    public override void Execute()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0f)
        {
            // Give up after timeout
            StateMachine.ChangeState(new PatrolState(Bot, StateMachine));
            return;
        }

        if (Vector2.Distance(Bot.transform.position, _captureTarget) < 1.5f)
        {
            // Pick the next point in the loop
            PickCaptureTarget();
        }

        Bot.MoveTowards(_captureTarget);
    }

    private void PickCaptureTarget()
    {
        // Choose a point away from current position to start drawing territory
        GameConfig cfg = Resources.Load<GameConfig>("GameConfig");
        float r = cfg != null ? cfg.mapRadius * 0.6f : 30f;

        // Aim to travel in a roughly circular path
        Vector2 pos = Bot.transform.position;
        float angle = Mathf.Atan2(pos.y, pos.x) + Mathf.PI * 0.5f;
        _captureTarget = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) *
                         Random.Range(r * 0.4f, r);
    }
}
