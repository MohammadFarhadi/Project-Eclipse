using UnityEngine;
using Unity.Netcode;

public class TimedPlatformMover : NetworkBehaviour
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
        if (currentPlayer != null && currentPlayer.IsInteracting())
        {
            RequestMovePlatform(currentPlayer);
        }

        if (ShouldMoveLocally())
        {
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
    }

    public void RequestMovePlatform(PlayerControllerBase player)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                StartPlatformMovement();
            }
            else
            {
                RequestMovePlatformServerRpc(player.OwnerClientId);
            }
        }
        else
        {
            StartPlatformMovement();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestMovePlatformServerRpc(ulong playerId)
    {
        // اختیاری → میتونی اینجا Validation انجام بدی
        StartPlatformMovement();

        StartPlatformMovementClientRpc();
    }

    [ClientRpc]
    private void StartPlatformMovementClientRpc()
    {
        if (!IsServer)
        {
            StartPlatformMovement();
        }
    }

    private void StartPlatformMovement()
    {
        isMoving = true;
    }

    private bool ShouldMoveLocally()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            return true;

        if (GameModeManager.Instance.CurrentMode == GameMode.Online && IsServer)
            return true;

        return false;
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
        if (player != null && player == currentPlayer)
        {
            currentPlayer = null;
        }
    }
}
