using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class MainMenuUIManager : NetworkBehaviour
{
    #region Fields
    private GameManager GM;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject champSelectUI;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;

    [SerializeField] private TMP_Dropdown networkDropdown;
    [SerializeField] private GameObject netDropDown;
    [SerializeField] private GameObject resetStatsToggle;
    [SerializeField] private GameObject champAdjustmentUI;
    [SerializeField] private GameObject adjustmentsParent;
    private Button asheButton;
    private Button vayneButton;

    [SerializeField] private GameObject SaveButton;
    private GameObject adjustmentButton;

    private bool statSaved = true; // Flag to check if stats are saved

    private List<GameObject> champModifiers = new List<GameObject>();

    private bool asheSelected = true;
    private bool vayneSelected = true;
    #endregion

    #region Unity Lifecycle Methods
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance
        if (GM == null)
        {
            Debug.LogError("GameManager instance is null. Ensure the GameManager is active in the scene.");
        }
        resetStatsToggle.SetActive(false); // Hide the reset stats toggle by default
        adjustmentButton = mainMenuUI.transform.Find("Adjustment").gameObject; // Find the Adjustments button in the UI
        adjustmentButton.SetActive(false); // Hide the Adjustments button by default
        // Always ensure PlayerStats.json is empty at startup if toggle is on
        if (resetStatsToggle.GetComponent<Toggle>().isOn)
        {
            AbilityStats.ResetPlayerStatsFile();
        }

        asheSelected = true;
        vayneSelected = true;
        asheButton.interactable = true;
        vayneButton.interactable = true;
        UpdateChampionButtonVisuals();

        champModifiers.Clear();
        foreach (Transform child in adjustmentsParent.transform)
        {
            champModifiers.Add(child.gameObject);
        }
    }

    private void Awake()
    {
        asheButton = champAdjustmentUI.transform.Find("ASHE").GetComponent<Button>();
        vayneButton = champAdjustmentUI.transform.Find("VAYNE").GetComponent<Button>();

        asheButton.onClick.AddListener(OnAsheButtonClicked);
        vayneButton.onClick.AddListener(OnVayneButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        if (networkDropdown.value == 0)
        {
            startButton.GetComponent<Button>().interactable = false;
            adjustmentButton.SetActive(false); // Hide the Adjustments button if no connection type is selected
        }
        else
        {
            startButton.GetComponent<Button>().interactable = true;
            // Only show the Adjustments button for Host or Server (not Client)
            if (networkDropdown.value == 1 || networkDropdown.value == 2)
            {
                adjustmentButton.SetActive(true);
            }
            else
            {
                adjustmentButton.SetActive(false);
            }
        }

        // Reset Stats Toggle
        if (networkDropdown.value == 1 || networkDropdown.value == 2){
            resetStatsToggle.SetActive(true); // Show the reset stats toggle for server/host
        }
        else
        {
            resetStatsToggle.SetActive(false); // Hide the reset stats toggle for client/none
        }

        if (champAdjustmentUI.activeSelf)
        {
            if (!statSaved)
            {
                SaveButton.GetComponent<Button>().interactable = true; // Enable the save button if stats are not saved
            }
            else
            {
                SaveButton.GetComponent<Button>().interactable = false; // Disable the save button if stats are saved
            }
        }
    }
    #endregion

    #region UI Button Methods
    public void ExitButtonClick(){
        Application.Quit();
    }

    public void StartButtonClick(){
        mainMenuUI.SetActive(false);
        ConnectionType();
    }

    public void BackToMainMenu(){
        mainMenuUI.SetActive(true);
        champAdjustmentUI.SetActive(false);
    }

    public void ChampAdjustmentClick(){
        LoadStats(2); // Load stats for Vayne by default
        champAdjustmentUI.SetActive(true);
        mainMenuUI.SetActive(false);
    }

    private void OnAsheButtonClicked()
    {
        asheSelected = !asheSelected;
        UpdateChampionButtonVisuals();
        LoadStats(3); // Assuming index 3 is Ashe
        statSaved = true;
    }

    private void OnVayneButtonClicked()
    {
        vayneSelected = !vayneSelected;
        UpdateChampionButtonVisuals();
        LoadStats(2); // Assuming index 2 is Vayne
        statSaved = true;
    }

    private void UpdateChampionButtonVisuals()
    {
        Color32 deselectedColor = new Color32(170, 172, 179, 255); // #AAACB3

        // Set the button's Image color directly
        var asheImage = asheButton.GetComponent<Image>();
        if (asheImage != null)
            asheImage.color = asheSelected ? Color.white : deselectedColor;

        var vayneImage = vayneButton.GetComponent<Image>();
        if (vayneImage != null)
            vayneImage.color = vayneSelected ? Color.white : deselectedColor;
    }
    #endregion

    #region Game Setup Methods
    public void EnterGame(){
        // Load the game scene here
        //SceneManager.LoadScene(sceneBuildIndex: 1);
        NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single); // Load the game scene
    }

    public void ConnectionType()
    {
        if (networkDropdown.value == 1)
        {
            Debug.Log("Starting as Server");
            if (resetStatsToggle.GetComponent<Toggle>().isOn)
            {
                GM.ResetPlayerStats(); // Reset player stats if the toggle is on
                AbilityStats.ResetPlayerStatsFile(); // Also call static helper to ensure file is empty
            }
            NetworkManager.Singleton.StartServer();
            GM.InitializeNetworkCallbacks(); // Initialize callbacks after starting the server
            GM.ServerID = NetworkManager.Singleton.LocalClientId; // Set the server ID to the local client ID
            EnterGame();
        }
        else if (networkDropdown.value == 2)
        {
            Debug.Log("Starting as Host");
            if (resetStatsToggle.GetComponent<Toggle>().isOn)
            {
                GM.ResetPlayerStats(); // Reset player stats if the toggle is on
                AbilityStats.ResetPlayerStatsFile(); // Also call static helper to ensure file is empty
            }
            NetworkManager.Singleton.StartHost();
            GM.InitializeNetworkCallbacks(); // Initialize callbacks after starting the host
            EnterGame();
        }
        else if (networkDropdown.value == 3)
        {
            Debug.Log("Starting as Client");
            NetworkManager.Singleton.StartClient();
            EnterGame();
        }
        else
        {
            Debug.Log("No connection type selected");
        }
    }
    #endregion

    #region Champ Alteration Methods
    public void UpdateSliderText(Slider slider)
    {
        GameObject valueText = slider.transform.Find("SliderValue").gameObject;
        TextMeshProUGUI text = valueText.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            string format = slider.wholeNumbers ? "0" : "0.000";
            text.text = slider.value.ToString(format); // Update the text to show the current value
        }
        statSaved = false; // Set the flag to false when a stat is changed
    }

    public void LoadStats(int index)
    {
        foreach (GameObject champModifier in champModifiers)
        {
            Slider slider = champModifier.GetComponentInChildren<Slider>();
            if (slider != null)
            {
                string statName = champModifier.name; // Get the name of the slider to identify the stat

                // If both buttons are interactable, check if values differ
                if (asheButton != null && vayneButton != null && asheSelected && vayneSelected)
                {
                    float asheValue = 0f, vayneValue = 0f;
                    switch (statName)
                    {
                        case "MaxHealth":
                            asheValue = GM.playerChampionsData[3].maxHealth;
                            vayneValue = GM.playerChampionsData[2].maxHealth;
                            break;
                        case "HealthRegen":
                            asheValue = GM.playerChampionsData[3].healthRegen;
                            vayneValue = GM.playerChampionsData[2].healthRegen;
                            break;
                        case "AD":
                            asheValue = GM.playerChampionsData[3].AD;
                            vayneValue = GM.playerChampionsData[2].AD;
                            break;
                        case "AP":
                            asheValue = GM.playerChampionsData[3].AP;
                            vayneValue = GM.playerChampionsData[2].AP;
                            break;
                        case "Armor":
                            asheValue = GM.playerChampionsData[3].armor;
                            vayneValue = GM.playerChampionsData[2].armor;
                            break;
                        case "MagicResist":
                            asheValue = GM.playerChampionsData[3].magicResist;
                            vayneValue = GM.playerChampionsData[2].magicResist;
                            break;
                        case "AttackSpeed":
                            asheValue = GM.playerChampionsData[3].attackSpeed;
                            vayneValue = GM.playerChampionsData[2].attackSpeed;
                            break;
                        case "MovementSpeed":
                            asheValue = GM.playerChampionsData[3].movementSpeed;
                            vayneValue = GM.playerChampionsData[2].movementSpeed;
                            break;
                        case "MaxMana":
                            asheValue = GM.playerChampionsData[3].maxMana;
                            vayneValue = GM.playerChampionsData[2].maxMana;
                            break;
                        case "ManaRegen":
                            asheValue = GM.playerChampionsData[3].manaRegen;
                            vayneValue = GM.playerChampionsData[2].manaRegen;
                            break;
                        case "AbilityHaste":
                            asheValue = GM.playerChampionsData[3].abilityHaste;
                            vayneValue = GM.playerChampionsData[2].abilityHaste;
                            break;
                        case "CritChance":
                            asheValue = GM.playerChampionsData[3].critChance;
                            vayneValue = GM.playerChampionsData[2].critChance;
                            break;
                        case "CritDamage":
                            asheValue = GM.playerChampionsData[3].critDamage;
                            vayneValue = GM.playerChampionsData[2].critDamage;
                            break;
                        case "ArmorPen":
                            asheValue = GM.playerChampionsData[3].armorPen;
                            vayneValue = GM.playerChampionsData[2].armorPen;
                            break;
                        case "MagicPen":
                            asheValue = GM.playerChampionsData[3].magicPen;
                            vayneValue = GM.playerChampionsData[2].magicPen;
                            break;
                        case "MissileSpeed":
                            asheValue = GM.playerChampionsData[3].missileSpeed;
                            vayneValue = GM.playerChampionsData[2].missileSpeed;
                            break;
                        default:
                            Debug.LogError($"Unknown stat name: {statName}");
                            break;
                    }

                    GameObject valueText = slider.transform.Find("SliderValue").gameObject;
                    TextMeshProUGUI text = valueText.GetComponent<TextMeshProUGUI>();
                    string format = slider.wholeNumbers ? "0" : "0.000";
                    if (Mathf.Approximately(asheValue, vayneValue))
                    {
                        slider.value = asheValue;
                        if (text != null)
                            text.text = asheValue.ToString(format);
                    }
                    else
                    {
                        slider.value = slider.minValue;
                        if (text != null)
                            text.text = "--";
                    }
                }
                else
                {
                    // ...existing switch for single champion...
                    switch (statName){
                        case "MaxHealth": slider.value = GM.playerChampionsData[index].maxHealth; break;
                        case "HealthRegen": slider.value = GM.playerChampionsData[index].healthRegen; break;
                        case "AD": slider.value = GM.playerChampionsData[index].AD; break;
                        case "AP": slider.value = GM.playerChampionsData[index].AP; break;
                        case "Armor": slider.value = GM.playerChampionsData[index].armor; break;
                        case "MagicResist": slider.value = GM.playerChampionsData[index].magicResist; break;
                        case "AttackSpeed": slider.value = GM.playerChampionsData[index].attackSpeed; break;
                        case "MovementSpeed": slider.value = GM.playerChampionsData[index].movementSpeed; break;
                        case "MaxMana": slider.value = GM.playerChampionsData[index].maxMana; break;
                        case "ManaRegen": slider.value = GM.playerChampionsData[index].manaRegen; break;
                        case "AbilityHaste": slider.value = GM.playerChampionsData[index].abilityHaste; break;
                        case "CritChance": slider.value = GM.playerChampionsData[index].critChance; break;
                        case "CritDamage": slider.value = GM.playerChampionsData[index].critDamage; break;
                        case "ArmorPen": slider.value = GM.playerChampionsData[index].armorPen; break;
                        case "MagicPen": slider.value = GM.playerChampionsData[index].magicPen; break;
                        case "MissileSpeed": slider.value = GM.playerChampionsData[index].missileSpeed; break;
                        default: Debug.LogError($"Unknown stat name: {statName}"); break;
                    }
                    UpdateSliderText(slider); // Update the text to show the current value
                }
            }
        }
    }

    public void SaveChampionChanges(){
        int index = 0; // Default index

        if (asheSelected && vayneSelected)
        {
            // Save both champions, but preserve their ChampionType
            var vayneData = GM.playerChampionsData[2];
            var asheData = GM.playerChampionsData[3];

            foreach (GameObject champModifier in champModifiers)
            {
                Slider slider = champModifier.GetComponentInChildren<Slider>();
                if (slider != null)
                {
                    float value = slider.value;
                    string statName = champModifier.name;

                    // Apply to both, but keep their ChampionType
                    switch (statName) {
                        case "MaxHealth": vayneData.maxHealth = value; asheData.maxHealth = value; break;
                        case "HealthRegen": vayneData.healthRegen = value; asheData.healthRegen = value; break;
                        case "AD": vayneData.AD = value; asheData.AD = value; break;
                        case "AP": vayneData.AP = value; asheData.AP = value; break;
                        case "Armor": vayneData.armor = value; asheData.armor = value; break;
                        case "MagicResist": vayneData.magicResist = value; asheData.magicResist = value; break;
                        case "AttackSpeed": vayneData.attackSpeed = value; asheData.attackSpeed = value; break;
                        case "MovementSpeed": vayneData.movementSpeed = value; asheData.movementSpeed = value; break;
                        case "MaxMana": vayneData.maxMana = value; asheData.maxMana = value; break;
                        case "ManaRegen": vayneData.manaRegen = value; asheData.manaRegen = value; break;
                        case "AbilityHaste": vayneData.abilityHaste = value; asheData.abilityHaste = value; break;
                        case "CritChance": vayneData.critChance = value; asheData.critChance = value; break;
                        case "CritDamage": vayneData.critDamage = value; asheData.critDamage = value; break;
                        case "ArmorPen": vayneData.armorPen = value; asheData.armorPen = value; break;
                        case "MagicPen": vayneData.magicPen = value; asheData.magicPen = value; break;
                        case "MissileSpeed": vayneData.missileSpeed = value; asheData.missileSpeed = value; break;
                        default: Debug.LogError($"Unknown stat name: {statName}"); break;
                    }
                }
            }
            vayneData.championType = "AD Range";
            asheData.championType = "AD Range2";

            GM.playerChampionsData[2] = vayneData;
            GM.playerChampionsData[3] = asheData;
            Debug.Log("Saved Vayne: " + JsonUtility.ToJson(vayneData, true));
            Debug.Log("Saved Ashe: " + JsonUtility.ToJson(asheData, true));
        }
        else
        {
            var champData = new ChampionData(); // Create a new instance of ChampionData
            if (vayneSelected) { index = 2; champData = GM.playerChampionsData[index]; }
            else if (asheSelected){ index = 3; champData = GM.playerChampionsData[index]; }
            else return; // Exit if no champion is selected

            foreach (GameObject champModifier in champModifiers)
            {
                Slider slider = champModifier.GetComponentInChildren<Slider>();
                if (slider != null)
                {
                    float value = slider.value;
                    string statName = champModifier.name;
                    switch (statName)
                    {
                        case "MaxHealth": champData.maxHealth = value; break;
                        case "HealthRegen": champData.healthRegen = value; break;
                        case "AD": champData.AD = value; break;
                        case "AP": champData.AP = value; break;
                        case "Armor": champData.armor = value; break;
                        case "MagicResist": champData.magicResist = value; break;
                        case "AttackSpeed": champData.attackSpeed = value; break;
                        case "MovementSpeed": champData.movementSpeed = value; break;
                        case "MaxMana": champData.maxMana = value; break;
                        case "ManaRegen": champData.manaRegen = value; break;
                        case "AbilityHaste": champData.abilityHaste = value; break;
                        case "CritChance": champData.critChance = value; break;
                        case "CritDamage": champData.critDamage = value; break;
                        case "ArmorPen": champData.armorPen = value; break;
                        case "MagicPen": champData.magicPen = value; break;
                        case "MissileSpeed": champData.missileSpeed = value; break;
                        default: Debug.LogError($"Unknown stat name: {statName}"); break;
                    }
                }
            }
            GM.playerChampionsData[index] = champData;
            Debug.Log("Saved " + champData.championType + ": " + JsonUtility.ToJson(champData, true));
        }
        statSaved = true; // Set the flag to true after saving
    }
    #endregion
}
