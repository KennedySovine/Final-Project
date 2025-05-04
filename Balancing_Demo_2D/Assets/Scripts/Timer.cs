using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI timerText; // Reference to the TextMeshProUGUI component for displaying the timer
    private GameManager GM;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        int minutes = Mathf.FloorToInt(GM.gameTime / 60); // Calculate the minutes
        int seconds = Mathf.FloorToInt(GM.gameTime % 60); // Calculate the seconds
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds); // Format the time as MM:SS
        
    }
}
