using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class ShootingEnemy : MonoBehaviour , InterfaceEnemies
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public float bulletSpeed = 10f;
    public int health = 3;
    // از بولت پول یه کلس می گیریم تا بتونیم ازش استفاده کنیم برای دسترسی به تیر ها
    private BulletPool bulletPool;


    [SerializeField] private Animator animator;
    private List<GameObject> playersInRange = new List<GameObject>();
    private GameObject currentTarget;
    private float timer = 0f;

    // 🩸 نمایش نوار سلامتی
    public EnemyHealthBarDisplay healthBarDisplay;
    
    //صدا زدن بولت پول و گرفتنش
    private void Awake()
    {
        bulletPool = FindFirstObjectByType<BulletPool>();
    }
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
                animator.SetTrigger("Attack");
                timer = 0f;
            }
        }
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

    public void TakeDamage(int damage, Transform attacker)
    {
        if (healthBarDisplay != null && !healthBarDisplay.gameObject.activeSelf)
        {
            healthBarDisplay.Show(health); // show it before taking damage
        }
        health -= damage;

        if (healthBarDisplay != null)
        {
            healthBarDisplay.UpdateHealthBar(health);
        }
        
        //از اینجا
        if (attacker != null)
        {
            float knockbackDistance = 0.5f; // مقدار جابه‌جایی به عقب
            Vector3 direction = (transform.position - attacker.position).normalized;

            // فقط در محور X جابه‌جا کن
            transform.position += new Vector3(direction.x, 0f, 0f) * knockbackDistance;
        }
        //تا اینجا هم اضافه شده

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
    
    
    // اینطوریه این کد که تیر زدن رو براساس event داخل انیمیشن انجام میده که با animation هماهنگ باشه.
    public void FireBullet()
    {
        if (currentTarget == null)
        {
            return;
        }
        // گرفتن تیر مورد نظر
        GameObject proj = bulletPool.GetBullet("Bullet");
        if (proj != null)
        {
            proj.transform.position = firePoint.position;
            proj.transform.rotation = firePoint.rotation; 
            Vector2 dir = (currentTarget.transform.position - firePoint.position).normalized;
            Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
            if (rbProj != null)
            {
                // تیر به سمت پلیر برتاب میشه هم چنین براساس موقعیتی که این شوتینگ انمی وایساده تیر زده میشه.
                Vector3 bulletScale = rbProj.transform.lossyScale;
                bulletScale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(bulletScale.x);
                rbProj.linearVelocity = dir * bulletSpeed;
                rbProj.transform.localScale = bulletScale;
            }
        }
    }
}
