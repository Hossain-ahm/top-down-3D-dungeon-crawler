using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Slider healthSlider;
    public Image fillImage;

    [Header("Colours")]
    public Color healthyColour = new Color(0.2f, 0.8f, 0.2f);
    public Color hurtColour    = new Color(0.9f, 0.7f, 0.1f);
    public Color criticalColour = new Color(0.9f, 0.2f, 0.2f);

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.maxHealth;
        }
    }

    private void Update()
    {
        if (playerHealth == null || healthSlider == null) return;

        float current = playerHealth.CurrentHealth;
        healthSlider.value = current;

        if (fillImage != null)
        {
            float percent = current / playerHealth.maxHealth;
            if (percent > 0.5f)
                fillImage.color = healthyColour;
            else if (percent > 0.25f)
                fillImage.color = hurtColour;
            else
                fillImage.color = criticalColour;
        }
    }
}