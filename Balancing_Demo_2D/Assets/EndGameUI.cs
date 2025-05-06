using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EndGameUI : MonoBehaviour
{
    private static GameManager GM;
    [SerializeField] private GameObject endGameUI; // Reference to the end game UI GameObject
    [SerializeField] private GameObject player1Stats; // Reference to the player stats GameObject
    [SerializeField] private GameObject player2Stats; // Reference to the player stats GameObject

    [SerializeField] private List<GameObject> player1StatsList;
    [SerializeField] private List<GameObject> player2StatsList;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance
        if (GM == null)
        {
            Debug.LogError("GameManager instance is null. Ensure the GameManager is active in the scene.");
        }
        
    }

    void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private List<GameObject> findAllChildren(GameObject parent, List<GameObject> children)
    {
        foreach (Transform child in parent.transform)
        {
            children.Add(child.gameObject);
        }
        return children; // Return the list of child GameObjects
    }

    public void displayEndGameUI(ulong playerId)
    {
        endGameUI.SetActive(true); // Activate the end game UI
        player1Stats.SetActive(false); // Deactivate player 1 stats
        player2Stats.SetActive(false); // Deactivate player 2 stats

        List<string> p1stats = findStats(GM.player1Id); // Get player 1 stats
        List<string> p2stats = findStats(GM.player2Id); // Get player 2 stats

        updateStatsUIRpc(p1stats, p2stats); // Update the stats UI for both players

        //Deactivate all Stats
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
    }

    [Rpc(SendTo.Everyone)]
    public void updateStatsUIRpc(List<string> stats1, List<string> stats2){
        for (int i = 0; i < 7; i++){
            player1StatsList[i].GetComponent<TextMeshProUGUI>().text = GM.player1Stats[i]; // Update player 1 stats text
            player2StatsList[i].GetComponent<TextMeshProUGUI>().text = GM.player2Stats[i]; // Update player 2 stats text
        }
        player1Stats.SetActive(true); // Activate player 1 stats UI
        player2Stats.SetActive(true); // Activate player 2 stats UI
        displayStats(); // Call the coroutine to display stats
    }

    public List<string> findStats(ulong playerId){
        List<string> stats = new List<string>(); // Create a list to hold stats as strings
        BaseChampion champion;
        if (playerId == GM.player1Id)
        {
            champion = GM.player1Controller.GetComponent<BaseChampion>(); // Get the BaseChampion component from player 1 controller
            stats.Add(champion.championType); // Add champion type to stats list
            stats.Add(GM.player1Augments[0].augmentName); // Add first augment name to stats list
            stats.Add(GM.player1Augments[1].augmentName); // Add second augment name to stats list
            stats.Add(GM.player1Augments[2].augmentName); // Add third augment name to stats list
            stats.Add(champion.passive.Stats.damageTotal.ToString()); // Add passive damage total to stats list
            stats.Add(champion.passive.Stats.damageOverTime.ToString()); // Add passive damage over time to stats list
            stats.Add(champion.passive.Stats.costToDamage.ToString()); // Add passive cost to damage to stats list
        }
        else if (playerId == GM.player2Id)
        {
            champion = GM.player2Controller.GetComponent<BaseChampion>(); // Get the BaseChampion component from player 2 controller
            stats.Add(champion.championType); // Add champion type to stats list
            stats.Add(GM.player2Augments[0].augmentName); // Add first augment name to stats list
            stats.Add(GM.player2Augments[1].augmentName); // Add second augment name to stats list
            stats.Add(GM.player2Augments[2].augmentName); // Add third augment name to stats list
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

    private IEnumerator displayStats(){
        yield return new WaitForSeconds (1f); // Wait for 1 second before displaying stats

        for (int i = 0; i < player1StatsList.Count; i++){
            player1StatsList[i].SetActive(true); // Activate each stat UI element for player 1
            player2StatsList[i].SetActive(true); // Activate each stat UI element for player 2
        }
    }
}
