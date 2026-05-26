using UnityEngine;

/// <summary>
/// Attaches to the Main Camera. Follows the player from a fixed top-down angle.
/// Drag the Player GameObject into the 'target' field in the Inspector.
/// </summary>
public class TopDownCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player transform this camera will follow.")]
    public Transform target;

    [Header("Camera Settings")]
    [Tooltip("Height above the target.")]
    public float height = 15f;

    [Tooltip("How far the camera is tilted forward (0 = straight down, 30 = slight angle).")]
    public float tiltAngle = 55f;

    [Tooltip("How smoothly the camera follows. Lower = snappier, higher = floatier.")]
    public float smoothSpeed = 8f;

    // Offset is calculated from height and tilt so it's always consistent
    private Vector3 _offset;

    private void Start()
    {
        RecalculateOffset();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + _offset;

        // Smooth damp for fluid camera movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Always look at target
        transform.LookAt(target.position);
    }

    private void RecalculateOffset()
    {
        // Convert tilt angle to a backwards + upwards offset
        float radians = tiltAngle * Mathf.Deg2Rad;
        float zBack = -height / Mathf.Tan(radians);
        _offset = new Vector3(0f, height, zBack);
    }

    // Allows live tweaking in the Inspector during Play mode
    private void OnValidate()
    {
        RecalculateOffset();
    }
}
