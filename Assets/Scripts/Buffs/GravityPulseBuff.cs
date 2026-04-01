using UnityEngine;

/// <summary>
/// Gravity Pulse buff — pushes all nearby enemy black holes away from the player.
/// </summary>
public class GravityPulseBuff : BuffBase
{
    protected override void Awake()
    {
        buffName  = "Gravity Pulse";
        buffColor = new Color(0.8f, 0.2f, 0.8f); // purple
        base.Awake();
    }

    public override void Apply(BlackHoleController target)
    {
        GameConfig cfg = Resources.Load<GameConfig>("GameConfig");
        float radius = cfg != null ? cfg.gravityPulseRadius : 10f;
        float force  = cfg != null ? cfg.gravityPulseForce  : 20f;

        // Push all other black holes away
        Collider2D[] nearby = Physics2D.OverlapCircleAll(target.transform.position, radius);
        foreach (var col in nearby)
        {
            BlackHoleController other = col.GetComponent<BlackHoleController>();
            if (other == null || other == target) continue;

            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 dir = ((Vector2)other.transform.position - (Vector2)target.transform.position).normalized;
            rb.AddForce(dir * force, ForceMode2D.Impulse);
        }
    }
}
