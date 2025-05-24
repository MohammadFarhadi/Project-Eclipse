using UnityEngine;
public class PatrollingEnemy : MonoBehaviour , InterfaceEnemies
{
    public float speed = 2f;
    private int direction = -1;

    public GameObject leftSensor;
    public GameObject rightSensor;
    [SerializeField] private Animator animator;

    public int health = 4;

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
        // else if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("PlayerBullet"))
        // {
        //     TakeDamage(1, this.transform);
        // }
        else if (collision.gameObject.CompareTag("Player"))
        {
            animator.SetTrigger("Attack");
            Debug.Log("Player hited ");
        }
    }

    public void TakeDamage(int damage, Transform attacker)
    {

        // 👇 بروز رسانی نوار سلامتی
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
        // تا اینجا اضافه شده

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