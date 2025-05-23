using UnityEngine;
using Unity.Netcode; // Required for NetworkVariable

[System.Serializable]
public class Ability
{
    #region Fields
    public Sprite icon; // Icon for the ability
    public Sprite icon2;
    public string name; // Name of the ability
    public string description; // Description of the ability
    public float cooldown = 0f; // Cooldown time in seconds
    public float manaCost = 0f; // Mana cost to use the ability
    public float range; // Range of the ability
    public float duration; // Duration of the ability effect (if applicable)
    public AbilityStats Stats; // Reference to the ability stats object

    public float timeOfCast; // Time when the ability was cast
    #endregion

    #region Properties
    public bool isOnCooldown // Flag to check if the ability is on cooldown
    {
        get {
            if (cooldown == 0f){
                return false; // No cooldown, ability is available
            }
            return timeOfCast > 0f && Time.time - timeOfCast < cooldown; 
        }
    }
    #endregion

    #region Constructors
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
    #endregion

    #region Public Methods
    public void Update()
    {
        if (timeOfCast > 0f && Time.time - timeOfCast >= cooldown)
        {
            timeOfCast = 0f; // Reset the time of cast when cooldown is over
        }
    }

    public void SetCooldown(float cooldown)
    {
        this.cooldown = cooldown;
    }
    
    public void SetManaCost(float manaCost)
    {
        this.manaCost = manaCost;
    }
    
    public void SetRange(float range)
    {
        this.range = range;
    }
    
    public void SetDuration(float duration)
    {
        this.duration = duration;
    }

    public bool CheckIfAvailable(float mana)
    {
        return (cooldown == 0 || (mana >= manaCost && !isOnCooldown)); // Check if the ability is available based on mana and cooldown
    }
    #endregion
}