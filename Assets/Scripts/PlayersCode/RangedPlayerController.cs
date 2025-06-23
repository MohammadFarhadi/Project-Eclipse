using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RangedPlayerController : PlayerControllerBase
{
    [Header("Sound Clips")]
    public AudioClip attackClip;

    [Header("Ranged Bullet")]
    [SerializeField] public Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private string bulletTag = "PlayerBullet";

    // ← ADDED: Light‐bullet settings
    [Header("Light Bullet (Pooling)")]
    [SerializeField] private Transform lightBulletFirePoint;   // assign in inspector
    [SerializeField] private string  lightBulletTag         = "LightBullet";
    [SerializeField] private float   lightBulletSpeed       = 8f;
    [SerializeField, Range(0,90)] private float launchAngle  = 45f;

    [Header("Stamina")]
    public float SpriniingCost = 5f;

    public float lightThrowCost = 8f;

    public bool doubleJump = true;
    [Header("Sprinting Settings")]
    public float sprintingCostPerSecond = 5f;
    private bool isSprinting = false;

    private BulletPool bulletPool;

    protected override void Start()
    {
        baseDamageMultiplier = 1f;
        base.Start();
        bulletPool = FindFirstObjectByType<BulletPool>();
    }

    public void Update()
    {
        // … your sprint logic unchanged …
        if (isSprinting  && isGrounded && Current_Stamina > 0)
        {
            StaminaSystem(sprintingCostPerSecond * Time.deltaTime, false);
            SetMoveSpeed(3);
            animator.SetBool("IsSprinting", true);
        }
        else
        {
            if (Current_Stamina < Stamina_max)
                StaminaSystem(sprintingCostPerSecond * Time.deltaTime, true);
            SetMoveSpeed(1.5f);
            animator.SetBool("IsSprinting", false);
        }
    }

    public override void Attack()
    {
        // ← THIS REMAINS YOUR NORMAL BULLET CODE
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

                    Bullet bulletScript = proj.GetComponent<Bullet>();
                    if (bulletScript != null)
                    {
                        bulletScript.SetAttacker(this.transform);
                        bulletScript.damage = GetAttackDamage();
                    }
                }
            }
            animator.SetBool("IsShooting", false);
        }
    }

    // ← ADDED: new Input callback to throw a light‐emitting projectile
    public void OnFireLight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || bulletPool == null || lightBulletFirePoint == null || SceneManager.GetActiveScene().buildIndex != 4) 
            return;

        // pull from pool
        GameObject lightProj = bulletPool.GetBullet(lightBulletTag);
        if (lightProj == null) return;
        
        if (Current_Stamina < lightThrowCost) return;                  // need enough stamina
        StaminaSystem(lightThrowCost, false);                          // subtract cost

        // position & rotation
        lightProj.transform.position = lightBulletFirePoint.position;
        lightProj.transform.rotation = Quaternion.identity;

        // compute launch direction at angle
        float dirSign = Mathf.Sign(transform.localScale.x);
        float angleRad = launchAngle * Mathf.Deg2Rad;
        Vector2 launchDir = new Vector2(Mathf.Cos(angleRad) * dirSign, Mathf.Sin(angleRad)).normalized;

        // apply velocity
        var rb2d = lightProj.GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.linearVelocity = launchDir * lightBulletSpeed;

        // set damage and attacker
        var bulletScript = lightProj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetAttacker(this.transform);
            bulletScript.damage = GetAttackDamage();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        PlayerMove(context);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (isGrounded) PlayerJump(context);
        else if (TryWallJump()) { }
        else DoubleJump();
    }

    void DoubleJump()
    {
        if (!isGrounded && doubleJump)
        {
            PlaySound(jumpClip);
            animator.SetBool("IsJumping", true);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            doubleJump = false;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        animator.SetBool("IsShooting", true);
    }

    public void Sprinting(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
    }

    protected override void HandleLanding()
    {
        doubleJump = true;
    }
}
