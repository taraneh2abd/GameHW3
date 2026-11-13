using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string FileName = "savegame.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void Save(SaveData data)
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not write save file: " + exception.Message);
        }
    }

    public static bool TryLoad(out SaveData data)
    {
        data = null;

        try
        {
            if (!File.Exists(SavePath))
            {
                return false;
            }

            SaveData loaded = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            if (loaded == null || loaded.levelIndex < 0 || loaded.roundIndex < 0 || loaded.lives < 1)
            {
                return false;
            }

            data = loaded;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static void Delete()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not delete save file: " + exception.Message);
        }
    }
}
