using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{

    public float damage = 0f;
    public bool isEmpowered = false; // Flag to check if the bullet is empowered

    public Vector3 targetPosition; 
    public ulong ownerId = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
