using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class InGameUIManager : NetworkBehaviour
{

    private GameManager GM;
    private InGameManager IGM;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;

    public GameObject inGameUI; // Reference to the in-game UI GameObject
    [SerializeField] public BaseChampion localPlayerChampion; // Reference to the local player champion object
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

    public void InitializeIGUIM(float maxHealth, float maxMana){
        if (!IsOwner) return; // Only run this for clients
        setHealthSlider(maxHealth); // Set the health slider maximum value
        setManaSlider(maxMana); // Set the mana slider maximum value
        Debug.Log("In-game UI initialized and activated.");
    }

    public void UpdateHealthSlider(float previousValue, float newValue)
    {
        if (!IsOwner) return; // Only run this for clients
        healthSlider.value = newValue; // Update the health slider value
    }
    public void UpdateManaSlider(float previousValue, float newValue)
    {
        if (!IsOwner) return; // Only run this for clients
        manaSlider.value = newValue; // Update the mana slider value
    }

    private void setHealthSlider(float max)
    {
        if (!IsOwner) return; // Only run this for clients
        healthSlider.maxValue = localPlayerChampion.maxHealth.Value; // Set the maximum value of the health slider
        healthSlider.value = localPlayerChampion.health.Value; // Set the current value of the health slider
    }
    
    private void setManaSlider(float max)
    {
        if (!IsOwner) return; // Only run this for clients
        manaSlider.maxValue = localPlayerChampion.maxMana.Value; // Set the maximum value of the mana slider
        manaSlider.value = localPlayerChampion.mana.Value; // Set the current value of the mana slider
    }
}
