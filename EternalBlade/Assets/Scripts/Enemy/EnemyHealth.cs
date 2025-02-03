using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;
    [SerializeField] private float startingHealth;

    public UnityEvent deathEvent;
    public GameObject dropItem;

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
        Instantiate(dropItem, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
