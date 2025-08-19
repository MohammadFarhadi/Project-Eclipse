using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;

public class ShootingEnemy : NetworkBehaviour , InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public float bulletSpeed = 30f;
    private NetworkVariable<int> health = new NetworkVariable<int>(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public int HealthPoint => Mathf.RoundToInt(health.Value);

    


    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    private List<GameObject> playersInRange = new List<GameObject>();
    private GameObject currentTarget;
    private float timer = 0f;
    
    private BulletPool bulletPool;
    private BulletPoolNGO bulletPoolNGO;


    //  نمایش نوار سلامتی
    public EnemyHealthBarDisplay healthBarDisplay;
    [Header("Possible Drops")]
    [SerializeField] private GameObject[] dropItems; // Prefabs of Health/Stamina/Other pickups
    [SerializeField] private GameObject Sonin;
    [SerializeField] private float dropChance = 0.5f; // بین ۰ تا ۱ مثلا 0.5 یعنی ۵۰٪ احتمال اسپاون آیتم

    void Start()
    {
        bulletPool = FindFirstObjectByType<BulletPool>();

        health.OnValueChanged += OnHealthChanged;

        if (healthBarDisplay != null)
        {
            healthBarDisplay.UpdateHealthBar(health.Value);
        }

        if (bulletPoolNGO == null)
        {
            bulletPoolNGO = FindObjectOfType<BulletPoolNGO>();
            if (bulletPoolNGO == null)
            {
                Debug.LogError("BulletPoolNGO not found in the scene!");
            }
        }
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (healthBarDisplay != null)
        {
            healthBarDisplay.Show(newValue);
            healthBarDisplay.UpdateHealthBar(newValue);
        }
    }

    private void OnDestroy()
    {
        health.OnValueChanged -= OnHealthChanged;
    }


    void Update()
    {
        if (playersInRange.Count > 0)
        {
            if (currentTarget == null || !playersInRange.Contains(currentTarget))
            {
                currentTarget = playersInRange[Random.Range(0, playersInRange.Count)];
            }

            timer += Time.deltaTime;
            if (timer >= fireRate)
            {
                ShootAt(currentTarget);
                timer = 0f;
            }
        }
    }

    void ShootAt(GameObject target)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            if (target == null || firePoint == null || bulletPool == null) return;

            Vector2 dir = (target.transform.position - firePoint.position).normalized;
            GameObject bullet;
        
            bullet = bulletPool.GetBullet("Bullet");

        
        
            if (bullet != null) 
            {
                bullet.transform.position = firePoint.position;
                bullet.transform.rotation = Quaternion.identity;
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetTrigger("Attack");  

                }
                else
                {
                    UpdateAnimatorTriggerParameterServerRpc("Attack");
                }

                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector3 bulletScale = rb.transform.localScale;
                    bulletScale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(bulletScale.x);
                    rb.transform.localScale = bulletScale;
                    rb.linearVelocity = dir * bulletSpeed;
                }

                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetAttacker(this.transform);
                }
            }
            GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
        }
        else
        {
            if (IsServer)
            {
                ShootAtServerRpc(target);
            }
        }
        
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playersInRange.Contains(other.gameObject))
        {
            playersInRange.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInRange.Remove(other.gameObject);
            if (currentTarget == other.gameObject)
            {
                currentTarget = null;
            }
        }
    }

    public void TakeDamage(int damage , Transform attacker)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                health.Value -= damage;
            }
            else
            {
                ApplyDamageServerRpc(damage);
            }
        }
        else
        {
            health.Value -= damage;
        }
        
        if (healthBarDisplay != null)
        {
            healthBarDisplay.Show(health.Value);
            healthBarDisplay.UpdateHealthBar(health.Value);
        }
       


        if (health.Value <= 0)
        {
            if (GameModeManager.Instance.CurrentMode == GameMode.Online)
            {
                UpdateAnimatorTriggerParameterServerRpc("Die");
            }
            else
            {
                animator.SetTrigger("Die");

            }
            Invoke(nameof(Die), 0.5f); 
        }
    }

    public void Die()
    {
        GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
        DropRandomItem();
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
    public void DropRandomItem()
    {
        if (dropItems.Length == 0) return;

        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
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
    [ClientRpc]
    private void DestroyObjectClientRpc()
    {
        Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootAtServerRpc(NetworkObjectReference targetRef)
    {
        if (targetRef.TryGet(out NetworkObject targetObj))
        {
            Transform target = targetObj.transform;
            Vector2 dir = (target.position - firePoint.position).normalized;

            GameObject bullet = bulletPoolNGO.GetBullet("Bullet");
            if (bullet != null)
            {
                bullet.transform.position = firePoint.position;
                bullet.transform.rotation = Quaternion.identity;
                animator.SetTrigger("Attack");

                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                float scaleX = 1f;

                if (rb != null)
                {
                    Vector3 bulletScale = rb.transform.localScale;
                    bulletScale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(bulletScale.x);
                    rb.transform.localScale = bulletScale;
                    rb.linearVelocity = dir * bulletSpeed;

                    scaleX = bulletScale.x;
                }

                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetAttacker(this.transform);
                }

                // صدای حمله روی سرور
                GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
                attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);

                NetworkObject bulletNetObj = bullet.GetComponent<NetworkObject>();

                // ارسال اطلاعات به کلاینت‌ها
                InitShootAtClientRpc(
                    bulletNetObj.NetworkObjectId,
                    firePoint.position,
                    dir,
                    bulletSpeed,
                    scaleX,
                    this.GetComponent<NetworkObject>().NetworkObjectId,
                    targetRef
                );
            }
        }
    }
    [ClientRpc]
    void InitShootAtClientRpc(
        ulong bulletNetId,
        Vector3 spawnPosition,
        Vector2 dir,
        float bulletSpeed,
        float scaleX,
        ulong attackerNetId,
        NetworkObjectReference targetRef)
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

            Bullet bulletScript = spawnedBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttacker(attacker);
            }
        }

        // انیمیشن اتک روی کلاینت‌ها
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(attackerNetId, out NetworkObject attackerNetObj))
        {
            Animator attackerAnimator = attackerNetObj.GetComponent<Animator>();
            if (attackerAnimator != null)
            {
                attackerAnimator.SetTrigger("Attack");
            }
        }

        // صدای حمله روی کلاینت
        GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, spawnPosition, Quaternion.identity);
        attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
    }
    [ServerRpc(RequireOwnership = false)]
    void ApplyDamageServerRpc(int damageAmount)
    {
        health.Value -= damageAmount;
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

}
