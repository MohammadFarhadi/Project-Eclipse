using UnityEngine;

public class PatrollingEnemy : MonoBehaviour, InterfaceEnemies
{
    public float speed = 2f;
    private int direction = -1;

    // رفرنس‌ها به سنسورها
    public GameObject leftSensor;
    public GameObject rightSensor;
    [SerializeField] private Animator animator;
    public int health = 3;
    void Update()
    {
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0, 0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // فقط وقتی به چیزی مثل دیوار خوردیم (تگ یا لایه خاص اگه خواستی اضافه کن)
        if (collision.gameObject.CompareTag("TurnPoint"))
        {
            direction *= -1;
            
            // برگردوندن اسپریت
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            animator.SetTrigger("Attack");
            Debug.Log("Player hited ");
        }
        
        
    }
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0 && direction == 1)
        {
            animator.SetTrigger("Die");
            Invoke(nameof(Die), 0.5f); 
        }
        else if (health <= 0 && direction == -1)
        {
            animator.SetTrigger("Die1");
            Invoke(nameof(Die), 0.5f); 
        }
    }
    public void Die()
    {
        Destroy(gameObject);
    }

}