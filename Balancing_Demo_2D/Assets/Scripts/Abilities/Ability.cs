using UnityEngine;
using Unity.Netcode; // Required for NetworkVariable

[System.Serializable]
public class Ability : NetworkBehaviour
{
    public Sprite icon; // Icon for the ability
    public Sprite icon2;
    public string name; // Name of the ability
    public string description; // Description of the ability
    public float cooldown = 0f; // Cooldown time in seconds
    public float manaCost = 0f; // Mana cost to use the ability
    public float range; // Range of the ability
    public float duration; // Duration of the ability effect (if applicable)
    public AbilityStats Stats; // Reference to the ability stats object

    public NetworkVariable<float> timeOfCast = new NetworkVariable<float>(0f); // Network variable for the time of cast
    public NetworkVariable<bool> isOnCooldown = new NetworkVariable<bool>(false); // Network variable for cooldown state

    public Ability(string name, string description, float cooldown, float manaCost, float range)
    {
        this.name = name;
        this.description = description;
        this.cooldown = cooldown;
        this.manaCost = manaCost;
        this.range = range;
        this.duration = 0f; // Default duration to 0, can be set later if needed
        this.Stats = new AbilityStats(); // Initialize ability stats
    }

    public void Update()
    {
        if (timeOfCast.Value > 0f && Time.time - timeOfCast.Value >= cooldown)
        {
            timeOfCast.Value = 0f; // Reset the time of cast when cooldown is over
            isOnCooldown.Value = false; // Update the network variable
        }
        else if (timeOfCast.Value > 0f && Time.time - timeOfCast.Value < cooldown)
        {
            isOnCooldown.Value = true; // Update the network variable
        }
    }

    public void setCooldown(float cooldown)
    {
        this.cooldown = cooldown;
    }
    public void setManaCost(float manaCost)
    {
        this.manaCost = manaCost;
    }
    public void setRange(float range)
    {
        this.range = range;
    }
    public void setDuration(float duration)
    {
        this.duration = duration;
    }

    public bool checkIfAvailable(float mana)
    {
        return (cooldown == 0 || (mana >= manaCost && !isOnCooldown.Value)); // Check if the ability is available based on mana and cooldown
    }
}