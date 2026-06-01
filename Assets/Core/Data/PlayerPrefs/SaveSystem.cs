using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string saveFilePath = Application.persistentDataPath + "/playerDatabase.json";

    public static void SaveDatabase(PlayerDatabase db)
    {
        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(saveFilePath, json);
    }

    public static PlayerDatabase LoadDatabase()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            return JsonUtility.FromJson<PlayerDatabase>(json);
        }
        return new PlayerDatabase();
    }

    public static bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }
}