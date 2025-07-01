using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkEndTrigger : MonoBehaviour
{
    public GameObject fadePanel;
    public float fadeDuration = 1f;

    private bool player1Inside = false;
    private bool player2Inside = false;
    private bool isTransitioning = false;

    private ChunkManager chunkManager;
    private GameObject currentChunk;

    private void Awake()
    {
        // پیدا کردن FadePanel حتی اگر غیرفعال باشد
        if (fadePanel == null)
        {
            fadePanel = GameObject.FindWithTag("FadePanel");
            if (fadePanel == null)
            {
                Debug.LogWarning("FadePanel not found. Please assign it manually or tag it as 'FadePanel'.");
                return;
            }
        }

        // اطمینان از وجود CanvasGroup
        if (fadePanel.GetComponent<CanvasGroup>() == null)
        {
            fadePanel.AddComponent<CanvasGroup>();
        }

        
        chunkManager = FindObjectOfType<ChunkManager>();
        currentChunk = transform.parent.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.name == "RangedPlayer(Clone)" || other.name == "Ranged1Player(Clone)")
                player1Inside = true;
            else if (other.name == "Melle1Player(Clone)" || other.name == "Melle2Player(Clone)")
                player2Inside = true;
        }
    }

    private void Update()
    {
        if (!isTransitioning && player1Inside && player2Inside)
        {
            isTransitioning = true;
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

        CanvasGroup canvasGroup = fadePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("FadePanel does not have a CanvasGroup component.");
            yield break;
        }

        fadePanel.SetActive(true);
        canvasGroup.alpha = 0;

        // Fade Out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // پیدا کردن چانک بعدی
        int currentIndex = chunkManager.activeChunks.IndexOf(currentChunk);
        if (currentIndex + 1 >= chunkManager.activeChunks.Count)
        {
            Debug.Log("چانک بعدی وجود ندارد");
            yield break;
        }

        GameObject nextChunk = chunkManager.activeChunks[currentIndex + 1];
        Transform nextStartPoint = nextChunk.transform.Find("StartPoint");
        if (nextStartPoint == null)
        {
            Debug.LogError("StartPoint داخل چانک بعدی پیدا نشد!");
            yield break;
        }

        // انتقال بازیکن‌ها
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.name == "RangedPlayer(Clone)" || p.name == "Ranged1Player(Clone)")
                p.transform.position = nextStartPoint.position + Vector3.left * 1.5f;
            else if (p.name == "Melle1Player(Clone)" || p.name == "Melle2Player(Clone)")
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

        // غیرفعال کردن تریگر برای جلوگیری از اجرای مجدد
        gameObject.SetActive(false);
    }
}
