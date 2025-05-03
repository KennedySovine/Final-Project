using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class InGameUIManager : NetworkBehaviour
{

    private GameManager GM;
    private InGameManager IGM;

    public Slider healthSlider;
    public Slider manaSlider;

    public GameObject inGameUI; // Reference to the in-game UI GameObject
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
        
    }

    public void InitializeIGUIM(){
        inGameUI.SetActive(true); // Activate the in-game UI
        Debug.Log("In-game UI initialized and activated.");
    }

    public void updateSliders(float health, float mana)
    {

        healthSlider.value = health; // Update the health slider value
        manaSlider.value = mana; // Update the mana slider value
    }
}
