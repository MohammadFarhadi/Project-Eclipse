using UnityEngine;
using TMPro;

public class EchoDialogueTrigger : MonoBehaviour
{
    private EchoTextDisplay echoDisplayScript;
    private EchoOrb attachedOrb;
    private EchoPuzzleController controller;

    public GameObject dialogueCanvas;
    public TextMeshProUGUI npcText;
    public string echoText;
    public Color echoColor = Color.white;
    public float verticalOffset = 1.2f;

    private bool dialogueDisabled = false;

    void Start()
    {
        echoDisplayScript = dialogueCanvas.GetComponentInChildren<EchoTextDisplay>();
        attachedOrb      = GetComponent<EchoOrb>();
        controller       = FindObjectOfType<EchoPuzzleController>();
    }



    public void ShowDialogue()
    {
        if (dialogueDisabled) return;

        dialogueCanvas.transform.position = transform.position + Vector3.up * verticalOffset;
        dialogueCanvas.SetActive(true);

        npcText.text  = echoText;
        npcText.color = echoColor;

        var mat = npcText.fontMaterial;
        if (mat.HasProperty("_OutlineColor"))
            mat.SetColor("_OutlineColor", echoColor);
        if (mat.HasProperty("_UnderlayColor"))
            mat.SetColor("_UnderlayColor", echoColor * 0.2f);

        echoDisplayScript?.ShowStreamText(echoText);
    }

    public void HideDialogue()
    {
        echoDisplayScript?.ForceHide();
        npcText.text = "";
        dialogueCanvas.SetActive(false);
    }

    public void DisableDialogue()
    {
        dialogueDisabled = true;
        HideDialogue();
    }

    public void EnableDialogue()
    {
        dialogueDisabled = false;
    }
}