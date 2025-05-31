using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTriggerBridge : MonoBehaviour
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

    void Update()
    {
        if (currentPlayer != null)
        {
            if (canTrigger && currentPlayer.IsInteracting())
            {
                Debug.Log("salammmmmmmmm");

                // تغییر جهت حرکت
                movingUp = !movingUp;
                moving = true;

                // فقط بار اول collider رو فعال کن
                bridgeCollider.isTrigger = false;
                canTrigger = false;

                // اگر انیمیشن هم هست
                Animator animator = bridgeObject.GetComponentInChildren<Animator>();
                if (animator != null)
                    animator.SetBool("MaskReveal", true);
            }
        }

        if (moving)
        {
            Vector3 destination = movingUp ? startPos + Vector3.up * moveDistance : startPos;
            bridgeObject.transform.position = Vector3.MoveTowards(bridgeObject.transform.position, destination, moveSpeed * Time.deltaTime);

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
}
