using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;
    [SerializeField] private float startingHealth;

    // TODO: Add separate variables for sword and wielder health
        // Add reference to contols (custom key binds) for "Heal Sword" (default X) and "Heal Wielder" (default C)

    public UnityEvent deathEvent;

    private bool dead;

    private void Awake()
    {
        currentHealth = startingHealth;
        maxHealth = startingHealth;
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
