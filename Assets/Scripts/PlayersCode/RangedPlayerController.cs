using System;
using Unity.Netcode;
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
    private BulletPoolNGO bulletPoolNGO;

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
        // … your sprint logic unchanged …
        if (isSprinting  && isGrounded && Current_Stamina.Value > 0)
        {
            StaminaSystem(sprintingCostPerSecond * Time.deltaTime, false);
            SetMoveSpeed(3);
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetBool("IsSprinting", true);
                
            }
            else
            {
                if (IsOwner)
                {
                    UpdateAnimatorBoolParameterServerRpc("IsSprinting", true);
                }
            }
            
        }
        else
        {
            if (Current_Stamina.Value < Stamina_max)
                StaminaSystem(sprintingCostPerSecond * Time.deltaTime, true);
            SetMoveSpeed(1.5f);
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetBool("IsSprinting", false);
                
            }
            else
            {
                if (IsOwner)
                {
                    UpdateAnimatorBoolParameterServerRpc("IsSprinting", false);
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

    public void OnFireLight(InputAction.CallbackContext ctx)
    {
        Debug.Log("ON FIRE LIGHT");
        if (!ctx.performed)
        {
            Debug.Log("Wo go To return !!");
            return;
            
        }
            

        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            ShootLightBullet();
        }
        else
        {
            if (IsOwner)
            {
                Debug.Log("Shooting Light Bullet ...");
                ShootLightBulletServerRpc();
            }
            else
            {
                Debug.Log(("It's Not owner"));
            }
        }
    }

    // ← ADDED: new Input callback to throw a light‐emitting projectile
    public void ShootLightBullet()
    {
        GameObject lightProj;
        // pull from pool
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
             lightProj = bulletPool.GetBullet(lightBulletTag);
             if (lightProj == null) return;
        
             if (Current_Stamina.Value < lightThrowCost) return;                  // need enough stamina
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

        else
        {
            if (IsOwner)
            {
                Debug.Log("Shooting in pool ");
                ShootLightBulletServerRpc();
            }
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
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetBool("IsJumping", true);
                
            }
            else
            {
                if (IsOwner)
                {
                    UpdateAnimatorBoolParameterServerRpc("IsJumping", true);
                }
            }
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            doubleJump = false;
        }
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

    public void Sprinting(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
    }

    protected override void HandleLanding()
    {
        doubleJump = true;
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

    [ServerRpc(RequireOwnership = false)]
    public void ShootLightBulletServerRpc()
    {
        Debug.Log("We Are Here Again...");
        GameObject lightProj;

        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            lightProj = bulletPoolNGO.GetBullet(lightBulletTag);
            if (lightProj == null) return;

            if (Current_Stamina.Value < lightThrowCost) return;
            StaminaSystem(lightThrowCost, false);

            // position & rotation
            lightProj.transform.position = lightBulletFirePoint.position;
            lightProj.transform.rotation = Quaternion.identity;

            // launch direction
            float dirSign = Mathf.Sign(transform.localScale.x);
            float angleRad = launchAngle * Mathf.Deg2Rad;
            Vector2 launchDir = new Vector2(Mathf.Cos(angleRad) * dirSign, Mathf.Sin(angleRad)).normalized;
            Vector2 velocity = launchDir * lightBulletSpeed;

            // apply velocity
            Rigidbody2D rb2d = lightProj.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.linearVelocity = velocity;

                // set proper scale based on direction
                Vector3 scale = rb2d.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * dirSign;
                rb2d.transform.localScale = scale;
            }

            // set attacker & damage
            Bullet bulletScript = lightProj.GetComponent<Bullet>();
            int dmg = GetAttackDamage();
            if (bulletScript != null)
            {
                bulletScript.SetAttacker(this.transform);
                bulletScript.damage = dmg;
            }

            // get NetObj
            NetworkObject netObj = lightProj.GetComponent<NetworkObject>();
            float scaleX = lightProj.transform.localScale.x;

            // Send info to all clients
            InitLightBulletClientRpc(
                netObj.NetworkObjectId,
                lightBulletFirePoint.position,
                velocity,
                scaleX,
                this.GetComponent<NetworkObject>().NetworkObjectId,
                dmg
            );
        }
    }
    [ClientRpc]
    void InitLightBulletClientRpc(ulong bulletNetId, Vector3 spawnPosition, Vector2 velocity, float scaleX, ulong attackerNetId, int damage)
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




   

    
 
}