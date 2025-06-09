using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public abstract class PlayerControllerBase : MonoBehaviour
{
    [Header("Sound Clips")]
    public AudioClip jumpClip;
    public AudioClip deathClip;

    [Header("One Shot Prefab")]
    public GameObject oneShotAudioPrefab;
    [Header("Movement")] [SerializeField] protected float moveSpeed = 1.5f;
    [SerializeField] protected float jumpForce = 100f;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
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
    [SerializeField] protected float Current_Stamina = 50;
    [SerializeField] protected float Stamina_gain = 5f;
    [SerializeField] protected float Stamina_max = 50;
    [SerializeField] private float staminaRegenDelay = 2f; // تاخیر پر شدن
    [SerializeField] private float staminaRegenRate = 5f;  // نرخ بازیابی در ثانیه
    private bool isRegeneratingStamina = false;
    private Coroutine regenCoroutine;


    [Header("Health")] private int HealthPoint = 3;
    [SerializeField] protected float max_health = 100f;
    [SerializeField] protected float current_health = 100;
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



    [SerializeField] private PlayersUI playersUI;

    private Vector3 originalScale;

    protected SpriteRenderer Sprite;
    public bool isGrounded = true;
    protected Vector2 move_input;

    protected virtual void Start()
    {
        Sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalScale = Sprite.transform.localScale;
        playersUI?.SetHealthBar(current_health, max_health);
        playersUI?.SetStaminaBar(Current_Stamina, Stamina_max);
        currentDamageMultiplier = baseDamageMultiplier;

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
            animator.SetFloat("IsRunning", Mathf.Abs(move_input.x));
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
            animator.SetBool("IsJumping", true);
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
            animator.SetBool("IsJumping", true);

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
                animator?.SetBool("IsJumping", false);
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
        {
            return;
        }

        
        if (status == true)
        {
            if (current_health + value > max_health)
            {
                if (HealthPoint == 3)
                {
                    current_health = max_health;
                }
                else if (HealthPoint < 3)
                {
                    if (playersUI != null)
                    {
                        for (int i = 2; i >= 0; i--)
                        {
                            if (playersUI.hearts[i].activeSelf == false)
                            {
                                playersUI.hearts[i].gameObject.SetActive(true);
                                break;
                            }
                        }
                    }

                    HealthPoint++;
                    current_health = current_health + value - max_health;
                }
            }
            else
            {
                current_health += value;
            }
        }
        else

        {
            value = ModifyDamage(value);
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("GetHit")) {
                animator.SetTrigger("GetHit");
            }
            

            if (current_health - value <= 0)
            {
                if (HealthPoint > 1)
                {
                    HealthPoint--;

                    if (playersUI != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (playersUI.hearts[i].activeSelf == true)
                            {
                                playersUI.hearts[i].SetActive(false);
                                break;
                            }
                        }
                    }

                    current_health = max_health;
                }
                else
                {
                    
                    current_health = 0;
                    playersUI.hearts[2].SetActive(false);
                    animator.SetTrigger("IsDead");
                    Invoke(nameof(OnDestory), 1f);
                }
            }
            else
            {
                current_health -= value;
            }

        }

        playersUI?.SetHealthBar(current_health, max_health);
    }

    public virtual void StaminaSystem(float value, bool status)
    {
        if (status == true)
        {
            if (Current_Stamina + value > Stamina_max)
            {
                Current_Stamina = Stamina_max;
            }
            else
            {
                Current_Stamina += value;
            }
            Current_Stamina = Mathf.Min(Current_Stamina + value, Stamina_max);

        }
        else
        {
            if (Current_Stamina - value <= 0)
            {
                Current_Stamina = 0;
            }
            else
            {
                Current_Stamina -= value;
            }
            Current_Stamina = Mathf.Max(Current_Stamina - value, 0f);

            if (Current_Stamina <= 0 && !isRegeneratingStamina)
            {
                if (regenCoroutine != null)
                    StopCoroutine(regenCoroutine);

                regenCoroutine = StartCoroutine(RegenerateStamina());
            }
        }
        playersUI?.SetStaminaBar(Current_Stamina, Stamina_max);
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

        while (Current_Stamina < Stamina_max)
        {
            Current_Stamina += staminaRegenRate * Time.deltaTime;
            if (Current_Stamina > Stamina_max)
                Current_Stamina = Stamina_max;
            playersUI?.SetStaminaBar(Current_Stamina, Stamina_max);
            yield return null;
        }

        isRegeneratingStamina = false;
    }


    
}