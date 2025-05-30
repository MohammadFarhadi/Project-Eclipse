using Unity.VisualScripting;
using UnityEngine;

public class MiniBossAI : MonoBehaviour, InterfaceEnemies
{
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
    private float currentHealth;
    public float normalFireRate = 2f;
    public float enragedFireRate = 0.8f;
    private float nextFireTime = 0f;

    [Header("Animation")]
    public Animator animator;
    [Header("Health Bar")]
    public EnemyHealthBarDisplay healthBarDisplay;

    [SerializeField] private GameObject Key;
    [SerializeField] private GameObject Sonin;
    private BulletPool bulletPool;



    void Start()
    {
        healthBarDisplay.gameObject.SetActive(false);
        currentHealth = maxHealth;

        bulletPool = FindFirstObjectByType<BulletPool>();
    }


    void Update()
    {
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
        if (Time.time >= nextFireTime && bulletPool != null)
        {
            bool isEnraged = currentHealth <= maxHealth / 2;
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
                float bulletSpeed = 5f;
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


    public void TakeDamage(int damage, Transform attacker)
    {
        
        healthBarDisplay.gameObject.SetActive(true);
        currentHealth -= damage * 5;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        animator.SetTrigger("hurt");

        int spriteIndex = Mathf.CeilToInt(currentHealth / 10f) - 1;
        healthBarDisplay.UpdateHealthBar(spriteIndex);
        healthBarDisplay.gameObject.SetActive(true);

        if (currentHealth <= 0)
        {
            animator.SetTrigger("Death");
            Invoke(nameof(Die), 0.2f); 
        }
        
        
    }

    public void Die()
    {
        Destroy(gameObject);
        Vector3 spawnPosition = transform.position + new Vector3(0f, 1f, 0f); // یک واحد بالاتر
        Instantiate(Key, spawnPosition, Quaternion.identity);
        Instantiate(Sonin, transform.position, Quaternion.identity);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PointA") || collision.CompareTag("PointB"))
        {
            Flip(); // تغییر جهت
            currentTargetTag = collision.tag;
            lastPatrolTarget = collision.transform;
        }
    }
}
