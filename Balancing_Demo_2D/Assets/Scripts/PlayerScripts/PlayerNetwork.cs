using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
public class PlayerNetwork : NetworkBehaviour
{
    public Vector3 mousePosition; // Mouse position in world space

    public Camera personalCamera;

    public BaseChampion champion; // Reference to the champion script

    void Start()
    {
        if (IsOwner)
        {
            Debug.Log("Local player spawned.");
            if (personalCamera != null)
            {
                personalCamera.enabled = true;
                personalCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Lock the camera's rotation
                Debug.Log("Camera enabled for local player.");
            }
            else
            {
                Debug.LogError("No camera found on the player's prefab!");
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return; // Only the owner can control the player

        if (Input.GetMouseButtonDown(1)) // Check if the right mouse button is pressed
        {
            mousePosition = personalCamera.ScreenToWorldPoint(Input.mousePosition); // Get the mouse position in world space
            mousePosition.z = -1; // Set z to 0 to keep it in 2D space
            Vector3 direction = mousePosition - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle); // Rotate the player to face the mouse position
        }

        if (transform.position != mousePosition){
            transform.position = Vector3.MoveTowards(transform.position, mousePosition, Time.deltaTime * 5f); // Move the player towards the mouse position
        }
    }
}