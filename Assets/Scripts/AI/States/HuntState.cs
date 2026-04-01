using UnityEngine;

/// <summary>
/// Bot chases a smaller black hole to absorb it.
/// </summary>
public class HuntState : BotStateBase
{
    private BlackHoleController _target;

    public HuntState(BotAI bot, BotStateMachine machine) : base(bot, machine) { }

    public override void Enter()
    {
        _target = Bot.FindHuntTarget();
    }

    public override void Execute()
    {
        // Re-acquire target if it was absorbed or died
        if (_target == null || !_target.IsAlive)
        {
            _target = Bot.FindHuntTarget();
            if (_target == null)
            {
                StateMachine.ChangeState(new PatrolState(Bot, StateMachine));
                return;
            }
        }

        // Give up if target grew too large
        MassSystem targetMass = _target.GetComponent<MassSystem>();
        MassSystem ownMass    = Bot.GetComponent<MassSystem>();
        if (targetMass != null && ownMass != null &&
            targetMass.Mass > ownMass.Mass * 0.9f)
        {
            StateMachine.ChangeState(new PatrolState(Bot, StateMachine));
            return;
        }

        Bot.MoveTowards(_target.transform.position);
    }
}
