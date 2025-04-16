using UnityEngine;

public class BaseChampion : MonoBehaviour
{
    [Header("Champion Stats")]
    public string championType = "";
    public float health = 600f;
    public float healthRegen = 5f;
    public float AD = 60f;
    public float AP = 0f;
    public float armor = 25f;
    public float magicResist = 30f;
    public float attackSpeed = 0.65f;
    public float movementSpeed = 300f;
    public float mana = 300f;
    public float manaRegen = 7f;
    public float abilityHaste = 0f;
    public float critChance = 0f;
    public float critDamage = 1.75f; // 175% damage on crit

    [Header("Champion Abilities")]
    public Ability autoAttack = new Ability("Auto Attack", "Basic attack", 0f, 0f, 0f);
    public Ability passive;
    public Ability ability1;
    public Ability ability2;
    public Ability ability3;

    private float lastAutoAttackTime = 0f; // Tracks the last time an auto-attack was fired

    void Start()
    {
        // Initialization logic if needed
    }

    void Update()
    {
        // Check if the player presses the right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            FireAutoAttack();
        }
    }

    private void FireAutoAttack()
    {
        // Check if the auto-attack is off cooldown
        ; // Cooldown based on attack speed
        if (Time.time - lastAutoAttackTime >= attackSpeed)
        {
            Debug.Log($"{championType} fires an auto-attack!");
            lastAutoAttackTime = Time.time;

            // Add logic to deal damage to the target here
            PerformAutoAttack();
        }
        else
        {
            Debug.Log("Auto-attack is on cooldown.");
        }
    }

    private void PerformAutoAttack()
    {
        // Example logic for dealing damage
        Debug.Log($"Dealing {AD} physical damage to the target.");
        // Add logic to find and damage the target here
    }

    public void updateHeath(float health)
    {
        if (health < 0 && health > -1) //If the health change is due to augment that will add %
        {
            float tempH = this.health * health;
            this.health += tempH;
        }
        else
        {
            this.health += health;
        }
    }
    public void updateAD(float ad)
    {
        if (ad < 0 && ad > -1) //If the AD change is due to augment that will add %
        {
            float tempAD = this.AD * ad;
            this.AD += tempAD;
        }
        else
        {
            this.AD += ad;
        }
    }
    public void updateAP(float ap)
    {
        if (ap < 0 && ap > -1) //If the AP change is due to augment that will add %
        {
            float tempAP = this.AP * ap;
            this.AP += tempAP;
        }
        else
        {
            this.AP += ap;
        }
    }
    public void updateArmor(float armor)
    {
        if (armor < 0 && armor > -1) //If the armor change is due to augment that will add %
        {
            float tempA = this.armor * armor;
            this.armor += tempA;
        }
        else
        {
            this.armor += armor;
        }
    }
    public void updateMagicResist(float magicResist)
    {
        if (magicResist < 0 && magicResist > -1) //If the magic resist change is due to augment that will add %
        {
            float tempMR = this.magicResist * magicResist;
            this.magicResist += tempMR;
        }
        else
        {
            this.magicResist += magicResist;
        }
    }
    public void updateAttackSpeed(float attackSpeed)
    {
        if (attackSpeed < 0 && attackSpeed > -1) //If the attack speed change is due to augment that will add %
        {
            float tempAS = this.attackSpeed * attackSpeed;
            this.attackSpeed += tempAS;
        }
        else
        {
            this.attackSpeed += attackSpeed;
        }
    }
    public void updateMovementSpeed(float movementSpeed)
    {
        if (movementSpeed < 0 && movementSpeed > -1) //If the movement speed change is due to augment that will add %
        {
            float tempMS = this.movementSpeed * movementSpeed;
            this.movementSpeed += tempMS;
        }
        else
        {
            this.movementSpeed += movementSpeed;
        }
    }
    public void updateMana(float mana)
    {
        if (mana < 0 && mana > -1) //If the mana change is due to augment that will add %
        {
            float tempM = this.mana * mana;
            this.mana += tempM;
        }
        else
        {
            this.mana += mana;
        }
    }
    public void updateManaRegen(float manaRegen)
    {
        if (manaRegen < 0 && manaRegen > -1) //If the mana regen change is due to augment that will add %
        {
            float tempMR = this.manaRegen * manaRegen;
            this.manaRegen += tempMR;
        }
        else
        {
            this.manaRegen += manaRegen;
        }
    }
    public void updateAbilityHaste(float abilityHaste)
    {
        if (abilityHaste < 0 && abilityHaste > -1) //If the ability haste change is due to augment that will add %
        {
            float tempAH = this.abilityHaste * abilityHaste;
            this.abilityHaste += tempAH;
        }
        else
        {
            this.abilityHaste += abilityHaste;
        }
    }
    public void updateCritChance(float critChance)
    {
        if (critChance < 0 && critChance > -1) //If the crit chance change is due to augment that will add %
        {
            float tempCC = this.critChance * critChance;
            this.critChance += tempCC;
        }
        else
        {
            this.critChance += critChance;
        }
    }
    public void updateCritDamage(float critDamage)
    {
        if (critDamage < 0 && critDamage > -1) //If the crit damage change is due to augment that will add %
        {
            float tempCD = this.critDamage * critDamage;
            this.critDamage += tempCD;
        }
        else
        {
            this.critDamage += critDamage;
        }
    }

}
