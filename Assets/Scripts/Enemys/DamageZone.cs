using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    public int damage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
            PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
            if (player != null)
            {
                player.HealthSystem(damage, false); // دمیج
            }
        }
    }

}
