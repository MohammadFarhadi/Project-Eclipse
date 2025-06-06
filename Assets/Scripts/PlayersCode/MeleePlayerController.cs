using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.InputSystem;

public class MeleePlayerController : PlayerControllerBase
{
    [Header("Sound Clips")]
    public AudioClip attackClip;
  //متغیر ها برای dash زدن  
    bool isDashing = false;
    private float addforceSync = 1f;
    [SerializeField] private float DashValue = 80f;
    
    //متغیر ها برای تعریف حمله کردن
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 1;
    
    
    protected override void Start()
    {
        baseDamageMultiplier = 0.5f;
        base.Start();
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
        if (context.performed && !isDashing)
        {
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
    


}