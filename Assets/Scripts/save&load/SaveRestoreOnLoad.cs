// using System.Collections.Generic;
// using UnityEngine;
//
// public class SaveRestoreOnLoad : MonoBehaviour
// {
//     [Header("Prefabs: names MUST match what you saved")]
//     public GameObject[] chunkPrefabs;
//     public GameObject[] enemyPrefabs;
//     public GameObject[] collectiblePrefabs;
//
//     [Header("Replace existing scene objects before spawning?")]
//     public bool replaceChunks       = true;
//     public bool replaceEnemies      = true;
//     public bool replaceCollectibles = true;
//
//     void Start()
//     {
//         // Load currently-active slot (set by your menu)
//         var data = SaveSystem.LoadActive();
//         if (data == null)
//         {
//             Debug.Log("[Restore] No active save (new game or no file). Skipping restore.");
//             return;
//         }
//
//         // If you loaded into a different scene than saved (e.g. cutscene), skip world restore.
//         var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
//         if (!string.Equals(currentScene, data.CurrentSceneName))
//         {
//             Debug.Log($"[Restore] Scene mismatch (current='{currentScene}' vs saved='{data.CurrentSceneName}'). " +
//                       "Skipping world restore; you can call restore later when the saved scene loads.");
//             // You could still apply players here if you want; most games wait for the real saved scene.
//             return;
//         }
//
//         // 1) Players
//         ApplyPlayers(data);
//
//         // 2) World (replace then respawn)
//         if (replaceChunks)       DestroyAllWithTag("Chunk");
//         if (replaceEnemies)      DestroyAllWithTag("Enemy");
//         if (replaceCollectibles) DestroyAllWithTag("SaveAble");
//
//         // Build name->prefab lookup
//         var chunkMap       = BuildLookup(chunkPrefabs);
//         var enemyMap       = BuildLookup(enemyPrefabs);
//         var collectibleMap = BuildLookup(collectiblePrefabs);
//
//         // 3) Spawn world
//         SpawnChunks(data, chunkMap);
//         SpawnEnemies(data, enemyMap);
//         SpawnCollectibles(data, collectibleMap);
//
//         Debug.Log("[Restore] Finished applying save for scene '" + data.CurrentSceneName + "'.");
//     }
//
//     // ---------- Players ----------
//     void ApplyPlayers(GameData d)
//     {
//         var melee  = FindActive<MeleePlayerController>();
//         var ranged = FindActive<RangedPlayerController>();
//
//         if (melee != null)
//         {
//             melee.transform.position    = new Vector3(d.Player1PosX, d.Player1PosY, d.Player1PosZ);
//             melee.Current_Stamina.Value = d.Player1Stamina;
//             melee.HealthPoint.Value     = d.Player1HealthPoint;
//             melee.current_health.Value  = d.Player1Health;
//         }
//         else Debug.LogWarning("[Restore] Active MeleePlayerController not found.");
//
//         if (ranged != null)
//         {
//             ranged.transform.position    = new Vector3(d.Player2PosX, d.Player2PosY, d.Player2PosZ);
//             ranged.Current_Stamina.Value = d.Player2Stamina;
//             ranged.HealthPoint.Value     = d.Player2HealthPoint;
//             ranged.current_health.Value  = d.Player2Health;
//         }
//         else Debug.LogWarning("[Restore] Active RangedPlayerController not found.");
//     }
//
//     T FindActive<T>() where T : MonoBehaviour
//     {
//         foreach (var c in FindObjectsOfType<T>())
//             if (c.gameObject.activeInHierarchy) return c;
//         return null;
//     }
//
//     // ---------- World destroy helpers ----------
//     void DestroyAllWithTag(string tag)
//     {
//         var objs = GameObject.FindGameObjectsWithTag(tag);
//         foreach (var o in objs) Destroy(o);
//     }
//
//     // ---------- Spawn ----------
//     void SpawnChunks(GameData d, Dictionary<string, GameObject> map)
//     {
//         foreach (var c in d.Chunks)
//         {
//             if (!map.TryGetValue(c.prefabName, out var prefab))
//             {
//                 Debug.LogWarning("[Restore] Chunk prefab not found by name: " + c.prefabName);
//                 continue;
//             }
//             Instantiate(prefab, new Vector3(c.PositionX, c.PositionY, c.PositionZ), Quaternion.identity);
//         }
//     }
//
//     void SpawnEnemies(GameData d, Dictionary<string, GameObject> map)
//     {
//         foreach (var e in d.Enemies)
//         {
//             // NOTE: You saved e.name from the scene object. Ideally this matches a prefab name.
//             if (!map.TryGetValue(e.name, out var prefab))
//             {
//                 Debug.LogWarning("[Restore] Enemy prefab not found by name: " + e.name);
//                 continue;
//             }
//
//             var go = Instantiate(prefab, new Vector3(e.PositionX, e.PositionY, e.PositionZ), Quaternion.identity);
//
//             // Try to set health if your enemy exposes a setter
//             // Your InterfaceEnemies only has a getter, so add an optional setter interface:
//             // public interface ISettableEnemyHealth { void SetHealth(int hp); }
//             var settable = go.GetComponent<ISettableEnemyHealth>() ??
//                            go.GetComponentInChildren<ISettableEnemyHealth>() ??
//                            go.GetComponentInParent<ISettableEnemyHealth>();
//             if (settable != null)
//                 settable.SetHealth(Mathf.RoundToInt(e.health));
//         }
//     }
//
//     void SpawnCollectibles(GameData d, Dictionary<string, GameObject> map)
//     {
//         foreach (var c in d.Collectibles)
//         {
//             if (!map.TryGetValue(c.name, out var prefab))
//             {
//                 Debug.LogWarning("[Restore] Collectible prefab not found by name: " + c.name);
//                 continue;
//             }
//             Instantiate(prefab, new Vector3(c.PositionX, c.PositionY, c.PositionZ), Quaternion.identity);
//         }
//     }
//
//     // ---------- Prefab map ----------
//     Dictionary<string, GameObject> BuildLookup(GameObject[] list)
//     {
//         var dict = new Dictionary<string, GameObject>();
//         if (list == null) return dict;
//
//         foreach (var p in list)
//         {
//             if (p == null) continue;
//             var key = p.name; // MUST match what you saved in GameData (prefabName / name)
//             if (!dict.ContainsKey(key)) dict.Add(key, p);
//         }
//         return dict;
//     }
// }
//
// // Optional: add this to your enemy scripts to allow restoring HP cleanly.
// public interface ISettableEnemyHealth
// {
//     void SetHealth(int hp);
// }
