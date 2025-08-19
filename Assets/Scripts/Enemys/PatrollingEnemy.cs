using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
public class PatrollingEnemy : NetworkBehaviour , InterfaceEnemies
{
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip deathClip;
    public GameObject oneShotAudioPrefab;

    public float speed = 2f;
    private int direction = -1;
    public GameObject leftSensor;
    public GameObject rightSensor;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    public int HealthPoint => Mathf.RoundToInt(health.Value);

    private NetworkVariable<int> health = new NetworkVariable<int>(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // 👇 اضافه کن:
    public EnemyHealthBarDisplay healthBarDisplay;
    [Header("Possible Drops")]
    [SerializeField] private GameObject[] dropItems; // Prefabs of Health/Stamina/Other pickups
    [SerializeField] private GameObject Sonin;
    
    void Start()
    {
        health.OnValueChanged += OnHealthChanged;

        if (healthBarDisplay != null)
        {
            healthBarDisplay.UpdateHealthBar(health.Value);
        }
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }
    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (healthBarDisplay != null)
        {
            healthBarDisplay.Show(newValue);
            healthBarDisplay.UpdateHealthBar(newValue);
        }
    }

    private void OnDestroy()
    {
        health.OnValueChanged -= OnHealthChanged;
    }
    void Update()
    {
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("TurnPoint"))
        {
            direction *= -1;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            GameObject attackSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            attackSoundObj.GetComponent<OneShotSound>().Play(attackClip);
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetTrigger("Attack");

            }
            else
            {
                UpdateAnimatorTriggerParameterServerRpc("Attack");
            }
            PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
            player.HealthSystem(50, false);
            Debug.Log("Player hited ");
        }
    }

    public void TakeDamage(int damage , Transform attacker)
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                health.Value -= damage;
            }
            else
            {
                ApplyDamageServerRpc(damage);
            }
        }
        else
        {
            health.Value -= damage;
        }

        // 👇 بروز رسانی نوار سلامتی
        if (healthBarDisplay != null)
        {
            healthBarDisplay.Show(health.Value);
            healthBarDisplay.UpdateHealthBar(health.Value);
        }
        


        if (health.Value <= 0 && direction == 1)
        {
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetTrigger("Die");
 
            }
            else
            {
                UpdateAnimatorTriggerParameterServerRpc("Die");
            }
            
            Invoke(nameof(Die), 0.5f);
        }
        else if (health.Value <= 0 && direction == -1)
        {
            
           
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                animator.SetTrigger("Die1");
 
            }
            else
            {
                UpdateAnimatorTriggerParameterServerRpc("Die1");
            }
            Invoke(nameof(Die), 0.5f);
        }

    }

    public void Die()
    {
        GameObject deathSoundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
        deathSoundObj.GetComponent<OneShotSound>().Play(deathClip);
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
        health.Value -= damageAmount;
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