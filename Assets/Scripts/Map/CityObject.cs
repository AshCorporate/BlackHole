using UnityEngine;

/// <summary>
/// Represents a city object on the map (bench, car, building, etc.).
/// Each object has a mass and can be absorbed by a black hole whose mass is large enough.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class CityObject : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private float objectMass = 1f;
    [SerializeField] private string objectName = "Object";
    [SerializeField] private Color objectColor = Color.gray;

    // ── Public State ───────────────────────────────────────────────────────────
    public float ObjectMass => objectMass;
    public bool IsAbsorbed { get; private set; }

    // ── Private ────────────────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private GameConfig _config;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _config = Resources.Load<GameConfig>("GameConfig");

        _rb.gravityScale = 0f;
        _rb.linearDamping = 5f;

        // Scale the object proportional to its mass (trash≈0.2, cars≈0.5, buildings 1-2, skyscrapers 2-3)
        float scale = Mathf.Clamp(objectMass * 0.025f, 0.2f, 3f);
        transform.localScale = Vector3.one * scale;

        ApplyColor();
    }

    private void FixedUpdate()
    {
        if (IsAbsorbed) return;

        // Check for nearby black holes that can absorb this object
        float checkRadius = _config != null ? _config.absorptionRangeMultiplier : 3f;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, checkRadius);
        foreach (var col in nearby)
        {
            BlackHolePhysics hole = col.GetComponent<BlackHolePhysics>();
            MassSystem mass = col.GetComponent<MassSystem>();
            if (hole != null && mass != null && mass.CanAbsorb(objectMass))
            {
                hole.PullObject(_rb);
            }
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Sets the mass and name of this city object (used by ObjectSpawner).</summary>
    public void Initialize(string objName, float mass, Color color)
    {
        objectName = objName;
        objectMass = mass;
        objectColor = color;

        float scale = Mathf.Clamp(mass * 0.025f, 0.2f, 3f);
        transform.localScale = Vector3.one * scale;

        ApplyColor();
    }

    /// <summary>Called when this object is absorbed by a black hole.</summary>
    public void OnAbsorbed()
    {
        if (IsAbsorbed) return;
        IsAbsorbed = true;
        // Notify ObjectSpawner so it can respawn later
        ObjectSpawner.Instance?.NotifyObjectAbsorbed(this);
        gameObject.SetActive(false);
    }

    /// <summary>Resets the object state so it can be reused (called via SendMessage by ObjectSpawner).</summary>
    private void ResetObject()
    {
        IsAbsorbed = false;
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void ApplyColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = objectColor;
    }
}
