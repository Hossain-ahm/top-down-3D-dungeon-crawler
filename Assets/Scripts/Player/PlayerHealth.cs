using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float _currentHealth;

    public float CurrentHealth => _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Max(0f, _currentHealth);

        if (_currentHealth <= 0f)
            Die();
    }
    
    private void OnEnable()
    {
        PlayerRegistry.Register(transform);
    }

    private void OnDisable()
    {
        PlayerRegistry.Unregister(transform);
    }

private void Die()
{
    if (GameManager.Instance != null)
        GameManager.Instance.GameOver();
}
}