using UnityEngine;

public class PlatformButtonTrigger : MonoBehaviour
{
    [Header("Platform Settings")]
    public Transform platform;           // پلتفرمی که باید حرکت کنه
    public Transform pointA;             // نقطه شروع
    public Transform pointB;             // نقطه پایان
    public float moveSpeed = 2f;         // سرعت حرکت
    public float waitTime = 1f;          // زمان توقف در هر نقطه

    [Header("Trigger Settings")]
    public string helperObjectName = "Helper";  // اسم ابجکت که باید فعال‌کننده باشه

    private bool isActivated = false;
    private bool movingToB = true;
    private bool isWaiting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == helperObjectName)
        {
            if (!isActivated)
            {
                isActivated = true;
                StartCoroutine(MovePlatform());
            }
        }
    }

    private System.Collections.IEnumerator MovePlatform()
    {
        while (true)
        {
            if (!isWaiting)
            {
                Vector3 target = movingToB ? pointB.position : pointA.position;
                platform.position = Vector3.MoveTowards(platform.position, target, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(platform.position, target) < 0.01f)
                {
                    isWaiting = true;
                    yield return new WaitForSeconds(waitTime);
                    movingToB = !movingToB;
                    isWaiting = false;
                }
            }
            yield return null;
        }
    }
}