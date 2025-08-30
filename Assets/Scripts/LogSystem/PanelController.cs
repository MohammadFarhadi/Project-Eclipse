using UnityEngine;
using System.Collections;

public class PanelController : MonoBehaviour
{
    private GameObject panel1;
    private GameObject panel2;
    private GameObject panel3;
    private PlayerControllerBase playerController;

    private bool playerInside = false;
    private bool isCoroutineRunning = false;

    void Start()
    {
        panel1 = PanelManager.Instance.panel1;
        panel2 = PanelManager.Instance.panel2;
        panel3 = PanelManager.Instance.panel3;
    }

    void Update()
    {
        if (playerInside && playerController.IsInteracting()&& !isCoroutineRunning)
        {
            StartCoroutine(ShowPanelsSequence());
        }
    }

    [Header("Log content for THIS trigger")]
    [TextArea] public string logDir1Text;
    [TextArea] public string logDir2Text;
    [TextArea] public string logDir3Text;

    IEnumerator ShowPanelsSequence()
    {
        isCoroutineRunning = true;

        // Step 1: show panel1 for 5s
        panel1.SetActive(true);
        panel2.SetActive(false);
        panel3.SetActive(false);

        yield return new WaitForSeconds(5f);

        // Step 2: inject per-object texts into the single LogSystem, then show panel2
        var log = panel2.GetComponentInChildren<LogSystem>(true); // true finds it even if inactive
        if (log)
        {
            log.SetTexts(logDir1Text, logDir2Text, logDir3Text);
        }

        panel1.SetActive(false);
        panel2.SetActive(true);

        isCoroutineRunning = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
    
}