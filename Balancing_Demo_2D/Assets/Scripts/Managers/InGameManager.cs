using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;

public class InGameManager : MonoBehaviour
{
    
    [SerializeField] private TMP_Dropdown networkDropdown;
    [SerializeField] private GameObject netDropDown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void connectionType(){
        if (networkDropdown.value == 1)
        {
            Debug.Log("Starting as Server");
            NetworkManager.Singleton.StartServer();
            netDropDown.SetActive(false);// Disable the dropdown after selection
        }
        else if (networkDropdown.value == 2)
        {
            Debug.Log("Starting as Host");
            NetworkManager.Singleton.StartHost();
            netDropDown.SetActive(false); // Disable the dropdown after selection
        }
        else if (networkDropdown.value == 3)
        {
            Debug.Log("Starting as Client");
            NetworkManager.Singleton.StartClient();
            netDropDown.SetActive(false); // Disable the dropdown after selection
        }
    }

}
