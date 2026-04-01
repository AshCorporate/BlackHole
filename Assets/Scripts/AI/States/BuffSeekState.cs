using UnityEngine;

/// <summary>
/// Bot seeks out and collects a nearby power-up buff.
/// </summary>
public class BuffSeekState : BotStateBase
{
    private BuffBase _target;
    private float    _stateTimer;
    private const float MAX_SEEK_DURATION = 6f;

    public BuffSeekState(BotAI bot, BotStateMachine machine) : base(bot, machine) { }

    public override void Enter()
    {
        _stateTimer = MAX_SEEK_DURATION;
        _target = Bot.FindNearestBuff();
    }

    public override void Execute()
    {
        _stateTimer -= Time.deltaTime;

        if (_target == null || !_target.gameObject.activeSelf || _stateTimer <= 0f)
        {
            StateMachine.ChangeState(new PatrolState(Bot, StateMachine));
            return;
        }

        Bot.MoveTowards(_target.transform.position);
    }
}
