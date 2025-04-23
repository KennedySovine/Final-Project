using UnityEngine;

public class ADMelee : BaseChampion
{

    void Start()
    {
        base.Start();
        UpdateStats();
        AddAbilities();

        health = maxHealth; // Initialize health to max health
        mana = maxMana; // Initialize mana to max mana
    }

    //Based on Vayne from LOL
    private void UpdateStats()
    {
        championType = "AD Melee";
        maxHealth = 550f;
        healthRegen = 0.7f;
        AD = 60f;
        AP = 0f;
        armor = 23f;
        magicResist = 30f;
        attackSpeed = 0.685f;
        movementSpeed = 330f;
        maxMana = 232f;
        manaRegen = 8f; 
        abilityHaste = 0f; 
        critChance = 0f;
        critDamage = 1.75f; // 175% damage on crit
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
            //Passive
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

    public void UseAbility1()
    {
        // Check if the ability is off cooldown and if there is enough mana
        // Put messages up on screen if the ability is on cooldown or not enough mana??? Maybe
        // Dash forward a bit in the direction of movement
        // Empower next attack for 3.5 seconds
        // Add countdown timer for that empower attack time limit
        // Alter bullet prefab with a 'damage dealt' variable to be used in the bullet script that will be increased for the empowered dmg
    }

    public void UseAbility2(){
        // No cooldown
        // No mana cost
        // In game manager, perhaps add a variable that can track these stacks and how many times it has been applied before dealing the true damage
        // Maybe in base character class? Add a variable that counts and checks.
        // Stack duration is 3 seconds before the stack is removed.
        // Make Game Manager bulky if need be
    }

    public void UseAbility3(){
        // Check if ability is off cooldown and if theres enough mana
        // Modify the bullet prefab to deal extra physical damage
        // Add a knockback effect to the target if they are hit by the bolt
    }
}
