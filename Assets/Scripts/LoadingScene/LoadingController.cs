using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public Slider slider;
    public float duration = 19f; // مدت زمان کامل شدن

    private float timer = 0f;

    void Update()
    {
        if (timer < duration)
        {
            timer += Time.deltaTime;
            slider.value = timer / duration;
        }
        else if (timer > duration)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}