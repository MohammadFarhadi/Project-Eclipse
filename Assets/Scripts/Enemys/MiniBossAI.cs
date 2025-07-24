using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
public class MiniBossAI : NetworkBehaviour, InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    [Header("Movement")]
    public float moveSpeed = 2f;
    private string currentTargetTag = "PointB";
    private Transform lastPatrolTarget;
    [Header("Players")]
    public Transform player1;
    public Transform player2;
    public float detectionRange = 5f;
    private Transform currentTargetPlayer;
    [Header("Shooting")]
    public GameObject normalBulletPrefab;
    public GameObject fireBulletPrefab;
    public Transform firePoint;
    public float maxHealth = 100f;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float normalFireRate = 2f;
    public float enragedFireRate = 0.8f;
    private float nextFireTime = 0f;
    [Header("Animation")]
    public Animator animator;
    protected NetworkAnimator networkAnimator;
    [Header("Health Bar")]
    public EnemyHealthBarDisplay healthBarDisplay;
    [SerializeField] private GameObject Key;
    [SerializeField] private GameObject Sonin;
    private BulletPool bulletPool;
    [SerializeField] private BulletPoolNGO bulletPoolNGO;
    public Animator targetAnimator; // Animator مربوط به شیء دیگر
    public string triggerBoolName = "Boss Is Dead"; 
    public void  InitializePlayers()
    {
        if (player1 != null && player2 != null) return;
        // منتظر بمون تا پلیرها در صحنه پیدا بشن (مثلاً حداکثر ۵ ثانیه)
        float timeout = 5f;
        float timer = 0f;

        while ((player1 == null || player2 == null) && timer < timeout)
        {
            GameObject p1 = GameObject.Find("RangedPlayer(Clone)") ?? GameObject.Find("Ranged1Player(Clone)");
            GameObject p2 = GameObject.Find("Melle1Player(Clone)") ?? GameObject.Find("Melle2Player(Clone)");

            if (p1 != null) player1 = p1.transform;
            if (p2 != null) player2 = p2.transform;

            timer += Time.deltaTime;

        }

        // اگه هنوز نال بودن، یک هشدار لاگ کن
        if (player1 == null || player2 == null)
            Debug.LogWarning("Players could not be found within the timeout!");

        healthBarDisplay.gameObject.SetActive(false);
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
        bulletPool = FindFirstObjectByType<BulletPool>();
    }
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
        InitializePlayers();
        currentTargetPlayer = GetClosestPlayer();

        if (currentTargetPlayer != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, currentTargetPlayer.position);

            if (distanceToPlayer <= detectionRange)
            {
                FacePlayer(currentTargetPlayer);
                animator.SetBool("run", false);
                animator.SetTrigger("shooting");
                Shoot();
                return;
            }
        }

        Patrol();
    }
    Transform GetClosestPlayer()
    {
       
        float distance1 = Vector2.Distance(transform.position, player1.position);
        float distance2 = Vector2.Distance(transform.position, player2.position);

        if (distance1 <= detectionRange || distance2 <= detectionRange)
        {
            return (distance1 < distance2) ? player1 : player2;
        }

        return null;
    }
    void Patrol()
    {
        animator.SetBool("run", true);
        animator.ResetTrigger("shooting");

        // حرکت به سمت جلو طبق جهت نگاه
        Vector2 moveDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    void FacePlayer(Transform player)
    {
        if (player.position.x < transform.position.x && transform.localScale.x > 0)
            Flip();
        else if (player.position.x > transform.position.x && transform.localScale.x < 0)
            Flip();
    }
    void Shoot()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            if (Time.time >= nextFireTime && bulletPool != null)
            {
                GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
                attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
                bool isEnraged = currentHealth.Value <= maxHealth / 2;
                string bulletTag = isEnraged ? "FireBullet" : "Bullet";
                float fireRate = isEnraged ? enragedFireRate : normalFireRate;
                GameObject bullet = bulletPool.GetBullet(bulletTag);
                if (bullet != null)
                {
                    bullet.transform.position = firePoint.position;
                    bullet.transform.rotation = Quaternion.identity;
                    Vector2 direction = (currentTargetPlayer.position - firePoint.position).normalized;
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    Vector3 bulletScale = rb.transform.localScale;
                    bulletScale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(bulletScale.x);
                    float bulletSpeed = 60f;
                    rb.transform.localScale = bulletScale;
                    rb.linearVelocity = direction * bulletSpeed;
                    Bullet bulletScript = bullet.GetComponent<Bullet>();
                    if (bulletScript != null)
                    {
                        bulletScript.SetAttacker(this.transform);
                    }
                    nextFireTime = Time.time + fireRate;
                }
            } 
        }
        else
        {
            if (IsServer)
            {
                ShootServerRpc();
            }
        }
    }
    public void TakeDamage(int damage, Transform attacker)
    {
        healthBarDisplay.gameObject.SetActive(true);
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                currentHealth.Value -= damage * 5;
                currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
            }
            else
            {
                ApplyDamageServerRpc(damage * 5);
            }
        }
        else
        {
            currentHealth.Value -= damage * 5;
            currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
        }
        
        
        animator.SetTrigger("hurt");
        int spriteIndex = Mathf.CeilToInt(currentHealth.Value / 10f) - 1;
        healthBarDisplay.UpdateHealthBar(spriteIndex);
        healthBarDisplay.gameObject.SetActive(true);

        if (currentHealth.Value <= 0)
        {
            animator.SetTrigger("Death");
            Invoke(nameof(Die), 0.2f); 
        }
    }
    public void Die()
    {
        GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
        if (targetAnimator != null)
        {
            targetAnimator.SetBool(triggerBoolName, true);
        }
        Vector3 spawnPosition = transform.position + new Vector3(0f, 1f, 0f); // یک واحد بالاتر
        GameObject soninDrop = Instantiate(Key, spawnPosition, Quaternion.identity);
        GameObject dropped = Instantiate(Sonin, transform.position, Quaternion.identity);
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (dropped.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn();
            }

           
            if (soninDrop.TryGetComponent(out NetworkObject soninNet))
            {
                soninNet.Spawn();
            }
        }
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
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
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PointA") || other.CompareTag("PointB"))
        {
            Flip(); // تغییر جهت
            currentTargetTag = other.tag;
            lastPatrolTarget = other.transform;
        }
        else if (other.CompareTag("Player"))
        {
            PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
            player.HealthSystem(50, false);
        }
    }
    [ClientRpc]
    private void DestroyObjectClientRpc()
    {
        Destroy(gameObject);
    }
    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc()
    {
        if (Time.time >= nextFireTime && bulletPoolNGO != null)
        {
            GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);

            bool isEnraged = currentHealth.Value <= maxHealth / 2;
            string bulletTag = isEnraged ? "FireBullet" : "Bullet";
            float fireRate = isEnraged ? enragedFireRate : normalFireRate;

            GameObject bullet = bulletPoolNGO.GetBullet(bulletTag);
            if (bullet != null)
            {
                bullet.transform.position = firePoint.position;
                bullet.transform.rotation = Quaternion.identity;

                Vector2 direction = (currentTargetPlayer.position - firePoint.position).normalized;

                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                Vector3 bulletScale = rb.transform.localScale;
                bulletScale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(bulletScale.x);
                float bulletSpeed = 60f;
                rb.transform.localScale = bulletScale;

                rb.linearVelocity = direction * bulletSpeed;

                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetAttacker(this.transform);
                }

                NetworkObject netObj = bullet.GetComponent<NetworkObject>();

                // اطلاع به کلاینت‌ها
                InitShootBulletClientRpc(
                    netObj.NetworkObjectId,
                    firePoint.position,
                    direction,
                    bulletSpeed,
                    bulletScale.x,
                    this.GetComponent<NetworkObject>().NetworkObjectId
                );

                nextFireTime = Time.time + fireRate;
            }
        }
    }
    [ClientRpc]
    void InitShootBulletClientRpc(ulong bulletNetId, Vector3 spawnPosition, Vector2 dir, float bulletSpeed, float scaleX, ulong attackerNetId)
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
    void ApplyDamageServerRpc(int damageAmount)
    {
        currentHealth.Value -= damageAmount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
        
    }
    
}
