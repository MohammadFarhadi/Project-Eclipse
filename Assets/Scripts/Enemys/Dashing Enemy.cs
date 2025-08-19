using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class DashingEnemy : NetworkBehaviour, InterfaceEnemies
{
    [SerializeField] private GameObject[] dropItems; // Prefabs of Health/Stamina/Other pickups
    [SerializeField] private GameObject Sonin;
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    [Header("General Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 1f;
    [SerializeField] private float idleDuration = 2f;
    [SerializeField] private float jumpForce = 4f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 9;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public int HealthPoint => Mathf.RoundToInt(currentHealth.Value);

    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ParticleSystem dashEffect; // 🎇 پارتیکل دش
    [SerializeField] private Transform graphicsTransform; // 🔁 برای flip کردن ظاهر

    private bool isDashing = false;
    private bool isIdle = true;
    private bool isGrounded = true;
    private int currentDirection = -1; // 1: Right, -1: Left

    private void Start()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                currentHealth.Value = maxHealth;
            }
        }
        else
        {
            currentHealth.Value = maxHealth;
        }
        
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
        Invoke(nameof(StartNextAttack), idleDuration);
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(currentDirection * dashSpeed, rb.linearVelocity.y);
        }
    }

    private void StartNextAttack()
    {
        if (currentHealth.Value < 4)
        {
            int mode = Random.Range(0, 2);
            if (mode == 0)
                StartGroundDash();
            else
                StartAirDash();
        }
        else
        {
            StartGroundDash();
        }
    }

    private void StartGroundDash()
    {
        isIdle = false;
        isDashing = true;

        if (dashEffect != null)
        {
            dashEffect.Play(); // 🎇 اجرای پارتیکل افکت
        }

        Invoke(nameof(EndDash), dashDuration);
    }

    private void StartAirDash()
    {
        isIdle = false;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        Invoke(nameof(StartGroundDash), 0.3f);
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            //animator.SetTrigger("Idle"); // فقط انیمیشن Idle

        currentDirection *= -1;
        FlipGraphics(); // فقط بعد از دش flip می‌شه
        Invoke(nameof(StartNextAttack), idleDuration);
    }

    private void FlipGraphics()
    {
        if (graphicsTransform != null)
        {
            Vector3 scale = graphicsTransform.localScale;
            scale.x *= -1;
            graphicsTransform.localScale = scale;
        }
    }

    

    public void TakeDamage(int damage, Transform attacker)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                currentHealth.Value -= damage;
            }
            else
            {
                ApplyDamageServerRpc(damage);
            }
        }
        else
        {
            currentHealth.Value -= damage;
        }

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
        DropRandomItem();
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                DestroyObjectClientRpc();
                Destroy(gameObject , 0.5f);
            }
            
        }
        else
        {
            Destroy(gameObject , 0.5f);
        }
    }

    public void DetectPlayer(GameObject p) { }
    public void LosePlayer() { }
    public void DropRandomItem()
    {
        if (dropItems.Length == 0) return;

        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (!IsServer) return;

            int index = Random.Range(0, dropItems.Length);
            Vector3 spawnPosition = transform.position + new Vector3(0f, 1f, 0f);
            GameObject dropped = Instantiate(dropItems[index], spawnPosition, Quaternion.identity);
            if (dropped.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn();
            }

            GameObject soninDrop = Instantiate(Sonin, transform.position, Quaternion.identity);
            if (soninDrop.TryGetComponent(out NetworkObject soninNet))
            {
                soninNet.Spawn();
            }
        }
        else
        {
            int index = Random.Range(0, dropItems.Length);
            Instantiate(dropItems[index], transform.position + Vector3.up, Quaternion.identity);
            Instantiate(Sonin, transform.position, Quaternion.identity);
        }
    }
    [ClientRpc]
    private void DestroyObjectClientRpc()
    {
        Destroy(gameObject , 0.5f);
    }
    [ServerRpc(RequireOwnership = false)]
    void ApplyDamageServerRpc(int damageAmount)
    {
        currentHealth.Value -= damageAmount;
    }
    [ServerRpc(RequireOwnership = false)]
    protected void UpdateAnimatorBoolParameterServerRpc( string parameterName, bool value)
    {
        networkAnimator.Animator.SetBool(parameterName, value);
    }
    [ServerRpc(RequireOwnership = false)]

    protected void UpdateAnimatorFloatParameterServerRpc( string parameterName, float value)
    {
        networkAnimator.Animator.SetFloat(parameterName, value);
    }
    [ServerRpc(RequireOwnership = false)]

    protected void UpdateAnimatorTriggerParameterServerRpc( string parameterName)
    {
        networkAnimator.Animator.SetTrigger(parameterName);
    }
}
