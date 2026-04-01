using UnityEngine;

/// <summary>
/// Abstract base class for all power-up buffs.
/// Derives should implement the Apply logic.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public abstract class BuffBase : MonoBehaviour
{
    [SerializeField] protected Color buffColor = Color.white;
    [SerializeField] protected string buffName = "Buff";

    protected virtual void Awake()
    {
        // Make collider a trigger so black holes walk through
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        // Create a simple visual (circle sprite)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.color = buffColor;
        sr.sortingOrder = 3;

        // Pulse animation via scale
        StartPulse();
    }

    /// <summary>
    /// Called when a black hole collides with this buff.
    /// Subclasses apply their specific effect.
    /// </summary>
    public abstract void Apply(BlackHoleController target);

    /// <summary>Deactivates the buff after pickup.</summary>
    protected void Pickup()
    {
        BuffSpawner.Instance?.NotifyBuffPickedUp(this);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BlackHoleController bh = other.GetComponent<BlackHoleController>();
        if (bh != null && bh.IsAlive)
        {
            Apply(bh);
            Pickup();
        }
    }

    private void StartPulse()
    {
        // Simple scale pulsing via coroutine
        StartCoroutine(PulseRoutine());
    }

    private System.Collections.IEnumerator PulseRoutine()
    {
        float t = 0f;
        Vector3 baseScale = transform.localScale;
        while (true)
        {
            t += Time.deltaTime * 2f;
            float s = 1f + 0.15f * Mathf.Sin(t);
            transform.localScale = baseScale * s;
            yield return null;
        }
    }
}
