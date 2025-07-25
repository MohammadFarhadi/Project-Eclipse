using UnityEngine;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;

public class EnemyAI : NetworkBehaviour, InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    public float moveSpeed = 2f;
    public float dashSpeed = 6f;
    public float directionChangeInterval = 5f;
    public float dashDuration = 0.3f;
    public float attackCooldown = 1.5f;
    public float chaseCooldown = 1f;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);    public int damageToPlayer = 30;

    private Vector2 moveDirection;
    private float changeDirectionTime;
    private bool isChasing = false;
    private bool isDashing = false;
    private float lastAttackTime = -Mathf.Infinity;
    private float chaseCooldownTimer = 0f;

    private Transform targetPlayer;
    private Animator animator;
    protected NetworkAnimator networkAnimator;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
        PickNewDirection();
    }

    void Update()
    {
        if (isDashing) return;

        if (isChasing && targetPlayer != null)
        {
            if (Time.time >= chaseCooldownTimer)
            {
                moveDirection = (targetPlayer.position - transform.position).normalized;
            }
            else
            {
                moveDirection = Vector2.zero;
            }
        }
        else
        {
            if (Time.time >= changeDirectionTime)
                PickNewDirection();
        }

        // اگر به دیوار نزدیک بود، مسیر جدید انتخاب کن
        if (IsHittingWall())
        {
            PickNewDirection();
        }

        Move();
    }

    void PickNewDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        moveDirection = directions[Random.Range(0, directions.Length)];
        changeDirectionTime = Time.time + directionChangeInterval;
    }

    void Move()
    {
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    bool IsHittingWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, 0.5f, LayerMask.GetMask("Wall"));
        return hit.collider != null;
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

        if (!isChasing)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float closestDistance = Mathf.Infinity;

            foreach (var player in players)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    targetPlayer = player.transform;
                }
            }

            isChasing = true;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Decor"))
        {
            if (!isChasing)
            {
                PickNewDirection();
            }
        }

        if (Time.time < lastAttackTime + attackCooldown) return;

        if (collision.CompareTag("Player"))
        {
            GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
            if (collision.TryGetComponent(out TopDownController player))
            {
                player.HealthSystem(damageToPlayer, false);
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetTrigger("Attack");
                }
                else
                {
                    UpdateAnimatorTriggerParameterServerRpc( "Attack");
                }

                lastAttackTime = Time.time;
                StartCoroutine(DashThroughPlayer());
            }
        }
    }

    IEnumerator DashThroughPlayer()
    {
        isDashing = true;
        Vector2 dashDir = moveDirection;
        rb.linearVelocity = dashDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        chaseCooldownTimer = Time.time + chaseCooldown;
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
}
