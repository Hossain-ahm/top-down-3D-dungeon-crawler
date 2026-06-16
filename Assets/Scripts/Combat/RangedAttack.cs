using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attached to the Player. Spawns a projectile on left mouse click,
/// fired from a spawn point in front of the player.
/// </summary>
public class RangedAttack : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Spawn Point")]
    [Tooltip("Empty GameObject placed in front of the player — child of Player.")]
    public Transform firePoint;

    [Header("Settings")]
    public float fireRate = 0.3f;      // Minimum seconds between shots

    private float _nextFireTime = 0f;

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;
        if (Time.time < _nextFireTime) return;
        if (projectilePrefab == null || firePoint == null) return;

        _nextFireTime = Time.time + fireRate;

        // Spawn projectile at fire point, inheriting player's rotation
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}