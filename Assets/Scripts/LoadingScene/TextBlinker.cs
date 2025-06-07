using UnityEngine;
using TMPro;

public class TextBlinker : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public float blinkInterval = 0.8f;
    public float fadeSpeed = 2f;

    private float timer = 0f;
    private bool fadingOut = true;
    private Color originalColor;

    void Start()
    {
        originalColor = loadingText.color;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float alphaChange = fadeSpeed * Time.deltaTime;
        Color color = loadingText.color;

        if (fadingOut)
        {
            color.a -= alphaChange;
            if (color.a <= 0f)
            {
                color.a = 0f;
                fadingOut = false;
                timer = 0f;
            }
        }
        else
        {
            color.a += alphaChange;
            if (color.a >= 1f)
            {
                color.a = 1f;
                fadingOut = true;
                timer = 0f;
            }
        }

        loadingText.color = color;
    }
}