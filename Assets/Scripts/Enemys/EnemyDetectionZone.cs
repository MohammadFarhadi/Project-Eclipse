using UnityEngine;

public class EnemyDetectionZone : MonoBehaviour
{
    private JumpingEnemy enemy;

    private void Start()
    {
        enemy = GetComponentInParent<JumpingEnemy>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemy?.DetectPlayer(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemy?.LosePlayer();
        }
    }
}