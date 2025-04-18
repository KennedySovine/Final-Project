using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance
    //public List<ulong> playerList = new List<ulong>(); // List of connected players

    // Used to prevent duplicate Server/Host creation
    public Dictionary<string, ulong> playerList = new Dictionary<string, ulong>(); // Dictionary to store player 'role' and IDs

    // Used to store the player prefab and connect it to the client ID
    public Dictionary<ulong, GameObject> playerChampions = new Dictionary<ulong, GameObject>(); // Dictionary to store player prefabs and connect it to the client ID
    public List<ulong> playerIDsSpawned = new List<ulong>(); // List of player IDs that have spawned champions

    [Header("Player Class Prefabs")]
    public List<GameObject> playerPrefabsList = new List<GameObject>(); // List of player prefabs

    [Header("Player References")]
    public GameObject player1;
    public GameObject player2;

    [Header("Game Settings")]
    public int playerCount = 0; // Number of players connected
    public int maxPlayers = 2;
    public float gameTime = 120f; // Game duration in seconds
    public float augmentBuffer = 40f; //Choose aug every 40 seconds
    public bool augmentChosing = false; //If the player is choosing an augment, dont countdown the game time

    [Header("Champion Management")]
    public GameObject championPrefab; // Prefab for spawning champions
    public Transform[] spawnPoints; // Array of spawn points for champions


    private void Awake()
    {
        // Ensure only one instance of GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("Game Manager Initialized");
    }

    private void Update()
    {

        //Check if 2 players have connected
        if (playerList.Count > 0)
        {
            //Start the game logic
            //Spawn in champions and update the controllers to each player
            spawnChampions();
        
            // Countdown the game time
            if (augmentChosing){} //If the player is choosing an augment, dont countdown the game time
            else if (gameTime > 0){
                gameTime -= Time.deltaTime;
            }
            else{
                EndGame();
            }

            if (augmentBuffer > 0){
                augmentBuffer -= Time.deltaTime;
            }
            else{
                augmentLogic();
            }
        }
    }

    public void spawnChampions()
    {
        foreach (var player in playerChampions)
        {
            GameObject playerClass = player.Value;
            ulong playerId = player.Key;

            if (!playerIDsSpawned.Contains(playerId))
            {
                switch (playerIDsSpawned.Count)
                {
                    case 0:
                        player1 = Instantiate(playerClass, spawnPoints[0].position, Quaternion.identity);
                        player1.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
                        playerIDsSpawned.Add(playerId);
                        break;

                    case 1:
                        player2 = Instantiate(playerClass, spawnPoints[1].position, Quaternion.identity);
                        player2.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
                        playerIDsSpawned.Add(playerId);
                        break;
                }
            }
        }
    }

    public void augmentLogic(){
        augmentChosing = true; //Start the augment choosing process
            // UI LOGIC to show the augment options to the player
            // Augment randomization (including which ones pop up and the stats they will give)
            // After selection, reset the buffer time
            augmentBuffer = 40f;
            augmentChosing = false; //End the augment choosing process
    }

    public void EndGame()
    {
        Debug.Log("Game Over!");
        // Add logic to handle end of the game (e.g., show results, restart, etc.)
    }
}