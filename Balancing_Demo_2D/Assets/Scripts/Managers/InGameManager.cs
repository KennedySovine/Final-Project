using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class InGameManager : MonoBehaviour
{
    private GameManager GM;
    [SerializeField] private TMP_Dropdown networkDropdown;
    [SerializeField] private GameObject netDropDown;
    [SerializeField] private GameObject beginButton;
    [SerializeField] private TMP_Dropdown champSelectDropdown;
    [SerializeField] private GameObject ChampSelectUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance

        // Initialize spawn points
        GM.spawnPoints[0] = GameObject.Find("SpawnPoint1").transform;
        GM.spawnPoints[1] = GameObject.Find("SpawnPoint2").transform;

        Debug.Log("Spawn points initialized.");
    }

    // Update is called once per frame
    void Update()
    {
        // Optional: Add any logic that needs to run every frame
    }

    public void dropDownSelectLogic()
    {
        // Enable the begin button only if a valid connection type and champion are selected
        if ((champSelectDropdown.value != 0 && networkDropdown.value != 0) || networkDropdown.value == 1)
        {
            beginButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            beginButton.GetComponent<Button>().interactable = false;
        }
    }

    public void connectionType()
    {
        if (networkDropdown.value == 1)
        {
            Debug.Log("Starting as Server");
            NetworkManager.Singleton.StartServer();
            GM.InitializeNetworkCallbacks(); // Initialize callbacks after starting the server
            ChampSelectUI.SetActive(false);
            GM.playerList.Add("Server", NetworkManager.Singleton.LocalClientId); // Add the local client ID to the player list
        }
        else if (networkDropdown.value == 2)
        {
            Debug.Log("Starting as Host");
            NetworkManager.Singleton.StartHost();
            GM.InitializeNetworkCallbacks(); // Initialize callbacks after starting the host
            GM.playerCount++;
            ChampSelectUI.SetActive(false);
            GM.playerList.Add("Host", NetworkManager.Singleton.LocalClientId); // Add the local client ID to the player list

            // Assign champion for the host
            int selectedChampion = champSelectDropdown.value - 1; // Adjust index to match prefab list
            if (selectedChampion >= 0 && selectedChampion < GM.playerPrefabsList.Count)
            {
                GM.playerChampions.Add(NetworkManager.Singleton.LocalClientId, GM.playerPrefabsList[selectedChampion]);
            }
            else
            {
                Debug.LogWarning("Invalid champion selection for the host.");
            }
        }
        else if (networkDropdown.value == 3)
        {
            Debug.Log("Starting as Client");
            NetworkManager.Singleton.StartClient();
            ChampSelectUI.SetActive(false);

            // Subscribe to the OnClientConnectedCallback to send the join request after connecting
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
            Debug.Log("No connection type selected");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Client successfully connected to the server.");

            // Request to join the game and select a champion
            int selectedChampion = champSelectDropdown.value - 1; // Adjust index to match prefab list
            if (selectedChampion >= 0 && selectedChampion < GM.playerPrefabsList.Count)
            {
                GM.AddClientToGameServerRpc(NetworkManager.Singleton.LocalClientId, GM.playerPrefabsList[selectedChampion]);
            }
            else
            {
                Debug.LogWarning("Invalid champion selection for the client.");
            }

            // Unsubscribe from the callback to avoid duplicate calls
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
