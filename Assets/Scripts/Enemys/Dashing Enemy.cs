using UnityEngine;

public class DashingEnemy : MonoBehaviour, InterfaceEnemies
{
    [Header("General Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 1f;
    [SerializeField] private float idleDuration = 2f;
    [SerializeField] private float jumpForce = 4f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 9;
    private int currentHealth;

    [Header("Damage Settings")]
    [SerializeField] private int damage = 50;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ParticleSystem dashEffect; // 🎇 پارتیکل دش
    [SerializeField] private Transform graphicsTransform; // 🔁 برای flip کردن ظاهر

    private bool isDashing = false;
    private bool isIdle = true;
    private bool isGrounded = true;
    private int currentDirection = -1; // 1: Right, -1: Left

    private void Start()
    {
        currentHealth = maxHealth;
        Invoke(nameof(StartNextAttack), idleDuration);
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(currentDirection * dashSpeed, rb.linearVelocity.y);
        }
    }

    private void StartNextAttack()
    {
        if (currentHealth < 4)
        {
            int mode = Random.Range(0, 2);
            if (mode == 0)
                StartGroundDash();
            else
                StartAirDash();
        }
        else
        {
            StartGroundDash();
        }
    }

    private void StartGroundDash()
    {
        isIdle = false;
        isDashing = true;

        if (dashEffect != null)
        {
            dashEffect.Play(); // 🎇 اجرای پارتیکل افکت
        }

        Invoke(nameof(EndDash), dashDuration);
    }

    private void StartAirDash()
    {
        isIdle = false;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        Invoke(nameof(StartGroundDash), 0.3f);
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Idle"); // فقط انیمیشن Idle

        currentDirection *= -1;
        FlipGraphics(); // فقط بعد از دش flip می‌شه
        Invoke(nameof(StartNextAttack), idleDuration);
    }

    private void FlipGraphics()
    {
        if (graphicsTransform != null)
        {
            Vector3 scale = graphicsTransform.localScale;
            scale.x *= -1;
            graphicsTransform.localScale = scale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isDashing) return;

        if (collision.CompareTag("Player"))
        {
            PlayerControllerBase player = collision.GetComponent<PlayerControllerBase>();
            if (player != null)
            {
                player.HealthSystem(damage, false);
            }
        }
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject, 0.5f);
    }

    public void DetectPlayer(GameObject p) { }
    public void LosePlayer() { }
}
