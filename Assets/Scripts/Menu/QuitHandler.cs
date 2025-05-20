using UnityEngine;

public class QuitHandler : MonoBehaviour
{
    public GameObject optionValue;
    public GameObject quitValue;
    public GameObject profileValue;
    public GameObject homeValue;

    // متصل به دکمه YES
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is quitting..."); // فقط برای تست در Editor
    }

    // متصل به دکمه NO
    public void CancelQuit()
    {
        optionValue.SetActive(false);
        quitValue.SetActive(false);
        profileValue.SetActive(false);
        homeValue.SetActive(true);
    }
}