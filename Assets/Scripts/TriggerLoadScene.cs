using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerLoadScene : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Level2"; // نام صحنه‌ای که می‌خواهی لود شود

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}