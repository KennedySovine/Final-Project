using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using TMPro;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance; // Singleton instance

    //public NetworkManager networkManager; // Reference to the NetworkManager

    [Header("Player Class Prefabs")]
    public List<GameObject> playerPrefabsList = new List<GameObject>(); // List of player prefabs
    public GameObject ghostBulletPrefab;

    [Header("Player References")]
    public GameObject player1;
    public GameObject player1Controller; // Reference to the player controller for player 1
    public GameObject player2;
    public GameObject player2Controller; // Reference to the player controller for player 2
    public NetworkList<int> player1Augments = new NetworkList<int>();
    public NetworkList<int> player2Augments = new NetworkList<int>();

    public Ability player1AbilityUsed = null; // Reference to the ability used by player 1
    public Ability player2AbilityUsed = null; // Reference to the ability used by player 2

    [Header("Server Settings")]
    public Dictionary<ulong, GameObject> playerChampions = new Dictionary<ulong, GameObject>(); // Dictionary to store player prefabs and connect it to the client ID
    public List<ulong> playerIDsSpawned = new List<ulong>(); // List of player IDs that have spawned champions
    public NetworkVariable<bool> hostReady = new NetworkVariable<bool>(false);
    private bool playerSpawningStart = false;
    public ulong ServerID = 3; // ID of the server
    public ulong player1ID = 0; // ID of player 1
    public ulong player2ID = 0; // ID of player 2

    [Header("Game Settings")]
    private bool gameEnded = false; // Flag to indicate if the game has ended
    public int playerCount = 0; // Number of players connected
    public int maxPlayers = 2;
    public NetworkVariable<bool> gamePaused = new NetworkVariable<bool>(false); // Flag to pause the game time
    [SerializeField] private float maxGameTime;
    public float gameTime = 120f; // Game duration in seconds
    public float augmentBuffer = 20f; //Choose aug every 40 seconds
    public NetworkVariable<bool> augmentChoosing = new NetworkVariable<bool>(false); //If the player is choosing an augment, dont countdown the game time
    private Camera serverCamera; // Reference to the server camera

    [Header("Champion Management")]
    public GameObject championPrefab; // Prefab for spawning champions
    public Transform[] spawnPoints; // Array of spawn points for champions

    [Header("Managers")]
    public AugmentManager AM; // Reference to the AugmentManager
    public InGameManager IGM; // Reference to the InGameManager
    public InGameUIManager IGUIM; // Reference to the InGameUIManager

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
        maxGameTime = gameTime; // Set the maximum game time
    }

    private void Update()
    {
        playerCount = playerChampions.Count; // Update player count

        if (IsServer || IsHost) // Ensure this runs only on the server
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

                if (augmentBuffer <= 0)
                {
                    augmentChoosing.Value = true;
                }

                if (augmentChoosing.Value)
                {
                    gamePaused.Value = true;
                }

                if (gameTime > 0 && !gamePaused.Value)
                {
                    gameTime -= Time.deltaTime;
                }

                if (augmentBuffer > 0 && !augmentChoosing.Value && !gamePaused.Value) // If Augment buffer is greater than 0, players are not choosing augments, and the game isn't paused.
                {
                    augmentBuffer -= Time.deltaTime;
                }
                else if (augmentChoosing.Value) // Ensure this block runs only once when augmentChoosing is false
                {
                    Debug.Log("Loading Augments for Player 1: " + player1ID);
                    loadAugmentsRpc(RpcTarget.Single(player1ID, RpcTargetUse.Temp));
                    Debug.Log("Loading Augments for Player 2: " + player2ID);
                    loadAugmentsRpc(RpcTarget.Single(player2ID, RpcTargetUse.Temp));

                    augmentChoosing.Value = false;
                    augmentBuffer = 30f; // Reset the augment buffer for the next cycle
                }
            }
            if (gameTime <= 0 && !gameEnded) // Check if the game time has expired
            {
                gamePaused.Value = true; // Pause the game
                gameEnded = true; // Set the game ended flag to true
                if (!IsServer) return; // Ensure this runs only on the server
                EndGame(); // Call the EndGame function to handle game over logic
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
        if (!IsServer) // Ensure only the server can execute this
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
                        player1Controller.GetComponent<PlayerNetwork>().targetPositionNet.Value = spawnPoints[0].position; // Set the target position for player 1
                        Debug.Log($"Spawned champion for Player 1 (Client {playerId}).");
                        break;

                    case 1:
                        player2 = Instantiate(playerClass, spawnPoints[1].position, Quaternion.identity);
                        findPlayerControllers(player2, ref player2Controller); // Find the PlayerController for player 2
                        player2.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
                        playerIDsSpawned.Add(playerId);
                        player2ID = playerId; // Store the ID of player 2
                        player2Controller.GetComponent<PlayerNetwork>().targetPositionNet.Value = spawnPoints[1].position; // Set the target position for player 2
                        Debug.Log($"Spawned champion for Player 2 (Client {playerId}).");
                        break;

                    default:
                        Debug.LogWarning("No available spawn points for additional players.");
                        break;
                }
            }

            if (player1 != null && player2 != null) // Check if both players have been spawned
            {
                Debug.Log("Both players have been spawned. Starting the game.");
                // Add any additional logic to start the game here
                player1Controller.GetComponent<BaseChampion>().enemyChampion = player2Controller;
                player2Controller.GetComponent<BaseChampion>().enemyChampion = player1Controller; // Set the enemy champion reference for both players

                player1Controller.GetComponent<BaseChampion>().enemyChampionId.Value = player2.GetComponent<NetworkObject>().OwnerClientId; // Set the player ID for player 1
                player2Controller.GetComponent<BaseChampion>().enemyChampionId.Value = player1.GetComponent<NetworkObject>().OwnerClientId; // Set the player ID for player 2

                initializeIGUIMRpc(RpcTarget.Single(player1ID, RpcTargetUse.Temp)); // Initialize the InGameUIManager for player 1
                initializeIGUIMRpc(RpcTarget.Single(player2ID, RpcTargetUse.Temp)); // Initialize the InGameUIManager for player 2
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
            hostReady.Value = true;
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
        List<Augment> player1Aug = new List<Augment>(); // Create a new list to store player 1's augments
        List<Augment> player2Aug = new List<Augment>(); // Create a new list to store player 2's augments
        foreach (int augmentID in player1Augments)
        {
            Augment augment = AM.augmentFromID(augmentID);
            if (augment != null)
            {
                player1Aug.Add(augment); // Add the augment to player 1's list
            }
        }
        foreach (int augmentID in player2Augments)
        {
            Augment augment = AM.augmentFromID(augmentID);
            if (augment != null)
            {
                player2Aug.Add(augment); // Add the augment to player 2's list
            }
        }

        player1Controller.GetComponent<BaseChampion>().passive.Stats.endGameCalculations(player1Aug, maxGameTime); // Call the endGameCalculations method for player 1's champion
        player2Controller.GetComponent<BaseChampion>().passive.Stats.endGameCalculations(player2Aug, maxGameTime); // Call the endGameCalculations method for player 2's champion

        player1Controller.GetComponent<BaseChampion>().ability1.Stats.endGameCalculations(player1Aug, maxGameTime); // Call the endGameCalculations method for player 1's ability
        player2Controller.GetComponent<BaseChampion>().ability1.Stats.endGameCalculations(player2Aug, maxGameTime); // Call the endGameCalculations method for player 2's ability
        player1Controller.GetComponent<BaseChampion>().ability2.Stats.endGameCalculations(player1Aug, maxGameTime); // Call the endGameCalculations method for player 1's ability
        player2Controller.GetComponent<BaseChampion>().ability2.Stats.endGameCalculations(player2Aug, maxGameTime); // Call the endGameCalculations method for player 2's ability
        player1Controller.GetComponent<BaseChampion>().ability3.Stats.endGameCalculations(player1Aug, maxGameTime); // Call the endGameCalculations method for player 1's ability
        player2Controller.GetComponent<BaseChampion>().ability3.Stats.endGameCalculations(player2Aug, maxGameTime); // Call the endGameCalculations method for player 2's ability
    }
    public void applyAugments(ulong playerID)
    {
        BaseChampion targetChampion = null;

        // Determine which player's champion to update
        if (playerID == player1ID)
        {
            targetChampion = player1Controller.GetComponent<BaseChampion>();
            if (player1Augments.Count == 0) return; // Ensure there are augments to apply
        }
        else if (playerID == player2ID)
        {
            targetChampion = player2Controller.GetComponent<BaseChampion>();
            if (player2Augments.Count == 0) return; // Ensure there are augments to apply
        }
        else
        {
            Debug.LogWarning($"Player ID {playerID} not found. Cannot apply augments.");
            return;
        }

        targetChampion.passive.Stats.saveBetweenAugments();

        // Get the last augment chosen by the player
        int augmentID = (playerID == player1ID) 
            ? player1Augments[player1Augments.Count - 1] 
            : player2Augments[player2Augments.Count - 1];
        Augment newAugment = AM.augmentFromID(augmentID);

        if (newAugment == null)
        {
            Debug.LogWarning($"Augment with ID {augmentID} not found.");
            return;
        }

        // Calculate the random adjustment value
        float randomAdjustment = newAugment.max;
        if (newAugment.min != newAugment.max)
        {
            randomAdjustment = Random.Range(newAugment.min, newAugment.max + 1); // Inclusive range
            if (randomAdjustment >= 1){ // Ignore % based adjustment from being rounded
                randomAdjustment = Mathf.Round(randomAdjustment); // Round to the nearest whole number
            }

        }

        // Apply the augment effect based on its type
        switch (newAugment.type)
        {
            case "AbilityHaste":
                targetChampion.updateAbilityHasteRpc(randomAdjustment);
                break;
            case "Armor":
                targetChampion.updateArmorRpc(randomAdjustment);
                break;
            case "AttackDamage":
                targetChampion.updateADRpc(randomAdjustment);
                break;
            case "AbilityPower":
                targetChampion.updateAPRpc(randomAdjustment);
                break;
            case "Health":
                targetChampion.updateMaxHealthRpc(randomAdjustment);
                break;
            case "AttackSpeed":
                targetChampion.updateAttackSpeedRpc(randomAdjustment);
                break;
            case "CriticalStrike":
                targetChampion.updateCritChanceRpc(randomAdjustment);
                break;
            case "CriticalDamage":
                targetChampion.updateCritDamageRpc(randomAdjustment);
                break;
            case "ArmorPenetration":
                targetChampion.updateArmorPenRpc(randomAdjustment);
                break;
            case "MagicPenetration":
                targetChampion.updateMagicPenRpc(randomAdjustment);
                break;
            case "MagicResist":
                targetChampion.updateMagicResistRpc(randomAdjustment);
                break;
            default:
                Debug.LogWarning($"Unknown augment type: {newAugment.type}");
                break;
        }

        Debug.Log($"Applied augment {newAugment.name} to player {playerID} with adjustment {randomAdjustment}.");
    }

    //Add Augments to UI for Choosing
    // Send to specified clients only
    [Rpc(SendTo.SpecifiedInParams)]
    public void loadAugmentsRpc(RpcParams rpcParams){
        Debug.Log("Loading Augments for Client " + NetworkManager.Singleton.LocalClientId); // Log the client ID for debugging
        AM.augmentUI.SetActive(true); // Show the augment UI
        AM.augmentUISetup(AM.augmentSelector()); // Get the list of chosen augments
    }

    [Rpc(SendTo.Server)]
    public void updatePlayerAbilityUsedRpc(ulong playerID, string abilityKey)
    {
        if (!IsServer) return; // Ensure this runs only on the server

        BaseChampion playerChampion = null;

        // Determine which player's champion to update
        if (playerID == player1ID)
        {
            playerChampion = player1Controller.GetComponent<BaseChampion>();
        }
        else if (playerID == player2ID)
        {
            playerChampion = player2Controller.GetComponent<BaseChampion>();
        }
        else
        {
            Debug.LogWarning($"Invalid player ID: {playerID}.");
            return;
        }

        // Update the ability used based on the ability key
        switch (abilityKey)
        {
            case "Q":
                if (playerID == player1ID)
                    player1AbilityUsed = playerChampion.ability1;
                else
                    player2AbilityUsed = playerChampion.ability1;
                break;

            case "W":
                if (playerID == player1ID)
                    player1AbilityUsed = playerChampion.ability2;
                else
                    player2AbilityUsed = playerChampion.ability2;
                break;

            case "E":
                if (playerID == player1ID)
                    player1AbilityUsed = playerChampion.ability3;
                else
                    player2AbilityUsed = playerChampion.ability3;
                break;

            default:
                Debug.LogWarning($"Invalid ability key: {abilityKey} for player {playerID}.");
                return;
        }

        // Log the ability used
        Ability abilityUsed = (playerID == player1ID) ? player1AbilityUsed : player2AbilityUsed;
        Debug.Log($"Player {playerID} used ability: {abilityUsed.name}");
    }
    [Rpc(SendTo.SpecifiedInParams)]
    public void initializeIGUIMRpc(RpcParams rpcParams)
    {
        IGUIM.inGameUI.SetActive(true); // Activate the in-game UI
        Debug.Log("In-game UI initialized and activated.");
    }


}