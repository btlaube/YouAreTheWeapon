using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    // public float currentHealth;
    // public float maxHealth;
    // [SerializeField] private float startingHealth;

    // TODO: Add separate variables for sword and wielder health
        // Add reference to contols (custom key binds) for "Heal Sword" (default X) and "Heal Wielder" (default C)
    public HealthBar swordHealthBar;
    public float swordCurrentHealth;
    [SerializeField] private float swordMaxHealth;
    [SerializeField] private float swordStartingHealth;
    
    public HealthBar wielderHealthBar;
    public float wielderCurrentHealth;
    [SerializeField] private float wielderMaxHealth;
    [SerializeField] private float wielderStartingHealth;

    public UnityEvent swordBreakEvent;
    public UnityEvent wielderDeathEvent;

    [SerializeField] private List<KeyCode> healSwordKeys;
    [SerializeField] private List<KeyCode> healWielderKeys;
    private bool dead;
    private bool swordDead;
    private bool wielderDead;

    private void Awake()
    {
        swordCurrentHealth = swordStartingHealth;
        swordMaxHealth = swordStartingHealth;

        wielderCurrentHealth = wielderStartingHealth;
        wielderMaxHealth = wielderStartingHealth;
    }

    void Start()
    {
        // Get reference to Settings instance
        Settings settings = Settings.Instance;
        healSwordKeys = settings.LookUpKeyBind("Heal Sword");
        healWielderKeys = settings.LookUpKeyBind("Heal Wielder");
    }

    void Update()
    {
        // Heal Inputs
        if (IsAnyKeyDown(healSwordKeys) && swordCurrentHealth != swordMaxHealth)
        {
            Debug.Log("Heal sword!");
            GainSwordHealth(1f);
            TakeWielderDamage(1f);
        }
        if (IsAnyKeyDown(healWielderKeys) && wielderCurrentHealth != wielderMaxHealth)
        {
            Debug.Log("Heal wielder!");
            GainWielderHealth(1f);
            TakeSwordDamage(1f);
        }
    }

    // Take damage functions
    public void TakeSwordDamage(float damage)
    {
        swordCurrentHealth = Mathf.Clamp(swordCurrentHealth - damage, 0, swordMaxHealth);
        if (swordCurrentHealth > 0)
        {
            // Take sword damage without dying
        }
        else
        {
            if (!swordDead)
            {
                // Enact sword death
                swordDead = true;
                SwordDeath();
            }
        }
        UpdateHealthBar(swordHealthBar, swordCurrentHealth, swordMaxHealth);
    }

    public void TakeWielderDamage(float damage)
    {
        wielderCurrentHealth = Mathf.Clamp(wielderCurrentHealth - damage, 0, wielderMaxHealth);
        if (wielderCurrentHealth > 0)
        {
            // Take wielder damage without dying
        }
        else
        {
            if (!wielderDead)
            {
                // Enact wielder death
                wielderDead = true;
                WielderDeath();
            }
        }
        UpdateHealthBar(wielderHealthBar, wielderCurrentHealth, wielderMaxHealth);
    }

    // Heal Functions
    public void GainSwordHealth(float health)
    {
        swordCurrentHealth = Mathf.Clamp(swordCurrentHealth + health, 0, swordMaxHealth);
        UpdateHealthBar(swordHealthBar, swordCurrentHealth, swordMaxHealth);
    }
    
    public void GainWielderHealth(float health)
    {
        wielderCurrentHealth = Mathf.Clamp(wielderCurrentHealth + health, 0, wielderMaxHealth);
        UpdateHealthBar(wielderHealthBar, wielderCurrentHealth, wielderMaxHealth);
    }

    public void UpdateHealthBar(HealthBar healthBar, float currentHealth, float maxHealth)
    {
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    private bool IsAnyKeyDown(List<KeyCode> attackKeys)
    {
        foreach (var key in attackKeys)
        {
            if (Input.GetKeyDown(key))
                return true;
        }
        return false;
    }

    public void SwordDeath()
    {
        Debug.Log("Sword Breaker!");
        // Display sacrifice wielder option
        swordBreakEvent.Invoke();
    }

    public void WielderDeath()
    {
        Debug.Log("Wielder Falls!");
        // Resets to end of previous room
        wielderDeathEvent.Invoke();
    }
}
