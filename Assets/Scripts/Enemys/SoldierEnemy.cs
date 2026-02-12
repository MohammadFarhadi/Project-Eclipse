using System;
using UnityEngine;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Random = UnityEngine.Random;


public class SoldierEnemy : NetworkBehaviour, InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private BulletPoolNGO bulletPoolNGO;
    [SerializeField] private string bulletTag = "SoldierBullet";
    [SerializeField] private string grenadeTag = "BombBullet";
    [SerializeField] private float bulletSpeed = 60f;
    [SerializeField] private GameObject[] dropItems; // Prefabs of Health/Stamina/Other pickups
    [SerializeField] private GameObject Sonin;
    [Header("Patrolling")]
    public float speed = 2f;
    public Transform leftLimit, rightLimit;
    public float waitTimeAtEnd = 1.5f;

    [Header("Combat")]
    public float detectionRange = 6f;
    public float fireRate = 1.2f;
    public Transform firePoint;
    public int maxHealth = 3;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public int HealthPoint => Mathf.RoundToInt(currentHealth.Value);

    private bool movingRight = true;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private float fireTimer = 0f;
    private int shotCount = 0;

    private Transform player;
    private Animator animator;
    protected NetworkAnimator networkAnimator;

    void Start()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                currentHealth.Value = maxHealth;
            }
        }
        else
        {
            currentHealth.Value = maxHealth;
        }
        animator = GetComponent<Animator>();
    }
    void Awake()
    {
        networkAnimator = GetComponent<NetworkAnimator>();
        if (bulletPool == null)
        {
            bulletPool = FindObjectOfType<BulletPool>();
            if (bulletPool == null)
            {
                Debug.LogError("BulletPool not found in the scene!");
            }
        }

        if (bulletPoolNGO == null)
        {
            bulletPoolNGO = FindObjectOfType<BulletPoolNGO>();
            if (bulletPoolNGO == null)
            {
                Debug.LogError("BulletPoolNGO not found in the scene!");
            }
        }
    }


    void Update()
    {
        FindClosestPlayer();

        if (currentHealth.Value <= 0) return;

        fireTimer += Time.deltaTime;

        if (PlayerInSight())
        {
            if (GameModeManager.Instance.CurrentMode == GameMode.Online)
            {
                UpdateAnimatorBoolParameterServerRpc("isRunning" , false );
            }
            else
            {
                animator.SetBool("isRunning", false);

            }
            FacePlayer();
            HandleCombat();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtEnd)
            {
                waitTimer = 0f;
                isWaiting = false;
                Flip();
            }
            return;
        }

        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            UpdateAnimatorBoolParameterServerRpc("isRunning" , true );
        }
        else
        {
            animator.SetBool("isRunning", true);

        }

        Vector3 dir = movingRight ? Vector3.right : Vector3.left;
        transform.Translate(dir * speed * Time.deltaTime);

        if ((movingRight && transform.position.x >= rightLimit.position.x) ||
            (!movingRight && transform.position.x <= leftLimit.position.x))
        {
            isWaiting = true;
            if (GameModeManager.Instance.CurrentMode == GameMode.Online)
            {
                UpdateAnimatorBoolParameterServerRpc("isRunning" , false );
            }
            else
            {
                animator.SetBool("isRunning", false);

            }
        }
    }

    bool PlayerInSight()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectionRange;
    }

    void HandleCombat()
    {
        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            shotCount++;

            if (shotCount >= 3)
            {
                if (GameModeManager.Instance.CurrentMode == GameMode.Online)
                {
                    UpdateAnimatorTriggerParameterServerRpc("ThrowGrenade");
                }
                else
                {
                    animator.SetTrigger("ThrowGrenade");
   
                }
                shotCount = 0; // fire happens inside animation event
            }
            else
            {
                if (GameModeManager.Instance.CurrentMode == GameMode.Online)
                {
                    UpdateAnimatorTriggerParameterServerRpc("Shoot");
                }
                else
                {
                    animator.SetTrigger("Shoot");

                }
                // fire happens in animation event
            }
        }
    }


    void FacePlayer()
    {
        if ((player.position.x < transform.position.x && transform.localScale.x > 0) ||
            (player.position.x > transform.position.x && transform.localScale.x < 0))
        {
            Flip();
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TakeDamage(int damage , Transform attacker)  
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                currentHealth.Value -= damage;
            }
            else
            {
                ApplyDamageServerRpc(damage);
            }
        }
        else
        {
            currentHealth.Value -= damage;
        }
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            UpdateAnimatorTriggerParameterServerRpc("Hit");
        }
        

        if (currentHealth.Value <= 0)
        {
            GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetTrigger("Die");
            }
            else
            {
                UpdateAnimatorTriggerParameterServerRpc("Die");
            }
            DropRandomItem();
            StartCoroutine(DeathEffect(GetComponent<SpriteRenderer>()));
        }
    }
    
    // Called by animation event
    public void FireBullet()
    {
        if (IsOnlineMode())
        {
            if (IsServer)
            {
                FireBulletServerRpc();
            }
        }
        else
        {
            GameObject bullet;
           
                bullet = bulletPool.GetBullet(bulletTag);

            
            if (bullet != null)
            {
                bullet.transform.position = firePoint.position;
                bullet.transform.rotation = Quaternion.identity;
                GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
                attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = (player.position - firePoint.position).normalized;
                    rb.linearVelocity = dir * bulletSpeed;

                    Vector3 scale = rb.transform.localScale;
                    scale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(scale.x);
                    rb.transform.localScale = scale;
                }

                Bullet bScript = bullet.GetComponent<Bullet>();
                if (bScript != null)
                {
                    bScript.SetAttacker(transform);
                }
            }
        }
        
    }

// Called by animation event
    public void ThrowGrenade()
    {
        if (IsOnlineMode())
        {
            if (IsServer)
            {
                ThrowGrenadeServerRpc();
            }
        }
        else
        {
            GameObject grenade;
            if (GameModeManager.Instance.CurrentMode == GameMode.Online)
            {
                grenade = bulletPoolNGO.GetBullet(grenadeTag);
            }
            else
            {
                grenade = bulletPool.GetBullet(grenadeTag);

            } // اول تعریف کن
            if (grenade != null)
            {
                grenade.transform.position = firePoint.position;
                grenade.transform.rotation = Quaternion.identity;

                Bullet bScript = grenade.GetComponent<Bullet>();
                if (bScript != null)
                {
                    bScript.SetAttacker(transform);
                }

                Grenade grenadeScript = grenade.GetComponent<Grenade>(); // بعد از تعریف grenade
                if (grenadeScript != null)
                {
                    grenadeScript.SetAttacker(transform);
                    grenadeScript.SetTarget(player.position); // تعیین هدف حرکت
                }
            }
        }
        
    }
    
    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = p.transform;
            }
        }

        player = closest;
    }
    
    IEnumerator DeathEffect(SpriteRenderer sr)
    {
        Color originalColor = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        float duration = 0.4f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (IsOnlineMode())
        {
            if (IsServer)
            {
                DestroyObjectClientRpc();
                Destroy(gameObject);
            }
            
        }
        else
        {
            Destroy(gameObject);
        }
    }  
    [ClientRpc]
    private void DestroyObjectClientRpc()
    {
        Destroy(gameObject);
    }
    public void DropRandomItem()
    {
        if (dropItems.Length == 0) return;

        if (IsOnlineMode())
        {
            if (!IsServer) return;

            int index = Random.Range(0, dropItems.Length);
            Vector3 spawnPosition = transform.position + new Vector3(0f, 1f, 0f);
            GameObject dropped = Instantiate(dropItems[index], spawnPosition, Quaternion.identity);
            if (dropped.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn();
            }

            GameObject soninDrop = Instantiate(Sonin, transform.position, Quaternion.identity);
            if (soninDrop.TryGetComponent(out NetworkObject soninNet))
            {
                soninNet.Spawn();
            }
        }
        else
        {
            int index = Random.Range(0, dropItems.Length);
            Instantiate(dropItems[index], transform.position + Vector3.up, Quaternion.identity);
            Instantiate(Sonin, transform.position, Quaternion.identity);
        }
    }   

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (IsOnlineMode())
            {
                if (!IsServer) return;

                PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
                player.HealthSystem(50, false);
                Debug.Log("Player hit");
            }
            else
            {
                PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
                player.HealthSystem(50, false);
                Debug.Log("Player hit");
            }
        }
    }
    private bool IsOnlineMode()
    {
        return GameModeManager.Instance != null && GameModeManager.Instance.CurrentMode == GameMode.Online;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void ThrowGrenadeServerRpc()
    {
        GameObject grenade = bulletPoolNGO.GetBullet(grenadeTag); // گرفتن نارنجک از استخر

        if (grenade != null)
        {
            grenade.transform.position = firePoint.position;
            grenade.transform.rotation = Quaternion.identity;

            NetworkObject netObj = grenade.GetComponent<NetworkObject>();

            Bullet bScript = grenade.GetComponent<Bullet>();
            if (bScript != null)
            {
                bScript.SetAttacker(transform);
            }

            Grenade grenadeScript = grenade.GetComponent<Grenade>();
            if (grenadeScript != null)
            {
                grenadeScript.SetAttacker(transform);
                grenadeScript.SetTarget(player.position); // تعیین هدف
            }

            // ارسال اطلاعات به همه کلاینت‌ها
            InitGrenadeClientRpc(
                netObj.NetworkObjectId,
                firePoint.position,
                player.position,
                this.GetComponent<NetworkObject>().NetworkObjectId
            );
        }
    }

    [ClientRpc]
    void InitGrenadeClientRpc(ulong grenadeNetId, Vector3 spawnPosition, Vector3 targetPosition, ulong attackerNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(grenadeNetId, out NetworkObject spawnedGrenade))
        {
            spawnedGrenade.transform.position = spawnPosition;

            Transform attacker = null;
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(attackerNetId, out NetworkObject attackerObj))
            {
                attacker = attackerObj.transform;
            }

            Bullet bScript = spawnedGrenade.GetComponent<Bullet>();
            if (bScript != null)
            {
                bScript.SetAttacker(attacker);
            }

            Grenade grenadeScript = spawnedGrenade.GetComponent<Grenade>();
            if (grenadeScript != null)
            {
                grenadeScript.SetAttacker(attacker);
                grenadeScript.SetTarget(targetPosition);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void FireBulletServerRpc()
    {
        GameObject bullet = bulletPoolNGO.GetBullet(bulletTag);
        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = Quaternion.identity;

            // Instantiate sound on server
            GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);

            NetworkObject netObj = bullet.GetComponent<NetworkObject>();
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            Vector2 dir = Vector2.zero;
            float bulletSpeedValue = bulletSpeed;
            float scaleX = 1f;

            if (rb != null)
            {
                dir = (player.position - firePoint.position).normalized;
                rb.linearVelocity = dir * bulletSpeed;

                Vector3 scale = rb.transform.localScale;
                scale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(scale.x);
                rb.transform.localScale = scale;

                scaleX = scale.x;
            }

            Bullet bScript = bullet.GetComponent<Bullet>();
            if (bScript != null)
            {
                bScript.SetAttacker(transform);
            }

            // اطلاع به کلاینت‌ها
            InitFireBulletClientRpc(
                netObj.NetworkObjectId,
                firePoint.position,
                dir,
                bulletSpeedValue,
                scaleX,
                this.GetComponent<NetworkObject>().NetworkObjectId
            );
        }
    }
    [ClientRpc]
    void InitFireBulletClientRpc(
        ulong bulletNetId,
        Vector3 spawnPosition,
        Vector2 dir,
        float bulletSpeed,
        float scaleX,
        ulong attackerNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(bulletNetId, out NetworkObject spawnedBullet))
        {
            spawnedBullet.transform.position = spawnPosition;

            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * bulletSpeed;

                Vector3 scale = spawnedBullet.transform.localScale;
                scale.x = scaleX;
                spawnedBullet.transform.localScale = scale;
            }   

            Transform attacker = null;
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(attackerNetId, out NetworkObject attackerObj))
            {
                attacker = attackerObj.transform;
            }

            Bullet bScript = spawnedBullet.GetComponent<Bullet>();
            if (bScript != null)
            {
                bScript.SetAttacker(attacker);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected void UpdateAnimatorTriggerParameterServerRpc(string parameterName)
    {
        networkAnimator.Animator.SetTrigger(parameterName);
    }
    [ServerRpc(RequireOwnership = false)]
    void ApplyDamageServerRpc(int damageAmount)
    {
        currentHealth.Value -= damageAmount;
    }
    [ServerRpc(RequireOwnership = false)]
    protected void UpdateAnimatorBoolParameterServerRpc( string parameterName, bool value)
    {
        networkAnimator.Animator.SetBool(parameterName, value);
    }
    [ServerRpc(RequireOwnership = false)]

    protected void UpdateAnimatorFloatParameterServerRpc( string parameterName, float value)
    {
        networkAnimator.Animator.SetFloat(parameterName, value);
    }


    public void SetHealth(int hp)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer) currentHealth.Value = hp;
            else ApplyDamageServerRpc(currentHealth.Value - hp); // or a dedicated SetHealthServerRpc
        }
        else
        {
            currentHealth.Value = hp;
        } 
    }


}
