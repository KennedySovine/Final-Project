using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    private GameManager GM; // Reference to the GameManager

    public float ADDamage = 0f;
    public float APDamage = 0f;
    public float empoweredDamageBasedOnTargetHP = 0f; // Damage based on target's HP
    public float speed = 0f; // Speed of the bullet

    public float armorPenetration = 0f; // Armor penetration value
    public float magicPenetration = 0f; // Magic penetration value
    public float slowAmount = 0f; // Slow amount value
    public Vector3 targetPosition; 
    public float range = 0f; // Range of the bullet
    public ulong ownerId = 3;

    public bool isAutoAttack = false; // Flag to indicate if it's an auto attack
    public GameObject targetPlayer = null; // Reference to the target player object
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    if (!IsServer) return;

        // Ensure the bullet has a valid target
        if (targetPlayer == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move the bullet towards the target's current position
        Vector3 targetPosition = targetPlayer.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        /*if (empoweredDamageBasedOnTargetHP != 0f){
            // Calculate the damage based on the target's current HP
            float targetHP = targetPlayer.GetComponent<BaseChampion>().maxHealth.Value;
            empoweredDamageBasedOnTargetHP = targetHP * empoweredDamageBasedOnTargetHP; // Example: 10% of target's current HP as damage
        }*/

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        // Check if the bullet hit the target player
        if (collision.gameObject == targetPlayer)
        {
            Debug.Log("Bullet hit the target player: " + targetPlayer.name);
            var champion = collision.GetComponent<BaseChampion>();
            if (champion != null)
            {
                // NO CODE FOR SELF DAMAGE NEEDED
                // Apply slow effect to the target player if applicable
                if (slowAmount != 0f){
                    champion.ApplySlow(slowAmount, 2f); // Apply slow for 1 second
                }

                champion.TakeDamage(ADDamage, APDamage, empoweredDamageBasedOnTargetHP);
                Destroy(gameObject);

                
            }
        }
    }

    // Function to destroy prefab if it goes outside of range
}