using UnityEngine;
using MinipollGame.Managers;

namespace MinipollGame.World
{
    /// <summary>
    /// Time management system for the Minipoll world - Updated
    /// </summary>
    public class MinipollTimeSystem : MonoBehaviour
    {
        [Header("Time Settings")]
        public float timeScale = 1f;
        public float dayDuration = 300f; // 5 minutes real time = 1 day game time
        public float yearDuration = 7300f; // Roughly 20 days real time = 1 year game time
        
        [Header("Day/Night Cycle")]
        public bool enableDayNightCycle = true;
        public AnimationCurve lightIntensityCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 1f);
        public Gradient dayNightColorGradient;
        public Light sunLight;
        public Light moonLight;
        
        [Header("Seasonal Changes")]
        public bool enableSeasons = true;
        public AnimationCurve temperatureCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public float baseTemperature = 20f;
        public float temperatureVariation = 15f;
        
        [Header("Events")]
        public UnityEngine.Events.UnityEvent<int> OnDayChanged;
        public UnityEngine.Events.UnityEvent<Season> OnSeasonChanged;
        public UnityEngine.Events.UnityEvent<int> OnYearChanged;
        public UnityEngine.Events.UnityEvent<float> OnTimeOfDayChanged;

        // Time tracking
        private float currentTimeOfDay = 0.5f; // Start at noon
        private int currentDay = 1;
        private int currentYear = 1;
        private Season currentSeason = Season.Spring;
        
        // System references (WeatherSystem will be added later)
        // private WeatherSystem weatherSystem;
        private WorldManager worldManager;
        private GameManager gameManager;
        
        // Properties
        public float CurrentTimeOfDay => currentTimeOfDay;
        public int CurrentDay => currentDay;
        public int CurrentYear => currentYear;
        public Season CurrentSeason => currentSeason;
        public float CurrentTemperature => CalculateCurrentTemperature();
        public bool IsDay => currentTimeOfDay >= 0.25f && currentTimeOfDay <= 0.75f;
        public bool IsNight => !IsDay;

        public enum Season
        {
            Spring,
            Summer,
            Autumn,
            Winter
        }

        private void Awake()
        {
            // weatherSystem = FindObjectOfType<WeatherSystem>();
            worldManager = FindObjectOfType<WorldManager>();
            gameManager = FindObjectOfType<GameManager>();
        }

        private void Start()
        {
            InitializeTimeSystem();
        }

        private void Update()
        {
            if (gameManager != null && gameManager.CurrentState == GameManager.GameState.Paused)
                return;

            UpdateTime();
            
            if (enableDayNightCycle)
                UpdateDayNightCycle();
        }

        /// <summary>
        /// Initialize the time system
        /// </summary>
        private void InitializeTimeSystem()
        {
            // Set initial lighting
            if (enableDayNightCycle)
            {
                UpdateDayNightCycle();
            }
            
            // Initialize events
            OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
            OnDayChanged?.Invoke(currentDay);
            OnSeasonChanged?.Invoke(currentSeason);
            OnYearChanged?.Invoke(currentYear);
        }

        /// <summary>
        /// Update the game time
        /// </summary>
        private void UpdateTime()
        {
            float timeIncrement = (Time.deltaTime * timeScale) / dayDuration;
            currentTimeOfDay += timeIncrement;
            
            OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
            
            // Check for day change
            if (currentTimeOfDay >= 1f)
            {
                currentTimeOfDay -= 1f;
                AdvanceDay();
            }
        }

        /// <summary>
        /// Advance to the next day
        /// </summary>
        private void AdvanceDay()
        {
            currentDay++;
            OnDayChanged?.Invoke(currentDay);
            
            // Check for season change (every 90 days)
            if (currentDay % 90 == 0)
            {
                AdvanceSeason();
            }
            
            // Check for year change (every 360 days)
            if (currentDay % 360 == 0)
            {
                AdvanceYear();
            }
            
            // Notify weather system of new day
            // if (weatherSystem != null)
            //     weatherSystem.OnNewDay();
        }

        /// <summary>
        /// Advance to the next season
        /// </summary>
        private void AdvanceSeason()
        {
            currentSeason = (Season)(((int)currentSeason + 1) % 4);
            OnSeasonChanged?.Invoke(currentSeason);
            
            // Notify weather system of season change
            // if (weatherSystem != null)
            //     weatherSystem.OnSeasonChanged(currentSeason);
        }

        /// <summary>
        /// Advance to the next year
        /// </summary>
        private void AdvanceYear()
        {
            currentYear++;
            OnYearChanged?.Invoke(currentYear);
        }

        /// <summary>
        /// Update day/night lighting cycle
        /// </summary>
        private void UpdateDayNightCycle()
        {
            if (sunLight == null) return;
            
            // Calculate sun angle (0-360 degrees)
            float sunAngle = (currentTimeOfDay - 0.25f) * 360f;
            
            // Rotate sun light
            sunLight.transform.rotation = Quaternion.Euler(sunAngle - 90f, 30f, 0f);
            
            // Adjust light intensity based on time of day
            float intensityFactor = lightIntensityCurve.Evaluate(currentTimeOfDay);
            sunLight.intensity = intensityFactor;
            
            // Adjust light color
            if (dayNightColorGradient != null && dayNightColorGradient.colorKeys.Length > 0)
            {
                sunLight.color = dayNightColorGradient.Evaluate(currentTimeOfDay);
            }
            
            // Handle moon light
            if (moonLight != null)
            {
                moonLight.gameObject.SetActive(IsNight);
                if (IsNight)
                {
                    float moonAngle = sunAngle + 180f;
                    moonLight.transform.rotation = Quaternion.Euler(moonAngle - 90f, 30f, 0f);
                    moonLight.intensity = (1f - intensityFactor) * 0.5f;
                }
            }
            
            // Update ambient lighting
            RenderSettings.ambientIntensity = Mathf.Lerp(0.3f, 1f, intensityFactor);
        }

        /// <summary>
        /// Calculate current temperature based on season and time
        /// </summary>
        private float CalculateCurrentTemperature()
        {
            float seasonalFactor = GetSeasonalTemperatureFactor();
            float dailyFactor = GetDailyTemperatureFactor();
            
            return baseTemperature + (seasonalFactor * temperatureVariation) + (dailyFactor * 5f);
        }

        /// <summary>
        /// Get seasonal temperature factor (-1 to 1)
        /// </summary>
        private float GetSeasonalTemperatureFactor()
        {
            switch (currentSeason)
            {
                case Season.Spring: return 0f;
                case Season.Summer: return 1f;
                case Season.Autumn: return 0f;
                case Season.Winter: return -1f;
                default: return 0f;
            }
        }

        /// <summary>
        /// Get daily temperature factor (-1 to 1)
        /// </summary>
        private float GetDailyTemperatureFactor()
        {
            // Temperature peaks at 14:00 (0.583 of day) and lowest at 6:00 (0.25 of day)
            float adjustedTime = (currentTimeOfDay + 0.25f) % 1f;
            return Mathf.Sin(adjustedTime * 2f * Mathf.PI) * 0.3f;
        }

        /// <summary>
        /// Set the time scale
        /// </summary>
        public void SetTimeScale(float newTimeScale)
        {
            timeScale = Mathf.Max(0f, newTimeScale);
        }

        /// <summary>
        /// Pause time
        /// </summary>
        public void PauseTime()
        {
            timeScale = 0f;
        }

        /// <summary>
        /// Resume time
        /// </summary>
        public void ResumeTime()
        {
            timeScale = 1f;
        }

        /// <summary>
        /// Set specific time of day (0-1)
        /// </summary>
        public void SetTimeOfDay(float timeOfDay)
        {
            currentTimeOfDay = Mathf.Clamp01(timeOfDay);
            OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
            
            if (enableDayNightCycle)
                UpdateDayNightCycle();
        }

        /// <summary>
        /// Set specific day
        /// </summary>
        public void SetDay(int day)
        {
            currentDay = Mathf.Max(1, day);
            
            // Recalculate season and year
            currentSeason = (Season)((currentDay / 90) % 4);
            currentYear = (currentDay / 360) + 1;
            
            OnDayChanged?.Invoke(currentDay);
            OnSeasonChanged?.Invoke(currentSeason);
            OnYearChanged?.Invoke(currentYear);
        }

        /// <summary>
        /// Get formatted time string
        /// </summary>
        public string GetFormattedTime()
        {
            int hours = Mathf.FloorToInt(currentTimeOfDay * 24f);
            int minutes = Mathf.FloorToInt((currentTimeOfDay * 24f - hours) * 60f);
            return $"{hours:00}:{minutes:00}";
        }

        /// <summary>
        /// Get formatted date string
        /// </summary>
        public string GetFormattedDate()
        {
            int dayInSeason = currentDay % 90;
            return $"Day {dayInSeason} of {currentSeason}, Year {currentYear}";
        }

        /// <summary>
        /// Get time data for saving/loading
        /// </summary>
        public TimeData GetTimeData()
        {
            return new TimeData
            {
                timeOfDay = currentTimeOfDay,
                day = currentDay,
                year = currentYear,
                season = currentSeason,
                timeScale = timeScale
            };
        }

        /// <summary>
        /// Load time data
        /// </summary>
        public void LoadTimeData(TimeData timeData)
        {
            currentTimeOfDay = timeData.timeOfDay;
            currentDay = timeData.day;
            currentYear = timeData.year;
            currentSeason = timeData.season;
            timeScale = timeData.timeScale;
            
            InitializeTimeSystem();
        }

        [System.Serializable]
        public class TimeData
        {
            public float timeOfDay;
            public int day;
            public int year;
            public Season season;
            public float timeScale;
        }
    }
}
