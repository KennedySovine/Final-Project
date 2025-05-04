using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Import TextMeshPro for text handling
using UnityEngine.EventSystems; // Import EventSystems for event handling

public class InGameUIManager : NetworkBehaviour
{
    public List<Sprite> abilityIconsADRange = new List<Sprite>(); // List to hold ability icons for AD range
    public List<Sprite> abilityIconsADRange2 = new List<Sprite>(); // List to hold ability icons for AP range

    public List<Button> abilityIcons = new List<Button>(); // List to hold ability icons

    [SerializeField] private TextMeshProUGUI timerText; // Reference to the TextMeshProUGUI component for displaying the timer

    private GameManager GM;
    private InGameManager IGM;

    public Slider healthSlider;
    public Slider manaSlider;

    public GameObject inGameUI; // Reference to the in-game UI GameObject
    public bool iconsSet = false; // Flag to check if icons are set
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        GM = GameManager.Instance; // Get the GameManager instance
        if (GM == null)
        {
            Debug.LogError("GameManager instance is null. Ensure the GameManager is active in the scene.");
        }
    }
    void Start()
    {
        GM.IGUIM = this; // Set the InGameUIManager instance in GameManager

        IGM = GM.IGM; // Get the InGameManager instance
        if (IGM == null)
        {
            Debug.LogError("InGameManager instance is null. Ensure the InGameManager is active in the scene.");
        }

        if (!IsClient) return; // Only run this for clients

    
        
    }

    // Update is called once per frame
    void Update()
    {
        int minutes = Mathf.FloorToInt(GM.gameTime / 60); // Calculate the minutes
        int seconds = Mathf.FloorToInt(GM.gameTime % 60); // Calculate the seconds
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds); // Format the time as MM:SS
    }

    public void UpdateHealthSlider(float previousValue, float newValue)
    {
        Debug.Log($"Updating health slider for Client {NetworkManager.Singleton.LocalClientId}. New Value: {newValue}");
        healthSlider.value = newValue; // Update the health slider in the UI
    }

    public void UpdateManaSlider(float previousValue, float newValue)
    {
        Debug.Log($"Updating mana slider for Client {NetworkManager.Singleton.LocalClientId}. New Value: {newValue}");
        manaSlider.value = newValue; // Update the mana slider in the UI
    }

    public void UpdateMaxHealthSlider(float previousValue, float newValue)
    {
        Debug.Log($"Updating max health slider for Client {NetworkManager.Singleton.LocalClientId}. New Value: {newValue}");
        healthSlider.maxValue = newValue; // Update the max health slider in the UI
    }

    public void UpdateMaxManaSlider(float previousValue, float newValue)
    {
        Debug.Log($"Updating max mana slider for Client {NetworkManager.Singleton.LocalClientId}. New Value: {newValue}");
        manaSlider.maxValue = newValue; // Update the max mana slider in the UI
    }


    public void SetIconImages(string championType){
        // Set the ability icons based on the champion type
        if (championType == "AD Range")
        {
            for (int i = 0; i < abilityIcons.Count; i++)
            {
                abilityIcons[i].GetComponent<Image>().sprite = abilityIconsADRange[i]; // Set the icon image for each ability
            }
        }
        else if (championType == "AD Range2")
        {
            for (int i = 0; i < abilityIcons.Count; i++)
            {
                abilityIcons[i].GetComponent<Image>().sprite = abilityIconsADRange2[i]; // Set the icon image for each ability
            }
        }
        else
        {
            Debug.LogWarning($"Unknown champion type: {championType}. No icons set.");
        }
    }

    public void AsheEmpowerIcon(bool isEmpowered)
    {
        // Set the empowered icon for Ashe
        if (isEmpowered)
        {
            abilityIcons[0].GetComponent<Image>().sprite = abilityIconsADRange[3]; // Set the empowered icon
        }
        else
        {
            abilityIcons[0].GetComponent<Image>().sprite = abilityIconsADRange[0]; // Set the normal icon
        }
    }

    public void buttonInteractable(string position, bool isInteractable){
        switch (position){
            case "Q":
                abilityIcons[0].interactable = isInteractable; // Disable the Q ability button
                break;
            case "W":
                abilityIcons[1].interactable = isInteractable; // Disable the W ability button
                break;
            case "E":
                abilityIcons[2].interactable = isInteractable; // Disable the E ability button
                break;
            default:
                Debug.LogWarning($"Unknown button position: {position}. No action taken.");
                break;
        }
    }

}
