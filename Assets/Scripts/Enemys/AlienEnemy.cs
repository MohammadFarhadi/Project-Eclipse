using System.Collections.Generic;
using UnityEngine;

public class AlienEnemy : MonoBehaviour, InterfaceEnemies
{
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackCooldown = 1f;

    [SerializeField] private Animator animator;
    [SerializeField] private int health = 3;

    public Transform pointA;  // نقطه شروع محدوده
    public Transform pointB;  // نقطه پایان محدوده

    private GameObject currentTarget;
    private float lastAttackTime;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        FindClosestPlayer();

        if (currentTarget != null)
        {
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;

            // چرخاندن به سمت پلیر
            if (direction.x > 0.01f)
                transform.localScale = new Vector3(-0.8f, 0.8f, 1);
            else if (direction.x < -0.01f)
                transform.localScale = new Vector3(0.8f, 0.8f, 1);

            // حرکت فقط در راستای X
            Vector3 targetPosition = new Vector3(currentTarget.transform.position.x, transform.position.y, transform.position.z);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // اجرای انیمیشن راه رفتن
            animator.SetTrigger("IsWalk");
        }
    }

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        GameObject closest = null;

        float minX = Mathf.Min(pointA.position.x, pointB.position.x);
        float maxX = Mathf.Max(pointA.position.x, pointB.position.x);

        foreach (GameObject player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            float playerX = player.transform.position.x;

            // فقط اگر پلیر داخل محدوده X بین pointA و pointB باشد
            if (playerX >= minX && playerX <= maxX)
            {
                if (dist < detectionRange && dist < closestDistance)
                {
                    closestDistance = dist;
                    closest = player;
                }
            }
        }

        currentTarget = closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time - lastAttackTime > attackCooldown)
            {
                PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
                if (player != null)
                {
                    player.HealthSystem(100, false);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
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
}
