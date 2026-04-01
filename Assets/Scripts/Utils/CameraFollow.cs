using UnityEngine;

/// <summary>
/// Smooth camera follow script.
/// Attach to the Main Camera in the Game scene.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float     smoothSpeed = 5f;
    [SerializeField] private float     cameraZ     = -10f;

    private void LateUpdate()
    {
        if (target == null)
        {
            // Auto-find player if not assigned
            BlackHoleController player = FindObjectOfType<BlackHoleController>();
            if (player != null) target = player.transform;
            return;
        }

        Vector3 desiredPos = new Vector3(target.position.x, target.position.y, cameraZ);
        transform.position = Vector3.Lerp(transform.position, desiredPos,
                                          1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
    }
}
