using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class RangedPlayerController: PlayerControllerBase
{
    [Header("Ranged Bullet")]
    [SerializeField] public GameObject projectilePrefab;
    [SerializeField] public Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    
    public bool doubleJump = true;
    private bool Is_Sprinting = false;
    private BulletPool bulletPool;



    private void Awake()
    {
        Debug.Log("salam");
        Debug.Log($"Player {gameObject.name} is using: {GetComponent<PlayerInput>().currentControlScheme}");
    }

    public  void  Start()
    {
        base.Start();
        bulletPool = FindFirstObjectByType<BulletPool>();
        Debug.Log($"{gameObject.name} Rigidbody assigned: {rb != null}");
        Debug.Log($"{gameObject.name} Animator assigned: {animator != null}");
    }

    public void Update()
    {
        if (Is_Sprinting && Stamina > 0)
        {
            SetMoveSpeed(3);
            Debug.Log(moveSpeed);
            Stamina -=  Stamina_loss * Time.deltaTime;
            animator.SetBool("IsSprinting", true);
        }
        else
        {
            SetMoveSpeed(1.5f);
            Debug.Log(moveSpeed);
            animator.SetBool("IsSprinting", false);
            if (Stamina < Stamina_max)
            {
                Stamina +=  Stamina_gain * Time.deltaTime;
            }
        }
    }
    
    public override void Attack()
    {
        animator.SetBool("IsShooting", false);
        Debug.Log((bool)firePoint);
        Debug.Log((bool)bulletPool);
        if (firePoint && bulletPool != null)
        {
            GameObject proj = bulletPool.GetBullet("PlayerBullet");
            Debug.Log(proj);
            if (proj != null)
            {
                proj.transform.position = firePoint.position;
                proj.transform.rotation = firePoint.rotation;

                Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
                if (rbProj != null)
                {
                    rbProj.linearVelocity = new Vector2(transform.localScale.x * projectileSpeed, 0f);
                }
            }
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log($"[Ranged] OnMove triggered | Phase: {context.phase}, Value: {context.ReadValue<Vector2>()}");
        PlayerMove(context); 
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isGrounded)
            {
                PlayerJump(context); // استفاده از AddForce در پرش اول اوکیه
            }
            else
            {
                DoubleJump(); // استفاده از velocity در پرش دوم
            }
        }
    }


    void DoubleJump()
    {
        if (!isGrounded && doubleJump)
        {
            animator.SetBool("IsJumping", true);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // پاک کردن سرعت عمودی فعلی
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // تعیین سرعت پرش دوم
            doubleJump = false;
        }
    }



    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log($"RangedPlayer Move input: {move_input}");
        animator.SetBool("IsShooting", true);
    }


    public void Sprinting(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Is_Sprinting = true;
        }
        else
        {
            Is_Sprinting = false;
        }
        
    }
    protected override void HandleLanding()
    {
        doubleJump = true;
    }
}
