using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public float currentHealth;
    [SerializeField] private float startingHealth;

    public UnityEvent deathEvent;

    private bool dead;

    private void Awake()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            // Take damage, not die
        }
        else
        {
            if (!dead)
            {
                // Die
                dead = true;
                Deactivate();
            }            
        }
    }

    public void Deactivate()
    {
        if (deathEvent != null) deathEvent.Invoke();
        Destroy(gameObject);
    }

}
