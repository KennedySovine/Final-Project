using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
public class PlayerNetwork : NetworkBehaviour
{
    public Vector3 mousePosition; // Mouse position in world space

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return; // Only the owner can control the player

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Get the mouse position in world space
        mousePosition.z = 0; // Set z to 0 to keep it in 2D space

        if (Input.GetMouseButton(1)) // Check if the right mouse button is pressed
        {
            Vector3 direction = mousePosition - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle); // Rotate the player to face the mouse position

            Vector3 moveDirection = direction.normalized; // Normalize the direction for consistent movement
            transform.position += moveDirection * Time.deltaTime * 5f; // Move the player
        }
    }
}