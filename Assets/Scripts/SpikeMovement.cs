using UnityEngine;

public class SpikeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Distance the trap travels along its local 'up' direction.")]
    public float moveDistance = 2f;

    [Tooltip("Speed at which the trap moves.")]
    public float moveSpeed = 2f;

    [Tooltip("Seconds to stay waiting at each extreme (up or down).")]
    public float waitTime = 1f;

    [Header("Phase Offset")]
    [Tooltip("Initial delay (in seconds) before this trap begins its first movement.")]
    public float initialDelay = 0f;

    private Vector2 _startPosition2D;
    private Vector2 _targetPosition2D;
    private Vector2 _upDirection2D;
    private bool _movingUp = true;
    private bool _waiting = false;
    private float _waitTimer = 0f;
    private bool _firstCycle = true;
    private float _initialZ;

    void Start()
    {
        // ذخیره موقعیت اولیه دوبعدی
        Vector3 startPos3D = transform.position;
        _startPosition2D = new Vector2(startPos3D.x, startPos3D.y);
        _initialZ = startPos3D.z;

        // محاسبه جهت up محلی
        _upDirection2D = new Vector2(transform.up.x, transform.up.y).normalized;

        // هدف اولیه حرکت به بالا است
        _movingUp = true;
        _targetPosition2D = _startPosition2D + _upDirection2D * moveDistance;

        // اگر delay اولیه وجود دارد
        if (initialDelay > 0f)
        {
            _waiting = true;
            _waitTimer = initialDelay;
        }
    }

    void Update()
    {
        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _waiting = false;

                if (_firstCycle)
                {
                    _firstCycle = false;
                    // هدف اولیه همان بالا رفتن است
                }
                else
                {
                    _movingUp = !_movingUp;
                    _targetPosition2D = _movingUp
                        ? _startPosition2D + _upDirection2D * moveDistance
                        : _startPosition2D;
                }
            }
            return;
        }

        // محاسبه موقعیت جدید دوبعدی
        Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 newPos2D = Vector2.MoveTowards(
            currentPos2D,
            _targetPosition2D,
            moveSpeed * Time.deltaTime
        );

        // ست کردن موقعیت جدید با حفظ Z
        transform.position = new Vector3(newPos2D.x, newPos2D.y, _initialZ);

        // اگر به مقصد رسیدیم، منتظر بمانیم
        if (Vector2.Distance(newPos2D, _targetPosition2D) < 0.01f)
        {
            _waiting = true;
            _waitTimer = waitTime;
        }
    }

    void OnDrawGizmos()
    {
        // جهت up را محاسبه کن (چه در حالت Play و چه در Editor)
        Vector2 upDir2D = Application.isPlaying
            ? _upDirection2D
            : new Vector2(transform.up.x, transform.up.y).normalized;

        Vector3 basePos = transform.position;
        Vector3 topPos = basePos + new Vector3(upDir2D.x, upDir2D.y, 0f) * moveDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(basePos, topPos);
        Gizmos.DrawSphere(basePos, 0.1f);
        Gizmos.DrawSphere(topPos, 0.1f);
    }
}
