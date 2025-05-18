using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class InGameManager : NetworkBehaviour
{
    #region Fields
    private GameManager GM;
    [SerializeField] private GameObject beginButton;
    [SerializeField] private TMP_Dropdown champSelectDropdown;
    [SerializeField] private GameObject ChampSelectUI;
    [SerializeField] private AugmentManager AM; // Reference to the AugmentManager
    [SerializeField] private InGameUIManager IGUIM; // Reference to the InGameUIManager
    public EndGameUI endGameUI; // Reference to the EndGameUI

    [SerializeField] private List<GameObject> healthPickups = new List<GameObject>(); // List of health pickups
    #endregion

    #region Unity Lifecycle Methods
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance
        if (GM == null)
        {
            Debug.LogError("GameManager instance is null. Ensure the GameManager is active in the scene.");
        }
        
        GM.IGM = this; // Set the InGameManager instance in GameManager

        // Initialize spawn points
        GM.spawnPoints[0] = GameObject.Find("SpawnPoint1").transform;
        GM.spawnPoints[1] = GameObject.Find("SpawnPoint2").transform;

        Debug.Log("Spawn points initialized.");
        beginButton.GetComponent<Button>().interactable = false;

        if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
        {
            GM.EnableServerObserverMode(); // Enable server observer mode
            Debug.Log("Server observer mode enabled.");
            ChampSelectUI.SetActive(false); // Do not show champ select UI to server
        }

        if (NetworkManager.Singleton.IsClient)
        {
            // Deactivate the MainCamera for clients
            GameObject mainCamera = GameObject.FindWithTag("MainCamera");
            if (mainCamera != null)
            {
                mainCamera.SetActive(false);
                Debug.Log("MainCamera deactivated for client.");
            }
            else
            {
                Debug.LogWarning("MainCamera not found in the scene.");
            }
        }

        GM.AM = AM; // Get the AugmentManager instance
    }

    // Update is called once per frame
    void Update()
    {
        // Optional: Add any logic that needs to run every frame
        checkHostReady(); // Check if the host is ready
        respawnHealthPickups(); // Respawn health pickups if needed
    }
    #endregion

    #region UI Methods
    public void dropDownSelectLogic()
    {
        // Enable the begin button only if a valid connection type and champion are selected
        if ((champSelectDropdown.value != 0 ))
        {
            beginButton.GetComponent<Button>().interactable = true; // Enable the begin button
        }
        else
        {
            beginButton.GetComponent<Button>().interactable = false;
        }
    }
    #endregion

    #region Game Logic Methods

    public void respawnHealthPickups()
    {
        // Check if the health pickups are active and respawn them if needed
        foreach (GameObject pickup in healthPickups)
        {
            if (!pickup.activeSelf && Time.time - pickup.GetComponent<HealthPickup>().disableTime >= pickup.GetComponent<HealthPickup>().respawnTime)
            {
                pickup.SetActive(true); // Respawn the health pickup
                Debug.Log("Health pickup respawned: " + pickup.name);
            }
        }
    }
    public void checkHostReady()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }
        else if (!GM.hostReady.Value)
        {
            champSelectDropdown.interactable = false; // Enable dropdown for client
        }
        else if (GM.hostReady.Value)
        {
            champSelectDropdown.interactable = true; // Disable dropdown for client
        }
        else
        {
            Debug.LogWarning("I dont know how we got here.");
        }
    }

    public void beginButtonLogic()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // Host is ready to start the game
            GM.hostReady.Value = true;
            Debug.Log("Host is ready to start the game.");
        }
        if (NetworkManager.Singleton.IsClient)
        {
            //Stop the client from joining if the host hasnt attempted to join.
            if (!GM.hostReady.Value)
            {
                Debug.LogWarning("Host is not ready yet. Please wait.");
                return; // Prevent the client from proceeding
            }

            Debug.Log("Client requesting to join the game.");
            ChampSelectUI.SetActive(false);

            ulong clientId = NetworkManager.Singleton.LocalClientId;
            int selectedChampion = champSelectDropdown.value - 1; // Adjust index to match prefab list
            AddClientToGameRpc(clientId, selectedChampion); // Call the RPC
        }
        else if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server starting the game.");
        }
    }
    #endregion

    #region Network Methods
    [Rpc(SendTo.Server)]
    public void AddClientToGameRpc(ulong clientID, int champChoiceIndex)
    {
        Debug.Log($"Server received request from Client {clientID} to join");

        if (!GM.playerChampions.ContainsKey(clientID))
        {
            if (GM.maxPlayers == GM.playerChampions.Count)
            {
                Debug.LogWarning("Max players reached. Cannot add more players.");
                return; // Prevent adding more players if max players reached
            }
            if (champChoiceIndex >= 0 && champChoiceIndex < GM.playerPrefabsList.Count)
            {
                GameObject champChoice = GM.playerPrefabsList[champChoiceIndex];
                GM.playerChampions.Add(clientID, champChoice); // Add the player prefab to the player list

                Debug.Log($"Client {clientID} added to game with champion {champChoice.name}.");
                ChampSelectUI.SetActive(false); // Hide the champion selection UI
            }
            else
            {
                Debug.LogWarning($"Invalid champion index {champChoiceIndex} for Client {clientID}.");
            }
        }
        else
        {
            Debug.LogWarning($"Client {clientID} is already in the game.");
        }
    }
    #endregion
}
