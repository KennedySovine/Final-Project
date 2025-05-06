using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.Collections;

public class EndGameUI : MonoBehaviour
{
    private static GameManager GM;
    [SerializeField] private GameObject endGameUI; // Reference to the end game UI GameObject
    [SerializeField] private GameObject player1Stats; // Reference to the player stats GameObject
    [SerializeField] private GameObject player2Stats; // Reference to the player stats GameObject

    [SerializeField] private List<GameObject> player1StatsList;
    [SerializeField] private List<GameObject> player2StatsList;

    public NetworkList<FixedString64Bytes> player1StatsText = new NetworkList<FixedString64Bytes>();
    public NetworkList<FixedString64Bytes> player2StatsText = new NetworkList<FixedString64Bytes>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance
        if (GM == null)
        {
            Debug.LogError("GameManager instance is null. Ensure the GameManager is active in the scene.");
        }
    }

    private List<GameObject> findAllChildren(GameObject parent, List<GameObject> children)
    {
        foreach (Transform child in parent.transform)
        {
            children.Add(child.gameObject);
        }
        return children; // Return the list of child GameObjects
    }

    public void displayEndGameUI(ulong p1, ulong p2)
    {
        endGameUI.SetActive(true); // Activate the end game UI
        player1Stats.SetActive(false); // Deactivate player 1 stats
        player2Stats.SetActive(false); // Deactivate player 2 stats

        foreach (GameObject stat in player1StatsList)
        {
            stat.GetComponent<TextMeshProUGUI>().text = ""; // Clear the text for player 1 stats
            stat.SetActive(false); // Deactivate player 1 stats
        }
        foreach (GameObject stat in player2StatsList)
        {
            stat.GetComponent<TextMeshProUGUI>().text = ""; // Clear the text for player 2 stats
            stat.SetActive(false); // Deactivate player 2 stats
        }

        if (NetworkManager.Singleton.IsServer)
        {
            List<string> p1StatsRaw = findStats(p1);
            List<string> p2StatsRaw = findStats(p2);

            List<StatBlock> p1Stats = new List<StatBlock>();
            List<StatBlock> p2Stats = new List<StatBlock>();

            foreach (var stat in p1StatsRaw)
                p1Stats.Add(new StatBlock(stat));
            foreach (var stat in p2StatsRaw)
                p2Stats.Add(new StatBlock(stat));

            updateStatsUIRpc(p1Stats, p2Stats);

        }
    }

    [Rpc(SendTo.Everyone)]
    public void updateStatsUIRpc(List<StatBlock> stats1, List<StatBlock> stats2)
    {
        for (int i = 0; i < 7; i++)
        {
            player1StatsList[i].GetComponent<TextMeshProUGUI>().text = stats1[i].value.ToString();
            player2StatsList[i].GetComponent<TextMeshProUGUI>().text = stats2[i].value.ToString();
        }

        player1Stats.SetActive(true);
        player2Stats.SetActive(true);
        StartCoroutine(displayStats());
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
            for (int i = 0; i < GM.player1Augments.Count; i++)
            {
                try
                {
                    stats.Add(GM.AM.augmentFromID(GM.player1Augments[i]).name); // Add augment name to stats list
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error retrieving augments: " + e.Message); // Log an error if there is an issue retrieving augments
                }
            }
            stats.Add(champion.passive.Stats.damageTotal.ToString()); // Add passive damage total to stats list
            stats.Add(champion.passive.Stats.damageOverTime.ToString()); // Add passive damage over time to stats list
            stats.Add(champion.passive.Stats.costToDamage.ToString()); // Add passive cost to damage to stats list
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
            for (int i = 0; i < GM.player2Augments.Count; i++)
            {
                try
                {
                    stats.Add(GM.AM.augmentFromID(GM.player2Augments[i]).name); // Add augment name to stats list
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error retrieving augments: " + e.Message); // Log an error if there is an issue retrieving augments
                }
            }
            stats.Add(champion.passive.Stats.damageTotal.ToString()); // Add passive damage total to stats list
            stats.Add(champion.passive.Stats.damageOverTime.ToString()); // Add passive damage over time to stats list
            stats.Add(champion.passive.Stats.costToDamage.ToString()); // Add passive cost to damage to stats list
        }

        if (champion == null)
        {
            Debug.LogError("Champion not found for player ID: " + playerId); // Log an error if champion is not found
        }
        return stats; // Return the list of stats as strings
    }

    private IEnumerator displayStats()
    {
        for (int i = 0; i < player1StatsList.Count; i++)
        {
            player1StatsList[i].SetActive(true); // Activate the current stat UI element for player 1
            player2StatsList[i].SetActive(true); // Activate the current stat UI element for player 2
            yield return new WaitForSeconds(1f); // Wait for 1 second before displaying the next stat
        }
    }
}

[System.Serializable]
public struct StatBlock : INetworkSerializable
{
    public FixedString64Bytes value;

    public StatBlock(string v) => value = v;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref value);
    }
}
