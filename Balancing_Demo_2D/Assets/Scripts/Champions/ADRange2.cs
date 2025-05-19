using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ADRange2 : BaseChampion
{
    #region Fields and Properties
    [Header("Champion Settings")]
    public int attackStacks = 0; // Gwen generates passive stacks per auto attack.
    #endregion

    #region Initialization Methods
    new void Start()
    {
        base.Start();
        UpdateStats();
        AddAbilities();

        if (IsOwner)
        {
            attackSpeed.OnValueChanged += (previousValue, newValue) =>
            { // Update the attack speed value
                UpdateAbility1CooldownRpc(newValue); // Update the ability cooldown based on attack speed
            };
        }
    }

    //Based on Ashe from LOL
    private void UpdateStats()
    {
        if (!IsServer) return;

        championType = "AD Range2";
        /*maxHealth.Value = 610f;
        healthRegen.Value = 0.7f;
        AD.Value = 60f;
        AP.Value = 0f;
        armor.Value = 26f;
        magicResist.Value = 30f;
        attackSpeed.Value = 0.658f;
        movementSpeed.Value = 10.8f; //325 original
        maxMana.Value = 280f;
        manaRegen.Value = 1.4f; 
        abilityHaste.Value = 0f; 
        critChance.Value = 0f;
        critDamage.Value = 1f; // 175% damage on crit*/
        health.Value = maxHealth.Value; // Initialize health to max health
        mana.Value = maxMana.Value; // Initialize mana to max mana

        //LoadModifiedStats(GM.playerChampionsData[3]);

        //Other stuff
        stackDuration.Value = 4f;
        rapidFire.Value = 1;
        maxStacks.Value = 4; // Maximum number of stacks for the ability
    }

    public override ChampionData ForTheMainMenu()
    {
        return new ChampionData
        {
            championType = "AD Range2",
            maxHealth = 610f,
            healthRegen = 0.7f,
            AD = 60f,
            AP = 0f,
            armor = 26f,
            magicResist = 30f,
            attackSpeed = 0.658f,
            movementSpeed = 10.8f, // 325 original
            maxMana = 280f,
            manaRegen = 1.4f,
            abilityHaste = 0f,
            critChance = 0f,
            critDamage = 1f, // 175% damage on crit
            armorPen = 0f,
            magicPen = 0f,
            missileSpeed = 0f // or whatever default you want
        };
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
            "Rapid Frost (Q)",
            "ACTIVE: For 6 seconds, gain <i>25% bonus attack speed<i> and fire 5 shots rapidly. Applies <i>21<i> AD per arrow. Cannot cast unless there are 4 stacks of Focus",
            0f,
            30f,
            0f
        );
        ability1.icon = Resources.Load<Sprite>("Sprites/Ashe_RangerFocus_Default");
        ability1.icon2 = Resources.Load<Sprite>("Sprites/Ashe_RangerFocus_Empowered");

        ability2 = new Ability(
            "Ranger's Focus (W)",
            "PASSIVE: Basic attacks generate a stack of Focus for 4 seconds, which refreshes on additional attacks and stacks up to 4, expriring after a second.",
            0f,
            0f,
            0f
        );
        ability2.icon = Resources.Load<Sprite>("Sprites/Ashe_Enchanted_Crystal_Arrow");

        ability3 = new Ability(
            "Volley (E)",
            "Next arrow applies critical Frost and deals extra physical damage.",
            18f,
            75f,
            0f
        );
        ability3.icon = Resources.Load<Sprite>("Sprites/Ashe_Volley");
        ability3.SetDuration(6f);

        autoAttack.SetRange(20f);
        ability1.SetDuration(6f);
        passive.Stats.championType = championType;

        abilityDict.Add("Q", ability1);
        abilityDict.Add("W", ability2);
        abilityDict.Add("E", ability3);

        SendToUI();
    }
    #endregion

    #region Core Game Loop Methods
    public override void Update()
    {
        // Ensure ability cooldowns are updated each frame
        base.Update();

        UpdateIsEmpoweredRpc(true);
        // Ashe is always 'empowered' so she can always apply frost.
        // Ashe's ranger's focus cooldown is dependent on attackspeed
    }

    public override void StackManager()
    {
        // 1 stack expires after 1 second
        if (stackCount.Value > 0)
        {
            if (Time.time > stackStartTime.Value + stackDuration.Value) // If the stack timer is up
            {
                UpdateStackCountRpc(-1, stackCount.Value, maxStacks.Value);
            }
        }
    }

    public override void AbilityIconCooldownManaChecks()
    {
        if (IsOwner && iconsSet) // Only the owner should check cooldowns and mana
        {
            // Check ability 1 (Rapid Frost)
            if (ability1 != null && isMaxStacks && !ability1.isOnCooldown && mana.Value >= ability1.manaCost) // Check if ability 1 is available and max stacks are present
            {
                IGUIM.ButtonInteractable("Q", true); // Enable button for ability 1
                IGUIM.AsheEmpowerIcon(true, ability1); // Set empowered icon for Ashe
            }
            else
            {
                IGUIM.ButtonInteractable("Q", false); // Disable button for ability 1
                IGUIM.AsheEmpowerIcon(false, ability1); // Set normal icon for Ashe
            }

            // Check ability 2 (Ranger's Focus)
            if (ability2 != null && !ability2.isOnCooldown && mana.Value >= ability2.manaCost) // Check if ability 2 is available
            {
                IGUIM.ButtonInteractable("W", true); // Enable button for ability 2
            }
            else
            {
                IGUIM.ButtonInteractable("W", false); // Disable button for ability 2
            }

            // Check ability 3 (Volley)
            if (ability3 != null && !ability3.isOnCooldown && mana.Value >= ability3.manaCost) // Check if ability 3 is available
            {
                IGUIM.ButtonInteractable("E", true); // Enable button for ability 3
            }
            else
            {
                IGUIM.ButtonInteractable("E", false); // Disable button for ability 3
            }
        }
    }
    #endregion

    #region Ability Logic Methods
    //Slow Logic
    public override GameObject EmpowerLogic(GameObject bullet)
    {
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.slowAmount = 0.2f; // Set the slow amount to 20%
        }
        return bullet;
    }

    public override GameObject CritLogic(GameObject bullet)
    {
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

    public override GameObject Ability3Logic(GameObject bullet)
    {
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.ADDamage = 20f + AD.Value;
            bulletComponent.slowAmount = 0.4f; // Set the slow amount to 40%
        }

        return bullet; // Return the modified bullet
    }

    private IEnumerator RapidFireCoroutine(float duration)
    {
        var tempAS = attackSpeed.Value; // Store the original attack speed
        var tempAD = AD.Value; // Store the original AD
        Debug.Log("Rapid Fire started!");
        UpdateAttackSpeedRpc(0.25f); // Increase attack speed by 25
        UpdateADRpc(.21f); // Increase AD by 21
        yield return new WaitForSeconds(duration); // Wait for the specified duration
        Debug.Log("Rapid Fire ended!");

        // Reset rapid fire state
        UpdateRapidFireRpc(1);
        attackSpeed.Value = tempAS; // Reset attack speed to original value
        AD.Value = tempAD; // Reset AD to original value
        Debug.Log("Rapid Fire state reset.");
    }
    #endregion

    #region RPC Methods
    [Rpc(SendTo.Server)]
    public override void PassiveAbilityRpc()
    {
        // Add a slow thing in base champion.
        // Frost = 20% slow for 2 seconds
        // Additional frost damage = 155% of crit chance as AD damage
        // Look at EmpowerLogic
    }

    [Rpc(SendTo.Server)]
    public override void UseAbility1Rpc()
    {
        // Time delta time to count to 6 seconds or do a coroutine? Maybe?
        // 25 attack speed for 6 seconds
        // Fire 5 arrows rapidly, each doing 21 AD damage
        // Check for stacks of focus and if they are present, consume them
        // Check for mana

        if (mana.Value < ability1.manaCost || !isMaxStacks || ability1.isOnCooldown) return; // Check if enough mana and stacks are present
        SetAbilityTimeOfCastRpc("Q", Time.time); // Set the ability time of cast for all clients
        if (!IsServer) return; // Ensure this is only executed on the server
        UpdateRapidFireRpc(5);
        StartCoroutine(RapidFireCoroutine(ability1.duration)); // Start the rapid fire coroutine

        UpdateManaRpc(-ability1.manaCost); // Deduct the mana cost
        ability1.Stats.totalManaSpent += ability1.manaCost; // Update the total mana spent for the ability

        ResetStackCountRpc(); // Reset the stack count
    }

    [Rpc(SendTo.Server)]
    public override void UseAbility2Rpc()
    {
        // Generate focus stacks 
        // Look at ADRange for code
        if (!IsServer) return; // Ensure this is only executed on the server
        Debug.Log("Ability is a passive. No active code to run.");
    }

    [Rpc(SendTo.Server)]
    public override void UseAbility3Rpc()
    {
        // Check mana and cooldown
        // Crit frost and deal 20 + 100% AD damage
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
        if (!IsServer) return; // Ensure this is only executed on the server
        SetAbilityTimeOfCastRpc("E", Time.time); // Set the ability time of cast for all clients
        UpdateAbility3UsedRpc(true); // Update the ability range
        UpdateManaRpc(-ability3.manaCost); // Deduct mana cost
        ability3.Stats.totalManaSpent += ability3.manaCost; // Update the total mana spent for the ability
    }

    [Rpc(SendTo.Server)]
    public void UpdateAbility1CooldownRpc(float attackSpeed)
    {
        if (!IsServer) return; // Ensure this is only executed on the server

        if (attackSpeed < 0.75f)
        {
            ability1.SetCooldown(6.08f);
        }
        else if (attackSpeed < 1f)
        {
            ability1.SetCooldown(5.33f);
        }
        else if (attackSpeed < 1.25f)
        {
            ability1.SetCooldown(4f);
        }
        else if (attackSpeed < 1.5f)
        {
            ability1.SetCooldown(3.2f);
        }
        else if (attackSpeed < 1.75f)
        {
            ability1.SetCooldown(2.67f);
        }
        else if (attackSpeed < 2f)
        {
            ability1.SetCooldown(2.29f);
        }
        else if (attackSpeed < 2.25f)
        {
            ability1.SetCooldown(1.78f);
        }
        else if (attackSpeed < 2.5f)
        {
            ability1.SetCooldown(1.6f);
        }
    }
    #endregion
    public override void LoadModifiedStats(ChampionData data)
    {
        base.LoadModifiedStats(data);
        health.Value = maxHealth.Value;
        mana.Value = maxMana.Value;
    }
}
