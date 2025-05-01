using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public Vector2 mousePosition; // Mouse position in world space
    public float dashSpeed;

    public NetworkVariable<Vector3> targetPositionNet = new NetworkVariable<Vector3>(); // Network variable for target position
    public NetworkVariable<bool> isDashing = new NetworkVariable<bool>(false); // Network variable for dash state

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
                Debug.Log("Camera enabled for local player.");
            }
            else
            {
                Debug.LogError("No camera found on the player's prefab!");
            }

            // Initialize mousePosition and targetPositionNet
            mousePosition = transform.position;
            dashSpeed = champion.movementSpeed.Value;
            //Set velocity 0
            champion.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }

    void Update()
    {
        if (!IsOwner || GM.gamePaused.Value) return; // Only the owner can control the player and only if the game is not paused

        // Constantly update mouse position
        mousePosition = personalCamera.ScreenToWorldPoint(Input.mousePosition);
        checkInputs(); // Check for player inputs

        if (!isDashing.Value)
        {
            MovePlayer(); // Move the player if not dashing
        }
    }

    private void checkInputs()
    {
        if (isDashing.Value) return; // Ignore inputs if the player is dashing

        if (Input.GetMouseButton(1)) // Right mouse button pressed
        {
            SendMousePositionRpc(mousePosition); // Send mouse position to the server
            RequestMoveRpc(mousePosition); // Request movement on the server
            Vector3 direction = targetPositionNet.Value - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            UpdateRotationRpc(angle); // Update rotation
        }

        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
        {
            PerformAutoAttack(); // Perform an auto-attack
        }

        if (Input.GetKeyDown(KeyCode.Q)) // Q key pressed
        {
            Debug.Log("Ability 1 key pressed.");
            champion.UseAbility1Rpc();
        }

        if (Input.GetKeyDown(KeyCode.W)) // W key pressed
        {
            Debug.Log("Ability 2 key pressed.");
            champion.UseAbility2Rpc();
        }

        if (Input.GetKeyDown(KeyCode.E)) // E key pressed
        {
            Debug.Log("Ability 3 key pressed.");
            champion.UseAbility3Rpc();
        }
    }

    public void PerformAutoAttack()
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && hit.collider.GetComponentInParent<NetworkObject>() != null)
        {
            Debug.Log("Raycast hit the enemy champion!");
            

            PerformAutoAttackRpc(mousePosition, hit.collider.GetComponentInParent<NetworkObject>().NetworkObjectId, champion.rapidFire.Value); // Call the auto-attack function on the server
        }
        else
        {
            Debug.Log("Raycast did not hit the enemy champion.");
        }
    }

    private void MovePlayer()
    {
        float speed = champion.movementSpeed.Value;
        if (Vector2.Distance(transform.position, targetPositionNet.Value) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPositionNet.Value, Time.deltaTime * speed);
        }
        else
        {
            champion.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }

    [Rpc(SendTo.Server)]
    public void SendMousePositionRpc(Vector2 mousePos)
    {
        if (!IsServer) return;

        // Update the server's copy of the mouse position
        mousePosition = mousePos;
        //Debug.Log($"Mouse position received on server: {mousePosition}");
    }

    [Rpc(SendTo.Server)]
    public void RequestMoveRpc(Vector2 targetPosition)
    {
        if (!IsServer) return;
        targetPositionNet.Value = targetPosition; // Update the target position on the server
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateRotationRpc(float angle)
    {
        champion.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    [Rpc(SendTo.Everyone)]
    public void ChampionDashRpc(Vector2 mousePos, float maxRange, float speed)
    {
        Vector2 dashDirection = (mousePos - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, mousePos);
        distance = Mathf.Min(distance, maxRange);

        Vector2 targetPosition = (Vector2)transform.position + dashDirection * distance;
        targetPositionNet.Value = targetPosition;
        dashSpeed = speed;
        isDashing.Value = true;

        StartCoroutine(DashToTarget(targetPosition));
    }

    private IEnumerator DashToTarget(Vector2 targetPosition)
    {
        while (Vector2.Distance(transform.position, targetPositionNet.Value) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPositionNet.Value, Time.deltaTime * dashSpeed);
            yield return null;
        }

        transform.position = targetPositionNet.Value; // Snap to the target position
        isDashing.Value = false; // Reset the dash state
        Debug.Log("Dash completed.");
    }

    [Rpc(SendTo.Server)]
    public void PerformAutoAttackRpc(Vector3 targetPosition, ulong targetNetworkObjectId, int rapidFire)
    {
        if (!IsServer) return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetObj))
        {
            Debug.LogWarning("Invalid target object id!");
            return;
        }

        GameObject enemyChampion = champion.enemyChampion;
        if (enemyChampion == null)
        {
            Debug.LogWarning("No enemy champion assigned.");
            return;
        }

        float distance = Vector2.Distance(transform.position, enemyChampion.transform.position);
        if (distance > champion.autoAttack.range)
        {
            Debug.Log("Target out of range!");
            return;
        }

        if (Time.time < champion.lastAutoAttackTime.Value + (1f / champion.attackSpeed.Value))
        {
            Debug.Log("Auto-attack is on cooldown!");
            Debug.Log($"Time: {Time.time}, Last Auto Attack Time: {champion.lastAutoAttackTime.Value}, Attack Speed: {champion.attackSpeed.Value}");
            return;
        }

        StartCoroutine(PerformAutoAttackCoroutine(targetPosition, enemyChampion, rapidFire));
        Debug.Log("Auto-attack performed on the server.");
    }

    private IEnumerator PerformAutoAttackCoroutine(Vector3 targetPosition, GameObject enemyChampion, int rapidFire)
    {
        for (int i = 0; i < rapidFire; i++)
        {
            GameObject bullet = Instantiate(champion.bulletPrefab, transform.position, Quaternion.identity, transform);
            Debug.Log("Bullet instantiated.");
            if (bullet == null)
            {
                Debug.LogError("Bullet prefab is null. Cannot instantiate.");
                yield break; // Exit the coroutine if the bullet prefab is null
            }
            bullet.SetActive(true);
            var networkObject = bullet.GetComponent<NetworkObject>();
            var bulletComponent = bullet.GetComponent<Bullet>();

            if (networkObject == null || bulletComponent == null)
            {
                Debug.LogError("Bullet prefab is missing required components.");
                Destroy(bullet);
                yield break; // Exit the coroutine if the bullet is invalid
            }

            networkObject.SpawnWithOwnership(transform.parent.GetComponent<NetworkObject>().OwnerClientId);
            bulletComponent.ADDamage = champion.AD.Value;
            bulletComponent.targetPosition = targetPosition;
            bulletComponent.targetPlayer = enemyChampion;
            bulletComponent.speed = champion.missileSpeed.Value;
            bulletComponent.ownerID = transform.parent.GetComponent<NetworkObject>().NetworkObjectId;

            SpawnGhostBulletRpc(targetPosition, transform.position, champion.missileSpeed.Value); // Spawn the ghost

            bullet = champion.critLogic(bullet); // Apply crit logic

            if (champion.isEmpowered.Value)
            {
                bullet = champion.empowerLogic(bullet);
                champion.updateIsEmpoweredRpc(false);
            }

            if (champion.isMaxStacks && champion.maxStacks.Value == 3) //only for vayne
            {
                bullet = champion.stackLogic(bullet);
                champion.resetStackCountRpc();
            }

            if (champion.ability3Used.Value)
            {
                bullet = champion.ability3Logic(bullet);
                champion.updateAbility3UsedRpc(false); // Reset the ability 3 used state
            }

            Debug.Log("Bullet spawned on the server.");
            Debug.Log("Auto-attack performed.");
            champion.getAbilityUsedRpc().Stats.damage += bulletComponent.ADDamage; // Update the total damage dealt by the ability

            // Update the last auto-attack time after firing each bullet
            champion.updateStackCountRpc(1, champion.stackCount.Value, champion.maxStacks.Value); // Update the stack count
            

            // Wait for 0.1 seconds before firing the next bullet
            yield return new WaitForSeconds(0.1f);
        }

        // Reset rapid fire after all bullets are fired
        champion.lastAutoAttackTime.Value = Time.time; // Update the last auto-attack time
        rapidFire = 1;
    }

    private IEnumerator waitForSec(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }


    //GHOST BULLET
    [Rpc(SendTo.NotServer)]
    public void SpawnGhostBulletRpc(Vector3 targetPosition, Vector3 startPos, float speed = 10f)
    {
        GameObject ghostBullet = Instantiate(GM.ghostBulletPrefab, startPos, Quaternion.identity);

        StartCoroutine(MoveGhostBullet(ghostBullet, targetPosition, speed));
        
    }

    private IEnumerator MoveGhostBullet(GameObject GB, Vector3 targetPosition, float speed)
    {
        while (GB!= null && Vector3.Distance(GB.transform.position, targetPosition) > 0.1f)
        {
            Debug.Log("Moving ghost bullet towards target position.");
            Debug.Log("Speed: " + speed);
            GB.transform.position = Vector3.MoveTowards(GB.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        Destroy(GB); // Destroy the ghost bullet after reaching the target position
    }
}