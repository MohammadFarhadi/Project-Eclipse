using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTriggerBridge : MonoBehaviour
{
    private PlayerControllerBase currentPlayer;
    
    private bool canTrigger = false;
    public GameObject bridgeObject; // پل را از Inspector وصل کن
    private BoxCollider2D bridgeCollider;

    private void Start()
    {
        if (bridgeObject != null)
        {
            bridgeCollider = bridgeObject.GetComponentInChildren<BoxCollider2D>();
        }
    }

    void Update()
    {
        

        if (currentPlayer != null)
        {        
            Debug.Log(canTrigger + " " + currentPlayer.IsInteracting());
            
            if (canTrigger && currentPlayer.IsInteracting())
            {
                Debug.Log("salammmmmmmmm");
                    Animator animator = bridgeObject.GetComponentInChildren<Animator>();
                    animator.SetBool("MaskReveal", true);
                    // 1. غیر فعال کردن isTrigger
                    bridgeCollider.isTrigger = false;
                    // فقط یک بار اجرا بشه
                    canTrigger = false;
                    
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