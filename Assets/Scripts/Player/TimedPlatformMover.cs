using UnityEngine;

public class TimedPlatformMover : MonoBehaviour
{
    public Transform platform;
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeed = 2f;

    private bool isMoving = false;
    private Transform currentTarget;

    private PlayerControllerBase currentPlayer;

    private void Start()
    {
        if (platform == null || startPoint == null || endPoint == null)
        {
            Debug.LogError("Platform or points not assigned!");
            enabled = false;
            return;
        }

        platform.position = startPoint.position;
        currentTarget = endPoint;
    }

    private void Update()
    {
        if (currentPlayer != null && currentPlayer.IsInteracting() && !isMoving)
        {
            isMoving = true;
        }

        if (isMoving)
        {
            platform.position = Vector3.MoveTowards(
                platform.position,
                currentTarget.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(platform.position, currentTarget.position) < 0.01f)
            {
                isMoving = false;
                currentTarget = (currentTarget == startPoint) ? endPoint : startPoint;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
        if (player != null)
        {
            currentPlayer = player;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
        if (player != null && currentPlayer == player)
        {
            currentPlayer = null;
        }
    }
}