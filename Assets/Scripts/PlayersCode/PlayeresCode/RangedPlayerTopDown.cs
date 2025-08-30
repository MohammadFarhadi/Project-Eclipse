using System;
using Unity.Netcode;
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
    private BulletPoolNGO bulletPoolNGO;

    
    
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        baseDamageMultiplier = 1f;
        base.Start();
        bulletPool = FindFirstObjectByType<BulletPool>();
        bulletPoolNGO = FindObjectOfType<BulletPoolNGO>();
    }

    public void Update()
    {
        if (playersUI == null)
        {
            playersUI = GameObject.Find("RangedUIManager  ").GetComponent<PlayersUI>();
            RefreshUI();
        }
    }

    protected override void FixedUpdate()
    {
        if (CanApplyMovement())
        {
            transform.Translate(move_input * GetMoveSpeed() * Time.fixedDeltaTime);
        }

        if (animator != null)
        {
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetFloat("IsRunning",  move_input.magnitude);
            }
            else
            {
                if (IsOwner)
                {
                    UpdateAnimatorFloatParameterServerRpc("IsRunning", move_input.magnitude);
                    
                }
            }
        }
    }
    public override void Attack()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            // مستقیم تیر را لوکال شلیک کن
            ShootBullet();
        }
        else
        {
            Debug.Log("Shooting Rpc ");
            // در حالت آنلاین، تیر را با ServerRpc شلیک کن
            if (IsOwner) ShootBullet();
        }
    }
    public void ShootBullet()
    {
        PlaySound(attackClip);
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            if (firePoint && bulletPool != null)
            {
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
        else
        {
            if (IsOwner)
            {
                Debug.Log("Shooting in pool ");
                ShootBulletServerRpc();
            }
        }
        
    }
    [ServerRpc(RequireOwnership = false)]
    public void ShootBulletServerRpc()
    {
        Debug.Log("We Are Here Bro");

        GameObject proj = bulletPoolNGO.GetBullet(bulletTag);
        Debug.Log(proj);
        if (proj != null)
        {
            // در سرور گلوله رو در جای درست بذار
            proj.transform.position = firePoint.position;
            proj.transform.rotation = firePoint.rotation;

            NetworkObject netObj = proj.GetComponent<NetworkObject>();
            Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();

            Vector2 bulletVelocity = Vector2.zero;
            float bulletScaleX = 1f;

            if (rbProj != null)
            {
                Vector3 bulletScale = rbProj.transform.localScale;
                bulletScale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(bulletScale.x);
                bulletVelocity = new Vector2(transform.localScale.x * projectileSpeed, 0f);
                rbProj.linearVelocity = bulletVelocity;
                rbProj.transform.localScale = bulletScale;

                Bullet bulletScript = proj.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetAttacker(this.transform);
                    bulletScript.damage = GetAttackDamage();
                }

                bulletScaleX = bulletScale.x;
            }

            // کلاینت‌ها رو هم sync کن
            InitBulletClientRpc(
                netObj.NetworkObjectId,
                firePoint.position,
                bulletVelocity,
                bulletScaleX,
                this.GetComponent<NetworkObject>().NetworkObjectId,
                GetAttackDamage()
            );
        }

        // بعد از شلیک، به همه کلاینت‌ها بگو انیمیشن قطع شه
        UpdateAnimatorBoolParameterServerRpc("IsShooting", false);
    }


    [ClientRpc]
    void InitBulletClientRpc(ulong bulletNetId, Vector3 spawnPosition, Vector2 velocity, float scaleX, ulong attackerNetId, int damage)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(bulletNetId, out NetworkObject spawnedBullet))
        {
            spawnedBullet.transform.position = spawnPosition;

            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = velocity;

                Vector3 scale = spawnedBullet.transform.localScale;
                scale.x = scaleX;
                spawnedBullet.transform.localScale = scale;
            }

            Bullet bulletScript = spawnedBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                Transform attacker = null;
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(attackerNetId, out NetworkObject attackerObj))
                {
                    attacker = attackerObj.transform;
                }
                bulletScript.SetAttacker(attacker);
                bulletScript.damage = damage;
            }
        }
    }


    

    public void OnMove(InputAction.CallbackContext context)
    {
        PlayerMove(context);
    }



    public void OnAttack(InputAction.CallbackContext context)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            animator.SetBool("IsShooting", true);
                
        }
        else
        {
            if (IsOwner)
            {
                UpdateAnimatorBoolParameterServerRpc("IsShooting", true);
            }
        }
    }

}
