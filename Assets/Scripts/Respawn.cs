using UnityEngine;

public class Respawn : MonoBehaviour
{
    public GameObject respawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
        if (player != null)
        {
            player.HealthSystem(100, false); // دمیج بخوره

            // اگر هنوز نمرده باشه، ریسپانش کن
            if (player.gameObject != null && player.gameObject.activeSelf)
            {
                StartCoroutine(DelayedRespawn(player));
            }
        }
    }

    private System.Collections.IEnumerator DelayedRespawn(PlayerControllerBase player)
    {
        yield return new WaitForSeconds(0.5f); // کمی صبر برای انیمیشن GetHit

        if (player != null)
        {
            player.Respawn(respawnPoint.transform.position);
        }
    }
}