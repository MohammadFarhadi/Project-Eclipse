using UnityEngine;
using System.Collections;

public class PanelController : MonoBehaviour
{
    private GameObject panel1;
    private GameObject panel2;
    private GameObject panel3;

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
        if (playerInside && Input.GetKeyDown(KeyCode.O) && !isCoroutineRunning)
        {
            StartCoroutine(ShowPanelsSequence());
        }
    }

    IEnumerator ShowPanelsSequence()
    {
        isCoroutineRunning = true;

        panel1.SetActive(true);
        panel2.SetActive(false);
        panel3.SetActive(false);

        yield return new WaitForSeconds(5f);

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