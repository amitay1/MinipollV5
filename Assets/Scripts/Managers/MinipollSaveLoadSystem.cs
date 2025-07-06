/***************************************************************
 *  MinipollSaveLoadSystem.cs
 *
 *  תיאור כללי:
 *    מערכת שמירה וטעינה מורחבת למשחק Minipoll:
 *      - יכול לשמור נתוני עולם (יום, מזג אוויר), מצב מיניפולים (מיקום, בריאות, צרכים),
 *        מצב מנהלים (מנהיגים, אינדקסים וכו’).
 *    - מאפשר שמירה לכמה "Slots" או לקובץ אחד,
 *    - ניתן להרחבה לשמירה מקוונת, ב-Cloud (כאן נדגים שמירה לקובץ JSON).
 *
 *  דרישות קדם:
 *    - רפרנסים ל-GameManager / WorldManager / MinipollManager.
 *    - מנגנון סריאליזציה נתמך (JsonUtility, או Newtonsoft Json).
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using MinipollGame.Managers;

[System.Serializable]
public class WorldSaveData
{
    public int dayCount;
    public float currentTime01;
    public string currentWeather;
}

[System.Serializable]
public class MinipollSaveData
{
    public string name;
    public float health;
    public float posX;
    public float posY;
    public float posZ;
    // אפשר להוסיף צרכים, רגשות, וכו’...
}

[System.Serializable]
public class SaveGameData
{
    public WorldSaveData worldData;
    public List<MinipollSaveData> minipollsData = new List<MinipollSaveData>();
    // אפשר להוסיף עוד: Tribe data, Economy data, Quest data...
}

public class MinipollSaveLoadSystem : MonoBehaviour
{
    [Header("File Settings")]
    public string saveFileName = "MinipollSave.json";

    [Header("References")]
    public GameManager gameManager;
    public WorldManager worldManager;
    public MinipollManager minipollManager;

    private void Awake()
    {
        if (!gameManager) gameManager = FindObjectOfType<GameManager>();
        if (!worldManager) worldManager = FindObjectOfType<WorldManager>();
        if (!minipollManager) minipollManager = FindObjectOfType<MinipollManager>();
    }

    /// <summary>
    /// שמירה למשחק (מייצר SaveGameData אחד וכותב ל-JSON)
    /// </summary>
    public void SaveGame()
    {
        SaveGameData data = new SaveGameData();

        // 1) מילוי נתוני עולם
        data.worldData = new WorldSaveData();
        data.worldData.dayCount = worldManager.timeSystem.dayCount;
        data.worldData.currentTime01 = worldManager.timeSystem.currentTime01;
        data.worldData.currentWeather = worldManager.weatherSystem.currentWeather.ToString();

        // 2) מילוי נתוני מיניפולים
        var allMinipolls = minipollManager.GetAllMinipolls();
        foreach (var mp in allMinipolls)
        {
            MinipollSaveData mpData = new MinipollSaveData();
            mpData.name = mp.name;
            mpData.health = mp.health; 
            // mpData.posX = mp.transform.position.x;
            // mpData.posY = mp.transform.position.y;
            // mpData.posZ = mp.transform.position.z;
            // אפשר להרחיב לצרכים וכו’
            data.minipollsData.Add(mpData);
        }

        // 3) כתיבה לקובץ
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"Minipoll game saved to {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save: {e.Message}");
        }
    }

    /// <summary>
    /// טעינה ממשחק שמור (קורא JSON ויוצר/מעדכן אובייקטים)
    /// </summary>
    public void LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save file found at {path}");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            SaveGameData data = JsonUtility.FromJson<SaveGameData>(json);

            // 1) שיחזור זמן ויום
            worldManager.timeSystem.dayCount = data.worldData.dayCount;
            worldManager.timeSystem.currentTime01 = data.worldData.currentTime01;
            // מזג אוויר
            var we = worldManager.weatherSystem;
            Enum.TryParse(data.worldData.currentWeather, out WorldManager.WeatherSystem.WeatherType wType);
            we.SetWeather(wType);

            // 2) מחיקת המיניפולים הקיימים והוספה מחדש?
            var all = minipollManager.GetAllMinipolls();
            // להרוס אותם
            for (int i = all.Count - 1; i >= 0; i--)
            {
                minipollManager.RemoveMinipoll(all[i]);
            }

            // 3) יצירת מיניפולים
            foreach (var mpData in data.minipollsData)
            {
                var newMp = minipollManager.SpawnMinipoll(new Vector3(mpData.posX, mpData.posY, mpData.posZ));
                if (newMp != null)
                {
                    newMp.name = mpData.name;
                    newMp.health = mpData.health;
                    // אפשר לשחזר צרכים וכו’
                }
            }

            Debug.Log("Game loaded successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load: {e.Message}");
        }
    }
}
