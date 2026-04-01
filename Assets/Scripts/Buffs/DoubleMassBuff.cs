using UnityEngine;

/// <summary>
/// Double Mass buff — doubles the mass gained from absorbing objects for a short time.
/// </summary>
public class DoubleMassBuff : BuffBase
{
    protected override void Awake()
    {
        buffName  = "x2 Mass";
        buffColor = new Color(1f, 0.84f, 0f); // gold
        base.Awake();
    }

    public override void Apply(BlackHoleController target)
    {
        MassSystem mass = target.GetComponent<MassSystem>();
        mass?.ActivateDoubleMass();
    }
}
