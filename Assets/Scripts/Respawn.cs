using UnityEngine;
using Unity.Netcode;

public class Respawn : NetworkBehaviour
{
    public GameObject respawnPoint;
    bool flag = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerControllerBase>();

        if (player != null)
        {
            if (!flag)
            {
                // اگر لوکال بازی می‌کنیم، مستقیم لوکال تغییر بده
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    player.HealthSystem(100, false);
                }
                else
                {
                    // اگر آنلاین هست، درخواست بده سرور دمیج بزنه
                    var netObj = player.GetComponent<NetworkObject>();
                    if (netObj != null)
                    {
                        DamagePlayerServerRpc(netObj.NetworkObjectId, 100);
                    }
                }

                flag = true;
            }

            // اگر هنوز نمرده باشه، Respawn کن
            if (player.gameObject.activeSelf)
            {
                if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                {
                    StartCoroutine(DelayedRespawn(player));
                }
                else
                {
                    var netObj = player.GetComponent<NetworkObject>();
                    if (netObj != null)
                    {
                        RespawnPlayerServerRpc(netObj.NetworkObjectId);
                    }
                }
            }
        }
    }

    #region Local Mode

    private System.Collections.IEnumerator DelayedRespawn(PlayerControllerBase player)
    {
        yield return new WaitForSeconds(0.5f);

        if (player != null)
        {
            player.Respawn(respawnPoint.transform.position);
            flag = false;
        }
    }

    #endregion

    #region Online Mode

    [ServerRpc(RequireOwnership = false)]
    private void DamagePlayerServerRpc(ulong playerNetworkId, int damageAmount)
    {
        var netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkId];
        var player = netObj.GetComponent<PlayerControllerBase>();

        if (player != null)
        {
            player.HealthSystem(damageAmount, false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RespawnPlayerServerRpc(ulong playerNetworkId)
    {
        StartCoroutine(DelayedRespawnServer(playerNetworkId));
    }

    private System.Collections.IEnumerator DelayedRespawnServer(ulong playerNetworkId)
    {
        yield return new WaitForSeconds(0.5f);

        var netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkId];
        var player = netObj.GetComponent<PlayerControllerBase>();

        if (player != null)
        {
            player.Respawn(respawnPoint.transform.position);
        }

        flag = false;
    }

    #endregion
}
