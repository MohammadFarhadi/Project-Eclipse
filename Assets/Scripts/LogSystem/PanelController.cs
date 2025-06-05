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
        panel1 = GameObject.Find("MatrixPanel");
        panel2 = GameObject.Find("LogPanel");
        panel3 = GameObject.Find("StoryPanel");
        panel1.SetActive(false);
        panel2.SetActive(false);
        panel3.SetActive(false);
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

        // فعال کردن پنل اول
        panel1.SetActive(true);
        panel2.SetActive(false);
        panel3.SetActive(false);

        yield return new WaitForSeconds(5f);

        // پنل اول خاموش، پنل دوم روشن
        panel1.SetActive(false);
        panel2.SetActive(true);

        // می‌تونی اینجا هم پنل سوم رو اضافه کنی یا هر کاری که می‌خوای

        isCoroutineRunning = false;
    }

    // فرض کنیم این تابع وقتی پلیر وارد منطقه می‌شود فراخوانی می‌شود
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    // وقتی پلیر خارج شد
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
}