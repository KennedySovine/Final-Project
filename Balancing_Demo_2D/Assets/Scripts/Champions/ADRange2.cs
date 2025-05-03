using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ADRange2 : BaseChampion
{

    [Header("Champion Settings")]
    public int attackStacks = 0; // Gwen generates passive stacks per auto attack.

    void Start()
    {
        base.Start();
        UpdateStats();
        AddAbilities();
    }

    //Based on Ashe from LOL
    private void UpdateStats()
    {
        if (!IsServer){
            Debug.LogWarning("UpdateStats can only be called on the server.");
            return;
        }
        
        championType = "AD Range2";
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

        autoAttack.setRange(20f); // Set the range of the auto attack ability
        health.Value = maxHealth.Value; // Initialize health to max health
        mana.Value = maxMana.Value; // Initialize mana to max mana

        //Other stuff
        stackDuration.Value = 4f;

        rapidFire.Value = 1;
        maxStacks.Value = 4; // Maximum number of stacks for the ability

    }

    public override void Update(){
        base.Update(); // Call the base class Update method

        updateIsEmpoweredRpc(true);
        // Ashe is always 'empowered' so she can always apply frost.
    }

    public override void stackManager(){
        // 1 stack expires after 1 second
        if (stackCount.Value > 0){
            if (Time.time > stackStartTime.Value + stackDuration.Value) // If the stack timer is up
            {
                updateStackCountRpc(-1, stackCount.Value, maxStacks.Value);
            }
        }
    }
    //Slow Logic
    public override GameObject empowerLogic(GameObject bullet)
    {
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null){
            bulletComponent.slowAmount = 0.2f; // Set the slow amount to 20%
        }
        return bullet;
    }

    public override GameObject critLogic(GameObject bullet){
        // CRIT DOES FROST AND DOES NOT DO DMG
        float chance = Random.Range(0f, 1f);
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (chance <= critChance.Value) // If the random chance is less than or equal to critChance
        {
            Debug.Log("Critical hit! Critical Frost applied.");
            bulletComponent.slowAmount = 0.4f; // Set the slow amount to 40%
        }
        else
        {
            Debug.Log("Normal hit. No critical damage.");
        }

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
            "PASSIVE: Basic attacks generate a stack of Focus for 4 seconds, which refreshes on additional attacks and stacks up to 4, expriring after a second.",
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

        passive.Stats.championType = championType; // Set the champion type for the passive ability

    }

    public virtual GameObject ability3Logic(GameObject bullet){
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null){
            bulletComponent.ADDamage = 20f + AD.Value; 
            bulletComponent.slowAmount = 0.4f; // Set the slow amount to 40%
        
        }

        return bullet; // Return the modified bullet
    }

    [Rpc(SendTo.Server)]
    public override void passiveAbilityRpc(){
        // Add a slow thing in base champion.
        // Frost = 20% slow for 2 seconds
        // Additional frost damage = 155% of crit chance as AD damage
        //Look at empowerLogic
        
    }

    [Rpc(SendTo.Server)]
    public override void UseAbility1Rpc(){
        // Time delta time to count to 6 seconds or do a coroutine? Maybe?
        // 25 attack speed for 6 seconds
        // Fire 5 arrows rapidly, each doing 21 AD damage
        // Check for stacks of focus and if they are present, consume them
        // Check for mana

        if (!IsServer) return; // Ensure this is only executed on the server
        if (mana.Value < ability1.manaCost && !isMaxStacks) return; // Check if enough mana and stacks are present
        
        updateRapidFireRpc(5);
        StartCoroutine(RapidFireCoroutine(ability1.duration)); // Start the rapid fire coroutine

        updateManaRpc(-ability1.manaCost); // Deduct the mana cost
        ability1.Stats.totalManaSpent += ability1.manaCost; // Update the total mana spent for the ability

        resetStackCountRpc(); // Reset the stack count
    }

    [Rpc(SendTo.Server)]
    public override void UseAbility2Rpc(){
        // Generate focus stacks 
        // Look at ADRange for code
        if (!IsServer) return; // Ensure this is only executed on the server
        Debug.Log("Ability is a passive. No active code to run.");
    }

    [Rpc(SendTo.Server)]
    public override void UseAbility3Rpc(){
        // Check mana and cooldown
        // Crit frost and deal 20 + 100% AD damage
        if (!IsServer) return; // Ensure this is only executed on the server
        if (ability3.isOnCooldown)
        {
            Debug.Log("Ability is on cooldown!");
            return;
        }
        else if (mana.Value < ability3.manaCost)
        {
            Debug.Log("Not enough mana!");
            return;
        }
        updateAbility3UsedRpc(true); // Update the ability range
        updateManaRpc(-ability3.manaCost); // Deduct mana cost
        ability3.Stats.totalManaSpent += ability3.manaCost; // Update the total mana spent for the ability
    }

    private IEnumerator RapidFireCoroutine(float duration)
    {
        var tempAS = attackSpeed.Value; // Store the original attack speed
        var tempAD = AD.Value; // Store the original AD
        Debug.Log("Rapid Fire started!");
        updateAttackSpeedRpc(0.25f); // Increase attack speed by 25
        updateADRpc(.21f); // Increase AD by 21
        yield return new WaitForSeconds(duration); // Wait for the specified duration
        Debug.Log("Rapid Fire ended!");

        // Reset rapid fire state
        updateRapidFireRpc(1);
        attackSpeed.Value = tempAS; // Reset attack speed to original value
        AD.Value = tempAD; // Reset AD to original value
        Debug.Log("Rapid Fire state reset.");
    }
}
