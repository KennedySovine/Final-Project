using UnityEngine;

[System.Serializable]
public class Ability
{
    public string name; // Name of the ability
    public string description; // Description of the ability
    public float cooldown; // Cooldown time in seconds
    public float manaCost; // Mana cost to use the ability
    public float range; // Range of the ability
    public float duration; // Duration of the ability effect (if applicable)

    public float timeOfCast; // Time when the ability was cast

    public bool isOnCooldown // Flag to check if the ability is on cooldown
    {
        get { return timeOfCast > 0f && Time.time - timeOfCast < cooldown; }
    }

    public Ability(string name, string description, float cooldown, float manaCost, float range)
    {
        this.name = name;
        this.description = description;
        this.cooldown = cooldown;
        this.manaCost = manaCost;
        this.range = range;
        this.duration = 0f; // Default duration to 0, can be set later if needed
    }

    public void Update()
    {
        if (timeOfCast > 0f && Time.time - timeOfCast >= cooldown)
        {
            timeOfCast = 0f; // Reset the time of cast when cooldown is over
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
}