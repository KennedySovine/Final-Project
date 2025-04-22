using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;

public class AugmentManager : MonoBehaviour
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

        if (NetworkManager.Singleton.IsServer && silverAugments.Count == 0) // Only load augments on server and if they haven't been loaded yet
        {
            Debug.Log("Loading augments...");
            // Load augments from JSON file
            LoadAugments();
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

    private Augment augmentFromID (int ID){
        foreach (Augment augment in allAugments) {
            if (augment.id == ID){
                Debug.Log($"Augment found: {augment.name}");
                return augment;
            }
        }
        return null; // Return null if no augment is found with the given ID
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

    //Add Augments to UI for Choosing
    // Send to specified clients only
    [Rpc(SendTo.SpecifiedInParams)]
    public void loadAugmentsClientRpc(RpcParams rpcParams){
        augmentUI.SetActive(true); // Show the augment UI

        List<Augment> augOptions = new List<Augment>(); // List to hold augment choices

        //Loads augments to the UI
        for (int i = 0; i < augmentUIList.Count; i++)
        {
            int randomIndex = Random.Range(0, 100); //random number to choose augment rarity
            Augment chosenAugment = null;
            switch (randomIndex){
                case < 50:
                    chosenAugment = silverAugments[Random.Range(0, silverAugments.Count)];
                    break;
                case < 80:
                    chosenAugment = goldAugments[Random.Range(0, goldAugments.Count)];
                    break;
                case < 100:
                    chosenAugment = prismaticAugments[Random.Range(0, prismaticAugments.Count)];
                    break;
            }

            augOptions.Add(chosenAugment); // Add the chosen augment to the list

            //Access augment name and description from the chosen augment
            TextMeshProUGUI augmentName = augmentUIList[i].transform.Find("AugName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI augmentDescription = augmentUIList[i].transform.Find("AugDesc").GetComponent<TextMeshProUGUI>();

            augmentName.text = chosenAugment.name; // Set the name text
            augmentDescription.text = chosenAugment.description; // Set the description text
        }
        
    }

    // Send to server
    [Rpc(SendTo.Server)]
    public void sendAugmentChoiceServerRpc(int augmentID, RpcParams rpcParam = default){
        ulong SenderClientID = rpcParams.Receive.SenderClientId; // Get the client ID of the sender
        // Aug choice for player1
        if (GM.player1ID == SenderClientID){
            GM.player1Augments.Add(augmentID); // Add the chosen augment to player1's list
        }
        // Aug choice for player2
        else if (GM.player2ID == SenderClientID){
            GM.player2Augments.Add(augmentID); // Add the chosen augment to player2's list
        }
 
    }

    // Function for button click

    public void augmentSelection(int augID){
        sendAugmentChoiceServerRpc(augID); // Send the augment choice to the server
        Debug.Log($"Augment {augID} selected!"); // Log the selected augment ID
    }
}
