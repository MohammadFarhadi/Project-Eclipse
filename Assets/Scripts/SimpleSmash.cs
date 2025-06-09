using UnityEngine;
using System.Collections;

public class SimpleSmash : MonoBehaviour
{
    public float waitTime = 3f;        // صبر بین ضربه‌ها
    public float moveDownSpeed = 15f;  // سرعت پایین اومدن
    public float moveUpSpeed = 3f;     // سرعت بالا رفتن
    public float groundY = 0f;         // Y مقصد پایین
    public float startY = 5f;          // Y شروع بالا

    private Vector3 groundPosition;
    private Vector3 startPosition;

    void Start()
    {
        groundPosition = new Vector3(transform.position.x, groundY, transform.position.z);
        startPosition = new Vector3(transform.position.x, startY, transform.position.z);

        transform.position = startPosition;

        StartCoroutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while (true)
        {
            // حرکت سریع به پایین
            while (Vector3.Distance(transform.position, groundPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, groundPosition, moveDownSpeed * Time.deltaTime);
                yield return null;
            }

            // توقف کوتاه روی زمین
            yield return new WaitForSeconds(0.2f);

            // حرکت آرام به بالا
            while (Vector3.Distance(transform.position, startPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, startPosition, moveUpSpeed * Time.deltaTime);
                yield return null;
            }

            // توقف قبل از ضربه‌ی بعدی
            yield return new WaitForSeconds(waitTime);
        }
    }
}