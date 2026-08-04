using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [Header("Melee Settings")]
    public float moveSpeed = 4f;
    public float attackDamage = 15f;
    public float attackCooldown = 1.5f;

    private float _nextAttackTime = 0f;
    
    public float lungeForce = 5f;
    
    private void FixedUpdate()
    {
        if (!_isDead && _playerTransform != null)
        {
            float distance = DistanceToPlayer();
        
            if (distance <= detectionRange && distance > attackRange)
            {

                ChasePlayer();
            }
            else
                StopMoving();
        }
    }

    protected override void HandleBehaviour()
    {
        float distance = DistanceToPlayer();

        if (distance <= detectionRange)
        {
            FacePlayer();
            if (distance <= attackRange)
                TryAttack();
        }
        else
        {
            StopMoving();
        }
    }

    private void ChasePlayer()
    {
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        direction.y = 0f;
        _rb.linearVelocity = new Vector3(
            direction.x * moveSpeed,
            _rb.linearVelocity.y,
            direction.z * moveSpeed
        );
    }

    private void StopMoving()
    {
        // Halt horizontal movement, keep vertical velocity for gravity
        _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
    }

    private void TryAttack()
    {
        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + attackCooldown;

            // Lunge toward player
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            direction.y = 0f;
            _rb.AddForce(direction * lungeForce, ForceMode.Impulse);

            // Deal damage
            PlayerHealth playerHealth = _playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }
}