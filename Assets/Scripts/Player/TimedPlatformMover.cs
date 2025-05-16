using UnityEngine;

public class TimedPlatformMover : MonoBehaviour
{
    public Transform platform;
    public float moveSpeed = 2f;
    public float moveDuration = 2.5f;

    private bool isMoving = false;
    private bool movingUp = true;
    private float moveTimer = 0f;
    private bool playerOnPad = false;

    void Update()
    {
        // وقتی بازیکن روی کنترل‌پد هست و X رو می‌زنه و هنوز در حال حرکت نیست
        if (playerOnPad && Input.GetKeyDown(KeyCode.X) && !isMoving)
        {
            isMoving = true;
            moveTimer = 0f;
        }

        // اگر در حال حرکت باشه
        if (isMoving)
        {
            float direction = movingUp ? 1f : -1f;
            platform.transform.Translate(Vector2.up * direction * moveSpeed * Time.deltaTime);

            moveTimer += Time.deltaTime;
            if (moveTimer >= moveDuration)
            {
                isMoving = false;
                movingUp = !movingUp; // دفعه بعد جهت برعکس
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPad = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPad = false;
        }
    }
}