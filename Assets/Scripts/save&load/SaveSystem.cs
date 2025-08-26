using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class SaveSystem
{
    private const string ActiveSlot = "ActiveSlot"; // PlayerPrefs key
    
    private static string Dir
    {
        get
        {
            return Application.persistentDataPath;
        }
    }

    private static string SlotPath(int slot)
    {
        return Path.Combine(Dir, "Save_" + slot + ".nig");
    }

    private static string MetaPath(int slot)
    {
        return Path.Combine(Dir, "Save_" + slot + ".meta.json");
    }

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

    public static int GetActiveSlot()
    {
        return PlayerPrefs.GetInt(ActiveSlot, -1);
    }

    public static bool Exists(int slot)
    {
        return File.Exists(SlotPath(slot));
    }

    public static (int slot, bool exists, SaveSlotMeta meta)[] ListSlots()
    {
        var res = new (int, bool, SaveSlotMeta)[5];
        for (int s = 1; s <= 5; s++)
        {
            res[s - 1] = (s, Exists(s), TryReadMeta(s));
        }
        return res;
    }

    public static void SaveData(int slot, MeleePlayerController melee, RangedPlayerController ranged)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError("[Save] Invalid slot " + slot);
            return;
        }

        if (melee == null || ranged == null)
        {
            Debug.LogError("[Save] SaveData aborted: player ref null");
            return;
        }

        try
        {
            Directory.CreateDirectory(Dir);

            GameData data = new GameData(melee, ranged);
            string path = SlotPath(slot);

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(stream, data);
            }

            SaveSlotMeta meta = SaveSlotMeta.From(data, slot);
            File.WriteAllText(MetaPath(slot), JsonUtility.ToJson(meta, true));

            Debug.Log("[Save] Wrote slot " + slot + ": " + path);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Save] Failed to save slot " + slot + ": " + ex);
        }
    }

    public static GameData LoadData(int slot)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError("[Save] Invalid slot " + slot);
            return null;
        }

        string path = SlotPath(slot);

        try
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning("[Save] Slot " + slot + " not found at '" + path + "'.");
                return null;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                GameData data = formatter.Deserialize(stream) as GameData;
                if (data == null)
                {
                    Debug.LogError("[Save] Deserialized GameData null (slot " + slot + ")");
                    return null;
                }

                Debug.Log("[Save] Loaded slot " + slot + " from '" + path + "'");
                return data;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[Save] Failed to load slot " + slot + ": " + ex);
            return null;
        }
    }

    public static void DeleteSlot(int slot)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError("[Save] Invalid slot " + slot);
            return;
        }

        string save = SlotPath(slot);
        string meta = MetaPath(slot);

        try
        {
            if (File.Exists(save)) File.Delete(save);
            if (File.Exists(meta)) File.Delete(meta);

            if (GetActiveSlot() == slot) BeginSessionInSlot(-1); // clear active
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
        if (!IsValidSlot(slot))
        {
            Debug.LogWarning("[Save] No active slot for autosave");
            return;
        }
        SaveData(slot, melee, ranged);
    }

    // --- Metadata ---
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

    private static bool IsValidSlot(int s)
    {
        return s >= 1 && s <= 5;
    }
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
        SaveSlotMeta m = new SaveSlotMeta();
        m.slot = slot;
        m.scene = d.CurrentSceneName;
        m.savedAtUnix = DateTimeOffset.Now.ToUnixTimeSeconds();
        m.savedAtLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        m.p1hp = d.Player1Health;
        m.p2hp = d.Player2Health;
        return m;
    }
}
