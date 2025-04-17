using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject champSelectUI;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
        enterGame();
        // Load the game scene here
        // SceneManager.LoadScene("GameSceneName"); // Uncomment and replace with your scene name
    }

    public void enterGame(){
        // Load the game scene here
        SceneManager.LoadScene(sceneBuildIndex: 1);
    }
}
