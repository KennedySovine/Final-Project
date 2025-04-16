using UnityEngine;

public class ADMelee : BaseChampion
{
    public Ability passiveAbility;
    public Ability ability1; // Q
    public Ability ability2; // W
    public Ability ability3; // E

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        UpdateStats();
        AddAbilities();
    }

    //Based on Vayne from LOL
    private void UpdateStats()
    {
        championType = "AD Melee";
        health = 550f;
        healthRegen = 3.5f;
        AD = 60f;
        AP = 0f;
        armor = 23f;
        magicResist = 30f;
        attackSpeed = 0.685f;
        movementSpeed = 330f;
        mana = 232f;
        manaRegen = 8f; 
        abilityHaste = 0f; 
        critChance = 0f;
        critDamage = 1.75f; // 175% damage on crit
    }

    private void AddAbilities()
    {
        passiveAbility = new Ability(
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
        
    }
}
