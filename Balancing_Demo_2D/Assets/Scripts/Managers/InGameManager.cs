using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;


public class InGameManager : NetworkBehaviour
{
    private GameManager GM;
    [SerializeField] private GameObject beginButton;
    [SerializeField] private TMP_Dropdown champSelectDropdown;
    [SerializeField] private GameObject ChampSelectUI;
    [SerializeField] private AugmentManager AM; // Reference to the AugmentManager

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
    }

    public void dropDownSelectLogic()
    {
        // Enable the begin button only if a valid connection type and champion are selected
        if ((champSelectDropdown.value != 0 ))
        {
            beginButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            beginButton.GetComponent<Button>().interactable = false;
        }
    }

    public void beginButtonLogic()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Client requesting to join the game.");
            ChampSelectUI.SetActive(false);

            // Debug before calling the RPC
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            int selectedChampion = champSelectDropdown.value - 1; // Adjust index to match prefab list
            Debug.Log($"Calling AddClientToGameRpc with ClientID: {clientId}, ChampionIndex: {selectedChampion}");

            AddClientToGameRpc(clientId, selectedChampion); // Call the RPC
        }
        else if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server starting the game.");
            // Start the game logic here
            // GM.StartGame();
        }
    }

    [Rpc(SendTo.Server)]
    public void AddClientToGameRpc(ulong clientID, int champChoiceIndex)
    {
        Debug.Log($"Server received request from Client {clientID} to join");

        if (!GM.playerChampions.ContainsKey(clientID))
        {
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
}
