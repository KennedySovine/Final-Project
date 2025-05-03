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

    [SerializeField] private ulong localClientId;
    [SerializeField] private GameObject localPlayer; // Reference to the local player object
    [SerializeField] private GameObject localPlayerController; // Reference to the local player controller object
    [SerializeField] private BaseChampion localPlayerChampion; // Reference to the local player champion object
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

        localClientId = NetworkManager.Singleton.LocalClientId; // Get the local client ID
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeIGUIM()
    {
        if (IsServer) return; // Only run this for clients

        inGameUI.SetActive(true); // Activate the in-game UI
        StartCoroutine(InitializeAfterPlayerIds());
    }

    private IEnumerator InitializeAfterPlayerIds()
    {
        // Wait for player IDs to be different
        yield return WaitforPlayerIds();

        // Proceed with initialization after player IDs are ready
        switch (localClientId)
        {
            case var id when id == GM.player1IDNet.Value:
                localPlayer = NetworkManager.Singleton.SpawnManager.SpawnedObjects[GM.player1IDNet.Value].gameObject; // Get the local player object
                localPlayerController = localPlayer.transform.Find("PlayerController").gameObject; // Get the local player controller object
                localPlayerChampion = localPlayerController.GetComponent<BaseChampion>(); // Get the BaseChampion component from the local player controller
                break;
            case var id when id == GM.player2IDNet.Value:
                localPlayer = NetworkManager.Singleton.SpawnManager.SpawnedObjects[GM.player2IDNet.Value].gameObject; // Get the local player object
                localPlayerController = localPlayer.transform.Find("PlayerController").gameObject; // Get the local player controller object
                localPlayerChampion = localPlayerController.GetComponent<BaseChampion>(); // Get the BaseChampion component from the local player controller
                break;
            default:
                Debug.LogError("Local player not found. Ensure the player is spawned correctly.");
                yield break; // Exit the coroutine
        }

        if (localPlayerChampion == null)
        {
            Debug.LogError("BaseChampion component not found on localPlayerController.");
            yield break; // Exit the coroutine
        }

        Debug.Log("In-game UI initialized and activated.");

        localPlayerChampion.health.OnValueChanged += UpdateHealthSlider; // Subscribe to health value changes
        localPlayerChampion.mana.OnValueChanged += UpdateManaSlider; // Subscribe to mana value changes

        setHealthSlider(); // Set the health slider
        setManaSlider(); // Set the mana slider
    }

    private void UpdateHealthSlider(float previousValue, float newValue)
    {
        if (IsServer) return; // Only run this for clients
        healthSlider.value = newValue; // Update the health slider value
    }
    private void UpdateManaSlider(float previousValue, float newValue)
    {
        if (IsServer) return; // Only run this for clients
        manaSlider.value = newValue; // Update the mana slider value
    }

    private void setHealthSlider()
    {
        if (IsServer) return; // Only run this for clients
        healthSlider.maxValue = localPlayerChampion.maxHealth.Value; // Set the maximum value of the health slider
        healthSlider.value = localPlayerChampion.health.Value; // Set the current value of the health slider
    }
    
    private void setManaSlider()
    {
        if (IsServer) return; // Only run this for clients
        manaSlider.maxValue = localPlayerChampion.maxMana.Value; // Set the maximum value of the mana slider
        manaSlider.value = localPlayerChampion.mana.Value; // Set the current value of the mana slider
    }

    private IEnumerator WaitforPlayerIds()
    {
        while (GM.player1IDNet.Value == GM.player2IDNet.Value)
        {
            yield return null; // Wait until player IDs are different
        }

        Debug.Log("Player IDs are different. Proceeding with initialization.");
        Debug.Log("Player 1 ID: " + GM.player1IDNet.Value); // Log player 1 ID
        Debug.Log("Player 2 ID: " + GM.player2IDNet.Value); // Log player 2 ID
    }
}
