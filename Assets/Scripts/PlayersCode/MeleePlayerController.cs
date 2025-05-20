using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.InputSystem;

public class MeleePlayerController : PlayerControllerBase
{
    public bool doubleJump = true;
    bool isDashing = false;
    private float addforceSync = 1f;
    [SerializeField] private float DashValue = 100;
    public void OnMove(InputAction.CallbackContext context)
    {   
        PlayerMove(context);
    }
    

    public override void Attack()
    {
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
        float delay = 0.165f; // how long to wait before force
        yield return new WaitForSeconds(delay);

        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 dashForce = new Vector2(DashValue * direction, 0);
        Debug.Log($"[Dash] Applying force: {dashForce}");
        rb.AddForce(dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.2f); // total dash duration
        isDashing = false;
    }

}