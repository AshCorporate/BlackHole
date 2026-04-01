using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages territory ownership for all players.
/// Tracks captured zones and detects trail collisions between players.
/// </summary>
public class TerritorySystem : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static TerritorySystem Instance { get; private set; }

    // ── Private ────────────────────────────────────────────────────────────────

    /// <summary>Maps each TerritoryTrail owner to its captured polygon vertices.</summary>
    private readonly Dictionary<TerritoryTrail, List<Vector2>> _territories =
        new Dictionary<TerritoryTrail, List<Vector2>>();

    // Mesh-based territory rendering (one mesh per player)
    private readonly Dictionary<TerritoryTrail, MeshFilter> _territoryMeshes =
        new Dictionary<TerritoryTrail, MeshFilter>();

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Registers a player with a small starting territory (circle).</summary>
    public void RegisterPlayer(TerritoryTrail trail, Vector2 startPosition, float startRadius = 3f)
    {
        var polygon = new List<Vector2>();
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            polygon.Add(startPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * startRadius);
        }
        _territories[trail] = polygon;
        CreateTerritoryMesh(trail, polygon);
    }

    /// <summary>Returns true if the given world position is inside the player's territory.</summary>
    public bool IsInsidePlayerTerritory(Vector2 pos, TerritoryTrail trail)
    {
        if (!_territories.TryGetValue(trail, out var poly)) return false;
        return MathHelpers.IsPointInPolygon(pos, poly);
    }

    /// <summary>
    /// Called by TerritoryTrail when a loop is closed.
    /// Merges the enclosed area into the player's territory polygon.
    /// </summary>
    public void CaptureTerritory(List<Vector2> trailPoints, TerritoryTrail trail)
    {
        if (!_territories.ContainsKey(trail))
            _territories[trail] = new List<Vector2>();

        // Merge trail loop with existing territory (simplified: replace with convex hull for now)
        var combined = new List<Vector2>(_territories[trail]);
        combined.AddRange(trailPoints);

        _territories[trail] = ComputeConvexHull(combined);
        UpdateTerritoryMesh(trail, _territories[trail]);

        Debug.Log($"[TerritorySystem] {trail.gameObject.name} captured territory. " +
                  $"Area: {Mathf.Abs(MathHelpers.PolygonSignedArea(_territories[trail])):F1}");
    }

    /// <summary>
    /// Checks whether world position pos crosses any enemy trail.
    /// Returns the owning TerritoryTrail if it does, null otherwise.
    /// </summary>
    public TerritoryTrail CheckTrailCollision(Vector2 pos, TerritoryTrail ownTrail)
    {
        foreach (var kvp in _territories)
        {
            TerritoryTrail t = kvp.Key;
            if (t == ownTrail) continue;
            if (t != null && t.IsOnTrail(pos))
                return t;
        }
        return null;
    }

    /// <summary>Returns a copy of the territory polygon points for the given trail (for minimap rendering).</summary>
    public List<Vector2> GetTerritoryPoints(TerritoryTrail trail)
    {
        if (!_territories.TryGetValue(trail, out var poly)) return null;
        return new List<Vector2>(poly);
    }

    /// <summary>Returns the total captured area for a given trail owner.</summary>
    public float GetCapturedArea(TerritoryTrail trail)
    {
        if (!_territories.TryGetValue(trail, out var poly)) return 0f;
        return Mathf.Abs(MathHelpers.PolygonSignedArea(poly));
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void CreateTerritoryMesh(TerritoryTrail trail, List<Vector2> polygon)
    {
        GameObject go = new GameObject($"Territory_{trail.gameObject.name}");
        go.transform.SetParent(transform);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sortingOrder = 0;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        Color c = trail.PlayerColor;
        c.a = 0.25f;
        mat.color = c;
        mr.material = mat;

        _territoryMeshes[trail] = mf;
        UpdateTerritoryMesh(trail, polygon);
    }

    private void UpdateTerritoryMesh(TerritoryTrail trail, List<Vector2> polygon)
    {
        if (!_territoryMeshes.TryGetValue(trail, out MeshFilter mf)) return;

        Mesh mesh = TriangulateFan(polygon);
        mf.mesh = mesh;
    }

    /// <summary>Simple triangle-fan triangulation for convex polygons.</summary>
    private Mesh TriangulateFan(List<Vector2> polygon)
    {
        Mesh mesh = new Mesh();
        if (polygon == null || polygon.Count < 3) return mesh;

        Vector3[] verts = new Vector3[polygon.Count];
        for (int i = 0; i < polygon.Count; i++)
            verts[i] = new Vector3(polygon[i].x, polygon[i].y, 0f);

        int triCount = polygon.Count - 2;
        int[] tris = new int[triCount * 3];
        for (int i = 0; i < triCount; i++)
        {
            tris[i * 3 + 0] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>Andrew's monotone chain convex hull algorithm.</summary>
    private List<Vector2> ComputeConvexHull(List<Vector2> points)
    {
        if (points.Count < 3) return new List<Vector2>(points);

        points.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

        var hull = new List<Vector2>();

        // Lower hull
        foreach (var p in points)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        // Upper hull
        int lowerCount = hull.Count + 1;
        for (int i = points.Count - 2; i >= 0; i--)
        {
            while (hull.Count >= lowerCount && Cross(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(points[i]);
        }

        hull.RemoveAt(hull.Count - 1);
        return hull;
    }

    private float Cross(Vector2 O, Vector2 A, Vector2 B)
    {
        return (A.x - O.x) * (B.y - O.y) - (A.y - O.y) * (B.x - O.x);
    }
}
