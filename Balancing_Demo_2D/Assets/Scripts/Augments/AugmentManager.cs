using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Unity.Netcode;

public class AugmentManager : MonoBehaviour
{
    public List<Augment> silverAugments = new List<Augment>();
    public List<Augment> goldAugments = new List<Augment>();
    public List<Augment> prismaticAugments = new List<Augment>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton.IsServer && silverAugments.Count == 0) // Only load augments on server and if they haven't been loaded yet
        {
            Debug.Log("Loading augments...");
            // Load augments from JSON file
            LoadAugments();
            PrintAugments(); //Testing
        }
    }

     private void LoadAugments()
    {
        // Load the JSON file from the Resources folder
        TextAsset jsonFile = Resources.Load<TextAsset>("Augments"); // File name without extension
        if (jsonFile != null)
        {
            // Deserialize the JSON into a list of Augments
            List<Augment> allAugments = JsonUtility.FromJson<AugmentListWrapper>("{\"augments\":" + jsonFile.text + "}").augments;

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
}
