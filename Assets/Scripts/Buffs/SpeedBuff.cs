using UnityEngine;

/// <summary>
/// Speed Boost buff — temporarily doubles the black hole's movement speed.
/// </summary>
public class SpeedBuff : BuffBase
{
    protected override void Awake()
    {
        buffName  = "Speed Boost";
        buffColor = new Color(0f, 1f, 1f); // cyan
        base.Awake();
    }

    public override void Apply(BlackHoleController target)
    {
        BlackHolePhysics physics = target.GetComponent<BlackHolePhysics>();
        physics?.ActivateSpeedBoost();
    }
}
