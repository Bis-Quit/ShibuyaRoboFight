using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string saveFilePath = Application.persistentDataPath + "/playerStats.json";

    public static void SaveProfile(PlayerData data)
    {
        string jsonText = JsonUtility.ToJson(data, true); 
        File.WriteAllText(saveFilePath, jsonText);
    }

    public static PlayerData LoadProfile()
    {
        if (File.Exists(saveFilePath))
        {
            string jsonText = File.ReadAllText(saveFilePath);
            return JsonUtility.FromJson<PlayerData>(jsonText);
        }
        return new PlayerData(); 
    }

    public static void DeleteProfile()
    {
        if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
    }

    public static bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }
}