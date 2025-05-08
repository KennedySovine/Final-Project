using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    #region Variables
    public Vector2 mousePosition; // Mouse position in world space
    public float dashSpeed;

    public NetworkVariable<Vector3> targetPositionNet = new NetworkVariable<Vector3>(); // Network variable for target position ONLY USED FOR DASHING
    public NetworkVariable<bool> isDashing = new NetworkVariable<bool>(false); // Network variable for dash state

    public Camera personalCamera;

    public BaseChampion champion; // Reference to the champion script
    private GameManager GM; // Reference to the GameManager

    private bool isCollidingWithTerrain = false; // Flag to check if colliding with terrain

    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false); // Network variable for movement state

    private Vector3 clickPosition; // Click position in world space || Used specifically for moving the player into range of the enemy 

    private bool cancelCurrentAction = false; // Flag to cancel the current action
    public Vector3 enemyPosition; // Position of the enemy champion

    private GameObject enemyChampion; // Reference to the enemy champion
    #endregion

    #region Unity Lifecycle Methods
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
        CheckInputs(); // Check for player inputs

        if (!isDashing.Value && !isCollidingWithTerrain) // If not dashing and not colliding with terrain
        {
            MovePlayer(); // Move the player if not dashing
        }

        //TODO: Check if colliding with terrain and if so stop moving
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            // Stop moving when colliding with terrain
            champion.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            Debug.Log("Collided with terrain. Stopping movement.");
            isCollidingWithTerrain = true; // Set the flag to true
        }
        isCollidingWithTerrain = false; // Reset the flag when not colliding with terrain
    }
    #endregion

    #region Input & Movement
    private void CheckInputs()
    {
        if (isDashing.Value) return; // Ignore inputs if the player is dashing

        if (Input.GetMouseButtonDown(1)) // Right mouse button pressed
        {
            cancelCurrentAction = true;
            if (AttackOrMove())
            {
                clickPosition = mousePosition; // Store the click position
                Debug.Log("Attack input detected.");
                PerformAutoAttack(); // Perform auto-attack
            }
        }

        if (Input.GetMouseButton(1)) // Right Mouse Button Pressed and Stay down
            {
            if (!AttackOrMove()){
                //Debug.Log("Move input detected.");
                SendMousePositionRpc(mousePosition); // Send mouse position to the server
                clickPosition = mousePosition; // Store the click position
                RequestMoveRpc(mousePosition); // Request movement on the server
                Vector3 direction = targetPositionNet.Value - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                UpdateRotationRpc(angle); // Update rotation
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Q)) // Q key pressed
        {
            Debug.Log("Ability 1 key pressed.");
            GM.UpdatePlayerAbilityUsedRpc(NetworkManager.Singleton.LocalClientId, "Q"); // Update ability 1 used state
            champion.UseAbility1Rpc();
        }

        if (Input.GetKeyDown(KeyCode.W)) // W key pressed
        {
            Debug.Log("Ability 2 key pressed.");
            GM.UpdatePlayerAbilityUsedRpc(NetworkManager.Singleton.LocalClientId, "W"); // Update ability 2 used state
            champion.UseAbility2Rpc();
        }

        if (Input.GetKeyDown(KeyCode.E)) // E key pressed
        {
            Debug.Log("Ability 3 key pressed.");
            GM.UpdatePlayerAbilityUsedRpc(NetworkManager.Singleton.LocalClientId, "E"); // Update ability 3 used state
            champion.UseAbility3Rpc();
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
    #endregion

    #region Combat & Abilities
    public bool AttackOrMove(){
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && hit.collider.GetComponentInParent<NetworkObject>() != null && (hit.collider.GetComponentInParent<NetworkObject>().OwnerClientId != champion.GetComponentInParent<NetworkObject>().OwnerClientId))
        {
            return true; // Attack
        }
        else
        {
            return false; // Move
        }
    }

    public void PerformAutoAttack()
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && hit.collider.GetComponentInParent<NetworkObject>() != null && hit.collider.GetComponentInParent<NetworkObject>().OwnerClientId != champion.GetComponentInParent<NetworkObject>().OwnerClientId)
        {
            enemyChampion = hit.collider.gameObject; // Get the enemy champion
            Debug.Log("Raycast hit the enemy champion!");

            // Start the coroutine to handle movement and attack
            StartCoroutine(MoveAndAttackCoroutine(enemyChampion));
        }
        else
        {
            Debug.Log("Raycast did not hit the enemy champion.");
        }
    }
    #endregion

    #region Network RPCs
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
        if (IsServer){
            isDashing.Value = true;
        }

        StartCoroutine(DashToTarget(targetPosition));
    }

    [Rpc(SendTo.Server)]
    public void PerformAutoAttackRpc(Vector3 targetPosition, ulong targetNetworkObjectId, int rapidFire)
    {
        if (!IsServer) return;
        // Enemy should already be in range before calling this function

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

        if (Time.time < champion.lastAutoAttackTime.Value + (1f / champion.attackSpeed.Value))
        {
            Debug.Log("Auto-attack is on cooldown!");
            Debug.Log($"Time: {Time.time}, Last Auto Attack Time: {champion.lastAutoAttackTime.Value}, Attack Speed: {champion.attackSpeed.Value}");
            return;
        }
        

        StartCoroutine(PerformAutoAttackCoroutine(targetPosition, enemyChampion, rapidFire));
        Debug.Log("Auto-attack performed on the server.");
    }

    [Rpc(SendTo.NotServer)]
    public void SpawnGhostBulletRpc(Vector3 targetPosition, Vector3 startPos, float speed = 10f)
    {
        GameObject ghostBullet = Instantiate(GM.ghostBulletPrefab, startPos, Quaternion.identity);

        if (ghostBullet == null)
        {
            Debug.LogError("Ghost bullet prefab is null. Cannot instantiate.");
            return; // Exit the coroutine if the ghost bullet prefab is null
        }

        StartCoroutine(MoveGhostBullet(ghostBullet, targetPosition, speed));
    }
    #endregion

    #region Coroutines & Utility Methods
    private IEnumerator MoveAndAttackCoroutine(GameObject enemyChampion)
    {
        cancelCurrentAction = false; // Reset the cancel flag

        while (!cancelCurrentAction)
        {
            enemyPosition = enemyChampion.transform.position; // Update the enemy position
            float distance = Vector2.Distance(transform.position, enemyPosition); // Calculate distance to the enemy champion
            Vector3 direc = enemyPosition - transform.position;
            float angle = Mathf.Atan2(direc.y, direc.x) * Mathf.Rad2Deg;
            UpdateRotationRpc(angle); // Update rotation
            
            if (distance <= champion.autoAttack.range)
            {
                Debug.Log("Player is in range of the enemy champion.");
                // Stop movement
                SendMousePositionRpc(transform.position);
                RequestMoveRpc(transform.position);

                direc = targetPositionNet.Value - transform.position;
                angle = Mathf.Atan2(direc.y, direc.x) * Mathf.Rad2Deg;
                UpdateRotationRpc(angle); // Update rotation

                // Perform the auto-attack
                PerformAutoAttackRpc(enemyPosition, enemyChampion.GetComponentInParent<NetworkObject>().NetworkObjectId, champion.rapidFire.Value);
                yield break; // Exit the coroutine after attacking
            }
            
            // Move toward the enemy
            Debug.Log("Moving toward the enemy champion.");
            RequestMoveRpc(enemyPosition); // Request movement to the enemy position
            distance = Vector2.Distance(transform.position, enemyPosition); // Update distance to the enemy champion
            yield return null; // Wait for the next frame
        }

        Debug.Log("Action canceled by the player.");
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
            bulletComponent.armorPenetration = champion.armorPen.Value;
            bulletComponent.magicPenetration = champion.magicPen.Value;
            bulletComponent.targetPosition = targetPosition;
            bulletComponent.targetPlayer = enemyChampion;
            bulletComponent.speed = champion.missileSpeed.Value;
            bulletComponent.owner = this.gameObject; // Set the owner of the bullet

            Debug.Log("Bullet properties set.");

            SpawnGhostBulletRpc(targetPosition, transform.position, champion.missileSpeed.Value); // Spawn the ghost

            bullet = champion.CritLogic(bullet); // Apply crit logic

            if (champion.isEmpowered.Value)
            {
                bullet = champion.EmpowerLogic(bullet);
                champion.UpdateIsEmpoweredRpc(false);
            }

            if (champion.isMaxStacks && champion.maxStacks.Value == 3) //only for vayne
            {
                bullet = champion.StackLogic(bullet);
                champion.ResetStackCountRpc();
            }

            if (champion.ability3Used.Value)
            {
                bullet = champion.Ability3Logic(bullet);
                champion.UpdateAbility3UsedRpc(false); // Reset the ability 3 used state
            }

            Debug.Log("Bullet spawned on the server.");
            Debug.Log("Auto-attack performed.");
            // Update the last auto-attack time after firing each bullet
            champion.UpdateStackCountRpc(1, champion.stackCount.Value, champion.maxStacks.Value); // Update the stack count
            

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

    private IEnumerator MoveGhostBullet(GameObject GB, Vector3 targetPosition, float speed)
    {
        while (GB!= null && Vector3.Distance(GB.transform.position, targetPosition) > 0.1f)
        {
            //Debug.Log("Moving ghost bullet towards target position.");
            //Debug.Log("Speed: " + speed);
            GB.transform.position = Vector3.MoveTowards(GB.transform.position, targetPosition, speed * Time.deltaTime);
            Debug.Log("Ghost bullet position: " + GB.transform.position);
            yield return null;
        }

        Destroy(GB); // Destroy the ghost bullet after reaching the target position
    }
    #endregion
}