using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;

[System.Serializable]
public class AbilityStats
{

    public List<float> damageValues = new List<float>(); // List to hold damage values between augments
    public float damageOverTime = 0f; // Damage over time value
    public float damage = 0f; // Total damage value
    public float costToDamage = 0f; // Cost to damage value
    public float totalManaSpent = 0f; // Total mana spent on the ability

    private float damageTotal = 0f; // Total damage calculated from the list of damage values
    public float gameTime = 0f; // Game time value

    public AbilityStats(Ability ability){
    }
    public void saveBetweenAugments()
    {
        damageValues.Add(damage); // Add the current damage value to the list
        damageTotal = 0f; // Reset the total damage for calculation
    }

    public void endGameCalculations(List<Augment> chosenAugments)
    {
        damageTotal = damageValues.Sum(); // Calculate the total damage from the list of damage values
        damageOverTime = gameTime > 0 ? damageTotal / gameTime : 0f; // Calculate damage over time
        costToDamage = damageTotal > 0 ? damageTotal / totalManaSpent : 0f; // Calculate cost to damage ratio

        SaveToFile(); // Save the ability stats to a JSON file
    }

    public void SaveToFile()
    {
        string json = JsonUtility.ToJson(this, true); // Convert the AbilityStats object to a JSON string (pretty print)
        string filePath = Path.Combine(Application.dataPath, "Resources/PlayerStats.json"); // Construct the file path

        File.WriteAllText(filePath, json); // Write the JSON string to the file
        Debug.Log($"Ability stats saved to {filePath}");
    }
}