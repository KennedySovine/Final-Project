using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class ADRange2 : BaseChampion
{

    [Header("Champion Settings")]
    public int attackStacks = 0; // Gwen generates passive stacks per auto attack.

    void Start()
    {
        base.Start();
        UpdateStats();
        AddAbilities();
        health.Value = maxHealth.Value; // Initialize health to max health
        mana.Value = maxMana.Value; // Initialize mana to max mana


    }

    //Based on Ashe from LOL
    private void UpdateStats()
    {
        if (!IsServer){
            Debug.LogWarning("UpdateStats can only be called on the server.");
            return;
        }
        
        championType = "AD Range";
        maxHealth.Value = 610f;
        healthRegen.Value = 0.7f;
        AD.Value = 59f;
        AP.Value = 10f;
        armor.Value = 26f;
        magicResist.Value = 30f;
        attackSpeed.Value = 0.658f;
        movementSpeed.Value = 10.8f; //325 original
        maxMana.Value = 280f;
        manaRegen.Value = 1.4f; 
        abilityHaste.Value = 0f; 
        critChance.Value = 0f;
        critDamage.Value = 1f; // 175% damage on crit

        autoAttack.setRange(2f); // Set the range of the auto attack ability
        health.Value = maxHealth.Value; // Initialize health to max health
        mana.Value = maxMana.Value; // Initialize mana to max mana

        //Other stuff
        stackDuration.Value = 4f;

    }

    public override void Update(){
        base.Update(); // Call the base class Update method

        if (stackCount.Value >= 4){
            maxStacks.Value = true; // Set the max stacks flag to true
            stackCount.Value = 4; // Reset the stack value
        }
    }

    public override GameObject stackLogic(GameObject bullet)
    {
        return bullet;
    }

    private void AddAbilities()
    {
        passive = new Ability(
            "Frost Shot",
            "Basic attacks and abilities apply Frost for 2 seconds, slowing them by 20%. Attacks do not crit but instead double frost.",
            0f, // No cooldown for passive
            0f, // No mana cost for passive
            0f  // No range for passive
        );

        ability1 = new Ability(
            "Rapid Frost",
            "ACTIVE: For 6 seconds, gain 25 bonus attack speed and fire 5 shots rapidly. Applies 21 AD per arrow. Cannot cast unless there are 4 stacks of Focus",
            0f, // Cooldown in seconds
            30f, // Mana cost
            0f  // No range
        );

        ability2 = new Ability(
            "Ranger's Focus",
            "PASSIVE: While inactive, basic attacks generate a stack of Focus for 4 seconds, which refreshes on additional attacks and stacks up to 4, expriring after a second.",
            0f, // Cooldown in seconds
            0f, // Mana cost
            0f // No range
            
        );

        ability3 = new Ability(
            "Volley",
            "Next arrow applies critical Frost and deals extra physical damage.",
            18f, // Cooldown in seconds
            75f, // Mana cost
            0f // Range
        );

        ability1.setDuration(6f);

    }

    public void passiveStats(){
        // Add a slow thing in base champion.
        // Frost = 20% slow for 2 seconds
        // Additional frost damage = 155% of crit chance as AD damage
        
    }

    [Rpc(SendTo.Server)]
    public void UseAbility1Rpc(){
        // Time delta time to count to 6 seconds or do a coroutine? Maybe?
        // 25 attack speed for 6 seconds
        // Fire 5 arrows rapidly, each doing 21 AD damage
        // Check for stacks of focus and if they are present, consume them
        // Check for mana

        if (!IsServer) return; // Ensure this is only executed on the server
        if (mana.Value < ability1.manaCost && maxStacks.Value == false) return; // Check if enough mana and stacks are present
        
        float startTime = Time.time; // Get the start time of the ability
        rapidFire.Value = 5; // Fire 5 arrows every auto
        StartCoroutine(RapidFireCoroutine(ability1.duration)); // Start the rapid fire coroutine

        mana.Value -= ability1.manaCost; // Deduct the mana cost
        maxStacks.Value = false; // Reset the max stacks flag
        stackCount.Value = 0; // Reset the stack count

    }

    [Rpc(SendTo.Server)]
    public void UseAbility2Rpc(){
        // Generate focus stacks 
        // Look at ADRange for code
        
    }

    [Rpc(SendTo.Server)]
    public void UseAbility3Rpc(){
        // Check mana and cooldown
        // Crit frost and deal 20 + 100% AD damage
    }

    private IEnumerator RapidFireCoroutine(float duration)
    {
        Debug.Log("Rapid Fire started!");
        yield return new WaitForSeconds(duration); // Wait for the specified duration
        Debug.Log("Rapid Fire ended!");

        // Reset rapid fire state
        rapidFire.Value = 0;
        Debug.Log("Rapid Fire state reset.");
    }
}
