using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        Debug.Log($"Player took {amount} damage ({_currentHealth}/{maxHealth} HP)");

        if (_currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        Debug.Log("Player died — TODO: game over screen");
    }
}