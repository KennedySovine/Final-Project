using UnityEngine;
using Unity.Netcode;

public class CameraController : MonoBehaviour
{
    public Transform target; // The player to follow
    public Vector3 offset = new Vector3(0f, 10f, -10f); // Offset from the player
    private Camera personalCamera;

    [SerializeField] private float smallestSize = 10f; // Furthest distance from the player
    [SerializeField] private float largestSize = 30;  // Closest distance to the player

    void Start()
    {
        personalCamera = GetComponent<Camera>();

        if (target != null && target.GetComponent<NetworkBehaviour>().IsOwner)
        {
            if (personalCamera != null)
            {
                personalCamera.enabled = true;
                Debug.Log($"Camera enabled for local player: {personalCamera.name}");

                // Make the camera face toward Y = 0
                FaceTowardsYZero();
            }
            else
            {
                Debug.LogError("No camera found on the player's prefab!");
            }
        }
        else
        {
            if (personalCamera != null)
            {
                personalCamera.enabled = false;
                Debug.Log("Camera disabled for non-local player.");
            }
        }
    }

    void Update()
    {
        if (target != null && target.GetComponent<NetworkBehaviour>().IsOwner)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (scrollInput != 0f)
            {
                // Adjust the orthographic size based on scroll input
                personalCamera.orthographicSize -= scrollInput * 5f; // Multiply by a factor to control zoom speed

                // Clamp the orthographic size to stay within the smallest and largest sizes
                personalCamera.orthographicSize = Mathf.Clamp(personalCamera.orthographicSize, smallestSize, largestSize);

                Debug.Log($"Zoom updated: orthographicSize = {personalCamera.orthographicSize}");
            }
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Follow the player's position with the specified offset
            transform.position = target.position + offset;

            // Lock the camera's rotation
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private void FaceTowardsYZero()
    {
        // Calculate the direction from the camera's position to Y = 0
        Vector3 directionToYZero = new Vector3(transform.position.x, 0f, transform.position.z) - transform.position;

        // Set the camera's rotation to face Y = 0
        if (directionToYZero != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToYZero, Vector3.up);
            transform.rotation = targetRotation;
        }
    }
}