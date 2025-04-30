using UnityEngine;
using Unity.Netcode;

public class BaseChampion : NetworkBehaviour
{
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
    public NetworkVariable<bool> maxStacks = new NetworkVariable<bool>(false); // Flag to check if max stacks are reached
    public NetworkVariable<bool> ability3Used = new NetworkVariable<bool>(false); // Flag to check if ability 3 has been used
    public NetworkVariable<int> rapidFire = new NetworkVariable<int>(1);

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

    public void Start()
    {
  
    }

    [Rpc(SendTo.Server)]
    public virtual void passiveAbilityRpc(){ Debug.Log("No passive ability assigned");}
    [Rpc(SendTo.Server)]
    public virtual void UseAbility1Rpc(){ Debug.Log("No ability 1 assigned");}
    [Rpc(SendTo.Server)]
    public virtual void UseAbility2Rpc(){ Debug.Log("No ability 2 assigned");}
    [Rpc(SendTo.Server)]
    public virtual void UseAbility3Rpc(){ Debug.Log("No ability 3 assigned");}

    public virtual GameObject empowerLogic(GameObject bullet){ Debug.Log("No empower logic assigned"); return null;}
    public virtual GameObject stackLogic(GameObject bullet){ Debug.Log("No stack logic assigned"); return null;}
    public virtual GameObject ability3Logic(GameObject bullet){ Debug.Log("No stack logic assigned"); return null;}
    
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

    //Also will track consecutive attacks based if the dmg type is AD or AP
    public void TakeDamage(float AD, float AP){
        if (IsServer)
        {
            // Calculate damage based on armor and magic resist
            float damage = 0f;
            if (AD > 0){
                damage = AD / (1 + (armor.Value / 100)); // Physical damage calculation
            }
            if (AP > 0){
                damage += AP / (1 + (magicResist.Value / 100)); // Magic damage calculation
            }
            

            updateHealthRpc(-damage); // Update health with negative damage value

            if (health.Value <= 0)
            {
                Debug.Log("Champion has died!");
                //Die(); // Call the die function if health is 0 or less
            }
        }
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
        if (IsServer)
        {
            if (healthChange < 0 && healthChange > -1) // If the health change is due to augment that will add %
            {
                float tempH = maxHealth.Value * healthChange;
                maxHealth.Value += tempH;
            }
            else
            {
                maxHealth.Value += healthChange;
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void updateHealthRpc(float healthChange)
    {
        if (IsServer)
        {
            health.Value += healthChange;
        }
    }
    [Rpc(SendTo.Server)]
    public void updateADRpc(float adChange)
    {
        if (IsServer)
        {
            if (adChange < 0 && adChange > -1) // If the AD change is due to augment that will add %
            {
                float tempAD = AD.Value * adChange;
                AD.Value += tempAD;
            }
            else
            {
                AD.Value += adChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateAPRpc(float apChange)
    {
        if (IsServer)
        {
            if (apChange < 0 && apChange > -1) // If the AP change is due to augment that will add %
            {
                float tempAP = AP.Value * apChange;
                AP.Value += tempAP;
            }
            else
            {
                AP.Value += apChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateArmorRpc(float armorChange)
    {
        if (IsServer)
        {
            if (armorChange < 0 && armorChange > -1) // If the armor change is due to augment that will add %
            {
                float tempA = armor.Value * armorChange;
                armor.Value += tempA;
            }
            else
            {
                armor.Value += armorChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateMagicResistRpc(float magicResistChange)
    {
        if (IsServer)
        {
            if (magicResistChange < 0 && magicResistChange > -1) // If the magic resist change is due to augment that will add %
            {
                float tempMR = magicResist.Value * magicResistChange;
                magicResist.Value += tempMR;
            }
            else
            {
                magicResist.Value += magicResistChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateAttackSpeedRpc(float attackSpeedChange)
    {
        if (IsServer)
        {
            if (attackSpeedChange < 0 && attackSpeedChange > -1) // If the attack speed change is due to augment that will add %
            {
                float tempAS = attackSpeed.Value * attackSpeedChange;
                attackSpeed.Value += tempAS;
            }
            else
            {
                attackSpeed.Value += attackSpeedChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateMovementSpeedRpc(float movementSpeedChange)
    {
        if (IsServer)
        {
            if (movementSpeedChange < 0 && movementSpeedChange > -1) // If the movement speed change is due to augment that will add %
            {
                float tempMS = movementSpeed.Value * movementSpeedChange;
                movementSpeed.Value += tempMS;
            }
            else
            {
                movementSpeed.Value += movementSpeedChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateMaxManaRpc(float manaChange)
    {
        if (IsServer)
        {
            if (manaChange < 0 && manaChange > -1) // If the mana change is due to augment that will add %
            {
                float tempM = maxMana.Value * manaChange;
                maxMana.Value += tempM;
            }
            else
            {
                maxMana.Value += manaChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateManaRpc(float manaChange)
    {
        if (IsServer)
        {
            mana.Value += manaChange;
        }
    }
    [Rpc(SendTo.Server)]
    public void updateManaRegenRpc(float manaRegenChange)
    {
        if (IsServer)
        {
            if (manaRegenChange < 0 && manaRegenChange > -1) // If the mana regen change is due to augment that will add %
            {
                float tempMR = manaRegen.Value * manaRegenChange;
                manaRegen.Value += tempMR;
            }
            else
            {
                manaRegen.Value += manaRegenChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateAbilityHasteRpc(float abilityHasteChange)
    {
        if (IsServer)
        {
            if (abilityHasteChange < 0 && abilityHasteChange > -1) // If the ability haste change is due to augment that will add %
            {
                float tempAH = abilityHaste.Value * abilityHasteChange;
                abilityHaste.Value += tempAH;
            }
            else
            {
                abilityHaste.Value += abilityHasteChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateCritChanceRpc(float critChanceChange)
    {
        if (IsServer)
        {
            if (critChanceChange < 0 && critChanceChange > -1) // If the crit chance change is due to augment that will add %
            {
                float tempCC = critChance.Value * critChanceChange;
                critChance.Value += tempCC;
            }
            else
            {
                critChance.Value += critChanceChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateCritDamageRpc(float critDamageChange)
    {
        if (IsServer)
        {
            if (critDamageChange < 0 && critDamageChange > -1) // If the crit damage change is due to augment that will add %
            {
                float tempCD = critDamage.Value * critDamageChange;
                critDamage.Value += tempCD;
            }
            else
            {
                critDamage.Value += critDamageChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateArmorPenRpc(float armorPenChange)
    {
        if (IsServer)
        {
            if (armorPenChange < 0 && armorPenChange > -1) // If the armor pen change is due to augment that will add %
            {
                float tempAP = armorPen.Value * armorPenChange;
                armorPen.Value += tempAP;
            }
            else
            {
                armorPen.Value += armorPenChange;
            }
        }
    }
    [Rpc(SendTo.Server)]
    public void updateMagicPenRpc(float magicPenChange)
    {
        if (IsServer)
        {
            if (magicPenChange < 0 && magicPenChange > -1) // If the magic pen change is due to augment that will add %
            {
                float tempMP = magicPen.Value * magicPenChange;
                magicPen.Value += tempMP;
            }
            else
            {
                magicPen.Value += magicPenChange;
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void updateStackCountRpc(int value, int current, int max)
    {
        if (!IsServer) return; // Ensure this is only executed on the server
        if (current + value >= max){
            maxStacks.Value = true; // Set the max stacks flag to true
            stackStartTime.Value = Time.time; // Reset the stack start time
        }
        else if (value == 0){
            stackCount.Value = 0; // Reset the stack count
            maxStacks.Value = false; // Reset the max stacks flag
        }
        else{
            stackCount.Value += value;
            stackStartTime.Value = Time.time; // Reset the stack start time
        }
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
}
