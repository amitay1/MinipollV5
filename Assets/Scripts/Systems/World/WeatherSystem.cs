using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinipollGame.Systems.World
{
    /// <summary>
    /// Standalone Weather System component that manages weather effects and temperature
    /// </summary>
    public class WeatherSystem : MonoBehaviour
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

        [Header("Temperature Settings")]
        public float temperatureCelsius = 20f;
        public float minTemperature = -5f;
        public float maxTemperature = 35f;

        [Header("References for Visual Effects")]
        public ParticleSystem rainEffect;
        public ParticleSystem snowEffect;
        public ParticleSystem fogEffect;

        // Events
        public event Action<WeatherType> OnWeatherChanged;
        public event Action<float> OnTemperatureChanged;

        private System.Random _rnd = new System.Random();
        private WorldManager worldManager;

        private void Awake()
        {
            worldManager = WorldManager.Instance;
        }

        private void Start()
        {
            UpdateWeatherEffects();
        }

        private void Update()
        {
            if (!dynamicWeather || worldManager == null) return;

            float deltaTime = Time.deltaTime;
            
            // Random weather changes
            float dayProgress = deltaTime / (worldManager.timeSystem.dayLengthInSeconds > 0 ? worldManager.timeSystem.dayLengthInSeconds : 300f);
            float chance = weatherChangeProbability * dayProgress;
            
            if ((float)_rnd.NextDouble() < chance)
            {
                RandomizeWeather(worldManager.timeSystem.currentSeason);
            }

            // Update temperature
            UpdateTemperature();
        }

        private void RandomizeWeather(WorldManager.TimeSystem.Season season)
        {
            var possibleWeathers = new List<WeatherType>();

            switch (season)
            {
                case WorldManager.TimeSystem.Season.Spring:
                    possibleWeathers.Add(WeatherType.Clear);
                    possibleWeathers.Add(WeatherType.Rain);
                    possibleWeathers.Add(WeatherType.Foggy);
                    break;
                case WorldManager.TimeSystem.Season.Summer:
                    possibleWeathers.Add(WeatherType.Clear);
                    possibleWeathers.Add(WeatherType.Storm);
                    break;
                case WorldManager.TimeSystem.Season.Autumn:
                    possibleWeathers.Add(WeatherType.Clear);
                    possibleWeathers.Add(WeatherType.Rain);
                    possibleWeathers.Add(WeatherType.Foggy);
                    break;
                case WorldManager.TimeSystem.Season.Winter:
                    possibleWeathers.Add(WeatherType.Snow);
                    possibleWeathers.Add(WeatherType.Clear);
                    possibleWeathers.Add(WeatherType.Storm);
                    break;
            }

            int idx = _rnd.Next(possibleWeathers.Count);
            SetWeather(possibleWeathers[idx]);
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
                    if (rainEffect) rainEffect.Play();
                    break;
            }
        }

        private void UpdateTemperature()
        {
            if (worldManager == null) return;

            // Base temperature by season
            float seasonBase = 20f;
            switch (worldManager.timeSystem.currentSeason)
            {
                case WorldManager.TimeSystem.Season.Spring:
                    seasonBase = 18f;
                    break;
                case WorldManager.TimeSystem.Season.Summer:
                    seasonBase = 30f;
                    break;
                case WorldManager.TimeSystem.Season.Autumn:
                    seasonBase = 15f;
                    break;
                case WorldManager.TimeSystem.Season.Winter:
                    seasonBase = 0f;
                    break;
            }

            // Weather influence
            float weatherOffset = 0f;
            switch (currentWeather)
            {
                case WeatherType.Rain: weatherOffset -= 2f; break;
                case WeatherType.Snow: weatherOffset -= 5f; break;
                case WeatherType.Foggy: weatherOffset -= 1f; break;
                case WeatherType.Storm: weatherOffset -= 3f; break;
            }

            // Day/night influence
            float sunlight = worldManager.timeSystem.GetSunlightFactor();
            float dayNightOffset = Mathf.Lerp(-5f, 3f, sunlight);

            float newTemp = seasonBase + weatherOffset + dayNightOffset;
            newTemp = Mathf.Clamp(newTemp, minTemperature, maxTemperature);

            // Update temperature
            if (Mathf.Abs(newTemp - temperatureCelsius) > 0.5f)
            {
                temperatureCelsius = newTemp;
                OnTemperatureChanged?.Invoke(temperatureCelsius);
            }
            else
            {
                temperatureCelsius = Mathf.Lerp(temperatureCelsius, newTemp, 0.01f);
            }
        }

        public WeatherType GetCurrentWeather()
        {
            return currentWeather;
        }

        public float GetTemperature()
        {
            return temperatureCelsius;
        }
    }
}
