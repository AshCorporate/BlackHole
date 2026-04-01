using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates the circular city map procedurally.
/// Enhanced version: cosmic city theme, zone rings, diagonal roads,
/// background star particles, and a centre marker.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    // ── Constants ──────────────────────────────────────────────────────────────
    private const float BorderThickness      = 5f;  // world units added to border mesh radius
    private const float ParticleFillSeconds  = 15f; // seconds to fill the star-particle system

    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;

    // Colours — exposed for tweaking without changing config
    [SerializeField] private Color groundColor   = new Color(0.04f, 0.04f, 0.12f);
    [SerializeField] private Color roadColor     = new Color(0.15f, 0.18f, 0.30f);
    [SerializeField] private Color borderColor   = new Color(0.02f, 0.02f, 0.08f);

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");
    }

    private void Start()
    {
        GenerateMap();
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void GenerateMap()
    {
        float radius = config != null ? config.mapRadius : 50f;

        CreateCircularGround(radius);
        CreateCircularBorder(radius);
        CreateRoadGrid(radius);
        CreateDiagonalRoads(radius);
        CreateBlockBoundary(radius);
        CreateZoneRings(radius);
        CreateCenterMarker();

        if (config == null || config.enableStarParticles)
            CreateBackgroundParticles(radius);
    }

    // ── Ground & Border ────────────────────────────────────────────────────────

    /// <summary>Fills the circular play area with a ground-coloured mesh.</summary>
    private void CreateCircularGround(float radius)
    {
        GameObject go = new GameObject("Ground");
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;

        MeshFilter   mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -10;
        mr.material = CreateColorMaterial(groundColor);
        mf.mesh = BuildCircleMesh(radius, 64);
    }

    /// <summary>Creates a dark ring border just outside the play area.</summary>
    private void CreateCircularBorder(float radius)
    {
        GameObject go = new GameObject("Border");
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;

        MeshFilter   mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -9;
        mr.material = CreateColorMaterial(borderColor);
        mf.mesh = BuildCircleMesh(radius + BorderThickness, 64);

        PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
        var path = new List<Vector2>();
        int segments = 64;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            path.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
        col.SetPath(0, path.ToArray());
        col.isTrigger = false;
    }

    // ── Roads ──────────────────────────────────────────────────────────────────

    /// <summary>Draws a simple road grid inside the circle.</summary>
    private void CreateRoadGrid(float radius)
    {
        float roadWidth = 1.2f;
        int   roadCount = 6;

        for (int i = -roadCount; i <= roadCount; i++)
        {
            float pos = i * (radius / roadCount);
            CreateRoadSegment(new Vector2(-radius, pos),  new Vector2(radius, pos),  roadWidth);
            CreateRoadSegment(new Vector2(pos, -radius),  new Vector2(pos, radius),  roadWidth);
        }
    }

    /// <summary>Adds diagonal roads at 45° and 135° for visual variety.</summary>
    private void CreateDiagonalRoads(float radius)
    {
        float roadWidth = 0.8f;
        // Four diagonals: 45°, 135°, 225°, 315°
        float[] angles = { 45f, 135f };
        foreach (float angleDeg in angles)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            CreateRoadSegment(-dir * radius, dir * radius, roadWidth);
        }
    }

    private void CreateRoadSegment(Vector2 start, Vector2 end, float width)
    {
        GameObject go = new GameObject("Road");
        go.transform.SetParent(transform);

        MeshFilter   mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -8;
        mr.material = CreateColorMaterial(roadColor);

        Vector2 dir  = (end - start).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * width * 0.5f;

        Vector3[] verts = new Vector3[4];
        verts[0] = new Vector3(start.x + perp.x, start.y + perp.y, 0f);
        verts[1] = new Vector3(start.x - perp.x, start.y - perp.y, 0f);
        verts[2] = new Vector3(end.x   - perp.x, end.y   - perp.y, 0f);
        verts[3] = new Vector3(end.x   + perp.x, end.y   + perp.y, 0f);

        Mesh mesh = new Mesh();
        mesh.vertices  = verts;
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }

    // ── Boundary ───────────────────────────────────────────────────────────────

    /// <summary>Adds a visual ring to show the outer boundary with an animated orange-red gradient.</summary>
    private void CreateBlockBoundary(float radius)
    {
        GameObject go = new GameObject("BlockBoundary");
        go.transform.SetParent(transform);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop          = true;
        lr.startWidth    = 0.6f;
        lr.endWidth      = 0.6f;

        // Animated gradient: orange → red
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.4f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.1f, 0.1f), 0.5f),
                new GradientColorKey(new Color(1f, 0.4f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
        lr.colorGradient = grad;
        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder  = 5;

        int segments = 128;
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    // ── Zone Rings ─────────────────────────────────────────────────────────────

    /// <summary>Draws N concentric zone rings grading from safe (green) to danger (red).</summary>
    private void CreateZoneRings(float radius)
    {
        int ringCount = config != null ? config.mapZoneRingCount : 3;
        Color innerColor = config != null ? config.safeZoneColor   : new Color(0.1f, 0.55f, 0.1f, 0.25f);
        Color outerColor = config != null ? config.dangerZoneColor : new Color(0.7f, 0.1f, 0.1f, 0.25f);

        for (int r = 1; r <= ringCount; r++)
        {
            float t       = (float)r / (ringCount + 1);
            float ringRad = radius * t;
            Color col     = Color.Lerp(innerColor, outerColor, t);

            GameObject go = new GameObject($"ZoneRing_{r}");
            go.transform.SetParent(transform);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop          = true;
            lr.startWidth    = 0.25f;
            lr.endWidth      = 0.25f;
            lr.startColor    = col;
            lr.endColor      = col;
            lr.material      = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder  = -7;

            int segs = 96;
            lr.positionCount = segs;
            for (int i = 0; i < segs; i++)
            {
                float angle = i * Mathf.PI * 2f / segs;
                lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * ringRad, Mathf.Sin(angle) * ringRad, 0f));
            }
        }
    }

    // ── Centre Marker ──────────────────────────────────────────────────────────

    /// <summary>Places a small glowing circle at the map centre (0,0).</summary>
    private void CreateCenterMarker()
    {
        GameObject go = new GameObject("CenterMarker");
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop          = true;
        lr.startWidth    = 0.15f;
        lr.endWidth      = 0.15f;
        lr.startColor    = new Color(0.4f, 0.2f, 1f, 0.8f);
        lr.endColor      = new Color(0.2f, 0.6f, 1f, 0.8f);
        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder  = 3;

        float markerRadius = 1.5f;
        int   segs = 32;
        lr.positionCount = segs;
        for (int i = 0; i < segs; i++)
        {
            float angle = i * Mathf.PI * 2f / segs;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * markerRadius, Mathf.Sin(angle) * markerRadius, 0f));
        }
    }

    // ── Background Particles ───────────────────────────────────────────────────

    /// <summary>Creates a looping star/particle system on the background layer.</summary>
    private void CreateBackgroundParticles(float radius)
    {
        int count = config != null ? config.starParticleCount : 200;

        GameObject go = new GameObject("StarParticles");
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();

        // Renderer
        psr.sortingOrder = -20;
        psr.material     = new Material(Shader.Find("Sprites/Default"));

        // Main module
        var main = ps.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(8f, 18f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.05f, 0.3f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.04f, 0.14f);
        main.maxParticles    = count;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Start colour: white to light blue
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.8f, 0.8f, 1f, 0.9f),
            new Color(0.5f, 0.7f, 1f, 0.6f));

        // Emission
        var emission = ps.emission;
        emission.rateOverTime = count / ParticleFillSeconds;

        // Shape: disc matching the map
        var shape = ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.Circle;
        shape.radius       = radius * 0.95f;
        shape.radiusThickness = 1f; // fill entire disc

        ps.Play();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private Mesh BuildCircleMesh(float radius, int segments)
    {
        Vector3[] verts = new Vector3[segments + 1];
        int[]     tris  = new int[segments * 3];

        verts[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            verts[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }
        for (int i = 0; i < segments; i++)
        {
            tris[i * 3 + 0] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = (i + 1) % segments + 1;
        }

        Mesh mesh = new Mesh();
        mesh.vertices  = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    private Material CreateColorMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }
}
