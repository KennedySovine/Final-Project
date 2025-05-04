using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode; // Import Netcode for Unity
using System.Collections.Generic; // Import System.Collections.Generic for List<T> usage
using TMPro; // Import TextMeshPro for text handling
using System;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Ability ability; // Reference to the Ability class (assuming it's defined elsewhere in your project)
    private InGameUIManager IGUIM; // Reference to the InGameUIManager class
    [SerializeField] private GameObject infoBox; 
    [SerializeField] private TextMeshProUGUI infoBoxText;
    public string abilityDescription = null; // Variable to hold the ability description
    public string abilityName; // Variable to hold the ability name

    void Start(){
        if (ability == null)
        {
            Debug.LogError("Ability reference is not set in the inspector.");
        }
        abilityDescription = ability.description; // Get the ability description from the Ability class
        abilityName = ability.name; // Get the ability name from the Ability class
        formatInfoBox(); // Format the info box text with ability name and description
        
    }

    void Update(){
        
    }

    public void addAbilityInfo(){
        abilityDescription = ability.description; // Get the ability description from the Ability class
        abilityName = ability.name; // Get the ability name from the Ability class
        formatInfoBox(); // Format the info box text with ability name and description
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Code to execute when the mouse enters the button
        infoBox.SetActive(true); // Show the info box when the mouse enters the button
        Debug.Log("Mouse entered button");
        // Example: Change button color, add audio feedback, etc.
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Code to execute when the mouse leaves the button
        infoBox.SetActive(false); // Hide the info box when the mouse exits the button
        Debug.Log("Mouse exited button");
        // Example: Restore original color, stop audio, etc.
    }

    private void formatInfoBox(){
        infoBoxText.text = string.Format("<b>{0}</b>\n{1}", abilityName, abilityDescription); // Format the info box text with ability name and description
    }
}