using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using System.Linq;

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

                if (augmentBuffer <=0){
                    augmentChoosing = true;
                }
                
                if (augmentChoosing)
                {
                    gamePaused = true; // Pause the game time while choosing an augment

                }
                if (gameTime > 0 && !gamePaused)
                {
                    gameTime -= Time.deltaTime;
                }

                if (augmentBuffer > 0 && !augmentChoosing && !gamePaused) // If Augment buffer is greater than 0, players are not choosing augments, and the game isn't paused.
                {
                    augmentBuffer -= Time.deltaTime;
                }
                else if (augmentChoosing) // Ensure this block runs only once when augmentChoosing is false
                {
                    Debug.Log("Loading Augments for Player 1: " + player1ID);
                    loadAugmentsRpc(RpcTarget.Single(player1ID, RpcTargetUse.Temp));
                    Debug.Log("Loading Augments for Player 2: " + player2ID);
                    loadAugmentsRpc(RpcTarget.Single(player2ID, RpcTargetUse.Temp));

                    augmentChoosing = false;
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

    public void EndGame()
    {
        Debug.Log("Game Over!");
        // Add logic to handle end of the game (e.g., show results, restart, etc.)
    }

    //Function to update player stats on the client side


    public void applyAugments(ulong playerID){

        if (playerID == player1ID) // Check if the player ID matches player 1
        {
            Augment newAugment = AM.augmentFromID(player1Augments.Last()); // Get the last augment chosen by player 1
            float randomAdjustment = newAugment.max; // Default adjustment value
    
            if (newAugment.min != newAugment.max)
            {
                randomAdjustment = Random.Range(newAugment.min, newAugment.max); // Get a random adjustment value between min and max
            }
    
            switch (newAugment.type)
            {
                case "AbilityHaste":
                    player1Controller.GetComponent<BaseChampion>().updateAbilityHaste(randomAdjustment); // Increase ability haste for player 1
                    break;
                case "Armor":
                    player1Controller.GetComponent<BaseChampion>().updateArmor(randomAdjustment); // Increase armor for player 1
                    break;
                case "AttackDamage":
                    player1Controller.GetComponent<BaseChampion>().updateAD(randomAdjustment); // Increase AD for player 1
                    break;
                case "AbilityPower":
                    player1Controller.GetComponent<BaseChampion>().updateAP(randomAdjustment); // Increase AP for player 1
                    break;
                case "Health":
                    player1Controller.GetComponent<BaseChampion>().updateMaxHeath(randomAdjustment); // Increase max health for player 1
                    break;
                case "AttackSpeed":
                    player1Controller.GetComponent<BaseChampion>().updateAttackSpeed(randomAdjustment); // Increase attack speed for player 1
                    break;
                case "CriticalStrike": // crit chance
                    player1Controller.GetComponent<BaseChampion>().updateCritChance(randomAdjustment); // Increase crit chance for player 1
                    break;
                case "CriticalDamage": // crit damage
                    player1Controller.GetComponent<BaseChampion>().updateCritDamage(randomAdjustment); // Increase crit damage for player 1
                    break;
                case "ArmorPenitration":
                    player1Controller.GetComponent<BaseChampion>().updateArmorPen(randomAdjustment); // Increase armor pen for player 1
                    break;
                case "MagicPenitration":
                    player1Controller.GetComponent<BaseChampion>().updateMagicPen(randomAdjustment); // Increase magic pen for player 1
                    break;
                case "MagicResist":
                    player1Controller.GetComponent<BaseChampion>().updateMagicResist(randomAdjustment); // Increase MR for player 1
                    break;
                default:
                    Debug.LogWarning("Unknown augment type: " + newAugment.type); // Log a warning for unknown augment types
                    break;
            }
        }
        else if (playerID == player2ID){
            Augment newAugment = AM.augmentFromID(player2Augments.Last()); // Get the last augment chosen by player 1
            float randomAdjustment = newAugment.max; // Default adjustment value
    
            if (newAugment.min != newAugment.max)
            {
                randomAdjustment = Random.Range(newAugment.min, newAugment.max); // Get a random adjustment value between min and max
            }
    
            switch (newAugment.type)
            {
                case "AbilityHaste":
                    player2Controller.GetComponent<BaseChampion>().updateAbilityHaste(randomAdjustment); // Increase ability haste for player 1
                    break;
                case "Armor":
                    player2Controller.GetComponent<BaseChampion>().updateArmor(randomAdjustment); // Increase armor for player 1
                    break;
                case "AttackDamage":
                    player2Controller.GetComponent<BaseChampion>().updateAD(randomAdjustment); // Increase AD for player 1
                    break;
                case "AbilityPower":
                    player2Controller.GetComponent<BaseChampion>().updateAP(randomAdjustment); // Increase AP for player 1
                    break;
                case "Health":
                    player2Controller.GetComponent<BaseChampion>().updateMaxHeath(randomAdjustment); // Increase max health for player 1
                    break;
                case "AttackSpeed":
                    player2Controller.GetComponent<BaseChampion>().updateAttackSpeed(randomAdjustment); // Increase attack speed for player 1
                    break;
                case "CriticalStrike": // crit chance
                    player2Controller.GetComponent<BaseChampion>().updateCritChance(randomAdjustment); // Increase crit chance for player 1
                    break;
                case "CriticalDamage": // crit damage
                    player2Controller.GetComponent<BaseChampion>().updateCritDamage(randomAdjustment); // Increase crit damage for player 1
                    break;
                case "ArmorPenitration":
                    player2Controller.GetComponent<BaseChampion>().updateArmorPen(randomAdjustment); // Increase armor pen for player 1
                    break;
                case "MagicPenitration":
                    player2Controller.GetComponent<BaseChampion>().updateMagicPen(randomAdjustment); // Increase magic pen for player 1
                    break;
                case "MagicResist":
                    player2Controller.GetComponent<BaseChampion>().updateMagicResist(randomAdjustment); // Increase MR for player 1
                    break;
                default:
                    Debug.LogWarning("Unknown augment type: " + newAugment.type); // Log a warning for unknown augment types
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Player ID not found in the game. Cannot apply augments.");
        }
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