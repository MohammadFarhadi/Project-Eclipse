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

    private void Start()
    {
        directory1Button.onClick.AddListener(() => ShowStory("This is a placeholder story for Directory 1. Real content will be added soon."));
        directory2Button.onClick.AddListener(() => ShowStory("This is a sample story for Directory 2. Currently just dummy text."));
        directory3Button.onClick.AddListener(() => ShowStory("Directory 3 also has its own story, but it’s empty for now."));

        backButton.onClick.AddListener(BackToLogPanel);
        exitButton.onClick.AddListener(CloseLogPanel);
    }

    void ShowStory(string story)
    {
        logPanel.SetActive(false);
        storyPanel.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(TypeWriterEffect(story));
    }

    IEnumerator TypeWriterEffect(string story)
    {
        storyText.text = "";
        foreach (char c in story)
        {
            storyText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
    }

    void BackToLogPanel()
    {
        StopAllCoroutines();
        storyPanel.SetActive(false);
        logPanel.SetActive(true);
    }

    void CloseLogPanel()
    {
        logPanel.SetActive(false);
        storyPanel.SetActive(false);
    }
}