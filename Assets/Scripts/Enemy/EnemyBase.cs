using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    protected float currentHealth;

    [Header("Ranges")]
    public float detectionRange = 10f;
    public float attackRange = 2f;

    // Protected fields so subclasses can access them
    protected Transform _playerTransform;
    protected Rigidbody _rb;
    protected bool _isDead = false;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Freeze Y position to keep them on the ground, and X/Z rotation so they don't tip over
        _rb.freezeRotation = true;
        _rb.constraints = RigidbodyConstraints.FreezePositionY 
                        | RigidbodyConstraints.FreezeRotationX 
                        | RigidbodyConstraints.FreezeRotationZ;
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }
    protected virtual void Update()
    {
        if (!_isDead && _playerTransform != null)
        {
            HandleBehaviour();
        }
    }

    // Virtual method to be overridden by subclasses (e.g., ChaserEnemy, ShooterEnemy)
    protected virtual void HandleBehaviour()
    {
        // Empty by default
    }

    // Helper method to get the distance to the player
    protected float DistanceToPlayer()
    {
        if (_playerTransform == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, _playerTransform.position);
    }

    // Helper method to smoothly rotate towards the player
    protected void FacePlayer()
    {
        if (_playerTransform == null) return;

        Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0f; // Keep the rotation strictly horizontal

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    // Standard damage taking logic
    public virtual void TakeDamage(float amount)
    {
        if (_isDead) return;

        currentHealth -= amount;
        
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        _isDead = true;
        Destroy(gameObject);
    }

    // Visualise detection and attack ranges in the editor
    protected virtual void OnDrawGizmosSelected()
    {
        // Draw detection range (Yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw attack range (Red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}