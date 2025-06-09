using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Movement & Explosion")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.3f;
    [SerializeField] private float explosionDelay = 1.0f;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private int damage = 20;
    [SerializeField] private LayerMask targetLayers; // Can be Player + Enemy
    [SerializeField] private ParticleSystem explosionEffect;

    private Vector2 targetPosition;
    private bool moving = false;
    private Transform attacker;

    public void SetAttacker(Transform attacker)
    {
        this.attacker = attacker;
    }

    public void SetTarget(Vector2 position)
    {
        targetPosition = position;
        moving = true;
    }

    private void OnEnable()
    {
        moving = true;
    }

    private void Update()
    {
        if (moving)
        {
            Vector2 current = transform.position;
            Vector2 target = new Vector2(targetPosition.x, current.y);
            transform.position = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);

            if (Mathf.Abs(current.x - target.x) < stopDistance)
            {
                moving = false;
                Invoke(nameof(Explode), explosionDelay);
            }
        }
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            var effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetLayers);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var player = hit.GetComponent<PlayerControllerBase>();
                if (player != null)
                {
                    player.HealthSystem(damage, false);

                    var rb = hit.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 dir = (hit.transform.position - transform.position).normalized;
                        rb.AddForce(dir * explosionForce);
                    }
                }
            }
            if (hit.CompareTag("Enemy"))
            {
                InterfaceEnemies enemy = hit.GetComponent<InterfaceEnemies>();
                if (enemy != null && hit.transform != attacker)
                {
                    enemy.TakeDamage(damage, attacker);

                    var rb = hit.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 dir = (hit.transform.position - transform.position).normalized;
                        rb.AddForce(dir * explosionForce);
                    }
                }
            }
        }


        gameObject.SetActive(false); // return to pool
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
