using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Unity.Netcode;

[System.Serializable]
public class AbilityStats{

    public Ability ability; // Reference to the ability object
    public List<float> damageValues = new List<float>(); // List to hold damage values between augments
    public float damageOverTime = 0f; // Damage over time value
    public float damageTotal = 0f; // Total damage value
    public float costToDamage = 0f; // Cost to damage value
    public float totalManaSpent = 0f; // Total mana spent on the ability

    public AbilityStats(Ability ability, float damageOverTime, float damageTotal, float costToDamage){
        this.ability = ability;
        this.damageOverTime = damageOverTime;
        this.damageTotal = damageTotal;
        this.costToDamage = costToDamage;
    }
}