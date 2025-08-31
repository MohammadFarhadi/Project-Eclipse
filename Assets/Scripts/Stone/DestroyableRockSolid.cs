using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DestroyableRockSolid : NetworkBehaviour, InterfaceEnemies
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private NetworkVariable<int> health = new NetworkVariable<int>(
        3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );
    public int HealthPoint => health.Value;

    [Header("UI")]
    public EnemyHealthBarDisplay healthBarDisplay;

    [Header("Destruction")]
    [SerializeField] private float destroyDelay = 1.0f;
    [SerializeField] private GameObject destroyedVFX;

    [Header("Sound")]
    [Tooltip("Prefab with AudioSource + OneShotSound (same as player uses).")]
    public GameObject oneShotAudioPrefab;
    [Tooltip("Sound clip to play when destroyed.")]
    public AudioClip destroyClip;

    private bool dying;

    void Awake() => health.Value = Mathf.Clamp(maxHealth, 0, maxHealth);

    void OnEnable()  { health.OnValueChanged += OnHealthChanged; }
    void OnDisable() { health.OnValueChanged -= OnHealthChanged; }

    void Start()
    {
        if (healthBarDisplay != null)
        {
            healthBarDisplay.Show(health.Value);
            healthBarDisplay.UpdateHealthBar(health.Value);
        }
    }

    private void OnHealthChanged(int oldVal, int newVal)
    {
        if (healthBarDisplay != null)
        {
            healthBarDisplay.Show(newVal);
            healthBarDisplay.UpdateHealthBar(newVal);
        }

        if (newVal <= 0 && !dying)
        {
            dying = true;
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        // play sound instantly
        PlaySound(destroyClip);

        yield return new WaitForSeconds(destroyDelay);

        if (destroyedVFX) Instantiate(destroyedVFX, transform.position, Quaternion.identity);

        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            Destroy(gameObject);
        }
        else if (IsServer)
        {
            DestroyClientRpc();
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void DestroyClientRpc()
    {
        if (this && gameObject) Destroy(gameObject);
    }

    // === InterfaceEnemies ===
    public void TakeDamage(int damage, Transform attacker)
    {
        if (dying) return;
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer) health.Value = Mathf.Max(health.Value - damage, 0);
            else ApplyDamageServerRpc(damage);
        }
        else
        {
            health.Value = Mathf.Max(health.Value - damage, 0);
        }
    }

    public void SetHealth(int hp)
    {
        hp = Mathf.Clamp(hp, 0, maxHealth);
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer) health.Value = hp;
            else SetHealthServerRpc(hp);
        }
        else health.Value = hp;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyDamageServerRpc(int dmg) => health.Value = Mathf.Max(health.Value - dmg, 0);

    [ServerRpc(RequireOwnership = false)]
    private void SetHealthServerRpc(int hp) => health.Value = Mathf.Clamp(hp, 0, maxHealth);

    // === Sound helper (same as PlayerControllerBase.PlaySound) ===
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && oneShotAudioPrefab != null)
        {
            GameObject soundObj = Instantiate(oneShotAudioPrefab, transform.position, Quaternion.identity);
            soundObj.GetComponent<OneShotSound>().Play(clip);
        }
    }
}
