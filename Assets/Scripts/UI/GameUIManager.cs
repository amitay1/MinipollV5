using UnityEngine;
using MinipollGame.UI;
using UnityEngine.InputSystem;

namespace MinipollGame.UI
{
    /// <summary>
    /// UI Manager that initializes and controls the comprehensive UI system
    /// Auto-creates and manages all UI components for MinipollV5
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        [Header("🎮 Game UI Configuration")]
        [SerializeField] private bool autoInitializeOnStart = true;
        [SerializeField] private bool createUIOnAwake = false;
        
        [Header("🎯 UI System References")]
        [SerializeField] private ComprehensiveUISystem uiSystem;
        [SerializeField] private Canvas gameCanvas;
        
        [Header("🔊 Audio Configuration")]
        [SerializeField] private AudioClip[] uiSounds;
        
        // Singleton instance
        public static GameUIManager Instance { get; private set; }
        
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (createUIOnAwake)
                {
                    InitializeUISystem();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            if (autoInitializeOnStart && uiSystem == null)
            {
                InitializeUISystem();
            }
        }
        
        /// <summary>
        /// Initialize the comprehensive UI system
        /// </summary>
        public void InitializeUISystem()
        {
            // Create UI system if it doesn't exist
            if (uiSystem == null)
            {
                GameObject uiSystemObj = new GameObject("ComprehensiveUISystem");
                uiSystemObj.transform.SetParent(transform);
                uiSystem = uiSystemObj.AddComponent<ComprehensiveUISystem>();
            }
        }
        
        /// <summary>
        /// Get reference to the UI system
        /// </summary>
        public ComprehensiveUISystem GetUISystem()
        {
            if (uiSystem == null)
            {
                InitializeUISystem();
            }
            return uiSystem;
        }
        
        /// <summary>
        /// Show a specific UI panel
        /// </summary>
        public void ShowPanel(string panelName)
        {
            if (uiSystem != null)
            {
                uiSystem.ShowPanel(panelName);
            }
            else
            {
                Debug.LogWarning("⚠️ UI System not initialized! Call InitializeUISystem() first.");
            }
        }
        
        /// <summary>
        /// Hide the current active panel
        /// </summary>
        public void HideCurrentPanel()
        {
            if (uiSystem != null)
            {
                uiSystem.HideCurrentPanel();
            }
        }
        
        /// <summary>
        /// Show a notification message
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            if (uiSystem != null)
            {
                uiSystem.ShowNotification(message, type, duration);
            }
        }
        
        /// <summary>
        /// Toggle pause menu
        /// </summary>
        public void TogglePauseMenu()
        {
            if (uiSystem != null)
            {
                // Check if currently showing pause menu, if so hide it, otherwise show it
                ShowPanel("PauseMenu");
            }
        }
        
        /// <summary>
        /// Start the game (show HUD, hide main menu)
        /// </summary>
        public void StartGame()
        {
            if (uiSystem != null)
            {
                ShowPanel("GameHUD");
                ShowNotification("🎮 Welcome to Minipoll!", NotificationType.Success);
            }
        }
        
        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (uiSystem != null)
            {
                ShowPanel("MainMenu");
                ShowNotification("🏠 Welcome back!", NotificationType.Info);
            }
        }
        
        /// <summary>
        /// Create a quick access method for showing achievements
        /// </summary>
        public void ShowAchievements()
        {
            ShowPanel("Achievements");
        }
        
        /// <summary>
        /// Create a quick access method for showing inventory
        /// </summary>
        public void ShowInventory()
        {
            ShowPanel("Inventory");
        }
        
        /// <summary>
        /// Create a quick access method for showing settings
        /// </summary>
        public void ShowSettings()
        {
            ShowPanel("Settings");
        }
        
        /// <summary>
        /// Handle input for UI navigation (ESC key, etc.)
        /// </summary>
        void Update()
        {
            HandleUIInput();
        }
        
        private void HandleUIInput()
        {
            // ESC key handling for quick pause/back navigation
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePauseMenu();
            }
            
            // Quick access keys
            if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
            {
                ShowInventory();
            }
            
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
            {
                ShowAchievements();
            }
            
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                ShowSettings();
            }
        }
        
        /// <summary>
        /// Create a UI manager instance if one doesn't exist
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CreateUIManager()
        {
            if (Instance == null)
            {
                GameObject uiManagerObj = new GameObject("GameUIManager");
                uiManagerObj.AddComponent<GameUIManager>();
            }
        }
    }
}
