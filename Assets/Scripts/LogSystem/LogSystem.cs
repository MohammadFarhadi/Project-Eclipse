using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogSystem : MonoBehaviour
{
    [Header("Panels")]
    public GameObject logPanel;
    public GameObject storyPanel;

    [Header("UI Elements")]
    public TMP_Text storyText;
    public Button backButton;

    [Header("Directory Buttons")]
    public Button directory1Button;
    public Button directory2Button;
    public Button directory3Button;

    [Header("Exit Button")]
    public Button exitButton;

    [Header("Click Area on the story panel (add a Button on the panel or a child)")]
    public Button storyPanelClick;

    [Header("Story texts (set per instance)")]
    [TextArea] public string directory1Text = "People are dying here, we need to evacuate them as fast as possible.";
    [TextArea] public string directory2Text = "There have been mutations in infected people; we quarantined them.";
    [TextArea] public string directory3Text = "Replace this with your third log entry.";

    // typing state (so we can fast-forward)
    Coroutine typingRoutine;
    bool isTyping;
    string fullStory;

    void Start()
    {
        directory1Button.onClick.AddListener(() => ShowStory(directory1Text));
        directory2Button.onClick.AddListener(() => ShowStory(directory2Text));
        directory3Button.onClick.AddListener(() => ShowStory(directory3Text));

        backButton.onClick.AddListener(BackToLogPanel);
        exitButton.onClick.AddListener(CloseLogPanel);

        if (storyPanelClick != null)
            storyPanelClick.onClick.AddListener(OnStoryPanelClick);
    }

    void ShowStory(string story)
    {
        logPanel.SetActive(false);
        storyPanel.SetActive(true);

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        fullStory = story ?? "";
        typingRoutine = StartCoroutine(TypeWriterEffect(fullStory));
    }

    IEnumerator TypeWriterEffect(string story)
    {
        isTyping = true;
        storyText.text = "";
        var wait = new WaitForSeconds(0.05f);

        foreach (char c in story)
        {
            storyText.text += c;
            yield return wait;
        }

        isTyping = false;
        typingRoutine = null;
    }

    // 1st tap while typing -> finish instantly; 2nd tap -> go back
    void OnStoryPanelClick()
    {
        if (isTyping)
        {
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            storyText.text = fullStory;
            isTyping = false;
            typingRoutine = null;
        }
        else
        {
            BackToLogPanel();
        }
    }

    void BackToLogPanel()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        isTyping = false;
        storyPanel.SetActive(false);
        logPanel.SetActive(true);
    }

    void CloseLogPanel()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        isTyping = false;
        logPanel.SetActive(false);
        storyPanel.SetActive(false);
    }
}
