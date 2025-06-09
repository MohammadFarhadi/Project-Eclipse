using UnityEngine;
public class PatrollingEnemy : MonoBehaviour , InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;

    public float speed = 2f;
    private int direction = -1;
    public GameObject leftSensor;
    public GameObject rightSensor;
    [SerializeField] private Animator animator;

    public int health = 3;

    // 👇 اضافه کن:
    public EnemyHealthBarDisplay healthBarDisplay;
    [Header("Possible Drops")]
    [SerializeField] private GameObject[] dropItems; // Prefabs of Health/Stamina/Other pickups
    [SerializeField] private GameObject Sonin;
    
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("TurnPoint"))
        {
            direction *= -1;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
            animator.SetTrigger("Attack");
            PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
            player.HealthSystem(50, false);
            Debug.Log("Player hited ");
        }
    }

    public void TakeDamage(int damage , Transform attacker)
    {
        health -= damage;

        // 👇 بروز رسانی نوار سلامتی
        if (healthBarDisplay != null)
        {
            healthBarDisplay.Show(health);
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