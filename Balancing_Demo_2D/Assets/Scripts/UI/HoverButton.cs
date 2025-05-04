using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode; // Import Netcode for Unity
using System.Collections.Generic; // Import System.Collections.Generic for List<T> usage
using TMPro; // Import TextMeshPro for text handling
using System;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Ability ability; // Reference to the Ability class (assuming it's defined elsewhere in your project)
    [SerializeField] private GameObject infoBox; 
    [SerializeField] private TextMeshProUGUI infoBoxText;
    private string abilityDescription; // Variable to hold the ability description
    private string abilityName; // Variable to hold the ability name

    void Start(){
        if (ability == null)
        {
            Debug.LogError("Ability reference is not set in the inspector.");
        }
        else{
            if (abilityDescription == null){
                abilityDescription = ability.description; // Get the ability description from the Ability class
                abilityName = ability.name; // Get the ability name from the Ability class
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Code to execute when the mouse enters the button
        Debug.Log("Mouse entered button");
        // Example: Change button color, add audio feedback, etc.
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Code to execute when the mouse leaves the button
        Debug.Log("Mouse exited button");
        // Example: Restore original color, stop audio, etc.
    }

    private void formatInfoBox(){
        infoBoxText.text = $"<b>{abilityName}</b>\n" + abilityDescription; // Format the info box text with ability name and description
    }
}