using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;


public abstract class PlayerControllerBase : NetworkBehaviour{

    
    
    
    [Header("Light")]
    protected Light2D AuraLight;
    // این چهار property را می‌توانید در مشتقات override کنید
    [SerializeField]protected virtual Color AuraColor => Color.white;
    [SerializeField]protected virtual float AuraIntensity => 0.8f;
    [SerializeField]protected virtual float AuraInnerRadius => 0.5f;
    [SerializeField]protected virtual float AuraOuterRadius => 3f;
    
    
    [Header("Sound Clips")]
    public AudioClip jumpClip;
    public AudioClip deathClip;

    [Header("One Shot Prefab")]
    public GameObject oneShotAudioPrefab;
    [Header("Movement")] [SerializeField] protected float moveSpeed = 1.5f;
    [SerializeField] protected float jumpForce = 100f;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    protected NetworkAnimator networkAnimator;

    public bool HasKey = false;
    
    /* --- Wall-jump settings --------------------------------- */
    
    [Header("Wall Jump Settings")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private float   wallCheckDistance = 0.15f;
    [SerializeField] private LayerMask wallLayer;

    protected bool  isTouchingWall { get; private set; }
    private   bool  canWallJump    = true;
    private   float wallJumpForce  => jumpForce;   // reuse the normal jump force
/* -------------------------------------------------------- */


    [Header("Stamina")] 
    public NetworkVariable<float> Current_Stamina = new NetworkVariable<float>(50f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);    [SerializeField] protected float Stamina_gain = 5f;
    [SerializeField] protected float Stamina_max = 50;
    [SerializeField] private float staminaRegenDelay = 2f; // تاخیر پر شدن
    [SerializeField] private float staminaRegenRate = 5f;  // نرخ بازیابی در ثانیه
    private bool isRegeneratingStamina = false;
    private Coroutine regenCoroutine;

    [Header("Health")]
    public NetworkVariable<int> HealthPoint = new NetworkVariable<int>(3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    [SerializeField] protected float max_health = 100f;
    public NetworkVariable<float> current_health = new NetworkVariable<float>(100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    [SerializeField] protected float Health_gain = 5f;

    protected float baseDamageMultiplier = 1f; // مقدار اولیه هر کلاس (1f برای Ranged، 0.5f برای Melee)
    protected float currentDamageMultiplier = 1f;
    protected int reducedHitsRemaining = 0;

    //player interaction
    protected bool Interacting = false;
    private float interactBufferTime = 0.2f;
    private float interactTimer = 0f;
    public int baseDamage = 1;         // دمیج پیش‌فرض
    public int boostedDamage = 2;      // دمیج تقویت‌شده
    public int boostedHitsRemaining = 0;




    [SerializeField] protected PlayersUI playersUI;
    

    private Vector3 originalScale;

    protected SpriteRenderer Sprite;
    public bool isGrounded = true;
    protected Vector2 move_input;

    public void SetPlayerUI(PlayersUI playerUI)
    {
        this.playersUI = playerUI;
    }

    
    protected virtual void Awake()
    {
        networkAnimator = GetComponent<NetworkAnimator>();

        //برای اینکه وقتی لول ۲ بودیم light 2d درست کنه
        if (SceneManager.GetActiveScene().buildIndex == 4)
        {
            // ساخت Light2D
            AuraLight = gameObject.AddComponent<Light2D>();
            AuraLight.lightType = Light2D.LightType.Point;
            AuraLight.shadowIntensity = 0f;
            AuraLight.falloffIntensity = 1f;
        }
    }
    protected virtual void Start()
    {
        Sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalScale = Sprite.transform.localScale;
        playersUI?.SetHealthBar(current_health.Value, max_health);
        playersUI?.SetStaminaBar(Current_Stamina.Value, Stamina_max);
        currentDamageMultiplier = baseDamageMultiplier;
        
        // کانفیگ اولیه‌ی نور براساس propertyها
        if (AuraLight != null)
        {
            AuraLight.color = AuraColor;
            AuraLight.intensity = AuraIntensity;
            AuraLight.pointLightInnerRadius = AuraInnerRadius;
            AuraLight.pointLightOuterRadius = AuraOuterRadius;
        }

    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {

        if (CanApplyMovement())
        {
            rb.linearVelocity = new Vector2(move_input.x * GetMoveSpeed(), rb.linearVelocity.y);
        }
        // ── Wall detection ───────────────────────────────────────
        bool leftHit  = Physics2D.Raycast(wallCheckLeft.position,  Vector2.left,  wallCheckDistance, wallLayer);
        bool rightHit = Physics2D.Raycast(wallCheckRight.position, Vector2.right, wallCheckDistance, wallLayer);
        isTouchingWall = (leftHit || rightHit);

        if (!isGrounded && !isTouchingWall)      // reset once player leaves wall
            canWallJump = true;
    // ─────────────────────────────────────────────────────────


        if (animator != null)
        {
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetFloat("IsRunning", Mathf.Abs(move_input.x));
            }
            else
            {
                if (IsOwner)
                {
                    UpdateAnimatorFloatParameterServerRpc("IsRunning", Mathf.Abs(move_input.x));
                    
                }
            }
            
        }

        if (interactTimer > 0f)
        {
            interactTimer -= Time.deltaTime;
        }
    }

    protected virtual void PlayerMove(InputAction.CallbackContext context)
    {
        move_input = context.ReadValue<Vector2>();
        FlipDirection(move_input.x);
    }

    protected virtual void PlayerJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
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
           
            PlaySound(jumpClip);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }
    
    protected bool TryWallJump()
    {
        if (!isGrounded && isTouchingWall && canWallJump)
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

            // cancel current vertical velocity then launch upward
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * wallJumpForce, ForceMode2D.Impulse);

            canWallJump = false;   // prevent spamming
            return true;
        }
        return false;
    }


    protected virtual void FlipDirection(float horizontalInput)
    {
        if (horizontalInput > 0.01f)
        {
            Sprite.transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (horizontalInput < -0.01f)
        {
            Sprite.transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }


    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground")) return;
    
        foreach (ContactPoint2D c in collision.contacts)
        {
            // Is the surface mostly horizontal?  (normal pointing upward)
            if (c.normal.y >= 0.5f)
            {
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetBool("IsJumping", false);
                
                }
                else
                {
                    if (IsOwner)
                    {
                        UpdateAnimatorBoolParameterServerRpc("IsJumping", false);
                    }
                }
                if (!isGrounded) HandleLanding();
                isGrounded = true;
                break;                      // one good contact is enough
            }
        }
    
    }

    public virtual void HanleFalling()
    {
    }

    public abstract void Attack();

    protected virtual float GetMoveSpeed()
    {
        return moveSpeed;
    }

    protected virtual void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    protected virtual void HandleLanding()
    {
        // Nothing here in base
    } 

    protected virtual bool CanApplyMovement()
    {
        return true;
    }

    protected virtual void HealthGain()
    {

    }
    public virtual void HealthSystem(int value, bool status)
    {
        if (IsInvincible())
            return;
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            HandleHealthLocally(value, status);
        }
        else
        {
            if (IsOwner)
            {
                HandleHealthServerRpc(value, status);
            }
        }
    }

    private void HandleHealthLocally(int value, bool status)
    {
        if (IsInvincible())
        {
            return;
        }

        if (status == true)
        {
            if (current_health.Value + value > max_health)
            {
                if (HealthPoint.Value == 3)
                {
                    current_health.Value = max_health;
                }
                else if (HealthPoint.Value < 3)
                {
                    if (playersUI != null)
                    {
                        for (int i = 2; i >= 0; i--)
                        {
                            if (playersUI.hearts[i].activeSelf == false)
                            {
                                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                                {
                                    playersUI.hearts[i].gameObject.SetActive(true);
                                }
                                else
                                {
                                    UpdateHeartsClientRpc(true, i);
                                }
                                break;
                            }
                        }
                    }

                    HealthPoint.Value++;
                    current_health.Value = current_health.Value + value - max_health;
                }
            }
            else
            {
                current_health.Value += value;
            }
        }
        else

        {
            value = ModifyDamage(value);
            Debug.Log(value);
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("GetHit")) {
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    animator.SetTrigger("GetHit");
                
                }
                else
                {
                    if (IsOwner)
                    {
                        UpdateAnimatorTriggerParameterServerRpc("GetHit");
                    }
                }
            }
            

            if (current_health.Value - value <= 0)
            {
                if (HealthPoint.Value > 1)
                {
                    HealthPoint.Value--;

                    if (playersUI != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (playersUI.hearts[i].activeSelf == true)
                            {
                                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                                {
                                    playersUI.hearts[i].gameObject.SetActive(false);
                                }
                                else
                                {
                                    UpdateHeartsClientRpc(false , i);
                                }
                                break;
                            }
                        }
                    }

                    current_health.Value = max_health;
                }
                else
                {
                    SceneManager.LoadScene("Game Over");
                    current_health.Value = 0;
                    playersUI.hearts[2].SetActive(false);
                    if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                    {
                        animator.SetTrigger("IsDead");
                
                    }
                    else
                    {
                        if (IsOwner)
                        {
                            UpdateAnimatorTriggerParameterServerRpc("IsDead");
                        }
                    }
                    Invoke(nameof(OnDestory), 1f);
                }
            }
            else
            {
                current_health.Value -= value;
            }

        }

        RefreshUI();

    }

    [ServerRpc]
    private void HandleHealthServerRpc(int value, bool status)
    {
        HandleHealthLocally(value, status);
    }

    public virtual void StaminaSystem(float value, bool status)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            HandleStaminaLocally(value, status);
        }
        else
        {
            if (IsOwner)
            {
                HandleStaminaServerRpc(value, status);
            }
        }
    }

    private void HandleStaminaLocally(float value, bool status)
    {
        if (status == true)
        {
            if (Current_Stamina.Value + value > Stamina_max)
            {
                Current_Stamina.Value = Stamina_max;
            }
            else
            {
                Current_Stamina.Value += value;
            }
            Current_Stamina.Value = Mathf.Min(Current_Stamina.Value + value, Stamina_max);

        }
        else
        {
            if (Current_Stamina.Value - value <= 0)
            {
                Current_Stamina.Value = 0;
            }
            else
            {
                Current_Stamina.Value -= value;
            }
            Current_Stamina.Value = Mathf.Max(Current_Stamina.Value - value, 0f);

            if (Current_Stamina.Value <= 0 && !isRegeneratingStamina)
            {
                if (regenCoroutine != null)
                {
                    regenCoroutine = StartCoroutine(RegenerateStamina());
                    StopCoroutine(regenCoroutine);
                }
                    

               
            }
           
        }

        RefreshUI();
    }

    [ServerRpc]
    private void HandleStaminaServerRpc(float value, bool status)
    {
        HandleStaminaLocally(value, status);
    }
    protected virtual void OnDestory()
    {
        PlaySound(deathClip);
        SceneManager.LoadScene("Game Over");
        Destroy(gameObject);
    }

    public virtual void Respawn(Vector3 position)
    {
        transform.position = position;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void SetStamina(float value)
    {
        Current_Stamina.Value += value;
        Stamina_max += value;
    }
    

    public virtual void OnInteracting(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Interacting = true;
            interactTimer = interactBufferTime;
        }
    }

    public virtual bool IsInteracting()
    {
        return interactTimer > 0;

    }

    protected virtual bool IsInvincible()
    {
        return false;
    }

    protected virtual int ModifyDamage(int value)
    {
        if (reducedHitsRemaining > 0)
        {
            reducedHitsRemaining--;
            Debug.Log("Reduced damage applied!");
            return Mathf.CeilToInt(value * currentDamageMultiplier);
        }

        return Mathf.CeilToInt(value * baseDamageMultiplier);
    }
    public virtual int GetAttackDamage()
    {
        if (boostedHitsRemaining > 0)
        {
            boostedHitsRemaining--;
            if (boostedHitsRemaining == 0)
            {
                Debug.Log("Boosted melee damage expired.");
            }
            return boostedDamage;
        }

        return baseDamage;
    }

    public void IncreaseMeleeDamageTemporarily(int hitCount)
    {
        boostedHitsRemaining = hitCount;
    }
    public void ActivateDamageResetItem()
    {
        reducedHitsRemaining = 5;
        currentDamageMultiplier = baseDamageMultiplier / 2f; // یعنی نصف مقدار معمول خود پلیر
    }
    protected void PlaySound(AudioClip clip)
    {
        if (clip != null && oneShotAudioPrefab != null)
        {
            GameObject soundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            soundObj.GetComponent<OneShotSound>().Play(clip);
        }
    }

    private IEnumerator RegenerateStamina()
    {
        isRegeneratingStamina = true;
        yield return new WaitForSeconds(staminaRegenDelay);

        while (Current_Stamina.Value < Stamina_max)
        {
            Current_Stamina.Value += staminaRegenRate * Time.deltaTime;
            if (Current_Stamina.Value > Stamina_max)
                Current_Stamina.Value = Stamina_max;
            playersUI?.SetStaminaBar(Current_Stamina.Value, Stamina_max);
            yield return null;
        }

        isRegeneratingStamina = false;
    }
    public void RefreshUI()
    {
        playersUI?.SetHealthBar(current_health.Value, max_health);
        playersUI?.SetStaminaBar(Current_Stamina.Value, Stamina_max);
    }
    [ClientRpc]
    private void UpdateHeartsClientRpc(bool status , int i)
    {
        
            playersUI.hearts[i].SetActive(status);
        
    }


    [ClientRpc]
    public void SetPlayerUIClientRpc(bool isMelee)
    {
        if (isMelee)
            playersUI = GameObject.Find("MeleeUIManager  ").GetComponent<PlayersUI>();
        else
            playersUI = GameObject.Find("RangedUIManager  ").GetComponent<PlayersUI>();

        RefreshUI();
    }
    [ServerRpc(RequireOwnership = false)]

    protected void UpdateAnimatorBoolParameterServerRpc(string parameterName, bool value)
    {
        networkAnimator.Animator.SetBool(parameterName, value);
    }
    [ServerRpc(RequireOwnership = false)]

    protected void UpdateAnimatorFloatParameterServerRpc(string parameterName, float value)
    {
        networkAnimator.Animator.SetFloat(parameterName, value);
    }
    [ServerRpc(RequireOwnership = false)]

    protected void UpdateAnimatorTriggerParameterServerRpc(string parameterName)
    {
        networkAnimator.Animator.SetTrigger(parameterName);
    }
    public override void OnNetworkSpawn()
    {
        // پیدا کردن کامپوننت UI (مثلا روی Canvas یا یه GameObject خاص)
       

        // Subscribe به تغییرات NetworkVariable
        Current_Stamina.OnValueChanged += OnStaminaChanged;
        current_health.OnValueChanged += OnHealthChanged;

        // مقدار اولیه UI رو هم ست کن
        OnStaminaChanged(0, Current_Stamina.Value);
        OnHealthChanged(0, current_health.Value);

    }

    private void OnStaminaChanged(float previous, float current)
    {
        if (playersUI != null)
            playersUI.SetStaminaBar(current, Stamina_max);
    }

    private void OnHealthChanged(float previous, float current)
    {
        
        if (playersUI != null)
            playersUI.SetHealthBar(current, max_health);
    }


}