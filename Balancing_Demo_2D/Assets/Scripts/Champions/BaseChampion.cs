using UnityEngine;
using Unity.Netcode;

public class BaseChampion : NetworkBehaviour
{
    public static GameManager GM; // Reference to the GameManager

    public InGameUIManager IGUIM; // Reference to the InGameUIManager
    [Header("Champion Stats")]
    public string championType = "";

    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(600f);
    public NetworkVariable<float> healthRegen = new NetworkVariable<float>(5f);
    public NetworkVariable<float> AD = new NetworkVariable<float>(60f);
    public NetworkVariable<float> AP = new NetworkVariable<float>(0f);
    public NetworkVariable<float> armor = new NetworkVariable<float>(25f);
    public NetworkVariable<float> magicResist = new NetworkVariable<float>(30f);
    public NetworkVariable<float> attackSpeed = new NetworkVariable<float>(0.65f);
    public NetworkVariable<float> movementSpeed = new NetworkVariable<float>(10f); //300 originally (original, / 3 - extra 0 )
    public NetworkVariable<float> maxMana = new NetworkVariable<float>(300f);
    public NetworkVariable<float> manaRegen = new NetworkVariable<float>(7f);
    public NetworkVariable<float> abilityHaste = new NetworkVariable<float>(0f);
    public NetworkVariable<float> critChance = new NetworkVariable<float>(0f);
    public NetworkVariable<float> critDamage = new NetworkVariable<float>(1.75f); // 175% damage on crit
    public NetworkVariable<float> armorPen = new NetworkVariable<float>(0f);
    public NetworkVariable<float> magicPen = new NetworkVariable<float>(0f);
    public NetworkVariable<float> missileSpeed = new NetworkVariable<float>(33f); // Health percentage for abilities

    public NetworkVariable<Vector3> currentPosition = new NetworkVariable<Vector3>(Vector3.zero);
    public NetworkVariable<float> slowAmount = new NetworkVariable<float>(0f); // Slow amount for abilities
    public NetworkVariable<float> slowDuration = new NetworkVariable<float>(0f); // Duration for the slow effect
    public NetworkVariable<float> slowStartTime = new NetworkVariable<float>(0f); // Time when the slow effect started

    [Header("Champion Ability Modifiers")]
    public NetworkVariable<bool> isEmpowered = new NetworkVariable<bool>(false); // Flag to check if the next attack is empowered
    public NetworkVariable<float> empowerStartTime = new NetworkVariable<float>(0f); // Time when the empowered state started
    public NetworkVariable<float> empowerDuration = new NetworkVariable<float>(3.5f); // Duration for the empowered state
    public NetworkVariable<int> stackCount = new NetworkVariable<int>(0); // Number of stacks for stacking abilities
    public NetworkVariable<float> stackStartTime = new NetworkVariable<float>(0f); // Time when the stack started
    public NetworkVariable<float> stackDuration = new NetworkVariable<float>(3.5f); // Duration for the stacks to last
    public NetworkVariable<int> maxStacks = new NetworkVariable<int>(10); // Maximum number of stacks for abilities
    public NetworkVariable<bool> ability3Used = new NetworkVariable<bool>(false); // Flag to check if ability 3 has been used
    public NetworkVariable<int> rapidFire = new NetworkVariable<int>(1);

    public bool isMaxStacks {
        get { return maxStacks.Value == stackCount.Value; } // Check if the current stack count is equal to the maximum stack count
    }

    [Header("Champion Resources")]
    public NetworkVariable<float> health = new NetworkVariable<float>(600f);
    public NetworkVariable<float> mana = new NetworkVariable<float>(300f);

    [Header("Champion Abilities")]
    public Ability autoAttack = new Ability("Auto Attack", "Basic attack", 0f, 0f, 5f);
    public Ability passive;
    public Ability ability1;
    public Ability ability2;
    public Ability ability3;

    public NetworkVariable<float> lastAutoAttackTime = new NetworkVariable<float>(0f); // Time of the last auto-attack

    [Header("Champion Settings")]
    public float regenTimer = 0f;

    public GameObject enemyChampion; // Reference to the enemy champion prefab
    public NetworkVariable<ulong> enemyChampionId = new NetworkVariable<ulong>(0); // ID of the enemy champion

    public GameObject bulletPrefab; // Prefab for the bullet to be fired

    public PlayerNetwork PN; // Reference to the PlayerNetwork script

    private bool iconsSet = false; // Flag to check if icons are set

    public void Start()
    {
        GM = GameManager.Instance; // Get the instance of the GameManager
        IGUIM = GM.IGUIM; // Get the instance of the InGameUIManager

        if (GM == null)
        {
            Debug.LogError("GameManager instance is null. Ensure the GameManager is active in the scene.");
        }
        if (IGUIM == null)
        {
            Debug.LogError("InGameUIManager instance is null. Ensure the InGameUIManager is active in the scene.");
        }

        // Subscribe to health and mana changes only for the owner
        if (IsOwner)
        {
            health.OnValueChanged += (previousValue, newValue) =>
            {
                //Debug.Log($"Client {NetworkManager.Singleton.LocalClientId}: Health changed from {previousValue} to {newValue}");
                IGUIM.UpdateHealthSlider(previousValue, newValue);
            };

            mana.OnValueChanged += (previousValue, newValue) =>
            {
                //Debug.Log($"Client {NetworkManager.Singleton.LocalClientId}: Mana changed from {previousValue} to {newValue}");
                IGUIM.UpdateManaSlider(previousValue, newValue);
            };

            maxHealth.OnValueChanged += (previousValue, newValue) =>
            {
                //Debug.Log($"Client {NetworkManager.Singleton.LocalClientId}: Max Health changed from {previousValue} to {newValue}");
                IGUIM.UpdateMaxHealthSlider(previousValue, newValue); // Update the health slider when max health changes
            };

            maxMana.OnValueChanged += (previousValue, newValue) =>
            {
                //Debug.Log($"Client {NetworkManager.Singleton.LocalClientId}: Max Mana changed from {previousValue} to {newValue}");
                IGUIM.UpdateMaxManaSlider(previousValue, newValue); // Update the mana slider when max mana changes
            };
        }
    }

    [Rpc(SendTo.Server)]
    public virtual void passiveAbilityRpc(){ Debug.Log("No passive ability assigned");}
    [Rpc(SendTo.Server)]
    public virtual void UseAbility1Rpc(){ Debug.Log("No ability 1 assigned");}
    [Rpc(SendTo.Server)]
    public virtual void UseAbility2Rpc(){ Debug.Log("No ability 2 assigned");}
    [Rpc(SendTo.Server)]
    public virtual void UseAbility3Rpc(){ Debug.Log("No ability 3 assigned");}

    public virtual GameObject empowerLogic(GameObject bullet){ Debug.Log("No empower logic assigned"); return bullet;}
    public virtual GameObject stackLogic(GameObject bullet){ Debug.Log("No stack logic assigned"); return bullet;}
    public virtual GameObject ability3Logic(GameObject bullet){ Debug.Log("No stack logic assigned"); return bullet;}

    public virtual void stackManager(){ Debug.Log("No stack manager assigned");}
    
    public virtual void Update()
    {
        if (IsServer) // Only the server should modify NetworkVariables
        {
            HealthandManaRegen();
        }

        if (passive != null)
        {
            passiveAbilityRpc(); // Call the passive ability logic
        }

        if (!IsServer) return; // Only the server should execute this logic
        //Timer for stacks

        if (isEmpowered.Value){
            if (Time.time > empowerStartTime.Value + empowerDuration.Value)
            {
                updateIsEmpoweredRpc(false); // Reset the empowered state
            }
        }

        if (slowAmount.Value > 0f && (Time.time > slowStartTime.Value + slowDuration.Value)) // If the slow timer is up
        {
            slowAmount.Value = 0f; // Reset the slow amount
            slowStartTime.Value = 0f; // Reset the slow start time
            applySlowRpc(0f, 0f); // Reset the slow effect on the client
        }

        if (!IsOwner) return; // Only the owner should execute this logic
        if (GM.playersSpawned.Value && !iconsSet){ // Check if players are spawned and icons are not set
            Debug.Log("Setting abilities to buttons for player " + NetworkManager.Singleton.LocalClientId);
            IGUIM.abilityDict.Add("Q", ability1); // Add ability 1 to the button dictionary
            IGUIM.abilityDict.Add("W", ability2); // Add ability 2 to the button dictionary
            IGUIM.abilityDict.Add("E", ability3); // Add ability 3 to the button dictionary
            IGUIM.setAbilityToButtons(); // Set the abilities to the buttons in the UI
            iconsSet = true; // Set the flag to true to indicate icons are set
        }

        abilityIconCooldownManaChecks(); // Check cooldowns and mana for abilities

        stackManager(); // Call the stack manager logic

    }

    public virtual void abilityIconCooldownManaChecks()
    {
        // The first or statement in the set true is to check if the ability is a passive as passives dont have a cooldown
        if (IsOwner) // Only the owner should check cooldowns and mana
        {
            if ((ability1.cooldown == 0) || (ability1 != null && !(ability1.isOnCooldown) && ability1.manaCost <= mana.Value)) // Check if ability 1 is not on cooldown and enough mana is available
            {
                IGUIM.buttonInteractable("Q", true);
            }
            else if (ability1 == null || ability1.isOnCooldown || ability1.manaCost > mana.Value) // Check if ability 1 is on cooldown or not enough mana is available
            {
                IGUIM.buttonInteractable("Q", false); // Disable the button if ability 1 is on cooldown or not enough mana
            }
            if ((ability2.cooldown == 0) || (ability2 != null && !(ability2.isOnCooldown) && ability2.manaCost <= mana.Value)) // Check if ability 1 is not on cooldown and enough mana is available
            {
                IGUIM.buttonInteractable("W", true);
            }
            else if (ability2 == null || ability2.isOnCooldown || ability2.manaCost > mana.Value) // Check if ability 1 is not on cooldown and enough mana is available
            {
                IGUIM.buttonInteractable("W", false); // Disable the button if ability 1 is on cooldown or not enough mana
            }
            if ((ability3.cooldown == 0) || (ability3 != null && !(ability3.isOnCooldown) && ability3.manaCost <= mana.Value)) // Check if ability 1 is not on cooldown and enough mana is available
            {
                IGUIM.buttonInteractable("E", true);
            }
            else if (ability3 == null || ability3.isOnCooldown || ability3.manaCost > mana.Value) // Check if ability 1 is not on cooldown and enough mana is available
            {
                IGUIM.buttonInteractable("E", false); // Disable the button if ability 1 is on cooldown or not enough mana
            }
            else{
                return; // No ability is available for use
            }
            
        }
    }

    private void HealthandManaRegen()
    {
        // Health and mana regen logic
        regenTimer += Time.deltaTime;
        if (regenTimer >= 1f)
        {
            regenTimer = 0f; // Reset the timer
            // Regenerate health and mana
            if (health.Value < maxHealth.Value)
            {
                health.Value = Mathf.Min(health.Value + healthRegen.Value, maxHealth.Value); // Ensure health does not exceed maxHealth
                //Debug.Log($"Regenerating health: {healthRegen.Value}");
            }
            if (mana.Value < maxMana.Value)
            {
                mana.Value = Mathf.Min(mana.Value + manaRegen.Value, maxMana.Value); // Ensure mana does not exceed maxMana
                //Debug.Log($"Regenerating mana: {manaRegen.Value}");
            }
        }
    }

    public void getEnemyChampion(ulong enemyId)
    {
        enemyChampion = NetworkManager.Singleton.SpawnManager.SpawnedObjects[enemyId].gameObject; // Get the enemy champion object
    }

    public virtual GameObject critLogic(GameObject bullet){
        float chance = Random.Range(0f, 1f);
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (chance <= critChance.Value) // If the random chance is less than or equal to critChance
        {
            Debug.Log("Critical hit! Damage multiplied by " + critDamage.Value);
            bulletComponent.ADDamage = AD.Value * critDamage.Value; // Multiply the damage by critDamage
        }
        else
        {
            Debug.Log("Normal hit. No critical damage.");
        }

        return bullet;
    }

    public void logAbilityUsedRpc(Ability ability)
    {
        if (NetworkManager.Singleton.LocalClientId == GM.player1ID)
        {
            GM.player1AbilityUsed = ability; // Log the ability used for player 1
        }
        else if (NetworkManager.Singleton.LocalClientId == GM.player2ID)
        {
            GM.player2AbilityUsed = ability; // Log the ability used for player 2
        }
    }

    public Ability getAbilityUsedRpc(){
        if (NetworkManager.Singleton.LocalClientId == GM.player1ID){
            return GM.player1AbilityUsed; // Get the ability used for player 1
        }
        else if (NetworkManager.Singleton.LocalClientId == GM.player2ID)
        {
            return GM.player2AbilityUsed; // Get the ability used for player 2
        }
        else{
            Debug.LogError("No player ID found for the current client.");
            return null; // Return null if no player ID is found
        }
    }

    //Also will track consecutive attacks based if the dmg type is AD or AP
    public void TakeDamage(float AD, float AP, float armorPen, float magicPen){
        if (!IsServer) return;

        float damage = 0f;
 
        damage += AD / (1 + (armor.Value - armorPen) / 100); // Calculate damage with armor penetration

        damage += AP / (1 + (magicResist.Value - magicPen) / 100); // Calculate damage with magic penetration

        updateHealthRpc(-damage); // Update health with the calculated damage
        Debug.Log("Damage taken: " + damage + " (AD: " + AD + ", AP: " + AP + ", Armor Pen: " + armorPen + ", Magic Pen: " + magicPen + ")");

    }

    [Rpc(SendTo.Server)]
    public void applySlowRpc(float slowAmount, float duration)
    {
        if (!IsServer) return; // Only the server should apply the slow
        if (slowAmount == 0f){
            slowAmount = 0f; // Ignore if the slow amount is zero or negative
            slowStartTime.Value = 0f; // Reset the slow start time
            slowDuration.Value = 0f; // Reset the slow duration
            return;
        } // Ignore if the slow amount is zero or negative
        Debug.Log("Applying slow effect: " + slowAmount + " for " + duration + " seconds.");
        this.slowAmount.Value = slowAmount; // Set the slow amount
        slowDuration.Value = duration; // Set the slow duration
        slowStartTime.Value = Time.time; // Set the start time for the slow effect
    }

    [Rpc(SendTo.Server)]
    public void updateMaxHealthRpc(float healthChange)
    {
        if (!IsServer) return; // Ensure this is only executed on the server

        if (healthChange < 1 && healthChange > 0) // If the health change is due to augment that will add %
        {
            float tempH = maxHealth.Value * healthChange;
            maxHealth.Value += tempH;
            return;
        }
        maxHealth.Value += healthChange;
    }

    [Rpc(SendTo.Server)]
    public void updateHealthRpc(float healthChange)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        health.Value += healthChange; // Update the health value
        if (health.Value > maxHealth.Value) // Ensure health does not exceed maxHealth
        {
            health.Value = maxHealth.Value;
        }
    }
    [Rpc(SendTo.Server)]
    public void updateADRpc(float adChange)
    {
        if (!IsServer) return;

        if (adChange < 1 && adChange > 0) // If the AD change is due to augment that will add %
        {
            float tempAD = AD.Value * adChange;
            AD.Value += tempAD;
            return;
        }
        AD.Value += adChange;
    }
    [Rpc(SendTo.Server)]
    public void updateAPRpc(float apChange)
    {
        if (!IsServer) return;

        if (apChange < 1 && apChange > 0) // If the AP change is due to augment that will add %
        {
            float tempAP = AP.Value * apChange;
            AP.Value += tempAP;
            return;
        }
        AP.Value += apChange;
    }
    [Rpc(SendTo.Server)]
    public void updateArmorRpc(float armorChange)
    {
        if (!IsServer) return;

        if (armorChange < 1 && armorChange > 0) // If the armor change is due to augment that will add %
        {
            float tempA = armor.Value * armorChange;
            armor.Value += tempA;
            return;
        }
        armor.Value += armorChange;
    }
    [Rpc(SendTo.Server)]
    public void updateMagicResistRpc(float magicResistChange)
    {
        if (!IsServer) return;

        if (magicResistChange < 1 && magicResistChange > 0) // If the magic resist change is due to augment that will add %
        {
            float tempMR = magicResist.Value * magicResistChange;
            magicResist.Value += tempMR;
            return;
        }
        magicResist.Value += magicResistChange;
    }
    [Rpc(SendTo.Server)]
    public void updateAttackSpeedRpc(float attackSpeedChange)
    {
        if (!IsServer) return;

        if (attackSpeedChange < 1 && attackSpeedChange > 0) // If the attack speed change is due to augment that will add %
        {
            float tempAS = attackSpeed.Value * attackSpeedChange;
            attackSpeed.Value += tempAS;
            return;
        }
        attackSpeed.Value += attackSpeedChange;
    }
    [Rpc(SendTo.Server)]
    public void updateMovementSpeedRpc(float movementSpeedChange)
    {
        if (!IsServer) return;

        if (movementSpeedChange < 1 && movementSpeedChange > 0) // If the movement speed change is due to augment that will add %
        {
            float tempMS = movementSpeed.Value * movementSpeedChange;
            movementSpeed.Value += tempMS;
            return;
        }
        movementSpeed.Value += movementSpeedChange;
    }
    [Rpc(SendTo.Server)]
    public void updateMaxManaRpc(float manaChange)
    {
        if (!IsServer) return;

        if (manaChange < 1 && manaChange > 0) // If the mana change is due to augment that will add %
        {
            float tempM = maxMana.Value * manaChange;
            maxMana.Value += tempM;
            return;
        }
        maxMana.Value += manaChange;
    }
    [Rpc(SendTo.Server)]
    public void updateManaRpc(float manaChange)
    {
        if (!IsServer) return;

        mana.Value += manaChange; // Update the mana value
        if (mana.Value > maxMana.Value) // Ensure mana does not exceed maxMana
        {
            mana.Value += maxMana.Value;
        }
    }
    [Rpc(SendTo.Server)]
    public void updateManaRegenRpc(float manaRegenChange)
    {
        if (!IsServer) return;

        if (manaRegenChange < 1 && manaRegenChange > 0) // If the mana regen change is due to augment that will add %
        {
            float tempMR = manaRegen.Value * manaRegenChange;
            manaRegen.Value += tempMR;
            return;
        }
        manaRegen.Value += manaRegenChange;
    }
    [Rpc(SendTo.Server)]
    public void updateAbilityHasteRpc(float abilityHasteChange)
    {
        if (!IsServer) return;

        if (abilityHasteChange < 1 && abilityHasteChange > 0) // If the ability haste change is due to augment that will add %
        {
            float tempAH = abilityHaste.Value * abilityHasteChange;
            abilityHaste.Value += tempAH;
        }
        else
        {
            abilityHaste.Value += abilityHasteChange;
        }
        ability1.setCooldown(ability1.cooldown * (1 - abilityHaste.Value / 100)); // Update the cooldown of ability 1 based on ability haste
        ability2.setCooldown(ability2.cooldown * (1 - abilityHaste.Value / 100)); // Update the cooldown of ability 2 based on ability haste
        ability3.setCooldown(ability3.cooldown * (1 - abilityHaste.Value / 100)); // Update the cooldown of ability 3 based on ability haste
    }
    [Rpc(SendTo.Server)]
    public void updateCritChanceRpc(float critChanceChange)
    {
        if (!IsServer) return;

        if (critChanceChange < 1 && critChanceChange > 0) // If the crit chance change is due to augment that will add %
        {
            float tempCC = critChance.Value * critChanceChange;
            critChance.Value += tempCC;
            return;
        }
        critChance.Value += critChanceChange;
    }
    [Rpc(SendTo.Server)]
    public void updateCritDamageRpc(float critDamageChange)
    {
        if (!IsServer) return;

        if (critDamageChange < 1 && critDamageChange > 0) // If the crit damage change is due to augment that will add %
        {
            float tempCD = critDamage.Value * critDamageChange;
            critDamage.Value += tempCD;
            return;
        }
        critDamage.Value += critDamageChange;
    }
    [Rpc(SendTo.Server)]
    public void updateArmorPenRpc(float armorPenChange)
    {
        if (!IsServer) return;

        if (armorPenChange < 1 && armorPenChange > 0) // If the armor pen change is due to augment that will add %
        {
            float tempAP = armorPen.Value * armorPenChange;
            armorPen.Value += tempAP;
            return;
        }
        armorPen.Value += armorPenChange;
    }
    [Rpc(SendTo.Server)]
    public void updateMagicPenRpc(float magicPenChange)
    {
        if (!IsServer) return;

        if (magicPenChange < 1 && magicPenChange > 0) // If the magic pen change is due to augment that will add %
        {
            float tempMP = magicPen.Value * magicPenChange;
            magicPen.Value += tempMP;
            return;
        }
        magicPen.Value += magicPenChange;
    }

    [Rpc(SendTo.Server)]
    public void updateStackCountRpc(int change, int current, int max)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        if (change == 0){
            if (current > max){
                stackCount.Value = max; // Set the stack count to max
            }
            return; // No change to the stack count
        }
        else if (change < 0){
            stackCount.Value += change; // Decrease the stack count
            stackStartTime.Value = Time.time; // Reset the stack start time
            if (stackCount.Value < 0){
                stackCount.Value = 0; // Ensure the stack count does not go below zero
            }
            return;
        }
        else if (current + change >= max){
            Debug.Log("Max stacks reached: " + max);
            stackCount.Value = max; // Set the stack count to max
            stackStartTime.Value = Time.time;
            return;
        }
 
        Debug.Log("Adding stacks: " + change);
        stackCount.Value += change;
        stackStartTime.Value = Time.time; // Reset the stack start time
    }
    [Rpc(SendTo.Server)]
    public void updateAbility3UsedRpc(bool value)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        ability3Used.Value = value;
        ability3.timeOfCast = Time.time; // Record the time when the ability was used
    }
    [Rpc(SendTo.Server)]
    public void updateRapidFireRpc(int value)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        rapidFire.Value = value;
    }
    [Rpc(SendTo.Server)]
    public void updateIsEmpoweredRpc(bool value)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        isEmpowered.Value = value;
        
    }

    [Rpc(SendTo.Server)]
    public void updateSlowAmountRpc(float value)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        slowAmount.Value = value;
    }

    [Rpc(SendTo.Server)]
    public void resetStackCountRpc()
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        stackCount.Value = 0; // Reset the stack count
    }
}
