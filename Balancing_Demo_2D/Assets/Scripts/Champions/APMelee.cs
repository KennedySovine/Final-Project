using UnityEngine;

public class APMelee : BaseChampion
{

    [Header("Champion Settings")]
    public int attackStacks = 0; // Gwen generates passive stacks per auto attack.

    void Start()
    {
        base.Start();
        UpdateStats();
        AddAbilities();
        health = maxHealth; // Initialize health to max health
        mana = maxMana; // Initialize mana to max mana
    }

    //Based on Gwen from LOL
    private void UpdateStats()
    {
        championType = "AP Melee";
        maxHealth = 620f;
        healthRegen = 1.8f;
        AD = 53f;
        AP = 10f;
        armor = 39f;
        magicResist = 32f;
        attackSpeed = 0.69f;
        movementSpeed = 340f;
        maxMana = 330f;
        manaRegen = 1.5f; 
        abilityHaste = 0f; 
        critChance = 0f;
        critDamage = 1.75f; // 175% damage on crit
    }

    protected override void OnSuccessfulAutoAttack()
    {
        base.OnSuccessfulAutoAttack(); // Optional: Call the base implementation
        attackStacks++; // Increase attack stacks
        Debug.Log($"{championType} now has {attackStacks} attack stacks.");
    }

    private void AddAbilities()
    {
        passive = new Ability(
            "Thousand Cuts",
            "Basic attacks and Snip Snip! deal bonus magic damage based on own AP and the target's maximum health. Attack also heals.",
            0f, // No cooldown for passive
            0f, // No mana cost for passive
            0f  // No range for passive
        );

        ability1 = new Ability(
            "Snip Snip!",
            "PASSIVE: Basic attacks generate a stack for 6 seconds, stacking up to 4 times. ACTIVE: Snip in a target direction, as many times as stacks, dealing magic damage to all enemies hit.",
            6.5f, // Cooldown in seconds
            40f, // Mana cost
            0f  // No range
        );

        ability2 = new Ability(
            "Hallowed Mist",
            "Gain bonus armor and magic resist for 5 seconds.",
            22f, // Cooldown in seconds
            60f, // Mana cost
            0f // No range
            
        );

        ability3 = new Ability(
            "Skip 'n Slash",
            "Dash to a target location and empower next basic attacks within the next 4 seconds to deal bonus magic damage and bonus attack range.",
            13f, // Cooldown in seconds
            35f, // Mana cost
            350f // Range
        );

    }

    public void passiveStats(){
        // Bonus magic damage = 1% (+0.6% per 100 Ap) of the target's maximum health
        // Healing is 50% of damage dealt after mitigation, capped at 10 + 6.5% AP
    }

    public void UseAbility1(){
        for (int i = 0; i < attackStacks; i++){
            // Perform the Snip Snip! action here
            attackStacks = 0; // Reset stacks after using the ability
            // Apply damage to enemies hit by the ability
            // Damage is 10 + 5% AP
            Debug.Log($"{championType} uses Snip Snip! for {ability1.duration} seconds!");
        }
    }

    public void UseAbility2(){
        // Do a timedelta time thing to calculate the 5 seconds
        // Check mana and cooldown
        //  Armor and MR increase by 22 + 7% AP
        
    }

    public void UseAbility3(){
        // Check mana and cooldown
        // Dash to target location
        // Empower next basic attacks within the next 4 seconds to deal bonus magic damage and bonus attack range.
        // Bonus magic damage = 15 + 20% AP
        // Bonus attack range = 75
    }
}

