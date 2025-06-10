using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; 
    public GameObject shopCanvas; // پنل UI مربوط به Pause
    private bool isPaused = false;

    private void Start()
    {
        shopCanvas.SetActive(false);
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // ادامه دادن بازی
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // توقف بازی
        isPaused = true;
    }
    public void Rematch()
    {
        Time.timeScale = 1f; // اطمینان از اینکه بازی Pause نباشه
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}       