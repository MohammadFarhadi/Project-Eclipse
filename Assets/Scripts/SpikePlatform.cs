using UnityEngine;

public class SpikePlatform : MonoBehaviour
{
    public float moveDistance = 2f;     // فاصله حرکت به بالا
    public float moveSpeed = 3f;        // سرعت حرکت تیغ
    public float waitTime = 2f;         // زمان مکث در بالا یا پایین

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool movingDown = false;    // شروع حرکت به بالا
    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector2.up * moveDistance;
    }

    void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                // تغییر جهت حرکت تیغ
                movingDown = !movingDown;
                targetPosition = startPosition + (movingDown ? Vector2.down : Vector2.up) * moveDistance;
            }
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
        {
            isWaiting = true;
            waitTimer = waitTime;
        }
    }
}