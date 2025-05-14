using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // فقط به Enemy آسیب بزن، نه به میدان دید
        if (other.CompareTag("Enemy"))
        {
            ShootingEnemy enemy = other.GetComponent<ShootingEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }

            Destroy(gameObject); // حذف گلوله
        }
    }
}