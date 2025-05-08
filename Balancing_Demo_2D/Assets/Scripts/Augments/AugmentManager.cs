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
    
    // Cache for faster lookups
    private List<Augment> allAugments = new List<Augment>();
    private Dictionary<int, Augment> augmentsById = new Dictionary<int, Augment>();
    private Dictionary<string, Augment> augmentsByName = new Dictionary<string, Augment>();
    
    void Start()
    {
        GM = GameManager.Instance;

        LoadAugments();

        if (NetworkManager.Singleton.IsServer && silverAugments.Count == 0)
        {
            Debug.Log("Loading augments...");
            PrintAugments(); // Only in development builds
        }

        // Setup UI elements
        augmentUI.SetActive(false);
        augmentUIList.Add(augment1);
        augmentUIList.Add(augment2);
        augmentUIList.Add(augment3);
    }


    public int GetFirstTwoDigits_Math(int value)
    {
        if (value == 0) return 0;
        
        int v = Mathf.Abs(value);
        int digits = (int)Mathf.Floor(Mathf.Log10(v)) + 1;
        if (digits <= 2) return v;
        return v / (int)Mathf.Pow(10, digits - 2);
    }


    private void LoadAugments()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Augments");
        if (jsonFile == null)
        {
            Debug.LogError("Augments.json file not found in Resources folder!");
            return;
        }

        try
        {
            // Deserialize the JSON into a list of Augments
            allAugments = JsonUtility.FromJson<AugmentListWrapper>("{\"augments\":" + jsonFile.text + "}").augments;
            
            // Clear existing collections
            silverAugments.Clear();
            goldAugments.Clear();
            prismaticAugments.Clear();
            augmentsById.Clear();
            augmentsByName.Clear();

            // Categorize augments by rarity and build lookup dictionaries
            foreach (Augment augment in allAugments)
            {
                // Add to lookup dictionaries
                augmentsById[augment.id] = augment;
                augmentsByName[augment.name] = augment;
                
                // Categorize by rarity
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

            Debug.Log($"Loaded {allAugments.Count} augments: {silverAugments.Count} silver, {goldAugments.Count} gold, {prismaticAugments.Count} prismatic");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading augments: {e.Message}");
        }
    }

    public List<Augment> AugmentSelector()
    {
        var augOptions = new List<Augment>();
        var usedPrefixes = new HashSet<int>();
        int maxAttempts = 100;

        for (int i = 0; i < augmentUIList.Count; i++)
        {
            // Select appropriate rarity list based on probabilities
            List<Augment> candidateList = null;
            int roll = Random.Range(0, 100);
            
            if (roll < 50 && silverAugments.Count > 0)
                candidateList = silverAugments;
            else if (roll < 80 && goldAugments.Count > 0)
                candidateList = goldAugments;
            else if (prismaticAugments.Count > 0)
                candidateList = prismaticAugments;
            
            // Handle case where selected rarity has no augments
            if (candidateList == null || candidateList.Count == 0)
            {
                // Fallback to any non-empty list
                if (silverAugments.Count > 0) candidateList = silverAugments;
                else if (goldAugments.Count > 0) candidateList = goldAugments;
                else if (prismaticAugments.Count > 0) candidateList = prismaticAugments;
                else
                {
                    Debug.LogError("No augments available to select from!");
                    augOptions.Add(null); // Add null as placeholder
                    continue;
                }
            }
            
            // Try to find an augment with unique prefix
            Augment chosenAug = null;
            int prefix = 0;
            int attempts = 0;
            
            while (attempts < maxAttempts)
            {
                attempts++;
                int index = Random.Range(0, candidateList.Count);
                chosenAug = candidateList[index];
                prefix = GetFirstTwoDigits_Math(chosenAug.id);
                
                if (!usedPrefixes.Contains(prefix))
                    break;
                
                // If we've tried too many times, accept a duplicate
                if (attempts >= maxAttempts - 1)
                {
                    Debug.LogWarning($"Allowing duplicate prefix {prefix} after {attempts} attempts");
                    break;
                }
            }
            
            usedPrefixes.Add(prefix);
            augOptions.Add(chosenAug);
        }

        return augOptions;
    }


    public void AugmentUISetup(List<Augment> augOptions)
    {
        for (int i = 0; i < augmentUIList.Count; i++)
        {
            if (i >= augOptions.Count || augOptions[i] == null)
            {
                Debug.LogWarning($"No augment available for slot {i}");
                continue;
            }

            GameObject augmentElement = augmentUIList[i];
            TextMeshProUGUI nameText = augmentElement.transform.Find("AugName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = augmentElement.transform.Find("AugDesc")?.GetComponent<TextMeshProUGUI>();
            
            if (nameText != null) nameText.text = augOptions[i].name;
            if (descText != null) descText.text = augOptions[i].description;
        }
    }


    public Augment AugmentFromID(int id)
    {
        if (augmentsById.TryGetValue(id, out Augment augment))
            return augment;
            
        Debug.LogWarning($"Augment with ID {id} not found");
        return null;
    }

    private Augment AugmentFromName(string name)
    {
        if (augmentsByName.TryGetValue(name, out Augment augment))
            return augment;
            
        Debug.LogWarning($"Augment with name '{name}' not found");
        return null;
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

    [Rpc(SendTo.Server)]
    public void SendAugmentChoiceRpc(int augmentID, RpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            Debug.LogError("This RPC should only be executed on the server");
            return;
        }
        
        ulong senderClientID = rpcParams.Receive.SenderClientId;
        
        // Validate the augment ID exists
        Augment selectedAugment = AugmentFromID(augmentID);
        if (selectedAugment == null)
        {
            Debug.LogError($"Invalid augment ID: {augmentID}");
            return;
        }

        // Add to appropriate player's augment list
        if (GM.player1ID == senderClientID)
            GM.player1Augments.Add(augmentID);
        else if (GM.player2ID == senderClientID)
            GM.player2Augments.Add(augmentID);
        else
        {
            Debug.LogError($"Unknown player ID: {senderClientID}");
            return;
        }

        Debug.Log($"Player {senderClientID} selected '{selectedAugment.name}' (ID: {augmentID})");
        
        // Apply the selected augment
        GM.ApplyAugments(senderClientID);

        // Check if both players have selected their augments
        if (GM.player1Augments.Count == GM.player2Augments.Count)
        {
            Debug.Log("Both players have selected their augments, resuming game");
            GM.gamePaused.Value = false;
        }
    }

    public void AugmentSelection(int augID)
    {
        if (augID < 0 || augID >= augmentUIList.Count)
        {
            Debug.LogError($"Invalid augment UI index: {augID}");
            return;
        }
        
        TextMeshProUGUI nameText = augmentUIList[augID].transform.Find("AugName")?.GetComponent<TextMeshProUGUI>();
        if (nameText == null)
        {
            Debug.LogError("Could not find augment name text component");
            return;
        }
        
        Augment selectedAugment = AugmentFromName(nameText.text);
        if (selectedAugment == null)
        {
            Debug.LogError($"Could not find augment with name: {nameText.text}");
            return;
        }
        
        SendAugmentChoiceRpc(selectedAugment.id);
        augmentUI.SetActive(false);
    }
}
