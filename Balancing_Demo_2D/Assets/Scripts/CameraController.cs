using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The player to follow
    public Vector3 offset = new Vector3(0f, 10f, -10f); // Offset from the player

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