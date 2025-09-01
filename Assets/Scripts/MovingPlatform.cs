using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum MoveDirection { Vertical, Horizontal }

    [Header("Movement Settings")]
    public MoveDirection moveDirection = MoveDirection.Vertical; // انتخاب جهت حرکت
    public float moveDistance = 3f;
    public float moveDuration = 2.5f;
    public float waitTime = 1f;
    public float startDelay = 0f;

    private float _startDelayTimer;
    private bool _startedMoving = false;

    private float _moveTimer = 0f;
    private bool _movingForward = true;
    private bool _waiting = false;
    private float _waitTimer = 0f;

    private Vector3 _initialLocalPos;

    void Start()
    {
        _startDelayTimer = startDelay;
        _initialLocalPos = transform.localPosition; // ذخیره موقعیت اولیه
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
                _movingForward = !_movingForward;
                _moveTimer = 0f;
            }
            return;
        }

        _moveTimer += Time.deltaTime;

        float t = Mathf.Clamp01(_moveTimer / moveDuration);
        float offset = Mathf.Lerp(
            _movingForward ? 0f : moveDistance,
            _movingForward ? moveDistance : 0f,
            t
        );

        // تغییر موقعیت بر اساس جهت انتخاب‌شده
        if (moveDirection == MoveDirection.Vertical)
        {
            transform.localPosition = new Vector3(
                _initialLocalPos.x,
                _initialLocalPos.y + offset,
                _initialLocalPos.z
            );
        }
        else if (moveDirection == MoveDirection.Horizontal)
        {
            transform.localPosition = new Vector3(
                _initialLocalPos.x + offset,
                _initialLocalPos.y,
                _initialLocalPos.z
            );
        }

        if (t >= 1f)
        {
            _waiting = true;
            _waitTimer = waitTime;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos;

        if (moveDirection == MoveDirection.Vertical)
            endPos += new Vector3(0, moveDistance, 0);
        else if (moveDirection == MoveDirection.Horizontal)
            endPos += new Vector3(moveDistance, 0, 0);

        Gizmos.DrawSphere(startPos, 0.1f);
        Gizmos.DrawSphere(endPos, 0.1f);
        Gizmos.DrawLine(startPos, endPos);
    }
}
