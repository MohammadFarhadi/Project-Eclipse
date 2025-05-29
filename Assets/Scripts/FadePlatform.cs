using System.Collections;
using UnityEngine;

public class FadePlatform : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Coroutine currentFade;
    public GameObject[] objectsToFade;
    public float fadeSpeed;
    bool landed = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!landed)
            {
                foreach (GameObject obj in objectsToFade)
                {
                    if (obj != null && obj != gameObject)
                    { 
                        FadePlatform fadeScript = obj.GetComponent<FadePlatform>();
                        if (fadeScript != null)
                        {
                            fadeScript.StartCoroutine(fadeScript.LittleFade());
                        }
                    }
                }
                landed = true;
            }

            StartFade(FadeToAlpha(1, 0.5f)); // This platform fully visible
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartFade(FadeToAlpha(0, 1.5f)); // Fade out when leaving
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

    public IEnumerator LittleFade()
    {
        StartFade(FadeToAlpha(0.6f, 0.1f));
        yield return new WaitForSeconds(0.5f);
        StartFade(FadeToAlpha(0f, 0.3f));
    }
}
