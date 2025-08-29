using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        IsRestoring = false;
        Debug.Log("[Save] Restore complete.");
    }

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
