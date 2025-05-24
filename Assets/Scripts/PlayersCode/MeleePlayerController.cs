using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.InputSystem;

public class MeleePlayerController : PlayerControllerBase
{
  //متغیر ها برای dash زدن  
    bool isDashing = false;
    private float addforceSync = 1f;
    [SerializeField] private float DashValue = 100;
    private bool canDash = true;
    [SerializeField] private float dashCooldown = 1f; 
    
    //متغیر ها برای تعریف حمله کردن
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 1;
    
    
    [Header("Stamina")]
    [SerializeField] private float dashCost = 25;
    
    
    
    public void OnMove(InputAction.CallbackContext context)
    {   
        PlayerMove(context);
    }
    

    public override void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            InterfaceEnemies enemy = enemyCollider.GetComponent<InterfaceEnemies>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage, this.transform);
            }
        }

    }

    
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isGrounded)
            {
                animator.SetBool("IsJumping", true);
                PlayerJump(context); // استفاده از AddForce در پرش اول اوکیه
            }
        }
    }

    public void OnAttack()
    {
        animator.SetBool("IsAttacking", true);
        StartCoroutine(ResetAttackBool());
    }
    
    
    
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && !isDashing && Stamina >= dashCost)
        {
            StaminaSystem(Mathf.RoundToInt(dashCost), false);
            isDashing = true;
            canDash = false;
            animator.SetTrigger("IsDashing");
            StartCoroutine(DelayedDashForce());
        }
    }


    protected override bool CanApplyMovement()
    {
        return !isDashing;
    }

    
    private IEnumerator DelayedDashForce()
    {
        float delay = 0.165f;
        yield return new WaitForSeconds(delay);

        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 dashForce = new Vector2(DashValue * direction, 0);
        rb.AddForce(dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.2f); // dash duration

        isDashing = false;

        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            animator.SetBool("IsFalling", true);
        }

        // ⏲ Start cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
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
}