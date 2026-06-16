using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MeleeAttack : MonoBehaviour
{
    [Header("Hit Detection")]
    public float range = 1.5f;
    public float radius = 1f;
    public float swingCooldown = 0.5f;

    [Header("Lunge")]
    public float lungeForce = 6f;

    [Header("Effects")]
    public GameObject swingEffectPrefab;
    public GameObject hitEffectPrefab;
    public Transform effectSpawnPoint; // reuse FirePoint

    private float _nextSwingTime = 0f;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Swing()
    {
        if (Time.time < _nextSwingTime) return;
        _nextSwingTime = Time.time + swingCooldown;

        _rb.AddForce(transform.forward * lungeForce, ForceMode.Impulse);

        if (swingEffectPrefab != null && effectSpawnPoint != null)
            Instantiate(swingEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation, effectSpawnPoint);

        Vector3 origin = transform.position + transform.forward * range;
        Collider[] hits = Physics.OverlapSphere(origin, radius);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            Debug.Log($"Melee hit: {hit.gameObject.name}");

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, hit.ClosestPoint(origin), Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + transform.forward * range;
        Gizmos.DrawWireSphere(origin, radius);
    }
}