using UnityEngine;
using System.Collections;

public class LaserController : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // گیم‌آبجکتی که باید کنترل بشه
    [SerializeField] private float delayToReactivate = 3f; // تأخیر برای فعال‌سازی دوباره

    private Coroutine reactivationCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        // غیر فعال کردن آبجکت هدف
        if (targetObject != null)
            targetObject.SetActive(false);

        // اگر تایمر فعال‌سازی در حال اجراست، متوقفش کن
        if (reactivationCoroutine != null)
        {
            StopCoroutine(reactivationCoroutine);
            reactivationCoroutine = null;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        // شروع تایمر فعال‌سازی دوباره
        if (targetObject != null)
            reactivationCoroutine = StartCoroutine(ReactivateAfterDelay());
    }

    private IEnumerator ReactivateAfterDelay()
    {
        yield return new WaitForSeconds(delayToReactivate);
        targetObject.SetActive(true);
    }

    // بررسی اینکه آیا آبجکت واردشده پلیره یا نه
    private bool IsPlayer(Collider2D col)
    {
        return col.CompareTag("Player");
    }
}