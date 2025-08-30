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

    // These are the texts we’ll override per trigger
    [TextArea] public string directory1Text;
    [TextArea] public string directory2Text;
    [TextArea] public string directory3Text;

    void Awake()
    {
        // Wire once; listeners read the *current field values* at click time
        directory1Button.onClick.RemoveAllListeners();
        directory1Button.onClick.AddListener(() => ShowStory(directory1Text));

        directory2Button.onClick.RemoveAllListeners();
        directory2Button.onClick.AddListener(() => ShowStory(directory2Text));

        directory3Button.onClick.RemoveAllListeners();
        directory3Button.onClick.AddListener(() => ShowStory(directory3Text));

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(BackToLogPanel);

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(CloseLogPanel);
    }

    // Call this before showing the panel to inject per-object data
    public void SetTexts(string t1, string t2, string t3)
    {
        directory1Text = t1;
        directory2Text = t2;
        directory3Text = t3;
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
