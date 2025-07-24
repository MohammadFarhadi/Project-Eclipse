using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Random = UnityEngine.Random;

public class BossController : NetworkBehaviour, InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    [Header("Health")]
    public int maxHealth = 30;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(30, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Attack Settings")]
    public float attackRange = 1f;
    public float attackCooldown = 2f;
    public float switchTargetTime = 5f;
    public string normalBulletTag = "BossBullet";
    public string strongBulletTag = "BossStrongBullet";
    public Transform firePoint;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform[] players;

    [Header("Enemy Spawn")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("Animations")]
    public Animator animator;
    protected NetworkAnimator networkAnimator;

    private float lastAttackTime = 0f;
    private int attackCount = 0;
    private int currentTargetIndex = 0;
    private float targetSwitchTimer = 0f;
    private bool isCharging = false;
    private bool isInCooldown = false;
    private BulletPool bulletPool;
    [SerializeField] private BulletPoolNGO bulletPoolNGO;

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
    

    private void Awake()
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
        if (players[0] == null || players[1] == null)
        {
            GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject obj in foundPlayers)
            {
                if (players[0] == null && obj.name.Contains("Ranged"))
                {
                    players[0] = obj.transform;
                }
                else if (players[1] == null && obj.name.Contains("Melle"))
                {
                    players[1] = obj.transform;
                }
            }
        }

        
        Transform currentTarget = GetNearestPlayer();
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        // اگر فاصله بیشتر از 10 بود، هیچ کاری نکن
        if (distanceToTarget > 10f)
        {
            PlayAnimation("Idle");
            return;
        }

        if (!IsTargetInRange(currentTarget))
        {
            MoveTowards(currentTarget);
            PlayAnimation("Move");
            return;
        }

        if (!isInCooldown)
        {
            HandleTargetSwitching();
            if (Time.time - lastAttackTime > attackCooldown)
            {
                StartCoroutine(Attack(currentTarget));
            }
        }
    }

    Transform GetCurrentTarget()
    {
        return players[currentTargetIndex];
    }

    bool IsTargetInRange(Transform target)
    {
        return Vector2.Distance(transform.position, target.position) < attackRange;
    }

    void MoveTowards(Transform target)
    {
        Vector3 scale = transform.localScale;
        if (target.position.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x); // به چپ نگاه کنه
        else
            scale.x = Mathf.Abs(scale.x);  // به راست نگاه کنه
        transform.localScale = scale;
        Vector2 dir = (target.position - transform.position).normalized;
        transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;
    }

    void HandleTargetSwitching()
    {
        if (IsTargetInRange(GetCurrentTarget()))
        {
            targetSwitchTimer += Time.deltaTime;
            if (targetSwitchTimer >= switchTargetTime)
            {
                currentTargetIndex = (currentTargetIndex + 1) % players.Length;
                targetSwitchTimer = 0f;
            }
        }
        else
        {
            targetSwitchTimer = 0f;
        }
    }


    bool AllPlayersInRange()
    {
        foreach (var p in players)
        {
            if (!IsTargetInRange(p)) return false;
        }
        return true;
    }

    IEnumerator Attack(Transform target)
    {
        GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            // Flip جهت Boss بر اساس موقعیت Target
       
    
            isInCooldown = true; // شروع کول‌دان
            lastAttackTime = Time.time;

            bool isStrong = currentHealth.Value < maxHealth / 2;

            if (isStrong)
            {
                PlayAnimation("Charge");
                yield return new WaitForSeconds(1f); // زمان شارژ
            }

            PlayAnimation("Attack");
            yield return new WaitForSeconds(0.5f); // صبر کن تا انیمیشن اجرا بشه

            GameObject bullet = bulletPool.GetBullet(isStrong ? strongBulletTag : normalBulletTag);
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, target.position - firePoint.position);

            Vector2 direction = (target.position - firePoint.position).normalized;
            float bulletSpeed = 5f;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttacker(this.transform);
            }

            attackCount++;
            if (attackCount >= 4)
            {
                SpawnRandomEnemy();
                currentTargetIndex = (currentTargetIndex + 1) % players.Length;
                attackCount = 0;
            }

            yield return new WaitForSeconds(1f); // صبر برای Idle
            PlayAnimation("Idle");

            // 🕒 اینجا زمان کول‌دان واقعی رو اضافه کن
            yield return new WaitForSeconds(attackCooldown); 

            isInCooldown = false;
        }
        else
        {
            if (IsServer)
            {
                AttackServerRpc();
            }
        }
    }


    void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(enemyToSpawn, spawnPoint.position, Quaternion.identity);
    }

    void PlayAnimation(string animName)
    {
        if (animator == null) return;

        switch (animName)
        {
            case "Move":
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetBool("Run", true);

                }
                else
                {
                    UpdateAnimatorBoolParameterServerRpc("Run" , true);
                }
                break;
            case "Idle":
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetBool("Run", false);

                }
                else
                {
                    UpdateAnimatorBoolParameterServerRpc("Run" , false);
                }
                break;
            default:
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetBool("Run", false);

                }
                else
                {
                    UpdateAnimatorBoolParameterServerRpc("Run" , false);
                } // مطمئن شو موقع حمله حرکت نکنه

                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetTrigger(animName);

                }
                else
                {
                    UpdateAnimatorTriggerParameterServerRpc(animName);
                }
                break;
        }
    }


    public void TakeDamage(int damage, Transform attacker)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                currentHealth.Value -= damage;
            }
            else
            {
                ApplyDamageServerRpc(damage * 5);
            }
        }
        else
        {
            currentHealth.Value -= damage;
        }
        
        if (currentHealth.Value <= 0)
        {
            GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
            PlayAnimation("Death");
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
            } // بعد از انیمیشن بمیره
        }
    }
    Transform GetNearestPlayer()
    {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform player in players)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = player;
            }
        }

        return nearest;
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void AttackServerRpc()
    {
        Transform target = GetNearestPlayer(); // یا از GetCurrentTarget() استفاده کن
        if (target == null || isInCooldown) return;

        isInCooldown = true;
        lastAttackTime = Time.time;

        bool isStrong = currentHealth.Value < maxHealth / 2;

        if (isStrong)
        {
            PlayAnimation("Charge");
            // زمان شارژ در حالت سرور باید با تاخیر همگام بشه، ولی فعلاً فرض کنیم روی سرور کافیه
            StartCoroutine(DelayedAttack(target, 1f, isStrong));
        }
        else
        {
            StartCoroutine(DelayedAttack(target, 0.5f, isStrong));
        }
    }

    IEnumerator DelayedAttack(Transform target, float delay, bool isStrong)
    {
        yield return new WaitForSeconds(delay);

        PlayAnimation("Attack");

        yield return new WaitForSeconds(0.5f); // صبر کن تا انیمیشن اجرا بشه

        GameObject bullet = bulletPoolNGO.GetBullet(isStrong ? strongBulletTag : normalBulletTag);
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, target.position - firePoint.position);

        Vector2 direction = (target.position - firePoint.position).normalized;
        float bulletSpeed = 5f;
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetAttacker(this.transform);
        }

        attackCount++;
        if (attackCount >= 4)
        {
            SpawnRandomEnemy();
            currentTargetIndex = (currentTargetIndex + 1) % players.Length;
            attackCount = 0;
        }

        yield return new WaitForSeconds(1f); // صبر برای Idle
        PlayAnimation("Idle");

        yield return new WaitForSeconds(attackCooldown);
        isInCooldown = false;
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
    [ServerRpc(RequireOwnership = false)]

    protected void UpdateAnimatorTriggerParameterServerRpc(string parameterName)
    {
        networkAnimator.Animator.SetTrigger(parameterName);
    }
    [ClientRpc]
    private void DestroyObjectClientRpc()
    {
        Destroy(gameObject , 1f );
    }
    

}
