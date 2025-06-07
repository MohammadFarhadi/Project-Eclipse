using System.Collections;
using UnityEngine;
using TMPro;

public class GameOverSceneTyper : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public float typingSpeed = 0.1f;
    public string message = "Subjects Failed \nWe Have To Choose Next Subject  ";

    void Start()
    {
        StartCoroutine(TypeGameOver());
    }

    private IEnumerator TypeGameOver()
    {
        gameOverText.text = "";
        foreach (char c in message)
        {
            gameOverText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}