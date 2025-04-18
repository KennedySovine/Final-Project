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
        if ((!GM.playerList.ContainsKey("Host") || !GM.playerList.ContainsKey("Server")) && networkDropdown.value == 3){
            //Pop up that says you cannot start as a client without a host/server
        }
        // Cant begin game unless you select which you connect as
        // Sever does not have to select a champion
        if ((champSelectDropdown.value != 0 && networkDropdown.value != 0) || networkDropdown.value == 1){
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
            //Dont add the server to the champion pool
        }
        else if (networkDropdown.value == 2)
        {
            Debug.Log("Starting as Host");
            NetworkManager.Singleton.StartHost();
            GM.playerCount++;
            ChampSelectUI.SetActive(false);
            GM.playerList.Add("Host", NetworkManager.Singleton.LocalClientId); // Add the local client ID to the player list
            switch (champSelectDropdown.value)
            {
                case 1:
                    GM.playerChampions.Add(NetworkManager.Singleton.LocalClientId, GM.playerPrefabsList[0]); // Add the player prefab to the player list
                    break;
                case 2:
                    GM.playerChampions.Add(NetworkManager.Singleton.LocalClientId, GM.playerPrefabsList[1]); // Add the player prefab to the player list
                    break;
                default:
                    Debug.Log("No champion selected");
                    break;
            }
        }
        else if (networkDropdown.value == 3)
        {
            Debug.Log("Starting as Client");
            NetworkManager.Singleton.StartClient();
            GM.playerCount++;
            ChampSelectUI.SetActive(false);
            switch (champSelectDropdown.value)
            {
                case 1:
                    GM.playerChampions.Add(NetworkManager.Singleton.LocalClientId, GM.playerPrefabsList[0]); // Add the player prefab to the player list
                    break;
                case 2:
                    GM.playerChampions.Add(NetworkManager.Singleton.LocalClientId, GM.playerPrefabsList[1]); // Add the player prefab to the player list
                    break;
                default:
                    Debug.Log("No champion selected");
                    break;
            }
        }
        else{
            Debug.Log("No connection type selected");
        }
    }

}
