using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SkipLevel2 : MonoBehaviour
{
    // Reference to your player controller so we can read HasKey
    private PlayerControllerBase player;

    // True while the player is inside the Level-2 trigger
    private bool inLevel2Zone = false;

    private void Awake()
    {
        // assume this script lives on the same GameObject as your PlayerControllerBase
        player = GetComponent<PlayerControllerBase>();
    }

    private void Update()
    {
        // on pressing G, if we have the key and are in the level-2 zone, skip ahead
        if (inLevel2Zone && player != null && player.HasKey && Input.GetKeyDown(KeyCode.G))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);            
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // tag your Level 2 trigger colliders as "Level2Trigger"
        if (other.CompareTag("Level2Trigger"))
            inLevel2Zone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Level2Trigger"))
            inLevel2Zone = false;
    }
}