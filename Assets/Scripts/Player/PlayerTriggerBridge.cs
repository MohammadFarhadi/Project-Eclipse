using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTriggerBridge : MonoBehaviour
{
    private bool canTrigger = false;
    public GameObject bridgeObject; // پل را از Inspector وصل کن
    private Rigidbody2D bridgeRb;
    private BoxCollider2D bridgeCollider;

    private void Start()
    {
        if (bridgeObject != null)
        {
            bridgeRb = bridgeObject.GetComponent<Rigidbody2D>();
            bridgeCollider = bridgeObject.GetComponent<BoxCollider2D>();
        }
    }

    void Update()
    {
        if (canTrigger && Keyboard.current.xKey.wasPressedThisFrame)
        {
            if (bridgeRb != null && bridgeCollider != null)
            {
                // 1. غیر فعال کردن isTrigger
                bridgeCollider.isTrigger = false;

                // 2. فعال‌سازی جاذبه
                bridgeRb.gravityScale = 1f;

                // فقط یک بار اجرا بشه
                canTrigger = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("TriggerZone"))
        {
            canTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("TriggerZone"))
        {
            canTrigger = false;
        }
    }
}