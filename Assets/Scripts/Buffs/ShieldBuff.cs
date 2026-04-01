using UnityEngine;

/// <summary>
/// Shield buff — protects the player's trail from being cut by enemies for a short time.
/// </summary>
public class ShieldBuff : BuffBase
{
    protected override void Awake()
    {
        buffName  = "Shield";
        buffColor = new Color(0.4f, 0.8f, 1f); // light blue
        base.Awake();
    }

    public override void Apply(BlackHoleController target)
    {
        target.ActivateShield();
    }
}
