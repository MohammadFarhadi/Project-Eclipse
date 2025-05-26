using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public float bounceForce = 10f; // مقدار نیروی پرش

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            Animator anim = collision.gameObject.GetComponent<Animator>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // برای اینکه پرش قبلی رو ریست کنیم
                rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
            }
            if (anim != null)
            {
                anim.SetTrigger("Jump"); // مطمئن شو که در Animator این Trigger وجود داره
            }
        }
    }
}