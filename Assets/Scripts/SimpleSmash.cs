using UnityEngine;
using System.Collections;

public class SimpleSmash : MonoBehaviour
{
    public float waitTime = 3f;        // چند ثانیه بین هر ضربه صبر کنه
    public float moveDownSpeed = 15f;  // سرعت پایین اومدن
    public float moveUpSpeed = 3f;     // سرعت بالا رفتن
    public float groundY = 0f;         // نقطه‌ای که باید به زمین برسه
    public float startY = 5f;          // نقطه‌ای که ازش شروع می‌کنه (بالا)

    void Start()
    {
        StartCoroutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while (true)
        {
            // حرکت سریع به پایین
            while (transform.position.y > groundY)
            {
                transform.position += Vector3.down * moveDownSpeed * Time.deltaTime;
                yield return null;
            }

            // توقف کوتاه روی زمین
            yield return new WaitForSeconds(0.2f);

            // حرکت آرام به بالا
            while (transform.position.y < startY)
            {
                transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;
                yield return null;
            }

            // صبر قبل از ضربه‌ی بعدی
            yield return new WaitForSeconds(waitTime);
        }
    }
}