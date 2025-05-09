using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    #region Singleton and References
    public static GameManager Instance; // Singleton instance

    [Header("Managers")]
    public AugmentManager AM; // Reference to the AugmentManager
    public InGameManager IGM; // Reference to the InGameManager
    public InGameUIManager IGUIM; // Reference to the InGameUIManager
    #endregion

    #region Player Prefabs and References
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
    #endregion

    #region Networking and Server Settings
    [Header("Server Settings")]
    public Dictionary<ulong, GameObject> playerChampions = new Dictionary<ulong, GameObject>(); // Dictionary to store player prefabs and connect it to the client ID
    public List<ulong> playerIDsSpawned = new List<ulong>(); // List of player IDs that have spawned champions
    public NetworkVariable<bool> hostReady = new NetworkVariable<bool>(false);
    private bool playerSpawningStart = false;
    public ulong ServerID = 3; // ID of the server
    public ulong player1ID = 0; // ID of player 1
    public ulong player2ID = 0; // ID of player 2
    public NetworkVariable<bool> playersSpawned = new NetworkVariable<bool>(false); // Flag to indicate if player 1 is ready
    private Camera serverCamera; // Reference to the server camera
    #endregion

    #region Game State and Settings
    [Header("Game Settings")]
    private bool gameEnded = false; // Flag to indicate if the game has ended
    public int playerCount = 0; // Number of players connected
    public int maxPlayers = 2;
    public NetworkVariable<bool> gamePaused = new NetworkVariable<bool>(false); // Flag to pause the game time
    [SerializeField] private float maxGameTime;
    public NetworkVariable<float> gameTime = new NetworkVariable<float>(60f); // Game time in seconds
    public float augmentBuffer = 20f; //Choose aug every 40 seconds
    public NetworkVariable<bool> augmentChoosing = new NetworkVariable<bool>(false); //If the player is choosing an augment, dont countdown the game time
    #endregion

    #region Champion Management
    [Header("Champion Management")]
    public GameObject championPrefab; // Prefab for spawning champions
    public Transform[] spawnPoints; // Array of spawn points for champions
    public int recievedCalcs = 0;
    private bool recievedEndGameCalculations => recievedCalcs >= 2;
    #endregion

    #region Unity Lifecycle Methods
    private void Awake()
    {
        InitializeSingleton();
    }

    private void OnEnable()
    {
        SubscribeToNetworkEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromNetworkEvents();
    }

    private void Start()
    {
        Debug.Log("Game Manager Initialized");
        maxGameTime = gameTime.Value; // Set the maximum game time
    }

    private void Update()
    {
        UpdatePlayerCount();
        
        if (!IsServer && !IsHost) return;
        
        HandleGameLogic();
    }
    #endregion

    #region Initialization Methods
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SubscribeToNetworkEvents()
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

    private void UnsubscribeFromNetworkEvents()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Unsubscribing from NetworkManager callbacks.");
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    public void InitializeNetworkCallbacks()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Subscribing to NetworkManager callbacks.");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null. Ensure the NetworkManager is active in the scene.");
        }
    }

    public void ResetPlayerStats(){
        if (!IsServer) return; // Only the server can clear player stats
        string filePath = Path.Combine(Application.persistentDataPath, "Resources/PlayerStats.json");
        try
        {
            // Overwrite the file with an empty stats array
            File.WriteAllText(filePath, "{ \"stats\": [] }");
            Debug.Log("Player stats wiped (file overwritten with empty stats array).");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to wipe PlayerStats.json: " + ex.Message);
        }
    }
    #endregion

    #region Game Logic
    private void UpdatePlayerCount()
    {
        playerCount = playerChampions.Count;
    }

    private void HandleGameLogic()
    {
        if (playerCount == maxPlayers)
        {
            HandlePlayerSpawning();
            ManageAugmentSystem();
            UpdateGameTime();
        }
        
        HandleGameEnd();
    }

    private void HandlePlayerSpawning()
    {
        if (!playerSpawningStart)
        {
            Debug.Log("Spawning champions for players.");
            SpawnChampions();
            playerSpawningStart = true;
        }
    }

    private void ManageAugmentSystem()
    {
        if (augmentBuffer <= 0)
        {
            augmentChoosing.Value = true;
        }

        if (augmentChoosing.Value)
        {
            gamePaused.Value = true;
        }

        if (augmentBuffer > 0 && !augmentChoosing.Value && !gamePaused.Value)
        {
            augmentBuffer -= Time.deltaTime;
        }
        else if (augmentChoosing.Value)
        {
            LoadAugmentsForPlayers();
            augmentChoosing.Value = false;
            augmentBuffer = 15f;
        }
    }

    private void LoadAugmentsForPlayers()
    {
        Debug.Log("Loading Augments for Player 1: " + player1ID);
        LoadAugmentsRpc(RpcTarget.Single(player1ID, RpcTargetUse.Temp));
        Debug.Log("Loading Augments for Player 2: " + player2ID);
        LoadAugmentsRpc(RpcTarget.Single(player2ID, RpcTargetUse.Temp));
    }

    private void UpdateGameTime()
    {
        if (gameTime.Value > 0 && !gamePaused.Value)
        {
            gameTime.Value -= Time.deltaTime;
        }
    }

    private void HandleGameEnd()
    {
        if (gameTime.Value <= 0 && !gameEnded)
        {
            gamePaused.Value = true;
            gameEnded = true;
            if (IsServer) EndGame();
        }
    }
    #endregion

    #region Network Event Handlers
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
        // TODO: Handle different player conenction here
        // This would be like, a player leaving and then a new one joining. Maybe make them re-choose their champion?
    }

    //TODO: Finish this function to handle player disconnects properly
    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected.");
        if (playerChampions.ContainsKey(clientId))
        {
            RemoveDisconnectedPlayer(clientId);
        }
    }

    private void RemoveDisconnectedPlayer(ulong clientId)
    {
        playerChampions.Remove(clientId);
        Debug.Log($"Removed player {clientId} from playerChampions.");
        playerIDsSpawned.Remove(clientId);
        Debug.Log($"Removed player {clientId} from playerIDsSpawned.");
        playerCount--;
        Debug.Log($"Player count decreased. Current count: {playerCount}.");
        
        HandlePlayerDisconnectGameState(clientId);
    }

    private void HandlePlayerDisconnectGameState(ulong clientId)
    {
        if (playerIDsSpawned.Count == 0)
        {
            gameEnded = true;
            Debug.Log("All players disconnected. Ending game.");
            // Basically, I want the server to reset the game if all players disconnect so they dont have to restart everything and reselect server yada yada
            if (IsServer)
                if (gameEnded)
                    EndGame();
                else
                    ResetGame();
        }
        else if (playerIDsSpawned.Count == 1)
        {
            gamePaused.Value = true;
            // TODO: Pause game and do a pop up saying waiting for other player to reconnect.
        }
    }

    public void ReturnToMainMenu()
    {
        NetworkManager.Singleton.Shutdown();
        Debug.Log("Returning to main menu.");
        NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); // Load the game scene
    }

    public void ResetGame()
    {
        if (IsServer)
        {
            gameEnded = false;
            gameTime.Value = maxGameTime;
            playerIDsSpawned.Clear();
            playerChampions.Clear();
            playerCount = 0;
            playersSpawned.Value = false;
            gamePaused.Value = false;
            augmentChoosing.Value = false;
            augmentBuffer = 20f;
        }
    }

    // Client code for when theyre disconnected from server
    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"The Server/Host has disconnected Client {clientId}.");
        NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); // Load the game scene
        NetworkManager.Singleton.Shutdown(); // Disconnect the client
    }
    #endregion

    #region Player and Champion Management
    public void SpawnChampions()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server can spawn champions!");
            return;
        }

        Debug.Log(playerChampions.Count + " players in the game. Spawning champions.");
    
        foreach (var player in playerChampions)
        {
            SpawnChampionForPlayer(player.Value, player.Key);
        }

        if (player1 != null && player2 != null) // If both players are spawned in
        {
            playersSpawned.Value = true;
            Debug.Log("Both players have been spawned. Starting the game.");
        
            SetupPlayerReferences();
            InitializeUIForPlayers();
        }
    }

    private void SpawnChampionForPlayer(GameObject playerClass, ulong playerId)
    {
        if (playerIDsSpawned.Contains(playerId))
            return;

        switch (playerIDsSpawned.Count)
        {
            case 0:
                player1 = Instantiate(playerClass, spawnPoints[0].position, Quaternion.identity);
                FindPlayerControllers(player1, ref player1Controller);
                player1.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
                playerIDsSpawned.Add(playerId);
                player1ID = playerId;
                player1Controller.GetComponent<PlayerNetwork>().targetPositionNet.Value = spawnPoints[0].position;
                Debug.Log($"Spawned champion for Player 1 (Client {playerId}).");
                break;
            case 1:
                player2 = Instantiate(playerClass, spawnPoints[1].position, Quaternion.identity);
                FindPlayerControllers(player2, ref player2Controller);
                player2.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
                playerIDsSpawned.Add(playerId);
                player2ID = playerId;
                player2Controller.GetComponent<PlayerNetwork>().targetPositionNet.Value = spawnPoints[1].position;
                Debug.Log($"Spawned champion for Player 2 (Client {playerId}).");
                break;
            default:
                Debug.LogWarning("No available spawn points for additional players.");
                break;
        }
    }

    private void SetupPlayerReferences()
    {
        player1Controller.GetComponent<BaseChampion>().enemyChampion = player2Controller;
        player2Controller.GetComponent<BaseChampion>().enemyChampion = player1Controller;

        player1Controller.GetComponent<BaseChampion>().enemyChampionId.Value = player2.GetComponent<NetworkObject>().OwnerClientId;
        player2Controller.GetComponent<BaseChampion>().enemyChampionId.Value = player1.GetComponent<NetworkObject>().OwnerClientId;
    }

    private void InitializeUIForPlayers()
    {
        InitializeIGUIMRpc(RpcTarget.Single(player1ID, RpcTargetUse.Temp));
        InitializeIGUIMRpc(RpcTarget.Single(player2ID, RpcTargetUse.Temp));
    }

    private void FindPlayerControllers(GameObject parent, ref GameObject controller)
    {
        Transform childTransform = parent.transform.Find("PlayerController");
        if (childTransform != null)
        {
            controller = childTransform.gameObject;
            Debug.Log("Found PlayerController: " + controller.name);
        }
        else
        {
            Debug.LogWarning("PlayerController not found in " + parent.name);
        }
    }
    #endregion

    #region Server Utilities
    public void EnableServerObserverMode()
    {
        if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
        {
            serverCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
            
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
    #endregion

    #region Augment System
    public void ApplyAugments(ulong playerID)
    {
        BaseChampion targetChampion = GetTargetChampion(playerID);
        if (targetChampion == null) return;

        if (!HasAugmentsToApply(playerID)) return;
        
        targetChampion.passive.Stats.SaveBetweenAugments();
        
        Augment newAugment = GetLastAugment(playerID);
        if (newAugment == null) return;
        
        float randomAdjustment = CalculateAugmentAdjustment(newAugment);
        ApplyAugmentEffect(targetChampion, newAugment.type, randomAdjustment);
        
        Debug.Log($"Applied augment {newAugment.name} to player {playerID} with adjustment {randomAdjustment}.");
    }

    private BaseChampion GetTargetChampion(ulong playerID)
    {
        if (playerID == player1ID)
            return player1Controller?.GetComponent<BaseChampion>();
        else if (playerID == player2ID)
            return player2Controller?.GetComponent<BaseChampion>();
        
        Debug.LogWarning($"Player ID {playerID} not found. Cannot apply augments.");
        return null;
    }

    private bool HasAugmentsToApply(ulong playerID)
    {
        return (playerID == player1ID && player1Augments.Count > 0) || 
               (playerID == player2ID && player2Augments.Count > 0);
    }

    private Augment GetLastAugment(ulong playerID)
    {
        int augmentID = (playerID == player1ID) 
            ? player1Augments[player1Augments.Count - 1] 
            : player2Augments[player2Augments.Count - 1];
            
        Augment newAugment = AM.AugmentFromID(augmentID);
        
        if (newAugment == null)
            Debug.LogWarning($"Augment with ID {augmentID} not found.");
            
        return newAugment;
    }

    private float CalculateAugmentAdjustment(Augment augment)
    {
        // If min equals max, just return the value (no randomness needed)
        if (augment.min == augment.max) return augment.max;
            
        // Check if the values appear to be integers
        bool isInteger = Mathf.Approximately(augment.min, Mathf.Round(augment.min)) && Mathf.Approximately(augment.max, Mathf.Round(augment.max));
        
        float adjustment;
        if (isInteger)
        {
            // For integer values (like flat health, damage, etc.)
            // Get a random integer between min (inclusive) and max (inclusive)
            adjustment = Mathf.Floor(Random.Range(augment.min, augment.max + 0.999f));
        }
        else
        {
            // For float values (like percentage multipliers)
            // Get a random float between min and max (both inclusive)
            adjustment = Random.Range(augment.min, augment.max);
            
            // For small decimal values (likely percentages), keep 2 decimal places
            if (augment.max < 1)
                adjustment = Mathf.Round(adjustment * 100) / 100f; // Round to 2 decimal places
            else
                adjustment = Mathf.Round(adjustment * 10) / 10f;   // Round to 1 decimal place
        }
        
        return adjustment;
    }

    private void ApplyAugmentEffect(BaseChampion champion, string augmentType, float value)
    {
        switch (augmentType)
        {
            case "AbilityHaste": champion.UpdateAbilityHasteRpc(value); break;
            case "Armor": champion.UpdateArmorRpc(value); break;
            case "AttackDamage": champion.UpdateADRpc(value); break;
            case "AbilityPower": champion.UpdateAPRpc(value); break;
            case "Health": champion.UpdateMaxHealthRpc(value); break;
            case "AttackSpeed": champion.UpdateAttackSpeedRpc(value); break;
            case "CriticalStrike": champion.UpdateCritChanceRpc(value); break;
            case "CriticalDamage": champion.UpdateCritDamageRpc(value); break;
            case "ArmorPenetration": champion.UpdateArmorPenRpc(value); break;
            case "MagicPenetration": champion.UpdateMagicPenRpc(value); break;
            case "MagicResist": champion.UpdateMagicResistRpc(value); break;
            default: Debug.LogWarning($"Unknown augment type: {augmentType}"); break;
        }
    }

    // Utility: Round a float to a specific number of decimal places
    public static float RoundToDecimals(float value, int decimals)
    {
        float multiplier = Mathf.Pow(10, decimals);
        return Mathf.Round(value * multiplier) / multiplier;
    }
    #endregion

    #region Game End Logic
    public void EndGame()
    {
        Debug.Log("Game Over!");
        
        List<Augment> player1Aug = ConvertAugmentIdsToAugments(player1Augments);
        List<Augment> player2Aug = ConvertAugmentIdsToAugments(player2Augments);
        
        if (!IsServer) return;
        
        StartCoroutine(WaitForEndGameStats());
        ProcessEndGameCalculationsForChampions();
    }

    private List<Augment> ConvertAugmentIdsToAugments(NetworkList<int> augmentIds)
    {
        List<Augment> augments = new List<Augment>();
        foreach (int augmentID in augmentIds)
        {
            Augment augment = AM.AugmentFromID(augmentID);
            if (augment != null)
            {
                augments.Add(augment);
            }
        }
        return augments;
    }

    private void ProcessEndGameCalculationsForChampions()
    {
        List<Augment> player1Aug = ConvertAugmentIdsToAugments(player1Augments);
        List<Augment> player2Aug = ConvertAugmentIdsToAugments(player2Augments);
        
        // Process passive stats
        player1Controller.GetComponent<BaseChampion>().passive.Stats.EndGameCalculations(player1Aug, maxGameTime);
        player2Controller.GetComponent<BaseChampion>().passive.Stats.EndGameCalculations(player2Aug, maxGameTime);
        
        // Process abilities stats
        ProcessAbilityEndGameCalculations(player1Controller, player1Aug);
        ProcessAbilityEndGameCalculations(player2Controller, player2Aug);
    }

    private void ProcessAbilityEndGameCalculations(GameObject playerController, List<Augment> augments)
    {
        BaseChampion champion = playerController.GetComponent<BaseChampion>();
        champion.ability1.Stats.EndGameCalculations(augments, maxGameTime);
        champion.ability2.Stats.EndGameCalculations(augments, maxGameTime);
        champion.ability3.Stats.EndGameCalculations(augments, maxGameTime);
    }

    private IEnumerator WaitForEndGameStats()
    {
        // Request stats from clients
        RequestEndGameStatsRpc();
        
        float timeout = 10f; // Timeout after 10 seconds
        float elapsed = 0f;
        
        while (!recievedEndGameCalculations && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // If we timed out, force proceed
        if (!recievedEndGameCalculations)
        {
            Debug.LogWarning("End game calculations timed out, proceeding anyway");
        }
        
        IGM.endGameUI.statsToList();
        EndGameUIRpc();
    }
    #endregion

    #region RPC Methods
    [Rpc(SendTo.Everyone)]
    public void RequestEndGameStatsRpc()
    {
        if (!IsServer && IsOwner)
        {
            // Clients send their stats to the server
            SubmitPlayerStatsToServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
    
    [Rpc(SendTo.Server)]
    public void SubmitPlayerStatsToServerRpc(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"Received end game stats from client {clientId}");
        recievedCalcs++;
        
        // Ensure we don't go over our expected count
        if (recievedCalcs > 2)
            recievedCalcs = 2;
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    public void LoadAugmentsRpc(RpcParams rpcParams)
    {
        Debug.Log("Loading Augments for Client " + NetworkManager.Singleton.LocalClientId);
        AM.augmentUI.SetActive(true);
        AM.AugmentUISetup(AM.AugmentSelector());
    }

    [Rpc(SendTo.Server)]
    public void UpdatePlayerAbilityUsedRpc(ulong playerID, string abilityKey)
    {
        if (!IsServer) return;

        BaseChampion playerChampion = GetTargetChampion(playerID);
        if (playerChampion == null) return;

        UpdateAbilityReference(playerID, playerChampion, abilityKey);
    }

    private void UpdateAbilityReference(ulong playerID, BaseChampion champion, string abilityKey)
    {
        Ability abilityToAssign = null;
        
        switch (abilityKey)
        {
            case "Q": abilityToAssign = champion.ability1; break;
            case "W": abilityToAssign = champion.ability2; break;
            case "E": abilityToAssign = champion.ability3; break;
            default:
                Debug.LogWarning($"Invalid ability key: {abilityKey} for player {playerID}.");
                return;
        }
        
        if (playerID == player1ID)
            player1AbilityUsed = abilityToAssign;
        else
            player2AbilityUsed = abilityToAssign;
            
        Debug.Log($"Player {playerID} used ability: {abilityToAssign.name}");
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void InitializeIGUIMRpc(RpcParams rpcParams)
    {
        IGUIM.inGameUI.SetActive(true);
        Debug.Log("In-game UI initialized and activated.");
    }

    [Rpc(SendTo.Everyone)]
    public void EndGameUIRpc()
    {
        IGM.endGameUI.displayEndGameUI();
        Debug.Log("End game UI initialized and activated.");
    }
    #endregion
}