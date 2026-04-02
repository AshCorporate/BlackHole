using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates a Paper.io-style square tiled map procedurally using colored quads.
/// No external textures are required — all visuals are created at runtime.
///
/// Map layout (100 × 100 units, from -50 to +50 on both axes):
///
/// sortingOrder layers (bottom to top):
///   -10  Grass ground fill
///    -9  Road strips (horizontal + vertical, every 10 units, 2 units wide)
///    -8  Sidewalk strips (0.5 unit border on each road edge) + intersection squares
///    -7  Road centre dashes (yellow, 0.25 wide × 1 long)
///    -6  Decorative building footprints (~80 small dark quads on grass tiles)
///     0  Territory meshes (managed by TerritorySystem)
///    10  Player circles (managed by PlayerVisual)
///
/// Map border:
///   A 3-unit-wide solid dark frame around the perimeter, plus a PolygonCollider2D
///   that keeps players inside the play area.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    // ── Paper.io Color Palette ─────────────────────────────────────────────────
    private static readonly Color ColorGrass        = new Color(0.35f, 0.65f, 0.25f);
    private static readonly Color ColorRoad         = new Color(0.35f, 0.35f, 0.38f);
    private static readonly Color ColorSidewalk     = new Color(0.75f, 0.75f, 0.72f);
    private static readonly Color ColorIntersection = new Color(0.55f, 0.55f, 0.58f);
    private static readonly Color ColorDash         = new Color(0.90f, 0.90f, 0.20f);
    private static readonly Color ColorBuilding     = new Color(0.25f, 0.28f, 0.32f);
    private static readonly Color ColorBorder       = new Color(0.10f, 0.10f, 0.12f);

    // ── Layout Constants ───────────────────────────────────────────────────────
    private const float RoadSpacing      = 10f;   // road grid interval
    private const float RoadWidth        = 2f;    // road strip width
    private const float SidewalkWidth    = 0.5f;  // sidewalk border on each road edge
    private const float DashWidth        = 0.25f; // road centre dash width
    private const float DashLength       = 1.0f;  // road centre dash length
    private const float DashGap          = 1.0f;  // gap between dashes
    private const float BorderWidth      = 3f;    // outer border wall thickness
    private const float BuildingMinSize  = 2f;
    private const float BuildingMaxSize  = 4f;
    private const int   BuildingCount    = 80;
    /// <summary>Max placement attempts per building = BuildingCount × this multiplier.</summary>
    private const int   BuildingMaxAttemptsMultiplier = 20;

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

    // ── Map Generation ─────────────────────────────────────────────────────────

    private void GenerateMap()
    {
        float mapSize = config != null ? config.mapSize : 100f;
        float half    = mapSize * 0.5f;

        CreateGrassBackground(mapSize);
        CreateRoadNetwork(mapSize, half);
        CreateBuildingDecorations(mapSize, half);
        CreateMapBorder(mapSize, half);
        CreateBoundaryCollider(half);
    }

    // ── Ground ─────────────────────────────────────────────────────────────────

    /// <summary>Fills the entire play area with a grass-coloured quad.</summary>
    private void CreateGrassBackground(float mapSize)
    {
        CreateQuad("Grass", transform, 0f, 0f, mapSize, mapSize, ColorGrass, -10);
    }

    // ── Roads ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws horizontal and vertical road strips at every RoadSpacing interval,
    /// with sidewalk borders and yellow centre dashes, plus intersection squares.
    /// </summary>
    private void CreateRoadNetwork(float mapSize, float half)
    {
        GameObject roadsParent = new GameObject("Roads");
        roadsParent.transform.SetParent(transform);

        // Collect all road centre positions
        var roadPositions = new List<float>();
        for (float p = -half; p <= half + 0.001f; p += RoadSpacing)
            roadPositions.Add(p);

        // ── Horizontal roads ──────────────────────────────────────────────────
        foreach (float y in roadPositions)
        {
            // Main road strip
            CreateQuad("HRoad", roadsParent.transform, 0f, y, mapSize, RoadWidth, ColorRoad, -9);
            // Sidewalks above and below
            float swOff = RoadWidth * 0.5f + SidewalkWidth * 0.5f;
            CreateQuad("HSW_Top",    roadsParent.transform, 0f, y + swOff, mapSize, SidewalkWidth, ColorSidewalk, -8);
            CreateQuad("HSW_Bottom", roadsParent.transform, 0f, y - swOff, mapSize, SidewalkWidth, ColorSidewalk, -8);
            // Centre dashes
            CreateHorizontalDashes(roadsParent.transform, y, half);
        }

        // ── Vertical roads ────────────────────────────────────────────────────
        foreach (float x in roadPositions)
        {
            CreateQuad("VRoad", roadsParent.transform, x, 0f, RoadWidth, mapSize, ColorRoad, -9);
            float swOff = RoadWidth * 0.5f + SidewalkWidth * 0.5f;
            CreateQuad("VSW_Left",  roadsParent.transform, x - swOff, 0f, SidewalkWidth, mapSize, ColorSidewalk, -8);
            CreateQuad("VSW_Right", roadsParent.transform, x + swOff, 0f, SidewalkWidth, mapSize, ColorSidewalk, -8);
            CreateVerticalDashes(roadsParent.transform, x, half);
        }

        // ── Intersection squares ──────────────────────────────────────────────
        float iSize = RoadWidth + SidewalkWidth * 2f;  // covers road + both sidewalks
        foreach (float x in roadPositions)
        {
            foreach (float y in roadPositions)
            {
                CreateQuad("Intersection", roadsParent.transform, x, y, iSize, iSize, ColorIntersection, -8);
            }
        }
    }

    private void CreateHorizontalDashes(Transform parent, float y, float half)
    {
        float step = DashLength + DashGap;
        for (float x = -half + DashLength * 0.5f; x < half; x += step)
            CreateQuad("HDash", parent, x, y, DashLength, DashWidth, ColorDash, -7);
    }

    private void CreateVerticalDashes(Transform parent, float x, float half)
    {
        float step = DashLength + DashGap;
        for (float y = -half + DashLength * 0.5f; y < half; y += step)
            CreateQuad("VDash", parent, x, y, DashWidth, DashLength, ColorDash, -7);
    }

    // ── Decorative Buildings ───────────────────────────────────────────────────

    /// <summary>
    /// Scatters ~BuildingCount small rectangular building footprints on grass tiles,
    /// avoiding road strips. Visual only — no colliders.
    /// </summary>
    private void CreateBuildingDecorations(float mapSize, float half)
    {
        GameObject container = new GameObject("Buildings");
        container.transform.SetParent(transform);

        Random.State savedState = Random.state;
        Random.InitState(42);   // fixed seed for reproducible layout

        int placed = 0;
        int attempts = 0;
        int maxAttempts = BuildingCount * BuildingMaxAttemptsMultiplier;
        float playArea = half - BorderWidth;   // stay inside the border

        while (placed < BuildingCount && attempts < maxAttempts)
        {
            attempts++;
            float bx = Random.Range(-playArea, playArea);
            float by = Random.Range(-playArea, playArea);
            float bw = Random.Range(BuildingMinSize, BuildingMaxSize);
            float bh = Random.Range(BuildingMinSize, BuildingMaxSize);

            // Skip if the centre is within a road strip (± half-road-width of any grid line)
            if (IsNearGridLine(bx) || IsNearGridLine(by)) continue;

            CreateQuad("Building", container.transform, bx, by, bw, bh, ColorBuilding, -6);
            placed++;
        }

        Random.state = savedState;
    }

    // ── Map Border ─────────────────────────────────────────────────────────────

    /// <summary>Creates a solid dark border frame around the entire map.</summary>
    private void CreateMapBorder(float mapSize, float half)
    {
        // Four rectangular strips forming the frame
        float outerHalf = half + BorderWidth;
        float frameSize = mapSize + BorderWidth * 2f;

        // Top strip
        CreateQuad("Border_Top",    transform, 0f,  half + BorderWidth * 0.5f, frameSize, BorderWidth, ColorBorder, -6);
        // Bottom strip
        CreateQuad("Border_Bottom", transform, 0f, -half - BorderWidth * 0.5f, frameSize, BorderWidth, ColorBorder, -6);
        // Left strip
        CreateQuad("Border_Left",   transform, -half - BorderWidth * 0.5f, 0f, BorderWidth, mapSize, ColorBorder, -6);
        // Right strip
        CreateQuad("Border_Right",  transform,  half + BorderWidth * 0.5f, 0f, BorderWidth, mapSize, ColorBorder, -6);
    }

    /// <summary>Adds a PolygonCollider2D at the play-area boundary to prevent players from leaving.</summary>
    private void CreateBoundaryCollider(float half)
    {
        GameObject go = new GameObject("BoundaryCollider");
        go.transform.SetParent(transform);

        PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
        col.isTrigger = false;

        // Path = square perimeter (clockwise winding)
        col.SetPath(0, new Vector2[]
        {
            new Vector2(-half, -half),
            new Vector2(-half,  half),
            new Vector2( half,  half),
            new Vector2( half, -half),
        });
    }

    // ── Quad Helper ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if coord is within half-road-width + sidewalk of the nearest road grid line.
    /// Used to avoid placing building decorations on road strips.
    /// </summary>
    private static bool IsNearGridLine(float coord)
    {
        float threshold = RoadWidth * 0.5f + SidewalkWidth;
        // Normalise to [0, RoadSpacing)
        float mod = ((coord % RoadSpacing) + RoadSpacing) % RoadSpacing;
        return mod < threshold || mod > RoadSpacing - threshold;
    }

    /// <summary>Creates a solid coloured rectangular quad at (cx, cy) with the given dimensions.</summary>
    private static void CreateQuad(string name, Transform parent,
                                   float cx, float cy, float width, float height,
                                   Color color, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(cx, cy, 0f);

        MeshFilter   mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = sortingOrder;
        mr.material     = CreateColorMaterial(color);

        float hw = width  * 0.5f;
        float hh = height * 0.5f;

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-hw, -hh, 0f),
            new Vector3(-hw,  hh, 0f),
            new Vector3( hw,  hh, 0f),
            new Vector3( hw, -hh, 0f),
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }

    /// <summary>Creates a default Sprites/Default material with the given solid color.</summary>
    private static Material CreateColorMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }
}

