using UnityEngine;
using UnityEngine.SceneManagement;

public class GameButtonsManager : MonoBehaviour
{
    // این متد بازی فعلی رو ری‌لود می‌کنه
    public void RestartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // این متد شما رو به منوی اصلی برمی‌گردونه (نام سین منو رو به‌جای "MainMenu" بذار)
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // اسم Scene منو رو دقیق بنویس
    }

    // این متد بازی رو می‌بنده
    public void ExitGame()
    {
        Debug.Log("بازی بسته شد"); // فقط در Editor دیده میشه
        Application.Quit();
    }
}