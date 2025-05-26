using UnityEngine;
using System.Collections;

public class ChunkEndTrigger : MonoBehaviour
{
    public Transform nextChunkStartPoint; // نقطه شروع چانک بعدی
    public GameObject fadePanel;          // پنل مشکی ترنزیشن
    public float fadeDuration = 1f;       // زمان ترنزیشن

    private bool player1Inside = false;
    private bool player2Inside = false;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.name == "RangedPlayer") player1Inside = true;
            if (other.name == "MeleePlayer") player2Inside = true;
        }

        if (player1Inside && player2Inside && !triggered)
        {
            triggered = true;
            StartCoroutine(TransitionToNextChunk());
        }
    }
    IEnumerator TransitionToNextChunk()
    {
        CanvasGroup canvasGroup = fadePanel.GetComponent<CanvasGroup>();

        // Fade Out
        fadePanel.SetActive(true);
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // جابجایی بازیکن‌ها
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.name == "RangedPlayer")
                p.transform.position = nextChunkStartPoint.position + Vector3.left * 1.5f;
            else if (p.name == "MeleePlayer")
                p.transform.position = nextChunkStartPoint.position + Vector3.right * 1.5f;
        }

        yield return new WaitForSeconds(0.3f);

        // Fade In
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = 1 - (t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;
        fadePanel.SetActive(false);

        gameObject.SetActive(false); // تریگر غیرفعال میشه
    }
}
