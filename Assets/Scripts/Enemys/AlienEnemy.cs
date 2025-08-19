using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class AlienEnemy : NetworkBehaviour, InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDying = false;
    [SerializeField] private GameObject[] dropItems; // Prefabs of Health/Stamina/Other pickups
    [SerializeField] private GameObject Sonin;
    
    
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackCooldown = 1f;

    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    [SerializeField] private NetworkVariable<int> health = new NetworkVariable<int>(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public int HealthPoint => health.Value;

    public Transform pointA;
    public Transform pointB;

    private GameObject currentTarget;
    private float lastAttackTime;
    private Rigidbody2D rb;

    private void Start()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        FindClosestTarget();

        if (currentTarget != null)
        {
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;

            if (direction.x > 0.01f)
                transform.localScale = new Vector3(-0.8f, 0.8f, 1);
            else if (direction.x < -0.01f)
                transform.localScale = new Vector3(0.8f, 0.8f, 1);

            Vector3 targetPosition = new Vector3(currentTarget.transform.position.x, transform.position.y, transform.position.z);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetTrigger("IsWalk");
            }
            else
            {
                UpdateAnimatorTriggerParameterServerRpc("IsWalk");
            }
        }
    }

    private void FindClosestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(players);
        allTargets.AddRange(enemies);

        float closestDistance = Mathf.Infinity;
        GameObject closest = null;

        float minX = Mathf.Min(pointA.position.x, pointB.position.x);
        float maxX = Mathf.Max(pointA.position.x, pointB.position.x);

        foreach (GameObject target in allTargets)
        {
            float dist = Vector2.Distance(transform.position, target.transform.position);
            float targetX = target.transform.position.x;
            float verticalDifference = Mathf.Abs(target.transform.position.y - transform.position.y);

            if (targetX >= minX && targetX <= maxX && verticalDifference <= 1f) // 👈 فقط هدف‌هایی که نزدیک به سطح خودشن
            {
                if (dist < detectionRange && dist < closestDistance && target != this.gameObject)
                {
                    closestDistance = dist;
                    closest = target;
                }
            }
        }

        currentTarget = closest;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Enemy")) && other.gameObject != this.gameObject)
        {
            if (Time.time - lastAttackTime > attackCooldown)
            {
                PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
                if (player != null )
                {
                    GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
                    attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
                    player.HealthSystem(100, false);
                }

                InterfaceEnemies enemy = other.GetComponent<InterfaceEnemies>();
                if (enemy != null && enemy != this)
                {
                    enemy.TakeDamage(3, transform); 
                }

                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (isDying) return;

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
        StartCoroutine(FlashRed());

        if (health.Value <= 0)
        {
            isDying = true;
            StartCoroutine(DeathEffect());
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
    
    
    private System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    
    
    
    
    private System.Collections.IEnumerator DeathEffect()
    {
        float duration = 0.5f;
        float timer = 0f;
        Vector3 startScale = transform.localScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Scale down
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // Fade out
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f - t;
                spriteRenderer.color = c;
            }

            yield return null;
        }
        DropRandomItem();
        GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
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
