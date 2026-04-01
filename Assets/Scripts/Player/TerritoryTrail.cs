using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Full Paper.io-style tail system for the black hole player.
///
/// States:
///   Safe      — inside own territory, no tail, invulnerable to line crossings
///   Capturing — outside own territory, tail active, vulnerable
///
/// Rules (per GDD §6-7):
///   • Tail records positions at MIN_POINT_DISTANCE intervals via LineRenderer.
///   • Tail has an EdgeCollider2D rebuilt each frame so it is a real physics collider.
///   • If an enemy crosses this tail → owner dies.
///   • If owner crosses its own tail → owner dies.
///   • Tail has a 10-second timer; on expiry tail is cleared and owner is stunned 1 s.
///   • Re-entering own territory while capturing → CaptureTerritory + trail cleared.
/// </summary>
[RequireComponent(typeof(BlackHoleController))]
public class TerritoryTrail : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;
    [SerializeField] private Color playerColor = Color.cyan;

    // ── Private ────────────────────────────────────────────────────────────────
    private LineRenderer     _lineRenderer;
    private EdgeCollider2D   _edgeCollider;
    private List<Vector2>    _trailPoints = new List<Vector2>();
    private TerritorySystem  _territorySystem;
    private BlackHoleController _controller;
    private StunSystem       _stunSystem;

    private bool  _insideOwnTerritory = true;
    private float _tailTimer;
    private bool  _tailActive;

    // How far player must move before a new trail point is recorded
    private const float MIN_POINT_DISTANCE = 0.25f;
    // Thickness of the collider hit-area for trail collision detection
    private const float TRAIL_HIT_THRESHOLD = 0.35f;
    // Enemy trail collision check runs every N frames to reduce overhead
    private const int   ENEMY_TRAIL_CHECK_INTERVAL = 3;
    private int _enemyTrailCheckFrame;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");

        _territorySystem = FindFirstObjectByType<TerritorySystem>();
        _controller      = GetComponent<BlackHoleController>();
        _stunSystem      = GetComponent<StunSystem>();

        SetupLineRenderer();
        SetupEdgeCollider();
    }

    private void Update()
    {
        if (!_controller.IsAlive) return;

        Vector2 pos = transform.position;
        bool inTerritory = _territorySystem != null &&
                           _territorySystem.IsInsidePlayerTerritory(pos, this);

        if (inTerritory)
        {
            if (_tailActive && _trailPoints.Count >= 3)
            {
                // Closed the loop — capture territory
                _trailPoints.Add(pos);
                CaptureTerritory();
            }

            _insideOwnTerritory = true;
            EndTail();
        }
        else
        {
            _insideOwnTerritory = false;

            // Start tail on first frame outside territory
            if (!_tailActive)
                StartTail();

            // Check enemy trail collision (throttled to every N frames)
            _enemyTrailCheckFrame++;
            if (_territorySystem != null && _enemyTrailCheckFrame >= ENEMY_TRAIL_CHECK_INTERVAL)
            {
                _enemyTrailCheckFrame = 0;
                TerritoryTrail enemyTrail = _territorySystem.CheckTrailCollision(pos, this);
                if (enemyTrail != null)
                {
                    _controller.Die();
                    return;
                }
            }

            // Extend trail
            if (_trailPoints.Count == 0 ||
                Vector2.Distance(pos, _trailPoints[_trailPoints.Count - 1]) > MIN_POINT_DISTANCE)
            {
                // Check self-intersection (skip last 5 points to avoid false positives near head)
                if (CheckSelfIntersection(pos))
                {
                    _controller.Die();
                    return;
                }

                _trailPoints.Add(pos);
                UpdateLineRenderer();
                UpdateEdgeCollider();
            }

            // Tail timer countdown (GDD §7: 10 seconds)
            if (_tailActive)
            {
                _tailTimer -= Time.deltaTime;
                if (_tailTimer <= 0f)
                    OnTailTimerExpired();
            }
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Player color used for trail and territory rendering.</summary>
    public Color PlayerColor => playerColor;

    /// <summary>True while the tail is active (player is outside own territory).</summary>
    public bool IsTailActive => _tailActive;

    /// <summary>Gets a copy of the current trail points.</summary>
    public List<Vector2> GetTrailPoints() => new List<Vector2>(_trailPoints);

    /// <summary>Sets the player color and updates the line renderer.</summary>
    public void SetColor(Color c)
    {
        playerColor = c;
        if (_lineRenderer != null)
        {
            _lineRenderer.startColor = c;
            _lineRenderer.endColor   = new Color(c.r, c.g, c.b, 0.4f); // fade tail end
        }
    }

    /// <summary>
    /// Returns true if world position pos lies on this player's active tail.
    /// Used by TerritorySystem for cross-player trail collision checks.
    /// </summary>
    public bool IsOnTrail(Vector2 pos, float threshold = 0.35f)
    {
        for (int i = 0; i < _trailPoints.Count - 1; i++)
        {
            float dist = DistancePointToSegment(pos, _trailPoints[i], _trailPoints[i + 1]);
            if (dist < threshold) return true;
        }
        return false;
    }

    /// <summary>Clears the trail visuals and physics collider without triggering capture.</summary>
    public void ClearTrail()
    {
        _trailPoints.Clear();
        if (_lineRenderer != null) _lineRenderer.positionCount = 0;
        if (_edgeCollider != null) _edgeCollider.enabled       = false;
        _tailActive = false;
        _tailTimer  = 0f;
    }

    /// <summary>Called on respawn — full trail reset.</summary>
    public void ResetTrail()
    {
        ClearTrail();
        _insideOwnTerritory = true;
    }

    // ── Private — Tail lifecycle ───────────────────────────────────────────────

    private void StartTail()
    {
        _tailActive = true;
        _tailTimer  = config != null ? config.tailMaxDuration : 10f;
        _trailPoints.Clear();
        if (_edgeCollider != null) _edgeCollider.enabled = true;
    }

    private void EndTail()
    {
        ClearTrail();
        _insideOwnTerritory = true;
    }

    private void OnTailTimerExpired()
    {
        // GDD §7: tail disappears, player gets 1-second stun
        ClearTrail();
        _stunSystem?.Stun();
    }

    // ── Private — Collision checks ─────────────────────────────────────────────

    /// <summary>Checks whether the new head position crosses the player's own tail.</summary>
    private bool CheckSelfIntersection(Vector2 newPoint)
    {
        // Skip the most-recent N points to avoid detecting proximity to the last segment
        int skipCount = Mathf.Min(6, _trailPoints.Count);
        int checkCount = _trailPoints.Count - skipCount - 1;
        for (int i = 0; i < checkCount; i++)
        {
            float dist = DistancePointToSegment(newPoint, _trailPoints[i], _trailPoints[i + 1]);
            if (dist < TRAIL_HIT_THRESHOLD) return true;
        }
        return false;
    }

    // ── Private — Territory capture ────────────────────────────────────────────

    private void CaptureTerritory()
    {
        _territorySystem?.CaptureTerritory(new List<Vector2>(_trailPoints), this);
        ClearTrail();
    }

    // ── Private — Rendering ────────────────────────────────────────────────────

    private void SetupLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.useWorldSpace  = true;
        _lineRenderer.startWidth     = config != null ? config.trailWidth : 0.2f;
        _lineRenderer.endWidth       = config != null ? config.trailWidth : 0.2f;
        _lineRenderer.startColor     = playerColor;
        _lineRenderer.endColor       = new Color(playerColor.r, playerColor.g, playerColor.b, 0.4f);
        _lineRenderer.sortingOrder   = 1;
        _lineRenderer.material       = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.positionCount  = 0;

        // Smooth line corners
        int corners = config != null ? config.trailCornerVertices : 4;
        _lineRenderer.numCornerVertices = corners;
        _lineRenderer.numCapVertices    = corners;
    }

    private void UpdateLineRenderer()
    {
        _lineRenderer.positionCount = _trailPoints.Count;
        for (int i = 0; i < _trailPoints.Count; i++)
            _lineRenderer.SetPosition(i, new Vector3(_trailPoints[i].x, _trailPoints[i].y, 0f));
    }

    // ── Private — Physics collider ─────────────────────────────────────────────

    private void SetupEdgeCollider()
    {
        _edgeCollider           = gameObject.AddComponent<EdgeCollider2D>();
        _edgeCollider.isTrigger = true;
        _edgeCollider.enabled   = false;
    }

    private void UpdateEdgeCollider()
    {
        if (_trailPoints.Count < 2) return;
        _edgeCollider.SetPoints(_trailPoints);
    }

    // ── Private — Trigger detection (enemy on my tail) ─────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_tailActive) return;

        // An enemy black hole entered our tail collider
        BlackHoleController enemy = other.GetComponent<BlackHoleController>();
        if (enemy != null && enemy != _controller && enemy.IsAlive)
        {
            // Enemy hit our tail → we die (GDD §6)
            _controller.Die();
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        if (ab.sqrMagnitude < 0.0001f) return Vector2.Distance(p, a);
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab.sqrMagnitude);
        Vector2 closest = a + t * ab;
        return Vector2.Distance(p, closest);
    }
}
