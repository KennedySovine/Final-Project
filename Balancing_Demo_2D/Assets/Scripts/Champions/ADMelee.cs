using UnityEngine;

public class ADMelee : BaseChampion
{
    void Start()
    {
        base.Start();
        UpdateStats();
        AddAbilities();

        health.Value = maxHealth.Value; // Initialize health to max health
        mana.Value = maxMana.Value; // Initialize mana to max mana
    }

    // Based on Vayne from LOL
    private void UpdateStats()
    {
        if (!IsServer){
            Debug.LogWarning("UpdateStats can only be called on the server.");
            return;
        }

        championType = "AD Melee";
        maxHealth.Value = 550f;
        healthRegen.Value = 0.7f;
        AD.Value = 60f;
        AP.Value = 0f;
        armor.Value = 23f;
        magicResist.Value = 30f;
        attackSpeed.Value = 0.685f;
        movementSpeed.Value = 330f;
        maxMana.Value = 232f;
        manaRegen.Value = 8f;
        abilityHaste.Value = 0f;
        critChance.Value = 0f;
        critDamage.Value = 1.75f; // 175% damage on crit
    }

    private void AddAbilities()
    {
        passive = new Ability(
            "Night Hunter",
            "Gain bonus movement speed when moving towards an enemy champion.",
            0f, // No cooldown for passive
            0f, // No mana cost for passive
            0f  // No range for passive
        );

        ability1 = new Ability(
            "Tumble",
            "Dash forward and empower next attack",
            6f, // Cooldown in seconds
            30f, // Mana cost
            300f  // Range
        );

        ability2 = new Ability(
            "Silver Bolts",
            "Basic attacks apply a stack and at 3 stacks, deal bonus true damage",
            0f, // Cooldown in seconds
            0f, // Mana cost
            0f   // No range
        );

        ability3 = new Ability(
            "Condemn",
            "Fire an extra heavy bolt that deals extra physical damage.",
            20f, // Cooldown in seconds
            90f, // Mana cost
            5f   // Range
        );

        ability3.setDuration(8f);
    }

    public void passiveAbility(){
        //Passive ability logic
    }

    public void UseAbility1()
    {
        // Check if the ability is off cooldown and if there is enough mana
        // Put messages up on screen if the ability is on cooldown or not enough mana??? Maybe
        // Dash forward a bit in the direction of movement
        // Empower next attack for 3.5 seconds
        // Add countdown timer for that empower attack time limit
        // Alter bullet prefab with a 'damage dealt' variable to be used in the bullet script that will be increased for the empowered dmg
        if (ability1.cooldownTimer == 0 && mana.Value >= ability1.manaCost)
        {
            // Perform the Tumble action here
            // Set the cooldown timer for the ability
            ability1.cooldownTimer = ability1.cooldown;
            mana.Value -= ability1.manaCost; // Deduct mana cost
        }
        else if (ability1.cooldownTimer > 0)
        {
            Debug.Log("Ability is on cooldown!");
        }
        else if (mana.Value < ability1.manaCost)
        {
            Debug.Log("Not enough mana!");
        }
    }

    public void UseAbility2()
    {
        // No cooldown
        // No mana cost
        // In game manager, perhaps add a variable that can track these stacks and how many times it has been applied before dealing the true damage
        // Maybe in base character class? Add a variable that counts and checks.
        // Stack duration is 3 seconds before the stack is removed.
        // Make Game Manager bulky if need be
    }

    public void UseAbility3()
    {
        // Check if ability is off cooldown and if theres enough mana
        if (ability3.cooldownTimer == 0 && mana.Value >= ability3.manaCost)
        {
            // Perform the Tumble action here
            // Set the cooldown timer for the ability
            ability3.cooldownTimer = ability3.cooldown;
            mana.Value -= ability3.manaCost; // Deduct mana cost
        }
        else if (ability3.cooldownTimer > 0)
        {
            Debug.Log("Ability is on cooldown!");
        }
        else if (mana.Value < ability3.manaCost)
        {
            Debug.Log("Not enough mana!");
        }
        // Modify the bullet prefab to deal extra physical damage
        // Add a knockback effect to the target if they are hit by the bolt
    }
}
