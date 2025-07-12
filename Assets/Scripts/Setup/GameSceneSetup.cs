using UnityEngine;
using MinipollGame.UI;
using MinipollGame.Interaction;
using MinipollGame.Core;

namespace MinipollGame.Setup
{
    /// <summary>
    /// Game Scene Setup - Automatically configures 02_GameScene with required UI components
    /// This script ensures the scene has all the elements needed for Feed/Play/Clean interactions
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        [Header("🎮 Auto Setup")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool forceRecreateUI = false;
        
        [Header("📊 Validation")]
        [SerializeField] private bool validateTutorialRequirements = true;
        
        void Start()
        {
            if (setupOnStart)
            {
                SetupGameScene();
            }
        }
        
        [ContextMenu("🎮 Setup Game Scene")]
        public void SetupGameScene()
        {
            Debug.Log("🎮 Setting up 02_GameScene with UI components...");
            
            // 1. Setup UI Builder and create UI elements
            SetupUIBuilder();
            
            // 2. Setup click handler for Minipoll selection
            SetupClickHandler();
            
            // 3. Setup GameSceneUI controller
            SetupUIController();
            
            // 4. Validate tutorial requirements
            if (validateTutorialRequirements)
            {
                ValidateTutorialRequirements();
            }
            
            Debug.Log("✅ Game Scene setup complete!");
        }
        
        void SetupUIBuilder()
        {
            GameSceneUIBuilder builder = FindFirstObjectByType<GameSceneUIBuilder>();
            if (builder == null)
            {
                Debug.Log("🏗️ Creating GameSceneUIBuilder");
                GameObject builderObj = new GameObject("GameSceneUIBuilder");
                builder = builderObj.AddComponent<GameSceneUIBuilder>();
            }
            
            // Force UI creation if requested
            if (forceRecreateUI)
            {
                builder.CleanUpUI();
            }
            
            builder.CreateGameSceneUI();
        }
        
        void SetupClickHandler()
        {
            MinipollClickHandler clickHandler = FindFirstObjectByType<MinipollClickHandler>();
            if (clickHandler == null)
            {
                Debug.Log("🎯 Creating MinipollClickHandler");
                GameObject handlerObj = new GameObject("MinipollClickHandler");
                clickHandler = handlerObj.AddComponent<MinipollClickHandler>();
            }
        }
        
        void SetupUIController()
        {
            GameSceneUI uiController = FindFirstObjectByType<GameSceneUI>();
            if (uiController == null)
            {
                Debug.Log("🎮 Creating GameSceneUI controller");
                GameObject controllerObj = new GameObject("GameSceneUIController");
                uiController = controllerObj.AddComponent<GameSceneUI>();
            }
        }
        
        void ValidateTutorialRequirements()
        {
            Debug.Log("🔍 Validating tutorial requirements...");
            
            bool allRequirementsMet = true;
            
            // Check for required GameObjects that tutorial expects
            string[] requiredObjects = { "FeedButton", "PlayButton", "CleanButton", "NeedsUI" };
            
            foreach (string objName in requiredObjects)
            {
                GameObject obj = GameObject.Find(objName);
                if (obj != null)
                {
                    Debug.Log($"✅ Found required object: {objName}");
                }
                else
                {
                    Debug.LogWarning($"❌ Missing required object: {objName}");
                    allRequirementsMet = false;
                }
            }
            
            // Check for Minipoll creatures in scene
            MinipollGame.Core.MinipollCore[] minipolls = FindObjectsOfType<MinipollGame.Core.MinipollCore>();
            if (minipolls.Length > 0)
            {
                Debug.Log($"✅ Found {minipolls.Length} Minipoll creatures in scene");
            }
            else
            {
                Debug.LogWarning("❌ No Minipoll creatures found in scene");
                allRequirementsMet = false;
            }
            
            // Check for UI components
            GameSceneUI uiController = FindFirstObjectByType<GameSceneUI>();
            if (uiController != null)
            {
                Debug.Log("✅ GameSceneUI controller present");
            }
            else
            {
                Debug.LogWarning("❌ GameSceneUI controller missing");
                allRequirementsMet = false;
            }
            
            MinipollClickHandler clickHandler = FindFirstObjectByType<MinipollClickHandler>();
            if (clickHandler != null)
            {
                Debug.Log("✅ MinipollClickHandler present");
            }
            else
            {
                Debug.LogWarning("❌ MinipollClickHandler missing");
                allRequirementsMet = false;
            }
            
            if (allRequirementsMet)
            {
                Debug.Log("🎉 All tutorial requirements met! Scene is ready for gameplay.");
            }
            else
            {
                Debug.LogWarning("⚠️ Some tutorial requirements are missing. Tutorial may not work correctly.");
            }
        }
        
        [ContextMenu("🔍 Validate Scene")]
        public void ValidateScene()
        {
            ValidateTutorialRequirements();
        }
        
        [ContextMenu("🎯 Test Minipoll Selection")]
        public void TestMinipollSelection()
        {
            MinipollGame.Core.MinipollCore[] minipolls = FindObjectsOfType<MinipollGame.Core.MinipollCore>();
            if (minipolls.Length > 0)
            {
                MinipollClickHandler clickHandler = FindFirstObjectByType<MinipollClickHandler>();
                if (clickHandler != null)
                {
                    Debug.Log($"🎯 Testing selection of {minipolls[0].name}");
                    clickHandler.SelectMinipoll(minipolls[0]);
                }
                else
                {
                    Debug.LogWarning("❌ No MinipollClickHandler found for testing");
                }
            }
            else
            {
                Debug.LogWarning("❌ No Minipolls found for testing");
            }
        }
        
        [ContextMenu("🎮 Test Feed Action")]
        public void TestFeedAction()
        {
            GameSceneUI uiController = FindFirstObjectByType<GameSceneUI>();
            if (uiController != null)
            {
                MinipollGame.Core.MinipollCore[] minipolls = FindObjectsOfType<MinipollGame.Core.MinipollCore>();
                if (minipolls.Length > 0)
                {
                    uiController.SelectMinipoll(minipolls[0]);
                    uiController.OnFeedAction();
                    Debug.Log("🍎 Tested feed action");
                }
                else
                {
                    Debug.LogWarning("❌ No Minipolls found for testing");
                }
            }
            else
            {
                Debug.LogWarning("❌ GameSceneUI not found for testing");
            }
        }
        
        /// <summary>
        /// Creates a comprehensive test scenario
        /// </summary>
        [ContextMenu("🧪 Full Test Scenario")]
        public void FullTestScenario()
        {
            Debug.Log("🧪 Running full test scenario...");
            
            // 1. Setup scene if needed
            SetupGameScene();
            
            // 2. Wait a frame for UI creation
            StartCoroutine(RunTestSequence());
        }
        
        System.Collections.IEnumerator RunTestSequence()
        {
            yield return null; // Wait one frame
            
            // 3. Test Minipoll selection
            TestMinipollSelection();
            
            yield return new WaitForSeconds(1f);
            
            // 4. Test UI actions
            TestFeedAction();
            
            yield return new WaitForSeconds(0.5f);
            
            GameSceneUI uiController = FindFirstObjectByType<GameSceneUI>();
            if (uiController != null)
            {
                uiController.OnPlayAction();
                Debug.Log("⚽ Tested play action");
                
                yield return new WaitForSeconds(0.5f);
                
                uiController.OnCleanAction();
                Debug.Log("🧼 Tested clean action");
            }
            
            Debug.Log("🎉 Full test scenario complete!");
        }
    }
}
