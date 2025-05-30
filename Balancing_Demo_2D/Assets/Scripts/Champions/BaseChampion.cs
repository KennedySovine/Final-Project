using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class BaseChampion : NetworkBehaviour
{
    #region Variables
    public static GameManager GM; // Reference to the GameManager

    public AbilityStatsData AbilityStatsData;

    public ChampionData championData; // Reference to the ChampionData struct

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

    public bool isMaxStacks
    {
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

    public Dictionary<string, Ability> abilityDict = new Dictionary<string, Ability>(); // Dictionary to hold abilities by name

    public NetworkVariable<float> lastAutoAttackTime = new NetworkVariable<float>(0f); // Time of the last auto-attack

    [Header("Champion Settings")]
    public float regenTimer = 0f;
    public GameObject enemyChampion; // Reference to the enemy champion prefab
    public NetworkVariable<ulong> enemyChampionId = new NetworkVariable<ulong>(0); // ID of the enemy champion

    public GameObject bulletPrefab; // Prefab for the bullet to be fired

    public PlayerNetwork PN; // Reference to the PlayerNetwork script

    public bool iconsSet = false; // Flag to check if icons are set

    private bool gameEndStuff = false;
    #endregion

    #region Initialization
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
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GM = GameManager.Instance;
        IGUIM = GM.IGUIM;

        if (IsOwner)
        {
            health.OnValueChanged += (previousValue, newValue) =>
            {
                IGUIM.UpdateHealthSlider(previousValue, newValue);
            };

            mana.OnValueChanged += (previousValue, newValue) =>
            {
                IGUIM.UpdateManaSlider(previousValue, newValue);
            };

            maxHealth.OnValueChanged += (previousValue, newValue) =>
            {
                IGUIM.UpdateMaxHealthSlider(previousValue, newValue);
            };

            maxMana.OnValueChanged += (previousValue, newValue) =>
            {
                IGUIM.UpdateMaxManaSlider(previousValue, newValue);
            };
        }
    }

    new private void OnDestroy()
    {
        if (IsOwner)
        {
            health.OnValueChanged -= (previousValue, newValue) =>
            {
                IGUIM.UpdateHealthSlider(previousValue, newValue);
            };

            mana.OnValueChanged -= (previousValue, newValue) =>
            {
                IGUIM.UpdateManaSlider(previousValue, newValue);
            };

            maxHealth.OnValueChanged -= (previousValue, newValue) =>
            {
                IGUIM.UpdateMaxHealthSlider(previousValue, newValue);
            };

            maxMana.OnValueChanged -= (previousValue, newValue) =>
            {
                IGUIM.UpdateMaxManaSlider(previousValue, newValue);
            };
        }
    }

    public virtual ChampionData ForTheMainMenu()
    {
        Debug.Log("no go away");
        return ChampionData.FromChampion(this); // Convert the champion data to a struct for the main menu
    }
    #endregion

    #region RPC Methods
    [Rpc(SendTo.Server)]
    public virtual void PassiveAbilityRpc() { Debug.Log("No passive ability assigned"); }
    [Rpc(SendTo.Server)]
    public virtual void UseAbility1Rpc() { Debug.Log("No ability 1 assigned"); }
    [Rpc(SendTo.Server)]
    public virtual void UseAbility2Rpc() { Debug.Log("No ability 2 assigned"); }
    [Rpc(SendTo.Server)]
    public virtual void UseAbility3Rpc() { Debug.Log("No ability 3 assigned"); }

    public virtual GameObject EmpowerLogic(GameObject bullet) { Debug.Log("No empower logic assigned"); return bullet; }
    public virtual GameObject StackLogic(GameObject bullet) { Debug.Log("No stack logic assigned"); return bullet; }
    public virtual GameObject Ability3Logic(GameObject bullet) { Debug.Log("No stack logic assigned"); return bullet; }

    public virtual void StackManager() { Debug.Log("No stack manager assigned"); }
    public virtual void LoadModifiedStats(ChampionData data)
    {
        championType = data.championType;
        maxHealth.Value = data.maxHealth;
        healthRegen.Value = data.healthRegen;
        AD.Value = data.AD;
        AP.Value = data.AP;
        armor.Value = data.armor;
        magicResist.Value = data.magicResist;
        attackSpeed.Value = data.attackSpeed;
        movementSpeed.Value = data.movementSpeed;
        maxMana.Value = data.maxMana;
        manaRegen.Value = data.manaRegen;
        abilityHaste.Value = data.abilityHaste;
        critChance.Value = data.critChance;
        critDamage.Value = data.critDamage;
        armorPen.Value = data.armorPen;
        magicPen.Value = data.magicPen;
    }
    #endregion

    #region Update Logic
    public virtual void Update()
    {
        if (!gameEndStuff && GM.gameTime.Value <= 0f)
        {
            gameEndStuff = true; // Set the flag to true to prevent multiple calls
            SubmitFinalAbilityStatsServerRpc(
                AbilityStatsData.FromAbilityStats(ability1.Stats),
                AbilityStatsData.FromAbilityStats(ability2.Stats),
                AbilityStatsData.FromAbilityStats(ability3.Stats),
                AbilityStatsData.FromAbilityStats(passive.Stats)
            );
        }

        if (passive != null)
        {
            PassiveAbilityRpc(); // Call the passive ability logic
        }

        if (IsOwner)
        {
            AbilityIconCooldownManaChecks(); // Check cooldowns and mana for abilities
        }

        if (!IsServer) return; // Only the server should execute this logic
        //Timer for stacks
        HealthandManaRegen();

        if (isEmpowered.Value)
        {
            if (Time.time > empowerStartTime.Value + empowerDuration.Value)
            {
                UpdateIsEmpoweredRpc(false); // Reset the empowered state
            }
        }

        if (slowAmount.Value > 0f && (Time.time > slowStartTime.Value + slowDuration.Value)) // If the slow timer is up
        {
            slowAmount.Value = 0f; // Reset the slow amount
            slowStartTime.Value = 0f; // Reset the slow start time
            ApplySlowRpc(0f, 0f); // Reset the slow effect on the client
        }


        StackManager(); // Call the stack manager logic

    }
    #endregion

    #region UI Management
    public virtual void AbilityIconCooldownManaChecks()
    {
        if (IsOwner && iconsSet) // Only the owner should check cooldowns and mana
        {
            // Check ability 1
            if (ability1 != null && !ability1.isOnCooldown && mana.Value >= ability1.manaCost)
            {
                IGUIM.ButtonInteractable("Q", true); // Enable the button if ability 1 is available
            }
            else
            {
                IGUIM.ButtonInteractable("Q", false); // Disable the button if ability 1 is on cooldown or not enough mana
            }

            // Check ability 2
            if (ability2 != null && !ability2.isOnCooldown && mana.Value >= ability2.manaCost)
            {
                IGUIM.ButtonInteractable("W", true); // Enable the button if ability 2 is available
            }
            else
            {
                IGUIM.ButtonInteractable("W", false); // Disable the button if ability 2 is on cooldown or not enough mana
            }

            // Check ability 3
            if (ability3 != null && !ability3.isOnCooldown && mana.Value >= ability3.manaCost)
            {
                IGUIM.ButtonInteractable("E", true); // Enable the button if ability 3 is available
            }
            else
            {
                IGUIM.ButtonInteractable("E", false); // Disable the button if ability 3 is on cooldown or not enough mana
            }
        }
    }

    public void SendToUI()
    {
        if (!IsOwner) return;
        while (!iconsSet)
        {
            if (abilityDict.ContainsKey("E")) // Check if the abilities are not null and icons are not set
            {
                Debug.Log("Setting abilities to buttons for player " + NetworkManager.Singleton.LocalClientId);
                IGUIM.SetAbilityToButtons(abilityDict); // Set the abilities to the buttons in the UI
                iconsSet = true; // Set the flag to true to indicate icons are set
            }
            else
            {
                Debug.LogWarning("Ability dictionary is empty or abilities are not set yet.");
                return; // Exit the loop if the abilities are not set
            }
        }
    }
    #endregion

    #region Health and Mana Management
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
    #endregion

    #region Enemy Management
    public void GetEnemyChampion(ulong enemyId)
    {
        enemyChampion = NetworkManager.Singleton.SpawnManager.SpawnedObjects[enemyId].gameObject; // Get the enemy champion object
    }

    public virtual GameObject CritLogic(GameObject bullet)
    {
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

    public void LogAbilityUsedRpc(Ability ability)
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

    //Also will track consecutive attacks based if the dmg type is AD or AP
    public void TakeDamage(float AD, float AP, float armorPen, float magicPen)
    {
        if (!IsServer) return;

        // Calculate and apply damage
        float damage = 0f;
        damage += AD / (1 + (armor.Value - armorPen) / 100); // Calculate damage with armor penetration
        damage += AP / (1 + (magicResist.Value - magicPen) / 100); // Calculate damage with magic penetration

        UpdateHealthRpc(-damage); // Update health with the calculated damage
        Debug.Log($"Damage taken: {damage} (AD: {AD}, AP: {AP}, Armor Pen: {armorPen}, Magic Pen: {magicPen})");

        // Assign damage to the ability stats of the enemy
        switch (enemyChampionId.Value)
        {
            case var id when id == GM.player1ID:
                AssignAbilityDamage(passive, damage);
                break;

            case var id when id == GM.player2ID:
                AssignAbilityDamage(passive, damage);
                break;

            default:
                Debug.LogError($"No valid player ID found for enemyChampionId: {enemyChampionId.Value}");
                break;
        }
    }

    private void AssignAbilityDamage(Ability ability, float damage)
    {
        if (ability != null)
        {
            if (ability.Stats == null)
            {
                ability.Stats = new AbilityStats();
            }
            ability.Stats.damage = damage;
            Debug.Log($"Assigned damage: {damage} to ability: {ability.name}");
        }
        else
        {
            Debug.LogError("Ability used is null.");
        }
    }
    #endregion

    #region Slow Management
    [Rpc(SendTo.Server)]
    public void ApplySlowRpc(float slowAmount, float duration)
    {
        if (!IsServer) return; // Only the server should apply the slow
        if (slowAmount == 0f)
        {
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
    #endregion

    #region Stat Updates
    [Rpc(SendTo.Server)]
    public void UpdateMaxHealthRpc(float healthChange)
    {
        if (!IsServer) return; // Ensure this is only executed on the server

        if (healthChange < 1 && healthChange > 0) // If the health change is due to augment that will add %
        {
            float tempH = maxHealth.Value * healthChange;
            maxHealth.Value += tempH;
            health.Value += tempH; // Update the health value
            return;
        }
        maxHealth.Value += healthChange;
        health.Value += healthChange; // Update the health value
    }

    [Rpc(SendTo.Server)]
    public void UpdateHealthRpc(float healthChange)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        health.Value += healthChange; // Update the health value
        if (health.Value > maxHealth.Value) // Ensure health does not exceed maxHealth
        {
            health.Value = maxHealth.Value;
        }
    }
    [Rpc(SendTo.Server)]
    public void UpdateADRpc(float adChange)
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
    public void UpdateAPRpc(float apChange)
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
    public void UpdateArmorRpc(float armorChange)
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
    public void UpdateMagicResistRpc(float magicResistChange)
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
    public void UpdateAttackSpeedRpc(float attackSpeedChange)
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
    public void UpdateMovementSpeedRpc(float movementSpeedChange)
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
    public void UpdateMaxManaRpc(float manaChange)
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
    public void UpdateManaRpc(float manaChange)
    {
        if (!IsServer) return;

        mana.Value += manaChange; // Update the mana value
        if (mana.Value > maxMana.Value) // Ensure mana does not exceed maxMana
        {
            mana.Value += maxMana.Value;
        }
    }

    [Rpc(SendTo.Server)]
    public void UpdateManaRegenRpc(float manaRegenChange)
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
    public void UpdateAbilityHasteRpc(float abilityHasteChange)
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
        ability1.SetCooldown(ability1.cooldown * (1 - abilityHaste.Value / 100)); // Update the cooldown of ability 1 based on ability haste
        ability2.SetCooldown(ability2.cooldown * (1 - abilityHaste.Value / 100)); // Update the cooldown of ability 2 based on ability haste
        ability3.SetCooldown(ability3.cooldown * (1 - abilityHaste.Value / 100)); // Update the cooldown of ability 3 based on ability haste
    }
    [Rpc(SendTo.Server)]
    public void UpdateCritChanceRpc(float critChanceChange)
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
    public void UpdateCritDamageRpc(float critDamageChange)
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
    public void UpdateArmorPenRpc(float armorPenChange)
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
    public void UpdateMagicPenRpc(float magicPenChange)
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
    public void UpdateStackCountRpc(int change, int current, int max)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        if (change == 0)
        {
            if (current > max)
            {
                stackCount.Value = max; // Set the stack count to max
            }
            return; // No change to the stack count
        }
        else if (change < 0)
        {
            stackCount.Value += change; // Decrease the stack count
            stackStartTime.Value = Time.time; // Reset the stack start time
            if (stackCount.Value < 0)
            {
                stackCount.Value = 0; // Ensure the stack count does not go below zero
            }
            return;
        }
        else if (current + change >= max)
        {
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
    public void UpdateAbility3UsedRpc(bool value)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        ability3Used.Value = value;
        ability3.timeOfCast = Time.time; // Record the time when the ability was used
    }
    [Rpc(SendTo.Server)]
    public void UpdateRapidFireRpc(int value)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        rapidFire.Value = value;
    }
    [Rpc(SendTo.Server)]
    public void UpdateIsEmpoweredRpc(bool value)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        isEmpowered.Value = value;

    }

    [Rpc(SendTo.Server)]
    public void UpdateSlowAmountRpc(float value)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        slowAmount.Value = value;
    }

    [Rpc(SendTo.Server)]
    public void ResetStackCountRpc()
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        stackCount.Value = 0; // Reset the stack count
    }
    #endregion

    #region Final Ability Stats
    [Rpc(SendTo.Server)]
    public void SubmitFinalAbilityStatsServerRpc(AbilityStatsData ability1, AbilityStatsData ability2, AbilityStatsData ability3, AbilityStatsData passive)
    {
        if (!IsServer) return; // Ensure this is only executed on the server

        var champ = GetComponent<BaseChampion>();

        ability1.ApplyTo(champ.ability1.Stats);
        ability2.ApplyTo(champ.ability2.Stats);
        ability3.ApplyTo(champ.ability3.Stats);
        passive.ApplyTo(champ.passive.Stats);

        GM.recievedCalcs++; // Increment the count of received stats

        Debug.Log($"Received stats from client {OwnerClientId}");
    }

    [Rpc(SendTo.Everyone)]
    public void SetAbilityTimeOfCastRpc(string abilityKey, float castTime)
    {
        Debug.Log($"Setting time of cast for ability {abilityKey} to {castTime}");
        if (abilityKey == "Q")
        {
            ability1.timeOfCast = castTime;
        }
        else if (abilityKey == "W")
        {
            ability2.timeOfCast = castTime;
        }
        else if (abilityKey == "E")
        {
            ability3.timeOfCast = castTime;
        }
    }
    #endregion

    #region Champion Data Assignment
    public void LoadData(ChampionData data)
    {
        championType = data.championType;
        maxHealth.Value = data.maxHealth;
        healthRegen.Value = data.healthRegen;
        AD.Value = data.AD;
        AP.Value = data.AP;
        armor.Value = data.armor;
        magicResist.Value = data.magicResist;
        attackSpeed.Value = data.attackSpeed;
        movementSpeed.Value = data.movementSpeed;
        maxMana.Value = data.maxMana;
        manaRegen.Value = data.manaRegen;
        abilityHaste.Value = data.abilityHaste;
        critChance.Value = data.critChance;
        critDamage.Value = data.critDamage;
        armorPen.Value = data.armorPen;
        magicPen.Value = data.magicPen;
        missileSpeed.Value = data.missileSpeed;

        // Set other fields as needed
    }
    #endregion
}
