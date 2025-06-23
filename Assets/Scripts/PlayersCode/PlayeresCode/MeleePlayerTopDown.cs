using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class MeleePlayerTopDown : TopDownController
{
    [Header("Sound Clips")]
    public AudioClip attackClip;
    
    
    [Header("Attack Settings")]
    //متغیر ها برای تعریف حمله کردن
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 1;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void Start()
    {
        baseDamageMultiplier = 0.5f;
        base.Start();

    }
    
    protected override void FixedUpdate()
    {
        if (CanApplyMovement())
        {
            transform.Translate(move_input * GetMoveSpeed() * Time.fixedDeltaTime);
        }

        if (animator != null)
        {
            animator.SetFloat("IsRunning", move_input.magnitude);
        }
    }

    
    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("MOVE INPUT TRIGGERED!");
        move_input = context.ReadValue<Vector2>();
        Debug.Log("INPUT VALUE: " + move_input);
        FlipDirection(move_input.x);
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
    
    public void OnAttack()
    {
        animator.SetBool("IsAttacking", true);
        StartCoroutine(ResetAttackBool());
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

    protected override int ModifyDamage(int value)
    {
        return Mathf.CeilToInt(value / 2f); // نصف دمیج (گرد به بالا)
    }

}