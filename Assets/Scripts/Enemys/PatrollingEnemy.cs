using UnityEngine;
public class PatrollingEnemy : MonoBehaviour , InterfaceEnemies
{
    public float speed = 2f;
    private int direction = -1;

    public GameObject leftSensor;
    public GameObject rightSensor;
    [SerializeField] private Animator animator;

    public int health = 3;

    // 👇 اضافه کن:
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
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("TurnPoint"))
        {
            direction *= -1;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        else if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Sword"))
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

        // 👇 بروز رسانی نوار سلامتی
        if (healthBarDisplay != null)
        {
            healthBarDisplay.UpdateHealthBar(health);
        }

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