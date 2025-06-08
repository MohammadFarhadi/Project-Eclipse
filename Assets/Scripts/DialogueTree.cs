using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class DialogueTree : MonoBehaviour
{
    GameObject player1;
    GameObject player2;
    public TextMeshProUGUI npcText;
    public Button[] optionButtons;

    private DialogueNode currentNode;
    public GameObject dialoguePanel; // ← اینو بالای کلاس اضافه کن
    public class DialogueNode
    {
        public string text;                   // NPC dialogue
        public string[] playerChoicesText;    // Player's choice texts
        public DialogueNode[] choices;        // Next dialogue nodes

        public DialogueNode(string text, string[] playerChoicesText = null)
        {
            this.text = text;
            this.playerChoicesText = playerChoicesText ?? new string[0];
            choices = new DialogueNode[this.playerChoicesText.Length];
        }
    }

    void Start()
    {
        player1 = GameObject.Find("Ranged1Player");
        if (player1 == null)
        {
            player1 = GameObject.Find("RangedPlayer");
        }
        player2 = GameObject.Find("Melle1Player");
        if (player2 == null)
        {
            player2 = GameObject.Find("Melle2Player");
        }
    }
    public void StartDialogue()
    {
        BuildTree();
        ShowNode(currentNode);
    }

    void BuildTree()
    {
        var root = new DialogueNode(
            "Old Man: So you've finally arrived... Are you ready?",
            new string[] { "Yes, I am ready", "I'm still unsure", "No, I don't want to" }
        );

        root.choices[0] = new DialogueNode(
            "Old Man: Very well, let's begin.",
            new string[] { "You're right", "I'm ready", "Go on" }
        );

        root.choices[1] = new DialogueNode(
            "Old Man: You still have time to think.",
            new string[] { "Time is running out", "Tomorrow might be too late" }
        );

        root.choices[2] = new DialogueNode(
            "Old Man: Good luck, stranger.",
            new string[] { "Goodbye" }
        );

        // End branches
        root.choices[0].choices[0] = new DialogueNode("Old Man: A tough road lies ahead.");
        root.choices[0].choices[1] = new DialogueNode("Old Man: Then show me your courage.");
        root.choices[0].choices[2] = new DialogueNode("Old Man: I'm glad you said that.");

        root.choices[1].choices[0] = new DialogueNode("Old Man: There's not much time left.");
        root.choices[1].choices[1] = new DialogueNode("Old Man: Tomorrow might be too late.");

        root.choices[2].choices[0] = new DialogueNode("Old Man: Farewell, traveler.");

        currentNode = root;
    }

    void ShowNode(DialogueNode node)
    {
        currentNode = node;
        npcText.text = node.text;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < node.choices.Length && node.choices[i] != null)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = node.playerChoicesText[i];
                int index = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => ShowNode(node.choices[index]));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        if (node.choices.Length == 0)
        {
            StartCoroutine(EndDialogueWithDelay());
        }
    }
    IEnumerator EndDialogueWithDelay()
    {
        yield return new WaitForSeconds(1f);
        player1.GetComponent<RangedPlayerController>().enabled = true; // غیرفعال‌کردن حرکت
        player2.GetComponent<MeleePlayerController>().enabled = true;
        player2.GetComponent<PlayerControllerBase>().enabled = true;
        player1.GetComponent<PlayerControllerBase>().enabled = true;

        dialoguePanel.SetActive(false);
    }
    public void ShowCustomDialogue(string customText)
    {
        npcText.text = customText;
        foreach (var btn in optionButtons)
            btn.gameObject.SetActive(false);

        StartCoroutine(EndDialogueWithDelay());
    }


}
