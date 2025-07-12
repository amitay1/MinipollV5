using UnityEngine;
using UnityEngine.UI;
using MinipollGame.UI;

namespace MinipollGame.Testing
{
    /// <summary>
    /// Test script to demonstrate and test the comprehensive UI system
    /// Creates buttons to test all UI functionality
    /// </summary>
    public class UITestController : MonoBehaviour
    {
        [Header("🧪 UI Testing Controls")]
        [SerializeField] private bool createTestControls = true;
        [SerializeField] private Vector2 testButtonSize = new Vector2(200, 40);
        
        private GameUIManager uiManager;
        private Canvas testCanvas;
        
        void Start()
        {
            if (createTestControls)
            {
                CreateTestControls();
            }
            
            // Get reference to UI manager
            uiManager = GameUIManager.Instance;
            if (uiManager == null)
            {
                Debug.LogWarning("⚠️ GameUIManager not found! Creating one...");
                GameObject uiManagerObj = new GameObject("GameUIManager");
                uiManager = uiManagerObj.AddComponent<GameUIManager>();
            }
        }
        
        private void CreateTestControls()
        {
            // Create test canvas
            GameObject canvasObj = new GameObject("UITestCanvas");
            testCanvas = canvasObj.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            testCanvas.sortingOrder = 2000; // Higher than main UI
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create test button panel
            GameObject testPanel = new GameObject("TestPanel");
            testPanel.transform.SetParent(testCanvas.transform, false);
            
            var rectTransform = testPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(220, -20);
            rectTransform.sizeDelta = new Vector2(200, 400);
            
            var vlg = testPanel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            
            var csf = testPanel.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Add background
            var bg = testPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);
            
            // Create test buttons
            CreateTestButton(testPanel, "🏠 Main Menu", () => TestShowPanel("MainMenu"));
            CreateTestButton(testPanel, "🎮 Game HUD", () => TestShowPanel("GameHUD"));
            CreateTestButton(testPanel, "⏸️ Pause Menu", () => TestShowPanel("PauseMenu"));
            CreateTestButton(testPanel, "⚙️ Settings", () => TestShowPanel("Settings"));
            CreateTestButton(testPanel, "🏆 Achievements", () => TestShowPanel("Achievements"));
            CreateTestButton(testPanel, "🎒 Inventory", () => TestShowPanel("Inventory"));
            
            CreateTestButton(testPanel, "---", null); // Separator
            
            CreateTestButton(testPanel, "✅ Success Note", () => TestNotification("Success!", NotificationType.Success));
            CreateTestButton(testPanel, "⚠️ Warning Note", () => TestNotification("Warning!", NotificationType.Warning));
            CreateTestButton(testPanel, "❌ Error Note", () => TestNotification("Error!", NotificationType.Error));
            CreateTestButton(testPanel, "ℹ️ Info Note", () => TestNotification("Information", NotificationType.Info));
            
            CreateTestButton(testPanel, "---", null); // Separator
            
            CreateTestButton(testPanel, "🚀 Start Game", () => uiManager?.StartGame());
            CreateTestButton(testPanel, "🏠 Main Menu", () => uiManager?.ReturnToMainMenu());
            CreateTestButton(testPanel, "❌ Hide Panel", () => uiManager?.HideCurrentPanel());
            
            // Debug logging removed to reduce console spam
        }
        
        private void CreateTestButton(GameObject parent, string text, System.Action onClick)
        {
            GameObject buttonObj = new GameObject($"TestButton_{text.Replace(" ", "")}");
            buttonObj.transform.SetParent(parent.transform, false);
            
            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();
            
            if (text == "---")
            {
                // Separator
                image.color = Color.clear;
                var layoutElement = buttonObj.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 10;
                return;
            }
            
            // Style button
            UIBrandingExtensions.StyleModernButton(button, UIBrandingExtensions.ButtonStyle.Secondary);
            
            var layoutEl = buttonObj.AddComponent<LayoutElement>();
            layoutEl.preferredHeight = testButtonSize.y;
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textComponent = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 12;
            textComponent.color = Color.white;
            textComponent.alignment = TMPro.TextAlignmentOptions.Center;
            
            var textRect = textComponent.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Add click handler
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick.Invoke());
            }
        }
        
        private void TestShowPanel(string panelName)
        {
            // Debug logging removed to reduce console spam
            uiManager?.ShowPanel(panelName);
        }
        
        private void TestNotification(string message, NotificationType type)
        {
            // Debug logging removed to reduce console spam
            uiManager?.ShowNotification(message, type);
        }
        
        private void LaunchFullUISystem()
        {
            Debug.Log("🚀 UITestController: Launching Full UI System...");
            
            try
            {
                // Try to create OneClickUICreator
                GameObject oneClickObj = new GameObject("OneClickUICreator");
                var oneClick = oneClickObj.AddComponent<OneClickUICreator>();
                oneClick.CreateCompleteUIInstantly();
                Debug.Log("✅ Full UI System launched via OneClickUICreator!");
                
                // Show success notification
                TestNotification("🎉 Full UI System Launched!", NotificationType.Success);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to launch full UI system: {e.Message}");
                TestNotification("❌ Failed to launch UI system!", NotificationType.Error);
            }
        }
        
        void Update()
        {
            // SPACE key to launch full UI system
            if (InputHelper.GetKeyDown(KeyCode.Space))
            {
                // Debug logging removed to reduce console spam
                LaunchFullUISystem();
            }
            
            // F2 key for alternative launch
            if (InputHelper.GetKeyDown(KeyCode.F2))
            {
                // Debug logging removed to reduce console spam
                LaunchFullUISystem();
            }
            
            // Additional test hotkeys
            if (InputHelper.GetKeyDown(KeyCode.F1))
            {
                TestShowPanel("MainMenu");
            }
            
            if (InputHelper.GetKeyDown(KeyCode.F3))
            {
                TestShowPanel("PauseMenu");
            }
            
            if (InputHelper.GetKeyDown(KeyCode.F4))
            {
                TestShowPanel("Settings");
            }
            
            if (InputHelper.GetKeyDown(KeyCode.F5))
            {
                TestNotification("🎮 Test notification!", NotificationType.Info);
            }
        }
        
        void OnGUI()
        {
            // Display help text
            GUI.Label(new Rect(10, 10, 250, 150), 
                "🧪 UI Test Controls:\n" +
                "SPACE - Launch Full UI System\n" +
                "F1 - Main Menu\n" +
                "F2 - Alternative Launch\n" +
                "F3 - Pause Menu\n" +
                "F4 - Settings\n" +
                "F5 - Test Notification\n" +
                "ESC - Toggle Pause");
        }
    }
}
