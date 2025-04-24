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
    public NetworkVariable<float> movementSpeed = new NetworkVariable<float>(300f);
    public NetworkVariable<float> maxMana = new NetworkVariable<float>(300f);
    public NetworkVariable<float> manaRegen = new NetworkVariable<float>(7f);
    public NetworkVariable<float> abilityHaste = new NetworkVariable<float>(0f);
    public NetworkVariable<float> critChance = new NetworkVariable<float>(0f);
    public NetworkVariable<float> critDamage = new NetworkVariable<float>(1.75f); // 175% damage on crit
    public NetworkVariable<float> armorPen = new NetworkVariable<float>(0f);
    public NetworkVariable<float> magicPen = new NetworkVariable<float>(0f);

    public NetworkVariable<Vector3> currentPosition = new NetworkVariable<Vector3>(Vector3.zero);

    [Header("Champion Resources")]
    public NetworkVariable<float> health = new NetworkVariable<float>(600f);
    public NetworkVariable<float> mana = new NetworkVariable<float>(300f);

    [Header("Champion Abilities")]
    public Ability autoAttack = new Ability("Auto Attack", "Basic attack", 0f, 0f, 0f);
    public Ability passive;
    public Ability ability1;
    public Ability ability2;
    public Ability ability3;

    private float lastAutoAttackTime = 0f; // Tracks the last time an auto-attack was fired

    [Header("Champion Settings")]
    public int attackConsecutive = 0; // Number of consecutive attacks against oneself
    public float regenTimer = 0f;

    public GameObject enemyChampion; // Reference to the enemy champion prefab

    public GameObject bulletPrefab; // Prefab for the bullet to be fired

    public PlayerNetwork PN; // Reference to the PlayerNetwork script

    public void Start()
    {
        // Initialization logic if needed

    }

    void Update()
    {
        // Example: Sync health regeneration logic
        if (IsServer) // Only the server should modify NetworkVariables
        {
            //HealthandManaRegen();
        }
    }

    /*private void HealthandManaRegen()
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
                Debug.Log($"Regenerating health: {healthRegen.Value}");
            }
            if (mana.Value < maxMana.Value)
            {
                mana.Value = Mathf.Min(mana.Value + manaRegen.Value, maxMana.Value); // Ensure mana does not exceed maxMana
                Debug.Log($"Regenerating mana: {manaRegen.Value}");
            }
        }
    }*/

    [Rpc(SendTo.Server)]
    public void basicAttackRpc()
    {
        if (!IsServer) return; // Ensure this logic runs only on the server

        // Check if the enemy champion exists
        if (enemyChampion == null)
        {
            Debug.LogWarning("No enemy champion assigned.");
            return;
        }

        // Perform a raycast from the mouse position
        Ray ray = PN.personalCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the raycast hit the enemy champion
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == enemyChampion)
            {
                // Check if the player is within range of the enemy champion
                float distance = Vector3.Distance(transform.position, enemyChampion.transform.position);
                if (distance <= autoAttack.range)
                {
                    Debug.Log("Basic Attack hit!");

                    // Check if the cooldown has passed
                    if (Time.time >= lastAutoAttackTime + autoAttack.cooldown)
                    {
                        // Instantiate and configure the bullet
                        GameObject attackObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                        attackObj.GetComponent<NetworkObject>().Spawn(); // Spawn the bullet object on the network
                        attackObj.GetComponent<Bullet>().ADDamage = AD.Value;
                        attackObj.GetComponent<Bullet>().armorPenetration = armorPen.Value;
                        attackObj.GetComponent<Bullet>().magicPenetration = magicPen.Value;
                        attackObj.GetComponent<Bullet>().targetPosition = enemyChampion.transform.position; // Set the target position
                        attackObj.GetComponent<Bullet>().ownerId = OwnerClientId; // Set the owner ID for the bullet
                        attackObj.GetComponent<Bullet>().isAutoAttack = true; // Mark the bullet as an auto attack
                        attackObj.GetComponent<Bullet>().targetPlayer = enemyChampion; // Set the target player

                        Debug.Log("Basic Attack performed!");

                        // Update the last auto-attack time
                        lastAutoAttackTime = Time.time;
                    }
                    else
                    {
                        Debug.Log("Basic Attack is on cooldown!");
                    }
                }
                else
                {
                    Debug.Log("Target out of range!");
                }
            }
            else
            {
                Debug.Log("Raycast did not hit the enemy champion!");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything!");
        }
    }

    public void critLogic(){}

    // Function to deal with being hit by projectiles (bullets)
    // Check the ownerID vs the playerID of the bullet
    // If they are the same, do not take damage

    public void updateMaxHealth(float healthChange)
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

    public void updateHealth(float healthChange)
    {
        if (IsServer)
        {
            health.Value += healthChange;
        }
    }
    public void updateAD(float adChange)
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

    public void updateAP(float apChange)
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

    public void updateArmor(float armorChange)
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

    public void updateMagicResist(float magicResistChange)
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

    public void updateAttackSpeed(float attackSpeedChange)
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

    public void updateMovementSpeed(float movementSpeedChange)
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

    public void updateMaxMana(float manaChange)
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

    public void updateMana(float manaChange)
    {
        if (IsServer)
        {
            mana.Value += manaChange;
        }
    }

    public void updateManaRegen(float manaRegenChange)
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

    public void updateAbilityHaste(float abilityHasteChange)
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

    public void updateCritChance(float critChanceChange)
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

    public void updateCritDamage(float critDamageChange)
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

    public void updateArmorPen(float armorPenChange)
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

    public void updateMagicPen(float magicPenChange)
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
}
