using UnityEngine;

public class KillPlayerOnTouch : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
            if (player != null)
            {
                player.HealthSystem(30, false);
            }
        }
    }
    
}