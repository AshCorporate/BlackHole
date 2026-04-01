using UnityEngine;

/// <summary>
/// Paper.io-style camera:
///   • Fixed orthographic zoom — player is always clearly visible, no mass-based zoom.
///   • Smooth follow with configurable speed.
///   • Slight look-ahead in movement direction (like Paper.io).
///   • Camera clamped to map circle boundary.
///   • Player always rendered at high sorting layer; bots/enemies on normal layer.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private Transform  playerTransform;
    [SerializeField] private MassSystem playerMass; // kept for API compatibility

    private Camera  _cam;
    private Vector3 _previousPlayerPos;

    // Smoothing
    private Vector3 _currentVelocity;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");

        // Paper.io fixed zoom — set once
        if (_cam != null && _cam.orthographic)
            _cam.orthographicSize = config != null ? config.cameraMinZoom : 12f;
    }

    /// <summary>Sets the player target. Call from GameManager after player spawn.</summary>
    public void SetTarget(Transform t, MassSystem ms)
    {
        playerTransform = t;
        playerMass      = ms;
        if (t != null)
            _previousPlayerPos = t.position;
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        float followSpeed = config != null ? config.cameraFollowSpeed : 8f;
        float lookAhead   = config != null ? config.cameraLookAhead   : 1.5f;
        float mapRadius   = config != null ? config.mapRadius         : 50f;
        float zoom        = config != null ? config.cameraMinZoom     : 12f;

        // --- Look-ahead based on frame-to-frame player movement ---
        Vector3 playerPos   = playerTransform.position;
        Vector3 playerDelta = playerPos - _previousPlayerPos;
        _previousPlayerPos  = playerPos;

        Vector3 lookOffset = playerDelta.magnitude > 0.001f
            ? playerDelta.normalized * lookAhead
            : Vector3.zero;

        Vector3 desiredPos = new Vector3(
            playerPos.x + lookOffset.x,
            playerPos.y + lookOffset.y,
            -10f);

        // --- Clamp to map boundary ---
        float aspect      = _cam != null ? _cam.aspect : 1.78f;
        float halfH       = zoom;
        float halfW       = zoom * aspect;
        float clampRadius = Mathf.Max(0f, mapRadius - Mathf.Max(halfH, halfW));
        Vector2 desiredXY = new Vector2(desiredPos.x, desiredPos.y);
        if (desiredXY.magnitude > clampRadius && clampRadius > 0f)
            desiredXY = desiredXY.normalized * clampRadius;
        desiredPos = new Vector3(desiredXY.x, desiredXY.y, -10f);

        // --- Smooth follow (SmoothDamp for Paper.io feel) ---
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref _currentVelocity,
            1f / followSpeed);

        // --- Fixed zoom (no mass-based change) ---
        if (_cam != null && _cam.orthographic)
            _cam.orthographicSize = zoom;
    }
}
