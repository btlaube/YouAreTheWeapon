using UnityEngine;

public class Health : MonoBehaviour
{
    public float currentHealth;
    [SerializeField] private float startingHealth;

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
        Destroy(gameObject);
    }

}
