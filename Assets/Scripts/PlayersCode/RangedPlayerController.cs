using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class RangedPlayerController: PlayerControllerBase
{
    [Header("Ranged Bullet")]
    [SerializeField] public GameObject projectilePrefab;
    [SerializeField] public Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Stamina")] public float SpriniingCost = 5f;
    
    public bool doubleJump = true;
    private bool Is_Sprinting = false;
    private BulletPool bulletPool;

    protected override void Start()
    {
        base.Start();
        bulletPool = FindFirstObjectByType<BulletPool>();
        
        Debug.Log($"[TEST] Rigidbody موجوده؟ {rb != null}");
        Debug.Log($"[TEST] Rigidbody simulated: {rb.simulated}");
        Debug.Log($"[TEST] BodyType: {rb.bodyType}");
    }

    public void Update()
    {
        if (Is_Sprinting && Stamina > 0)
        {
            SetMoveSpeed(3);
            Debug.Log(moveSpeed);
            StaminaSystem(Mathf.RoundToInt(SpriniingCost * Time.deltaTime), false);
            animator.SetBool("IsSprinting", true);
        }
        else
        {
            SetMoveSpeed(1.5f);
            animator.SetBool("IsSprinting", false);
            if (Stamina < Stamina_max)
            {
                StaminaSystem(Mathf.RoundToInt(Stamina_gain * Time.deltaTime), true);
            }
        }
    }
    
    public override void Attack()
    {
        animator.SetBool("IsShooting", false);
        if (firePoint && bulletPool != null)
        {
            GameObject proj = bulletPool.GetBullet("PlayerBullet1");
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
                    }
                    // تا اینجا اضافه شده
                }
            }
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
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