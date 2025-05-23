using UnityEngine;
using Unity.Netcode;

public class ADRange : BaseChampion
{
    #region Initialization Methods
    new void Start()
    {
        base.Start();
        UpdateStats();
        AddAbilities();
    }

    // Based on Vayne from LOL
    private void UpdateStats()
    {
        if (!IsServer) return;

        championType = "AD Range";
        /*maxHealth.Value = 550f;
        healthRegen.Value = 0.7f;
        AD.Value = 60f;
        AP.Value = 5f;
        armor.Value = 23f;
        magicResist.Value = 30f;
        attackSpeed.Value = 0.685f;
        movementSpeed.Value = 11f; // Look at Base Champ for calculation
        maxMana.Value = 232f;
        manaRegen.Value = 1.4f;
        abilityHaste.Value = 0f;
        critChance.Value = 0f;
        critDamage.Value = 1.75f; // 175% damage on crit*/
        //LoadModifiedStats(GM.playerChampionsData[2]); // Load the modified stats from the GameManager

        // Don't set health/mana here, let LoadModifiedStats handle it after stats are loaded
        missileSpeed.Value = 33f;
        maxStacks.Value = 3; // Maximum number of stacks for the ability
    }

    public override ChampionData ForTheMainMenu()
    {
        return new ChampionData
        {
            championType = "AD Range",
            maxHealth = 550f,
            healthRegen = 0.7f,
            AD = 60f,
            AP = 5f,
            armor = 23f,
            magicResist = 30f,
            attackSpeed = 0.685f,
            movementSpeed = 11f, // Look at Base Champ for calculation
            maxMana = 232f,
            manaRegen = 1.4f,
            abilityHaste = 0f,
            critChance = 0f,
            critDamage = 1.75f, // 175% damage on crit
            armorPen = 0f,
            magicPen = 0f,
            missileSpeed = 33f
        };
    }

    private void AddAbilities()
    {
        autoAttack.SetRange(18f); // Set the range of the auto attack ability
        
        passive = new Ability(
            "Night Hunter",
            "Gain bonus movement speed when moving towards an enemy champion.",
            0f, // No cooldown for passive
            0f, // No mana cost for passive
            0f  // No range for passive
        );

        ability1 = new Ability(
            "Tumble (Q)",
            "Dash forward and empower next attack within 3 seconds. Deal bonus AD damage <i>(75 + 50% AP)<i>",
            6f, // Cooldown in seconds
            30f, // Mana cost
            10f  // Range
        );
        ability1.icon = Resources.Load<Sprite>("Sprites/Vayne_Tumble"); // Load the icon for the ability from Resources folder

        ability2 = new Ability(
            "Silver Bolts (W)",
            "Basic attacks apply a stack and at 3 stacks, deal bonus true damage based on <i>6% of the target's max health</i>. Deals minimum <i>50</i> bonus damage.",
            0f, // Cooldown in seconds
            0f, // Mana cost
            0f  // No range
        );
        ability2.icon = Resources.Load<Sprite>("Sprites/Vayne_Silver_Bolts"); // Load the icon for the ability from Resources folder

        ability3 = new Ability(
            "Condemn (E)",
            "Fire an extra heavy bolt that deals extra physical damage <i>(50 + 50% AD)</i>.",
            20f, // Cooldown in seconds
            90f, // Mana cost
            5f   // Range
        );
        ability3.icon = Resources.Load<Sprite>("Sprites/Vayne_Condemn"); // Load the icon for the ability from Resources folder
        ability3.SetDuration(8f);

        passive.Stats.championType = championType; // Set the champion type for the passive ability

        abilityDict.Add("Q", ability1); // Add the ability to the UI manager
        abilityDict.Add("W", ability2); // Add the ability to the UI manager
        abilityDict.Add("E", ability3); // Add the ability to the UI manager

        SendToUI();
    }
    #endregion

    #region Core Game Loop Methods
    public override void Update(){
        base.Update(); // Call the base class Update method
    }

    public override void StackManager(){
        if (stackCount.Value > 0){
            if (Time.time > stackStartTime.Value + stackDuration.Value) // If the stack timer is up
            {
                ResetStackCountRpc(); // Reset the stack count
            }
        }
    }
    #endregion

    #region Ability Logic Methods
    public override GameObject EmpowerLogic(GameObject bullet)
    {
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.ADDamage += 75f + (AP.Value * 0.5f); // 50% more dmg based on AP, but dealt as AD
        }
        else
        {
            Debug.LogError("Bullet component is missing on the bullet prefab.");
        }
        return bullet;
    }

    public override GameObject StackLogic(GameObject bullet)
    {
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.ADDamage = Mathf.Max(50f, 0.06f * enemyChampion.GetComponent<BaseChampion>().maxHealth.Value); // Minimum damage is 50, max is 6% of target's max health
        }
        else
        {
            Debug.LogError("Bullet component is missing on the bullet prefab.");
        }

        return bullet;
    }

    public override GameObject Ability3Logic(GameObject bullet)
    {
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.ADDamage = 50f + (AD.Value * 0.5f);
        }
        else
        {
            Debug.LogError("Bullet component is missing on the bullet prefab.");
        }
        return bullet;
    }
    #endregion

    #region RPC Methods
    [Rpc(SendTo.Server)]
    public override void PassiveAbilityRpc(){
        if (!IsServer) return; // Only the owner can use this ability
        //Passive ability logic
        if (enemyChampion == null){
            Debug.LogWarning("No enemy champion assigned.");
            return;
        }

        // Get the direction to the enemy
        Vector3 directionToEnemy = (enemyChampion.transform.position - transform.position).normalized;

        // Get the player's movement direction
        Vector3 movementDirection = (PN.targetPositionNet.Value - transform.position).normalized;

        // Check if the player is moving towards the enemy
        float dotProduct = Vector3.Dot(directionToEnemy, movementDirection);

        if (dotProduct > 0.5f){
            //Debug.Log("Player is moving towards the enemy. Passive ability activated!");
            movementSpeed.Value = 12f;
        }
        else{
            //Debug.Log("Player is not moving towards the enemy.");
            // Reset movement speed or remove the passive effect
            movementSpeed.Value = 11f; // Reset to default speed
        }
    }

    [Rpc(SendTo.Server)]
    public override void UseAbility1Rpc()
    {
        if (!IsServer) return; // Only the server can execute this logic
        
        if (ability1.isOnCooldown)
        {
            Debug.Log("Ability is on cooldown!");
            return;
        }
        else if (mana.Value < ability1.manaCost)
        {
            Debug.Log("Not enough mana!");
            return;
        }

        // Record the time of cast and synchronize it with clients
        float castTime = Time.time;
        SetAbilityTimeOfCastRpc("Q", castTime);

        float newMoveSpeed = movementSpeed.Value + 17f; // Increase movement speed by 1 unit
        UpdateManaRpc(-ability1.manaCost); // Deduct mana cost
        ability1.Stats.totalManaSpent += ability1.manaCost; // Update total mana spent for the ability
        Debug.Log("Tumble ability used. Player dashed towards the target position.");

        // Empower the next attack
        UpdateIsEmpoweredRpc(true); // Set the empowered state to true

        PN.ChampionDashRpc(PN.mousePosition, ability1.range, newMoveSpeed); // Call the dash function on the player network object
    }

    [Rpc(SendTo.Server)]
    public override void UseAbility2Rpc()
    {
        // No cooldown
        // No mana cost
        // In game manager, perhaps add a variable that can track these stacks and how many times it has been applied before dealing the true damage
        // Maybe in base character class? Add a variable that counts and checks.
        // Stack duration is 3 seconds before the stack is removed.
        // 6% of targets maximum health as bonus true damage on 3rd attack.

        // INSTEAD, track how many attacks, third always does more damage
        // Make Game Manager bulky if need be
        if (!IsServer) return; // Only the owner can use this ability

        Debug.Log("This ability is a passive. No active use needed.");
    }

    [Rpc(SendTo.Server)]
    public override void UseAbility3Rpc()
    {
        if (!IsServer) return; // Only the owner can use this ability
        // Check if ability is off cooldown and if theres enough mana
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
        else if (ability3Used.Value)
        {
            Debug.Log("Ability 3 has already been used!");
            return;
        }

        SetAbilityTimeOfCastRpc("E", Time.time); // Record the time of cast and synchronize it with clients

        UpdateManaRpc(-ability3.manaCost); // Update the mana on the server
        ability3.Stats.totalManaSpent += ability3.manaCost; // Update the total mana spent for the ability
        LogAbilityUsedRpc(ability3); // Log the ability used
        UpdateAbility3UsedRpc(true); // Update the ability used flag
        // Modify the bullet prefab to deal extra physical damage
        // Add a knockback effect to the target if they are hit by the bolt
    }
    #endregion

    // Ensure health/mana are set to max after stats are loaded
    public override void LoadModifiedStats(ChampionData data)
    {
        base.LoadModifiedStats(data);
        health.Value = maxHealth.Value;
        mana.Value = maxMana.Value;
    }
}
