using UnityEngine;

public class ADMelee : BaseChampion
{

    void Start()
    {
        base.Start();
        UpdateStats();
        AddAbilities();
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
        movementSpeed.Value = 11f;
        maxMana.Value = 232f;
        manaRegen.Value = 8f;
        abilityHaste.Value = 0f;
        critChance.Value = 0f;
        critDamage.Value = 1.75f; // 175% damage on crit

        autoAttack.setRange(50f); // Set the range of the auto attack ability
        health.Value = maxHealth.Value; // Initialize health to max health
        mana.Value = maxMana.Value; // Initialize mana to max mana
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

    public override GameObject empowerLogic(GameObject bullet)
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

    public override GameObject stackLogic(GameObject bullet)
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

    public override GameObject ability3Logic(GameObject bullet)
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

    public override void passiveAbility(){
        //Passive ability logic
        if (enemyChampion == null){
            Debug.LogWarning("No enemy champion assigned.");
            return;
        }

        // Get the direction to the enemy
        Vector3 directionToEnemy = (enemyChampion.transform.position - transform.position).normalized;

        // Get the player's movement direction
        Vector3 movementDirection = (PN.targetPosition - transform.position).normalized;

        // Check if the player is moving towards the enemy
        float dotProduct = Vector3.Dot(directionToEnemy, movementDirection);

        if (dotProduct > 0.5f){
            Debug.Log("Player is moving towards the enemy. Passive ability activated!");
            movementSpeed.Value = 12f;
        }
        else{
            Debug.Log("Player is not moving towards the enemy.");
            // Reset movement speed or remove the passive effect
            movementSpeed.Value = 11f; // Reset to default speed
        }
    }

    public override void UseAbility1()
    {
        // Check if the ability is off cooldown and if there is enough mana
        if (ability1.cooldownTimer == 0 && mana.Value >= ability1.manaCost){
            // Perform the Tumble action here
            Vector3 dashDirection = (PN.mousePosition - transform.position).normalized; // Get the direction to the mouse position
            dashDirection.z = 0; // Ensure the z coordinate is 0

            // Calculate the target position 3f away in the direction of the mouse
            Vector3 targetPosition = transform.position + dashDirection * 3f;

            // Move the player towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, (17 + movementSpeed.Value) * Time.deltaTime);

            // Set the cooldown timer for the ability
            ability1.cooldownTimer = ability1.cooldown;
            mana.Value -= ability1.manaCost; // Deduct mana cost

            Debug.Log("Tumble ability used. Player dashed towards the target position.");
        }
        else if (ability1.cooldownTimer > 0)
        {
            Debug.Log("Ability is on cooldown!");
            return;
        }
        else if (mana.Value < ability1.manaCost)
        {
            Debug.Log("Not enough mana!");
            return;
        }

        isEmpowered.Value = true; // Set the empowered state to true
        empowerStartTime = Time.time; // Record the time when the ability was used
        // Put messages up on screen if the ability is on cooldown or not enough mana??? Maybe

        
        // Empower next attack for 3.5 seconds
        // Add countdown timer for that empower attack time limit --> Done in Base Champion Update
        // Alter bullet prefab with a 'damage dealt' variable to be used in the bullet script that will be increased for the empowered dmg

        
    }

    public override void UseAbility2()
    {
        // No cooldown
        // No mana cost
        // In game manager, perhaps add a variable that can track these stacks and how many times it has been applied before dealing the true damage
        // Maybe in base character class? Add a variable that counts and checks.
        // Stack duration is 3 seconds before the stack is removed.
        // 6% of targets maximum health as bonus true damage on 3rd attack.

        // INSTEAD, track how many attacks, third always does more damage
        // Make Game Manager bulky if need be

        if (stackCount == 3){
            //Minimum damage is 50, max is 6% of target's max health
            maxStacks = true;
        }
    }

    public override void UseAbility3()
    {
        // Check if ability is off cooldown and if theres enough mana
        if (ability3.cooldownTimer == 0 && mana.Value >= ability3.manaCost)
        {
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

        ability3Used = true; // Set the ability used flag to true
        // Modify the bullet prefab to deal extra physical damage
        // Add a knockback effect to the target if they are hit by the bolt
    }
}
