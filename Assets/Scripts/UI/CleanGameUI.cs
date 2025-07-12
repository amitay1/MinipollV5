using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MinipollGame.Core;
using MinipollGame.Managers;

namespace MinipollGame.UI
{
    /// <summary>
    /// Clean, Modern Game UI - Single UI system for the main gameplay
    /// Replaces all other cluttered UI systems with a clean, game-like interface
    /// </summary>
    public class CleanGameUI : MonoBehaviour
    {
        [Header("🎮 Core UI Elements")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject actionPanel;
        [SerializeField] private GameObject minipollInfoPanel;
        
        [Header("🎯 Action Buttons")]
        [SerializeField] private Button feedButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button cleanButton;
        [SerializeField] private Button sleepButton;
        
        [Header("📊 Minipoll Status")]
        [SerializeField] private TextMeshProUGUI minipollNameText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider hungerBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Slider happinessBar;
        
        [Header("🕒 Game Info")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI weatherText;
        
        [Header("🎨 Modern Styling")]
        [SerializeField] private Color primaryColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color dangerColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        
        // Runtime state
        private MinipollGame.Core.MinipollCore selectedMinipoll;
        private UIManager uiManager;
        private GameManager gameManager;
        
        private void Awake()
        {
            InitializeUI();
        }
        
        private void Start()
        {
            FindManagers();
            SetupButtons();
            SetupStyling();
            HideMinipollInfo(); // Start with no minipoll selected
        }
        
        private void Update()
        {
            UpdateGameInfo();
            UpdateMinipollInfo();
            HandleInput();
        }
        
        #region Initialization
        
        private void InitializeUI()
        {
            // Ensure we have a main canvas
            if (mainCanvas == null)
            {
                mainCanvas = GetComponent<Canvas>();
                if (mainCanvas == null)
                {
                    mainCanvas = gameObject.AddComponent<Canvas>();
                    mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    gameObject.AddComponent<CanvasScaler>();
                    gameObject.AddComponent<GraphicRaycaster>();
                }
            }
            
            CreateUIIfMissing();
        }
        
        private void CreateUIIfMissing()
        {
            // Create main panels if missing
            if (hudPanel == null)
                hudPanel = CreatePanel("HUD Panel", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0.8f), new Vector2(1, 1));
                
            if (actionPanel == null)
                actionPanel = CreatePanel("Action Panel", new Vector2(0, 0), new Vector2(0.3f, 0.2f), new Vector2(0, 0), new Vector2(0.3f, 0.2f));
                
            if (minipollInfoPanel == null)
                minipollInfoPanel = CreatePanel("Minipoll Info", new Vector2(1, 1), new Vector2(1, 1), new Vector2(0.7f, 0.6f), new Vector2(1, 1));
        }
        
        private GameObject CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(mainCanvas.transform, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.3f); // Semi-transparent background
            
            return panel;
        }
        
        private void FindManagers()
        {
            uiManager = FindFirstObjectByType<UIManager>();
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        private void SetupButtons()
        {
            if (feedButton != null)
                feedButton.onClick.AddListener(() => PerformAction("feed"));
                
            if (playButton != null)
                playButton.onClick.AddListener(() => PerformAction("play"));
                
            if (cleanButton != null)
                cleanButton.onClick.AddListener(() => PerformAction("clean"));
                
            if (sleepButton != null)
                sleepButton.onClick.AddListener(() => PerformAction("sleep"));
        }
        
        private void SetupStyling()
        {
            // Apply modern styling to buttons
            StyleButton(feedButton, successColor);
            StyleButton(playButton, primaryColor);
            StyleButton(cleanButton, warningColor);
            StyleButton(sleepButton, new Color(0.6f, 0.4f, 0.8f, 1f)); // Purple for sleep
        }
        
        private void StyleButton(Button button, Color color)
        {
            if (button == null) return;
            
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            button.colors = colors;
        }
        
        #endregion
        
        #region Game Info Updates
        
        private void UpdateGameInfo()
        {
            if (timeText != null && gameManager != null)
            {
                timeText.text = System.DateTime.Now.ToString("HH:mm");
            }
            
            if (dayText != null && gameManager != null)
            {
                dayText.text = $"Day {gameManager.currentDay}";
            }
            
            if (weatherText != null)
            {
                weatherText.text = "Sunny"; // Simple placeholder
            }
        }
        
        #endregion
        
        #region Minipoll Selection & Info
        
        public void SelectMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            selectedMinipoll = minipoll;
            
            if (minipoll != null)
            {
                ShowMinipollInfo();
                EnableActionButtons(true);
            }
            else
            {
                HideMinipollInfo();
                EnableActionButtons(false);
            }
        }
        
        private void ShowMinipollInfo()
        {
            if (minipollInfoPanel != null)
                minipollInfoPanel.SetActive(true);
        }
        
        private void HideMinipollInfo()
        {
            if (minipollInfoPanel != null)
                minipollInfoPanel.SetActive(false);
        }
        
        private void EnableActionButtons(bool enabled)
        {
            if (feedButton != null) feedButton.interactable = enabled;
            if (playButton != null) playButton.interactable = enabled;
            if (cleanButton != null) cleanButton.interactable = enabled;
            if (sleepButton != null) sleepButton.interactable = enabled;
        }
        
        private void UpdateMinipollInfo()
        {
            if (selectedMinipoll == null) return;
            
            // Update name
            if (minipollNameText != null)
                minipollNameText.text = selectedMinipoll.Name;
            
            // Update needs bars
            var needs = selectedMinipoll.GetComponent<MinipollNeedsSystem>();
            if (needs != null)
            {
                if (healthBar != null)
                    healthBar.value = selectedMinipoll.Health != null ? selectedMinipoll.Health.CurrentHealth / selectedMinipoll.Health.MaxHealth : 1f;
                    
                if (hungerBar != null)
                    hungerBar.value = needs.GetNormalizedNeed("Hunger");
                    
                if (energyBar != null)
                    energyBar.value = needs.GetNormalizedNeed("Energy");
                    
                if (happinessBar != null)
                    happinessBar.value = needs.GetNormalizedNeed("Fun");
            }
        }
        
        #endregion
        
        #region Actions
        
        private void PerformAction(string actionType)
        {
            if (selectedMinipoll == null) return;
            
            switch (actionType.ToLower())
            {
                case "feed":
                    selectedMinipoll.GetComponent<MinipollNeedsSystem>()?.Eat(30f);
                    Debug.Log($"[CleanGameUI] Fed {selectedMinipoll.Name}");
                    break;
                    
                case "play":
                    selectedMinipoll.GetComponent<MinipollNeedsSystem>()?.Socialize(25f);
                    Debug.Log($"[CleanGameUI] Played with {selectedMinipoll.Name}");
                    break;
                    
                case "clean":
                    // Implement cleaning logic
                    Debug.Log($"[CleanGameUI] Cleaned {selectedMinipoll.Name}");
                    break;
                    
                case "sleep":
                    selectedMinipoll.Sleep(1.0f);
                    Debug.Log($"[CleanGameUI] Put {selectedMinipoll.Name} to sleep");
                    break;
            }
        }
        
        #endregion
        
        #region Input Handling
        
        private void HandleInput()
        {
            // Click to select Minipoll
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var minipoll = hit.collider.GetComponent<MinipollGame.Core.MinipollCore>();
                    if (minipoll != null)
                    {
                        SelectMinipoll(minipoll);
                    }
                    else
                    {
                        SelectMinipoll(null); // Deselect if clicking elsewhere
                    }
                }
            }
            
            // Keyboard shortcuts
            if (selectedMinipoll != null)
            {
                if (Input.GetKeyDown(KeyCode.F)) PerformAction("feed");
                if (Input.GetKeyDown(KeyCode.P)) PerformAction("play");
                if (Input.GetKeyDown(KeyCode.C)) PerformAction("clean");
                if (Input.GetKeyDown(KeyCode.S)) PerformAction("sleep");
            }
        }
        
        #endregion
    }
}
