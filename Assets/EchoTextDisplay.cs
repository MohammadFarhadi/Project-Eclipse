using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EchoTextDisplay : MonoBehaviour
{
    public TextMeshProUGUI npcText;
    public float typeSpeed = 0.03f;
    public float lineDelay = 1f;

    private Coroutine activeRoutine;

    public void ShowStreamText(string message)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(TypeLineByLine(message));
    }

    IEnumerator TypeLineByLine(string message)
    {
        npcText.text = "";

        string[] lines = message.Split('\n');
        foreach (string line in lines)
        {
            string current = "";
            foreach (char c in line)
            {
                current += c;
                npcText.text = current;
                yield return new WaitForSeconds(typeSpeed);
            }

            yield return new WaitForSeconds(lineDelay);
        }

        npcText.text = "";
    }

    public void ForceHide()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        npcText.text = "";
    }
    public void SetColor(Color newColor)
    {
        npcText.color = newColor;
    }
}