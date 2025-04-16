using UnityEngine;

[System.Serializable]
public class Augment
{
    public string name;
    public int id;
    public string description;
    public string type;
    // Ability haste, armor, AP, AD, Attack speed, crit chance, crit damage, health, armor pen, magic pen, MR
    public float min;
    public float max;
    public int rarity; // Silver 1, Gold 2, Prismatic 3
}