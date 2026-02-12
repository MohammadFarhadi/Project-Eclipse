using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class DialogueTree : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI npcText;
    public Button[] optionButtons;
    public GameObject dialoguePanel;

    [Header("Data")]
    public DialogueDefinition definition;   // assign a DialogueDefinition asset in Inspector

    [Header("Events")]
    public UnityEvent OnDialogueEnded;      // external listeners (e.g., disable ground) subscribe here

    // runtime state (index into definition.nodes)
    private int currentIndex = -1;

    // ---- Public API ----
    public void StartDialogue()
    {
        if (definition == null || definition.nodes == null || definition.nodes.Length == 0)
        {
            Debug.LogWarning("DialogueTree: No DialogueDefinition assigned or it's empty.");
            return;
        }

        // show the starting node (usually 0)
        ShowNode(definition.rootIndex);
    }

    public void ShowCustomDialogue(string customText)
    {
        if (npcText) npcText.text = customText;
        HideAllButtons();
        StartCoroutine(EndDialogueWithDelay());
    }

    // ---- Internal ----
    private void ShowNode(int index)
    {
        // guard against bad indices
        if (index < 0 || index >= definition.nodes.Length)
        {
            Debug.LogWarning($"DialogueTree: Node index {index} is out of range.");
            StartCoroutine(EndDialogueWithDelay());
            return;
        }

        currentIndex = index;
        var node = definition.nodes[currentIndex];

        if (npcText) npcText.text = node.text;

        // wire buttons
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (node.choices != null && i < node.choices.Length)
            {
                var btn = optionButtons[i];
                btn.gameObject.SetActive(true);

                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label) label.text = node.choices[i];

                // capture next index safely
                int nextIndex = (node.next != null && i < node.next.Length) ? node.next[i] : -1;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (nextIndex >= 0) ShowNode(nextIndex);
                    else StartCoroutine(EndDialogueWithDelay());
                });
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
                optionButtons[i].onClick.RemoveAllListeners();
            }
        }

        // end node if there are no choices
        if (node.choices == null || node.choices.Length == 0)
            StartCoroutine(EndDialogueWithDelay());
    }

    private IEnumerator EndDialogueWithDelay()
    {
        yield return new WaitForSeconds(1f);

        if (dialoguePanel) dialoguePanel.SetActive(false);
        OnDialogueEnded?.Invoke();  // let external listeners handle gameplay (offline/online)
    }

    private void HideAllButtons()
    {
        foreach (var btn in optionButtons)
        {
            if (!btn) continue;
            btn.onClick.RemoveAllListeners();
            btn.gameObject.SetActive(false);
        }
    }
}
