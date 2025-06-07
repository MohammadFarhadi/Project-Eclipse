using System.Collections;
using UnityEngine;
using TMPro;

public class EchoTextDisplay : MonoBehaviour
{
    public TextMeshProUGUI npcText;
    public float typeSpeed = 0.05f;
    public int maxVisibleChars = 30; // حداکثر تعداد حروف روی صفحه

    private Coroutine activeRoutine;

    public void ShowStreamText(string message)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(StreamEffect(message));
    }

    IEnumerator StreamEffect(string message)
    {
        npcText.text = "";
        npcText.alpha = 1f;

        string visible = "";

        for (int i = 0; i < message.Length; i++)
        {
            visible += message[i];

            if (visible.Length > maxVisibleChars)
                visible = visible.Substring(1); // پاک کردن اولین کاراکتر

            npcText.text = visible;

            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitForSeconds(1.5f); // مکث پایانی

        // پاک شدن نهایی مثل شن
        for (int j = 0; j < maxVisibleChars; j++)
        {
            if (npcText.text.Length > 0)
                npcText.text = npcText.text.Substring(1);

            yield return new WaitForSeconds(typeSpeed * 0.8f);
        }

        npcText.text = "";
    }

    public void ForceHide()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        npcText.text = "";
        npcText.alpha = 0f;
    }
}