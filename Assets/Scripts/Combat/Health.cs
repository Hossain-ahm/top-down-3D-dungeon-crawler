using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 50f;

    private float _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (_currentHealth <= 0f) return;

        _currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage ({_currentHealth}/{maxHealth} HP)");

        if (_currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died");
        Destroy(gameObject);
    }
}