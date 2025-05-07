using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public Vector2 mousePosition;
    public float dashSpeed;

    public NetworkVariable<Vector3> targetPositionNet = new NetworkVariable<Vector3>();
    public NetworkVariable<bool> isDashing = new NetworkVariable<bool>(false);

    public Camera personalCamera;

    public BaseChampion champion;
    private GameManager GM;
    private Rigidbody2D rb;

    void Start()
    {
        GM = GameManager.Instance;
        rb = champion.GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("No Rigidbody2D found on champion!");

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

            mousePosition = transform.position;
            dashSpeed = champion.movementSpeed.Value;
            rb.velocity = Vector2.zero;
        }
    }

    void Update()
    {
        if (!IsOwner || GM.gamePaused.Value) return;

        mousePosition = personalCamera.ScreenToWorldPoint(Input.mousePosition);
        checkInputs();
    }

    void FixedUpdate()
    {
        if (!IsOwner || GM.gamePaused.Value || isDashing.Value) return;

        Vector2 target = targetPositionNet.Value;
        Vector2 current = rb.position;
        float speed = champion.movementSpeed.Value;

        if (Vector2.Distance(current, target) > 0.05f)
        {
            Vector2 newPosition = Vector2.MoveTowards(current, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void checkInputs()
    {
        if (isDashing.Value) return;

        if (Input.GetMouseButtonDown(1))
        {
            if (AttackOrMove())
            {
                Debug.Log("Attack input detected.");
                PerformAutoAttack();
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (!AttackOrMove()){
                Debug.Log("Move input detected.");
                SendMousePositionRpc(mousePosition);
                RequestMoveRpc(mousePosition);
                Vector3 direction = targetPositionNet.Value - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                UpdateRotationRpc(angle);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Ability 1 key pressed.");
            GM.updatePlayerAbilityUsedRpc(NetworkManager.Singleton.LocalClientId, "Q");
            champion.UseAbility1Rpc();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Ability 2 key pressed.");
            GM.updatePlayerAbilityUsedRpc(NetworkManager.Singleton.LocalClientId, "W");
            champion.UseAbility2Rpc();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Ability 3 key pressed.");
            GM.updatePlayerAbilityUsedRpc(NetworkManager.Singleton.LocalClientId, "E");
            champion.UseAbility3Rpc();
        }
    }

    public bool AttackOrMove()
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && hit.collider.GetComponentInParent<NetworkObject>() != null && (hit.collider.GetComponentInParent<NetworkObject>().OwnerClientId != champion.GetComponentInParent<NetworkObject>().OwnerClientId))
        {
            return true;
        }
        return false;
    }

    public void PerformAutoAttack()
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && hit.collider.GetComponentInParent<NetworkObject>() != null && (hit.collider.GetComponentInParent<NetworkObject>().OwnerClientId != champion.GetComponentInParent<NetworkObject>().OwnerClientId))
        {
            Debug.Log("Raycast hit the enemy champion!");
            Vector3 direction = targetPositionNet.Value - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            UpdateRotationRpc(angle);

            PerformAutoAttackRpc(mousePosition, hit.collider.GetComponentInParent<NetworkObject>().NetworkObjectId, champion.rapidFire.Value);
            SendMousePositionRpc(transform.position);
            RequestMoveRpc(transform.position);
        }
        else
        {
            Debug.Log("Raycast did not hit the enemy champion.");
        }
    }

    [Rpc(SendTo.Server)]
    public void SendMousePositionRpc(Vector2 mousePos)
    {
        if (!IsServer) return;
        mousePosition = mousePos;
    }

    [Rpc(SendTo.Server)]
    public void RequestMoveRpc(Vector2 targetPosition)
    {
        if (!IsServer) return;
        targetPositionNet.Value = targetPosition;
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateRotationRpc(float angle)
    {
        champion.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    [Rpc(SendTo.Everyone)]
    public void ChampionDashRpc(Vector2 mousePos, float maxRange, float speed)
    {
        Vector2 dashDirection = (mousePos - rb.position).normalized;
        float distance = Vector2.Distance(rb.position, mousePos);
        distance = Mathf.Min(distance, maxRange);

        Vector2 targetPosition = rb.position + dashDirection * distance;
        targetPositionNet.Value = targetPosition;
        dashSpeed = speed;
        if (IsServer)
        {
            isDashing.Value = true;
        }

        StartCoroutine(DashToTarget(targetPosition));
    }

    private IEnumerator DashToTarget(Vector2 targetPosition)
    {
        while (Vector2.Distance(rb.position, targetPositionNet.Value) > 0.05f)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPositionNet.Value, Time.deltaTime * dashSpeed);
            rb.MovePosition(newPosition);
            yield return null;
        }

        rb.MovePosition(targetPositionNet.Value);
        isDashing.Value = false;
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
            bulletComponent.armorPenetration = champion.armorPen.Value;
            bulletComponent.magicPenetration = champion.magicPen.Value;
            bulletComponent.targetPosition = targetPosition;
            bulletComponent.targetPlayer = enemyChampion;
            bulletComponent.speed = champion.missileSpeed.Value;
            bulletComponent.owner = this.gameObject; // Set the owner of the bullet

            Debug.Log("Bullet properties set.");

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

        if (ghostBullet == null)
        {
            Debug.LogError("Ghost bullet prefab is null. Cannot instantiate.");
            return; // Exit the coroutine if the ghost bullet prefab is null
        }

        StartCoroutine(MoveGhostBullet(ghostBullet, targetPosition, speed));

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
}