using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    [Header("References")]
    public GameObject uiRoot;
    public Slider healthSlider;
    public TextMeshProUGUI bossNameText;

    private EnemyBase _boss;

    private void Start()
    {
        if (uiRoot != null)
            uiRoot.SetActive(false);
    }

    public void SetBoss(EnemyBase boss)
    {
        _boss = boss;

        if (uiRoot != null)
            uiRoot.SetActive(true);

        if (healthSlider != null && _boss != null)
        {
            healthSlider.maxValue = _boss.maxHealth;
            healthSlider.value = _boss.maxHealth;
        }
    }

    private void Update()
    {
        if (_boss == null)
        {
            if (uiRoot != null && uiRoot.activeSelf)
                uiRoot.SetActive(false);
            return;
        }

        if (healthSlider != null)
            healthSlider.value = _boss.CurrentHealth;
    }
}