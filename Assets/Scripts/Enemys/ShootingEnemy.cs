using UnityEngine;
using System.Collections.Generic;

public class ShootingEnemy : MonoBehaviour , InterfaceEnemies
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public float bulletSpeed = 10f;
    public int health = 3;

    [SerializeField] private Animator animator;
    private List<GameObject> playersInRange = new List<GameObject>();
    private GameObject currentTarget;
    private float timer = 0f;

    // 🩸 نمایش نوار سلامتی
    public EnemyHealthBarDisplay healthBarDisplay;

    void Start()
    {
        if (healthBarDisplay != null)
        {
            healthBarDisplay.UpdateHealthBar(health);
        }
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
        if (target == null) return;

        Vector2 dir = (target.transform.position - firePoint.position).normalized;
        animator.SetTrigger("Attack");
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = dir * bulletSpeed;
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
        health -= damage;

        if (healthBarDisplay != null)
        {
            healthBarDisplay.UpdateHealthBar(health);
        }
        if (attacker != null)
        {
            float knockbackDistance = 0.5f; // مقدار جابه‌جایی به عقب
            Vector3 direction = (transform.position - attacker.position).normalized;

            // فقط در محور X جابه‌جا کن
            transform.position += new Vector3(direction.x, 0f, 0f) * knockbackDistance;
        }


        if (health <= 0)
        {
            animator.SetTrigger("Die");
            Invoke(nameof(Die), 0.5f); 
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
