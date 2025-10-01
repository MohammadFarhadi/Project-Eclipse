using UnityEngine;
using DG.Tweening;

public class UIAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float duration = 0.3f;
    public Ease ease = Ease.OutQuad;

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    private void Awake()
    {
        Debug.Log("[UIAnimator] Awake called");

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.Log("[UIAnimator] CanvasGroup component added");
        }
        else
        {
            Debug.Log("[UIAnimator] CanvasGroup component found");
        }

        originalScale = transform.localScale;
        Debug.Log($"[UIAnimator] Original scale saved: {originalScale}");
    }

    public void Show()
    {
        
        Debug.Log("[UIAnimator] Show() called");

        gameObject.SetActive(true);
        Debug.Log("[UIAnimator] GameObject set active");

        // مطمئن شو alpha و scale درست تنظیم شده
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true; // برای تعامل کاربر
        transform.localScale = Vector3.zero;

        Debug.Log("[UIAnimator] CanvasGroup.alpha set to 0, blocksRaycasts enabled, scale set to zero");

        // همزمان Fade و Scale
        canvasGroup.DOFade(1f, duration)
            .SetEase(ease)
            .OnStart(() => Debug.Log("[UIAnimator] Fade started"))
            .OnComplete(() => Debug.Log("[UIAnimator] Fade completed"));

        transform.DOScale(originalScale, duration)
            .SetEase(ease)
            .OnStart(() => Debug.Log("[UIAnimator] Scale animation started"))
            .OnComplete(() => Debug.Log("[UIAnimator] Scale animation completed"));

        Debug.Log("[UIAnimator] Show animations triggered");
    }

    public void Hide()
    {
        gameObject.SetActive(true); 
        Debug.Log("[UIAnimator] Hide() called");

        // غیر فعال کردن تعامل کاربر بلافاصله
        canvasGroup.blocksRaycasts = false;
        Debug.Log("[UIAnimator] blocksRaycasts disabled");

        // Fade و Scale همزمان و بعد از اتمام hide، GameObject را غیر فعال کن
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(0f, duration)
            .SetEase(ease)
            .OnStart(() => Debug.Log("[UIAnimator] Fade out started"))
            .OnComplete(() => Debug.Log("[UIAnimator] Fade out completed")));

        seq.Join(transform.DOScale(Vector3.zero, duration)
            .SetEase(ease)
            .OnStart(() => Debug.Log("[UIAnimator] Scale down started"))
            .OnComplete(() => Debug.Log("[UIAnimator] Scale down completed")));

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            Debug.Log("[UIAnimator] Hide sequence completed, GameObject set inactive");
        });

        Debug.Log("[UIAnimator] Hide sequence triggered");
    }
}
