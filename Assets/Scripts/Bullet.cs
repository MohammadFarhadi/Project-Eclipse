using Mono.Cecil;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private Transform attacker;
    public int damage = 1;
    [Header("Pool Config")]
    public string bulletTag; // تگ این گلوله در pool

    private BulletPoolNGO pool;
    private void Awake()
    {
        // پیدا کردن pool در صحنه (یا میتوان dependency injection کرد)
        pool = FindObjectOfType<BulletPoolNGO>();
    }
    public void SetAttacker(Transform attacker)
    {
        this.attacker = attacker;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Hello");
        Debug.Log(other.gameObject.name);
        if (GameModeManager.Instance.CurrentMode == GameMode.Online && !IsServer) return;

        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<InterfaceEnemies>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, attacker);
            }

            HandleBulletEnd();
        }
        else if (other.CompareTag("Player"))
        {
            Debug.Log("Hello");
            
            var targetPlayer = other.GetComponent<PlayerControllerBase>();
            if (targetPlayer == null) Debug.Log("targetPlayer is null");
            if (targetPlayer != null)
            {
                if (GameModeManager.Instance.CurrentMode == GameMode.Online && IsServer)
                {
                    targetPlayer.TakeDamageFromServer(30, false);
                }
                else
                {
                    targetPlayer.HealthSystem(30, false);
                }

                Debug.Log($"{targetPlayer.name} got hit by bullet from {attacker?.name}");
            }

            var targertPlayerTopDown = other.GetComponent<TopDownController>();
            if (targertPlayerTopDown == null)Debug.Log("targertPlayerTopDown is null");
            if (targertPlayerTopDown != null)
            {
                if (GameModeManager.Instance.CurrentMode == GameMode.Online && IsServer)
                {
                   // targertPlayerTopDown.TakeDamageFromServer(30, false);
                }
                else
                {
                    targertPlayerTopDown.HealthSystem(30, false);
                }

                Debug.Log($"{targertPlayerTopDown.name} got hit by bullet from {attacker?.name}");
            }

            HandleBulletEnd();
        }
        else if (other.CompareTag("Ground"))
        {
            HandleBulletEnd();
        }
    }

    private void OnEnable()
    {
        Invoke(nameof(HandleBulletEnd), 3f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void HandleBulletEnd()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (IsServer)
            {
                pool.ReturnBullet(bulletTag, gameObject);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            // چون هنوز Instantiate شده ولی هنوز اکتیو نیست
            gameObject.SetActive(false);
        }
    }
}