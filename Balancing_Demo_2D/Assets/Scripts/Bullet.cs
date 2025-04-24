using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{

    public float ADDamage = 0f;
    public float APDamage = 0f;
    public float empoweredDamageBasedOnTargetHP = 0f; // Damage based on target's HP

    public float armorPenetration = 0f; // Armor penetration value
    public float magicPenetration = 0f; // Magic penetration value
    public Vector3 targetPosition; 
    public float range = 0f; // Range of the bullet
    public ulong ownerId = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Function to destroy prefab if it goes outside of range
}
