using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ShooterEnemyAI : NetworkBehaviour, InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    public float moveSpeed = 2f;
    public float attackRange = 5f;
    public float shootCooldown = 1.5f;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Transform firePoint;

    public string normalBulletTag = "EnemyBullet";

    private Transform targetPlayer;
    private float lastShootTime = -Mathf.Infinity;
    private Rigidbody2D rb;
    private Animator animator;
    protected NetworkAnimator networkAnimator;

    private BulletPool bulletPool;
    [SerializeField] private BulletPoolNGO bulletPoolNGO;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bulletPool = FindObjectOfType<BulletPool>();

        // فرض می‌کنیم فقط یک پلیر داریم
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
        }
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
        if (!IsServer && GameModeManager.Instance.CurrentMode == GameMode.Online) return;

        FindClosestPlayer(); // اضافه شده

        if (targetPlayer == null) return;

        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        if (distance > attackRange)
        {
            // حرکت به سمت نزدیک‌ترین پلیر
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            // توقف و شلیک
            rb.linearVelocity = Vector2.zero;

            if (Time.time >= lastShootTime + shootCooldown)
            {
                Shoot();
                lastShootTime = Time.time;
            }
        }
    }


    void Shoot()
    {
        GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                ShootServerRpc();
            }
        }
        else
        {
            if (bulletPool == null || firePoint == null || targetPlayer == null) return;

            GameObject bullet = bulletPool.GetBullet(normalBulletTag);
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, targetPlayer.position - firePoint.position);

            Vector2 direction = (targetPlayer.position - firePoint.position).normalized;
            float bulletSpeed = 5f;

            Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
            if (rbBullet != null)
            {
                rbBullet.linearVelocity = direction * bulletSpeed;
            }

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttacker(this.transform);
            }
            
        }
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            animator.SetTrigger("Attack");
        }
        else
        {
            UpdateAnimatorTriggerParameterServerRpc( "Attack");
        }
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            currentHealth.Value -= damage;
        }
        else
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
        
        

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
        rb.linearVelocity = Vector2.zero;
        
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                UpdateAnimatorTriggerParameterServerRpc( "Death");
                DestroyObjectClientRpc();
                Destroy(gameObject , 1f );
            }
            
        }
        else
        {
            animator.SetTrigger("Death");
            Destroy(gameObject , 1f );
        }
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
    [ServerRpc(RequireOwnership = false)]

    protected void UpdateAnimatorTriggerParameterServerRpc( string parameterName)
    {
        networkAnimator.Animator.SetTrigger(parameterName);
    }
    [ServerRpc(RequireOwnership = false)]
    void ApplyDamageServerRpc(int damageAmount)
    {
        currentHealth.Value -= damageAmount;
        
    }
    [ClientRpc]
    private void DestroyObjectClientRpc()
    {
        Destroy(gameObject , 1f );
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc()
    {
        if (bulletPool == null || firePoint == null || targetPlayer == null) return;

        GameObject bullet = bulletPoolNGO.GetBullet(normalBulletTag);
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, targetPlayer.position - firePoint.position);

        Vector2 direction = (targetPlayer.position - firePoint.position).normalized;
        float bulletSpeed = 5f;

        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
        {
            rbBullet.linearVelocity = direction * bulletSpeed;
        }

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetAttacker(this.transform);
        }
        NetworkObject netObj = bullet.GetComponent<NetworkObject>();
        InitShootBulletClientRpc(
            netObj.NetworkObjectId,
            firePoint.position,
            direction,
            bulletSpeed,
            this.GetComponent<NetworkObject>().NetworkObjectId
        );
    }
    [ClientRpc]
    void InitShootBulletClientRpc(ulong bulletNetId, Vector3 spawnPosition, Vector2 dir, float bulletSpeed, ulong attackerNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(bulletNetId, out NetworkObject spawnedBullet))
        {
            spawnedBullet.transform.position = spawnPosition;

            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * bulletSpeed;

                Vector3 scale = spawnedBullet.transform.localScale;
                
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
    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            if (p == null) continue;

            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = p.transform;
            }
        }

        targetPlayer = closest;
    }

}
