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

    private Vector2 _targetPosition;
    private bool _waiting = false;
    private float _waitTimer = 0f;
    private bool _goingToB = true;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("MovingPlatform: Please assign both Point A and Point B in the Inspector.");
            enabled = false;
            return;
        }

        // Initially, move toward point B
        _targetPosition = pointB.position;
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
                // Swap direction
                _goingToB = !_goingToB;
                _targetPosition = _goingToB ? pointB.position : pointA.position;
            }
            return;
        }

        // Move toward the current target
        Vector2 currentPos = transform.position;
        Vector2 newPos = Vector2.MoveTowards(currentPos, _targetPosition, moveSpeed * Time.deltaTime);
        transform.position = newPos;

        // Check if we've reached the target (within a small tolerance)
        if (Vector2.Distance(newPos, _targetPosition) < 0.01f)
        {
            // Start waiting
            _waiting = true;
            _waitTimer = waitTime;
        }
    }

    // Draw gizmos in the Scene view to visualize the two points and a line between them
    void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.1f);
            Gizmos.DrawSphere(pointB.position, 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}
