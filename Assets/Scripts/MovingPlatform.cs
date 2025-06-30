using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Point References")]
    [Tooltip("The first point the platform moves to.")]
    public Transform pointA;

    [Tooltip("The second point the platform moves to.")]
    public Transform pointB;

    [Header("Movement Settings")]
    [Tooltip("Speed at which the platform moves.")]
    public float moveSpeed = 2f;

    [Tooltip("Time (in seconds) to wait when the platform reaches a point.")]
    public float waitTime = 1f;

    private Vector2 _targetPosition2D;
    private bool _waiting = false;
    private float _waitTimer = 0f;
    private bool _goingToB = true;
    private float _initialZ;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("MovingPlatform: Please assign both Point A and Point B in the Inspector.");
            enabled = false;
            return;
        }

        // ذخیره Z اولیه
        _initialZ = transform.position.z;

        // هدف اولیه حرکت به سمت B
        _targetPosition2D = new Vector2(pointB.position.x, pointB.position.y);
        _goingToB = true;
    }

    void Update()
    {
        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _waiting = false;

                // تغییر جهت حرکت
                _goingToB = !_goingToB;
                _targetPosition2D = _goingToB
                    ? new Vector2(pointB.position.x, pointB.position.y)
                    : new Vector2(pointA.position.x, pointA.position.y);
            }
            return;
        }

        // موقعیت فعلی دوبعدی
        Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.y);

        // محاسبه موقعیت جدید دوبعدی
        Vector2 newPos2D = Vector2.MoveTowards(currentPos2D, _targetPosition2D, moveSpeed * Time.deltaTime);

        // تنظیم موقعیت جدید با حفظ Z
        transform.position = new Vector3(newPos2D.x, newPos2D.y, _initialZ);

        // اگر به مقصد رسیدیم
        if (Vector2.Distance(newPos2D, _targetPosition2D) < 0.01f)
        {
            _waiting = true;
            _waitTimer = waitTime;
        }
    }

    void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Vector3 pointAPos = new Vector3(pointA.position.x, pointA.position.y, transform.position.z);
            Vector3 pointBPos = new Vector3(pointB.position.x, pointB.position.y, transform.position.z);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointAPos, 0.1f);
            Gizmos.DrawSphere(pointBPos, 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointAPos, pointBPos);
        }
    }
}
