using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    // [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image totalHealthBar;
    [SerializeField] private Image currentHealthBar;

    // void Awake()
    // {
    //     playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
    // }

    // void Update()
    // {
    //     if (playerHealth == null) return;
    //     totalHealthBar.fillAmount = playerHealth.maxHealth / 10;
    //     currentHealthBar.fillAmount = playerHealth.currentHealth / 10;
        
    // }


    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        currentHealthBar.fillAmount = currentHealth / 10;
        totalHealthBar.fillAmount = maxHealth / 10;
    }

}
