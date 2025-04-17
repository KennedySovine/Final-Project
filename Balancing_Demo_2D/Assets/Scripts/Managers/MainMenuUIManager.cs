using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject champSelectUI;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private GameObject beginButton;
    [SerializeField] private TMP_Dropdown champSelectDropdown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        beginButton.GetComponent<Button>().interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void exitButtonClick(){
        Application.Quit();
    }

    public void startButtonClick(){
        mainMenuUI.SetActive(false);
        champSelectUI.SetActive(true);
        // Load the game scene here
        // SceneManager.LoadScene("GameSceneName"); // Uncomment and replace with your scene name
    }

    public void dropDownSelectLogic(){
        Debug.Log("Dropdown value: " + champSelectDropdown.value);
        if (champSelectDropdown.value != 0){
            beginButton.GetComponent<Button>().interactable = true;
        }
        else{
            beginButton.GetComponent<Button>().interactable = false;
        }

    }
}
