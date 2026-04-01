using UnityEngine;

/// <summary>
/// Smooth follow camera with dynamic zoom based on player mass and look-ahead offset.
/// Attach to the Main Camera. Assign playerTransform at runtime via SetTarget().
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private MassSystem playerMass;

    private Camera _cam;
    private Vector2 _velocity; // for SmoothDamp
    private Vector3 _previousPlayerPosition;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");
    }

    /// <summary>Sets the target player transform and mass system for the camera to follow.</summary>
    public void SetTarget(Transform t, MassSystem ms)
    {
        playerTransform = t;
        playerMass = ms;
        if (t != null)
            _previousPlayerPosition = t.position;
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        float followSpeed  = config != null ? config.cameraFollowSpeed  : 5f;
        float zoomSpeed    = config != null ? config.cameraZoomSpeed    : 3f;
        float minZoom      = config != null ? config.cameraMinZoom      : 8f;
        float maxZoom      = config != null ? config.cameraMaxZoom      : 28f;
        float zoomFactor   = config != null ? config.cameraZoomMassFactor : 0.15f;
        float lookAhead    = config != null ? config.cameraLookAhead    : 2.5f;
        float mapRadius    = config != null ? config.mapRadius          : 50f;

        // --- Look-ahead ---
        // Use player's actual frame-to-frame velocity so the offset is independent
        // of the camera's own position (avoids feedback loop).
        Vector3 playerPos    = playerTransform.position;
        Vector3 playerDelta  = playerPos - _previousPlayerPosition;
        _previousPlayerPosition = playerPos;

        Vector3 lookAheadOffset = playerDelta.magnitude > 0.0001f
            ? playerDelta.normalized * lookAhead
            : Vector3.zero;

        Vector3 desiredPos = new Vector3(
            playerPos.x + lookAheadOffset.x,
            playerPos.y + lookAheadOffset.y,
            transform.position.z);

        // --- Clamp camera so it never shows outside the circle ---
        float currentZoom = _cam != null ? _cam.orthographicSize : minZoom;
        float aspect      = _cam != null ? _cam.aspect : 1.78f;
        float halfH = currentZoom;
        float halfW = currentZoom * aspect;
        float clampRadius = Mathf.Max(0f, mapRadius - Mathf.Max(halfH, halfW));
        // Only clamp/normalize when the position has non-zero magnitude to avoid
        // snapping to the origin when the player is at the map centre.
        Vector2 desiredXY = new Vector2(desiredPos.x, desiredPos.y);
        if (desiredXY.magnitude > clampRadius && desiredXY.magnitude > 0.0001f)
            desiredXY = desiredXY.normalized * clampRadius;
        desiredPos = new Vector3(desiredXY.x, desiredXY.y, -10f);

        // --- Follow ---
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        // --- Zoom ---
        if (_cam != null && _cam.orthographic)
        {
            float mass = playerMass != null ? playerMass.Mass : 5f;
            float targetZoom = Mathf.Clamp(minZoom + mass * zoomFactor, minZoom, maxZoom);
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
        }
    }
}
