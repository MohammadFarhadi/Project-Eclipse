using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Animator))]
public class Trampoline : MonoBehaviour
{
    public float bounceForce = 10f;
    private Animator trampolineAnimator;

    private void Awake()
    {
        trampolineAnimator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Vector2.Dot(contact.normal, Vector2.down) > 0.5f)
            {
                Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
                }

                if (trampolineAnimator != null)
                    trampolineAnimator.SetTrigger("Bounce");

                Animator playerAnim = collision.gameObject.GetComponent<Animator>();
                if (playerAnim != null)
                    playerAnim.SetTrigger("IsJumping");

                // 👇 Set the isJumping flag
                var playerScript = collision.gameObject.GetComponent<PlayerControllerBase>();
                if (playerScript != null)
                    playerScript.isGrounded = false;

                break;
            }
        }
    }
}