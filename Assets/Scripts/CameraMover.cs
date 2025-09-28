using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [Header("Target")]
    public Transform target;   // نقطه‌ای که باید دوربین بره
    [Header("Settings")]
    public float speed = 5f;   // سرعت ثابت حرکت

    private bool isMoving = true;

    public void MoveToTarget()
    {
        if (target != null)
            isMoving = true;
    }

    void Update()
    {
        if (isMoving && target != null)
        {
            // حرکت با سرعت ثابت
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );

            // وقتی رسید، متوقف کن
            if (Vector3.Distance(transform.position, target.position) < 0.01f)
            {
                transform.position = target.position;
                isMoving = false;
            }
        }
    }
}