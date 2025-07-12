using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MinipollGame.Core;
using MinipollGame.Managers;

namespace MinipollGame.UI.HUD
{
    /// <summary>
    /// Displays emotion indicators for the selected Minipoll
    /// </summary>
    public class EmotionsDisplay : MonoBehaviour
    {
        [Header("UI References")]
        public Transform emotionsContainer;
        public GameObject emotionIndicatorPrefab;
        
        [Header("Display Settings")]
        public int maxEmotionsToShow = 5;
        public float updateInterval = 1f;
        public bool showOnlyDominantEmotion = false;
        
        [Header("Visual Settings")]
        public Color positiveEmotionColor = Color.green;
        public Color negativeEmotionColor = Color.red;
        public Color neutralEmotionColor = Color.yellow;
        
        private MinipollGame.Core.MinipollCore currentMinipoll;
        private List<EmotionIndicatorItem> emotionIndicators = new List<EmotionIndicatorItem>();
        private float lastUpdateTime;
        private UIManager uiManager;

        [System.Serializable]
        public class EmotionIndicatorItem
        {
            public GameObject gameObject;
            public TextMeshProUGUI emotionNameText;
            public Slider intensitySlider;
            public Image backgroundImage;
            public Image iconImage;
        }

        private void Awake()
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        private void Start()
        {
            // Initially hide the emotions display
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (currentMinipoll != null && Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateEmotionDisplay();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// Set the Minipoll to display emotions for
        /// </summary>
        public void SetMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            currentMinipoll = minipoll;
            
            if (currentMinipoll == null)
            {
                HideEmotionsDisplay();
                return;
            }
            
            ShowEmotionsDisplay();
            CreateEmotionIndicators();
            UpdateEmotionDisplay();
        }

        /// <summary>
        /// Show the emotions display
        /// </summary>
        public void ShowEmotionsDisplay()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the emotions display
        /// </summary>
        public void HideEmotionsDisplay()
        {
            gameObject.SetActive(false);
            ClearEmotionIndicators();
        }

        /// <summary>
        /// Create emotion indicator UI elements
        /// </summary>
        private void CreateEmotionIndicators()
        {
            if (currentMinipoll == null || currentMinipoll.Emotions == null) return;
            
            ClearEmotionIndicators();
            
            if (showOnlyDominantEmotion)
            {
                CreateSingleEmotionIndicator();
            }
            else
            {
                CreateMultipleEmotionIndicators();
            }
        }

        /// <summary>
        /// Create a single indicator for the dominant emotion
        /// </summary>
        private void CreateSingleEmotionIndicator()
        {
            if (emotionIndicatorPrefab == null || emotionsContainer == null) return;

            GameObject indicatorObject = Instantiate(emotionIndicatorPrefab, emotionsContainer);
            EmotionIndicatorItem indicator = SetupEmotionIndicator(indicatorObject);
            emotionIndicators.Add(indicator);
        }

        /// <summary>
        /// Create multiple indicators for different emotions
        /// </summary>
        private void CreateMultipleEmotionIndicators()
        {
            if (emotionIndicatorPrefab == null || emotionsContainer == null) return;

            // Create indicators up to the maximum limit
            for (int i = 0; i < maxEmotionsToShow; i++)
            {
                GameObject indicatorObject = Instantiate(emotionIndicatorPrefab, emotionsContainer);
                EmotionIndicatorItem indicator = SetupEmotionIndicator(indicatorObject);
                emotionIndicators.Add(indicator);
                
                // Initially hide the indicator
                indicatorObject.SetActive(false);
            }
        }

        /// <summary>
        /// Setup an emotion indicator with component references
        /// </summary>
        private EmotionIndicatorItem SetupEmotionIndicator(GameObject indicatorObject)
        {
            EmotionIndicatorItem indicator = new EmotionIndicatorItem();
            indicator.gameObject = indicatorObject;
            
            // Find components in the prefab
            indicator.emotionNameText = indicatorObject.GetComponentInChildren<TextMeshProUGUI>();
            indicator.intensitySlider = indicatorObject.GetComponentInChildren<Slider>();
            indicator.backgroundImage = indicatorObject.GetComponent<Image>();
            
            // Look for icon image (might be a child)
            var images = indicatorObject.GetComponentsInChildren<Image>();
            if (images.Length > 1)
                indicator.iconImage = images[1]; // First is background, second might be icon
            
            return indicator;
        }

        /// <summary>
        /// Update emotion display with current values
        /// </summary>
        private void UpdateEmotionDisplay()
        {
            if (currentMinipoll == null || currentMinipoll.Emotions == null) return;

            if (showOnlyDominantEmotion)
            {
                UpdateDominantEmotionDisplay();
            }
            else
            {
                UpdateMultipleEmotionsDisplay();
            }
        }

        /// <summary>
        /// Update display for dominant emotion only
        /// </summary>
        private void UpdateDominantEmotionDisplay()
        {
            if (emotionIndicators.Count == 0) return;

            // For now, show a placeholder since GetDominantEmotion method doesn't exist yet
            var indicator = emotionIndicators[0];
            UpdateEmotionIndicator(indicator, "Happy", 0.8f);
        }

        /// <summary>
        /// Update display for multiple emotions
        /// </summary>
        private void UpdateMultipleEmotionsDisplay()
        {
            // For now, show placeholder emotions since GetAllEmotions method doesn't exist yet
            var placeholderEmotions = new[] {
                ("Happy", 0.8f),
                ("Excited", 0.6f),
                ("Content", 0.4f)
            };
            
            // Update indicators
            for (int i = 0; i < emotionIndicators.Count; i++)
            {
                var indicator = emotionIndicators[i];
                
                if (i < placeholderEmotions.Length)
                {
                    indicator.gameObject.SetActive(true);
                    UpdateEmotionIndicator(indicator, placeholderEmotions[i].Item1, placeholderEmotions[i].Item2);
                }
                else
                {
                    indicator.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Update a single emotion indicator
        /// </summary>
        private void UpdateEmotionIndicator(EmotionIndicatorItem indicator, string emotionName, float intensity)
        {
            // Update emotion name
            if (indicator.emotionNameText != null)
                indicator.emotionNameText.text = emotionName;
            
            // Update intensity slider
            if (indicator.intensitySlider != null)
                indicator.intensitySlider.value = intensity;
            
            // Update colors based on emotion type
            Color emotionColor = GetEmotionColor(emotionName);
            
            if (indicator.backgroundImage != null)
                indicator.backgroundImage.color = emotionColor;
            
            if (indicator.iconImage != null)
                indicator.iconImage.color = emotionColor;
        }

        /// <summary>
        /// Get the appropriate color for an emotion
        /// </summary>
        private Color GetEmotionColor(string emotionName)
        {
            // Categorize emotions as positive, negative, or neutral
            string emotion = emotionName.ToLower();
            
            // Positive emotions
            if (emotion.Contains("happy") || emotion.Contains("joy") || emotion.Contains("excited") || 
                emotion.Contains("content") || emotion.Contains("satisfied") || emotion.Contains("love"))
            {
                return positiveEmotionColor;
            }
            
            // Negative emotions
            if (emotion.Contains("angry") || emotion.Contains("sad") || emotion.Contains("fear") || 
                emotion.Contains("disgusted") || emotion.Contains("frustrated") || emotion.Contains("jealous"))
            {
                return negativeEmotionColor;
            }
            
            // Neutral or unknown emotions
            return neutralEmotionColor;
        }

        /// <summary>
        /// Clear all emotion indicators
        /// </summary>
        private void ClearEmotionIndicators()
        {
            foreach (var indicator in emotionIndicators)
            {
                if (indicator.gameObject != null)
                    Destroy(indicator.gameObject);
            }
            emotionIndicators.Clear();
        }

        /// <summary>
        /// Called when a Minipoll is selected
        /// </summary>
        public void OnMinipollSelected(MinipollGame.Core.MinipollCore minipoll)
        {
            SetMinipoll(minipoll);
        }

        /// <summary>
        /// Called when a Minipoll is deselected
        /// </summary>
        public void OnMinipollDeselected()
        {
            SetMinipoll(null);
        }

        /// <summary>
        /// Toggle between showing only dominant emotion or all emotions
        /// </summary>
        public void ToggleDisplayMode()
        {
            showOnlyDominantEmotion = !showOnlyDominantEmotion;
            
            if (currentMinipoll != null)
            {
                CreateEmotionIndicators();
                UpdateEmotionDisplay();
            }
        }

        private void OnDestroy()
        {
            ClearEmotionIndicators();
        }
    }
}
