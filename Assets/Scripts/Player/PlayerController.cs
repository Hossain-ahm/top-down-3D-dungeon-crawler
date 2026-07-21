using UnityEngine;

/// <summary>
/// Top-down player controller using Rigidbody for physics-based movement.
/// 
/// Setup:
///   - Add this script to your Player GameObject.
///   - Player needs: Rigidbody (freeze Y position & X/Z rotation), Capsule Collider.
///   - Set the 'groundLayer' mask to whatever layer your floor is on.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 15f;   // How quickly the player reaches full speed
    public float deceleration = 20f;   // How quickly the player stops

    [Header("Rotation")]
    [Tooltip("Player rotates to face the mouse cursor on the ground plane.")]
    public bool rotateToCursor = true;
    public float rotationSpeed = 720f; // Degrees per second

    [Header("Ground")]
    public LayerMask groundLayer;      // Assign in Inspector — the floor layer

    // Components
    private Rigidbody _rb;
    private Camera _cam;

    // State
    private Vector3 _moveInput;
    private Vector3 _targetVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main;

        // Lock physics so player doesn't tip over
        _rb.freezeRotation = true;
        _rb.constraints = RigidbodyConstraints.FreezePositionY
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        GatherInput();

        if (rotateToCursor)
            RotateToCursor();
    }

    private void FixedUpdate()
    {
        Move();
    }

    // ─── Input ───────────────────────────────────────────────────────────────

    private void GatherInput()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        // Normalise so diagonal movement isn't faster
        _moveInput = new Vector3(h, 0f, v).normalized;
    }

    // ─── Movement ────────────────────────────────────────────────────────────

    private void Move()
    {
        _targetVelocity = _moveInput * moveSpeed;

        // Choose accel or decel depending on whether we're giving input
        float rate = _moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;

        Vector3 velocity = _rb.linearVelocity;
        velocity.x = Mathf.MoveTowards(velocity.x, _targetVelocity.x, rate * Time.fixedDeltaTime);
        velocity.z = Mathf.MoveTowards(velocity.z, _targetVelocity.z, rate * Time.fixedDeltaTime);

        _rb.linearVelocity = velocity;
    }

    // ─── Rotation ────────────────────────────────────────────────────────────

    private void RotateToCursor()
    {
        if (_cam == null) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        // Raycast against the ground plane to find where the cursor is in world space
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 direction = hit.point - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // ─── Debug ───────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Visualise movement direction in Scene view
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, _moveInput * 2f);
    }
}
