using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
public class PlayerNetwork : NetworkBehaviour
{
    public Vector3 mousePosition; // Mouse position in world space
    public Vector3 targetPosition; // Target position for the player to move towards

    public Camera personalCamera;

    public BaseChampion champion; // Reference to the champion script

    private GameManager GM; // Reference to the GameManager

    void Start()
    {
        GM = GameManager.Instance; // Get the GameManager instance
        if (IsOwner)
        {
            Debug.Log("Local player spawned.");
            if (personalCamera != null)
            {
                personalCamera.enabled = true;
                //personalCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Lock the camera's rotation
                Debug.Log("Camera enabled for local player.");
            }
            else
            {
                Debug.LogError("No camera found on the player's prefab!");
            }

            // Initialize mousePosition to the player's starting position
            mousePosition = transform.position;
            targetPosition = transform.position; // Set the target position to the player's starting position
            targetPosition.z = 0; // Set the z coordinate to 0
        }
    }
    // Update is called once per frame
    void Update()
    {

        //If game paused, disable input

        if (!IsOwner) return; // Only the owner can control the player
        if (GM.gamePaused.Value) return; // If the game is paused, disable input

        //Constantly update mouse position
        mousePosition = personalCamera.ScreenToWorldPoint(Input.mousePosition); // Get the mouse position in world space
        mousePosition.z = 0; // Set the z coordinate to 0

        checkInputs(); // Check for player inputs

        if (transform.position != targetPosition){
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * champion.movementSpeed.Value); // Move the player towards the mouse position
        }
        else
        {
            champion.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Set the linear velocity to 0 when the player reaches the target position
        }

    }

    private void checkInputs(){
        if (Input.GetMouseButton(1)) // Check if the right mouse button is pressed
        {
            //Debug.Log("Right mouse button clicked.");
            targetPosition.x = mousePosition.x; // Set the target position's x coordinate
            targetPosition.y = mousePosition.y; // Set the target position's y coordinate
            Vector3 direction = targetPosition - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            UpdateRotationRpc(angle); // Server version of the rotation update
        }

        if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is pressed
        {
            //Debug.Log("Left mouse button clicked.");
            // Perform the attack action here
            // Basic Attack
            //champion.UseAbility1(); // Call the UseAbility1 method from the champion script
            PerformAutoAttack(); // Call the PerformAutoAttack method to perform the auto attack
        }

        if (Input.GetKeyDown(KeyCode.Q)) // Check if the Q key is pressed
        {
            Debug.Log("Ability 1 key pressed.");
            champion.UseAbility1Rpc(); 
        }

        if (Input.GetKeyDown(KeyCode.W)) // Check if the W key is pressed
        {
            Debug.Log("Ability 2 key pressed.");
            champion.UseAbility2Rpc(); 
        }

        if (Input.GetKeyDown(KeyCode.E)) // Check if the E key is pressed
        {
            Debug.Log("Ability 3 key pressed.");
            champion.UseAbility3Rpc(); 
        }
    }
    public void PerformAutoAttack()
    {
        // Perform a raycast to check if the enemy is hit
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && hit.collider.GetComponentInParent<NetworkObject>() != null)
        {
            Debug.Log("Raycast hit the enemy champion!");
            //AD ability 2 logic for stacks
            // Maxes at 3 and subsequent stacks refresh the timer
            if (champion.stackCount.Value >= 3){
                champion.stackStartTime.Value = Time.time; // Set the stack start time
            }
            else {
                champion.stackCount.Value += 1; // Increment the stack count
                champion.stackStartTime.Value = Time.time; // Set the stack start time
            }

            champion.lastAutoAttackTime.Value = Time.time;
            PerformAutoAttackRpc(mousePosition, hit.collider.GetComponentInParent<NetworkObject>().NetworkObjectId); // Call the PerformAutoAttackRpc method to perform the auto attack on the server
        }
        else
        {
            Debug.Log("Raycast did not hit the enemy champion.");
        }
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateRotationRpc(float angle)
    {
        champion.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    [Rpc(SendTo.Server)]
    public void PerformAutoAttackRpc(Vector3 targetPosition, ulong targetNetworkObjectId)
    {
        if (!IsServer) return; // Only the server can execute this logic

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetObj)){
            Debug.LogWarning("Invalid target object id!");
            return;
        }

        GameObject enemyChampion = champion.enemyChampion; // Get the enemy champion reference

        // Validate the target
        if (enemyChampion == null)
        {
            Debug.LogWarning("No enemy champion assigned.");
            return;
        }

        // Check range
        float distance = Vector2.Distance(transform.position, enemyChampion.transform.position);
        if (distance > champion.autoAttack.range)
        {
            Debug.Log("Target out of range!");
            return;
        }

        // Check cooldown
        if (Time.time < champion.lastAutoAttackTime.Value + (1f /champion.attackSpeed.Value))
        {
            Debug.Log("Auto-attack is on cooldown!");
            return;
        }

        // Instantiate and configure the bullet
        GameObject bullet = Instantiate(champion.bulletPrefab, transform.position, Quaternion.identity, transform); // Parent to the champion
        bullet.SetActive(true); // Activate the bullet prefab
        var networkObject = bullet.GetComponent<NetworkObject>();
        var bulletComponent = bullet.GetComponent<Bullet>();

        if (networkObject == null || bulletComponent == null)
        {
            Debug.LogError("Bullet prefab is missing required components.");
            Destroy(bullet);
            return;
        }
        // Configure the bullet
        networkObject.SpawnWithOwnership(transform.parent.GetComponent<NetworkObject>().OwnerClientId);
        bulletComponent.ADDamage = champion.critLogic();
        bulletComponent.targetPosition = targetPosition;
        bulletComponent.targetPlayer = enemyChampion;

        if (champion.isEmpowered.Value){
            champion.empowerLogic(bullet); // Call the empower logic if empowered
            champion.isEmpowered.Value = false; // Reset the empowered state
        }

        if (champion.maxStacks.Value){
            champion.stackLogic(bullet); // Call the stack logic if max stacks are reached
            champion.maxStacks.Value = false; // Reset the max stacks flag
        }

        if (champion.ability3Used.Value){
            champion.ability3Logic(bullet); // Call the ability 3 logic if used
            champion.ability3Used.Value = false; // Reset the ability 3 used flag
        }
        
        Debug.Log("Bullet spawned on the server.");

        Debug.Log("Auto-attack performed!");

        // Update the last auto-attack time
    }
}