using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns power-up buffs randomly on the map throughout the game.
/// Maintains a maximum number of active buffs at any time.
/// </summary>
public class BuffSpawner : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static BuffSpawner Instance { get; private set; }

    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;

    [SerializeField] private int initialBuffCount = 6;

    // Prefab references — assign in the Inspector or create at runtime
    [SerializeField] private SpeedBuff        speedBuffPrefab;
    [SerializeField] private MagnetBuff       magnetBuffPrefab;
    [SerializeField] private DoubleMassBuff   doubleMassBuffPrefab;
    [SerializeField] private ShieldBuff       shieldBuffPrefab;
    [SerializeField] private GravityPulseBuff gravityPulseBuffPrefab;

    // ── Private ────────────────────────────────────────────────────────────────
    private readonly List<BuffBase> _activeBuffs = new List<BuffBase>();
    private readonly Queue<BuffBase> _pickedUpBuffs = new Queue<BuffBase>();

    private BuffBase[] _buffPrefabs;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");
    }

    private void Start()
    {
        // Gather prefab references (they may be set in inspector or auto-created)
        _buffPrefabs = new BuffBase[]
        {
            speedBuffPrefab,
            magnetBuffPrefab,
            doubleMassBuffPrefab,
            shieldBuffPrefab,
            gravityPulseBuffPrefab
        };

        // Create default prefabs if none assigned
        EnsurePrefabs();

        // Spawn initial set of buffs immediately
        int spawnCount = Mathf.Clamp(initialBuffCount, 0, config != null ? config.maxActiveBuffs : 5);
        for (int i = 0; i < spawnCount; i++)
            SpawnRandomBuff();

        StartCoroutine(SpawnRoutine());
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public void NotifyBuffPickedUp(BuffBase buff)
    {
        _activeBuffs.Remove(buff);
        _pickedUpBuffs.Enqueue(buff);
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float interval = config != null ? config.buffSpawnInterval : 20f;
            yield return new WaitForSeconds(interval);

            int maxBuffs = config != null ? config.maxActiveBuffs : 5;
            if (_activeBuffs.Count < maxBuffs)
            {
                SpawnRandomBuff();
            }
        }
    }

    private void SpawnRandomBuff()
    {
        // Reuse a picked-up buff if available; otherwise pick a random type
        BuffBase buff;
        if (_pickedUpBuffs.Count > 0)
        {
            buff = _pickedUpBuffs.Dequeue();
        }
        else
        {
            int idx = Random.Range(0, _buffPrefabs.Length);
            BuffBase prefab = _buffPrefabs[idx];
            if (prefab == null) return;
            buff = Instantiate(prefab, transform);
        }

        float mapRadius = config != null ? config.mapRadius : 50f;
        Vector2 pos = MathHelpers.RandomPointInCircle(mapRadius * 0.8f);
        buff.transform.position = new Vector3(pos.x, pos.y, 0f);
        buff.gameObject.SetActive(true);
        _activeBuffs.Add(buff);
    }

    /// <summary>
    /// Creates simple runtime prefabs for each buff type if none are assigned.
    /// Replace with actual prefabs in the Unity Inspector for production.
    /// </summary>
    private void EnsurePrefabs()
    {
        if (speedBuffPrefab       == null) speedBuffPrefab       = CreateRuntimePrefab<SpeedBuff>();
        if (magnetBuffPrefab      == null) magnetBuffPrefab      = CreateRuntimePrefab<MagnetBuff>();
        if (doubleMassBuffPrefab  == null) doubleMassBuffPrefab  = CreateRuntimePrefab<DoubleMassBuff>();
        if (shieldBuffPrefab      == null) shieldBuffPrefab      = CreateRuntimePrefab<ShieldBuff>();
        if (gravityPulseBuffPrefab == null) gravityPulseBuffPrefab = CreateRuntimePrefab<GravityPulseBuff>();

        _buffPrefabs = new BuffBase[]
        {
            speedBuffPrefab, magnetBuffPrefab, doubleMassBuffPrefab,
            shieldBuffPrefab, gravityPulseBuffPrefab
        };
    }

    private T CreateRuntimePrefab<T>() where T : BuffBase
    {
        GameObject go = new GameObject(typeof(T).Name + "_Prefab");
        go.SetActive(false);
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<CircleCollider2D>();
        T buff = go.AddComponent<T>();
        go.transform.SetParent(transform);
        return buff;
    }
}
