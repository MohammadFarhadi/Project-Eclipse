using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class SaveSystem
{
    public static bool IsRestoring { get; private set; }

    private const string ActiveSlot = "ActiveSlot";

    // Saved selection to reuse during restore/spawn
    public static int RestoreMeleeID  = -1;
    public static int RestoreRangedID = -1;

    private static GameData _pendingData;   // data read before scene load

    // Paths
    private static string Dir => Application.persistentDataPath;
    private static string SlotPath(int slot) => Path.Combine(Dir, $"Save_{slot}.nig");
    private static string MetaPath(int slot) => Path.Combine(Dir, $"Save_{slot}.meta.json");

    // --- Session / basic helpers ---
    public static void BeginSessionInSlot(int slot)
    {
        if (slot == -1)
        {
            PlayerPrefs.DeleteKey(ActiveSlot);
            PlayerPrefs.Save();
            Debug.Log("[Save] Active slot cleared");
            return;
        }

        if (!IsValidSlot(slot))
        {
            Debug.LogError("[Save] Invalid slot " + slot);
            return;
        }

        PlayerPrefs.SetInt(ActiveSlot, slot);
        PlayerPrefs.Save();
        Debug.Log("[Save] Active slot set: " + slot);
    }

    public static int GetActiveSlot() => PlayerPrefs.GetInt(ActiveSlot, -1);
    public static bool Exists(int slot) => File.Exists(SlotPath(slot));

    // --- Save ---
    public static void SaveData(int slot, MeleePlayerController melee, RangedPlayerController ranged)
    {
        if (!IsValidSlot(slot)) { Debug.LogError("[Save] Invalid slot " + slot); return; }
        if (melee == null || ranged == null) { Debug.LogError("[Save] SaveData aborted: player ref null"); return; }

        try
        {
            Directory.CreateDirectory(Dir);

            var data = new GameData(melee, ranged);
            var path = SlotPath(slot);

            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(path, FileMode.Create))
                formatter.Serialize(stream, data);

            var meta = SaveSlotMeta.From(data, slot);
            File.WriteAllText(MetaPath(slot), JsonUtility.ToJson(meta, true));

            Debug.Log("[Save] Wrote slot " + slot + ": " + path);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Save] Failed to save slot " + slot + ": " + ex);
        }
    }

    // --- Restore (event-driven; survives scene change) ---
    public static void StartRestore(int slot)
    {
        Debug.Log("[Save] StartRestore for slot: " + slot);
        IsRestoring = true;

        if (!IsValidSlot(slot)) { Debug.LogError("[Save] Invalid slot " + slot); IsRestoring = false; return; }

        string path = SlotPath(slot);
        if (!File.Exists(path)) { Debug.LogWarning("[Save] Slot not found at " + path); IsRestoring = false; return; }

        try
        {
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(path, FileMode.Open))
                _pendingData = formatter.Deserialize(stream) as GameData;
            
        }
        catch (Exception ex)
        {
            Debug.LogError("[Save] Deserialize failed: " + ex);
            IsRestoring = false;
            return;
        }
        // Debug chunk order in GameData after loading
        if (_pendingData != null && _pendingData.Chunks != null)
        {
            Debug.Log("[Debug] Loaded GameData chunk count: " + _pendingData.Chunks.Count);
            for (int i = 0; i < _pendingData.Chunks.Count; i++)
            {
                var c = _pendingData.Chunks[i];
                Debug.Log($"[Debug] Chunk {i}: prefabName={c.prefabName}, Order={c.Order}, Pos=({c.PositionX}, {c.PositionY}, {c.PositionZ})");
            }
        }

        if (_pendingData == null) { Debug.LogError("[Save] Deserialized GameData is null."); IsRestoring = false; return; }

        // Remember which characters to spawn
        RestoreMeleeID  = _pendingData.MeleeCharacterID;
        RestoreRangedID = _pendingData.RangedCharacterID;

        Debug.Log("[Save] Loading scene: " + _pendingData.CurrentSceneName);
        SceneManager.sceneLoaded += OnSceneLoadedAfterRestore;
        SceneManager.LoadSceneAsync(_pendingData.CurrentSceneName, LoadSceneMode.Single);
    }

    
    private static void OnSceneLoadedAfterRestore(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedAfterRestore;
        Debug.Log("[Save] Scene loaded: " + scene.name + " — restoring players…");

        // Find a spawner in the new scene (even if it's inactive)
        PlayerSpawnerManager spawner = null;
        var spawners = UnityEngine.Object.FindObjectsOfType<PlayerSpawnerManager>(true);
        if (spawners != null && spawners.Length > 0) spawner = spawners[0];

        if (spawner == null)
        {
            Debug.LogWarning("[Save] PlayerSpawnerManager not found.");
            IsRestoring = false;
            return;
        }

        // Force spawn now (don’t rely on Start)
        spawner.SpawnLocalPlayers();
        RestoreMeleeID = -1;
        RestoreRangedID = -1;
        // Apply saved values to the correct players
        var all = UnityEngine.Object.FindObjectsOfType<PlayerControllerBase>(true);
        MeleePlayerController  melee  = null;
        RangedPlayerController ranged = null;

        foreach (var pcb in all)
        {
            int id = pcb.CharacterID.Value;
            if (id == _pendingData.MeleeCharacterID  && melee  == null) melee  = pcb.GetComponent<MeleePlayerController>();
            if (id == _pendingData.RangedCharacterID && ranged == null) ranged = pcb.GetComponent<RangedPlayerController>();
        }

        if (melee != null)
        {
            melee.transform.position    = new Vector3(_pendingData.Player1PosX, _pendingData.Player1PosY, _pendingData.Player1PosZ);
            melee.Current_Stamina.Value = _pendingData.Player1Stamina;
            melee.HealthPoint.Value     = _pendingData.Player1HealthPoint;
            melee.current_health.Value  = _pendingData.Player1Health;
        }
        else Debug.LogWarning("[Save] MeleePlayerController not found after spawn.");

        if (ranged != null)
        {
            ranged.transform.position    = new Vector3(_pendingData.Player2PosX, _pendingData.Player2PosY, _pendingData.Player2PosZ);
            ranged.Current_Stamina.Value = _pendingData.Player2Stamina;
            ranged.HealthPoint.Value     = _pendingData.Player2HealthPoint;
            ranged.current_health.Value  = _pendingData.Player2Health;
        }
        else Debug.LogWarning("[Save] RangedPlayerController not found after spawn.");

        spawner.StartCoroutine(RestoreWorldNextFrame());
        return;
        
IEnumerator RestoreWorldNextFrame()
         {
             float t = 0f;
             while (GameObject.FindGameObjectsWithTag("Chunk").Length == 0 && t < 2f)
             {
                 t += Time.deltaTime;
                 yield return null;
             }
             Debug.Log("[RestoreWorldNextFrame] Found " + GameObject.FindGameObjectsWithTag("Chunk").Length + " live chunks after waiting.");
         
             yield return null;
         
// --- CHUNKS: restore exact saved layout, shifted so saved-start aligns with live-start ---
             var cm = UnityEngine.Object.FindFirstObjectByType<ChunkManager>(FindObjectsInactive.Include);
             if (cm != null && cm.chunks != null && _pendingData.Chunks != null && _pendingData.Chunks.Count > 0)
             {
                 // saved order 0..N-1
                 var saved = new System.Collections.Generic.List<GameData.ChunkData>(_pendingData.Chunks);
                 saved.Sort((a, b) => a.Order.CompareTo(b.Order));

                 // map live by canonical name
                 string Canon(string s) => s.Replace("(Clone)", "").Replace(" ", "").ToLowerInvariant();
                 var liveByName = new System.Collections.Generic.Dictionary<string, GameObject>();
                 foreach (var go in cm.chunks)
                     if (go) liveByName[Canon(go.name)] = go;

                 if (liveByName.TryGetValue(Canon(saved[0].prefabName), out var startLive))
                 {
                     // shift so saved-start.x -> live-start.x (preserve relative spacing exactly)
                     float dx = startLive.transform.position.x - saved[0].PositionX;

                     // float dy = startLive.transform.position.y - saved[0].PositionY;

                     for (int i = 0; i < saved.Count; i++)
                     {
                         var s = saved[i];
                         if (!liveByName.TryGetValue(Canon(s.prefabName), out var go)) continue;

                         var p = go.transform.position;
                         p.x   = s.PositionX + dx;
                         // p.y = s.PositionY + dy;   // uncomment if you want exact Y too
                         go.transform.position = p;
                     }
                 }
             }



         
             // --- ENEMIES (by name still ok if unique) ---
             foreach (var e in _pendingData.Enemies)
             {
                 var obj = GameObject.Find(e.name);
                 Debug.Log("[RestoreWorldNextFrame] Enemy: " + e.name + " found: " + (obj != null));
                 if (!obj) continue;
         
                 if (!e.isAlive) { obj.SetActive(false); Debug.Log("[RestoreWorldNextFrame] Enemy " + e.name + " set inactive."); continue; }
         
                 obj.transform.position = new Vector3(e.PositionX, e.PositionY, e.PositionZ);
                 Debug.Log("[RestoreWorldNextFrame] Enemy " + e.name + " position set to: " + obj.transform.position);
         
                 var iEnemy = obj.GetComponent<InterfaceEnemies>()
                            ?? obj.GetComponentInChildren<InterfaceEnemies>()
                            ?? obj.GetComponentInParent<InterfaceEnemies>();
                 if (iEnemy != null)
                 {
                     try { iEnemy.SetHealth(Mathf.RoundToInt(e.health)); Debug.Log("[RestoreWorldNextFrame] Enemy " + e.name + " health set to: " + e.health); }
                     catch (Exception ex) { Debug.LogError("[RestoreWorldNextFrame] Enemy SetHealth failed for '" + e.name + "': " + ex); }
                 }
             }
         
             // --- SAVEABLES / COLLECTIBLES ---
             foreach (var col in _pendingData.Collectibles)
             {
                 var obj = GameObject.Find(col.name);
                 Debug.Log("[RestoreWorldNextFrame] Collectible: " + col.name + " found: " + (obj != null));
                 if (!obj) continue;
                 obj.transform.position = new Vector3(col.PositionX, col.PositionY, col.PositionZ);
                 Debug.Log("[RestoreWorldNextFrame] Collectible " + col.name + " position set to: " + obj.transform.position);
             }
         
             IsRestoring = false;
             _pendingData = null;
             RestoreMeleeID = -1;
             RestoreRangedID = -1;
             Debug.Log("[RestoreWorldNextFrame] World restored.");
         }    }

    // --- Delete / Autosave / Meta ---
    public static void DeleteSlot(int slot)
    {
        if (!IsValidSlot(slot)) { Debug.LogError("[Save] Invalid slot " + slot); return; }

        string save = SlotPath(slot);
        string meta = MetaPath(slot);

        try
        {
            if (File.Exists(save)) File.Delete(save);
            if (File.Exists(meta)) File.Delete(meta);

            if (GetActiveSlot() == slot) BeginSessionInSlot(-1);
            Debug.Log("[Save] Deleted slot " + slot);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Save] Failed to delete slot " + slot + ": " + ex);
        }
    }

    public static void AutoSave(MeleePlayerController melee, RangedPlayerController ranged)
    {
        int slot = GetActiveSlot();
        if (!IsValidSlot(slot)) { Debug.LogWarning("[Save] No active slot for autosave"); return; }
        SaveData(slot, melee, ranged);
    }

    public static SaveSlotMeta TryReadMeta(int slot)
    {
        if (!IsValidSlot(slot)) return null;

        string p = MetaPath(slot);
        try
        {
            if (!File.Exists(p)) return null;
            string json = File.ReadAllText(p);
            return JsonUtility.FromJson<SaveSlotMeta>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Save] Read meta failed for slot " + slot + ": " + ex);
            return null;
        }
    }

    private static bool IsValidSlot(int s) => s >= 1 && s <= 5;
}

[Serializable]
public class SaveSlotMeta
{
    public int slot;
    public string scene;
    public string savedAtLocal;
    public long savedAtUnix;
    public float p1hp, p2hp;

    public static SaveSlotMeta From(GameData d, int slot)
    {
        var m = new SaveSlotMeta
        {
            slot = slot,
            scene = d.CurrentSceneName,
            savedAtUnix = DateTimeOffset.Now.ToUnixTimeSeconds(),
            savedAtLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            p1hp = d.Player1Health,
            p2hp = d.Player2Health
        };
        return m;
    }
}
