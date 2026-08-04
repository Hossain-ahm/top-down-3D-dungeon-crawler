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
        // Enemy projectile — damages player, ignores enemies
        if (collision.gameObject.CompareTag("Enemy")) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
        }
    }
    else
    {
        // Player projectile — damages enemies, ignores player
        if (collision.gameObject.CompareTag("Player")) return;
        if (collision.gameObject.CompareTag("Enemy")) return;

        EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
        if (enemy != null)
            enemy.TakeDamage(damage);
    }

    if (impactEffectPrefab != null)
        Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

    Destroy(gameObject);
}
}