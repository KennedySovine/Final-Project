using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance

    [Header("Player References")]
    public BaseChampion player1;
    public BaseChampion player2;


    [Header("Game Settings")]
    public int maxPlayers = 2;
    public float gameTime = 120f; // Game duration in seconds

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
        SpawnChampions();
    }

    private void Update()
    {
        // Example: Countdown game time
        if (gameTime > 0)
        {
            gameTime -= Time.deltaTime;
        }
        else
        {
            EndGame();
        }
    }

    public void SpawnChampions()
    {
        // Spawn champions at each spawn point
        foreach (Transform spawnPoint in spawnPoints)
        {
            Instantiate(championPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    public void EndGame()
    {
        Debug.Log("Game Over!");
        // Add logic to handle end of the game (e.g., show results, restart, etc.)
    }
}