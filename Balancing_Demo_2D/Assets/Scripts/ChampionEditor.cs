using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;

public class ChampionEditor : MonoBehaviour
{
    
}

[System.Serializable]
public struct ChampionData{
    #region Fields
    public string championType;
    public float maxHealth;
    public float healthRegen;
    public float AD;
    public float AP;
    public float armor;
    public float magicResist;
    public float attackSpeed;
    public float movementSpeed;
    public float maxMana;
    public float manaRegen;
    public float abilityHaste;
    public float critChance;
    public float critDamage;
    public float armorPen;
    public float magicPen;
    public float missileSpeed;
    #endregion


    #region Serialization Methods
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref championType);
        serializer.SerializeValue(ref maxHealth);
        serializer.SerializeValue(ref healthRegen);
        serializer.SerializeValue(ref AD);
        serializer.SerializeValue(ref AP);
        serializer.SerializeValue(ref armor);
        serializer.SerializeValue(ref magicResist);
        serializer.SerializeValue(ref attackSpeed);
        serializer.SerializeValue(ref movementSpeed);
        serializer.SerializeValue(ref maxMana);
        serializer.SerializeValue(ref manaRegen);
        serializer.SerializeValue(ref abilityHaste);
        serializer.SerializeValue(ref critChance);
        serializer.SerializeValue(ref critDamage);
        serializer.SerializeValue(ref armorPen);
        serializer.SerializeValue(ref magicPen);
        serializer.SerializeValue(ref missileSpeed);
    }
    #endregion

    public static ChampionData FromChampion(BaseChampion champion)
    {
        return new ChampionData
        {
            championType = champion.championType,
            maxHealth = champion.maxHealth.Value,
            healthRegen = champion.healthRegen.Value,
            AD = champion.AD.Value,
            AP = champion.AP.Value,
            armor = champion.armor.Value,
            magicResist = champion.magicResist.Value,
            attackSpeed = champion.attackSpeed.Value,
            movementSpeed = champion.movementSpeed.Value,
            maxMana = champion.maxMana.Value,
            manaRegen = champion.manaRegen.Value,
            abilityHaste = champion.abilityHaste.Value,
            critChance = champion.critChance.Value,
            critDamage = champion.critDamage.Value,
            armorPen = champion.armorPen.Value,
            magicPen = champion.magicPen.Value,
            missileSpeed = champion.missileSpeed.Value
        };
    }
    
    public void ApplyTo(BaseChampion champion)
    {
        champion.championType = championType;
        champion.maxHealth.Value = maxHealth;
        champion.healthRegen.Value = healthRegen;
        champion.AD.Value = AD;
        champion.AP.Value = AP;
        champion.armor.Value = armor;
        champion.magicResist.Value = magicResist;
        champion.attackSpeed.Value = attackSpeed;
        champion.movementSpeed.Value = movementSpeed;
        champion.maxMana.Value = maxMana;
        champion.manaRegen.Value = manaRegen;
        champion.abilityHaste.Value = abilityHaste;
        champion.critChance.Value = critChance;
        champion.critDamage.Value = critDamage;
        champion.armorPen.Value = armorPen;
        champion.magicPen.Value = magicPen;
        champion.missileSpeed.Value = missileSpeed;
    }
}