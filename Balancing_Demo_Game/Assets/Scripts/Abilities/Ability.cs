using UnityEngine;

[System.Serializable]
public class Ability
{
    public string name; // Name of the ability
    public string description; // Description of the ability
    public float cooldown; // Cooldown time in seconds
    public float manaCost; // Mana cost to use the ability
    public float range; // Range of the ability

    public Ability(string name, string description, float cooldown, float manaCost, float range)
    {
        this.name = name;
        this.description = description;
        this.cooldown = cooldown;
        this.manaCost = manaCost;
        this.range = range;
    }

    public void UseAbility()
    {
        Debug.Log($"Using ability: {name} - {description}");
    }

    public void setCooldown(float cooldown)
    {
        this.cooldown = cooldown;
    }
    public void setManaCost(float manaCost)
    {
        this.manaCost = manaCost;
    }
}