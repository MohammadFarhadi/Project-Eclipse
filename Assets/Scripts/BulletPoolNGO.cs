using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletPoolNGO : NetworkBehaviour
{
    [System.Serializable]
    public class BulletType
    {
        public string bulletTag;
        public GameObject bulletPrefab;
        public int poolSize;
    }

    public List<BulletType> bulletTypes;

    private Dictionary<string, List<GameObject>> bulletPools;
    private Dictionary<string, GameObject> bulletPrefabs;

    private void Start()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local) return;

        // منتظر می‌مونیم تا NGO کامل راه‌اندازی بشه
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server Started ...");
            InitPool();
        }
        else
        {
            NetworkManager.Singleton.OnServerStarted += InitPool;
        }
    }

    private void InitPool()
    {
        if (!IsServer) return;

        Debug.Log("✅ Initializing bullet pool on server.");

        bulletPools = new Dictionary<string, List<GameObject>>();
        bulletPrefabs = new Dictionary<string, GameObject>();

        foreach (var type in bulletTypes)
        {
            List<GameObject> pool = new List<GameObject>();
            bulletPools[type.bulletTag] = pool;
            bulletPrefabs[type.bulletTag] = type.bulletPrefab;

            for (int i = 0; i < type.poolSize; i++)
            {
                GameObject bulletObj = Instantiate(type.bulletPrefab);
                var netObj = bulletObj.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                    Debug.Log($"🔫 Bullet spawned [{type.bulletTag}]");
                }
                else
                {
                    Debug.LogError("❌ Bullet prefab has no NetworkObject!");
                }

                pool.Add(bulletObj);
            }
        }
    }
   

    public GameObject GetBullet(string bulletTag)
    {
        if (!IsServer) {
            Debug.Log("Is Not Server");
            return null; // 🔴 فقط سرور حق دارد گلوله بدهد
        }

        if (!bulletPools.ContainsKey(bulletTag))
        {
            Debug.LogWarning("No bullet pool with tag: " + bulletTag);
            return null;
        }

        foreach (var bullet in bulletPools[bulletTag])
        {
            if (!bullet.activeInHierarchy)
            {
                var netObj = bullet.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    Debug.Log("Bullet found");
                    bullet.SetActive(true);
                    SetBulletActiveClientRpc(netObj.NetworkObjectId, true);
                }
                return bullet;
            }
        }
        Debug.Log("No bullet in Hierachy");
        // ایجاد گلوله جدید فقط در صورت نیاز
        if (bulletPrefabs.ContainsKey(bulletTag))
        {
            GameObject obj = Instantiate(bulletPrefabs[bulletTag]);
            obj.SetActive(true); // ✅ فعال کن
            var netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn(true); // 🔵 Spawn بدون مالک خاص
            SetBulletActiveClientRpc(netObj.NetworkObjectId, true);
            bulletPools[bulletTag].Add(obj);
            return obj;
        }
        Debug.Log("No bullet in Pool");
        return null;
    }


    public void ReturnBullet(string bulletTag, GameObject bulletObj)
    {
        if (!IsServer) return; // فقط سرور مدیریت کند
        var netObj = bulletObj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            SetBulletActiveClientRpc(netObj.NetworkObjectId, false);
        }
    }
    [ClientRpc]
    void SetBulletActiveClientRpc(ulong netId, bool isActive)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out var netObj))
        {
            netObj.gameObject.SetActive(isActive);
        }
        else
        {
            Debug.LogWarning($"Bullet with id {netId} not found on client.");
        }
    }
    public void DespawnAllBullets()
    {
        if (!IsServer) return;

        foreach (var pool in bulletPools.Values)
        {
            foreach (var bullet in pool)
            {
                if (bullet != null && bullet.activeInHierarchy)
                {
                    bullet.GetComponent<NetworkObject>().Despawn(true); // با destroy=true حذف کامل میشه
                }
            }
            pool.Clear();
        }

        bulletPools.Clear();
        bulletPrefabs.Clear();

        Debug.Log("🧹 All bullets despawned before scene change.");
    }




}
