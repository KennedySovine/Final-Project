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

    public Ability(string name, string description, float cooldown, float manaCost, float range)
    {
        this.name = name;
        this.description = description;
        this.cooldown = cooldown;
        this.manaCost = manaCost;
        this.range = range;
        this.duration = 0f; // Default duration to 0, can be set later if needed
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