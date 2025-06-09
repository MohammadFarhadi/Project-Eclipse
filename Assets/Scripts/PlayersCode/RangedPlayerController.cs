using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class RangedPlayerController: PlayerControllerBase
{
    [Header("Sound Clips")]
    public AudioClip attackClip;
    [Header("Ranged Bullet")]
    [SerializeField] public Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private string bulletTag = "PlayerBullet"; 


    [Header("Stamina")] public float SpriniingCost = 5f;
    
    public bool doubleJump = true;
    [Header("Sprinting Settings")]
    public float sprintingCostPerSecond = 5f; // مقدار مصرف در ثانیه
    private bool isSprinting = false;
    private Coroutine sprintCoroutine;        // ✅ آیا در حال دویدن هست؟

    private BulletPool bulletPool;

    protected override void Start()
    {
        baseDamageMultiplier = 1f;
        base.Start();
        bulletPool = FindFirstObjectByType<BulletPool>();
        
        
    }

    public void Update()
    {
        
        if (isSprinting  && isGrounded && Current_Stamina > 0)
        {
            StaminaSystem(sprintingCostPerSecond * Time.deltaTime, false);
            SetMoveSpeed(3);
            Debug.Log(moveSpeed);
            animator.SetBool("IsSprinting", true); 

        }
        else
        {
            if (Current_Stamina < Stamina_max)
            {
                StaminaSystem(sprintingCostPerSecond * Time.deltaTime, true);
            }
            SetMoveSpeed(1.5f);
            animator.SetBool("IsSprinting", false);
        }
    }
    
    public override void Attack()
    {
        if (firePoint && bulletPool != null)
        {
            PlaySound(attackClip);
            GameObject proj = bulletPool.GetBullet(bulletTag);
            if (proj != null)
            {
                proj.transform.position = firePoint.position;
                proj.transform.rotation = firePoint.rotation;

                Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
                if (rbProj != null)
                {
                    Vector3 bulletScale = rbProj.transform.localScale;
                    bulletScale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(bulletScale.x);
                    rbProj.linearVelocity = new Vector2(transform.localScale.x * projectileSpeed, 0f);
                    rbProj.transform.localScale = bulletScale;
                    // از اینجا 
                    Bullet bulletScript = proj.GetComponent<Bullet>();
                    if (bulletScript != null)
                    {
                        bulletScript.SetAttacker(this.transform);
                        int currentDamage = GetAttackDamage();
                        bulletScript.damage = currentDamage;

                    }
                    // تا اینجا اضافه شده
                }
            } 
            animator.SetBool("IsShooting", false);
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        PlayerMove(context); 
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isGrounded)
        {
            PlayerJump(context);
        }
        else if (TryWallJump())       // ⬅️ first priority: wall-jump
        {
            // nothing more; handled inside base
        }
        else
        {
            DoubleJump();             // ⬅️ otherwise do regular double-jump
        }
    }



    void DoubleJump()
    {
        if (!isGrounded && doubleJump)
        {
            PlaySound(jumpClip);
            animator.SetBool("IsJumping", true);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // پاک کردن سرعت عمودی فعلی
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // تعیین سرعت پرش دوم
            doubleJump = false;
        }
    }



    public void OnAttack(InputAction.CallbackContext context)
    {
        animator.SetBool("IsShooting", true);
    }


    public void Sprinting(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
        
    }
    protected override void HandleLanding()
    {
        doubleJump = true;
    }
    
   
    

}