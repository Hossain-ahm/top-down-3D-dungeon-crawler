using UnityEngine;

/// <summary>
/// Fired by the player. Travels forward, destroys on impact or after lifetime expires.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifetime = 3f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Launch in the direction this projectile is facing
        _rb.linearVelocity = transform.forward * speed;

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Don't collide with the player who fired it
        if (collision.gameObject.CompareTag("Player")) return;

        Destroy(gameObject);
    }
}