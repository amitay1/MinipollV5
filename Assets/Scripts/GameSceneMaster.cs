using UnityEngine;
using MinipollGame.UI;
using MinipollGame.Interaction;
using MinipollGame.Setup;
using MinipollGame.Tutorial;
using MinipollGame.Core;

namespace MinipollGame
{
    /// <summary>
    /// Game Scene Master Controller - One-stop solution for 02_GameScene UI setup
    /// Automatically creates and configures all UI elements needed for Feed/Play/Clean interactions
    /// 
    /// USAGE: Simply add this script to any GameObject in the 02_GameScene and it will handle everything
    /// </summary>
    public class GameSceneMaster : MonoBehaviour
    {
        [Header("🎮 Master Control")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool enableDebugMode = true;
        
        [Header("🎯 Feature Toggles")]
        [SerializeField] private bool enableUI = true;
        [SerializeField] private bool enableClickSelection = true;
        [SerializeField] private bool enableTutorialIntegration = true;
        [SerializeField] private bool enableValidation = true;
        
        [Header("🔧 Force Options")]
        [SerializeField] private bool forceRecreateUI = false;
        [SerializeField] private bool cleanupOnDestroy = false;
        
        // Component references (auto-managed)
        private GameSceneUIBuilder uiBuilder;
        private GameSceneUI uiController;
        private MinipollClickHandler clickHandler;
        private GameSceneSetup sceneSetup;
        private TutorialUIIntegration tutorialIntegration;
        
        // Status tracking
        private bool isSetupComplete = false;
        private bool hasValidatedScene = false;
        
        void Start()
        {
            if (autoSetupOnStart)
            {
                SetupCompleteGameScene();
            }
        }
        
        /// <summary>
        /// Main setup method - creates all required components for the game scene
        /// </summary>
        [ContextMenu("🎮 Setup Complete Game Scene")]
        public void SetupCompleteGameScene()
        {
            Debug.Log("🎮 GameSceneMaster: Starting complete setup for 02_GameScene...");
            
            try
            {
                // Phase 1: Core UI Creation
                if (enableUI)
                {
                    SetupUIComponents();
                }
                
                // Phase 2: Interaction System
                if (enableClickSelection)
                {
                    SetupInteractionSystem();
                }
                
                // Phase 3: Tutorial Integration
                if (enableTutorialIntegration)
                {
                    SetupTutorialSystem();
                }
                
                // Phase 4: Scene Validation
                if (enableValidation)
                {
                    ValidateSceneSetup();
                }
                
                isSetupComplete = true;
                Debug.Log("✅ GameSceneMaster: Complete setup finished successfully!");
                
                // Show summary
                DisplaySetupSummary();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ GameSceneMaster setup failed: {e.Message}");
                Debug.LogError(e.StackTrace);
            }
        }
        
        #region Setup Phases
        
        void SetupUIComponents()
        {
            Debug.Log("🏗️ Phase 1: Setting up UI components...");
            
            // Create UI Builder
            uiBuilder = FindOrCreateComponent<GameSceneUIBuilder>("GameSceneUIBuilder");
            if (forceRecreateUI)
            {
                uiBuilder.CleanUpUI();
            }
            uiBuilder.CreateGameSceneUI();
            
            // Create UI Controller
            uiController = FindOrCreateComponent<GameSceneUI>("GameSceneUIController");
            
            Debug.Log("✅ UI components setup complete");
        }
        
        void SetupInteractionSystem()
        {
            Debug.Log("🎯 Phase 2: Setting up interaction system...");
            
            // Create Click Handler
            clickHandler = FindOrCreateComponent<MinipollClickHandler>("MinipollClickHandler");
            
            Debug.Log("✅ Interaction system setup complete");
        }
        
        void SetupTutorialSystem()
        {
            Debug.Log("🎓 Phase 3: Setting up tutorial integration...");
            
            // Create Tutorial Integration
            tutorialIntegration = FindOrCreateComponent<TutorialUIIntegration>("TutorialUIIntegration");
            
            Debug.Log("✅ Tutorial integration setup complete");
        }
        
        void ValidateSceneSetup()
        {
            Debug.Log("🔍 Phase 4: Validating scene setup...");
            
            // Create Scene Setup validator
            sceneSetup = FindOrCreateComponent<GameSceneSetup>("GameSceneSetup");
            sceneSetup.ValidateScene();
            
            hasValidatedScene = true;
            Debug.Log("✅ Scene validation complete");
        }
        
        #endregion
        
        #region Utility Methods
        
        T FindOrCreateComponent<T>(string gameObjectName) where T : MonoBehaviour
        {
            T component = FindFirstObjectByType<T>();
            if (component == null)
            {
                if (enableDebugMode)
                    Debug.Log($"🔧 Creating {typeof(T).Name}");
                
                GameObject obj = new GameObject(gameObjectName);
                component = obj.AddComponent<T>();
            }
            else
            {
                if (enableDebugMode)
                    Debug.Log($"🔧 Found existing {typeof(T).Name}");
            }
            return component;
        }
        
        void DisplaySetupSummary()
        {
            Debug.Log("📊 === GAME SCENE SETUP SUMMARY ===");
            Debug.Log($"🎮 Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            Debug.Log($"🏗️ UI Builder: {(uiBuilder != null ? "✅" : "❌")}");
            Debug.Log($"🎮 UI Controller: {(uiController != null ? "✅" : "❌")}");
            Debug.Log($"🎯 Click Handler: {(clickHandler != null ? "✅" : "❌")}");
            Debug.Log($"🎓 Tutorial Integration: {(tutorialIntegration != null ? "✅" : "❌")}");
            
            // Check required GameObjects
            Debug.Log("🔍 Required UI Elements:");
            Debug.Log($"   🍎 FeedButton: {(GameObject.Find("FeedButton") != null ? "✅" : "❌")}");
            Debug.Log($"   ⚽ PlayButton: {(GameObject.Find("PlayButton") != null ? "✅" : "❌")}");
            Debug.Log($"   🧼 CleanButton: {(GameObject.Find("CleanButton") != null ? "✅" : "❌")}");
            Debug.Log($"   📊 NeedsUI: {(GameObject.Find("NeedsUI") != null ? "✅" : "❌")}");
            
            // Check Minipolls
            MinipollGame.Core.MinipollCore[] minipolls = FindObjectsOfType<MinipollGame.Core.MinipollCore>();
            Debug.Log($"🐧 Minipolls in scene: {minipolls.Length}");
            
            Debug.Log("📊 === SETUP SUMMARY END ===");
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Check if the scene is ready for gameplay
        /// </summary>
        public bool IsSceneReady()
        {
            bool hasRequiredUI = GameObject.Find("FeedButton") != null && 
                                GameObject.Find("PlayButton") != null && 
                                GameObject.Find("CleanButton") != null && 
                                GameObject.Find("NeedsUI") != null;
            
            bool hasRequiredComponents = uiController != null && clickHandler != null;
            
            bool hasMinipolls = FindObjectsOfType<MinipollGame.Core.MinipollCore>().Length > 0;
            
            return hasRequiredUI && hasRequiredComponents && hasMinipolls && isSetupComplete;
        }
        
        /// <summary>
        /// Get the UI controller for external access
        /// </summary>
        public GameSceneUI GetUIController()
        {
            return uiController;
        }
        
        /// <summary>
        /// Get the click handler for external access
        /// </summary>
        public MinipollClickHandler GetClickHandler()
        {
            return clickHandler;
        }
        
        /// <summary>
        /// Get the tutorial integration for external access
        /// </summary>
        public TutorialUIIntegration GetTutorialIntegration()
        {
            return tutorialIntegration;
        }
        
        #endregion
        
        #region Context Menu Methods
        
        [ContextMenu("🔄 Force Recreate Everything")]
        public void ForceRecreateEverything()
        {
            forceRecreateUI = true;
            CleanupAllComponents();
            SetupCompleteGameScene();
            forceRecreateUI = false;
        }
        
        [ContextMenu("🗑️ Cleanup All Components")]
        public void CleanupAllComponents()
        {
            Debug.Log("🗑️ Cleaning up all GameSceneMaster components...");
            
            // Find and remove UI elements
            GameObject[] uiObjects = {
                GameObject.Find("ActionButtonsPanel"),
                GameObject.Find("NeedsUI"),
                GameObject.Find("FeedButton"),
                GameObject.Find("PlayButton"),
                GameObject.Find("CleanButton")
            };
            
            foreach (GameObject obj in uiObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            
            // Find and remove component GameObjects
            string[] componentNames = {
                "GameSceneUIBuilder",
                "GameSceneUIController", 
                "MinipollClickHandler",
                "GameSceneSetup",
                "TutorialUIIntegration"
            };
            
            foreach (string name in componentNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            
            isSetupComplete = false;
            hasValidatedScene = false;
            
            Debug.Log("🗑️ Cleanup complete");
        }
        
        [ContextMenu("📊 Show Status Report")]
        public void ShowStatusReport()
        {
            DisplaySetupSummary();
            Debug.Log($"🔄 Setup Complete: {isSetupComplete}");
            Debug.Log($"🔍 Validated: {hasValidatedScene}");
            Debug.Log($"🎮 Scene Ready: {IsSceneReady()}");
        }
        
        [ContextMenu("🧪 Test All Features")]
        public void TestAllFeatures()
        {
            Debug.Log("🧪 Testing all GameSceneMaster features...");
            
            if (!IsSceneReady())
            {
                Debug.LogWarning("⚠️ Scene not ready, running setup first...");
                SetupCompleteGameScene();
            }
            
            StartCoroutine(RunFeatureTests());
        }
        
        System.Collections.IEnumerator RunFeatureTests()
        {
            yield return null;
            
            // Test Minipoll selection
            MinipollGame.Core.MinipollCore[] minipolls = FindObjectsOfType<MinipollGame.Core.MinipollCore>();
            if (minipolls.Length > 0 && clickHandler != null)
            {
                Debug.Log("🎯 Testing Minipoll selection...");
                clickHandler.SelectMinipoll(minipolls[0]);
                yield return new WaitForSeconds(1f);
            }
            
            // Test UI actions
            if (uiController != null)
            {
                Debug.Log("🍎 Testing Feed action...");
                uiController.OnFeedAction();
                yield return new WaitForSeconds(0.5f);
                
                Debug.Log("⚽ Testing Play action...");
                uiController.OnPlayAction();
                yield return new WaitForSeconds(0.5f);
                
                Debug.Log("🧼 Testing Clean action...");
                uiController.OnCleanAction();
                yield return new WaitForSeconds(0.5f);
            }
            
            // Test tutorial integration
            if (tutorialIntegration != null)
            {
                Debug.Log("🎯 Testing tutorial highlighting...");
                tutorialIntegration.HighlightActionButton("feed");
                yield return new WaitForSeconds(1f);
                tutorialIntegration.ClearAllHighlights();
            }
            
            Debug.Log("🎉 All feature tests complete!");
        }
        
        #endregion
        
        void OnDestroy()
        {
            if (cleanupOnDestroy)
            {
                CleanupAllComponents();
            }
        }
    }
}
