using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[Serializable]
public class GameData
{
    // Scene
    public string CurrentSceneName;

    //saving which prefab player choose
    public int MeleeCharacterID;
    public int RangedCharacterID;
    
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
    public List<string> ChunkOrder = new List<string>();
    public List<ChunkData>       Chunks       = new();
    public List<EnemyData>       Enemies      = new();
    public List<CollectibleData> Collectibles = new();

    
    //that one fucking object in chunk4
    public bool TempGroundActive = true; 

    public GameData(MeleePlayerController player1, RangedPlayerController player2)
    {

            // ---- Players ----
            if (player1 == null) { Debug.LogError("[Save] player1 (melee) is NULL"); return; }
            if (player2 == null) { Debug.LogError("[Save] player2 (ranged) is NULL"); return; }
            var p1Base = player1.GetComponent<PlayerControllerBase>();
            var p2Base = player2.GetComponent<PlayerControllerBase>();

            MeleeCharacterID  = p1Base ? p1Base.CharacterID.Value : 0; // your melee indices
            RangedCharacterID = p2Base ? p2Base.CharacterID.Value : 2; // your ranged indices

            CurrentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            

            // which prefabs to respawn later (keep names simple + stable)

            
                Player1Stamina     = player1.Current_Stamina.Value;
                Player1HealthPoint = player1.HealthPoint.Value;
                Player1Health      = player1.current_health.Value;
                Player1PosX        = player1.transform.position.x;
                Player1PosY        = player1.transform.position.y;
                Player1PosZ        = player1.transform.position.z;


                Player2Stamina     = player2.Current_Stamina.Value;
                Player2HealthPoint = player2.HealthPoint.Value;
                Player2Health      = player2.current_health.Value;
                Player2PosX        = player2.transform.position.x;
                Player2PosY        = player2.transform.position.y;
                Player2PosZ        = player2.transform.position.z;


                // ---- Chunks (save left→right order) ----
                var live = GameObject.FindGameObjectsWithTag("Chunk");
                Array.Sort(live, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

                for (int i = 0; i < live.Length; i++)
                {
                    var obj = live[i];
                    if (!obj) continue;

                    var t   = obj.transform;
                    string baseName = obj.name.Replace("(Clone)", string.Empty).Trim();

                    Chunks.Add(new ChunkData {
                        prefabName = baseName,
                        Order      = i,                     // <<— save the slot index
                        PositionX  = t.position.x,
                        PositionY  = t.position.y,
                        PositionZ  = t.position.z
                    });
                }

// ---- Enemies (ساده) ----
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var obj in enemies)
            {
                if (obj == null) continue;

                // اگر InterfaceEnemies روی خود آبجکت نبود، در بچه‌ها/والد هم چک کن
                var iEnemy = obj.GetComponent<InterfaceEnemies>()
                             ?? obj.GetComponentInChildren<InterfaceEnemies>()
                             ?? obj.GetComponentInParent<InterfaceEnemies>();

                float hp      = (iEnemy != null) ? iEnemy.HealthPoint : 0f;
                bool  isAlive = (iEnemy != null) ? (iEnemy.HealthPoint > 0f) : obj.activeInHierarchy;

                var t = obj.transform;
                Enemies.Add(new EnemyData
                {
                    name      = obj.name,
                    health    = hp,
                    isAlive   = isAlive,    // اگر فیلد isAlive داری، ذخیره کن
                    PositionX = t.position.x,
                    PositionY = t.position.y,
                    PositionZ = t.position.z
                });
            }


// ---- SaveAble / Collectibles (ساده) ----
            var saveables = GameObject.FindGameObjectsWithTag("SaveAble");
            foreach (var obj in saveables)
            {
                if (obj == null) continue;

                var t = obj.transform;
                Collectibles.Add(new CollectibleData
                {
                    name      = obj.name,
                    PositionX = t.position.x,
                    PositionY = t.position.y,
                    PositionZ = t.position.z
                });
            }

            // ---- Temp Ground (special one-off) ----
            var tempGround = GameObject.Find("TempGround");  
            if (tempGround != null)
            {
                TempGroundActive = tempGround.activeSelf;

            }
            
        }



    [Serializable]
    public class ChunkData
    {
        public string prefabName; 
        public int    Order; 
        public float PositionX, PositionY, PositionZ;
    }

    [Serializable]
    public class EnemyData
    {
        public string name; 
        public float health; 
        public bool   isAlive;                 // <— NEW
        public float PositionX, PositionY, PositionZ;
    }
    [Serializable] public class CollectibleData
    { 
        public string name;
        // public bool isAlive;
        public float PositionX, PositionY, PositionZ;
        
    }
}
