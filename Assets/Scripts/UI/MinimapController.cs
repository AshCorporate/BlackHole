using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Paper.io-style minimap rendered via OnGUI, displayed in a corner of the screen.
///
/// Shows:
///   • Enemy territory fills (color-coded)
///   • Enemy tail lines
///   • Enemy position dots
///   • Player position dot (distinct color/size)
///   • Map boundary circle
///
/// Does NOT show: city objects, buffs (to keep minimap clean like Paper.io).
///
/// Setup: Add this component to any persistent GameObject (e.g. GameManager).
/// </summary>
public class MinimapController : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private GameConfig config;
    [SerializeField] private Transform  playerTransform;

    [Header("Minimap Display")]
    [Tooltip("Normalized screen rect for the minimap (bottom-right corner by default)")]
    [SerializeField] private Rect minimapScreenRect = new Rect(0.75f, 0.02f, 0.23f, 0.23f);
    [Tooltip("Size of the minimap camera in world units (should cover full map)")]
    [SerializeField] private float minimapWorldSize = 55f;
    [Tooltip("Background color of the minimap")]
    [SerializeField] private Color minimapBgColor = new Color(0.05f, 0.05f, 0.15f, 0.85f);

    // ── Private ────────────────────────────────────────────────────────────────
    private TerritorySystem _territorySystem;
    private List<TerritoryTrail> _allTrails = new List<TerritoryTrail>();
    private TerritoryTrail _playerTrail;

    // Dot sizes (in normalized minimap coordinates)
    private const float ENEMY_DOT_SIZE  = 0.035f;
    private const float PLAYER_DOT_SIZE = 0.05f;
    // Pixel size for trail/polygon point dots on the minimap
    private const float TRAIL_DOT_SIZE  = 2f;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");

        _territorySystem = FindFirstObjectByType<TerritorySystem>();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Sets the player's trail reference so it is highlighted differently on the minimap.</summary>
    public void SetPlayerTrail(TerritoryTrail trail)
    {
        _playerTrail = trail;
    }

    /// <summary>Registers a trail (player or bot) to be tracked on the minimap.</summary>
    public void RegisterTrail(TerritoryTrail trail)
    {
        if (trail != null && !_allTrails.Contains(trail))
            _allTrails.Add(trail);
    }

    /// <summary>Sets the player transform to mark on the minimap.</summary>
    public void SetPlayerTransform(Transform t)
    {
        playerTransform = t;
    }

    // ── GUI rendering ──────────────────────────────────────────────────────────

    private void OnGUI()
    {
        float scrW = Screen.width;
        float scrH = Screen.height;

        // Convert normalized rect to pixel rect
        Rect pixRect = new Rect(
            minimapScreenRect.x * scrW,
            (1f - minimapScreenRect.y - minimapScreenRect.height) * scrH,
            minimapScreenRect.width  * scrW,
            minimapScreenRect.height * scrH);

        float mapR = config != null ? config.mapRadius : 50f;

        // Background
        GUI.color = minimapBgColor;
        GUI.DrawTexture(pixRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Draw registered trails (enemies + player territories/tails)
        foreach (var trail in _allTrails)
        {
            if (trail == null) continue;
            bool isPlayer = (trail == _playerTrail);

            // Draw territory fill
            if (_territorySystem != null)
            {
                var pts = _territorySystem.GetTerritoryPoints(trail);
                if (pts != null && pts.Count >= 3)
                    DrawPolygonOnMinimap(pts, trail.PlayerColor, 0.25f, pixRect, mapR);
            }

            // Draw tail line (only enemies — optional: show player tail too)
            if (!isPlayer && trail.IsTailActive)
            {
                var tailPts = trail.GetTrailPoints();
                DrawPolylineOnMinimap(tailPts, trail.PlayerColor, pixRect, mapR);
            }

            // Draw position dot
            Vector2 worldPos = trail.transform.position;
            Color dotColor   = isPlayer ? Color.white : trail.PlayerColor;
            float dotSize    = isPlayer ? PLAYER_DOT_SIZE : ENEMY_DOT_SIZE;
            DrawDotOnMinimap(worldPos, dotColor, dotSize, pixRect, mapR);
        }

        // Draw map circle border
        GUI.color = new Color(1f, 0.5f, 0f, 0.7f);
        GUI.Box(pixRect, GUIContent.none);
        GUI.color = Color.white;
    }

    // ── Private — Drawing helpers ──────────────────────────────────────────────

    /// <summary>Converts world position to normalized minimap UV coords.</summary>
    private Vector2 WorldToMinimap(Vector2 worldPos, float mapRadius)
    {
        // Normalize to [-1,1] then to [0,1]
        float u = (worldPos.x / mapRadius) * 0.5f + 0.5f;
        float v = (worldPos.y / mapRadius) * 0.5f + 0.5f;
        return new Vector2(u, 1f - v); // flip Y for screen space
    }

    private void DrawDotOnMinimap(Vector2 worldPos, Color color, float normalizedSize,
                                   Rect mapRect, float mapRadius)
    {
        Vector2 uv   = WorldToMinimap(worldPos, mapRadius);
        float pixX   = mapRect.x + uv.x * mapRect.width;
        float pixY   = mapRect.y + uv.y * mapRect.height;
        float dotPx  = normalizedSize * Mathf.Min(mapRect.width, mapRect.height);

        Rect dotRect = new Rect(pixX - dotPx * 0.5f, pixY - dotPx * 0.5f, dotPx, dotPx);
        GUI.color = color;
        GUI.DrawTexture(dotRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void DrawPolylineOnMinimap(List<Vector2> pts, Color color,
                                        Rect mapRect, float mapRadius)
    {
        if (pts == null || pts.Count < 2) return;
        GUI.color = color;
        for (int i = 0; i < pts.Count; i++)
        {
            Vector2 uv  = WorldToMinimap(pts[i], mapRadius);
            float pixX  = mapRect.x + uv.x * mapRect.width;
            float pixY  = mapRect.y + uv.y * mapRect.height;
            GUI.DrawTexture(new Rect(pixX - TRAIL_DOT_SIZE * 0.5f, pixY - TRAIL_DOT_SIZE * 0.5f, TRAIL_DOT_SIZE, TRAIL_DOT_SIZE),
                            Texture2D.whiteTexture);
        }
        GUI.color = Color.white;
    }

    private void DrawPolygonOnMinimap(List<Vector2> pts, Color color, float alpha,
                                       Rect mapRect, float mapRadius)
    {
        Color c = color;
        c.a = alpha;
        GUI.color = c;
        foreach (var p in pts)
        {
            Vector2 uv  = WorldToMinimap(p, mapRadius);
            float pixX  = mapRect.x + uv.x * mapRect.width;
            float pixY  = mapRect.y + uv.y * mapRect.height;
            GUI.DrawTexture(new Rect(pixX - TRAIL_DOT_SIZE * 0.5f, pixY - TRAIL_DOT_SIZE * 0.5f, TRAIL_DOT_SIZE, TRAIL_DOT_SIZE),
                            Texture2D.whiteTexture);
        }
        GUI.color = Color.white;
    }
}
