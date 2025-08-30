using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 3f;
    public float moveDuration = 2.5f;
    public float waitTime = 1f;
    public float startDelay = 15f;

    private float _startDelayTimer;
    private bool _startedMoving = false;

    private float _moveTimer = 0f;
    private bool _movingUp = true;
    private bool _waiting = false;
    private float _waitTimer = 0f;

    private float _initialLocalX;
    private float _initialLocalY;
    private float _initialLocalZ;

    void Start()
    {
        _startDelayTimer = startDelay;

        // ذخیره لوکال پوزیشن
        _initialLocalX = transform.localPosition.x;
        _initialLocalY = transform.localPosition.y;
        _initialLocalZ = transform.localPosition.z;
    }

    void Update()
    {
        if (!_startedMoving)
        {
            _startDelayTimer -= Time.deltaTime;
            if (_startDelayTimer <= 0f)
            {
                _startedMoving = true;
                _moveTimer = 0f;
            }
            return;
        }

        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _waiting = false;
                _movingUp = !_movingUp;
                _moveTimer = 0f;
            }
            return;
        }

        _moveTimer += Time.deltaTime;

        float t = Mathf.Clamp01(_moveTimer / moveDuration);
        float offsetY = Mathf.Lerp(
            _movingUp ? 0f : moveDistance,
            _movingUp ? moveDistance : 0f,
            t
        );

        // استفاده از لوکال پوزیشن
        transform.localPosition = new Vector3(
            _initialLocalX,
            _initialLocalY + offsetY,
            _initialLocalZ
        );

        if (t >= 1f)
        {
            _waiting = true;
            _waitTimer = waitTime;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 bottomPos = transform.position;
        Vector3 topPos = bottomPos + new Vector3(0, moveDistance, 0);

        Gizmos.DrawSphere(bottomPos, 0.1f);
        Gizmos.DrawSphere(topPos, 0.1f);
        Gizmos.DrawLine(bottomPos, topPos);
    }
}
