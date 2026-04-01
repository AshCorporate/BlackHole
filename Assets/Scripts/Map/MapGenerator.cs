using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates the circular city map procedurally using placeholder shapes.
/// Creates a round boundary and road-grid background.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;

    // Colours for the procedural map
    [SerializeField] private Color groundColor  = new Color(0.15f, 0.18f, 0.15f);
    [SerializeField] private Color roadColor    = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] private Color borderColor  = new Color(0.05f, 0.05f, 0.05f);

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
        CreateBlockBoundary(radius);
    }

    /// <summary>Fills the circular play area with a ground-coloured mesh.</summary>
    private void CreateCircularGround(float radius)
    {
        GameObject go = new GameObject("Ground");
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;

        MeshFilter mf = go.AddComponent<MeshFilter>();
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

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -9;
        mr.material = CreateColorMaterial(borderColor);
        mf.mesh = BuildCircleMesh(radius + 3f, 64);

        // Add an edge collider to block players from leaving
        PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
        List<Vector2> path = new List<Vector2>();
        int segments = 64;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            path.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
        col.SetPath(0, path.ToArray());
        col.isTrigger = false;
    }

    /// <summary>Draws a simple road grid inside the circle.</summary>
    private void CreateRoadGrid(float radius)
    {
        float roadWidth = 1.5f;
        int roadCount = 8; // number of roads per axis

        for (int i = -roadCount; i <= roadCount; i++)
        {
            float pos = i * (radius / roadCount);

            // Horizontal road
            CreateRoadSegment(new Vector2(-radius, pos), new Vector2(radius, pos), roadWidth, radius);
            // Vertical road
            CreateRoadSegment(new Vector2(pos, -radius), new Vector2(pos, radius), roadWidth, radius);
        }
    }

    private void CreateRoadSegment(Vector2 start, Vector2 end, float width, float mapRadius)
    {
        GameObject go = new GameObject("Road");
        go.transform.SetParent(transform);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = -8;
        mr.material = CreateColorMaterial(roadColor);

        Vector2 dir = (end - start).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * width * 0.5f;

        Vector3[] verts = new Vector3[4];
        verts[0] = new Vector3(start.x + perp.x, start.y + perp.y, 0f);
        verts[1] = new Vector3(start.x - perp.x, start.y - perp.y, 0f);
        verts[2] = new Vector3(end.x - perp.x,   end.y - perp.y,   0f);
        verts[3] = new Vector3(end.x + perp.x,   end.y + perp.y,   0f);

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }

    /// <summary>Adds a visual ring to show the outer boundary of the city block.</summary>
    private void CreateBlockBoundary(float radius)
    {
        GameObject go = new GameObject("BlockBoundary");
        go.transform.SetParent(transform);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = 0.5f;
        lr.endWidth = 0.5f;
        lr.startColor = new Color(1f, 0.5f, 0f);
        lr.endColor   = new Color(1f, 0.5f, 0f);
        lr.material   = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 5;

        int segments = 128;
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private Mesh BuildCircleMesh(float radius, int segments)
    {
        Vector3[] verts = new Vector3[segments + 1];
        int[] tris = new int[segments * 3];

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
