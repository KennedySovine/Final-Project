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

        if (Input.GetMouseButtonDown(1)) // Check if the right mouse button is pressed
        {
            //Debug.Log("Right mouse button clicked.");
            targetPosition.x = mousePosition.x; // Set the target position's x coordinate
            targetPosition.y = mousePosition.y; // Set the target position's y coordinate
            Vector3 direction = targetPosition - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            champion.transform.rotation = Quaternion.Euler(0, 0, angle); // Rotate the player to face the mouse position
        }

        if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is pressed
        {
            //Debug.Log("Left mouse button clicked.");
            // Perform the attack action here
            // Basic Attack
            //champion.UseAbility1(); // Call the UseAbility1 method from the champion script
            PerformAutoAttack(); // Call the PerformAutoAttack method to perform the auto attack

        }

        if (transform.position != targetPosition){
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5f); // Move the player towards the mouse position
        }
        else
        {
            champion.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Set the linear velocity to 0 when the player reaches the target position
        }

    }
    private void PerformAutoAttack()
    {
        // Perform a raycast to check if the enemy is hit
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == champion.enemyChampion)
        {
            Debug.Log("Raycast hit the enemy champion!");
            champion.PerformAutoAttackServerRpc(mousePosition);
        }
        else
        {
            Debug.Log("Raycast did not hit the enemy champion.");
        }
    }
}