using UnityEngine;
using Unity.Netcode;

public class PlayerTriggerBridge : NetworkBehaviour
{
    private PlayerControllerBase currentPlayer;

    private bool canTrigger = false;
    public GameObject bridgeObject; // پل را از Inspector وصل کن
    private BoxCollider2D bridgeCollider;

    [Header("Movement Settings")]
    public float moveDistance = 3f;      // چقدر حرکت کنه
    public float moveSpeed = 2f;         // سرعت حرکت

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool moving = false;
    private bool movingUp = true;

    private void Start()
    {
        if (bridgeObject != null)
        {
            bridgeCollider = bridgeObject.GetComponentInChildren<BoxCollider2D>();
            startPos = bridgeObject.transform.position;
            targetPos = startPos + Vector3.up * moveDistance;
        }
    }

    private void Update()
    {
        if (currentPlayer != null)
        {
            if (canTrigger && currentPlayer.IsInteracting())
            {
                Debug.Log("Bridge interaction triggered!");

                TriggerBridge();
            }
        }

        if (moving)
        {
            Vector3 destination = movingUp ? startPos + Vector3.up * moveDistance : startPos;
            bridgeObject.transform.position = Vector3.MoveTowards(
                bridgeObject.transform.position,
                destination,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(bridgeObject.transform.position, destination) < 0.01f)
            {
                bridgeObject.transform.position = destination;
                moving = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.GetComponent<PlayerControllerBase>();
            canTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canTrigger = false;
        }
    }

    /// <summary>
    /// تصمیم می‌گیرد پل لوکال فعال شود یا به‌صورت آنلاین سینک شود
    /// </summary>
    public void TriggerBridge()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            TriggerBridgeServerRpc();
        }
        else
        {
            PlayBridgeAnimationLocal();
        }
    }

    #region Local Mode

    private void PlayBridgeAnimationLocal()
    {
        movingUp = !movingUp;
        moving = true;

        bridgeCollider.isTrigger = false;
        canTrigger = false;

        Animator animator = bridgeObject.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetBool("MaskReveal", true);
        }
    }

    #endregion

    #region Online Mode

    [ServerRpc(RequireOwnership = false)]
    private void TriggerBridgeServerRpc()
    {
        movingUp = !movingUp;
        moving = true;

        bridgeCollider.isTrigger = false;
        canTrigger = false;

        PlayBridgeAnimationClientRpc();
    }

    [ClientRpc]
    private void PlayBridgeAnimationClientRpc()
    {
        bridgeCollider.isTrigger = false;
        Animator animator = bridgeObject.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetBool("MaskReveal", true);
        }
    }

    #endregion
}
