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

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Subscribing to NetworkManager callbacks in OnEnable.");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
            Debug.LogWarning("NetworkManager.Singleton is null in OnEnable. Ensure InitializeNetworkCallbacks is called after starting the host/server.");
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Unsubscribing from NetworkManager callbacks.");
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void Start()
    {
        Debug.Log("Game Manager Initialized");
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer) // Ensure this runs only on the server
        {
            
            //Debug.Log("Player Count: " + playerList.Count); // Debug log for player count
            if (playerList.Count == maxPlayers) // Check if the maximum number of players is reached
            {
                // Start the game logic here
                //Debug.Log("Game Starting with " + playerList.Count + " players.");
                spawnChampions(); // Spawn champions for both players
                if (augmentChosing)
                {
                    // Augment logic
                }
                else if (gameTime > 0)
                {
                    gameTime -= Time.deltaTime;
                }
                else
                {
                    EndGame();
                }

                if (augmentBuffer > 0)
                {
                    augmentBuffer -= Time.deltaTime;
                }
                else
                {
                    augmentLogic();
                }
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
        playerCount++; // Increment player count
        playerList.Add("Client", NetworkManager.Singleton.LocalClientId); // Add the local client ID to the player list
    }

    public void InitializeNetworkCallbacks()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Subscribing to NetworkManager callbacks.");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null. Ensure the NetworkManager is active in the scene.");
        }
    }

    public void spawnChampions()
    {
        if (!NetworkManager.Singleton.IsServer) // Ensure only the server can execute this
        {
            Debug.LogWarning("Only the server can spawn champions!");
            return;
        }
        Debug.Log(playerChampions.Count + " players in the game. Spawning champions.");
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
                        Debug.Log($"Spawned champion for Player 1 (Client {playerId}).");
                        break;

                    case 1:
                        player2 = Instantiate(playerClass, spawnPoints[1].position, Quaternion.identity);
                        player2.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
                        playerIDsSpawned.Add(playerId);
                        Debug.Log($"Spawned champion for Player 2 (Client {playerId}).");
                        break;

                    default:
                        Debug.LogWarning("No available spawn points for additional players.");
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

    [ServerRpc(RequireOwnership = false)]
    public void AddClientToGameServerRpc(ulong clientID, GameObject champChoice){
        Debug.Log($"Server received request from Client {clientID} to join");
        if (NetworkManager.Singleton.IsServer) // Ensure this runs only on the server
        {
            playerChampions.Add(clientID, champChoice); // Add the player prefab to the player list
            Debug.Log($"Client {clientID} added to game with champion {champChoice.name}.");
        }
        else
        {
            Debug.LogWarning("Only the server can add clients to the game!");
        }

    }

    
}