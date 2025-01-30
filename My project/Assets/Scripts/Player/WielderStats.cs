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
    public float attackSpeed;
    // Player "Tankiness"
    public int maxHealth;
    // Possibly randomize starting health below maxHealth?
    public int startingHealth;


    [Header("Canvas Controls")]
    public TMP_Text movementText;
    public TMP_Text attackSpeedText;
    public TMP_Text maxHealthText;


    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;

    void Awake()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
        playerHealth = transform.parent.GetComponent<PlayerHealth>();
        InitializeStats();
        ApplyStats();
        UpdateStatDisplay();
    }

    public void InitializeStats()
    {
        movement = Random.Range(1, 6);
        attackSpeed = Random.Range(1, 6);
        maxHealth = Random.Range(4, 7);
        startingHealth = Random.Range(3, 6);
    }

    public void ApplyStats()
    {
        playerHealth.SetWielderMaxHealth((float)maxHealth);
        playerHealth.SetWielderStartingHealth((float)startingHealth);
        playerController.airAcceleration *= movement;
        playerController.acceleration *= movement;
        playerController.jump.y += ((float)movement / 10f) * playerController.jump.y;
    }

    public void UpdateStatDisplay()
    {
        movementText.text = $"{movement}";
        attackSpeedText.text = $"{attackSpeed}";
        maxHealthText.text = $"{maxHealth}";
    }

}
