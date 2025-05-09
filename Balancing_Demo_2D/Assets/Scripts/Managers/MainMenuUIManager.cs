using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenuUIManager : NetworkBehaviour
{
    #region Fields
    private GameManager GM;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject champSelectUI;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;

    [SerializeField] private TMP_Dropdown networkDropdown;
    [SerializeField] private GameObject netDropDown;
    [SerializeField] private GameObject resetStatsToggle;
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
        resetStatsToggle.SetActive(false); // Hide the reset stats toggle by default
        // Always ensure PlayerStats.json is empty at startup if toggle is on
        if (resetStatsToggle.GetComponent<Toggle>().isOn)
        {
            AbilityStats.ResetPlayerStatsFile();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (networkDropdown.value == 0)
        {
            startButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            startButton.GetComponent<Button>().interactable = true;
        }
        

        // Reset Stats Toggle
        if (networkDropdown.value == 1 || networkDropdown.value == 2){
            resetStatsToggle.SetActive(true); // Show the reset stats toggle for server
        }
        else
        {
            resetStatsToggle.SetActive(false); // Hide the reset stats toggle for client/host
        }
    }
    #endregion

    #region UI Button Methods
    public void ExitButtonClick(){
        Application.Quit();
    }

    public void StartButtonClick(){
        mainMenuUI.SetActive(false);
        ConnectionType();
    }
    #endregion

    #region Game Setup Methods
    public void EnterGame(){
        // Load the game scene here
        //SceneManager.LoadScene(sceneBuildIndex: 1);
        NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single); // Load the game scene
    }

    public void ConnectionType()
    {
        if (networkDropdown.value == 1)
        {
            Debug.Log("Starting as Server");
            if (resetStatsToggle.GetComponent<Toggle>().isOn)
            {
                GM.ResetPlayerStats(); // Reset player stats if the toggle is on
                AbilityStats.ResetPlayerStatsFile(); // Also call static helper to ensure file is empty
            }
            NetworkManager.Singleton.StartServer();
            GM.InitializeNetworkCallbacks(); // Initialize callbacks after starting the server
            GM.ServerID = NetworkManager.Singleton.LocalClientId; // Set the server ID to the local client ID
            EnterGame();
        }
        else if (networkDropdown.value == 2)
        {
            Debug.Log("Starting as Host");
            if (resetStatsToggle.GetComponent<Toggle>().isOn)
            {
                GM.ResetPlayerStats(); // Reset player stats if the toggle is on
                AbilityStats.ResetPlayerStatsFile(); // Also call static helper to ensure file is empty
            }
            NetworkManager.Singleton.StartHost();
            GM.InitializeNetworkCallbacks(); // Initialize callbacks after starting the host
            EnterGame();
        }
        else if (networkDropdown.value == 3)
        {
            Debug.Log("Starting as Client");
            NetworkManager.Singleton.StartClient();
            EnterGame();
        }
        else
        {
            Debug.Log("No connection type selected");
        }
    }
    #endregion
}
