using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Import TextMeshPro for text handling
using UnityEngine.EventSystems; // Import EventSystems for event handling

public class InGameUIManager : NetworkBehaviour
{
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

        GM.gameTime.OnValueChanged += (previousValue, newValue) =>
        {
            int minutes = Mathf.FloorToInt(newValue / 60); // Calculate the minutes
            int seconds = Mathf.FloorToInt(newValue % 60); // Calculate the seconds
            string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds); // Format the time as MM:SS
            timerText.text = formattedTime; // Update the timer text in the UI
        };

        if (!IsClient) return; // Only run this for clients
    }

    // Update is called once per frame
    void Update()
    {


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

    public void AsheEmpowerIcon(bool isEmpowered)
    {
        abilityIcons[0].GetComponent<Image>().sprite = isEmpowered ? abilityIcons[0].GetComponent<HoverButton>().ability.icon : abilityIcons[0].GetComponent<HoverButton>().ability.icon; // Set the icon for the button based on empowerment
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

    public void setAbilityToButtons(Dictionary<string, Ability> abilityDict){
        // Set the ability to the button based on the position
        foreach (Button button in abilityIcons){
            if (button == null){
                Debug.LogError("Button reference is null. Ensure the button is assigned in the inspector.");
                continue; // Skip to the next button if the current one is null
            }
            string position = button.name; // Get the name of the button to determine its position
            if (abilityDict.ContainsKey(position)){
                Ability ability = abilityDict[position]; // Get the ability from the dictionary
                button.GetComponent<Image>().sprite = ability.icon; // Set the icon for the button
                button.GetComponent<HoverButton>().ability = ability; // Set the ability reference in the HoverButton component
                button.GetComponent<HoverButton>().addAbilityInfo(); // Add ability info to the button
                //button.GetComponent<HoverButton>().IGUIM = this; // Set the InGameUIManager reference in the HoverButton component
            }
            else{
                Debug.LogWarning($"Ability not found for position: {position}. No action taken.");
            }
        }
    }

    
    

}
