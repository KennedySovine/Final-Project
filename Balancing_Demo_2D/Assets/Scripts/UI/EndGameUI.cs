using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.Collections;

public class EndGameUI : NetworkBehaviour
{
    #region Fields
    private static GameManager GM;
    [SerializeField] private GameObject IGUI;
    [SerializeField] private GameObject augUI;
    [SerializeField] private GameObject endGameUI; // Reference to the end game UI GameObject
    [SerializeField] private GameObject player1Stats; // Reference to the player stats GameObject
    [SerializeField] private GameObject player2Stats; // Reference to the player stats GameObject

    [SerializeField] private List<GameObject> player1StatsList;
    [SerializeField] private List<GameObject> player2StatsList;

    public NetworkList<FixedString64Bytes> player1StatsText = new NetworkList<FixedString64Bytes>();
    public NetworkList<FixedString64Bytes> player2StatsText = new NetworkList<FixedString64Bytes>();

    public NetworkVariable<bool> statsAssigned = new NetworkVariable<bool>(false); // Flag to check if stats are assigned
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Try to find the GameManager using the correct singleton property name
        GM = GameManager.Instance;
        if (GM == null)
        {
            Debug.LogError("GameManager instance is null. Ensure the GameManager is active in the scene.");
        }
    }
    #endregion

    #region Public Methods

    public void ReturnToMainMenu(){
        GM.ReturnToMainMenu(); // Call the return to main menu method in GameManager
    }
    
    public void displayEndGameUI()
    {
        HIDEALLOTHERUI(); // Hide all other UI elements
        endGameUI.SetActive(true); // Activate the end game UI
        player1Stats.SetActive(false); // Deactivate player 1 stats
        player2Stats.SetActive(false); // Deactivate player 2 stats

        StartCoroutine(displayStats()); // Start the coroutine to display stats
    }

    public void updateStatsUI(List<StatBlock> stats1, List<StatBlock> stats2)
    {
        // Make sure we don't try to access non-existent elements
        for (int i = 0; i < Mathf.Min(7, stats1.Count, player1StatsList.Count); i++)
        {
            player1StatsList[i].GetComponent<TextMeshProUGUI>().text = stats1[i].value.ToString();
        }
        
        for (int i = 0; i < Mathf.Min(7, stats2.Count, player2StatsList.Count); i++)
        {
            player2StatsList[i].GetComponent<TextMeshProUGUI>().text = stats2[i].value.ToString();
        }
        
        statsAssigned.Value = true;
    }

    public List<string> findStats(ulong playerId)
    {
        List<string> stats = new List<string>(); // Create a list to hold stats as strings
        BaseChampion champion = null; // Initialize the BaseChampion variable
        if (playerId == GM.player1ID)
        {
            champion = GM.player1Controller.GetComponent<BaseChampion>(); // Get the BaseChampion component from player 1 controller
            if (champion == null)
            {
                Debug.LogError("Champion not found for player 1 controller."); // Log an error if champion is not found
                return stats; // Return empty stats list
            }
            stats.Add(champion.championType); // Add champion type to stats list
            for (int i = 0; i < Mathf.Clamp(GM.player2Augments.Count, 0, 3); i++)
            {
                try
                {
                    stats.Add(GM.AM.AugmentFromID(GM.player1Augments[i]).name); // Add augment name to stats list
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error retrieving augments: " + e.Message); // Log an error if there is an issue retrieving augments
                }
            }
            stats.Add(GameManager.RoundToDecimals(champion.passive.Stats.damageTotal, 0).ToString()); // Add passive damage total to stats list
            stats.Add(GameManager.RoundToDecimals(champion.passive.Stats.damageOverTime, 2).ToString()); // Add passive damage over time to stats list
            stats.Add(GameManager.RoundToDecimals(champion.passive.Stats.costToDamage, 2).ToString()); // Add passive cost to damage to stats list
        }
        else if (playerId == GM.player2ID)
        {
            champion = GM.player2Controller.GetComponent<BaseChampion>(); // Get the BaseChampion component from player 2 controller
            if (champion == null)
            {
                Debug.LogError("Champion not found for player 2 controller."); // Log an error if champion is not found
                return stats; // Return empty stats list
            }
            stats.Add(champion.championType); // Add champion type to stats list
            for (int i = 0; i < Mathf.Clamp(GM.player2Augments.Count, 0, 3); i++)
            {
                try
                {
                    stats.Add(GM.AM.AugmentFromID(GM.player2Augments[i]).name); // Add augment name to stats list
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error retrieving augments: " + e.Message); // Log an error if there is an issue retrieving augments
                }
            }
            stats.Add(GameManager.RoundToDecimals(champion.passive.Stats.damageTotal, 0).ToString()); // Add passive damage total to stats list
            stats.Add(GameManager.RoundToDecimals(champion.passive.Stats.damageOverTime, 2).ToString()); // Add passive damage over time to stats list
            stats.Add(GameManager.RoundToDecimals(champion.passive.Stats.costToDamage, 2).ToString()); // Add passive cost to damage to stats list
        }

        if (champion == null)
        {
            Debug.LogError("Champion not found for player ID: " + playerId); // Log an error if champion is not found
        }
        return stats; // Return the list of stats as strings
    }

    public void statsToList(){
        if (NetworkManager.Singleton.IsServer)
        {
            // Clear previous data
            player1StatsText.Clear();
            player2StatsText.Clear();
            
            // Get raw stats
            List<string> p1StatsRaw = findStats(GM.player1ID);
            List<string> p2StatsRaw = findStats(GM.player2ID);
            
            // Convert to StatBlocks for UI
            List<StatBlock> p1Stats = new List<StatBlock>();
            List<StatBlock> p2Stats = new List<StatBlock>();
            
            // Populate NetworkLists for network synchronization
            foreach (var stat in p1StatsRaw) {
                player1StatsText.Add(new FixedString64Bytes(stat));
                p1Stats.Add(new StatBlock(stat));
            }
            
            foreach (var stat in p2StatsRaw) {
                player2StatsText.Add(new FixedString64Bytes(stat));
                p2Stats.Add(new StatBlock(stat));
            }
            
            // Update UI
            updateStatsUI(p1Stats, p2Stats);
            
            // Notify clients that stats are ready
            SyncEndGameStatsRpc();
        }
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void SyncEndGameStatsRpc()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            // Convert NetworkList data to StatBlocks
            List<StatBlock> p1Stats = new List<StatBlock>();
            List<StatBlock> p2Stats = new List<StatBlock>();
            
            foreach (var stat in player1StatsText)
                p1Stats.Add(new StatBlock(stat.ToString()));
            
            foreach (var stat in player2StatsText)
                p2Stats.Add(new StatBlock(stat.ToString()));
            
            // Update UI with received data
            updateStatsUI(p1Stats, p2Stats);
        }
    }
    #endregion

    #region Private Methods
    private void HIDEALLOTHERUI(){
        IGUI.SetActive(false); // Deactivate the in-game UI
        augUI.SetActive(false); // Deactivate the augment UI
    }

    private IEnumerator displayStats()
    {
        yield return new WaitUntil(() => statsAssigned.Value); // Wait until stats are assigned
        player1Stats.SetActive(true);
        player2Stats.SetActive(true);

        for (int i = 0; i < player1StatsList.Count; i++)
        {
            player1StatsList[i].SetActive(true); // Activate the current stat UI element for player 1
            player2StatsList[i].SetActive(true); // Activate the current stat UI element for player 2
            yield return new WaitForSeconds(1f); // Wait for 1 second before displaying the next stat
        }
    }
    #endregion
}

[System.Serializable]
public struct StatBlock : INetworkSerializable
{
    #region Fields
    public FixedString64Bytes value;
    #endregion

    #region Serialization Methods
    public StatBlock(string v) => value = v;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref value);
    }
    #endregion
}
