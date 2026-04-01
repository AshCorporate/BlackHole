using UnityEngine;

/// <summary>
/// Smooth camera follow script with dynamic zoom based on player mass.
/// Attach to the Main Camera in the Game scene.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float     smoothSpeed           = 5f;
    [SerializeField] private float     cameraZ               = -10f;

    [Header("Dynamic Zoom")]
    [SerializeField] private float baseOrthographicSize = 5f;
    [SerializeField] private float zoomScaleFactor      = 3f;
    [SerializeField] private float zoomSmoothSpeed      = 3f;
    [SerializeField] private float maxOrthographicSize  = 30f;

    private MassSystem _massSystem;
    private Camera     _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            // Auto-find player if not assigned
            BlackHoleController player = FindObjectOfType<BlackHoleController>();
            if (player != null)
            {
                target = player.transform;
                _massSystem = player.GetComponent<MassSystem>();
            }
            return;
        }

        // Auto-find MassSystem if not yet cached
        if (_massSystem == null)
            _massSystem = target.GetComponent<MassSystem>();

        // Smooth position follow
        Vector3 desiredPos = new Vector3(target.position.x, target.position.y, cameraZ);
        transform.position = Vector3.Lerp(transform.position, desiredPos,
                                          1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        // Dynamic zoom based on mass
        if (_camera != null && _massSystem != null)
        {
            float currentMass = Mathf.Max(0f, _massSystem.Mass);
            float targetSize  = baseOrthographicSize + zoomScaleFactor * Mathf.Log(1f + currentMass);
            targetSize = Mathf.Clamp(targetSize, baseOrthographicSize, maxOrthographicSize);
            _camera.orthographicSize = Mathf.Lerp(
                _camera.orthographicSize,
                targetSize,
                1f - Mathf.Exp(-zoomSmoothSpeed * Time.deltaTime));
        }
    }
}
