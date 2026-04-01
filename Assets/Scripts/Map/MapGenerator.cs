using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates a procedural Cyberpunk Detroit city map.
/// All visuals are drawn in code — no external textures required.
///
/// Layers (sortingOrder from bottom to top):
///   -25  Void background (space beyond the city)
///   -20  Dark ground fill
///   -15  Building blocks with optional neon rooftop lines
///   -10  Road grid (horizontal/vertical grid streets)
///    -8  Neon road edge lines along radial spoke roads
///    -6  Hazard zone rings (dashed)
///    -5  Neon street light dots at intersections
///    10  Map boundary (glowing cyan ring + PolygonCollider2D)
/// </summary>
public class MapGenerator : MonoBehaviour
{
    // ── Cyberpunk Color Palette ────────────────────────────────────────────────
    private static readonly Color ColorGround       = new Color(0.05f, 0.06f, 0.10f);
    private static readonly Color ColorVoid         = Color.black;
    private static readonly Color ColorRoad         = new Color(0.12f, 0.12f, 0.18f);
    private static readonly Color ColorNeonCyan     = new Color(0f,   0.9f,  1f);
    private static readonly Color ColorNeonMagenta  = new Color(1f,   0f,    0.8f);
    private static readonly Color ColorNeonOrange   = new Color(1f,   0.4f,  0f);
    private static readonly Color ColorHazardRing   = new Color(1f,   0.4f,  0f,   0.4f);

    private static readonly Color[] ColorBuildings  = new Color[]
    {
        new Color(0.06f, 0.05f, 0.15f),   // very dark blue
        new Color(0.10f, 0.05f, 0.18f),   // dark purple
        new Color(0.04f, 0.10f, 0.15f),   // dark teal
        new Color(0.08f, 0.08f, 0.12f),   // very dark grey
    };

    private static readonly Color[] ColorNeonAccents = new Color[]
    {
        new Color(0f,  0.9f, 1f),    // cyan
        new Color(1f,  0f,   0.8f),  // magenta
        new Color(1f,  0.4f, 0f),    // orange
    };

    // ── Sizes & Counts ─────────────────────────────────────────────────────────
    private const float BorderWidth          = 0.6f;
    private const float RoadWidthMajor       = 2.5f;
    private const float RoadWidthGrid        = 1.2f;
    private const float NeonEdgeWidth        = 0.08f;
    private const float NeonLightRadius      = 0.15f;
    private const int   NeonLightMaxCount    = 60;
    private const int   RadialRoadCount      = 4;
    private const int   HazardRingDashCount  = 16;
    private const float BuildingMinSize      = 1.5f;
    private const float BuildingMaxSize      = 5f;
    private const float BuildingNeonChance   = 0.3f;
    private const int   CircleSegments       = 64;

    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;

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

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Generates the full cyberpunk city map. Called automatically in Start.</summary>
    private void GenerateMap()
    {
        float radius = config != null ? config.mapRadius : 50f;

        CreateVoidBackground(radius);
        CreateCircularGround(radius);
        CreateBuildingBlocks(radius);
        CreateRoadGrid(radius);
        CreateRadialRoads(radius);
        CreateHazardRings(radius);
        CreateNeonStreetLights(radius);
        CreateCircularBorder(radius);
    }

    // ── Ground & Border ────────────────────────────────────────────────────────

    /// <summary>Fills the circular play area with a dark cyberpunk ground mesh.</summary>
    private void CreateCircularGround(float radius)
    {
        GameObject go = new GameObject("Ground");
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;

        MeshFilter   mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -20;
        mr.material     = CreateColorMaterial(ColorGround);
        mf.mesh         = BuildCircleMesh(radius, CircleSegments);
    }

    /// <summary>
    /// Creates a glowing neon cyan border ring and the PolygonCollider2D wall
    /// that keeps players inside the circular boundary.
    /// </summary>
    private void CreateCircularBorder(float radius)
    {
        GameObject go = new GameObject("Border");
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;

        // Glowing neon ring
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace  = true;
        lr.loop           = true;
        lr.startWidth     = BorderWidth;
        lr.endWidth       = BorderWidth;
        lr.startColor     = ColorNeonCyan;
        lr.endColor       = ColorNeonCyan;
        lr.material       = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder   = 10;

        int segs = CircleSegments * 2;
        lr.positionCount = segs;
        for (int i = 0; i < segs; i++)
        {
            float angle = i * Mathf.PI * 2f / segs;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }

        // Collision wall
        PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
        var path = new List<Vector2>();
        for (int i = 0; i < CircleSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / CircleSegments;
            path.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
        col.SetPath(0, path.ToArray());
        col.isTrigger = false;
    }

    // ── Void Background ────────────────────────────────────────────────────────

    /// <summary>Draws the outer void/space around the map as a large black circle.</summary>
    private void CreateVoidBackground(float radius)
    {
        GameObject go = new GameObject("VoidBackground");
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;

        MeshFilter   mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -25;
        mr.material     = CreateColorMaterial(ColorVoid);
        mf.mesh         = BuildCircleMesh(radius + 20f, CircleSegments);
    }

    // ── Building Blocks ────────────────────────────────────────────────────────

    /// <summary>
    /// Procedurally generates ~80-120 rectangular building footprints on a loose grid,
    /// placed between road lanes. Some buildings get neon-lit rooftop edge lines.
    /// </summary>
    private void CreateBuildingBlocks(float radius)
    {
        GameObject container = new GameObject("Buildings");
        container.transform.SetParent(transform);

        // Fixed seed for reproducible layout
        Random.State savedState = Random.state;
        Random.InitState(12345);

        float gridSpacing = radius / 6f;   // matches the road grid spacing
        int   gridHalf    = Mathf.CeilToInt(radius / gridSpacing);

        // Buildings sit in the centers of road "blocks" (offset by half a cell)
        for (int gx = -gridHalf; gx < gridHalf; gx++)
        {
            for (int gy = -gridHalf; gy < gridHalf; gy++)
            {
                float cx = (gx + 0.5f) * gridSpacing;
                float cy = (gy + 0.5f) * gridSpacing;

                // Random offset within block (±30% of half-spacing)
                float maxOff = gridSpacing * 0.3f;
                float bx = cx + Random.Range(-maxOff, maxOff);
                float by = cy + Random.Range(-maxOff, maxOff);

                // Skip if outside circular boundary
                if (bx * bx + by * by > (radius * 0.9f) * (radius * 0.9f)) continue;

                float w       = Random.Range(BuildingMinSize, BuildingMaxSize);
                float h       = Random.Range(BuildingMinSize, BuildingMaxSize);
                Color col     = ColorBuildings[Random.Range(0, ColorBuildings.Length)];
                bool  neonRoof = Random.value < BuildingNeonChance;
                Color neonCol  = ColorNeonAccents[Random.Range(0, ColorNeonAccents.Length)];

                CreateBuilding(container.transform, bx, by, w, h, col, neonRoof, neonCol);
            }
        }

        Random.state = savedState;
    }

    private void CreateBuilding(Transform parent, float cx, float cy,
                                float w, float h, Color color,
                                bool neonRoof, Color neonColor)
    {
        GameObject go = new GameObject("Building");
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(cx, cy, 0f);

        MeshFilter   mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -15;
        mr.material     = CreateColorMaterial(color);

        float hw = w * 0.5f;
        float hh = h * 0.5f;
        Mesh mesh = new Mesh();
        mesh.vertices  = new Vector3[]
        {
            new Vector3(-hw, -hh, 0f),
            new Vector3(-hw,  hh, 0f),
            new Vector3( hw,  hh, 0f),
            new Vector3( hw, -hh, 0f),
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        // Neon rooftop outline
        if (neonRoof)
        {
            GameObject roofGo = new GameObject("NeonRoof");
            roofGo.transform.SetParent(go.transform);
            roofGo.transform.localPosition = Vector3.zero;

            LineRenderer lr = roofGo.AddComponent<LineRenderer>();
            lr.useWorldSpace  = false;
            lr.loop           = true;
            lr.startWidth     = 0.06f;
            lr.endWidth       = 0.06f;
            lr.startColor     = neonColor;
            lr.endColor       = neonColor;
            lr.material       = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder   = -14;
            lr.positionCount  = 4;
            lr.SetPosition(0, new Vector3(-hw, -hh, 0f));
            lr.SetPosition(1, new Vector3(-hw,  hh, 0f));
            lr.SetPosition(2, new Vector3( hw,  hh, 0f));
            lr.SetPosition(3, new Vector3( hw, -hh, 0f));
        }
    }

    // ── Roads ──────────────────────────────────────────────────────────────────

    /// <summary>Draws the cyberpunk city grid road network at spacing = radius / 6.</summary>
    private void CreateRoadGrid(float radius)
    {
        float spacing  = radius / 6f;
        int   roadCount = Mathf.RoundToInt(radius / spacing);

        for (int i = -roadCount; i <= roadCount; i++)
        {
            float pos = i * spacing;
            CreateRoadSegment(new Vector2(-radius, pos), new Vector2(radius, pos), RoadWidthGrid);
            CreateRoadSegment(new Vector2(pos, -radius), new Vector2(pos, radius), RoadWidthGrid);
        }
    }

    /// <summary>Draws 4 wide radial/spoke roads emanating from the center.</summary>
    private void CreateRadialRoads(float radius)
    {
        float angleStep = 360f / RadialRoadCount;
        for (int i = 0; i < RadialRoadCount; i++)
        {
            float angleRad = (i * angleStep + 22.5f) * Mathf.Deg2Rad;
            Vector2 dir    = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            Vector2 start  = -dir * radius;
            Vector2 end    =  dir * radius;

            CreateRoadSegment(start, end, RoadWidthMajor);
            CreateNeonRoadEdge(start, end);
        }
    }

    private void CreateRoadSegment(Vector2 start, Vector2 end, float width)
    {
        GameObject go = new GameObject("Road");
        go.transform.SetParent(transform);

        MeshFilter   mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -10;
        mr.material     = CreateColorMaterial(ColorRoad);

        Vector2 dir  = (end - start).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * width * 0.5f;

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(start.x + perp.x, start.y + perp.y, 0f),
            new Vector3(start.x - perp.x, start.y - perp.y, 0f),
            new Vector3(end.x   - perp.x, end.y   - perp.y, 0f),
            new Vector3(end.x   + perp.x, end.y   + perp.y, 0f),
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }

    private void CreateNeonRoadEdge(Vector2 start, Vector2 end)
    {
        // Two thin neon lines flanking the radial road — one cyan, one magenta
        Vector2 dir  = (end - start).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);
        float   edgeOffset = RoadWidthMajor * 0.5f + 0.1f;

        for (int side = 0; side < 2; side++)
        {
            float  mult  = side == 0 ? 1f : -1f;
            Color  color = side == 0 ? ColorNeonCyan : ColorNeonMagenta;
            Vector2 off  = perp * edgeOffset * mult;

            GameObject go = new GameObject("NeonEdge");
            go.transform.SetParent(transform);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.startWidth    = NeonEdgeWidth;
            lr.endWidth      = NeonEdgeWidth;
            lr.startColor    = color;
            lr.endColor      = color;
            lr.material      = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder  = -8;
            lr.SetPosition(0, new Vector3(start.x + off.x, start.y + off.y, 0f));
            lr.SetPosition(1, new Vector3(end.x   + off.x, end.y   + off.y, 0f));
        }
    }

    // ── Hazard Rings ───────────────────────────────────────────────────────────

    /// <summary>
    /// Draws dashed concentric hazard rings at 30%, 60%, and 90% of map radius
    /// using dim orange neon LineRenderers.
    /// </summary>
    private void CreateHazardRings(float radius)
    {
        float[] fractions = { 0.3f, 0.6f, 0.9f };

        for (int r = 0; r < fractions.Length; r++)
        {
            float ringRadius = radius * fractions[r];

            GameObject container = new GameObject($"HazardRing_{r}");
            container.transform.SetParent(transform);

            // Create each dash as a short LineRenderer segment
            for (int d = 0; d < HazardRingDashCount; d++)
            {
                float startAngle = d * Mathf.PI * 2f / HazardRingDashCount;
                float endAngle   = startAngle + 0.65f * Mathf.PI * 2f / HazardRingDashCount;

                GameObject dashGo = new GameObject("Dash");
                dashGo.transform.SetParent(container.transform);

                LineRenderer lr = dashGo.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.loop          = false;
                lr.startWidth    = 0.12f;
                lr.endWidth      = 0.12f;
                lr.startColor    = ColorHazardRing;
                lr.endColor      = ColorHazardRing;
                lr.material      = new Material(Shader.Find("Sprites/Default"));
                lr.sortingOrder  = -6;
                lr.positionCount = 4;

                for (int i = 0; i < 4; i++)
                {
                    float angle = Mathf.Lerp(startAngle, endAngle, i / 3f);
                    lr.SetPosition(i, new Vector3(
                        Mathf.Cos(angle) * ringRadius,
                        Mathf.Sin(angle) * ringRadius,
                        0f));
                }
            }
        }
    }

    // ── Neon Street Lights ─────────────────────────────────────────────────────

    /// <summary>Places small neon glowing dot markers at road grid intersections.</summary>
    private void CreateNeonStreetLights(float radius)
    {
        GameObject container = new GameObject("NeonLights");
        container.transform.SetParent(transform);

        Random.State savedState = Random.state;
        Random.InitState(54321);

        float spacing  = radius / 6f;
        int   gridHalf = Mathf.RoundToInt(radius / spacing);
        int   placed   = 0;

        for (int x = -gridHalf; x <= gridHalf && placed < NeonLightMaxCount; x++)
        {
            for (int y = -gridHalf; y <= gridHalf && placed < NeonLightMaxCount; y++)
            {
                float px = x * spacing;
                float py = y * spacing;

                if (px * px + py * py > (radius * 0.95f) * (radius * 0.95f)) continue;

                Color col = ColorNeonAccents[Random.Range(0, ColorNeonAccents.Length)];
                CreateNeonDot(container.transform, px, py, col);
                placed++;
            }
        }

        Random.state = savedState;
    }

    private void CreateNeonDot(Transform parent, float x, float y, Color color)
    {
        GameObject go = new GameObject("NeonDot");
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(x, y, 0f);

        MeshFilter   mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -5;
        mr.material     = CreateColorMaterial(color);
        mf.mesh         = BuildCircleMesh(NeonLightRadius, 12);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>Builds a filled circle mesh centered at origin.</summary>
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

    /// <summary>Creates a default Sprites/Default material with the given solid color.</summary>
    private Material CreateColorMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }
}
