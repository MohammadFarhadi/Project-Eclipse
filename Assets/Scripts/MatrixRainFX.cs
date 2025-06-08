using UnityEngine;
using TMPro;
using System.Collections;

public class MatrixRainFX : MonoBehaviour
{
    public TextMeshProUGUI matrixText;
    public float refreshRate = 0.05f;
    public int lineCount = 20;
    public int lineLength = 80;
    public float delayBeforeStart = 0.5f; // boot wait

    private void OnEnable()
    {
        matrixText.text = ""; // clear old text
        StartCoroutine(BootThenRain());
    }

    IEnumerator BootThenRain()
    {
        yield return new WaitForSeconds(delayBeforeStart);
        yield return StartCoroutine(PlayRain());
    }

    IEnumerator PlayRain()
    {
        matrixText.text = ""; // clean again before writing
        for (int i = 0; i < lineCount; i++)
        {
            matrixText.text = GenerateLines();
            yield return new WaitForSeconds(refreshRate);
        }
    }

    string GenerateLines()
    {
        const string chars = "01ABCDEFGHIJKLniggers niggers i hate niggers NOPQRSTUVWXYZ";
        string result = "";
        for (int i = 0; i < lineLength; i++)
        {
            for (int j = 0; j < 40; j++)
                result += chars[Random.Range(0, chars.Length)];
            result += "\n";
        }
        return result;
    }
}
