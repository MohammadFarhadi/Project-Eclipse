using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class MeleePlayerController : PlayerControllerBase
{
    [Header("Sound Clips")]
    public AudioClip attackClip;
    
    
    [Header("Dash Settings")]
  //متغیر ها برای dash زدن  
    bool isDashing = false;
    private float addforceSync = 1f;
    [SerializeField] private float DashValue = 80f;
    
    [Header("Attack Settings")]
    //متغیر ها برای تعریف حمله کردن
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 1;
    
    
    [Header("AuraLight")]
    // light system
    [SerializeField]protected override Color AuraColor => new Color(1f, 0.9f, 0.6f);
    [SerializeField]protected override float AuraIntensity => 0.8f;
    [SerializeField]protected override float AuraInnerRadius => 0.5f;
    [SerializeField]protected override float AuraOuterRadius => 5f;
    
    [Header("Flashlight Settings")]
    [SerializeField] private float staminaDrainRate     = 5f;
    [SerializeField] private float flashIntensityOn    = 1.2f;
    [SerializeField] private float flashInnerRadius    = 0.2f;
    [SerializeField] private float flashOuterRadius    = 6f;
    [SerializeField, Range(1, 179)] private float flashInnerAngle = 30f;
    [SerializeField, Range(1, 179)] private float flashOuterAngle = 60f;
    [SerializeField] private Vector3  flashlightOffset  = new Vector3(0.5f, 0f, 0f);
    private float    rotationOffset; 
    private Transform spotlightT;

    
    private Light2D spotlight;
    private bool    isFlashOn;

    
    protected override void Awake()
    {
        base.Awake();

        var go = new GameObject("Spotlight");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = flashlightOffset;

        // rotate the empty so its Y‐axis (the light’s “forward”) now points along +X
        float initialZ = -90f;
        go.transform.localRotation = Quaternion.Euler(0,0, initialZ);
        rotationOffset = initialZ;
        spotlightT = go.transform;

        spotlight = go.AddComponent<Light2D>();
        spotlight.lightType             = Light2D.LightType.Point;
        spotlight.pointLightInnerRadius = flashInnerRadius;
        spotlight.pointLightOuterRadius = flashOuterRadius;
        spotlight.pointLightInnerAngle  = flashInnerAngle;
        spotlight.pointLightOuterAngle  = flashOuterAngle;
        spotlight.intensity             = 0f;
        spotlight.color                 = Color.white;
        spotlight.shadowIntensity       = 0f;
        spotlight.falloffIntensity      = 1f;
    }
    protected override void Start()
    {
        baseDamageMultiplier = 0.5f;
        base.Start();

        if (SceneManager.GetActiveScene().buildIndex == 4)
        {
            // شعاع‌ها و زاویه‌ها
            spotlight.pointLightInnerRadius = flashInnerRadius;
            spotlight.pointLightOuterRadius = flashOuterRadius;
            spotlight.pointLightInnerAngle  = flashInnerAngle;
            spotlight.pointLightOuterAngle  = flashOuterAngle;
        }
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();      // حرکت و physics پایه
        HandleFlashlight();
    }
    void Update()
    {
        // Flip light cone when player flips
        float facing = Mathf.Sign(transform.localScale.x);
        spotlightT.localScale = new Vector3(-1f, 0f, 0f);

        // Toggle on/off and drain/recover stamina
        HandleFlashlight();
    }


    public void OnMove(InputAction.CallbackContext context)
    {   
        PlayerMove(context);
    }
    

    public override void Attack()
    {
        PlaySound(attackClip);
        Debug.Log("Attacking...");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        Debug.Log("Enemies hit: " + hitEnemies.Length);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Debug.Log("Hit enemy: " + enemyCollider.name);
            InterfaceEnemies enemy = enemyCollider.GetComponent<InterfaceEnemies>();
            if (enemy != null)
            {
                int currentDamage = GetAttackDamage();
                enemy.TakeDamage(currentDamage , this.transform);
            }
        }
    }


    
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isGrounded)
        {
            animator.SetBool("IsJumping", true);
            PlayerJump(context);
        }
        else
        {
            TryWallJump();   // ⬅️ single call does the trick
        }
    }


    public void OnAttack()
    {
        animator.SetBool("IsAttacking", true);
        StartCoroutine(ResetAttackBool());
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing && Current_Stamina >= 15f)
        {
            StaminaSystem(15f, false);
            isDashing = true;
            animator.SetTrigger("IsDashing");
            StartCoroutine(DelayedDashForce());
            addforceSync = 1f;
        }
    }

    protected override bool CanApplyMovement()
    {
        return !isDashing;
    }

    
    private IEnumerator DelayedDashForce()
    {
        float delay = 0.165f; // time before dash force applies
        yield return new WaitForSeconds(delay);

        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 dashForce = new Vector2(DashValue * direction, 0);
        rb.AddForce(dashForce, ForceMode2D.Impulse);
    
        // dash force applied
        yield return new WaitForSeconds(0.2f); // dash duration

        isDashing = false;

        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            animator.SetBool("IsFalling", true);
        }
    }

    // برای اینه که بیاد هیت باکس رو نشون بده داخل بازی بهمون
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    // زمانیکه طول می کشه که انیمیشن اتک انجام بشه
    private IEnumerator ResetAttackBool()
    {
        //چقدر طول میکشه که انیمیشن اتکمون اجرا بشه.
        yield return new WaitForSeconds(0.2f); 
        animator.SetBool("IsAttacking", false);
    }
    
    protected override void HandleLanding()
    {
        animator.SetBool("IsFalling", false);
    }
    protected override bool IsInvincible()
    {
        return isDashing;
    }
    protected override int ModifyDamage(int value)
    {
        return Mathf.CeilToInt(value / 2f); // نصف دمیج (گرد به بالا)
    }

    private void HandleFlashlight()
    {
        if (isFlashOn && Current_Stamina > 0f)
        {
            spotlight.intensity = flashIntensityOn;
            StaminaSystem(staminaDrainRate * Time.deltaTime, false);

            // اگر بعد از کم شدن استامینا رسید صفر، خودکار خاموشش کن
            if (Current_Stamina <= 0f)
                isFlashOn = false;
        }
        else
        {
            spotlight.intensity = 0f;
            if (Current_Stamina < Stamina_max)
            {
                StaminaSystem(Stamina_gain * Time.deltaTime, true);
            }
        }
    }


    public void OnFlash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // فقط اگر استامینا بیشتر از مصرف لحظه‌ای (مثلاً staminaDrainRate) داشت مجوز بده
        if (Current_Stamina > staminaDrainRate * Time.deltaTime)
            isFlashOn = !isFlashOn;
    }




}