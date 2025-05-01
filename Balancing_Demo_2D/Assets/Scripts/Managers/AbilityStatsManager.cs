using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;

public class AbilityStatsManager : NetworkBehaviour
{
    private GameManager GM; // Reference to the GameManager

    [Header("Ability Lists")]
    public List<AbilityStats> abilityStatsList = new List<AbilityStats>(); // List to hold ability stats

    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance
    }
}