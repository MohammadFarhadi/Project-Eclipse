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
    public float initialDelay = 15f;

    private Vector2 _startLocalPos2D;
    private Vector2 _targetLocalPos2D;
    private Vector2 _upDirection2D;
    private bool _movingUp = true;
    private bool _waiting = false;
    private float _waitTimer = 0f;
    private bool _firstCycle = true;
    private float _initialZ;

    void Start()
    {
        // ذخیره localPosition اولیه
        Vector3 startLocalPos3D = transform.localPosition;
        _startLocalPos2D = new Vector2(startLocalPos3D.x, startLocalPos3D.y);
        _initialZ = startLocalPos3D.z;

        // محاسبه جهت up محلی
        _upDirection2D = new Vector2(transform.up.x, transform.up.y).normalized;

        // هدف اولیه حرکت به بالا است
        _movingUp = true;
        _targetLocalPos2D = _startLocalPos2D + _upDirection2D * moveDistance;

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
                    _targetLocalPos2D = _movingUp
                        ? _startLocalPos2D + _upDirection2D * moveDistance
                        : _startLocalPos2D;
                }
            }
            return;
        }

        // محاسبه موقعیت جدید دوبعدی (لوکال)
        Vector2 currentLocalPos2D = new Vector2(transform.localPosition.x, transform.localPosition.y);
        Vector2 newLocalPos2D = Vector2.MoveTowards(
            currentLocalPos2D,
            _targetLocalPos2D,
            moveSpeed * Time.deltaTime
        );

        // ست کردن لوکال پوزیشن
        transform.localPosition = new Vector3(newLocalPos2D.x, newLocalPos2D.y, _initialZ);

        // اگر به مقصد رسیدیم، منتظر بمانیم
        if (Vector2.Distance(newLocalPos2D, _targetLocalPos2D) < 0.01f)
        {
            _waiting = true;
            _waitTimer = waitTime;
        }
    }

    void OnDrawGizmos()
    {
        Vector2 upDir2D = Application.isPlaying
            ? _upDirection2D
            : new Vector2(transform.up.x, transform.up.y).normalized;

        Vector3 basePos = Application.isPlaying
            ? transform.parent != null
                ? transform.parent.TransformPoint(new Vector3(_startLocalPos2D.x, _startLocalPos2D.y, _initialZ))
                : new Vector3(_startLocalPos2D.x, _startLocalPos2D.y, _initialZ)
            : transform.position;

        Vector3 topPos = basePos + new Vector3(upDir2D.x, upDir2D.y, 0f) * moveDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(basePos, topPos);
        Gizmos.DrawSphere(basePos, 0.1f);
        Gizmos.DrawSphere(topPos, 0.1f);
    }
}
