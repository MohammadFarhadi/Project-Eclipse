using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{  
    //می خوام یه دیکشنری درست کنم که این دیکشنری از مدل های مختلف تیر درست شده که هر کدوم از این مدل ها خودشون یه آراین.
    [System.Serializable]
    public class BulletType
    {
        public string bulletTag;
        public GameObject bulletPrefab;
        public int poolSize;
    }
    public List<BulletType> bulletTypes;
    private Dictionary<string, List<GameObject>> BulletPools;
    void Start()
    {
        BulletPools = new Dictionary<string, List<GameObject>>();
        for (int i = 0; i < bulletTypes.Count; i++)
        {
            List<GameObject> pool = new List<GameObject>();
            BulletPools[bulletTypes[i].bulletTag] = pool;
            for (int j = 1; j < bulletTypes[i].poolSize; j++)
            {
                GameObject obj = Instantiate(bulletTypes[i].bulletPrefab);
                obj.SetActive(false);
                pool.Add(obj);
            }
        }
    }
    public GameObject GetBullet(string bulletTag)
    {
        if (!BulletPools.ContainsKey(bulletTag))
        {
            Debug.LogWarning("No bullet pool with tag: " + bulletTag);
            return null;
        }

        foreach (var bullet in BulletPools[bulletTag])
        {
            if (!bullet.activeInHierarchy)
            {
                bullet.SetActive(true);
                return bullet;
            }
        }

        var bulletType = bulletTypes.Find(bt => bt.bulletTag == bulletTag);
        if (bulletType != null)
        {
            GameObject obj = Instantiate(bulletType.bulletPrefab);
            obj.SetActive(true);
            BulletPools[bulletTag].Add(obj);
            return obj;
        }

        return null;
    }
}
