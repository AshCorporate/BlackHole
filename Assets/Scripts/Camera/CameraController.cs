using UnityEngine;

/// <summary>
/// Paper.io-style camera:
///   • Smooth follow with configurable speed.
///   • Camera zoom (orthographicSize) scales with player mass:
///       targetSize = cameraBaseSize + (mass * cameraMassZoomFactor)
///       clamped between cameraMinZoom and cameraMaxZoom.
///   • Slight look-ahead in movement direction (like Paper.io).
///   • Camera clamped to square map boundary (mapSize × mapSize).
///   • Auto-finds the player by tag "Player" if no target was set.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private Transform  playerTransform;
    [SerializeField] private MassSystem playerMass; // kept for API compatibility

    private Camera  _cam;
    private Vector3 _previousPlayerPos;
    private BlackHoleController _playerController;

    // Smoothing
    private Vector3 _currentVelocity;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");

        // Initial zoom
        if (_cam != null && _cam.orthographic)
            _cam.orthographicSize = config != null ? config.cameraBaseSize : 15f;
    }

    private void Start()
    {
        // Auto-find the player by tag if no target was set from GameManager
        if (playerTransform == null)
        {
            GameObject playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                playerTransform   = playerGo.transform;
                playerMass        = playerGo.GetComponent<MassSystem>();
                _playerController = playerGo.GetComponent<BlackHoleController>();
                _previousPlayerPos = playerGo.transform.position;
            }
        }
    }

    /// <summary>Sets the player target. Call from GameManager after player spawn.</summary>
    public void SetTarget(Transform t, MassSystem ms)
    {
        playerTransform = t;
        playerMass      = ms;
        _playerController = t != null ? t.GetComponent<BlackHoleController>() : null;
        if (t != null)
            _previousPlayerPos = t.position;
    }

    /// <summary>Sets the player target from a BlackHoleController. Call from GameManager after player spawn.</summary>
    public void SetTarget(BlackHoleController controller)
    {
        if (controller == null) return;
        playerTransform   = controller.transform;
        playerMass        = controller.GetComponent<MassSystem>();
        _playerController = controller;
        _previousPlayerPos = controller.transform.position;
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // If player is dead, camera stays in place
        if (_playerController != null && !_playerController.IsAlive) return;

        float followSpeed = config != null ? config.cameraFollowSpeed : 8f;
        float lookAhead   = config != null ? config.cameraLookAhead   : 1.5f;
        float mapSize     = config != null ? config.mapSize           : 100f;
        float mapHalf     = mapSize * 0.5f;

        // --- Mass-based zoom ---
        float baseSize    = config != null ? config.cameraBaseSize         : 15f;
        float massFactor  = config != null ? config.cameraMassZoomFactor   : 0.15f;
        float minZoom     = config != null ? config.cameraMinZoom          : 10f;
        float maxZoom     = config != null ? config.cameraMaxZoom          : 40f;
        float zoomSpeed   = config != null ? config.cameraZoomSpeed        : 3f;

        float currentMass = playerMass != null ? playerMass.Mass : 0f;
        float targetZoom  = Mathf.Clamp(baseSize + currentMass * massFactor, minZoom, maxZoom);

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

        // --- Clamp to square map boundary ---
        float zoom   = _cam != null ? _cam.orthographicSize : targetZoom;
        float aspect = _cam != null ? _cam.aspect : 1.78f;
        float halfH  = zoom;
        float halfW  = zoom * aspect;

        desiredPos.x = Mathf.Clamp(desiredPos.x, -mapHalf + halfW, mapHalf - halfW);
        desiredPos.y = Mathf.Clamp(desiredPos.y, -mapHalf + halfH, mapHalf - halfH);
        desiredPos.z = -10f;

        // --- Smooth follow (SmoothDamp for Paper.io feel) ---
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref _currentVelocity,
            1f / followSpeed);

        // --- Smooth zoom based on mass ---
        if (_cam != null && _cam.orthographic)
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
    }
}
