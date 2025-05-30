using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Movement & Explosion")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float explosionDelay = 1.0f;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private int damage = 20;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private ParticleSystem explosionEffect;

    private Transform attacker;
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool grounded = false;
    private bool moving = false;

    public void SetAttacker(Transform attacker)
    {
        this.attacker = attacker;
    }

    public void SetTarget(Vector2 position)
    {
        targetPosition = position;
        // اینجا دیگه moving رو true نمی‌کنیم چون منتظر برخورد به زمینیم
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        grounded = false;
        moving = false;

        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.isKinematic = false;
            rb.linearVelocity = Vector2.zero;
        }

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = false;
    }

    private void Update()
    {
        if (grounded && moving)
        {
            Vector2 current = transform.position;
            Vector2 target = new Vector2(targetPosition.x, current.y); // فقط محور X
            transform.position = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - target.x) < 0.1f)
            {
                moving = false;
                ExplodeSoon();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!grounded && collision.collider.CompareTag("Ground"))
        {
            grounded = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                rb.isKinematic = true;
            }

            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            // حالا که به زمین رسیدیم، شروع به حرکت کن
            moving = true;
        }
    }

    private void ExplodeSoon()
    {
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            var effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerLayer);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var player = hit.GetComponent<PlayerControllerBase>();
                if (player != null)
                {
                    player.HealthSystem(damage, false);

                    var rbTarget = hit.GetComponent<Rigidbody2D>();
                    if (rbTarget != null)
                    {
                        Vector2 dir = (hit.transform.position - transform.position).normalized;
                        rbTarget.AddForce(dir * explosionForce);
                    }
                }
            }
        }

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
