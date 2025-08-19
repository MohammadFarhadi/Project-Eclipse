using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    public static void SaveData(MeleePlayerController player1,  RangedPlayerController player2)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/Save.nig";
        FileStream stream = new FileStream(path, FileMode.Create);
        GameData playerData = new GameData(player1, player2);
        formatter.Serialize(stream, playerData);
        stream.Close();
    }

    public static GameData LoadData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/Save.nig";
        FileStream stream = new FileStream(path, FileMode.Open);
        if (File.Exists(path))
        {
            GameData data = formatter.Deserialize(stream) as GameData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("save file not found.");
            return null;
        }
    }
    
}
