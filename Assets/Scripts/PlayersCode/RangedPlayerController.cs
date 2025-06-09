using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class RangedPlayerController: PlayerControllerBase
{
    
    [Header("Light Projectile (Pooling)")]
    [SerializeField] private string lightBulletTag = "LightBullet";
    [SerializeField] private Transform lightBulbfirePoint;
    [SerializeField] private float fireprojectileSpeed = 10f;
    [SerializeField, Range(0f, 90f)] private float launchAngle = 45f;
    
    [Header("Sound Clips")]
    public AudioClip attackClip;
    [Header("Ranged Bullet")]
    [SerializeField] public Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private string bulletTag = "PlayerBullet"; 


    [Header("Stamina")] public float SpriniingCost = 5f;
    
    public bool doubleJump = true;
    private bool Is_Sprinting = false;
    private BulletPool bulletPool;
    
    // light system
    [SerializeField]protected override Color AuraColor => Color.cyan;
    [SerializeField]protected override float AuraIntensity => 0.6f;
    [SerializeField]protected override float AuraInnerRadius => 0.3f;
    [SerializeField]protected override float AuraOuterRadius => 4f;

    protected override void Start()
    {
        baseDamageMultiplier = 1f;
        base.Start();
        bulletPool = FindFirstObjectByType<BulletPool>();
        
        
    }

    public void Update()
    {
        if (Is_Sprinting && Stamina > 0 && isGrounded)
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
    
    public void OnFireLight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || bulletPool == null || firePoint == null) return;

        GameObject proj = bulletPool.GetBullet(lightBulletTag);
        if (proj == null) return;

        // قرار دادن توپ در نقطهٔ شلیک
        proj.transform.position = firePoint.position;
        proj.transform.rotation = firePoint.rotation;

        // محاسبه بردار پرتاب با زاویه
        float facing = Mathf.Sign(transform.localScale.x);              // +1 یا -1 بسته به جهت پلیر
        float angleRad = launchAngle * Mathf.Deg2Rad;                  // تبدیل به رادیان
        Vector2 launchDir = new Vector2(Mathf.Cos(angleRad) * facing, Mathf.Sin(angleRad));
        launchDir.Normalize();

        // اعمال سرعت اولیه
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = launchDir * projectileSpeed;
            // مطمئن شوید gravityScale روی prefab تنظیم شده است
        }
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