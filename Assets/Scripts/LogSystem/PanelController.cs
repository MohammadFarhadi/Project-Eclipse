using UnityEngine;
using System.Collections;

public class PanelController : MonoBehaviour
{
    [Header("Panels (filled from PanelManager at runtime)")]
    private GameObject panel1, panel2, panel3;

    [Header("Wire this in the Inspector OR we’ll find it in Start")]
    [SerializeField] private LogSystem logSystem;

    private PlayerControllerBase playerController;
    private bool playerInside, isCoroutineRunning;

    [Header("Log content for THIS trigger")]
    [TextArea] public string logDir1Text;
    [TextArea] public string logDir2Text;
    [TextArea] public string logDir3Text;

    void Start()
    {
        panel1 = PanelManager.Instance.panel1;
        panel2 = PanelManager.Instance.panel2;
        panel3 = PanelManager.Instance.panel3;

        // If not assigned, look under panel2 once.
        if (!logSystem && panel2)
            logSystem = panel2.GetComponentInChildren<LogSystem>(true);
    }

    void Update()
    {
        if (playerInside && playerController != null &&
            playerController.IsInteracting() && !isCoroutineRunning)
        {
            StartCoroutine(ShowPanelsSequence());
        }
    }

    IEnumerator ShowPanelsSequence()
    {
        isCoroutineRunning = true;

        // Show panel1 briefly (your intro)
        panel1.SetActive(true);
        panel2.SetActive(false);
        panel3.SetActive(false);

        yield return new WaitForSeconds(5f);

        // Inject this trigger’s texts, then open the log UI
        if (logSystem)
        {
            logSystem.SetTexts(logDir1Text, logDir2Text, logDir3Text);
            logSystem.OpenLog(); // see addition below
        }

        panel1.SetActive(false);
        panel2.SetActive(true); // shows the log UI

        isCoroutineRunning = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            playerController = other.GetComponent<PlayerControllerBase>();
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
