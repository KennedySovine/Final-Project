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

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void dropDownSelectLogic(){
        //Debug.Log("CHAMP SELECT Dropdown value changed: " + champSelectDropdown.value);
        //Check to see if host/server is already created
        if (GM.playerList.Count > 0 ){
            if (networkDropdown.value == 1 && GM.playerList.ContainsKey("Server")){
                //Pop up message saying the debug log
                Debug.Log("Server already created, cannot create another one.");
                networkDropdown.value = 0; // Reset the dropdown value to 0
            }
            else if (networkDropdown.value == 2 && GM.playerList.ContainsKey("Host")){
                //Pop up message saying the debug log
                Debug.Log("Host already created, cannot create another one.");
                networkDropdown.value = 0; // Reset the dropdown value to 0
            }
        }
        if (!GM.playerList.ContainsKey("Host") && !GM.playerList.ContainsKey("Server") && networkDropdown.value == 3){
            //Pop up that says you cannot start as a client without a host/server
        }
        // Cant begin game unless you select which you connect as
        if (champSelectDropdown.value != 0 && networkDropdown.value != 0){
            beginButton.GetComponent<Button>().interactable = true;
        }
        else {
            beginButton.GetComponent<Button>().interactable = false;
        }
    }

    public void connectionType(){
        if (networkDropdown.value == 1)
        {
            Debug.Log("Starting as Server");
            NetworkManager.Singleton.StartServer();
            ChampSelectUI.SetActive(false);
            GM.playerList.Add("Server", NetworkManager.Singleton.LocalClientId); // Add the local client ID to the player list
            //AssignClass();
        }
        else if (networkDropdown.value == 2)
        {
            Debug.Log("Starting as Host");
            NetworkManager.Singleton.StartHost();
            ChampSelectUI.SetActive(false);
            GM.playerList.Add("Host", NetworkManager.Singleton.LocalClientId); // Add the local client ID to the player list
            //AssignClass();
        }
        else if (networkDropdown.value == 3)
        {
            Debug.Log("Starting as Client");
            NetworkManager.Singleton.StartClient();
            ChampSelectUI.SetActive(false);
            GM.playerList.Add("Client", NetworkManager.Singleton.LocalClientId); // Add the local client ID to the player list
            //AssignClass();
        }
        else{
            Debug.Log("No connection type selected");
        }
    }
    
    public void AssignClass()
    {
        // Ensure the dropdown value is valid
        if (champSelectDropdown.value == 0)
        {
            Debug.LogError("No champion selected!");
            return;
        }

        // Determine which prefab to spawn based on the dropdown value
        GameObject selectedChampionPrefab = null;

        if (champSelectDropdown.value == 1)
        {
            Debug.Log("Spawning ADMelee prefab");
            selectedChampionPrefab = GM.ADMeleePrefab;
        }
        else if (champSelectDropdown.value == 2)
        {
            Debug.Log("Spawning APMelee prefab");
            selectedChampionPrefab = GM.APMeleePrefab;
        }
        else
        {
            Debug.LogError("Invalid champion selection!");
            return;
        }
    }

}
