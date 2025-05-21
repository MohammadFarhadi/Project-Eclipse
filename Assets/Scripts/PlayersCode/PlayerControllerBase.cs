using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerControllerBase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 1.5f;
    [SerializeField] protected float jumpForce = 100f;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    
    [Header("Stamina")]
    public  float Stamina = 50;
    public float Stamina_gain = 5f;
    public float Stamina_loss = 10f;
    public float Stamina_max = 50;
    
    [Header("Health")]
    public float Health = 30;
    public float Health_gain = 5f;
    
    private Vector3 originalScale;

    protected SpriteRenderer Sprite;
    protected bool isGrounded = true;
    protected Vector2 move_input;
    protected virtual void Start()
    {
        Sprite  = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalScale = Sprite.transform.localScale;
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        if (CanApplyMovement())
        {
            rb.linearVelocity = new Vector2(move_input.x * GetMoveSpeed(), rb.linearVelocity.y);
        }

        if (animator != null)
        {
            animator.SetFloat("IsRunning", Mathf.Abs(move_input.x));
        }
    }

    protected virtual void PlayerMove(InputAction.CallbackContext context)
    {
        move_input = context.ReadValue<Vector2>();
        FlipDirection(move_input.x);
    }

    protected virtual void PlayerJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            animator.SetBool("IsJumping", true);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }
    protected virtual void FlipDirection(float horizontalInput)
    {
        if (horizontalInput > 0.01f)
        {
            Sprite.transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (horizontalInput < -0.01f)
        {
            Sprite.transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            animator?.SetBool("IsJumping", false);
            HandleLanding();
            isGrounded = true;
            HandleLanding(); 
        }
    }

    public virtual void HanleFalling()
    {
    }

    public abstract void Attack();
    
    protected virtual float GetMoveSpeed()
    {
        return moveSpeed;
    }

    protected virtual void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    protected virtual void HandleLanding()
    {
        // Nothing here in base
    }
    protected virtual bool CanApplyMovement()
    {
        return true;
    }

    protected virtual void HealthGain()
    {
        
    }
}
