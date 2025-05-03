using UnityEngine;

public class InGameUIManager : MonoBehaviour
{

    private GameManager GM;
    private InGameManager IGM;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance
        if (GM == null)
        {
            Debug.LogError("GameManager instance is null. Ensure the GameManager is active in the scene.");
        }

        IGM = GM.IGM; // Get the InGameManager instance
        if (IGM == null)
        {
            Debug.LogError("InGameManager instance is null. Ensure the InGameManager is active in the scene.");
        }

        // Initialize the health and mana sliders
        setHealthSlider();
        setManaSlider();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void setHealthSlider()
    {
        if (GM.localPlayer != null && GM.localPlayer.GetComponent<NetworkObject>() != null)
        {
            healthSlider.maxValue = GM.localPlayer.GetComponent<NetworkObject>().GetComponent<BaseChampion>().maxHealth.Value;
            healthSlider.value = GM.localPlayer.GetComponent<NetworkObject>().GetComponent<BaseChampion>().health.Value;
        }
    }
    
    private void setManaSlider()
    {
        if (GM.localPlayer != null && GM.localPlayer.GetComponent<NetworkObject>() != null)
        {
            manaSlider.maxValue = GM.localPlayer.GetComponent<NetworkObject>().GetComponent<BaseChampion>().maxMana.Value;
            manaSlider.value = GM.localPlayer.GetComponent<NetworkObject>().GetComponent<BaseChampion>().mana.Value;
        }
}
