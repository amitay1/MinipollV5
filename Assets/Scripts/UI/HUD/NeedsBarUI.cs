using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MinipollGame.Core;
using MinipollGame.Managers;

namespace MinipollGame.UI.HUD
{
    /// <summary>
    /// Displays needs bars for the selected Minipoll
    /// </summary>
    public class NeedsBarUI : MonoBehaviour
    {
        [Header("UI References")]
        public Transform needBarsContainer;
        public GameObject needBarPrefab;
        
        [Header("Need Bar Settings")]
        public Color healthyColor = Color.green;
        public Color warningColor = Color.yellow;
        public Color criticalColor = Color.red;
        
        [Header("Update Settings")]
        public float updateInterval = 0.5f; // Update every 0.5 seconds
        
        private MinipollGame.Core.MinipollCore currentMinipoll;
        private Dictionary<string, NeedBarItem> needBars = new Dictionary<string, NeedBarItem>();
        private float lastUpdateTime;
        private UIManager uiManager;

        [System.Serializable]
        public class NeedBarItem
        {
            public GameObject gameObject;
            public TextMeshProUGUI nameText;
            public Slider valueSlider;
            public Image fillImage;
            public TextMeshProUGUI valueText;
        }

        private void Awake()
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        private void Start()
        {
            // Subscribe to minipoll selection events
            if (uiManager != null)
            {
                // Assuming UIManager has these events
                // uiManager.OnMinipollSelected += OnMinipollSelected;
                // uiManager.OnMinipollDeselected += OnMinipollDeselected;
            }
            
            // Initially hide the needs display
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (currentMinipoll != null && Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateNeedBars();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// Set the Minipoll to display needs for
        /// </summary>
        public void SetMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            currentMinipoll = minipoll;
            
            if (currentMinipoll == null)
            {
                HideNeedsDisplay();
                return;
            }
            
            ShowNeedsDisplay();
            CreateNeedBars();
            UpdateNeedBars();
        }

        /// <summary>
        /// Show the needs display
        /// </summary>
        public void ShowNeedsDisplay()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the needs display
        /// </summary>
        public void HideNeedsDisplay()
        {
            gameObject.SetActive(false);
            ClearNeedBars();
        }

        /// <summary>
        /// Create need bars for the current Minipoll
        /// </summary>
        private void CreateNeedBars()
        {
            if (currentMinipoll == null || currentMinipoll.NeedsSystem == null) return;
            
            ClearNeedBars();
            
            var needsSystem = currentMinipoll.NeedsSystem;
            
            // Create bars for each need
            CreateNeedBar("Energy", needsSystem.energy);
            CreateNeedBar("Hunger", needsSystem.hunger);
            CreateNeedBar("Social", needsSystem.social);
            CreateNeedBar("Fun", needsSystem.fun);
            CreateNeedBar("Hygiene", needsSystem.hygiene);
        }

        /// <summary>
        /// Create a single need bar
        /// </summary>
        private void CreateNeedBar(string needName, Need need)
        {
            if (needBarPrefab == null || needBarsContainer == null) return;

            GameObject barObject = Instantiate(needBarPrefab, needBarsContainer);
            NeedBarItem barItem = new NeedBarItem();
            barItem.gameObject = barObject;
            
            // Find components in the prefab
            barItem.nameText = barObject.GetComponentInChildren<TextMeshProUGUI>();
            barItem.valueSlider = barObject.GetComponentInChildren<Slider>();
            
            if (barItem.valueSlider != null)
            {
                barItem.fillImage = barItem.valueSlider.fillRect?.GetComponent<Image>();
                
                // Look for value text (might be a child of the slider)
                var textComponents = barObject.GetComponentsInChildren<TextMeshProUGUI>();
                if (textComponents.Length > 1)
                    barItem.valueText = textComponents[1]; // First is name, second might be value
            }

            // Set up the bar
            if (barItem.nameText != null)
                barItem.nameText.text = needName;

            needBars[needName] = barItem;
        }

        /// <summary>
        /// Update all need bars with current values
        /// </summary>
        private void UpdateNeedBars()
        {
            if (currentMinipoll == null || currentMinipoll.NeedsSystem == null) return;

            var needsSystem = currentMinipoll.NeedsSystem;
            
            UpdateNeedBar("Energy", needsSystem.energy);
            UpdateNeedBar("Hunger", needsSystem.hunger);
            UpdateNeedBar("Social", needsSystem.social);
            UpdateNeedBar("Fun", needsSystem.fun);
            UpdateNeedBar("Hygiene", needsSystem.hygiene);
        }

        /// <summary>
        /// Update a single need bar
        /// </summary>
        private void UpdateNeedBar(string needName, Need need)
        {
            if (!needBars.ContainsKey(needName)) return;

            var barItem = needBars[needName];
            float normalizedValue = need.currentValue / 100f; // Assuming needs are 0-100
            
            // Update slider value
            if (barItem.valueSlider != null)
                barItem.valueSlider.value = normalizedValue;
            
            // Update value text
            if (barItem.valueText != null)
                barItem.valueText.text = $"{need.currentValue:F0}/100";
            
            // Update color based on value
            if (barItem.fillImage != null)
            {
                Color barColor = GetNeedColor(need.currentValue, need.criticalThreshold);
                barItem.fillImage.color = barColor;
            }
        }

        /// <summary>
        /// Get the appropriate color for a need value
        /// </summary>
        private Color GetNeedColor(float currentValue, float criticalThreshold)
        {
            if (currentValue <= criticalThreshold)
                return criticalColor;
            else if (currentValue <= criticalThreshold * 2f)
                return warningColor;
            else
                return healthyColor;
        }

        /// <summary>
        /// Clear all need bars
        /// </summary>
        private void ClearNeedBars()
        {
            foreach (var barItem in needBars.Values)
            {
                if (barItem.gameObject != null)
                    Destroy(barItem.gameObject);
            }
            needBars.Clear();
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

        private void OnDestroy()
        {
            ClearNeedBars();
        }
    }
}
