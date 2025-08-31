using UnityEngine;

public class TriggerUI : MonoBehaviour
{
    public GameObject uiPanel; // پنل UI که می‌خوای ظاهر بشه

    private void Start()
    {
        // اول مطمئن می‌شیم که پنل غیرفعاله
        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // اگر واردکننده تریگر پلیر بود
        if (other.CompareTag("Player"))
        {
            if (uiPanel != null)
                uiPanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // وقتی پلیر خارج شد، UI بسته بشه
        if (other.CompareTag("Player"))
        {
            if (uiPanel != null)
                uiPanel.SetActive(false);
        }
    }
}