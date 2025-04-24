using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    private GameManager GM; // Reference to the GameManager

    public float ADDamage = 0f;
    public float APDamage = 0f;
    public float empoweredDamageBasedOnTargetHP = 0f; // Damage based on target's HP

    public float armorPenetration = 0f; // Armor penetration value
    public float magicPenetration = 0f; // Magic penetration value
    public Vector3 targetPosition; 
    public float range = 0f; // Range of the bullet
    public ulong ownerId = 3;

    public bool isAutoAttack = false; // Flag to indicate if it's an auto attack
    public ulong targetId = 0; // ID of the target
    public GameObject targetPlayer = null; // Reference to the target player object
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Only 2 players at any time, one is 0 and the other is 1
        if (ownerId == 0){
            targetId = 1;
        }
        else{
            targetId = 0;
        }
        //auto attack always hits player.
        if (isAutoAttack){
            // Check if the bullet is an auto attack and has a target
            if (targetId != 0)
            {
                // Find the target object using the targetId
                targetPlayer = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetId].gameObject;
                if (targetPlayer != null)
                {
                    // Set the target position to the target object's position
                    targetPosition = targetObject.transform.position;
                }
            }
        }

        if (transform.position != targetPosition)
        {
            // Move the bullet towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 10f); // Adjust speed as needed
        }
        else
        {
            // Destroy the bullet if it reaches the target position
            Destroy(gameObject);
        }
    }

    // Function to destroy prefab if it goes outside of range
}
