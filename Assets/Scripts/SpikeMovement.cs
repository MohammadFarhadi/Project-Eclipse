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

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private Vector3 _upDirection;
    private bool _movingUp = true;
    private bool _waiting = false;
    private float _waitTimer = 0f;
    private bool _firstCycle = true;

    void Start()
    {
        // Cache the starting world‐position
        _startPosition = transform.position;

        // Capture the initial local "up" direction in world space
        _upDirection = transform.up.normalized;

        // We want to move "up" first (along the local up)
        _movingUp = true;
        _targetPosition = _startPosition + _upDirection * moveDistance;

        // If an initial delay is specified, begin in "waiting" mode for that delay
        if (initialDelay > 0f)
        {
            _waiting = true;
            _waitTimer = initialDelay;
        }
    }

    void Update()
    {
        // If currently waiting (either the initial delay or a pause at an endpoint)
        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _waiting = false;

                // If this was the very first wait (the initialDelay), just start moving up:
                if (_firstCycle)
                {
                    _firstCycle = false;
                    // _movingUp is already true, and _targetPosition is set to "up" point.
                    // So no toggle here—just let it move toward "_targetPosition".
                }
                else
                {
                    // After reaching an endpoint, toggle direction for the next move
                    _movingUp = !_movingUp;
                    _targetPosition = _movingUp
                        ? _startPosition + _upDirection * moveDistance
                        : _startPosition;
                }
            }
            return;
        }

        // Move toward the current target (either up or back down)
        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Once close enough to the target, begin waiting for 'waitTime'
        if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
        {
            _waiting = true;
            _waitTimer = waitTime;
        }
    }

    // Visualize the trap's travel path in the Scene view
    void OnDrawGizmos()
    {
        // In Edit mode, _upDirection isn't cached yet, so use transform.up
        Vector3 upDir = Application.isPlaying
            ? _upDirection
            : transform.up.normalized;

        Vector3 basePos = transform.position;
        Vector3 topPos = basePos + upDir * moveDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(basePos, topPos);
        Gizmos.DrawSphere(basePos, 0.1f);
        Gizmos.DrawSphere(topPos, 0.1f);
    }
}
