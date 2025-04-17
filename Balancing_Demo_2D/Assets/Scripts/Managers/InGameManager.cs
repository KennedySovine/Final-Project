using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class InGameManager : MonoBehaviour
{
    
    [SerializeField] private TMP_Dropdown networkDropdown;
    [SerializeField] private GameObject netDropDown;
    [SerializeField] private GameObject beginButton;
    [SerializeField] private TMP_Dropdown champSelectDropdown;
    [SerializeField] private GameObject ChampSelectUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
        }
        else if (networkDropdown.value == 2)
        {
            Debug.Log("Starting as Host");
            NetworkManager.Singleton.StartHost();
            ChampSelectUI.SetActive(false);
        }
        else if (networkDropdown.value == 3)
        {
            Debug.Log("Starting as Client");
            NetworkManager.Singleton.StartClient();
            ChampSelectUI.SetActive(false);
        }
        else{
            Debug.Log("No connection type selected");
        }
    }
    

}
