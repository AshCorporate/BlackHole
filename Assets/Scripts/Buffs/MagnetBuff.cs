using UnityEngine;

/// <summary>
/// Magnet buff — pulls nearby city objects toward the black hole for a short time.
/// </summary>
public class MagnetBuff : BuffBase
{
    protected override void Awake()
    {
        buffName  = "Magnet";
        buffColor = new Color(1f, 0.4f, 0f); // orange
        base.Awake();
    }

    public override void Apply(BlackHoleController target)
    {
        BlackHolePhysics physics = target.GetComponent<BlackHolePhysics>();
        physics?.ActivateMagnet();
    }
}
