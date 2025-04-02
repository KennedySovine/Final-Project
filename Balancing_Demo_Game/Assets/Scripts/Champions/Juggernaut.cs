using UnityEngine;

public class Juggernaut : BaseChampion
{
    public Ability passiveAbility;
    public Ability ability1; // Q
    public Ability ability2; // W
    public Ability ability3; // E

    void Start()
    {
        UpdateStats();
        AddAbilities();
    }

    private void UpdateStats()
    {
        championType = "Juggernaut";
        health = 600f;
        healthRegen = 8f;
        AD = 65f;
        AP = 40f;
        armor = 33f;
        magicResist = 33f;
        attackSpeed = 0.64f;
        movementSpeed = 342f;
        mana = 350f;
        manaRegen = 7f;
    }

    private void AddAbilities()
    {
        passiveAbility = new Ability(
            "Bleed",
            "Cause opponent to bleed for 1.25 seconds for 5 seconds with up to 5 stacks.",
            0f, // No cooldown for passive
            0f, // No mana cost for passive
            0f  // No range for passive
        );

        ability1 = new Ability(
            "Cleave",
            "Damage in a circle with a wind-up.",
            9f, // Cooldown in seconds
            25f, // Mana cost
            1f  // Range
        );

        ability2 = new Ability(
            "Empower",
            "Empower the next ability and slow the enemy for 1 second.",
            5f, // Cooldown in seconds
            40f, // Mana cost
            300f  // Range
        );

        ability3 = new Ability(
            "Draw In",
            "Draw the enemy in.",
            26f, // Cooldown in seconds
            70f, // Mana cost
            460f  // Range
        );
    }

    public void UseAbility1()
    {
        ability1.UseAbility();
    }

    public void UseAbility2()
    {
        ability2.UseAbility();
    }

    public void UseAbility3()
    {
        ability3.UseAbility();
    }
}