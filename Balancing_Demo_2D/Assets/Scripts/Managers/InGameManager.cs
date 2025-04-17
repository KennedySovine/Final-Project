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
        Debug.Log("CHAMP SELECT Dropdown value changed: " + champSelectDropdown.value);
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
            //AssignClass();
        }
        else if (networkDropdown.value == 2)
        {
            Debug.Log("Starting as Host");
            NetworkManager.Singleton.StartHost();
            ChampSelectUI.SetActive(false);
            //AssignClass();
        }
        else if (networkDropdown.value == 3)
        {
            Debug.Log("Starting as Client");
            NetworkManager.Singleton.StartClient();
            ChampSelectUI.SetActive(false);
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

        // Spawn the selected prefab at the first spawn point
        Transform spawnPoint = GM.spawnPoints[0]; // Assuming you want to spawn at the first spawn point
        GameObject championInstance = Instantiate(selectedChampionPrefab, spawnPoint.position, Quaternion.identity);
        championInstance.GetComponent<NetworkObject>().Spawn(); // Spawn the champion on the network

        // Set the player as the owner of the champion instance
        NetworkObject networkObject = championInstance.GetComponent<NetworkObject>();
        networkObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
    }

}
