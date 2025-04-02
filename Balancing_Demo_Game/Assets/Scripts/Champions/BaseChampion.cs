using UnityEngine;

public class BaseChampion : MonoBehaviour
{
    [Header("Champion Stats")]
    public string championType = "";
    public float health = 600f;
    public float healthRegen = 5f;
    public float AD = 60f;
    public float AP = 0f;
    public float armor = 25f;
    public float magicResist = 30f;
    public float attackSpeed = 0.65f;
    public float movementSpeed = 300f;
    public float mana = 300f;
    public float manaRegen = 7f;

    [Header("Champion Abilities")]
    public string passive = "Passive Ability";
    public string ability1 = "Q";
    public string ability2 = "W";
    public string ability3 = "E";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
