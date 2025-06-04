using UnityEngine;

public class JumpingEnemy : MonoBehaviour, InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float horizontalForce = 3f;

    [Header("Health")]
    [SerializeField] private int maxHealthPoints = 3;
    private int currentHealthPoints;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    private GameObject player;
    private bool isGrounded = true;
    private bool isJumpingAtPlayer = false;

    private void Start()
    {
        currentHealthPoints = maxHealthPoints;
    }

    private void Update()
    {
        if (player != null && isGrounded && !isJumpingAtPlayer)
        {
            JumpAtPlayer();
        }
    }

    private void JumpAtPlayer()
    {
        if (player == null || rb == null) return;

        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
        Vector2 jumpVector = new Vector2(direction * horizontalForce, jumpForce);

        rb.linearVelocity = jumpVector;
        isGrounded = false;
        isJumpingAtPlayer = true;

        if (animator != null)
            animator.SetTrigger("Jump");

        transform.localScale = new Vector3(Mathf.Sign(direction), 1, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = true;
            isJumpingAtPlayer = false;
        }
    }


    // ✅ این روش با استفاده از یک Trigger بیرونی کار می‌کند
    public void DetectPlayer(GameObject playerObject)
    {
        player = playerObject;
    }

    public void LosePlayer()
    {
        player = null;
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        currentHealthPoints -= damage;

        if (currentHealthPoints <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
                animator.SetTrigger("GetHit");
        }
    }

    private void Die()
    {
        if (animator != null)
        {
            GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
            animator.SetTrigger("IsDead");
            
        }
            

        Destroy(gameObject, 0.5f);
    }
    

    
}
