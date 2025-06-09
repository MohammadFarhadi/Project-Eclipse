using UnityEngine;

public class Respawn : MonoBehaviour
{
    public GameObject respawnPoint;
    bool falg = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
        if (player != null)
        {
            if (!falg)
            {
             
                player.HealthSystem(100, false); // دمیج بخ   
                falg = true;
            }
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
            falg = false;
        }
    }
}