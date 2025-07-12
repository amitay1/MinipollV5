using UnityEngine;
using MinipollGame.UI;
using MinipollGame.Testing;

namespace MinipollGame.UI
{
    /// <summary>
    /// Scene UI Initializer - Add this to any scene to automatically set up the UI system
    /// Handles scene-specific UI initialization and setup
    /// </summary>
    public class SceneUIInitializer : MonoBehaviour
    {
        [Header("🎬 Scene UI Configuration")]
        [SerializeField] private string defaultPanelToShow = "MainMenu";
        [SerializeField] private bool showWelcomeNotification = true;
        [SerializeField] private string welcomeMessage = "Welcome to Minipoll!";
        
        [Header("🧪 Testing")]
        [SerializeField] private bool enableTestControls = false;
        [SerializeField] private bool enableDebugMode = true;
        
        void Start()
        {
            InitializeSceneUI();
        }
        
        private void InitializeSceneUI()
        {
            Debug.Log("🎬 SceneUIInitializer: Setting up scene UI...");
            
            // Ensure GameUIManager exists
            var uiManager = GameUIManager.Instance;
            if (uiManager == null)
            {
                Debug.Log("🎮 Creating GameUIManager...");
                GameObject uiManagerObj = new GameObject("GameUIManager");
                uiManager = uiManagerObj.AddComponent<GameUIManager>();
            }
            
            // Wait a frame for UI system to initialize, then show default panel
            StartCoroutine(ShowDefaultPanelAfterDelay());
            
            // Create test controls if enabled
            if (enableTestControls)
            {
                CreateTestController();
            }
            
            Debug.Log("✨ Scene UI initialized successfully!");
        }
        
        private System.Collections.IEnumerator ShowDefaultPanelAfterDelay()
        {
            yield return new WaitForEndOfFrame();
            
            var uiManager = GameUIManager.Instance;
            if (uiManager != null)
            {
                // Show default panel
                if (!string.IsNullOrEmpty(defaultPanelToShow))
                {
                    uiManager.ShowPanel(defaultPanelToShow);
                    
                    if (enableDebugMode)
                    {
                        Debug.Log($"🎯 Showing default panel: {defaultPanelToShow}");
                    }
                }
                
                // Show welcome notification
                if (showWelcomeNotification && !string.IsNullOrEmpty(welcomeMessage))
                {
                    uiManager.ShowNotification(welcomeMessage, NotificationType.Success);
                }
            }
        }
        
        private void CreateTestController()
        {
            GameObject testControllerObj = new GameObject("UITestController");
            testControllerObj.AddComponent<UITestController>();
            
            if (enableDebugMode)
            {
                Debug.Log("🧪 Test controller created for scene");
            }
        }
        
        /// <summary>
        /// Call this method to change the default panel for this scene
        /// </summary>
        public void SetDefaultPanel(string panelName)
        {
            defaultPanelToShow = panelName;
            
            var uiManager = GameUIManager.Instance;
            if (uiManager != null)
            {
                uiManager.ShowPanel(panelName);
            }
        }
        
        /// <summary>
        /// Show a scene-specific notification
        /// </summary>
        public void ShowSceneNotification(string message, NotificationType type = NotificationType.Info)
        {
            var uiManager = GameUIManager.Instance;
            if (uiManager != null)
            {
                uiManager.ShowNotification(message, type);
            }
        }
    }
}
