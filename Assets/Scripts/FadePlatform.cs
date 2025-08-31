using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class FadePlatform : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Coroutine currentFade;

    [Header("Fade Settings")]
    public float fadeInTime = 0.2f;
    public float fadeOutTime = 0.5f;

    private int playerContacts = 0; // handles multi-collisions

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // start invisible
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerContacts++;
            if (playerContacts == 1) // first contact
            {
                StartFade(FadeToAlpha(1f, fadeInTime)); // fully visible
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerContacts--;
            if (playerContacts <= 0)
            {
                playerContacts = 0;
                StartFade(FadeToAlpha(0f, fadeOutTime)); // invisible when no player
            }
        }
    }

    void StartFade(IEnumerator routine)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);
        currentFade = StartCoroutine(routine);
    }

    IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        float startAlpha = spriteRenderer.color.a;
        float time = 0f;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
            time += Time.deltaTime;
            yield return null;
        }

        Color final = spriteRenderer.color;
        final.a = targetAlpha;
        spriteRenderer.color = final;
    }
}
