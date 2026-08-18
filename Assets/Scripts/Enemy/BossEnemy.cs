using UnityEngine;
using System.Collections;

public class BossEnemy : EnemyBase
{
    // Phase tracking
    private int _currentPhase = 1;

    [Header("Phase 1 - Shooting")]
    public float fireRate = 0.8f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    private float _nextFireTime = 0f;

    [Header("Phase 1 - Teleportation")]
    public float teleportInterval = 5f;
    public float teleportMinDistance = 6f;
    public float arenaRadius = 15f;
    public Vector3 arenaCentre;
    public float fadeDuration = 0.5f;

    private float _nextTeleportTime = 0f;
    private bool _isTeleporting = false;

    [Header("Phase 2 - Transformation")]
    public float phase2ScaleMultiplier = 1.4f;
    public Color phase2Colour = Color.red;
    public float transformDuration = 1f;
    public GameObject meleeMinionPrefab;

    [Header("Phase 2 - Attacks")]
    public GameObject swingEffectPrefab;
    public float swingDamage = 30f;
    public float swingRadius = 3f;
    public float swingCooldown = 2f;
    public float lungeRange = 8f;
    public float lungeForce = 20f;
    public float lungeDamage = 25f;
    public float lungeCooldown = 3f;

    private float _nextSwingTime = 0f;
    private float _nextLungeTime = 0f;
    private bool _isLunging = false;
    private bool _isTransforming = false;

    // Material / Renderer reference for alpha fading & color changes
    private Renderer _renderer;
    private Material _material;

    protected override void Awake()
    {
        base.Awake();

        // Boss needs free Y movement for lunge impulses to resolve correctly
        _rb.constraints = RigidbodyConstraints.FreezeRotationX
                          | RigidbodyConstraints.FreezeRotationZ;

        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _material = _renderer.material;
    }

    protected override void Start()
    {
        base.Start();
        _nextTeleportTime = Time.time + teleportInterval;
    }

    public void SetArenaBounds(Vector3 centre, float radius)
    {
        arenaCentre = centre;
        arenaRadius = radius;
    }

    // ─── Health & Phase Switch ───────────────────────────────────────────────

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);

        // Check for 50% health threshold to trigger Phase 2
        if (_currentPhase == 1 && currentHealth <= maxHealth * 0.5f && !_isDead)
        {
            OnPhaseSwitch();
        }
    }

    private void OnPhaseSwitch()
    {
        _currentPhase = 2;

        // Cancel any mid-flight teleportation or fade coroutines
        StopAllCoroutines();
        _isTeleporting = false;

        // Ensure boss is fully visible (opaque) before transforming
        if (_material != null)
        {
            Color c = _material.color;
            c.a = 1f;
            _material.color = c;
        }

        StartCoroutine(TransformCoroutine());
    }

    private IEnumerator TransformCoroutine()
    {
        _isTransforming = true;

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * phase2ScaleMultiplier;
        Color startColour = _material != null ? _material.color : Color.white;

        float elapsed = 0f;
        while (elapsed < transformDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transformDuration;

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            if (_material != null)
            {
                Color c = Color.Lerp(startColour, phase2Colour, t);
                c.a = 1f; // maintain opacity
                _material.color = c;
            }

            yield return null;
        }

        transform.localScale = targetScale;
        if (_material != null)
        {
            Color finalColor = phase2Colour;
            finalColor.a = 1f;
            _material.color = finalColor;
        }

        SpawnMeleeMinions();

        _isTransforming = false;
    }

    private void SpawnMeleeMinions()
    {
        if (meleeMinionPrefab == null) return;

        Vector3 leftOffset  = -transform.right * 2.5f;
        Vector3 rightOffset =  transform.right * 2.5f;
        Vector3 basePos = new Vector3(transform.position.x, 1.5f, transform.position.z);

        GameObject minion1 = Instantiate(meleeMinionPrefab, basePos + leftOffset, Quaternion.identity);
        minion1.tag = "Enemy";

        GameObject minion2 = Instantiate(meleeMinionPrefab, basePos + rightOffset, Quaternion.identity);
        minion2.tag = "Enemy";
    }

    // ─── Main Behaviour Loop ────────────────────────────────────────────────

    protected override void HandleBehaviour()
    {
        // Pause combat logic while teleporting or transforming
        if (_isTeleporting || _isTransforming) return;

        if (_currentPhase == 1)
        {
            Phase1Behaviour();
        }
        else
        {
            Phase2Behaviour();
        }
    }

    private void Phase1Behaviour()
    {
        FacePlayer();
        TryShoot();
        TryTeleport();
    }

    private void Phase2Behaviour()
    {
        FacePlayer();

        float distance = DistanceToPlayer();

        // Choose attack based on distance
        if (distance > lungeRange)
        {
            TryLunge();
        }
        else
        {
            TryHeavySwing();
        }
    }

    // ─── Phase 1: Ranged & Teleport ─────────────────────────────────────────

    private void TryShoot()
    {
        if (Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + fireRate;

            if (projectilePrefab != null && firePoint != null && _playerTransform != null)
            {
                Vector3 direction = (_playerTransform.position - firePoint.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);
                proj.tag = "EnemyProjectile";

                Projectile projectile = proj.GetComponent<Projectile>();
                if (projectile != null)
                    projectile.firedByEnemy = true;
            }
        }
    }

    private void TryTeleport()
    {
        if (Time.time >= _nextTeleportTime)
        {
            Vector3 destination = GetRandomArenaPosition();
            StartCoroutine(TeleportCoroutine(destination));
        }
    }

    private Vector3 GetRandomArenaPosition()
    {
        for (int i = 0; i < 20; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(0f, arenaRadius - 2f);

            Vector3 candidate = arenaCentre + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );
            candidate.y = transform.position.y;

            if (_playerTransform == null || Vector3.Distance(candidate, _playerTransform.position) >= teleportMinDistance)
            {
                return candidate;
            }
        }

        return transform.position;
    }

    private IEnumerator TeleportCoroutine(Vector3 destination)
    {
        _isTeleporting = true;

        yield return StartCoroutine(FadeTo(0f));

        _rb.isKinematic = true;
        transform.position = destination;
        Physics.SyncTransforms();
        _rb.isKinematic = false;

        yield return StartCoroutine(FadeTo(1f));

        _isTeleporting = false;
        _nextTeleportTime = Time.time + teleportInterval;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (_material == null) yield break;

        float startAlpha = _material.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            Color c = _material.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            _material.color = c;

            yield return null;
        }

        Color finalColor = _material.color;
        finalColor.a = targetAlpha;
        _material.color = finalColor;
    }

    // ─── Phase 2: Melee Attacks ─────────────────────────────────────────────

    private void TryHeavySwing()
    {
        if (Time.time < _nextSwingTime) return;
        _nextSwingTime = Time.time + swingCooldown;

        if (swingEffectPrefab != null)
            Instantiate(swingEffectPrefab, transform.position + transform.forward, transform.rotation, transform);

        Vector3 origin = transform.position + transform.forward * (swingRadius * 0.5f);
        Collider[] hits = Physics.OverlapSphere(origin, swingRadius);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(swingDamage);
        }
    }

    private void TryLunge()
    {
        if (Time.time < _nextLungeTime || _playerTransform == null) return;
        _nextLungeTime = Time.time + lungeCooldown;

        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        direction.y = 0f;

        _isLunging = true;
        _rb.AddForce(direction * lungeForce, ForceMode.Impulse);

        StartCoroutine(ResetLungeState());
    }

    private IEnumerator ResetLungeState()
    {
        float elapsed = 0f;
        while (elapsed < 0.8f)
        {
            elapsed += Time.deltaTime;

            if (_isLunging && DistanceToPlayer() <= 2f)
            {
                PlayerHealth playerHealth = _playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(lungeDamage);
                    _isLunging = false;
                    yield break;
                }
            }

            yield return null;
        }

        _isLunging = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Deal damage on contact only while actively lunging
        if (_isLunging)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(lungeDamage);
                _isLunging = false;
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Vector3 origin = transform.position + transform.forward * (swingRadius * 0.5f);
        Gizmos.DrawWireSphere(origin, swingRadius);
    }
}