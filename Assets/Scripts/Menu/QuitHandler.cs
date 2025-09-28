using UnityEngine;

public class QuitHandler : MonoBehaviour
{
   
    // متصل به دکمه YES
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is quitting..."); // فقط برای تست در Editor
    }

    // متصل به دکمه NO
    
}