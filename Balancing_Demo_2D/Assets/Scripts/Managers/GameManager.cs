using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance; // Singleton instance

    //public NetworkManager networkManager; // Reference to the NetworkManager

    [Header("Player Class Prefabs")]
    public List<GameObject> playerPrefabsList = new List<GameObject>(); // List of player prefabs

    [Header("Player References")]
    public GameObject player1;
    public GameObject player1Controller; // Reference to the player controller for player 1
    public GameObject player2;
    public GameObject player2Controller; // Reference to the player controller for player 2
    public List<int> player1Augments = new List<int>();
    public List<int> player2Augments = new List<int>();

    [Header("Server Settings")]
    public Dictionary<ulong, GameObject> playerChampions = new Dictionary<ulong, GameObject>(); // Dictionary to store player prefabs and connect it to the client ID
    public List<ulong> playerIDsSpawned = new List<ulong>(); // List of player IDs that have spawned champions
    private bool playerSpawningStart = false;
    public ulong ServerID = 3; // ID of the server
    public ulong player1ID = 0; // ID of player 1
    public ulong player2ID = 0; // ID of player 2

    [Header("Game Settings")]
    public int playerCount = 0; // Number of players connected
    public int maxPlayers = 2;
    public bool gamePaused = false; // Flag to pause the game time
    public float gameTime = 120f; // Game duration in seconds
    public float augmentBuffer = 20f; //Choose aug every 40 seconds
    public bool augmentChoosing = false; //If the player is choosing an augment, dont countdown the game time

    [Header("Champion Management")]
    public GameObject championPrefab; // Prefab for spawning champions
    public Transform[] spawnPoints; // Array of spawn points for champions

    private Camera serverCamera; // Reference to the server camera
    public AugmentManager AM; // Reference to the AugmentManager
    public InGameManager IGM; // Reference to the InGameManager

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

        /*networkManager = FindObjectOfType<NetworkManager>(); // Find the NetworkManager in the scene
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found in the scene. Ensure it is present.");
        }*/
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
        playerCount = playerChampions.Count; // Update player count

        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost) // Ensure this runs only on the server
        {
            
            //Debug.Log("Player Count: " + playerCount); // Debug log for player count
            if (playerCount == maxPlayers) // Check if the maximum number of players is reached
            {
                // Start the game logic here
                //Debug.Log("Game Starting with " + playerCount + " players.");

                if (!playerSpawningStart) // Check if champions have not been spawned yet
                {
                    Debug.Log("Spawning champions for players.");
                    spawnChampions(); // Spawn champions for both players
                    playerSpawningStart = true; // Set the flag to true to prevent multiple spawns
                }
                else
                {
                    //Debug.Log("Champions already spawned. Waiting for game time to end.");
                }
                
                if (augmentChoosing)
                {
                    gamePaused = true; // Pause the game time while choosing an augment

                }
                else if (gameTime > 0 && !gamePaused)
                {
                    gameTime -= Time.deltaTime;
                }
                else
                {
                    EndGame();
                }

                if (augmentBuffer > 0 && !augmentChoosing && !gamePaused) // If Augment buffer is greater than 0, players are not choosing augments, and the game isn't paused.
                {
                    augmentBuffer -= Time.deltaTime;
                }
                else if (!augmentChoosing) // Ensure this block runs only once when augmentChoosing is false
                {
                    Debug.Log("Loading Augments for Player 1: " + player1ID);
                    loadAugmentsRpc(RpcTarget.Single(player1ID, RpcTargetUse.Temp));
                    Debug.Log("Loading Augments for Player 2: " + player2ID);
                    loadAugmentsRpc(RpcTarget.Single(player2ID, RpcTargetUse.Temp));

                    augmentChoosing = true; // Start the augment choosing process
                    augmentBuffer = 20f; // Reset the augment buffer for the next cycle
                }
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
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
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost) // Ensure only the server can execute this
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
                        findPlayerControllers(player1, ref player1Controller); // Find the PlayerController for player 1
                        player1.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
                        playerIDsSpawned.Add(playerId);
                        player1ID = playerId; // Store the ID of player 1
                        Debug.Log($"Spawned champion for Player 1 (Client {playerId}).");
                        break;

                    case 1:
                        player2 = Instantiate(playerClass, spawnPoints[1].position, Quaternion.identity);
                        findPlayerControllers(player2, ref player2Controller); // Find the PlayerController for player 2
                        player2.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
                        playerIDsSpawned.Add(playerId);
                        player2ID = playerId; // Store the ID of player 2
                        Debug.Log($"Spawned champion for Player 2 (Client {playerId}).");
                        break;

                    default:
                        Debug.LogWarning("No available spawn points for additional players.");
                        break;
                }
            }
        }
    }

    public void EnableServerObserverMode()
    {
        if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
        {
            if (serverCamera == null)
            {
                // Find the camera even if it's inactive
                serverCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
            }

            if (serverCamera != null)
            {
                serverCamera.enabled = true;
                Debug.Log("Server observer mode enabled.");
            }
            else
            {
                Debug.LogWarning("No camera found for the server.");
            }
        }
    }

    private void findPlayerControllers(GameObject parent, ref GameObject controller)
    {
        Transform childTransform = parent.transform.Find("PlayerController");
        if (childTransform != null)
        {
            controller = childTransform.gameObject; // Assign the found child object to the controller variable
            Debug.Log("Found PlayerController: " + controller.name);
        }
        else
        {
            Debug.LogWarning("PlayerController not found in " + parent.name);
        }
    }

    public void augmentLogic(){
        augmentChoosing = true; //Start the augment choosing process
            // UI LOGIC to show the augment options to the player
            // Augment randomization (including which ones pop up and the stats they will give)
            // After selection, reset the buffer time
            augmentBuffer = 40f;
            augmentChoosing = false; //End the augment choosing process
    }

    public void EndGame()
    {
        Debug.Log("Game Over!");
        // Add logic to handle end of the game (e.g., show results, restart, etc.)
    }

    //Add Augments to UI for Choosing
    // Send to specified clients only
    [Rpc(SendTo.SpecifiedInParams)]
    public void loadAugmentsRpc(RpcParams rpcParams){
        Debug.Log("Loading Augments for Client " + NetworkManager.Singleton.LocalClientId); // Log the client ID for debugging
        AM.augmentUI.SetActive(true); // Show the augment UI
        AM.augmentUISetup(AM.augmentSelector()); // Get the list of chosen augments
    }
}