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

    void Start()
    {
        currentHealth = maxHealth;
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
        if (Time.time >= nextFireTime)
        {
            bool isEnraged = currentHealth <= maxHealth / 2;
            GameObject bulletToShoot = isEnraged ? fireBulletPrefab : normalBulletPrefab;
            float fireRate = isEnraged ? enragedFireRate : normalFireRate;

            GameObject bullet = Instantiate(bulletToShoot, firePoint.position, Quaternion.identity);

            Vector2 direction = (currentTargetPlayer.position - firePoint.position).normalized;

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            float bulletSpeed = 10f;
            rb.linearVelocity = direction * bulletSpeed;

            nextFireTime = Time.time + fireRate;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        animator.SetTrigger("hurt");
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
