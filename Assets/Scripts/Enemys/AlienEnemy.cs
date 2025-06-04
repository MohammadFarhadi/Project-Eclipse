using System.Collections.Generic;
using UnityEngine;

public class AlienEnemy : MonoBehaviour, InterfaceEnemies
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
    [SerializeField] private int health = 3;

    public Transform pointA;
    public Transform pointB;

    private GameObject currentTarget;
    private float lastAttackTime;
    private Rigidbody2D rb;

    private void Start()
    {
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

            animator.SetTrigger("IsWalk");
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

        health -= damage;
        StartCoroutine(FlashRed());

        if (health <= 0)
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
        Destroy(gameObject);
    }
    public void DropRandomItem()
    {
        if (dropItems.Length == 0) return;
        
        int index = Random.Range(0, dropItems.Length);
        Vector3 spawnPosition = transform.position + new Vector3(0f, 1f, 0f); // یک واحد بالاتر
        Instantiate(dropItems[index], spawnPosition, Quaternion.identity);
        Instantiate(Sonin, transform.position, Quaternion.identity);
        
    }
    
}
