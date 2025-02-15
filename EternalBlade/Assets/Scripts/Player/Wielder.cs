using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wielder
{

    // Movement affects speed, jump height, and in air manueverabiliity, wall cling gravity scale
        // Based on theoretical weight and agility of player
    private int movement;
    // Attack controls length of cooldown between attacks
        // Player agility and combat skill
    private int attackSpeed;
    // Player "Tankiness"
    private int maxHealth;
    // Possibly randomize starting health below maxHealth?
    private int startingHealth;

    public Wielder(int movement, int attackSpeed, int maxHealth, int startingHealth)
    {
        this.movement = movement;
        this.attackSpeed = attackSpeed;
        this.maxHealth = maxHealth;
        this.startingHealth = startingHealth;
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

    public int GetMovement()
    {
        return this.movement;
    }

    public int GetAttackSpeed()
    {
        return this.attackSpeed;
    }

    public int GetMaxHealth()
    {
        return this.maxHealth;
    }

    public int GetStartingHealth()
    {
        return this.startingHealth;
    }

}
