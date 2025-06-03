using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveDistance = 2f;      // فاصله حرکت به بالا و پایین
    public float moveSpeed = 2f;         // سرعت حرکت
    public float waitTime = 1f;          // زمان توقف در نقاط بالا/پایین

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool movingDown = true;
    private bool waiting = false;
    private float waitTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector2.down * moveDistance;
    }

    void Update()
    {
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                // تعویض جهت حرکت
                movingDown = !movingDown;
                targetPosition = startPosition + (movingDown ? Vector2.down : Vector2.up) * moveDistance;
            }
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
        {
            waiting = true;
            waitTimer = waitTime;
        }
    }
}