using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RangedAttack : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float fireRate = 0.3f;

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;
    public float recoilForce = 2f;

    private float _nextFireTime = 0f;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Fire()
    {
        if (Time.time < _nextFireTime) return;
        if (projectilePrefab == null || firePoint == null) return;

        _nextFireTime = Time.time + fireRate;

        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (muzzleFlashPrefab != null)
            Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);

        _rb.AddForce(-transform.forward * recoilForce, ForceMode.Impulse);
    }
}