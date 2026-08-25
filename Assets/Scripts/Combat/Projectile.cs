using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifetime = 3f;
    public float damage = 10f;

    [Header("Effects")]
    public GameObject impactEffectPrefab;

	[Header("Ownership")]
	public bool firedByEnemy = false;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (firedByEnemy)
        {
            // Enemy projectile: ignore other enemies, damage the player
            if (collision.gameObject.CompareTag("Enemy")) return;

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
        }
        else
        {
            // Player projectile: ignore the player, damage enemies
            if (collision.gameObject.CompareTag("Player")) return;

            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}