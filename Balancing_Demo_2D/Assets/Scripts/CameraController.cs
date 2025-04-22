using UnityEngine;
using Unity.Netcode;

public class CameraController : MonoBehaviour
{
    public Transform target; // The player to follow
    public Vector3 offset = new Vector3(0f, 10f, -10f); // Offset from the player
    private Camera personalCamera;

    void Start()
    {

        personalCamera = GetComponent<Camera>();

        if (target != null && target.GetComponent<NetworkBehaviour>().IsOwner)
        {
            if (personalCamera != null)
            {
                personalCamera.enabled = true;
                Debug.Log($"Camera enabled for local player: {personalCamera.name}");
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

    void LateUpdate()
    {
        if (target != null)
        {
            // Follow the player's position with the specified offset
            transform.position = target.position + offset;

            // Lock the camera's rotation
            transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
    }
}