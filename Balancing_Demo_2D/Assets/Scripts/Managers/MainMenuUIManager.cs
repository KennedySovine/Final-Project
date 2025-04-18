using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenuUIManager : NetworkBehaviour
{
    private GameManager GM;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject champSelectUI;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;

    [SerializeField] private TMP_Dropdown networkDropdown;
    [SerializeField] private GameObject netDropDown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance
        if (GM == null)
        {
            Debug.LogError("GameManager instance is null. Ensure the GameManager is active in the scene.");
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
        
    }

    public void exitButtonClick(){
        Application.Quit();
    }

    public void startButtonClick(){
        mainMenuUI.SetActive(false);
        connectionType();
        // Load the game scene here
        // SceneManager.LoadScene("GameSceneName"); // Uncomment and replace with your scene name
    }

    public void enterGame(){
        // Load the game scene here
        //SceneManager.LoadScene(sceneBuildIndex: 1);
        NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single); // Load the game scene
    }

    public void connectionType()
    {
        if (networkDropdown.value == 1)
        {
            Debug.Log("Starting as Server");
            NetworkManager.Singleton.StartServer();
            GM.InitializeNetworkCallbacks(); // Initialize callbacks after starting the server
            GM.ServerID = NetworkManager.Singleton.LocalClientId; // Set the server ID to the local client ID
            enterGame();
        }
        else if (networkDropdown.value == 2)
        {
            Debug.Log("Starting as Host");
            NetworkManager.Singleton.StartHost();
            GM.InitializeNetworkCallbacks(); // Initialize callbacks after starting the host
            enterGame();
        }
        else if (networkDropdown.value == 3)
        {
            Debug.Log("Starting as Client");
            NetworkManager.Singleton.StartClient();
            enterGame();
        }
        else
        {
            Debug.Log("No connection type selected");
        }
    }
}
