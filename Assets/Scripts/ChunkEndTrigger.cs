using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkEndTrigger : MonoBehaviour
{
    public GameObject fadePanel;
    public float fadeDuration = 1f;

    private bool player1Inside = false;
    private bool player2Inside = false;

    private ChunkManager chunkManager;
    private GameObject currentChunk;

    private void Start()
    {
        // پیدا کردن FadePanel حتی اگر غیرفعال باشد
        if (fadePanel == null)
        {
            fadePanel = GameObject.FindWithTag("FadePanel");
            fadePanel.SetActive(false);
            if (fadePanel == null)
            {
                Debug.LogWarning("FadePanel not found in the scene. Please assign it manually or tag it as 'FadePanel'.");
            }
        }


        chunkManager = FindObjectOfType<ChunkManager>();
        currentChunk = transform.parent.gameObject; // پدر = چانک فعلی
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.name == "RangedPlayer" || other.name == "Ranged1Player") player1Inside = true;
            if (other.name == "Melle2Player" || other.name == "Melle1Player") player2Inside = true;
        }

        if (player1Inside && player2Inside)
        {
            StartCoroutine(TransitionToNextChunk());
        }
    }

    IEnumerator TransitionToNextChunk()
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("Cannot transition because fadePanel is null.");
            yield break;
        }

        // Fade Out
        CanvasGroup canvasGroup = fadePanel.GetComponent<CanvasGroup>();
        fadePanel.SetActive(true);
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // پیدا کردن چانک بعدی
        int currentIndex = chunkManager.instantiatedChunks.IndexOf(currentChunk);
        if (currentIndex + 1 >= chunkManager.instantiatedChunks.Count)
        {
            Debug.Log("چانک بعدی وجود ندارد");
            yield break;
        }

        GameObject nextChunk = chunkManager.instantiatedChunks[currentIndex + 1];
        Transform nextStartPoint = nextChunk.transform.Find("StartPoint");
        if (nextStartPoint == null)
        {
            Debug.LogError("StartPoint داخل چانک بعدی پیدا نشد!");
            yield break;
        }


        // جابجایی بازیکن‌ها
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.name == "RangedPlayer" || p.name == "Ranged1Player")
                p.transform.position = nextStartPoint.position + Vector3.left * 1.5f;
            else if (p.name == "Melle1Player" || p.name == "Melle2Player")
                p.transform.position = nextStartPoint.position + Vector3.right * 1.5f;
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

        gameObject.SetActive(false); // غیرفعال کردن تریگر
    }
}
