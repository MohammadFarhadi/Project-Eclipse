using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[Serializable]
public class GameData
{
    // Scene
    public string CurrentSceneName;

    // Players
    public float Player1Stamina;
    public int   Player1HealthPoint;
    public float Player1Health;
    public float Player1PosX, Player1PosY, Player1PosZ;

    public float Player2Stamina;
    public int   Player2HealthPoint;
    public float Player2Health;
    public float Player2PosX, Player2PosY, Player2PosZ;

    // World
    public List<ChunkData>       Chunks       = new();
    public List<EnemyData>       Enemies      = new();
    public List<CollectibleData> Collectibles = new();

    public GameData(MeleePlayerController player1, RangedPlayerController player2)
    {
        try
        {
            // ---- Players ----
            if (player1 == null) { Debug.LogError("[Save] player1 (melee) is NULL"); return; }
            if (player2 == null) { Debug.LogError("[Save] player2 (ranged) is NULL"); return; }

            CurrentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            try {
                Player1Stamina     = player1.Current_Stamina.Value;
                Player1HealthPoint = player1.HealthPoint.Value;
                Player1Health      = player1.current_health.Value;
                Player1PosX        = player1.transform.position.x;
                Player1PosY        = player1.transform.position.y;
                Player1PosZ        = player1.transform.position.z;
            } catch (Exception ex) {
                Debug.LogError($"[Save] Failed reading player1: {ex}");
            }

            try {
                Player2Stamina     = player2.Current_Stamina.Value;
                Player2HealthPoint = player2.HealthPoint.Value;
                Player2Health      = player2.current_health.Value;
                Player2PosX        = player2.transform.position.x;
                Player2PosY        = player2.transform.position.y;
                Player2PosZ        = player2.transform.position.z;
            } catch (Exception ex) {
                Debug.LogError($"[Save] Failed reading player2: {ex}");
            }

            // ---- Chunks ----
            SafeForEach("Chunk", obj =>
            {
                try
                {
                    if (obj == null) throw new NullReferenceException("chunk obj is null");
                    var t = obj.transform;
                    Chunks.Add(new ChunkData {
                        prefabName = obj.name,
                        PositionX  = t.position.x,
                        PositionY  = t.position.y,
                        PositionZ  = t.position.z
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Save] Chunk '{obj?.name ?? "NULL"}' failed: {ex}");
                }
            });

            // ---- Enemies ----
            SafeForEach("Enemy", obj =>
            {
                try
                {
                    if (obj == null) throw new NullReferenceException("enemy obj is null");

                    // interface might be on self/parent/children
                    var iEnemy = obj.GetComponent<InterfaceEnemies>()
                                ?? obj.GetComponentInParent<InterfaceEnemies>()
                                ?? obj.GetComponentInChildren<InterfaceEnemies>();

                    if (iEnemy == null)
                        throw new MissingComponentException($"InterfaceEnemies not found on '{obj.name}' (self/parent/children)");

                    float hp;
                    try { hp = iEnemy.HealthPoint; }
                    catch (Exception ex) { throw new Exception($"HealthPoint getter threw: {ex.Message}", ex); }

                    var t = obj.transform;
                    Enemies.Add(new EnemyData {
                        name      = obj.name,
                        health    = hp,
                        isAlive   = hp > 0f,                         // <— NEW
                        PositionX = t.position.x,
                        PositionY = t.position.y,
                        PositionZ = t.position.z
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Save] Enemy '{obj?.name ?? "NULL"}' failed: {ex}");
                }
            });

            // ---- SaveAble / Collectibles ----
            SafeForEach("SaveAble", obj =>
            {
                try
                {
                    if (obj == null) throw new NullReferenceException("saveable obj is null");
                    var t = obj.transform;
                    Collectibles.Add(new CollectibleData {
                        name      = obj.name,
                        PositionX = t.position.x,
                        PositionY = t.position.y,
                        PositionZ = t.position.z
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Save] SaveAble '{obj?.name ?? "NULL"}' failed: {ex}");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Save] Fatal error building GameData: {ex}");
        }
    }

    // Helper: runs the action on each object with the tag and logs only on error.
    private static void SafeForEach(string tag, Action<GameObject> action)
    {
        GameObject[] objs;
        try
        {
            objs = GameObject.FindGameObjectsWithTag(tag);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Save] FindGameObjectsWithTag('{tag}') failed: {ex}");
            return;
        }

        for (int i = 0; i < objs.Length; i++)
        {
            var obj = objs[i];
            try { action(obj); }
            catch (Exception ex)
            {
                Debug.LogError($"[Save] '{tag}' index {i} object '{obj?.name ?? "NULL"}' failed: {ex}");
            }
        }
    }

    [Serializable] public class ChunkData { public string prefabName; public float PositionX, PositionY, PositionZ; }

    [Serializable]
    public class EnemyData
    {
        public string name; 
        public float health; 
        public bool   isAlive;                 // <— NEW
        public float PositionX, PositionY, PositionZ;
    }
    [Serializable] public class CollectibleData { public string name; public float PositionX, PositionY, PositionZ; }
}
