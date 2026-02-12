using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enemys;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BossController : NetworkBehaviour, InterfaceEnemies
{
    // ===================== NEW: Simple State Machine =====================
    public enum BossState { Chasing, Attacking }

    [Header("Targeting & Movement Logic")]
    [SerializeField] private float stopDistance = 5f;            // فاصله توقف برای حمله
    [SerializeField] private float attackPhaseDuration = 15f;     // مدت فاز حمله به یک تارگت
    private BossState state = BossState.Chasing;
    private float attackPhaseEndTime = 0f;
    // ====================================================================

    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;

    [Header("Health")]
    public int maxHealth = 30;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(30, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int HealthPoint => Mathf.RoundToInt(currentHealth.Value);

    [Header("Attack Settings")]
    public float attackRange = 1f;
    public float attackCooldown = 2f;
    public float switchTargetTime = 5f; // (دیگر استفاده‌ای ندارد ولی حفظ شده)
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
    public int attackCount1 = 0;
    private int currentTargetIndex = 0;
    private float targetSwitchTimer = 0f; // (دیگر استفاده‌ای ندارد ولی حفظ شده)
    private bool isCharging = false;
    private bool isInCooldown = false;
    private BulletPool bulletPool;
    [SerializeField] private BulletPoolNGO bulletPoolNGO;
    [SerializeField] private bool Is_First = true;

    Transform currentTarget;
[SerializeField] private EnemyUI enemysUI;
    void Start()
    {
        RefreshUI();
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

        // هدف اولیه: رندوم
        currentTargetIndex = Random.Range(0, 2);
        if (players != null && players.Length >= 2)
        {
            if (currentTargetIndex < players.Length)
                currentTarget = players[currentTargetIndex];
        }

        state = BossState.Chasing;
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

    // ===================== NEW/UPDATED: Helpers =====================
    private void EnsurePlayersCached()
    {
        if (players == null || players.Length < 2)
            players = new Transform[2];

        // پیدا کردن بازیکن‌های موجود
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in foundPlayers)
        {
            if (obj == null) continue;

            if (players[0] == null && obj.name.Contains("Ranged"))
                players[0] = obj.transform;
            else if (players[1] == null && (obj.name.Contains("Melle") || obj.name.Contains("Melee")))
                players[1] = obj.transform;
        }

        // اگر currentTarget مرده بود، برو سراغ یکی دیگه
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            if (players[0] != null && players[0].gameObject.activeInHierarchy)
                currentTarget = players[0];
            else if (players[1] != null && players[1].gameObject.activeInHierarchy)
                currentTarget = players[1];
            else
                currentTarget = null; // یعنی دیگه هیچ بازیکنی زنده نیست
        }
    }


    private void SwitchToOtherTarget()
    {
        if (players == null || players.Length < 2) return;

        EnsurePlayersCached();
        if (players[0] == null && players[1] == null) return;

        // تناوبی بین 0 و 1
        currentTargetIndex = (currentTargetIndex == 0) ? 1 : 0;

        // اگر تارگت جدید تهی بود و قبلی موجود، روی قبلی بمان
        if (players[currentTargetIndex] == null)
            currentTargetIndex = (currentTargetIndex == 0) ? 1 : 0;

        currentTarget = players[currentTargetIndex];
        targetSwitchTimer = 0f;
    }

    private void FaceTarget(Transform target)
    {
        if (target == null) return;
        Vector3 scale = transform.localScale;
        scale.x = (target.position.x < transform.position.x) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
    // ================================================================

    // ===================== UPDATED: Update() =====================
    void Update()
    {
        // در حالت آنلاین، منطق حرکت/تارگت فقط روی سرور اجرا شود تا دسی‌سینک نشود
        if (GameModeManager.Instance.CurrentMode == GameMode.Online && !IsServer)
            return;

        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            SwitchToOtherTarget(); // برو سراغ اون یکی
            if (currentTarget == null)
            {
                PlayAnimation("Idle"); 
                return; // هیچ بازیکنی نیست → بیکار باشه
            }
        }


        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        // ✅ شرط جدید: اگر فاصله بیشتر از 15 باشه، هیچ کاری نکن
        if (distanceToTarget > 15f)
        {
            PlayAnimation("Idle");
            return;
        }

        switch (state)
        {
            case BossState.Chasing:
                if (distanceToTarget > stopDistance)
                {
                    MoveTowards(currentTarget);
                    PlayAnimation("Move");
                }
                else
                {
                    // ورود به فاز حمله
                    PlayAnimation("Idle");
                    FaceTarget(currentTarget);
                    state = BossState.Attacking;
                    attackCount = 0; // ✅ شروع فاز حمله با صفر کردن شمارنده
                }
                break;

            case BossState.Attacking:
                if (distanceToTarget > stopDistance)
                {
                    state = BossState.Chasing;
                    PlayAnimation("Move");
                    break;
                }

                FaceTarget(currentTarget);
                PlayAnimation("Idle");

                if (!isInCooldown && (Time.time - lastAttackTime) > attackCooldown)
                {
                    StartCoroutine(Attack(currentTarget));
                }

                // ✅ شرط جدید: بعد از 5 حمله، سوییچ تارگت
                if (attackCount1 >= 5)
                {
                    SwitchToOtherTarget();
                    state = BossState.Chasing;
                    attackCount1 = 0; 
                }
                break;
        }
    }

    // ============================================================

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
        if (target == null) return;

        Vector3 scale = transform.localScale;
        scale.x = (target.position.x < transform.position.x) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        Vector2 dir = (target.position - transform.position).normalized;
        transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;
    }

    void HandleTargetSwitching()
    {
        // (حفظ شده اما دیگر استفاده نمی‌شود)
        if (IsTargetInRange(GetCurrentTarget()))
        {
            targetSwitchTimer += Time.deltaTime;
            if (targetSwitchTimer >= switchTargetTime)
            {
                currentTargetIndex = (currentTargetIndex + 1) % players.Length;
                currentTarget = players[currentTargetIndex];
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
            if (p == null) return false;
            if (!IsTargetInRange(p)) return false;
        }
        return true;
    }

    IEnumerator Attack(Transform target)
    {
        attackCount1++;
        // رو به تارگت
        Vector3 scale = transform.localScale;
        scale.x = (target.position.x < transform.position.x) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);

        GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);

        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            isInCooldown = true; // شروع کول‌دان
            lastAttackTime = Time.time;

            bool isStrong = currentHealth.Value < maxHealth / 2;

            if (isStrong)
            {
                PlayAnimation("Charge");
                yield return new WaitForSeconds(1f); // زمان شارژ
            }

            PlayAnimation("Attack");
            yield return new WaitForSeconds(0.5f); // صبر تا اجرای انیمیشن

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
            if (attackCount >= 2)
            {
                SpawnRandomEnemy();
                currentTargetIndex = (currentTargetIndex + 1) % players.Length;
                attackCount = 0;
            }

            yield return new WaitForSeconds(1f);
            PlayAnimation("Idle");

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
        if (GameModeManager.Instance.CurrentMode == GameMode.Online && !IsServer) return;
        if (enemyPrefabs == null || spawnPoints == null) return;
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject SpawnedObject = Instantiate(enemyToSpawn, spawnPoint.position, Quaternion.identity);
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (SpawnedObject.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn();
            }
        }
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
                    UpdateAnimatorBoolParameterServerRpc("Run", true);
                }
                break;

            case "Idle":
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetBool("Run", false);
                }
                else
                {
                    UpdateAnimatorBoolParameterServerRpc("Run", false);
                }
                break;

            default:
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetBool("Run", false);
                }
                else
                {
                    UpdateAnimatorBoolParameterServerRpc("Run", false);
                }

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
                    // (در کد اصلی ضربدر 5 بود؛ همان را حفظ می‌کنیم)
                    ApplyDamageServerRpc(damage);
                }
            }
            else
            {
                currentHealth.Value -= damage;
            }

            RefreshUI();
            

            if (currentHealth.Value <= 0)
            {
                GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
                deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
                PlayAnimation("Death");

                if (GameModeManager.Instance.CurrentMode == GameMode.Online)
                {
                    if (IsServer)
                    {
                        UpdateAnimatorTriggerParameterServerRpc("Death");
                        DestroyObjectClientRpc();
                        Destroy(gameObject, 1f);
                    }
                }
                else
                {
                    animator.SetTrigger("Death");
                    Destroy(gameObject, 1f);
                }

                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    SceneManager.LoadScene("Victory");
                }
                else
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        NetworkManager.Singleton.SceneManager.LoadScene("Victory", UnityEngine.SceneManagement.LoadSceneMode.Single);
                    }
                }
            }
        }

    Transform GetNearestPlayer()
    {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform player in players)
        {
            if (player == null) continue;
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = player;
            }
        }

        return nearest;
    }

    // ======================= Server/Client RPCs =======================
    [ServerRpc(RequireOwnership = false)]
    public void AttackServerRpc()
    {
        Transform target = GetNearestPlayer(); // یا GetCurrentTarget()
        if (target == null || isInCooldown) return;

        isInCooldown = true;
        lastAttackTime = Time.time;

        bool isStrong = currentHealth.Value < maxHealth / 2;

        if (isStrong)
        {
            PlayAnimation("Charge");
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
        yield return new WaitForSeconds(0.5f);

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

        NetworkObject netObj = bullet.GetComponent<NetworkObject>();

        // اطلاع به کلاینت‌ها
        InitShootBulletClientRpc(
            netObj.NetworkObjectId,
            firePoint.position,
            direction,
            bulletSpeed,
            this.GetComponent<NetworkObject>().NetworkObjectId
        );

        yield return new WaitForSeconds(1f);
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
    protected void UpdateAnimatorBoolParameterServerRpc(string parameterName, bool value)
    {
        networkAnimator.Animator.SetBool(parameterName, value);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void UpdateAnimatorFloatParameterServerRpc(string parameterName, float value)
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
        Destroy(gameObject, 1f);
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
    
    public void SetHealth(int hp)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer) currentHealth.Value = hp;
            else ApplyDamageServerRpc(Mathf.RoundToInt(currentHealth.Value - hp)); // or a dedicated SetHealthServerRpc
        }
        else
        {
            currentHealth.Value = hp;
        } 
    }

    public void RefreshUI()
    {
        enemysUI?.SetHealthBar(currentHealth.Value, maxHealth);
    }
    public override void OnNetworkSpawn()
    {
        // به تغییرات مقدار health گوش می‌کنیم
        currentHealth.OnValueChanged += OnHealthChanged;

        // وقتی کلاینت جدید join میشه، باید ui درست باشه
        RefreshUI();
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        if (enemysUI != null)
        {
            enemysUI.SetHealthBar(newValue, maxHealth);
        }
    }
}
