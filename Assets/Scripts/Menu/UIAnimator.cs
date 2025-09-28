using UnityEngine;
using DG.Tweening;

public class UIAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float duration = 0.3f;   // مدت زمان انیمیشن
    public Ease ease = Ease.OutQuad; // نوع Ease انیمیشن

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    private void Awake()
    {
        // برای fade نیاز به CanvasGroup داریم
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // مقیاس اولیه رو ذخیره میکنیم
        originalScale = transform.localScale;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        transform.localScale = Vector3.zero;

        // هم fade هم scale با هم
        canvasGroup.DOFade(1, duration);
        transform.DOScale(originalScale, duration).SetEase(ease);
    }

    public void Hide()
    {
        // اول انیمیشن بسته شدن
        canvasGroup.DOFade(0, duration);
        transform.DOScale(Vector3.zero, duration).SetEase(ease)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}