using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance

    [Header("Player References")]
    public BaseChampion player1;
    public BaseChampion player2;

    [Header("Game Settings")]
    public int maxPlayers = 2;
    public float gameTime = 120f; // Game duration in seconds
    public float augmentBuffer = 40f; //Choose aug every 40 seconds
    public bool augmentChosing = false; //If the player is choosing an augment, dont countdown the game time

    [Header("Champion Management")]
    public GameObject championPrefab; // Prefab for spawning champions
    public Transform[] spawnPoints; // Array of spawn points


    private void Awake()
    {
        // Ensure only one instance of GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("Game Manager Initialized");
    }

    private void Update()
    {
        // Countdown the game time
        if (augmentChosing){} //If the player is choosing an augment, dont countdown the game time
        else if (gameTime > 0)
        {
            gameTime -= Time.deltaTime;
        }
        else
        {
            EndGame();
        }

        if (augmentBuffer > 0)
        {
            augmentBuffer -= Time.deltaTime;
        }
        else
        {
            augmentLogic();
        }
    }

    public void augmentLogic(){
        augmentChosing = true; //Start the augment choosing process
            // UI LOGIC to show the augment options to the player
            // Augment randomization (including which ones pop up and the stats they will give)
            // After selection, reset the buffer time
            augmentBuffer = 40f;
            augmentChosing = false; //End the augment choosing process
    }

    public void EndGame()
    {
        Debug.Log("Game Over!");
        // Add logic to handle end of the game (e.g., show results, restart, etc.)
    }
}