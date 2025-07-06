/***************************************************************
 *  WorldManager.cs (Advanced Version)
 *
 *  תיאור כללי:
 *    סקריפט אחד שמנהל את הסביבה במשחק, ברמה משודרגת:
 *      1) TimeSystem – מחזור יום/לילה + אירועי ביניים (בוקר/ערב/עונה חדשה).
 *      2) WeatherSystem – מזג אוויר דינמי, פרופילים רחבים (גשם, שלג, ערפל, סערה),
 *         שינוי טמפרטורה תלוי יום/לילה, עונה, ומשתנה "לחות".
 *      3) EcologySystem – ניהול צמיחה של משאבים (צמחים, מים, חקלאות),
 *         כעת גם כולל SoilMoisture (לחות קרקע) שמושפעת ממשקעים.
 *      4) DisasterSystem – אפשרות לאירועים גדולים (סערת ברד קיצונית, ארבה, וכו').
 *      5) דוגמת Save/Load פשוטה ל-WorldState (בלי מימוש מלא, רק הדגמה).
 *
 *  דרישות קדם:
 *    - צריך אובייקט אחד בסצנה עם WorldManager
 *    - גישה ל-GameManager לבדיקת מצב משחק וקצב זמן (gameSpeed)
 *    - הגדרת Particle Systems/Prefabs לחלק ממזג האוויר (גשם/שלג/ערפל)
 *    - בקוד הדוגמה: מערכת ScriptableObject ל-WeatherProfile (לא חובה)
 *
 *  שימוש:
 *    1) הנח "WorldManager" יחיד בסצנה עם הסקריפט הזה.
 *    2) ב-Awake/Start הוא מאתחל את תת-המערכות.
 *    3) Update() קורא לפונקציות Update של כל תת-מערכת, כל עוד המשחק במצב Playing.
 *
 ***************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class WorldManager : MonoBehaviour
{
    #region Singleton
    private static WorldManager _instance;
    public static WorldManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<WorldManager>();
                if (_instance == null)
                {
                    // אם אין אובייקט בסצנה, ניצור אחד
                    GameObject wm = new GameObject("WorldManager_AutoCreated");
                    _instance = wm.AddComponent<WorldManager>();
                }
            }
            return _instance;
        }    }

    public static object WeatherType { get; internal set; }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (_instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
        }

        // אתחול תת-מערכות
        timeSystem.Init();
        weatherSystem.Init();
        ecologySystem.Init();
        disasterSystem.Init();
    }
    #endregion

    [Header("Reference to GameManager")]
    [Tooltip("נחפש אוטומטית אם לא הוגדר")]
    public GameManager gameManager;

    [Header("Time System (Advanced)")]
    public TimeSystem timeSystem = new TimeSystem();

    [Header("Weather System (Advanced)")]
    public WeatherSystem weatherSystem = new WeatherSystem();

    [Header("Ecology System (Advanced)")]
    public EcologySystem ecologySystem = new EcologySystem();

    [Header("Disaster System (Optional)")]
    public DisasterSystem disasterSystem = new DisasterSystem();

    // דגל זה מונע עדכונים כשמשחק בפאוז
    [Header("Global Update Settings")]
    public bool pauseWhenGameIsNotPlaying = true;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        // אפשר לטעון מצב עולם משמור (אם רוצים)
        // LoadWorldState();
    }

    private void Update()
    {
        if (pauseWhenGameIsNotPlaying && gameManager != null)
        {
            if (gameManager.CurrentState != GameManager.GameState.Playing)
                return;
        }

        float deltaTime = Time.deltaTime * (gameManager ? gameManager.gameSpeed : 1f);

        // עדכון של תת-מערכות
        timeSystem.UpdateTime(deltaTime);
        weatherSystem.UpdateWeather(deltaTime, timeSystem);
        ecologySystem.UpdateEcology(deltaTime, weatherSystem);
        disasterSystem.UpdateDisasters(deltaTime, timeSystem, weatherSystem, ecologySystem);
    }

    #region Save/Load World State (Example)
    /// <summary>
    /// שמירת מצב הסביבה (דוגמה מאוד בסיסית).
    /// </summary>
    public void SaveWorldState()
    {
        WorldSaveData data = new WorldSaveData()
        {
            dayCount = timeSystem.dayCount,
            currentTime01 = timeSystem.currentTime01,
            currentSeason = timeSystem.currentSeason.ToString(),
            currentWeather = weatherSystem.currentWeather.ToString(),
            temperatureCelsius = weatherSystem.temperatureCelsius,
            soilMoisture = ecologySystem.soilMoisture
        };

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, "world_save.json");
        File.WriteAllText(path, json);
        Debug.Log("WorldManager: World state saved to " + path);
    }

    /// <summary>
    /// טעינת מצב הסביבה (דוגמה מאוד בסיסית).
    /// </summary>
    public void LoadWorldState()
    {
        string path = Path.Combine(Application.persistentDataPath, "world_save.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("No world save file found.");
            return;
        }

        string json = File.ReadAllText(path);
        WorldSaveData data = JsonUtility.FromJson<WorldSaveData>(json);

        // שחזור חלקי
        timeSystem.dayCount = data.dayCount;
        timeSystem.currentTime01 = data.currentTime01;
        if (Enum.TryParse(data.currentSeason, out TimeSystem.Season loadedSeason))
        {
            timeSystem.currentSeason = loadedSeason;
        }
        if (Enum.TryParse(data.currentWeather, out WeatherSystem.WeatherType loadedWeather))
        {
            weatherSystem.currentWeather = loadedWeather;
        }
        weatherSystem.temperatureCelsius = data.temperatureCelsius;
        ecologySystem.soilMoisture = data.soilMoisture;

        // עדכון אפקטים/מצב מיד לאחר הטעינה
        weatherSystem.UpdateWeatherEffects();

        Debug.Log("WorldManager: World state loaded from " + path);
    }    internal object GetCurrentWeather()
    {
        return weatherSystem.currentWeather;
    }

    internal object GetCurrentSeason()
    {
        return timeSystem.currentSeason;
    }

    internal bool IsNight()
    {
        return timeSystem.currentTime01 > 0.8f || timeSystem.currentTime01 < 0.2f;
    }

    internal float GetTemperature()
    {
        return weatherSystem.temperatureCelsius;
    }

    [Serializable]
    public class WorldSaveData
    {
        public int dayCount;
        public float currentTime01;
        public string currentSeason;
        public string currentWeather;
        public float temperatureCelsius;
        public float soilMoisture;
    }
    #endregion

    #region TimeSystem Class (Advanced)
    [Serializable]
    public class TimeSystem
    {
        [Header("Day Cycle")]
        [Tooltip("משך יום במשחק בשניות-מציאות (לדוגמה: 300 => 5 דקות ליום מלא).")]
        public float dayLengthInSeconds = 300f;
        [Range(0f, 1f)]
        public float currentTime01 = 0f; // 0=שחר, 0.5=צהריים, 1=סוף יום
        public int dayCount = 1;
        public bool useSeasons = true;
        [Tooltip("אורך עונה בשיטה: X ימים => מעביר לעונה הבאה")]
        public float seasonLengthInDays = 3f;

        public enum Season
        {
            Spring,
            Summer,
            Autumn,
            Winter
        }
        public Season currentSeason = Season.Spring;

        // אירועים
        public event Action<int> OnNewDay;
        public event Action<Season> OnSeasonChanged;

        // אירועי ביניים
        public event Action OnMorning;   // לדוגמה, currentTime01 עולה מעל ~0.0
        public event Action OnNoon;      // around 0.5
        public event Action OnEvening;   // around 0.8?
        public event Action OnNight;     // after ~0.9

        // Flags לעזור שלא נקרא אירוע פעמיים באותו יום
        private bool hasTriggeredMorning, hasTriggeredNoon, hasTriggeredEvening, hasTriggeredNight;

        public void Init()
        {
            currentTime01 = 0f;
            dayCount = 1;
            currentSeason = Season.Spring;
            ResetDailyEvents();
        }

        public void UpdateTime(float deltaTime)
        {
            if (dayLengthInSeconds <= 0) return;
            float dayProgress = deltaTime / dayLengthInSeconds;
            currentTime01 += dayProgress;

            // בדיקה האם התחיל יום חדש
            if (currentTime01 >= 1f)
            {
                currentTime01 -= 1f;
                dayCount++;
                OnNewDay?.Invoke(dayCount);
                ResetDailyEvents(); // לאתחל flags של אירועי ביניים

                if (useSeasons)
                {
                    UpdateSeason();
                }
            }

            // טריגרים לחלקי היום
            CheckDailyEvents();
        }

        private void UpdateSeason()
        {
            float totalDaysPassed = dayCount;
            float cycleLength = seasonLengthInDays * 4f;
            float cyclePosition = totalDaysPassed % cycleLength;

            float span = seasonLengthInDays;
            if (cyclePosition < span)
            {
                if (currentSeason != Season.Spring)
                {
                    currentSeason = Season.Spring;
                    OnSeasonChanged?.Invoke(currentSeason);
                }
            }
            else if (cyclePosition < span * 2f)
            {
                if (currentSeason != Season.Summer)
                {
                    currentSeason = Season.Summer;
                    OnSeasonChanged?.Invoke(currentSeason);
                }
            }
            else if (cyclePosition < span * 3f)
            {
                if (currentSeason != Season.Autumn)
                {
                    currentSeason = Season.Autumn;
                    OnSeasonChanged?.Invoke(currentSeason);
                }
            }
            else
            {
                if (currentSeason != Season.Winter)
                {
                    currentSeason = Season.Winter;
                    OnSeasonChanged?.Invoke(currentSeason);
                }
            }
        }

        private void CheckDailyEvents()
        {
            // לדוגמה, בוקר ~0.0-0.1
            if (!hasTriggeredMorning && currentTime01 >= 0.0f)
            {
                hasTriggeredMorning = true;
                OnMorning?.Invoke();
            }
            // צהריים ~0.5
            if (!hasTriggeredNoon && currentTime01 >= 0.5f)
            {
                hasTriggeredNoon = true;
                OnNoon?.Invoke();
            }
            // ערב ~0.8
            if (!hasTriggeredEvening && currentTime01 >= 0.8f)
            {
                hasTriggeredEvening = true;
                OnEvening?.Invoke();
            }
            // לילה ~0.9
            if (!hasTriggeredNight && currentTime01 >= 0.9f)
            {
                hasTriggeredNight = true;
                OnNight?.Invoke();
            }
        }

        private void ResetDailyEvents()
        {
            hasTriggeredMorning = false;
            hasTriggeredNoon = false;
            hasTriggeredEvening = false;
            hasTriggeredNight = false;
        }

        public float GetSunlightFactor()
        {
            // סינוס/קוסינוס כדי לדמות אורך אור
            float sunlight = Mathf.Cos((currentTime01 - 0.25f) * Mathf.PI * 2f) * 0.5f + 0.5f;
            return Mathf.Clamp01(sunlight);
        }
    }
    #endregion

    #region WeatherSystem Class (Advanced)
    [Serializable]
    public class WeatherSystem
    {
        public enum WeatherType
        {
            Clear,
            Rain,
            Snow,
            Foggy,
            Storm
        }

        [Header("Weather Settings")]
        public bool dynamicWeather = true;
        [Range(0f, 1f)]
        public float weatherChangeProbability = 0.01f;
        public WeatherType currentWeather = WeatherType.Clear;

        // אפשר להשתמש בפרופילים מורכבים (ScriptableObject) לכל WeatherType
        [Header("Weather Profiles (Optional)")]
        public List<WeatherProfile> weatherProfiles = new List<WeatherProfile>();

        [Header("Temperature Settings")]
        public float temperatureCelsius = 20f;
        public float minTemperature = -5f;
        public float maxTemperature = 35f;

        [Header("References for Visual Effects")]
        public ParticleSystem rainEffect;
        public ParticleSystem snowEffect;
        public ParticleSystem fogEffect;

        // אירועים
        public event Action<WeatherType> OnWeatherChanged;
        public event Action<float> OnTemperatureChanged;

        private System.Random _rnd = new System.Random();

        public void Init()
        {
            UpdateWeatherEffects();
        }

        public void UpdateWeather(float deltaTime, TimeSystem timeSys)
        {
            if (!dynamicWeather) return;

            // הגרלת שינוי מזג אוויר בהסתמך על התקדמות היום
            float dayProgress = deltaTime / timeSys.dayLengthInSeconds;
            float chance = weatherChangeProbability * dayProgress; 
            if ((float)_rnd.NextDouble() < chance)
            {
                RandomizeWeather(timeSys.currentSeason);
            }

            // עדכון טמפ'
            UpdateTemperature(timeSys);
        }

        private void RandomizeWeather(TimeSystem.Season season)
        {
            // אם יש WeatherProfiles, אפשר לבחור מהם לפי עונה
            var possibleWeathers = new List<WeatherType>();

            // אם אין הגדרה מפורשת, פשוט fallback
            if (weatherProfiles.Count == 0)
            {
                // לוגיקה בסיסית
                switch (season)
                {
                    case TimeSystem.Season.Spring:
                        possibleWeathers.Add(WeatherType.Clear);
                        possibleWeathers.Add(WeatherType.Rain);
                        possibleWeathers.Add(WeatherType.Foggy);
                        break;
                    case TimeSystem.Season.Summer:
                        possibleWeathers.Add(WeatherType.Clear);
                        possibleWeathers.Add(WeatherType.Storm);
                        break;
                    case TimeSystem.Season.Autumn:
                        possibleWeathers.Add(WeatherType.Clear);
                        possibleWeathers.Add(WeatherType.Rain);
                        possibleWeathers.Add(WeatherType.Foggy);
                        break;
                    case TimeSystem.Season.Winter:
                        possibleWeathers.Add(WeatherType.Snow);
                        possibleWeathers.Add(WeatherType.Clear);
                        possibleWeathers.Add(WeatherType.Storm);
                        break;
                }
                int idx = _rnd.Next(possibleWeathers.Count);
                SetWeather(possibleWeathers[idx]);
            }
            else
            {
                // משתמשים בפרופילים
                var filteredProfiles = weatherProfiles.FindAll(p => p.seasons.Contains(season));
                if (filteredProfiles.Count == 0)
                {
                    // אם אין התאמה לעונה, fallback למשהו
                    SetWeather(WeatherType.Clear);
                }
                else
                {
                    int idx = _rnd.Next(filteredProfiles.Count);
                    SetWeather(filteredProfiles[idx].weatherType);
                }
            }
        }

        public void SetWeather(WeatherType newType)
        {
            if (currentWeather == newType) return;
            currentWeather = newType;
            UpdateWeatherEffects();
            OnWeatherChanged?.Invoke(currentWeather);
        }

        public void UpdateWeatherEffects()
        {
            if (rainEffect) rainEffect.Stop();
            if (snowEffect) snowEffect.Stop();
            if (fogEffect) fogEffect.Stop();

            switch (currentWeather)
            {
                case WeatherType.Clear:
                    break;
                case WeatherType.Rain:
                    if (rainEffect) rainEffect.Play();
                    break;
                case WeatherType.Snow:
                    if (snowEffect) snowEffect.Play();
                    break;
                case WeatherType.Foggy:
                    if (fogEffect) fogEffect.Play();
                    break;
                case WeatherType.Storm:
                    // סופה => גשם חזק וכו’
                    if (rainEffect) rainEffect.Play();
                    break;
            }
        }

        private void UpdateTemperature(TimeSystem timeSys)
        {
            // בסיס לפי העונה
            float seasonBase = 20f;
            switch (timeSys.currentSeason)
            {
                case TimeSystem.Season.Spring:
                    seasonBase = 18f;
                    break;
                case TimeSystem.Season.Summer:
                    seasonBase = 30f;
                    break;
                case TimeSystem.Season.Autumn:
                    seasonBase = 15f;
                    break;
                case TimeSystem.Season.Winter:
                    seasonBase = 0f;
                    break;
            }

            // השפעת מזג אוויר
            float weatherOffset = 0f;
            switch (currentWeather)
            {
                case WeatherType.Rain:   weatherOffset -= 2f; break;
                case WeatherType.Snow:   weatherOffset -= 5f; break;
                case WeatherType.Foggy:  weatherOffset -= 1f; break;
                case WeatherType.Storm:  weatherOffset -= 3f; break;
            }

            // השפעת יום/לילה
            float sunlight = timeSys.GetSunlightFactor(); // 0=לילה, 1=צהריים
            float dayNightOffset = Mathf.Lerp(-5f, 3f, sunlight);

            float newTemp = seasonBase + weatherOffset + dayNightOffset;
            newTemp = Mathf.Clamp(newTemp, minTemperature, maxTemperature);

            // עדכון
            if (Mathf.Abs(newTemp - temperatureCelsius) > 0.5f)
            {
                temperatureCelsius = newTemp;
                OnTemperatureChanged?.Invoke(temperatureCelsius);
            }
            else
            {
                // מעבר הדרגתי
                temperatureCelsius = Mathf.Lerp(temperatureCelsius, newTemp, 0.01f);
            }
        }
    }
    #endregion

    #region EcologySystem Class (Advanced)
    [Serializable]
    public class EcologySystem
    {
        [Header("Resource Nodes")]
        public List<ResourceNode> resourceNodes = new List<ResourceNode>();

        [Header("Soil Moisture (0..1)")]
        [Range(0f,1f)]
        public float soilMoisture = 0.5f;   // לחות קרקע התחלתית

        [Header("Growth Settings")]
        [Tooltip("כל כמה זמן (שניות משחק) נבדוק צמיחה מחודשת.")]
        public float growthCheckInterval = 10f;
        private float growthTimer = 0f;

        [Tooltip("מכפיל גדילה כללי. ערך >1 => גדילה מהירה יותר.")]
        [Range(0f, 2f)]
        public float growthRateMultiplier = 1.0f;

        [Header("Population Control")]
        public float globalResourceLimit = 999999f;

        private System.Random _rnd = new System.Random();

        public void Init()
        {
            // אפשר לטעון ResourceNodes מסצנה או להשאירם מוכתבים ידנית
        }

        public void UpdateEcology(float deltaTime, WeatherSystem weatherSys)
        {
            UpdateSoilMoisture(deltaTime, weatherSys);
            growthTimer += deltaTime;
            if (growthTimer >= growthCheckInterval)
            {
                growthTimer = 0f;
                PerformGrowthCheck(weatherSys);
            }
        }

        private void UpdateSoilMoisture(float deltaTime, WeatherSystem weatherSys)
        {
            // אם יש גשם/סופה => להעלות soilMoisture בהדרגה
            if (weatherSys.currentWeather == WeatherSystem.WeatherType.Rain ||
                weatherSys.currentWeather == WeatherSystem.WeatherType.Storm)
            {
                soilMoisture = Mathf.Clamp01(soilMoisture + 0.05f * deltaTime);
            }
            else if (weatherSys.currentWeather == WeatherSystem.WeatherType.Snow)
            {
                // שלג יעלה לחות לאט יותר (נמס בהדרגה)
                soilMoisture = Mathf.Clamp01(soilMoisture + 0.02f * deltaTime);
            }
            else
            {
                // Clear/Foggy => אובדן לחות קל
                soilMoisture = Mathf.Clamp01(soilMoisture - 0.01f * deltaTime);
            }
        }

        private void PerformGrowthCheck(WeatherSystem weatherSys)
        {
            foreach (var node in resourceNodes)
            {
                switch (node.resourceType)
                {
                    case ResourceNode.ResourceType.Plant:
                        GrowPlant(node, weatherSys);
                        break;
                    case ResourceNode.ResourceType.Water:
                        // אם יש גשם/סופה => למלא מעט מים (כמו אגם/באר)
                        if (weatherSys.currentWeather == WeatherSystem.WeatherType.Rain ||
                            weatherSys.currentWeather == WeatherSystem.WeatherType.Storm)
                        {
                            node.currentQuantity = Mathf.Min(node.currentQuantity + 2, node.maxQuantity);
                        }
                        break;
                    case ResourceNode.ResourceType.Food:
                    case ResourceNode.ResourceType.Other:
                        // מפושט
                        break;
                }
            }
        }

        private void GrowPlant(ResourceNode node, WeatherSystem weatherSys)
        {
            // הסתברות בסיסית
            float growthChance = 0.5f * growthRateMultiplier;

            // השפעת מזג אוויר
            if (weatherSys.currentWeather == WeatherSystem.WeatherType.Rain ||
                weatherSys.currentWeather == WeatherSystem.WeatherType.Storm)
            {
                growthChance += 0.3f;
            }
            else if (weatherSys.currentWeather == WeatherSystem.WeatherType.Snow)
            {
                growthChance -= 0.2f;
            }

            // השפעת טמפרטורה (אם מתחת לאפס => קשה לצמוח)
            if (weatherSys.temperatureCelsius < 0f)
            {
                growthChance -= 0.3f;
            }

            // השפעת soilMoisture
            // אם לחות קרקע גבוהה => סיכוי צמיחה עולה
            float moistureBonus = Mathf.Clamp01(soilMoisture) - 0.5f;
            growthChance += moistureBonus * 0.4f; // למקד אפקט

            float roll = (float)_rnd.NextDouble();
            if (roll < Mathf.Clamp01(growthChance))
            {
                node.currentQuantity = Mathf.Min(node.currentQuantity + 1, node.maxQuantity);
            }
        }
    }
    #endregion

    #region DisasterSystem Class
    [Serializable]
    public class DisasterSystem
    {
        [Header("Disaster Settings")]
        public bool enableDisasters = true;
        [Range(0f,1f)]
        public float dailyDisasterChance = 0.01f; // סיכוי 1% ל"אסון" כל יום

        // אפשר להצמיד סכנות שונות (אולי ScriptableObject)
        public List<DisasterDefinition> possibleDisasters = new List<DisasterDefinition>();

        private System.Random _rnd = new System.Random();
        private int lastDayChecked = -1;

        public event Action<DisasterDefinition> OnDisasterTriggered;

        public void Init()
        {
        }

        /// <summary>
        /// בודק אם הגיע זמן לאסון (פעם ביום, בסוף יום).
        /// יכול לקרוא גם לכמה עדכונים (למשל בכל frame) - עניין של עיצוב.
        /// </summary>
        public void UpdateDisasters(float deltaTime, 
            WorldManager.TimeSystem timeSys, 
            WorldManager.WeatherSystem weatherSys,
            WorldManager.EcologySystem ecologySys)
        {
            if (!enableDisasters) return;
            if (timeSys.dayCount != lastDayChecked)
            {
                lastDayChecked = timeSys.dayCount;
                float roll = (float)_rnd.NextDouble();
                if (roll < dailyDisasterChance)
                {
                    TriggerRandomDisaster(timeSys, weatherSys, ecologySys);
                }
            }
        }

        private void TriggerRandomDisaster(TimeSystem timeSys, WeatherSystem weatherSys, EcologySystem ecologySys)
        {
            if (possibleDisasters.Count == 0) return;

            int idx = _rnd.Next(possibleDisasters.Count);
            var disaster = possibleDisasters[idx];
            Debug.Log($"DisasterSystem: Triggering disaster {disaster.disasterName}!");
            OnDisasterTriggered?.Invoke(disaster);

            // החלת האפקט
            disaster.ApplyEffect(timeSys, weatherSys, ecologySys);
        }
    }

    internal class Season
    {
    }
    #endregion
}

/***************************************************************
 * ResourceNode.cs
 *  ייצוג נקודת משאב בעולם (Plant, Water, etc.).
 *  אפשר לשים את זה בקובץ נפרד או באותו קובץ. 
 ***************************************************************/
[System.Serializable]
public class ResourceNode
{
    public enum ResourceType
    {
        Plant,
        Water,
        Food,
        Other
    }    public ResourceType resourceType;
    public int currentQuantity = 10;
    public int maxQuantity = 20;
}

/***************************************************************
 * DisasterDefinition.cs
 *  ייצוג "אסון" אפשרי (למשל Class ScriptableObject),
 *  אפשר להרחיב ב-Editor ולתת השפעות שונות.
 ***************************************************************/
[System.Serializable]
public class DisasterDefinition
{
    public string disasterName = "Locust Swarm";  // לדוגמה מכת ארבה
    public float severity = 1f;                  // עוצמה
    // וכו'

    /// <summary>
    /// מפעיל את האפקט ההרסני על הסביבה
    /// </summary>
    public void ApplyEffect(WorldManager.TimeSystem timeSys,
                            WorldManager.WeatherSystem weatherSys,
                            WorldManager.EcologySystem ecologySys)
    {
        // לדוגמה: פוגע בכל הצמחים
        foreach (var node in ecologySys.resourceNodes)
        {
            if (node.resourceType == ResourceNode.ResourceType.Plant)
            {
                // הורדת כמות בשיעור severity
                int loss = Mathf.RoundToInt(5f * severity);
                node.currentQuantity = Mathf.Max(node.currentQuantity - loss, 0);
            }
        }
    }
}

/***************************************************************
 *  WeatherProfile.cs (ScriptableObject אופציונלי)
 *  מאפשר להגדיר פרופיל מזג אוויר לעונה מסוימת + הגדרות נוספות
 ***************************************************************/


[CreateAssetMenu(fileName="NewWeatherProfile", menuName="Minipoll/WeatherProfile", order=1)]
public class WeatherProfile : ScriptableObject
{
    public WorldManager.WeatherSystem.WeatherType weatherType;
    public List<WorldManager.TimeSystem.Season> seasons;
    // אפשר להוסיף עוד נתונים ל-Weather (רמת משקעים, עוצמת רוח, וכו')
}
