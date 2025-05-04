using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode; // Import Netcode for Unity
using System.Collections.Generic; // Import System.Collections.Generic for List<T> usage
using TMPro; // Import TextMeshPro for text handling

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Ability ability; // Reference to the Ability class (assuming it's defined elsewhere in your project)
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
}