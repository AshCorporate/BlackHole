using UnityEngine;

/// <summary>
/// Lightweight state machine wrapper held by each BotAI instance.
/// Allows bot states to request transitions without a direct BotAI reference.
/// </summary>
public class BotStateMachine
{
    private BotAI _bot;

    public void Init(BotAI bot) => _bot = bot;

    /// <summary>Delegates a state change request to the owning BotAI.</summary>
    public void ChangeState(BotStateBase newState)
    {
        _bot?.ChangeState(newState);
    }
}

/// <summary>
/// Base class for all bot AI states.
/// Each state implements Enter, Execute (called every tick), and Exit.
/// </summary>
public abstract class BotStateBase
{
    protected BotAI Bot { get; }
    protected BotStateMachine StateMachine { get; }

    protected BotStateBase(BotAI bot, BotStateMachine machine)
    {
        Bot          = bot;
        StateMachine = machine;
    }

    /// <summary>Called once when entering the state.</summary>
    public virtual void Enter() { }

    /// <summary>Called every state-machine tick.</summary>
    public abstract void Execute();

    /// <summary>Called once when leaving the state.</summary>
    public virtual void Exit() { }
}
