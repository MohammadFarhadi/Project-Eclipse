using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RangedPlayerTopDown : TopDownController
{
    [Header("Sound Clips")]
    public AudioClip attackClip;

    [Header("Ranged Bullet")]
    [SerializeField] public Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private string bulletTag = "PlayerBullet";
    

    [Header("Stamina")]
    public float SpriniingCost = 5f;
    
    private BulletPool bulletPool;

    
    
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        baseDamageMultiplier = 1f;
        base.Start();
        bulletPool = FindFirstObjectByType<BulletPool>();
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
    

    public void OnMove(InputAction.CallbackContext context)
    {
        PlayerMove(context);
    }



    public void OnAttack(InputAction.CallbackContext context)
    {
        animator.SetBool("IsShooting", true);
    }

}
