using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Bullet : NetworkBehaviour
{
    private GameManager GM; // Reference to the GameManager
    public Vector3 targetPosition; 
    public GameObject targetPlayer = null; // Reference to the target player object
    public GameObject owner; // Reference to the owner of the bullet

    [Header("Bullet Settings")]

    public float ADDamage = 0f;
    public float APDamage = 0f;
    public float speed = 0f; // Speed of the bullet
    public float armorPenetration = 0f; // Armor penetration value
    public float magicPenetration = 0f; // Magic penetration value
    public float slowAmount = 0f; // Slow amount value
    public float range = 0f; // Range of the bullet
    public bool isAutoAttack = false; // Flag to indicate if it's an auto attack

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {  
        if (!IsServer) return; // Only the server controls bullet movement

        // Ensure the bullet has a valid target
        if (targetPlayer == null)
        {
            if (IsServer){ GetComponent<NetworkObject>().Despawn(); } // Destroy the bullet
            return;
        }

        // Move the bullet towards the target's current position
        Vector3 targetPosition = targetPlayer.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        // Check if the bullet hit the target player
        if (collision.gameObject == targetPlayer)
        {
            Debug.Log("Bullet hit the target player: " + targetPlayer.name);
            var champion = collision.GetComponent<BaseChampion>();
            var hasFrost = false;
            if (champion.slowAmount.Value != 0f) { hasFrost = true; }; // Check if the champion has a slow effect
            if (champion != null)
            {
                // NO CODE FOR SELF DAMAGE NEEDED
                // Apply slow effect to the target player if applicable
                if (slowAmount != 0f){
                    champion.applySlowRpc(slowAmount, 2f);
                    // Extra dmg for frost
                    if (hasFrost){
                        champion.TakeDamage(ADDamage + (owner.GetComponent<BaseChampion>().critChance.Value * 0.75f), APDamage); // Apply damage with crit chance (ASHE)
                    }
                }

                //HideBulletRpc(); // Hide the bullet on the server

                Debug.Log($"Bullet hit the target player: {targetPlayer.name}");
                Debug.Log($"Target Position: {transform.position}");
                Debug.Log($"Current position: {owner.name}");

                champion.TakeDamage(ADDamage, APDamage);
                GetComponent<NetworkObject>().Despawn();
                Debug.Log("Bullet despawned on the server.");
            }
        }
    }

    // Function to destroy prefab if it goes outside of range

    /*[Rpc(SendTo.NotServer)]
    private void HideBulletRpc()
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            Debug.Log("Bullet is out of range. Hiding it.");
            gameObject.SetActive(false); // Hide the bullet
            break; // Exit the loop after hiding the bullet
        }
    }

    private IEnumerator DelayedDespawn(float delay){
        yield return new WaitForSeconds(delay);
        if (IsServer)
            GetComponent<NetworkObject>().Despawn();
    }*/
}