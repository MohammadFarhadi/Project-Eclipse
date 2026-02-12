using UnityEngine;

public class SpikePlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 2f;
    public float moveSpeed = 3f;
    public float waitTime = 2f;
    public float initialDelay = 15f;

    private Vector2 _startPosition2D;
    private Vector2 _upDirection2D;
    private Vector2 _targetPosition2D;
    private bool _movingUp = true;
    private bool _waiting = false;
    private float _waitTimer = 0f;
    private float _initialZ;
    private bool _firstCycle = true;

    void Start()
    {
        Vector3 startPos3D = transform.localPosition;
        _startPosition2D = new Vector2(startPos3D.x, startPos3D.y);
        _initialZ = startPos3D.z;

        _upDirection2D = new Vector2(transform.up.x, transform.up.y).normalized;

        _movingUp = true;
        _targetPosition2D = _startPosition2D + _upDirection2D * moveDistance;

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
                    // ادامه حرکت به سمت بالا
                }
                else
                {
                    _movingUp = !_movingUp;
                    _targetPosition2D = _startPosition2D +
                        (_movingUp ? _upDirection2D : -_upDirection2D) * moveDistance;
                }
            }
            return;
        }

        Vector2 currentPos2D = new Vector2(transform.localPosition.x, transform.localPosition.y);
        Vector2 newPos2D = Vector2.MoveTowards(
            currentPos2D,
            _targetPosition2D,
            moveSpeed * Time.deltaTime
        );

        transform.localPosition = new Vector3(newPos2D.x, newPos2D.y, _initialZ);

        if (Vector2.Distance(newPos2D, _targetPosition2D) < 0.01f)
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

        Vector3 basePos = transform.localPosition;
        Vector3 topPos = basePos + new Vector3(upDir2D.x, upDir2D.y, 0f) * moveDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(basePos, topPos);
        Gizmos.DrawSphere(basePos, 0.1f);
        Gizmos.DrawSphere(topPos, 0.1f);
    }
}
