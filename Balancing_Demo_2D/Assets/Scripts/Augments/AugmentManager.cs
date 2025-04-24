using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;

public class AugmentManager : NetworkBehaviour
{
    private GameManager GM;

    [Header("Augment Lists")]
    public List<Augment> silverAugments = new List<Augment>();
    public List<Augment> goldAugments = new List<Augment>();
    public List<Augment> prismaticAugments = new List<Augment>();


    [Header("UI Elements")]
    private List<GameObject> augmentUIList = new List<GameObject>(); // List to hold UI elements for augments
    public GameObject augmentUI; // Reference to the UI element for displaying augments
    public GameObject augment1; //Prefab for augment 1
    public GameObject augment2; //Prefab for augment 2
    public GameObject augment3; //Prefab for augment 3
    private List<Augment> allAugments = new List<Augment>(); // List to hold all augments
    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance

        LoadAugments();

        if (NetworkManager.Singleton.IsServer && silverAugments.Count == 0) // Only load augments on server and if they haven't been loaded yet
        {
            Debug.Log("Loading augments...");
            // Load augments from JSON file
            PrintAugments(); //Testing
        }

        augmentUI.SetActive(false); // Hide the augment UI at the start
        augmentUIList.Add(augment1); // Add augment UI elements to the list
        augmentUIList.Add(augment2);
        augmentUIList.Add(augment3);
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetFirstTwoDigits_Math(int value){
        int v = Mathf.Abs(value);
        int digits = (int)Mathf.Floor(Mathf.Log10(v)) + 1;
        if (digits <= 2) return v;
        // divide off the trailing (digits−2) places
        return v / (int)Mathf.Pow(10, digits - 2);
    }

     private void LoadAugments()
    {
        // Load the JSON file from the Resources folder
        TextAsset jsonFile = Resources.Load<TextAsset>("Augments"); // File name without extension
        if (jsonFile != null)
        {
            // Deserialize the JSON into a list of Augments
            allAugments = JsonUtility.FromJson<AugmentListWrapper>("{\"augments\":" + jsonFile.text + "}").augments;

            // Categorize augments by rarity
            foreach (Augment augment in allAugments)
            {
                switch (augment.rarity)
                {
                    case 1:
                        silverAugments.Add(augment);
                        break;
                    case 2:
                        goldAugments.Add(augment);
                        break;
                    case 3:
                        prismaticAugments.Add(augment);
                        break;
                    default:
                        Debug.LogWarning($"Unknown rarity: {augment.rarity} for augment {augment.name}");
                        break;
                }
            }

            Debug.Log("Augments loaded and categorized by rarity!");
        }
        else
        {
            Debug.LogError("Augments.json file not found in Resources folder!");
        }
    }

    public List<Augment> augmentSelector()
    {
        var augOptions    = new List<Augment>();
        var usedPrefixes  = new HashSet<int>();
        int maxAttempts   = 100;       // avoid infinite loop

        for (int i = 0; i < augmentUIList.Count; i++)
        {
            Augment chosenAug = null;
            int prefix = 0;
            int attempts = 0;

            // keep rolling until we find one whose first‑two digits haven't been used
            do
            {
                attempts++;
                int roll = Random.Range(0, 100);
                if (roll < 50 && silverAugments.Count > 0)
                    chosenAug = silverAugments[Random.Range(0, silverAugments.Count)];
                else if (roll < 80 && goldAugments.Count > 0)
                    chosenAug = goldAugments[Random.Range(0, goldAugments.Count)];
                else if (prismaticAugments.Count > 0)
                    chosenAug = prismaticAugments[Random.Range(0, prismaticAugments.Count)];

                if (chosenAug == null)
                    break;  // no candidates in that rarity

                prefix = GetFirstTwoDigits_Math(chosenAug.id);

                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning($"Could not find unique augment for slot {i} after {maxAttempts} tries. " +
                                     $"Allowing duplicate prefix {prefix}.");
                    break;
                }
            }
            while (usedPrefixes.Contains(prefix));

            // record and add
            usedPrefixes.Add(prefix);
            augOptions.Add(chosenAug);
        }

        return augOptions;
    }

    public void augmentUISetup(List<Augment> augOptions){
        for (int i = 0; i < augmentUIList.Count; i++)
        {
            //Access augment name and description from the chosen augment
            TextMeshProUGUI augmentName = augmentUIList[i].transform.Find("AugName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI augmentDescription = augmentUIList[i].transform.Find("AugDesc").GetComponent<TextMeshProUGUI>();

            augmentName.text = augOptions[i].name; // Set the name text
            augmentDescription.text = augOptions[i].description; // Set the description text
        }
    }

    public Augment augmentFromID (int ID){
        Debug.Log($"Searching for augment with ID: {ID}"); // Log the ID being searched for
        foreach (Augment augment in allAugments) {
            if (augment.id == ID){
                Debug.Log($"Augment ID: {ID}"); // Log the ID of the found augment
                Debug.Log($"Augment found: {augment.name}");
                return augment;
            }
        }
        return null; // Return null if no augment is found with the given ID
    }

    private Augment augmentFromName (string name){
        foreach (Augment augment in allAugments) {
            if (augment.name == name){
                Debug.Log($"Augment found: {augment.name}");
                return augment;
            }
        }
        return null; // Return null if no augment is found with the given name
    }

    private void PrintAugments()
    {
        Debug.Log("Silver Augments:");
        foreach (var augment in silverAugments)
        {
            Debug.Log($"- {augment.name}: {augment.description}");
        }

        Debug.Log("Gold Augments:");
        foreach (var augment in goldAugments)
        {
            Debug.Log($"- {augment.name}: {augment.description}");
        }

        Debug.Log("Prismatic Augments:");
        foreach (var augment in prismaticAugments)
        {
            Debug.Log($"- {augment.name}: {augment.description}");
        }
    }

    // Wrapper class to handle JSON arrays
    [System.Serializable]
    private class AugmentListWrapper
    {
        public List<Augment> augments;
    }

    // Send to server
    [Rpc(SendTo.Everyone)]
    public void sendAugmentChoiceRpc(int augmentID, RpcParams rpcParam = default){
        ulong SenderClientID = rpcParam.Receive.SenderClientId; // Get the client ID of the sender
        // Aug choice for player1
        if (GM.player1ID == SenderClientID){
            GM.player1Augments.Add(augmentID); // Add the chosen augment to player1's list
        }
        // Aug choice for player2
        else if (GM.player2ID == SenderClientID){
            GM.player2Augments.Add(augmentID); // Add the chosen augment to player2's list
        }
        else{
            Debug.LogError($"Unknown player ID: {SenderClientID}"); // Log an error if the player ID is unknown
            return; // Exit the function if the player ID is not recognized
        }

        Debug.Log($"Player {SenderClientID} selected augment {augmentID} and it is being applied"); // Log the selected augment ID
        GM.applyAugments(SenderClientID); // Apply the selected augment to player stats

        if (GM.player1Augments.Count == GM.player2Augments.Count){
            Debug.Log("Both players have selected their augments!");
            GM.gamePaused = false; // Unpause the game
        }
        else{
    
        }

    }

    [Rpc(SendTo.Server)]
    public void sendAugmentChoiceServerRpc(int augmentID, ServerRpcParams rpcParams = default)
    {
        ulong senderClientID = rpcParams.Receive.SenderClientId; // Get the client ID of the sender

        // Determine which player's augment list to update
        if (GM.player1ID == senderClientID)
        {
            GM.player1Augments.Add(augmentID); // Add the chosen augment to player1's list
        }
        else if (GM.player2ID == senderClientID)
        {
            GM.player2Augments.Add(augmentID); // Add the chosen augment to player2's list
        }
        else
        {
            Debug.LogError($"Unknown player ID: {senderClientID}"); // Log an error if the player ID is unknown
            return; // Exit the function if the player ID is not recognized
        }

        Debug.Log($"Player {senderClientID} selected augment {augmentID} and it is being applied."); // Log the selected augment ID

        // Apply the selected augment to the player's stats
        GM.applyAugments(senderClientID);

        // Check if both players have selected their augments
        if (GM.player1Augments.Count == GM.player2Augments.Count)
        {
            Debug.Log("Both players have selected their augments!");
            GM.gamePaused = false; // Unpause the game
        }
    }

    // Function for button click

    public void augmentSelection(int augID){
        int augmenID = augmentFromName(augmentUIList[augID].transform.Find("AugName").GetComponent<TextMeshProUGUI>().text).id; // Get the augment ID from the selected UI element
        Debug.Log($"Augment ID: {augID}"); // Log the augment ID for debugging
        sendAugmentChoiceRpc(augmenID); // Send the augment choice to the server
        augmentUI.SetActive(false); // Hide the augment UI after selection
        //Debug.Log($"Augment {augID} selected!"); // Log the selected augment ID
    }
}
