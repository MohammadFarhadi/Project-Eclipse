using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class JumpingEnemy : NetworkBehaviour, InterfaceEnemies
{
    [SerializeField] private GameObject[] dropItems; // Prefabs of Health/Stamina/Other pickups
    [SerializeField] private GameObject Sonin;
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float horizontalForce = 3f;

    [Header("Health")]
    [SerializeField] private int maxHealthPoints = 3;
    private NetworkVariable<int> currentHealthPoints = new NetworkVariable<int>(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public int HealthPoint => Mathf.RoundToInt(currentHealthPoints.Value);

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    [SerializeField] private Rigidbody2D rb;

    private GameObject player;
    private bool isGrounded = true;
    private bool isJumpingAtPlayer = false;

    private void Start()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                currentHealthPoints.Value = maxHealthPoints;
            }
        }
        else
        {
            currentHealthPoints.Value = maxHealthPoints;
        }
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
       
    }

    private void Update()
    {
        if (player != null && isGrounded && !isJumpingAtPlayer)
        {
            JumpAtPlayer();
        }
    }

    private void JumpAtPlayer()
    {
        if (player == null || rb == null) return;

        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
        Vector2 jumpVector = new Vector2(direction * horizontalForce, jumpForce);

        rb.linearVelocity = jumpVector;
        isGrounded = false;
        isJumpingAtPlayer = true;
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            UpdateAnimatorTriggerParameterServerRpc("Jump");
        }
        else
        {
            if (animator != null)
                animator.SetTrigger("Jump");

        }
        
        transform.localScale = new Vector3(Mathf.Sign(direction), 1, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = true;
            isJumpingAtPlayer = false;
        }
    }


    // ✅ این روش با استفاده از یک Trigger بیرونی کار می‌کند
    public void DetectPlayer(GameObject playerObject)
    {
        player = playerObject;
    }

    public void LosePlayer()
    {
        player = null;
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                currentHealthPoints.Value -= damage;
            }
            else
            {
                ApplyDamageServerRpc(damage);
            }
        }
        else
        {
            currentHealthPoints.Value -= damage;
        }

        if (currentHealthPoints.Value <= 0)
        {
            Die();
        }
        else
        {
            if (GameModeManager.Instance.CurrentMode == GameMode.Online)
            {
                UpdateAnimatorTriggerParameterServerRpc("GetHit");
            }
            else
            {
                if (animator != null)
                    animator.SetTrigger("GetHit");
            }
           
        }
    }

    private void Die()
    {
        if (animator != null)
        {
            GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
            if (GameModeManager.Instance.CurrentMode == GameMode.Online)
            {
                UpdateAnimatorTriggerParameterServerRpc("IsDead");
            }
            else
            {
                animator.SetTrigger("IsDead");

            }
            
        }
            
        DropRandomItem();
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                DestroyObjectClientRpc();
                Destroy(gameObject);
            }
            
        }
        else
        {
            Destroy(gameObject);
        }
    }
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
        Destroy(gameObject);
    }
    [ServerRpc(RequireOwnership = false)]
    void ApplyDamageServerRpc(int damageAmount)
    {
        currentHealthPoints.Value -= damageAmount;
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
