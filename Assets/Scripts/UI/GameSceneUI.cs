using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using MinipollGame.Core;

namespace MinipollGame.UI
{
    /// <summary>
    /// Clean Game Scene UI - Simple, elegant UI for the main gameplay
    /// Provides essential controls without cluttering the screen
    /// </summary>
    public class GameSceneUI : MonoBehaviour
    {
        [Header("🎮 Action Buttons")]
        [SerializeField] private Button feedButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button cleanButton;
        
        [Header("📊 Minipoll Info")]
        [SerializeField] private GameObject minipollInfoPanel;
        [SerializeField] private TextMeshProUGUI minipollNameText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider hungerBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Slider happinessBar;
        
        [Header("🎨 Modern Colors")]
        [SerializeField] private Color feedColor = new Color(0.2f, 0.8f, 0.2f, 1f);    // Green
        [SerializeField] private Color playColor = new Color(0.2f, 0.6f, 1f, 1f);      // Blue  
        [SerializeField] private Color cleanColor = new Color(1f, 0.6f, 0.2f, 1f);     // Orange
        
        [Header("🔔 Events")]
        public UnityEvent OnFeedButtonClicked = new UnityEvent();
        public UnityEvent OnPlayButtonClicked = new UnityEvent();
        public UnityEvent OnCleanButtonClicked = new UnityEvent();
        
        // Runtime state
        private MinipollGame.Core.MinipollCore selectedMinipoll;
        
        private void Start()
        {
            SetupUI();
            SetupButtons();
            HideMinipollInfo();
        }
        
        private void Update()
        {
            UpdateMinipollInfo();
            HandleInput();
        }
        
        #region Setup
        
        private void SetupUI()
        {
            // Style buttons with modern colors
            StyleButton(feedButton, feedColor);
            StyleButton(playButton, playColor);
            StyleButton(cleanButton, cleanColor);
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
        
        private void SetupButtons()
        {
            if (feedButton != null)
                feedButton.onClick.AddListener(() => FeedMinipoll());
                
            if (playButton != null)
                playButton.onClick.AddListener(() => PlayWithMinipoll());
                
            if (cleanButton != null)
                cleanButton.onClick.AddListener(() => CleanMinipoll());
                
            EnableButtons(false); // Start disabled until minipoll selected
        }
        
        #endregion
        
        #region Minipoll Selection
        
        public void SelectMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            selectedMinipoll = minipoll;
            
            if (minipoll != null)
            {
                ShowMinipollInfo();
                EnableButtons(true);
                Debug.Log($"[GameSceneUI] Selected {minipoll.Name}");
            }
            else
            {
                HideMinipollInfo();
                EnableButtons(false);
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
        
        private void EnableButtons(bool enabled)
        {
            if (feedButton != null) feedButton.interactable = enabled;
            if (playButton != null) playButton.interactable = enabled;
            if (cleanButton != null) cleanButton.interactable = enabled;
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateMinipollInfo()
        {
            if (selectedMinipoll == null) return;
            
            // Update name
            if (minipollNameText != null)
                minipollNameText.text = selectedMinipoll.Name;
            
            // Update status bars
            var needs = selectedMinipoll.GetComponent<MinipollNeedsSystem>();
            if (needs != null)
            {
                if (healthBar != null)
                    healthBar.value = selectedMinipoll.Health != null ? 
                        selectedMinipoll.Health.CurrentHealth / selectedMinipoll.Health.MaxHealth : 1f;
                        
                if (hungerBar != null)
                    hungerBar.value = needs.GetNormalizedNeed("Hunger");
                    
                if (energyBar != null)
                    energyBar.value = needs.GetNormalizedNeed("Energy");
                    
                if (happinessBar != null)
                    happinessBar.value = needs.GetNormalizedNeed("Fun");
            }
        }
        
        #endregion
        
        #region Public Action Methods
        
        /// <summary>
        /// Public method for feeding the selected minipoll
        /// </summary>
        public void OnFeedAction()
        {
            FeedMinipoll();
            OnFeedButtonClicked.Invoke();
        }
        
        /// <summary>
        /// Public method for playing with the selected minipoll
        /// </summary>
        public void OnPlayAction()
        {
            PlayWithMinipoll();
            OnPlayButtonClicked.Invoke();
        }
        
        /// <summary>
        /// Public method for cleaning the selected minipoll
        /// </summary>
        public void OnCleanAction()
        {
            CleanMinipoll();
            OnCleanButtonClicked.Invoke();
        }
        
        /// <summary>
        /// Public method for deselecting the current minipoll
        /// </summary>
        public void DeselectMinipoll()
        {
            SelectMinipoll(null);
        }
        
        /// <summary>
        /// Highlight button for tutorial system
        /// </summary>
        public void HighlightButton(string buttonName)
        {
            // Simple highlight implementation
            switch (buttonName.ToLower())
            {
                case "feed":
                    if (feedButton != null)
                        feedButton.GetComponent<Image>().color = new Color(1f, 1f, 0f, 0.8f); // Yellow highlight
                    break;
                case "play":
                    if (playButton != null)
                        playButton.GetComponent<Image>().color = new Color(1f, 1f, 0f, 0.8f);
                    break;
                case "clean":
                    if (cleanButton != null)
                        cleanButton.GetComponent<Image>().color = new Color(1f, 1f, 0f, 0.8f);
                    break;
            }
        }
        
        /// <summary>
        /// Clear all button highlights
        /// </summary>
        public void ClearHighlights()
        {
            if (feedButton != null)
                feedButton.GetComponent<Image>().color = Color.white;
            if (playButton != null)
                playButton.GetComponent<Image>().color = Color.white;
            if (cleanButton != null)
                cleanButton.GetComponent<Image>().color = Color.white;
        }
        
        #endregion
        
        #region Actions
        
        private void FeedMinipoll()
        {
            if (selectedMinipoll == null) return;
            
            var needs = selectedMinipoll.GetComponent<MinipollNeedsSystem>();
            if (needs != null)
            {
                needs.Eat(30f);
                Debug.Log($"[GameSceneUI] Fed {selectedMinipoll.Name}");
            }
        }
        
        private void PlayWithMinipoll()
        {
            if (selectedMinipoll == null) return;
            
            var needs = selectedMinipoll.GetComponent<MinipollNeedsSystem>();
            if (needs != null)
            {
                needs.Socialize(25f);
                Debug.Log($"[GameSceneUI] Played with {selectedMinipoll.Name}");
            }
        }
        
        private void CleanMinipoll()
        {
            if (selectedMinipoll == null) return;
            
            // Simple cleaning action
            var needs = selectedMinipoll.GetComponent<MinipollNeedsSystem>();
            if (needs != null)
            {
                needs.FillNeed("Hygiene", 40f);
                Debug.Log($"[GameSceneUI] Cleaned {selectedMinipoll.Name}");
            }
        }
        
        #endregion
        
        #region Input
        
        private void HandleInput()
        {
            // Click to select Minipoll
            if (Input.GetMouseButtonDown(0))
            {
                Camera camera = Camera.main;
                if (camera == null) return;
                
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var minipoll = hit.collider.GetComponent<MinipollGame.Core.MinipollCore>();
                    SelectMinipoll(minipoll);
                }
                else
                {
                    SelectMinipoll(null); // Deselect if clicking empty space
                }
            }
            
            // Keyboard shortcuts for actions
            if (selectedMinipoll != null)
            {
                if (Input.GetKeyDown(KeyCode.F)) FeedMinipoll();
                if (Input.GetKeyDown(KeyCode.P)) PlayWithMinipoll();
                if (Input.GetKeyDown(KeyCode.C)) CleanMinipoll();
                if (Input.GetKeyDown(KeyCode.S)) selectedMinipoll.Sleep(1.0f);
            }
        }
        
        #endregion
    }
}
