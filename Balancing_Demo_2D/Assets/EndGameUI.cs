using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;

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

    public void displayEndGameUI()
    {
        endGameUI.SetActive(true); // Activate the end game UI
        player1Stats.SetActive(false); // Deactivate player 1 stats
        player2Stats.SetActive(false); // Deactivate player 2 stats

        List<GameObject> children = new List<GameObject>(); // Create a list to hold child GameObjects
        findAllChildren(endGameUI, children); // Find all child GameObjects of the end game UI

        foreach (GameObject child in children)
        {
            child.SetActive(true); // Activate each child GameObject
        }
    }

    private void findStats(ulong playerId){

    }
}
