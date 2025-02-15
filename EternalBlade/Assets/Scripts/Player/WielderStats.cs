using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WielderStats : MonoBehaviour
{
    // Movement affects speed, jump height, and in air manueverabiliity, wall cling gravity scale
        // Based on theoretical weight and agility of player
    public int movement;
    // Attack controls length of cooldown between attacks
        // Player agility and combat skill
    public int attackSpeed;
    // Player "Tankiness"
    public int maxHealth;
    // Possibly randomize starting health below maxHealth?
    public int startingHealth;

    [Header("Canvas Controls")]
    public TMP_Text movementText;
    public TMP_Text attackSpeedText;
    public TMP_Text maxHealthText;


    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private PlayerHealth playerHealth;
    
    // Animator
    private Animator playerAnimator;

    public int randomizeStats;

    void Awake()
    {
        randomizeStats = PlayerPrefs.GetInt("SpawnWielder", 0);
        playerController = transform.parent.GetComponent<PlayerController>();
        playerAttack = transform.parent.GetComponent<PlayerAttack>();
        playerHealth = transform.parent.GetComponent<PlayerHealth>();

        playerAnimator = transform.parent.GetComponent<Animator>();
    }
    
    public void InitializePreviousWielder()
    {
        LoadStats();
        ApplyStats();
        UpdateStatDisplay();
    }

    public void InitializeNewWielder()
    {
        InitializeStats();
        ApplyStats();
        UpdateStatDisplay();
    }

    public void InitializeStats()
    {
        movement = Random.Range(1, 6);
        attackSpeed = Random.Range(1, 6);
        maxHealth = Random.Range(4, 7);
        startingHealth = Random.Range(maxHealth - 2, maxHealth + 1);
    }

    public void SetStats(int movement, int attackSpeed, int maxHealth, int startingHealth)
    {
        this.movement = movement;
        this.attackSpeed = attackSpeed;
        this.maxHealth = maxHealth;
        this.startingHealth = startingHealth;
    }

    public void ApplyStats()
    {
        playerHealth.SetWielderMaxHealth((float)maxHealth);
        playerHealth.SetWielderStartingHealth((float)startingHealth);
        playerController.airAcceleration *= movement;
        playerController.acceleration *= movement;
        playerController.jump.y += ((float)movement / 10f) * playerController.jump.y;
        playerAttack.attackCooldown -= (float)attackSpeed / 5f;
    }

    public void SaveStats()
    {
        PlayerPrefs.SetInt("PlayerMovement", movement);
        PlayerPrefs.SetInt("PlayerAttackSpeed", attackSpeed);
        PlayerPrefs.SetInt("PlayerMaxHealth", maxHealth);
        PlayerPrefs.SetInt("PlayerStartingHealth", (int)playerHealth.wielderCurrentHealth);
        // Sword health
        // PlayerPrefs.SetInt("SwordStartingHealth", (int)playerHealth.swordCurrentHealth);
        // PlayerPrefs.Save();
    }

    public void LoadStats()
    {
        movement = PlayerPrefs.GetInt("PlayerMovement", Random.Range(1, 6));
        attackSpeed = PlayerPrefs.GetInt("PlayerAttackSpeed", Random.Range(1, 6));
        maxHealth = PlayerPrefs.GetInt("PlayerMaxHealth", Random.Range(4, 7));
        startingHealth = PlayerPrefs.GetInt("PlayerStartingHealth", Random.Range(maxHealth - 2, maxHealth + 1));
        // Sword Health
        // playerHealth.SetSwordStartingHealth(PlayerPrefs.GetInt("SwordStartingHealth", 5));
    }

    public void UpdateStatDisplay()
    {
        movementText.text = $"{movement}";
        attackSpeedText.text = $"{attackSpeed}";
        maxHealthText.text = $"{maxHealth}";
    }

}
