using UnityEngine;
using TMPro;

public class EchoDialogueTrigger : MonoBehaviour
{
    private EchoTextDisplay echoDisplayScript;
    public GameObject dialogueCanvas;
    public TextMeshProUGUI npcText;
    public string echoText;
    public Color echoColor = Color.white;  // ← اضافه کن برای هر گوی

    public float verticalOffset = 1.2f;
    void Start()
    {
        echoDisplayScript = dialogueCanvas.GetComponentInChildren<EchoTextDisplay>();
    }


    public void ShowDialogue()
    {
        dialogueCanvas.transform.position = transform.position + new Vector3(0, verticalOffset, 0);

        dialogueCanvas.SetActive(true);
        npcText.text = echoText;

        // تنظیم رنگ face
        npcText.color = echoColor;

        // تنظیم رنگ Outline و Underlay
        Material mat = npcText.fontMaterial;

        // ست کردن outline
        if (mat.HasProperty("_OutlineColor"))
            mat.SetColor("_OutlineColor", new Color(echoColor.r, echoColor.g, echoColor.b, 1f));

        // ست کردن underlay
        if (mat.HasProperty("_UnderlayColor"))
            mat.SetColor("_UnderlayColor", new Color(echoColor.r * 0.2f, echoColor.g * 0.2f, echoColor.b * 0.2f, 0.5f));
        echoDisplayScript.ShowStreamText(echoText);

    }


    public void HideDialogue()
    {
        npcText.text = "";
        dialogueCanvas.SetActive(false);
    }
}