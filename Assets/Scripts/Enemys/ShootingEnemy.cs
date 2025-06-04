using UnityEngine;
using System.Collections.Generic;

public class ShootingEnemy : MonoBehaviour , InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public float bulletSpeed = 10f;
    public int health = 3;

    [SerializeField] private Animator animator;
    private List<GameObject> playersInRange = new List<GameObject>();
    private GameObject currentTarget;
    private float timer = 0f;
    
    private BulletPool bulletPool;


    // 🩸 نمایش نوار سلامتی
    public EnemyHealthBarDisplay healthBarDisplay;
    [Header("Possible Drops")]
    [SerializeField] private GameObject[] dropItems; // Prefabs of Health/Stamina/Other pickups
    [SerializeField] private GameObject Sonin;
    [SerializeField] private float dropChance = 0.5f; // بین ۰ تا ۱ مثلا 0.5 یعنی ۵۰٪ احتمال اسپاون آیتم

    void Start()
    {
        bulletPool = FindFirstObjectByType<BulletPool>();

        if (healthBarDisplay != null)
        {
            healthBarDisplay.UpdateHealthBar(health);
        }

        
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
        if (target == null || firePoint == null || bulletPool == null) return;

        Vector2 dir = (target.transform.position - firePoint.position).normalized;

        GameObject bullet = bulletPool.GetBullet("Bullet"); 
        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = Quaternion.identity;
                animator.SetTrigger("Attack");  

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector3 bulletScale = rb.transform.localScale;
                bulletScale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(bulletScale.x);
                rb.transform.localScale = bulletScale;
                rb.linearVelocity = dir * bulletSpeed;
            }

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetAttacker(this.transform);
            }
        }
        GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
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
            healthBarDisplay.Show(health);
            healthBarDisplay.UpdateHealthBar(health);
        }
       


        if (health <= 0)
        {
            animator.SetTrigger("Die");
            Invoke(nameof(Die), 0.5f); 
        }
    }

    public void Die()
    {
        GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
        DropRandomItem();
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
