using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns and manages city objects on the circular map.
/// Balances object counts for a 10-minute session with 6-8 players.
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static ObjectSpawner Instance { get; private set; }

    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;
    [SerializeField] private CityObject cityObjectPrefab;

    // ── Private ────────────────────────────────────────────────────────────────
    private readonly List<CityObject> _activeObjects = new List<CityObject>();
    private readonly Queue<CityObject> _absorbedObjects = new Queue<CityObject>();

    // Object type definitions (name, mass, color, count)
    private readonly ObjectDefinition[] _objectDefs = new ObjectDefinition[]
    {
        // Light (mass 1-5)
        new ObjectDefinition("Trash Can",   1f,  new Color(0.5f, 0.5f, 0.5f), 80),
        new ObjectDefinition("Bench",       2f,  new Color(0.6f, 0.4f, 0.2f), 60),
        new ObjectDefinition("Street Lamp", 3f,  new Color(0.9f, 0.9f, 0.3f), 50),
        new ObjectDefinition("Bush",        4f,  new Color(0.1f, 0.7f, 0.1f), 50),

        // Medium (mass 10-30)
        new ObjectDefinition("Car",         15f, new Color(0.8f, 0.2f, 0.2f), 40),
        new ObjectDefinition("Tree",        10f, new Color(0.0f, 0.5f, 0.0f), 35),
        new ObjectDefinition("Small Building", 25f, new Color(0.6f, 0.6f, 0.8f), 20),

        // Heavy (mass 50-100+)
        new ObjectDefinition("Large Building", 60f, new Color(0.4f, 0.4f, 0.7f), 10),
        new ObjectDefinition("Skyscraper",    100f, new Color(0.3f, 0.3f, 0.9f), 5),
    };

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");
    }

    private void Start()
    {
        SpawnAllObjects();
        StartCoroutine(RespawnRoutine());
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Called by CityObject when it gets absorbed.</summary>
    public void NotifyObjectAbsorbed(CityObject obj)
    {
        _activeObjects.Remove(obj);
        _absorbedObjects.Enqueue(obj);
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void SpawnAllObjects()
    {
        foreach (var def in _objectDefs)
        {
            for (int i = 0; i < def.Count; i++)
            {
                SpawnObject(def);
            }
        }
        Debug.Log($"[ObjectSpawner] Spawned {_activeObjects.Count} city objects.");
    }

    private void SpawnObject(ObjectDefinition def)
    {
        EnsureCityObjectPrefab();
        if (cityObjectPrefab == null) return;

        float mapRadius = config != null ? config.mapRadius : 50f;
        Vector2 pos = FindValidPosition(mapRadius);

        CityObject obj = Instantiate(cityObjectPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity, transform);
        obj.Initialize(def.Name, def.Mass, def.Color);
        _activeObjects.Add(obj);
    }

    /// <summary>
    /// Creates a minimal runtime CityObject prefab if none is assigned in the Inspector.
    /// </summary>
    private void EnsureCityObjectPrefab()
    {
        if (cityObjectPrefab != null) return;

        GameObject go = new GameObject("CityObject_Prefab");
        go.SetActive(false);
        go.AddComponent<SpriteRenderer>();
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        go.AddComponent<CircleCollider2D>();
        cityObjectPrefab = go.AddComponent<CityObject>();
        go.transform.SetParent(transform);
    }

    private Vector2 FindValidPosition(float mapRadius)
    {
        float minSpacing = config != null ? config.minObjectSpacing : 1.5f;
        const int maxAttempts = 30;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = MathHelpers.RandomPointInCircle(mapRadius * 0.9f);
            bool valid = true;

            foreach (var obj in _activeObjects)
            {
                if (obj == null) continue;
                if (Vector2.Distance(candidate, obj.transform.position) < minSpacing)
                {
                    valid = false;
                    break;
                }
            }

            if (valid) return candidate;
        }

        // Fallback: return any random position
        return MathHelpers.RandomPointInCircle(mapRadius * 0.9f);
    }

    /// <summary>Gradually respawns absorbed objects to keep the map populated.</summary>
    private IEnumerator RespawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (_absorbedObjects.Count > 0)
            {
                CityObject obj = _absorbedObjects.Dequeue();
                if (obj != null)
                {
                    float mapRadius = config != null ? config.mapRadius : 50f;
                    Vector2 pos = FindValidPosition(mapRadius);
                    obj.transform.position = new Vector3(pos.x, pos.y, 0f);
                    // Reset absorbed state via SendMessage (avoid tight coupling)
                    obj.SendMessage("ResetObject", SendMessageOptions.DontRequireReceiver);
                    obj.gameObject.SetActive(true);
                    _activeObjects.Add(obj);
                }
            }
        }
    }

    // ── Inner Type ─────────────────────────────────────────────────────────────

    private struct ObjectDefinition
    {
        public string Name;
        public float  Mass;
        public Color  Color;
        public int    Count;

        public ObjectDefinition(string name, float mass, Color color, int count)
        {
            Name  = name;
            Mass  = mass;
            Color = color;
            Count = count;
        }
    }
}
