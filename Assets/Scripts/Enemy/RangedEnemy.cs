using UnityEngine;

public class RangedEnemy : EnemyBase
{
    [Header("Ranged Settings")]
    public float moveSpeed = 3.5f;
    public float preferredDistance = 6f; // Backs away if player gets closer than this
    public float fireRate = 1.5f;        // Seconds between shots

    [Header("Shooting Setup")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    private float _nextFireTime = 0f;

    private void FixedUpdate()
    {
        if (!_isDead && _playerTransform != null)
        {
            float distance = DistanceToPlayer();

            // If detected and player is closer than preferred distance, back away
            if (distance <= detectionRange && distance < preferredDistance)
            {
                RetreatFromPlayer();
            }
            else
            {
                StopMoving();
            }
        }
    }

    protected override void HandleBehaviour()
    {
        float distance = DistanceToPlayer();

        // Always face the player and try to shoot whenever in detection range
        if (distance <= detectionRange)
        {
            FacePlayer();
            TryShoot();
        }
    }

    private void RetreatFromPlayer()
    {
        // Direction vector pointing away from the player
        Vector3 directionAway = (transform.position - _playerTransform.position).normalized;
        directionAway.y = 0f;

        // Apply movement while preserving vertical gravity
        _rb.linearVelocity = new Vector3(
            directionAway.x * moveSpeed,
            _rb.linearVelocity.y,
            directionAway.z * moveSpeed
        );
    }

    private void StopMoving()
    {
        // Halt horizontal velocity, keep vertical velocity for gravity
        _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
    }

    private void TryShoot()
    {
        if (Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + fireRate;

            if (projectilePrefab != null && firePoint != null)
            {
                Vector3 direction = (_playerTransform.position - firePoint.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);
                proj.tag = "EnemyProjectile";
            
                // Mark as enemy projectile
                Projectile projectile = proj.GetComponent<Projectile>();
                if (projectile != null)
                    projectile.firedByEnemy = true;
            }
        }
    }
}