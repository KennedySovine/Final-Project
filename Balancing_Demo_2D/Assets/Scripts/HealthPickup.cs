using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float respawnTime = 30f; // 30 seconds respawn time
    [SerializeField] private float healPercentage = 0.15f; // 15% of max health

    public float disableTime = 0f;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("HealthPickup: OnTriggerEnter2D called with " + other.name);
        if (!other.CompareTag("Player")) return;

        var champion = other.GetComponent<BaseChampion>();
        Debug.Log("HealthPickup: OnTriggerEnter2D found champion: " + champion?.name);
        if (champion != null)
        {
            // Heal the champion by defined percentage of their max health
            champion.UpdateHealthRpc(champion.maxHealth.Value * healPercentage);
            
            // Record the time when we disable the pickup
            disableTime = Time.time;
            Debug.Log(disableTime);
            gameObject.SetActive(false); // Deactivate the pickup object
        }
        else
        {
            Debug.LogWarning("No BaseChampion component found on the player.");
        }
    }
}
