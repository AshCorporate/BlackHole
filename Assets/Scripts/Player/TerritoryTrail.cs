using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Draws the Paper.io-style territory trail behind the black hole and handles
/// territory capture when the trail reconnects to the player's home zone.
/// </summary>
public class TerritoryTrail : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;
    [SerializeField] private Color playerColor = Color.cyan;

    // ── Private ────────────────────────────────────────────────────────────────
    private LineRenderer _lineRenderer;
    private List<Vector2> _trailPoints = new List<Vector2>();
    private TerritorySystem _territorySystem;
    private bool _insideOwnTerritory = true;

    private const float MIN_POINT_DISTANCE = 0.3f; // minimum distance before adding a new point

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");

        _territorySystem = FindObjectOfType<TerritorySystem>();
        SetupLineRenderer();
    }

    private void Update()
    {
        Vector2 pos = transform.position;

        bool inTerritory = _territorySystem != null &&
                           _territorySystem.IsInsidePlayerTerritory(pos, this);

        if (inTerritory)
        {
            // Re-entered own territory — attempt capture
            if (!_insideOwnTerritory && _trailPoints.Count >= 3)
            {
                _trailPoints.Add(pos);
                CaptureTerritory();
            }
            _insideOwnTerritory = true;
            ClearTrail();
        }
        else
        {
            _insideOwnTerritory = false;

            // Extend trail
            if (_trailPoints.Count == 0 ||
                Vector2.Distance(pos, _trailPoints[_trailPoints.Count - 1]) > MIN_POINT_DISTANCE)
            {
                _trailPoints.Add(pos);
                UpdateLineRenderer();
            }
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public Color PlayerColor => playerColor;

    public void SetColor(Color c)
    {
        playerColor = c;
        if (_lineRenderer != null)
        {
            _lineRenderer.startColor = c;
            _lineRenderer.endColor = c;
        }
    }

    /// <summary>Returns true if the given world position lies on this player's active trail.</summary>
    public bool IsOnTrail(Vector2 pos, float threshold = 0.4f)
    {
        for (int i = 0; i < _trailPoints.Count - 1; i++)
        {
            float dist = DistancePointToSegment(pos, _trailPoints[i], _trailPoints[i + 1]);
            if (dist < threshold) return true;
        }
        return false;
    }

    /// <summary>Clears the current trail line.</summary>
    public void ClearTrail()
    {
        _trailPoints.Clear();
        if (_lineRenderer != null)
            _lineRenderer.positionCount = 0;
    }

    /// <summary>Called on respawn — resets trail state.</summary>
    public void ResetTrail()
    {
        ClearTrail();
        _insideOwnTerritory = true;
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void SetupLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.startWidth = config != null ? config.trailWidth : 0.2f;
        _lineRenderer.endWidth = config != null ? config.trailWidth : 0.2f;
        _lineRenderer.startColor = playerColor;
        _lineRenderer.endColor = playerColor;
        _lineRenderer.sortingOrder = 1;

        // Use a default material that renders in 2D
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void UpdateLineRenderer()
    {
        _lineRenderer.positionCount = _trailPoints.Count;
        for (int i = 0; i < _trailPoints.Count; i++)
            _lineRenderer.SetPosition(i, new Vector3(_trailPoints[i].x, _trailPoints[i].y, 0f));
    }

    private void CaptureTerritory()
    {
        if (_territorySystem != null)
            _territorySystem.CaptureTerritory(new List<Vector2>(_trailPoints), this);
        ClearTrail();
    }

    /// <summary>Distance from point p to segment (a,b).</summary>
    private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab.sqrMagnitude);
        Vector2 closest = a + t * ab;
        return Vector2.Distance(p, closest);
    }
}
