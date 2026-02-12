using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TerminalUIManager : MonoBehaviour
{
    public static TerminalUIManager Instance;

    public GameObject bootBackground;
    public GameObject bootScreen;
    public GameObject directoryScreen;
    public GameObject logContentScreen;

    public TextMeshProUGUI logText;
    public Button backButton;

    private void Awake()
    {
        Instance = this;
        CloseAll();
    }

    public void StartTerminal()
    {
        StartCoroutine(BootSequence());
    }

    IEnumerator BootSequence()
    {
        CloseAll();
        bootBackground.SetActive(true);
        bootScreen.SetActive(true);
        yield return new WaitForSeconds(2f); // Simulate matrix boot
        bootScreen.SetActive(false);
        directoryScreen.SetActive(true);
    }

    public void ShowLog(string content)
    {
        directoryScreen.SetActive(false);
        logContentScreen.SetActive(true);
        StartCoroutine(TypeText(content));
    }

    IEnumerator TypeText(string content)
    {
        logText.text = "";
        foreach (char c in content)
        {
            logText.text += c;
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void GoBackToDirectory()
    {
        logContentScreen.SetActive(false);
        directoryScreen.SetActive(true);
    }

    void CloseAll()
    {
        bootScreen.SetActive(false);
        directoryScreen.SetActive(false);
        logContentScreen.SetActive(false);
        bootBackground.SetActive(false);
    }
}