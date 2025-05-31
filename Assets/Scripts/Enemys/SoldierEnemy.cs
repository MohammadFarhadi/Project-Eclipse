using UnityEngine;
using System.Collections;


public class SoldierEnemy : MonoBehaviour, InterfaceEnemies
{
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private string bulletTag = "SoldierBullet";
    [SerializeField] private string grenadeTag = "BombBullet";
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private GameObject[] dropItems; // Prefabs of Health/Stamina/Other pickups
    [SerializeField] private GameObject Sonin;
    [Header("Patrolling")]
    public float speed = 2f;
    public Transform leftLimit, rightLimit;
    public float waitTimeAtEnd = 1.5f;

    [Header("Combat")]
    public float detectionRange = 6f;
    public float fireRate = 1.2f;
    public Transform firePoint;
    public int maxHealth = 3;

    private int currentHealth;
    private bool movingRight = true;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private float fireTimer = 0f;
    private int shotCount = 0;

    private Transform player;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        FindClosestPlayer();

        if (currentHealth <= 0) return;

        fireTimer += Time.deltaTime;

        if (PlayerInSight())
        {
            animator.SetBool("isRunning", false);
            FacePlayer();
            HandleCombat();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtEnd)
            {
                waitTimer = 0f;
                isWaiting = false;
                Flip();
            }
            return;
        }

        animator.SetBool("isRunning", true);

        Vector3 dir = movingRight ? Vector3.right : Vector3.left;
        transform.Translate(dir * speed * Time.deltaTime);

        if ((movingRight && transform.position.x >= rightLimit.position.x) ||
            (!movingRight && transform.position.x <= leftLimit.position.x))
        {
            isWaiting = true;
            animator.SetBool("isRunning", false);
        }
    }

    bool PlayerInSight()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectionRange;
    }

    void HandleCombat()
    {
        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            shotCount++;

            if (shotCount >= 3)
            {
                animator.SetTrigger("ThrowGrenade");
                shotCount = 0; // fire happens inside animation event
            }
            else
            {
                animator.SetTrigger("Shoot");
                // fire happens in animation event
            }
        }
    }


    void FacePlayer()
    {
        if ((player.position.x < transform.position.x && transform.localScale.x > 0) ||
            (player.position.x > transform.position.x && transform.localScale.x < 0))
        {
            Flip();
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TakeDamage(int damage , Transform attacker)  
    {
        currentHealth -= damage;
        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            animator.SetTrigger("Die");
            DropRandomItem();
            StartCoroutine(DeathEffect(GetComponent<SpriteRenderer>()));
        }
    }
    
    // Called by animation event
    public void FireBullet()
    {
        GameObject bullet = bulletPool.GetBullet(bulletTag);
        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (player.position - firePoint.position).normalized;
                rb.linearVelocity = dir * bulletSpeed;

                Vector3 scale = rb.transform.localScale;
                scale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(scale.x);
                rb.transform.localScale = scale;
            }

            Bullet bScript = bullet.GetComponent<Bullet>();
            if (bScript != null)
            {
                bScript.SetAttacker(transform);
            }
        }
    }

// Called by animation event
    public void ThrowGrenade()
    {
        GameObject grenade = bulletPool.GetBullet(grenadeTag); // اول تعریف کن
        if (grenade != null)
        {
            grenade.transform.position = firePoint.position;
            grenade.transform.rotation = Quaternion.identity;

            Bullet bScript = grenade.GetComponent<Bullet>();
            if (bScript != null)
            {
                bScript.SetAttacker(transform);
            }

            Grenade grenadeScript = grenade.GetComponent<Grenade>(); // بعد از تعریف grenade
            if (grenadeScript != null)
            {
                grenadeScript.SetAttacker(transform);
                grenadeScript.SetTarget(player.position); // تعیین هدف حرکت
            }
        }
    }
    
    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = p.transform;
            }
        }

        player = closest;
    }
    
     IEnumerator DeathEffect(SpriteRenderer sr)
    {
        Color originalColor = sr.color;
        sr.color = Color.white; // Flash white
        yield return new WaitForSeconds(0.1f);

        // Fade out
        float duration = 0.4f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
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
