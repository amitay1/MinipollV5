using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MinipollGame.UI
{
    /// <summary>
    /// Builder script to create comprehensive UI for all Minipoll game systems
    /// This will create a beautiful and functional UI displaying all system information
    /// </summary>
    public class MinipollGameUIBuilder : MonoBehaviour
    {
        [Header("UI Configuration")]
        public bool buildUIOnStart = true;
        public Font uiFont;
        
        [Header("Colors")]
        public Color panelColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        public Color textColor = Color.white;
        public Color accentColor = new Color(0.3f, 0.7f, 1f, 1f); // Cyan
        
        // References to created UI elements
        private Canvas mainCanvas;
        private GameObject topPanel;
        private GameObject leftPanel;
        private GameObject rightPanel;
        private GameObject bottomPanel;
        
        void Start()
        {
            if (buildUIOnStart)
            {
                BuildCompleteUI();
            }
        }

        public void BuildCompleteUI()
        {
            Debug.Log("🎨 Building comprehensive Minipoll Game UI...");
            
            // 1. Create main canvas
            CreateMainCanvas();
            
            // 2. Create main panels
            CreateTopPanel();
            CreateLeftPanel();
            CreateRightPanel();
            CreateBottomPanel();
            
            // 3. Populate panels with system information
            PopulateTopPanel();
            PopulateLeftPanel();
            PopulateRightPanel();
            PopulateBottomPanel();
            
            Debug.Log("✅ UI Building complete!");
        }

        private void CreateMainCanvas()
        {
            GameObject canvasGO = new GameObject("MinipollGameUI");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;
            
            // Add CanvasScaler for responsive UI
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // Add GraphicRaycaster for interactions
            canvasGO.AddComponent<GraphicRaycaster>();
            
            Debug.Log("📱 Main Canvas created");
        }

        private void CreateTopPanel()
        {
            topPanel = CreatePanel("TopPanel", mainCanvas.transform);
            RectTransform rect = topPanel.GetComponent<RectTransform>();
            
            // Position at top of screen
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(0, -80);
            rect.offsetMax = new Vector2(0, 0);
            
            Debug.Log("⬆️ Top Panel created");
        }

        private void CreateLeftPanel()
        {
            leftPanel = CreatePanel("LeftPanel", mainCanvas.transform);
            RectTransform rect = leftPanel.GetComponent<RectTransform>();
            
            // Position at left side of screen
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 1);
            rect.offsetMin = new Vector2(0, 80);
            rect.offsetMax = new Vector2(300, -80);
            
            Debug.Log("⬅️ Left Panel created");
        }

        private void CreateRightPanel()
        {
            rightPanel = CreatePanel("RightPanel", mainCanvas.transform);
            RectTransform rect = rightPanel.GetComponent<RectTransform>();
            
            // Position at right side of screen
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(-300, 80);
            rect.offsetMax = new Vector2(0, -80);
            
            Debug.Log("➡️ Right Panel created");
        }

        private void CreateBottomPanel()
        {
            bottomPanel = CreatePanel("BottomPanel", mainCanvas.transform);
            RectTransform rect = bottomPanel.GetComponent<RectTransform>();
            
            // Position at bottom of screen
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.offsetMin = new Vector2(300, 0);
            rect.offsetMax = new Vector2(-300, 80);
            
            Debug.Log("⬇️ Bottom Panel created");
        }

        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            // Add RectTransform
            RectTransform rect = panel.AddComponent<RectTransform>();
            
            // Add Image component for background
            Image image = panel.AddComponent<Image>();
            image.color = panelColor;
            
            return panel;
        }

        private void PopulateTopPanel()
        {
            // Create title
            CreateText("GameTitle", topPanel.transform, "🐧 MINIPOLL SIMULATION V5", 24, TextAnchor.MiddleCenter);
            
            // Create horizontal layout for top info
            GameObject infoContainer = CreateHorizontalLayout("TopInfo", topPanel.transform);
            
            // Game time and day
            CreateText("TimeInfo", infoContainer.transform, "Day: 1 | Time: 06:00", 16, TextAnchor.MiddleLeft);
            
            // Weather
            CreateText("WeatherInfo", infoContainer.transform, "Weather: Sunny ☀️", 16, TextAnchor.MiddleCenter);
            
            // Game speed control
            GameObject speedContainer = CreateHorizontalLayout("SpeedControl", infoContainer.transform);
            CreateText("SpeedLabel", speedContainer.transform, "Speed:", 14, TextAnchor.MiddleLeft);
            CreateSlider("SpeedSlider", speedContainer.transform, 0.1f, 3f, 1f);
            
            Debug.Log("⬆️ Top Panel populated with game info");
        }

        private void PopulateLeftPanel()
        {
            // Population & Statistics Panel
            CreateSectionHeader("PopulationHeader", leftPanel.transform, "📊 POPULATION & STATS");
            
            GameObject populationContainer = CreateVerticalLayout("PopulationStats", leftPanel.transform);
            
            // Population count
            CreateText("MinipollCount", populationContainer.transform, "Minipolls: 1", 14, TextAnchor.MiddleLeft);
            CreateText("PopulationGrowth", populationContainer.transform, "Growth Rate: +0.1/day", 12, TextAnchor.MiddleLeft);
            
            // Age distribution
            CreateSectionHeader("AgeHeader", populationContainer.transform, "Age Distribution:");
            CreateText("BabiesCount", populationContainer.transform, "👶 Babies: 0", 12, TextAnchor.MiddleLeft);
            CreateText("ChildrenCount", populationContainer.transform, "🧒 Children: 0", 12, TextAnchor.MiddleLeft);
            CreateText("AdultsCount", populationContainer.transform, "🧑 Adults: 1", 12, TextAnchor.MiddleLeft);
            CreateText("EldersCount", populationContainer.transform, "👴 Elders: 0", 12, TextAnchor.MiddleLeft);
            
            // Resources
            CreateSectionHeader("ResourcesHeader", populationContainer.transform, "🏪 RESOURCES");
            CreateText("FoodStatus", populationContainer.transform, "🍎 Food: Available", 12, TextAnchor.MiddleLeft);
            CreateText("WaterStatus", populationContainer.transform, "💧 Water: Available", 12, TextAnchor.MiddleLeft);
            CreateText("ShelterStatus", populationContainer.transform, "🏠 Shelter: Available", 12, TextAnchor.MiddleLeft);
            
            Debug.Log("⬅️ Left Panel populated with population stats");
        }

        private void PopulateRightPanel()
        {
            // Selected Minipoll Info Panel
            CreateSectionHeader("SelectedHeader", rightPanel.transform, "🎯 SELECTED MINIPOLL");
            
            GameObject selectedContainer = CreateVerticalLayout("SelectedMinipollInfo", rightPanel.transform);
            
            // Basic info
            CreateText("SelectedName", selectedContainer.transform, "Name: Bubbleina", 14, TextAnchor.MiddleLeft);
            CreateText("SelectedGender", selectedContainer.transform, "Gender: Female ♀", 12, TextAnchor.MiddleLeft);
            CreateText("SelectedAge", selectedContainer.transform, "Age: Adult (Day 0)", 12, TextAnchor.MiddleLeft);
            
            // Health and status
            CreateSectionHeader("HealthHeader", selectedContainer.transform, "❤️ HEALTH & STATUS");
            CreateProgressBar("HealthBar", selectedContainer.transform, "Health", 1f, Color.red);
            CreateProgressBar("EnergyBar", selectedContainer.transform, "Energy", 0.8f, Color.yellow);
            
            // Needs system
            CreateSectionHeader("NeedsHeader", selectedContainer.transform, "🎯 NEEDS SYSTEM");
            CreateProgressBar("HungerBar", selectedContainer.transform, "Hunger", 0.7f, Color.green);
            CreateProgressBar("ThirstBar", selectedContainer.transform, "Thirst", 0.6f, Color.blue);
            CreateProgressBar("SleepBar", selectedContainer.transform, "Sleep", 0.9f, Color.purple);
            CreateProgressBar("SocialBar", selectedContainer.transform, "Social", 0.5f, Color.cyan);
            
            // Emotions
            CreateSectionHeader("EmotionsHeader", selectedContainer.transform, "😊 EMOTIONS");
            CreateText("CurrentEmotion", selectedContainer.transform, "Current: Happy 😊", 12, TextAnchor.MiddleLeft);
            CreateProgressBar("EmotionIntensity", selectedContainer.transform, "Intensity", 0.6f, Color.magenta);
            
            // Brain/AI Status
            CreateSectionHeader("AIHeader", selectedContainer.transform, "🧠 AI BRAIN STATUS");
            CreateText("CurrentGoal", selectedContainer.transform, "Goal: Explore", 12, TextAnchor.MiddleLeft);
            CreateText("CurrentAction", selectedContainer.transform, "Action: Walking", 12, TextAnchor.MiddleLeft);
            CreateText("AIDecisionTime", selectedContainer.transform, "Next Decision: 5s", 10, TextAnchor.MiddleLeft);
            
            Debug.Log("➡️ Right Panel populated with minipoll details");
        }

        private void PopulateBottomPanel()
        {
            // System Status Panel
            CreateSectionHeader("SystemsHeader", bottomPanel.transform, "⚙️ SYSTEM STATUS");
            
            GameObject systemsContainer = CreateHorizontalLayout("SystemsStatus", bottomPanel.transform);
            
            // Movement System
            GameObject movementStatus = CreateVerticalLayout("MovementSystem", systemsContainer.transform);
            CreateText("MovementTitle", movementStatus.transform, "🚶 Movement", 12, TextAnchor.MiddleCenter);
            CreateText("MovementStatus", movementStatus.transform, "✅ Active", 10, TextAnchor.MiddleCenter);
            
            // Animation System
            GameObject animationStatus = CreateVerticalLayout("AnimationSystem", systemsContainer.transform);
            CreateText("AnimationTitle", animationStatus.transform, "🎭 Animation", 12, TextAnchor.MiddleCenter);
            CreateText("AnimationStatus", animationStatus.transform, "✅ Sync OK", 10, TextAnchor.MiddleCenter);
            
            // NEEDSIM System
            GameObject needsimStatus = CreateVerticalLayout("NEEDSIMSystem", systemsContainer.transform);
            CreateText("NEEDSIMTitle", needsimStatus.transform, "🎯 NEEDSIM", 12, TextAnchor.MiddleCenter);
            CreateText("NEEDSIMStatus", needsimStatus.transform, "✅ Running", 10, TextAnchor.MiddleCenter);
            
            // Brain/AI System
            GameObject brainStatus = CreateVerticalLayout("BrainSystem", systemsContainer.transform);
            CreateText("BrainTitle", brainStatus.transform, "🧠 AI Brain", 12, TextAnchor.MiddleCenter);
            CreateText("BrainStatus", brainStatus.transform, "✅ Thinking", 10, TextAnchor.MiddleCenter);
            
            // Memory System
            GameObject memoryStatus = CreateVerticalLayout("MemorySystem", systemsContainer.transform);
            CreateText("MemoryTitle", memoryStatus.transform, "💭 Memory", 12, TextAnchor.MiddleCenter);
            CreateText("MemoryStatus", memoryStatus.transform, "✅ Learning", 10, TextAnchor.MiddleCenter);
            
            // Social System
            GameObject socialStatus = CreateVerticalLayout("SocialSystem", systemsContainer.transform);
            CreateText("SocialTitle", socialStatus.transform, "👥 Social", 12, TextAnchor.MiddleCenter);
            CreateText("SocialStatus", socialStatus.transform, "⚠️ Lonely", 10, TextAnchor.MiddleCenter);
            
            Debug.Log("⬇️ Bottom Panel populated with system status");
        }

        private void CreateSectionHeader(string name, Transform parent, string text)
        {
            GameObject header = CreateText(name, parent, text, 14, TextAnchor.MiddleLeft);
            Text textComponent = header.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.color = accentColor;
                textComponent.fontStyle = FontStyle.Bold;
            }
        }

        private GameObject CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, fontSize + 4);
            
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = textColor;
            textComponent.alignment = alignment;
            
            return textObj;
        }

        private GameObject CreateHorizontalLayout(string name, Transform parent)
        {
            GameObject layout = new GameObject(name);
            layout.transform.SetParent(parent, false);
            
            RectTransform rect = layout.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 30);
            
            HorizontalLayoutGroup hlg = layout.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            
            return layout;
        }

        private GameObject CreateVerticalLayout(string name, Transform parent)
        {
            GameObject layout = new GameObject(name);
            layout.transform.SetParent(parent, false);
            
            RectTransform rect = layout.AddComponent<RectTransform>();
            
            VerticalLayoutGroup vlg = layout.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            
            // Add ContentSizeFitter to auto-resize
            ContentSizeFitter csf = layout.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            return layout;
        }

        private GameObject CreateSlider(string name, Transform parent, float minValue, float maxValue, float currentValue)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);
            
            RectTransform rect = sliderObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 20);
            
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = currentValue;
            
            // Create background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Create fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillRect = fillArea.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillChildRect = fill.AddComponent<RectTransform>();
            fillChildRect.sizeDelta = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = accentColor;
            
            slider.fillRect = fillChildRect;
            
            return sliderObj;
        }

        private GameObject CreateProgressBar(string name, Transform parent, string label, float value, Color color)
        {
            GameObject container = CreateHorizontalLayout(name + "Container", parent);
            
            // Label
            CreateText(name + "Label", container.transform, label + ":", 10, TextAnchor.MiddleLeft);
            
            // Progress bar
            GameObject progressBar = CreateSlider(name + "Bar", container.transform, 0f, 1f, value);
            Slider slider = progressBar.GetComponent<Slider>();
            if (slider.fillRect != null)
            {
                Image fillImage = slider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = color;
                }
            }
            
            // Value text
            CreateText(name + "Value", container.transform, $"{value:P0}", 10, TextAnchor.MiddleRight);
            
            return container;
        }

        [ContextMenu("Build UI Now")]
        public void BuildUIFromEditor()
        {
            BuildCompleteUI();
        }

        [ContextMenu("Clear UI")]
        public void ClearUI()
        {
            if (mainCanvas != null)
            {
                DestroyImmediate(mainCanvas.gameObject);
                Debug.Log("🗑️ UI Cleared");
            }
        }
    }
}
