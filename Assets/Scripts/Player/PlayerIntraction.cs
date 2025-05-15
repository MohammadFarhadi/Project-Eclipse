using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private bool canInteract = false;
    [SerializeField] GameObject targetGround;
    [SerializeField] float moveSpeed = 2f;

    private bool isMoving = false;
    private float moveTimer = 0f;
    private float moveDuration = 2.5f;

    void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.X))
        {
            isMoving = true;
            moveTimer = 0f; // ریست تایمر در لحظه شروع
        }

        if (isMoving)
        {
            moveTimer += Time.deltaTime;

            // حرکت کردن به سمت چپ
            targetGround.transform.position += Vector3.left * moveSpeed * Time.deltaTime;

            // اگر ۲.۵ ثانیه گذشت، متوقف شو
            if (moveTimer >= moveDuration)
            {
                isMoving = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interaction"))
        {
            canInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interaction"))
        {
            canInteract = false;
        }
    }
}