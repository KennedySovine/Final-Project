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

    public string championType = ""; // Type of champion using the ability

    public float damageTotal = 0f; // Total damage calculated from the list of damage values
    public float gameTime = 0f; // Game time value

    public List<Augment> chosenAugments = new List<Augment>(); // List of chosen augments for the ability

    public AbilityStats()
    {

    }
    public void saveBetweenAugments()
    {
        damageValues.Add(damage); // Add the current damage value to the list
        Debug.Log($"Damage value saved: {damage}"); // Log the saved damage value
        damageTotal = 0f; // Reset the total damage for calculation
    }

    public void endGameCalculations(List<Augment> chosenAugments, float gameTime)
    {
        this.chosenAugments = chosenAugments; // Assign the chosen augments to the class variable
        damageTotal = damageValues.Sum(); // Calculate the total damage from the list of damage values
        damageOverTime = gameTime > 0 ? damageTotal / gameTime : 0f; // Calculate damage over time
        costToDamage = damageTotal > 0 ? damageTotal / totalManaSpent : 0f; // Calculate cost to damage ratio

        SaveToFile(); // Save the ability stats to a JSON file
    }

    public void SaveToFile()
    {
        string filePath = Path.Combine(Application.dataPath, "Resources/PlayerStats.json"); // Construct the file path
        List<AbilityStats> statsList = new List<AbilityStats>();

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the existing file content
            string existingJson = File.ReadAllText(filePath);

            // Parse the existing JSON into a list of AbilityStats
            statsList = JsonUtility.FromJson<AbilityStatsListWrapper>(existingJson)?.stats ?? new List<AbilityStats>();
        }

        // Add the current AbilityStats object to the list
        statsList.Add(this);

        // Wrap the list in a wrapper class and convert it back to JSON
        string updatedJson = JsonUtility.ToJson(new AbilityStatsListWrapper { stats = statsList }, true);

        // Write the updated JSON back to the file
        File.WriteAllText(filePath, updatedJson);

        Debug.Log($"Ability stats appended to {filePath}");
    }

    // Wrapper class to handle JSON arrays
    [System.Serializable]
    private class AbilityStatsListWrapper
    {
        public List<AbilityStats> stats;
    }
}