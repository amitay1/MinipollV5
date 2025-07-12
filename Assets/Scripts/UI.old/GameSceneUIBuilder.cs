using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MinipollGame.UI
{
    /// <summary>
    /// Game Scene UI Builder - Automatically creates Feed/Play/Clean buttons and Needs UI for 02_GameScene
    /// This utility creates the required UI elements that the tutorial system expects
    /// </summary>
    public class GameSceneUIBuilder : MonoBehaviour
    {
        [Header("🏗️ UI Creation Settings")]
        [SerializeField] private bool autoCreateOnStart = true;
        [SerializeField] private bool replaceExisting = false;
        
        [Header("📐 Layout Settings")]
        [SerializeField] private Vector2 buttonSize = new Vector2(120, 60);
        [SerializeField] private float buttonSpacing = 20f;
        [SerializeField] private Vector2 needsPanelSize = new Vector2(200, 150);
        
        [Header("🎨 Styling")]
        [SerializeField] private Font buttonFont;
        [SerializeField] private int buttonFontSize = 16;
        [SerializeField] private Color feedButtonColor = new Color(0.3f, 0.8f, 0.3f, 1f); // Green
        [SerializeField] private Color playButtonColor = new Color(0.3f, 0.5f, 0.9f, 1f); // Blue
        [SerializeField] private Color cleanButtonColor = new Color(0.8f, 0.8f, 0.3f, 1f); // Yellow
        
        private Canvas gameCanvas;
        private GameObject actionButtonsPanel;
        private GameObject needsUIPanel;
        
        void Start()
        {
            if (autoCreateOnStart)
            {
                CreateGameSceneUI();
            }
        }
        
        [ContextMenu("🏗️ Create Game Scene UI")]
        public void CreateGameSceneUI()
        {
            Debug.Log("🏗️ Building Game Scene UI for 02_GameScene...");
            
            // Find or create main canvas
            FindOrCreateCanvas();
            
            // Create action buttons (Feed, Play, Clean)
            CreateActionButtons();
            
            // Create needs display panel
            CreateNeedsUI();
            
            // Connect to GameSceneUI controller
            SetupGameSceneUIController();
            
            Debug.Log("✅ Game Scene UI creation complete!");
        }
        
        void FindOrCreateCanvas()
        {
            gameCanvas = FindFirstObjectByType<Canvas>();
            
            if (gameCanvas == null)
            {
                Debug.Log("📱 Creating new Canvas for Game Scene UI");
                GameObject canvasObj = new GameObject("GameCanvas");
                gameCanvas = canvasObj.AddComponent<Canvas>();
                gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameCanvas.sortingOrder = 10; // Above other UI
                
                // Add CanvasScaler for responsive design
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                // Add GraphicRaycaster for UI interaction
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                Debug.Log($"📱 Using existing Canvas: {gameCanvas.name}");
            }
        }
        
        void CreateActionButtons()
        {
            // Check if action buttons panel already exists
            GameObject existingPanel = GameObject.Find("ActionButtonsPanel");
            if (existingPanel != null && !replaceExisting)
            {
                Debug.Log("🎮 Action buttons panel already exists, skipping creation");
                actionButtonsPanel = existingPanel;
                return;
            }
            
            if (existingPanel != null && replaceExisting)
            {
                DestroyImmediate(existingPanel);
                Debug.Log("🗑️ Replaced existing action buttons panel");
            }
            
            // Create action buttons container
            actionButtonsPanel = new GameObject("ActionButtonsPanel");
            actionButtonsPanel.transform.SetParent(gameCanvas.transform, false);
            
            RectTransform panelRect = actionButtonsPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0, 100); // Bottom center, slightly up
            panelRect.sizeDelta = new Vector2((buttonSize.x + buttonSpacing) * 3, buttonSize.y);
            
            // Add horizontal layout group
            HorizontalLayoutGroup layout = actionButtonsPanel.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = buttonSpacing;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;
            
            // Create individual buttons
            CreateButton("FeedButton", "🍎 FEED", feedButtonColor, actionButtonsPanel.transform);
            CreateButton("PlayButton", "⚽ PLAY", playButtonColor, actionButtonsPanel.transform);
            CreateButton("CleanButton", "🧼 CLEAN", cleanButtonColor, actionButtonsPanel.transform);
            
            Debug.Log("🎮 Created action buttons: Feed, Play, Clean");
        }
        
        GameObject CreateButton(string buttonName, string buttonText, Color buttonColor, Transform parent)
        {
            // Check if button already exists
            GameObject existingButton = GameObject.Find(buttonName);
            if (existingButton != null && !replaceExisting)
            {
                Debug.Log($"🎮 Button {buttonName} already exists, skipping");
                return existingButton;
            }
            
            if (existingButton != null && replaceExisting)
            {
                DestroyImmediate(existingButton);
            }
            
            GameObject buttonObj = new GameObject(buttonName);
            buttonObj.transform.SetParent(parent, false);
            
            // Setup RectTransform
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = buttonSize;
            
            // Add Image component for button background
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = buttonColor;
            buttonImage.type = Image.Type.Sliced;
            
            // Add Button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            // Create button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Try to use TextMeshPro if available, fallback to Text
            if (buttonFont != null)
            {
                Text text = textObj.AddComponent<Text>();
                text.text = buttonText;
                text.font = buttonFont;
                text.fontSize = buttonFontSize;
                text.color = Color.white;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontStyle = FontStyle.Bold;
            }
            else
            {
                // Try TextMeshPro
                try
                {
                    TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
                    tmpText.text = buttonText;
                    tmpText.fontSize = buttonFontSize;
                    tmpText.color = Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.fontStyle = FontStyles.Bold;
                }
                catch
                {
                    // Fallback to regular Text
                    Text text = textObj.AddComponent<Text>();
                    text.text = buttonText;
                    text.fontSize = buttonFontSize;
                    text.color = Color.white;
                    text.alignment = TextAnchor.MiddleCenter;
                    text.fontStyle = FontStyle.Bold;
                }
            }
            
            Debug.Log($"✅ Created button: {buttonName}");
            return buttonObj;
        }
        
        void CreateNeedsUI()
        {
            // Check if needs UI already exists
            GameObject existingNeedsUI = GameObject.Find("NeedsUI");
            if (existingNeedsUI != null && !replaceExisting)
            {
                Debug.Log("📊 NeedsUI already exists, skipping creation");
                needsUIPanel = existingNeedsUI;
                return;
            }
            
            if (existingNeedsUI != null && replaceExisting)
            {
                DestroyImmediate(existingNeedsUI);
                Debug.Log("🗑️ Replaced existing NeedsUI");
            }
            
            // Create needs panel
            needsUIPanel = new GameObject("NeedsUI");
            needsUIPanel.transform.SetParent(gameCanvas.transform, false);
            
            RectTransform needsRect = needsUIPanel.AddComponent<RectTransform>();
            needsRect.anchorMin = new Vector2(0f, 1f);
            needsRect.anchorMax = new Vector2(0f, 1f);
            needsRect.anchoredPosition = new Vector2(20, -20); // Top left corner
            needsRect.sizeDelta = needsPanelSize;
            
            // Add background panel
            Image panelBg = needsUIPanel.AddComponent<Image>();
            panelBg.color = new Color(0f, 0f, 0f, 0.3f); // Semi-transparent background
            
            // Add vertical layout group
            VerticalLayoutGroup layout = needsUIPanel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5f;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Create title
            CreateNeedsTitle();
            
            // Create need sliders
            CreateNeedSlider("HungerSlider", "🍎 Hunger", Color.green);
            CreateNeedSlider("EnergySlider", "⚡ Energy", Color.yellow);
            CreateNeedSlider("HappinessSlider", "😊 Happiness", Color.cyan);
            CreateNeedSlider("CleanlinessSlider", "🧼 Clean", Color.blue);
            
            // Initially hide the panel (will be shown when Minipoll is selected)
            needsUIPanel.SetActive(false);
            
            Debug.Log("📊 Created NeedsUI panel with sliders");
        }
        
        void CreateNeedsTitle()
        {
            GameObject titleObj = new GameObject("NeedsTitle");
            titleObj.transform.SetParent(needsUIPanel.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 25);
            
            try
            {
                TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
                titleText.text = "🎯 Minipoll Needs";
                titleText.fontSize = 14;
                titleText.color = Color.white;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.fontStyle = FontStyles.Bold;
            }
            catch
            {
                Text titleText = titleObj.AddComponent<Text>();
                titleText.text = "🎯 Minipoll Needs";
                titleText.fontSize = 14;
                titleText.color = Color.white;
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.fontStyle = FontStyle.Bold;
            }
        }
        
        void CreateNeedSlider(string sliderName, string labelText, Color sliderColor)
        {
            GameObject sliderObj = new GameObject(sliderName);
            sliderObj.transform.SetParent(needsUIPanel.transform, false);
            
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(0, 20);
            
            // Create slider
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f; // Default middle value
            
            // Create background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            
            // Create fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillRect = fillArea.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            // Create fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillImageRect = fill.AddComponent<RectTransform>();
            fillImageRect.anchorMin = Vector2.zero;
            fillImageRect.anchorMax = Vector2.one;
            fillImageRect.offsetMin = Vector2.zero;
            fillImageRect.offsetMax = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = sliderColor;
            
            // Setup slider references
            slider.fillRect = fillImageRect;
            
            // Create label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(sliderObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.offsetMin = new Vector2(5, 0);
            labelRect.offsetMax = new Vector2(-5, 0);
            
            try
            {
                TextMeshProUGUI labelTextTMP = labelObj.AddComponent<TextMeshProUGUI>();
                labelTextTMP.text = labelText;
                labelTextTMP.fontSize = 10;
                labelTextTMP.color = Color.white;
                labelTextTMP.alignment = TextAlignmentOptions.MidlineLeft;
            }
            catch
            {
                Text labelTextUI = labelObj.AddComponent<Text>();
                labelTextUI.text = labelText;
                labelTextUI.fontSize = 10;
                labelTextUI.color = Color.white;
                labelTextUI.alignment = TextAnchor.MiddleLeft;
            }
        }
        
        void SetupGameSceneUIController()
        {
            // Find or create GameSceneUI controller
            GameSceneUI controller = FindFirstObjectByType<GameSceneUI>();
            if (controller == null)
            {
                Debug.Log("🎮 Creating GameSceneUI controller");
                GameObject controllerObj = new GameObject("GameSceneUIController");
                controller = controllerObj.AddComponent<GameSceneUI>();
            }
            
            // The controller will auto-find the UI elements we created
            Debug.Log("🔗 GameSceneUI controller ready");
        }
        
        [ContextMenu("🗑️ Clean Up UI")]
        public void CleanUpUI()
        {
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
                    Debug.Log($"🗑️ Removed {obj.name}");
                }
            }
            
            Debug.Log("🗑️ UI cleanup complete");
        }
    }
}
