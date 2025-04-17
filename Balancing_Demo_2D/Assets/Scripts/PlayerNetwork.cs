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

        if (Input.GetMouseButtonDown(1)) // Check if the right mouse button is pressed
        {
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Get the mouse position in world space
            mousePosition.z = 0; // Set z to 0 to keep it in 2D space
            Vector3 direction = mousePosition - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle); // Rotate the player to face the mouse position
        }

        if (transform.position != mousePosition){
            transform.position = Vector3.MoveTowards(transform.position, mousePosition, Time.deltaTime * 5f); // Move the player towards the mouse position
        }
    }
}