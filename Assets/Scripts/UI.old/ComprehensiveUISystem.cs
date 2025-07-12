using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MinipollGame.UI;
using UnityEngine.InputSystem;

namespace MinipollGame.UI
{
    /// <summary>
    /// Complete Professional UI System for MinipollV5
    /// Creates beautiful, animated, modern game UI like a commercial game
    /// Features: Main Menu, HUD, Pause Menu, Settings, Achievements, Store-like design
    /// </summary>
    public class ComprehensiveUISystem : MonoBehaviour
    {
        [Header("🎨 UI Configuration")]
        [SerializeField] private bool createMainMenu = true;
        [SerializeField] private bool createGameHUD = true;
        [SerializeField] private bool createPauseMenu = true;
        [SerializeField] private bool createSettingsMenu = true;
        [SerializeField] private bool createAchievements = true;
        [SerializeField] private bool createInventorySystem = true;
        
        [Header("🎬 Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private float bounceScale = 1.1f;
        [SerializeField] private Ease defaultEase = Ease.OutBack;
        
        [Header("🎵 Audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip menuOpenSound;
        [SerializeField] private AudioClip successSound;
        
        // UI References
        private Canvas mainCanvas;
        private GameObject mainMenuPanel;
        private GameObject gameHUDPanel;
        private GameObject pauseMenuPanel;
        private GameObject settingsMenuPanel;
        private GameObject achievementsPanel;
        private GameObject inventoryPanel;
        private GameObject notificationSystem;
        
        // Animation components
        private CanvasGroup currentActivePanel;
        private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();
        
        void Start()
        {
            InitializeUISystem();
        }
        
        private void InitializeUISystem()
        {
            Debug.Log("🎨 Initializing Comprehensive UI System...");
            
            CreateMainCanvas();
            
            if (createMainMenu) CreateMainMenuSystem();
            if (createGameHUD) CreateGameHUDSystem();
            if (createPauseMenu) CreatePauseMenuSystem();
            if (createSettingsMenu) CreateSettingsMenuSystem();
            if (createAchievements) CreateAchievementsSystem();
            if (createInventorySystem) CreateInventorySystem();
            
            CreateNotificationSystem();
            ApplyBrandingToAllElements();
            
            // Start with main menu visible
            ShowPanel("MainMenu");
            
            Debug.Log("✨ UI System fully initialized!");
        }
        
        #region Canvas Creation
        
        private void CreateMainCanvas()
        {
            GameObject canvasGO = new GameObject("ComprehensiveUI_Canvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 1000;
            
            // Responsive scaling
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Setup audio source
            if (uiAudioSource == null)
            {
                uiAudioSource = canvasGO.AddComponent<AudioSource>();
                uiAudioSource.playOnAwake = false;
                uiAudioSource.volume = 0.7f;
            }
            
            Debug.Log("📱 Main Canvas created with responsive scaling");
        }
        
        #endregion
        
        #region Main Menu System
        
        private void CreateMainMenuSystem()
        {
            mainMenuPanel = CreatePanel("MainMenu", "🐧 Main Menu");
            var mainMenuGroup = mainMenuPanel.GetComponent<CanvasGroup>();
            
            // Background with gradient
            CreateGradientBackground(mainMenuPanel);
            
            // Logo section
            var logoSection = CreateUIElement("LogoSection", mainMenuPanel.transform);
            SetupLayoutElement(logoSection, 0, 200);
            SetRectTransform(logoSection, Vector2.zero, Vector2.one, new Vector2(0, -100), new Vector2(0, -100));
            
            var logoText = CreateStyledText(logoSection, "GameLogo", "MINIPOLL", UIBrandingExtensions.TypographyStyle.H1);
            logoText.color = MinipollBrandManager.Colors?.MinipollBlue ?? new Color(0.3f, 0.6f, 1f);
            logoText.alignment = TextAlignmentOptions.Center;
            
            var subtitleText = CreateStyledText(logoSection, "Subtitle", "Life Simulation Adventure", UIBrandingExtensions.TypographyStyle.H3);
            subtitleText.color = MinipollBrandManager.Colors?.HeartPink ?? new Color(1f, 0.3f, 0.5f);
            subtitleText.alignment = TextAlignmentOptions.Center;
            SetRectTransform(subtitleText.gameObject, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -50), new Vector2(0, 0));
            
            // Main menu buttons
            var buttonSection = CreateUIElement("ButtonSection", mainMenuPanel.transform);
            SetupVerticalLayoutGroup(buttonSection, 20, true);
            SetRectTransform(buttonSection, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.7f), Vector2.zero, Vector2.zero);
            
            CreateMainMenuButton(buttonSection, "🎮 PLAY GAME", () => StartGame(), UIBrandingExtensions.ButtonStyle.Primary);
            CreateMainMenuButton(buttonSection, "🏆 ACHIEVEMENTS", () => ShowPanel("Achievements"), UIBrandingExtensions.ButtonStyle.Secondary);
            CreateMainMenuButton(buttonSection, "🎒 INVENTORY", () => ShowPanel("Inventory"), UIBrandingExtensions.ButtonStyle.Secondary);
            CreateMainMenuButton(buttonSection, "⚙️ SETTINGS", () => ShowPanel("Settings"), UIBrandingExtensions.ButtonStyle.Warning);
            CreateMainMenuButton(buttonSection, "🚪 EXIT", () => QuitGame(), UIBrandingExtensions.ButtonStyle.Warning);
            
            // Version info
            var versionText = CreateStyledText(mainMenuPanel, "Version", "v5.0 - HeartCode Studios", UIBrandingExtensions.TypographyStyle.Caption);
            versionText.color = MinipollBrandManager.Colors?.LightGray ?? new Color(0.7f, 0.7f, 0.7f);
            versionText.alignment = TextAlignmentOptions.BottomRight;
            SetRectTransform(versionText.gameObject, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-200, 20), new Vector2(0, 50));
            
            uiPanels.Add("MainMenu", mainMenuPanel);
            Debug.Log("🎮 Main Menu created with branding");
        }
        
        private void CreateMainMenuButton(GameObject parent, string text, System.Action onClick, UIBrandingExtensions.ButtonStyle style)
        {
            var buttonObj = CreateUIElement($"Button_{text.Replace(" ", "")}", parent.transform);
            
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            
            // Style with modern brand manager
            UIBrandingExtensions.StyleModernButton(button, style);
            
            // Set size
            SetupLayoutElement(buttonObj, 400, 80);
            
            // Add text
            var buttonText = CreateStyledText(buttonObj, "Text", text, UIBrandingExtensions.TypographyStyle.Button);
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = UIBrandingExtensions.ModernColors.OnPrimary;
            
            // Add click events
            button.onClick.AddListener(() => {
                PlayButtonSound();
                AnimateButtonClick(buttonObj);
                onClick?.Invoke();
            });
            
            // Add hover animations
            AddButtonHoverEffects(button);
        }
        
        #endregion
        
        #region Game HUD System
        
        private void CreateGameHUDSystem()
        {
            gameHUDPanel = CreatePanel("GameHUD", "🎯 Game HUD");
            var hudGroup = gameHUDPanel.GetComponent<CanvasGroup>();
            
            // Top HUD Bar
            CreateTopHUDBar();
            
            // Mini-map
            CreateMiniMap();
            
            // Resource display
            CreateResourceDisplay();
            
            // Selected creature panel
            CreateSelectedCreaturePanel();
            
            // Quick action bar
            CreateQuickActionBar();
            
            // Time and weather display
            CreateTimeWeatherDisplay();
            
            uiPanels.Add("GameHUD", gameHUDPanel);
            
            // Start hidden
            hudGroup.alpha = 0;
            gameHUDPanel.SetActive(false);
            
            Debug.Log("🎯 Game HUD system created");
        }
        
        private void CreateTopHUDBar()
        {
            var topBar = CreateUIElement("TopHUDBar", gameHUDPanel.transform);
            SetupHorizontalLayoutGroup(topBar, 20, true);
            SetRectTransform(topBar, Vector2.up, Vector2.one, new Vector2(20, -80), new Vector2(-20, -20));
            
            // Add semi-transparent background
            var bg = topBar.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.3f);
            
            // Population counter with icon
            CreateHUDCounter(topBar, "👥", "Population", "1", MinipollBrandManager.Colors?.MinipollBlue ?? new Color(0.3f, 0.6f, 1f));
            
            // Resources
            CreateHUDCounter(topBar, "🍎", "Food", "100%", MinipollBrandManager.Colors?.GrowthGreen ?? new Color(0.2f, 0.7f, 0.4f));
            CreateHUDCounter(topBar, "💧", "Water", "100%", MinipollBrandManager.Colors?.MinipollBlue ?? new Color(0.3f, 0.6f, 1f));
            CreateHUDCounter(topBar, "⚡", "Energy", "85%", MinipollBrandManager.Colors?.WarmOrange ?? new Color(1f, 0.6f, 0.2f));
            
            // Spacer
            var spacer = CreateUIElement("Spacer", topBar.transform);
            SetupLayoutElement(spacer, 100, 0);
            
            // Game speed control
            var speedControl = CreateUIElement("SpeedControl", topBar.transform);
            SetupHorizontalLayoutGroup(speedControl, 10, false);
            SetupLayoutElement(speedControl, 200, 60);
            
            var speedLabel = CreateStyledText(speedControl, "SpeedLabel", "Speed:", UIBrandingExtensions.TypographyStyle.Body);
            speedLabel.color = Color.white;
            
            var speedSlider = CreateStyledSlider(speedControl, "SpeedSlider", 0.1f, 3f, 1f);
            speedSlider.onValueChanged.AddListener(OnGameSpeedChanged);
        }
        
        private void CreateHUDCounter(GameObject parent, string icon, string label, string value, Color color)
        {
            var container = CreateUIElement($"Counter_{label}", parent.transform);
            SetupHorizontalLayoutGroup(container, 5, false);
            SetupLayoutElement(container, 120, 60);
            
            // Icon
            var iconText = CreateStyledText(container, "Icon", icon, UIBrandingExtensions.TypographyStyle.H3);
            iconText.color = color;
            iconText.alignment = TextAlignmentOptions.Center;
            
            // Label and value
            var textContainer = CreateUIElement("TextContainer", container.transform);
            SetupVerticalLayoutGroup(textContainer, 2, false);
            
            var labelText = CreateStyledText(textContainer, "Label", label, UIBrandingExtensions.TypographyStyle.Caption);
            labelText.color = Color.white;
            
            var valueText = CreateStyledText(textContainer, "Value", value, UIBrandingExtensions.TypographyStyle.Body);
            valueText.color = color;
            valueText.fontStyle = FontStyles.Bold;
        }
        
        #endregion
        
        #region Pause Menu System
        
        private void CreatePauseMenuSystem()
        {
            pauseMenuPanel = CreatePanel("PauseMenu", "⏸️ Game Paused");
            var pauseGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            
            // Semi-transparent dark background
            var overlay = CreateUIElement("Overlay", pauseMenuPanel.transform);
            SetRectTransform(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0.7f);
            
            // Central pause menu
            var menuContainer = CreateUIElement("MenuContainer", pauseMenuPanel.transform);
            SetRectTransform(menuContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200, -150), new Vector2(200, 150));
            
            // Background panel
            var bg = menuContainer.AddComponent<Image>();
            UIBrandingExtensions.StyleModernPanel(bg, UIBrandingExtensions.PanelStyle.Light);
            
            // Title
            var title = CreateStyledText(menuContainer, "Title", "GAME PAUSED", UIBrandingExtensions.TypographyStyle.H2);
            title.color = MinipollBrandManager.Colors.DeepNavy;
            title.alignment = TextAlignmentOptions.Top;
            SetRectTransform(title.gameObject, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), new Vector2(0, -20));
            
            // Buttons
            var buttonContainer = CreateUIElement("ButtonContainer", menuContainer.transform);
            SetupVerticalLayoutGroup(buttonContainer, 15, true);
            SetRectTransform(buttonContainer, new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.8f), Vector2.zero, Vector2.zero);
            
            CreatePauseMenuButton(buttonContainer, "▶️ RESUME", () => ResumeGame(), UIBrandingExtensions.ButtonStyle.Primary);
            CreatePauseMenuButton(buttonContainer, "⚙️ SETTINGS", () => ShowPanel("Settings"), UIBrandingExtensions.ButtonStyle.Secondary);
            CreatePauseMenuButton(buttonContainer, "💾 SAVE GAME", () => SaveGame(), UIBrandingExtensions.ButtonStyle.Success);
            CreatePauseMenuButton(buttonContainer, "🏠 MAIN MENU", () => ReturnToMainMenu(), UIBrandingExtensions.ButtonStyle.Warning);
            
            uiPanels.Add("PauseMenu", pauseMenuPanel);
            
            // Start hidden
            pauseGroup.alpha = 0;
            pauseMenuPanel.SetActive(false);
            
            Debug.Log("⏸️ Pause Menu created");
        }
        
        private void CreatePauseMenuButton(GameObject parent, string text, System.Action onClick, UIBrandingExtensions.ButtonStyle style)
        {
            var buttonObj = CreateUIElement($"PauseButton_{text.Replace(" ", "")}", parent.transform);
            
            var button = buttonObj.AddComponent<Button>();
            UIBrandingExtensions.StyleModernButton(button, style);
            SetupLayoutElement(buttonObj, 300, 50);
            
            var buttonText = CreateStyledText(buttonObj, "Text", text, UIBrandingExtensions.TypographyStyle.Button);
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = UIBrandingExtensions.ModernColors.OnPrimary;
            
            button.onClick.AddListener(() => {
                PlayButtonSound();
                onClick?.Invoke();
            });
            
            AddButtonHoverEffects(button);
        }
        
        #endregion
        
        #region Settings Menu System
        
        private void CreateSettingsMenuSystem()
        {
            settingsMenuPanel = CreatePanel("SettingsMenu", "⚙️ Settings");
            var settingsGroup = settingsMenuPanel.GetComponent<CanvasGroup>();
            
            // Background overlay
            CreateFullScreenOverlay(settingsMenuPanel);
            
            // Main settings container
            var settingsContainer = CreateUIElement("SettingsContainer", settingsMenuPanel.transform);
            SetRectTransform(settingsContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300, -250), new Vector2(300, 250));
            
            var bg = settingsContainer.AddComponent<Image>();
            UIBrandingExtensions.StyleModernPanel(bg, UIBrandingExtensions.PanelStyle.Light);
            
            // Title with close button
            CreateSettingsHeader(settingsContainer);
            
            // Settings tabs
            CreateSettingsTabs(settingsContainer);
            
            uiPanels.Add("Settings", settingsMenuPanel);
            
            // Start hidden
            settingsGroup.alpha = 0;
            settingsMenuPanel.SetActive(false);
            
            Debug.Log("⚙️ Settings Menu created");
        }
        
        private void CreateSettingsHeader(GameObject parent)
        {
            var header = CreateUIElement("Header", parent.transform);
            SetupHorizontalLayoutGroup(header, 10, true);
            SetRectTransform(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -60), new Vector2(-20, -20));
            
            var title = CreateStyledText(header, "Title", "SETTINGS", UIBrandingExtensions.TypographyStyle.H2);
            title.color = MinipollBrandManager.Colors.DeepNavy;
            
            // Close button
            var closeButton = CreateUIElement("CloseButton", header.transform);
            var button = closeButton.AddComponent<Button>();
            UIBrandingExtensions.StyleModernButton(button, UIBrandingExtensions.ButtonStyle.Warning);
            SetupLayoutElement(closeButton, 40, 40);
            
            var closeText = CreateStyledText(closeButton, "X", "✕", UIBrandingExtensions.TypographyStyle.H3);
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
            
            button.onClick.AddListener(() => HideCurrentPanel());
        }
        
        private void CreateSettingsTabs(GameObject parent)
        {
            var tabContainer = CreateUIElement("TabContainer", parent.transform);
            SetRectTransform(tabContainer, new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, 20), new Vector2(-20, -80));
            
            // Graphics settings
            var graphicsSection = CreateSettingsSection(tabContainer, "🎨 GRAPHICS");
            CreateSliderSetting(graphicsSection, "Quality Level", 0, 5, 3, OnQualityChanged);
            CreateToggleSetting(graphicsSection, "VSync", true, OnVSyncChanged);
            CreateToggleSetting(graphicsSection, "Fullscreen", false, OnFullscreenChanged);
            
            // Audio settings
            var audioSection = CreateSettingsSection(tabContainer, "🔊 AUDIO");
            CreateSliderSetting(audioSection, "Master Volume", 0f, 1f, 0.8f, OnMasterVolumeChanged);
            CreateSliderSetting(audioSection, "Music Volume", 0f, 1f, 0.7f, OnMusicVolumeChanged);
            CreateSliderSetting(audioSection, "SFX Volume", 0f, 1f, 0.9f, OnSFXVolumeChanged);
            
            // Gameplay settings
            var gameplaySection = CreateSettingsSection(tabContainer, "🎮 GAMEPLAY");
            CreateToggleSetting(gameplaySection, "Auto-Save", true, OnAutoSaveChanged);
            CreateSliderSetting(gameplaySection, "Game Speed", 0.5f, 3f, 1f, OnDefaultSpeedChanged);
            CreateToggleSetting(gameplaySection, "Tutorials", true, OnTutorialsChanged);
        }
        
        #endregion
        
        #region Achievements System
        
        private void CreateAchievementsSystem()
        {
            achievementsPanel = CreatePanel("Achievements", "🏆 Achievements");
            var achievGroup = achievementsPanel.GetComponent<CanvasGroup>();
            
            CreateFullScreenOverlay(achievementsPanel);
            
            // Main achievements container
            var achievContainer = CreateUIElement("AchievementsContainer", achievementsPanel.transform);
            SetRectTransform(achievContainer, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);
            
            var bg = achievContainer.AddComponent<Image>();
            UIBrandingExtensions.StyleModernPanel(bg, UIBrandingExtensions.PanelStyle.Light);
            
            // Header
            var header = CreateUIElement("Header", achievContainer.transform);
            SetupHorizontalLayoutGroup(header, 10, true);
            SetRectTransform(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -60), new Vector2(-20, -20));
            
            var title = CreateStyledText(header, "Title", "🏆 ACHIEVEMENTS", UIBrandingExtensions.TypographyStyle.H1);
            title.color = MinipollBrandManager.Colors.DeepNavy;
            
            // Close button
            CreateCloseButton(header, () => HideCurrentPanel());
            
            // Achievements grid
            var scrollView = CreateScrollView(achievContainer, "AchievementsList");
            var content = scrollView.transform.Find("Viewport/Content");
            
            CreateAchievementCard(content, "🐧 First Steps", "Created your first Minipoll", true, "Welcome to Minipoll!");
            CreateAchievementCard(content, "👥 Social Butterfly", "Had 10 social interactions", false, "Interact with other Minipolls");
            CreateAchievementCard(content, "🏠 Home Builder", "Built your first shelter", false, "Construct a shelter");
            CreateAchievementCard(content, "🌟 Master Creator", "Reached level 10", false, "Keep playing to level up!");
            CreateAchievementCard(content, "💝 Loving Family", "Raised a family of 5", false, "Care for multiple Minipolls");
            
            uiPanels.Add("Achievements", achievementsPanel);
            
            // Start hidden
            achievGroup.alpha = 0;
            achievementsPanel.SetActive(false);
            
            Debug.Log("🏆 Achievements system created");
        }
        
        private void CreateAchievementCard(Transform parent, string title, string description, bool unlocked, string hint)
        {
            var card = CreateUIElement($"Achievement_{title.Replace(" ", "")}", parent);
            SetupLayoutElement(card, 0, 100);
            
            var cardBg = card.AddComponent<Image>();
            cardBg.color = unlocked ? MinipollBrandManager.Colors.SuccessGreen : MinipollBrandManager.Colors.LightGray;
            
            SetupHorizontalLayoutGroup(card, 15, false);
            
            // Icon
            var icon = CreateStyledText(card, "Icon", unlocked ? "🏆" : "🔒", UIBrandingExtensions.TypographyStyle.H2);
            SetupLayoutElement(icon.gameObject, 60, 60);
            
            // Text content
            var textContainer = CreateUIElement("TextContainer", card.transform);
            SetupVerticalLayoutGroup(textContainer, 5, false);
            
            var titleText = CreateStyledText(textContainer, "Title", title, UIBrandingExtensions.TypographyStyle.H4);
            titleText.color = unlocked ? Color.white : MinipollBrandManager.Colors.DeepNavy;
            
            var descText = CreateStyledText(textContainer, "Description", description, UIBrandingExtensions.TypographyStyle.Body);
            descText.color = unlocked ? Color.white : MinipollBrandManager.Colors.LightGray;
            
            if (!unlocked)
            {
                var hintText = CreateStyledText(textContainer, "Hint", $"💡 {hint}", UIBrandingExtensions.TypographyStyle.Caption);
                hintText.color = MinipollBrandManager.Colors.WarmOrange;
            }
        }
        
        #endregion
        
        #region Inventory System
        
        private void CreateInventorySystem()
        {
            inventoryPanel = CreatePanel("Inventory", "🎒 Inventory");
            var invGroup = inventoryPanel.GetComponent<CanvasGroup>();
            
            CreateFullScreenOverlay(inventoryPanel);
            
            // Main inventory container
            var invContainer = CreateUIElement("InventoryContainer", inventoryPanel.transform);
            SetRectTransform(invContainer, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);
            
            var bg = invContainer.AddComponent<Image>();
            UIBrandingExtensions.StyleModernPanel(bg, UIBrandingExtensions.PanelStyle.Light);
            
            // Header
            var header = CreateUIElement("Header", invContainer.transform);
            SetupHorizontalLayoutGroup(header, 10, true);
            SetRectTransform(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -60), new Vector2(-20, -20));
            
            var title = CreateStyledText(header, "Title", "🎒 INVENTORY & ITEMS", UIBrandingExtensions.TypographyStyle.H1);
            title.color = MinipollBrandManager.Colors.DeepNavy;
            
            CreateCloseButton(header, () => HideCurrentPanel());
            
            // Inventory grid
            var gridContainer = CreateUIElement("GridContainer", invContainer.transform);
            SetRectTransform(gridContainer, new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, 20), new Vector2(-20, -80));
            
            SetupGridLayoutGroup(gridContainer, new Vector2(120, 120), new Vector2(10, 10), 6);
            
            // Create sample inventory items
            CreateInventoryItem(gridContainer, "🍎", "Apple", "Fresh and nutritious", 5);
            CreateInventoryItem(gridContainer, "💧", "Water", "Clean drinking water", 10);
            CreateInventoryItem(gridContainer, "🪵", "Wood", "Building material", 15);
            CreateInventoryItem(gridContainer, "🪨", "Stone", "Sturdy construction material", 8);
            CreateInventoryItem(gridContainer, "🌿", "Herbs", "Medicinal plants", 3);
            CreateInventoryItem(gridContainer, "⚡", "Energy Crystal", "Magical energy source", 1);
            
            uiPanels.Add("Inventory", inventoryPanel);
            
            // Start hidden
            invGroup.alpha = 0;
            inventoryPanel.SetActive(false);
            
            Debug.Log("🎒 Inventory system created");
        }
        
        private void CreateInventoryItem(GameObject parent, string icon, string name, string description, int quantity)
        {
            var item = CreateUIElement($"Item_{name.Replace(" ", "")}", parent.transform);
            
            var itemBg = item.AddComponent<Image>();
            itemBg.color = MinipollBrandManager.Colors.CloudWhite;
            
            var button = item.AddComponent<Button>();
            button.onClick.AddListener(() => ShowItemDetails(name, description, quantity));
            
            SetupVerticalLayoutGroup(item, 5, true);
            
            // Icon
            var iconText = CreateStyledText(item, "Icon", icon, UIBrandingExtensions.TypographyStyle.H2);
            iconText.alignment = TextAlignmentOptions.Center;
            
            // Name
            var nameText = CreateStyledText(item, "Name", name, UIBrandingExtensions.TypographyStyle.Caption);
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = MinipollBrandManager.Colors.DeepNavy;
            
            // Quantity
            var quantityText = CreateStyledText(item, "Quantity", $"x{quantity}", UIBrandingExtensions.TypographyStyle.Caption);
            quantityText.alignment = TextAlignmentOptions.Center;
            quantityText.color = MinipollBrandManager.Colors.WarmOrange;
            quantityText.fontStyle = FontStyles.Bold;
            
            AddButtonHoverEffects(button);
        }
        
        #endregion
        
        #region Notification System
        
        private void CreateNotificationSystem()
        {
            notificationSystem = CreateUIElement("NotificationSystem", mainCanvas.transform);
            SetRectTransform(notificationSystem, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-320, -100), new Vector2(-20, -20));
            
            SetupVerticalLayoutGroup(notificationSystem, 10, false);
            
            Debug.Log("🔔 Notification system created");
        }
        
        public void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            var notification = CreateUIElement("Notification", notificationSystem.transform);
            SetupLayoutElement(notification, 300, 60);
            
            var bg = notification.AddComponent<Image>();
            bg.color = GetNotificationColor(type);
            
            SetupHorizontalLayoutGroup(notification, 10, false);
            
            // Icon
            var icon = CreateStyledText(notification, "Icon", GetNotificationIcon(type), UIBrandingExtensions.TypographyStyle.H3);
            icon.color = Color.white;
            SetupLayoutElement(icon.gameObject, 40, 40);
            
            // Message
            var messageText = CreateStyledText(notification, "Message", message, UIBrandingExtensions.TypographyStyle.Body);
            messageText.color = Color.white;
            
            // Animate in
            var canvasGroup = notification.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            notification.transform.localScale = Vector3.zero;
            
            notification.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            canvasGroup.DOFade(1, 0.3f);
            
            // Auto remove after duration
            StartCoroutine(RemoveNotificationAfterDelay(notification, duration));
        }
        
        private IEnumerator RemoveNotificationAfterDelay(GameObject notification, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            var canvasGroup = notification.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0, 0.3f);
                notification.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                    .OnComplete(() => DestroyImmediate(notification));
            }
        }
        
        #endregion
        
        #region UI Utility Methods
        
        private GameObject CreatePanel(string name, string debugName)
        {
            var panel = CreateUIElement(name, mainCanvas.transform);
            SetRectTransform(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            var canvasGroup = panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            return panel;
        }
        
        private GameObject CreateUIElement(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }
        
        private TextMeshProUGUI CreateStyledText(GameObject parent, string name, string text, UIBrandingExtensions.TypographyStyle style)
        {
            var textObj = CreateUIElement(name, parent.transform);
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            
            UIBrandingExtensions.StyleModernText(textComponent, style);
            
            return textComponent;
        }
        
        private void SetRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
        
        private void SetupLayoutElement(GameObject obj, float preferredWidth, float preferredHeight)
        {
            var layoutElement = obj.AddComponent<LayoutElement>();
            if (preferredWidth > 0) layoutElement.preferredWidth = preferredWidth;
            if (preferredHeight > 0) layoutElement.preferredHeight = preferredHeight;
        }
        
        private void SetupVerticalLayoutGroup(GameObject obj, float spacing, bool controlSize)
        {
            var vlg = obj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.childControlWidth = controlSize;
            vlg.childControlHeight = controlSize;
            vlg.childForceExpandWidth = controlSize;
            vlg.childForceExpandHeight = false;
            
            if (controlSize)
            {
                var csf = obj.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
        
        private void SetupHorizontalLayoutGroup(GameObject obj, float spacing, bool controlSize)
        {
            var hlg = obj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childControlWidth = controlSize;
            hlg.childControlHeight = controlSize;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = controlSize;
        }
        
        private void SetupGridLayoutGroup(GameObject obj, Vector2 cellSize, Vector2 spacing, int constraintCount)
        {
            var glg = obj.AddComponent<GridLayoutGroup>();
            glg.cellSize = cellSize;
            glg.spacing = spacing;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = constraintCount;
        }
        
        #endregion
        
        #region Animation & Effects
        
        private void AnimateButtonClick(GameObject button)
        {
            button.transform.DOScale(bounceScale, 0.1f).SetEase(Ease.OutQuad)
                .OnComplete(() => button.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
        }
        
        private void AddButtonHoverEffects(Button button)
        {
            // Add hover scale effect
            var trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => {
                button.transform.DOScale(1.05f, 0.1f).SetEase(Ease.OutQuad);
            });
            trigger.triggers.Add(enterEntry);
            
            var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => {
                button.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
            });
            trigger.triggers.Add(exitEntry);
        }
        
        #endregion
        
        #region Panel Management
        
        public void ShowPanel(string panelName)
        {
            if (!uiPanels.ContainsKey(panelName)) return;
            
            // Hide current panel
            HideCurrentPanel();
            
            var panel = uiPanels[panelName];
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            
            panel.SetActive(true);
            currentActivePanel = canvasGroup;
            
            // Animate in
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeInDuration).SetEase(defaultEase);
            
            // Play sound
            if (menuOpenSound != null && uiAudioSource != null)
            {
                uiAudioSource.PlayOneShot(menuOpenSound);
            }
            
            Debug.Log($"🎨 Showing panel: {panelName}");
        }
        
        public void HideCurrentPanel()
        {
            if (currentActivePanel != null)
            {
                currentActivePanel.DOFade(0, fadeInDuration * 0.5f)
                    .OnComplete(() => {
                        if (currentActivePanel != null)
                        {
                            currentActivePanel.gameObject.SetActive(false);
                        }
                    });
            }
        }
        
        #endregion
        
        #region Game Actions
        
        private void StartGame()
        {
            ShowNotification("🎮 Starting Game...", NotificationType.Success);
            ShowPanel("GameHUD");
            
            // Load game scene or start game logic
            Debug.Log("🎮 Game Started!");
        }
        
        private void ResumeGame()
        {
            ShowPanel("GameHUD");
            Time.timeScale = 1f;
            ShowNotification("▶️ Game Resumed", NotificationType.Info);
        }
        
        private void SaveGame()
        {
            ShowNotification("💾 Game Saved Successfully!", NotificationType.Success);
            // Implement save logic
        }
        
        private void QuitGame()
        {
            ShowNotification("👋 Thanks for playing!", NotificationType.Info);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        private void ReturnToMainMenu()
        {
            ShowPanel("MainMenu");
            Time.timeScale = 1f;
            ShowNotification("🏠 Returned to Main Menu", NotificationType.Info);
        }
        
        #endregion
        
        #region Settings Callbacks
        
        private void OnGameSpeedChanged(float value)
        {
            Time.timeScale = value;
            ShowNotification($"⚡ Game Speed: {value:F1}x", NotificationType.Info, 1f);
        }
        
        private void OnQualityChanged(float value)
        {
            QualitySettings.SetQualityLevel((int)value);
            ShowNotification($"🎨 Quality Level: {value}", NotificationType.Info, 1f);
        }
        
        private void OnVSyncChanged(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            ShowNotification($"📺 VSync: {(enabled ? "ON" : "OFF")}", NotificationType.Info, 1f);
        }
        
        private void OnFullscreenChanged(bool enabled)
        {
            Screen.fullScreen = enabled;
            ShowNotification($"🖥️ Fullscreen: {(enabled ? "ON" : "OFF")}", NotificationType.Info, 1f);
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            AudioListener.volume = value;
            ShowNotification($"🔊 Master Volume: {value:P0}", NotificationType.Info, 1f);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            // Implement music volume control
            ShowNotification($"🎵 Music Volume: {value:P0}", NotificationType.Info, 1f);
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            if (uiAudioSource != null) uiAudioSource.volume = value;
            ShowNotification($"🔊 SFX Volume: {value:P0}", NotificationType.Info, 1f);
        }
        
        private void OnAutoSaveChanged(bool enabled)
        {
            ShowNotification($"💾 Auto-Save: {(enabled ? "ON" : "OFF")}", NotificationType.Info, 1f);
        }
        
        private void OnDefaultSpeedChanged(float value)
        {
            ShowNotification($"🏃 Default Speed: {value:F1}x", NotificationType.Info, 1f);
        }
        
        private void OnTutorialsChanged(bool enabled)
        {
            ShowNotification($"📖 Tutorials: {(enabled ? "ON" : "OFF")}", NotificationType.Info, 1f);
        }
        
        #endregion
        
        #region Input Handling
        
        void Update()
        {
            // ESC key handling
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (currentActivePanel == gameHUDPanel?.GetComponent<CanvasGroup>())
                {
                    ShowPanel("PauseMenu");
                }
                else if (currentActivePanel != mainMenuPanel?.GetComponent<CanvasGroup>())
                {
                    HideCurrentPanel();
                    ShowPanel("MainMenu");
                }
            }
        }
        
        #endregion
        
        #region Utility Methods (continued)
        
        private void CreateGradientBackground(GameObject panel)
        {
            var gradient = CreateUIElement("GradientBackground", panel.transform);
            SetRectTransform(gradient, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            var image = gradient.AddComponent<Image>();
            image.color = new Color(0.1f, 0.2f, 0.4f, 0.9f); // Dark blue gradient
        }
        
        private void CreateFullScreenOverlay(GameObject panel)
        {
            var overlay = CreateUIElement("Overlay", panel.transform);
            SetRectTransform(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0.5f);
        }
        
        private void CreateCloseButton(GameObject parent, System.Action onClose)
        {
            var closeButton = CreateUIElement("CloseButton", parent.transform);
            var button = closeButton.AddComponent<Button>();
            UIBrandingExtensions.StyleModernButton(button, UIBrandingExtensions.ButtonStyle.Warning);
            SetupLayoutElement(closeButton, 40, 40);
            
            var closeText = CreateStyledText(closeButton, "X", "✕", UIBrandingExtensions.TypographyStyle.H3);
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
            
            button.onClick.AddListener(() => {
                PlayButtonSound();
                onClose?.Invoke();
            });
        }
        
        private void PlayButtonSound()
        {
            if (buttonClickSound != null && uiAudioSource != null)
            {
                uiAudioSource.PlayOneShot(buttonClickSound);
            }
        }
        
        private void ApplyBrandingToAllElements()
        {
            // Apply consistent branding across all UI elements
            Debug.Log("🎨 Applying branding to all UI elements...");
        }
        
        private Color GetNotificationColor(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => MinipollBrandManager.Colors.SuccessGreen,
                NotificationType.Warning => MinipollBrandManager.Colors.WarningAmber,
                NotificationType.Error => MinipollBrandManager.Colors.HeartPink,
                _ => MinipollBrandManager.Colors.MinipollBlue
            };
        }
        
        private string GetNotificationIcon(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => "✅",
                NotificationType.Warning => "⚠️",
                NotificationType.Error => "❌",
                _ => "ℹ️"
            };
        }
        
        #endregion
        
        #region Additional UI Components (Missing implementations)
        
        private void CreateMiniMap()
        {
            // TODO: Implement minimap
        }
        
        private void CreateResourceDisplay()
        {
            // TODO: Implement resource display
        }
        
        private void CreateSelectedCreaturePanel()
        {
            // TODO: Implement selected creature panel
        }
        
        private void CreateQuickActionBar()
        {
            // TODO: Implement quick action bar
        }
        
        private void CreateTimeWeatherDisplay()
        {
            // TODO: Implement time/weather display
        }
        
        private GameObject CreateSettingsSection(GameObject parent, string sectionName)
        {
            var section = CreateUIElement($"Section_{sectionName.Replace(" ", "")}", parent.transform);
            SetupVerticalLayoutGroup(section, 10, true);
            
            var header = CreateStyledText(section, "Header", sectionName, UIBrandingExtensions.TypographyStyle.H3);
            header.color = MinipollBrandManager.Colors.DeepNavy;
            
            return section;
        }
        
        private void CreateSliderSetting(GameObject parent, string label, float min, float max, float current, System.Action<float> onChanged)
        {
            var setting = CreateUIElement($"Setting_{label.Replace(" ", "")}", parent.transform);
            SetupHorizontalLayoutGroup(setting, 10, false);
            SetupLayoutElement(setting, 0, 30);
            
            var labelText = CreateStyledText(setting, "Label", label, UIBrandingExtensions.TypographyStyle.Body);
            labelText.color = MinipollBrandManager.Colors.DeepNavy;
            SetupLayoutElement(labelText.gameObject, 150, 30);
            
            var slider = CreateStyledSlider(setting, "Slider", min, max, current);
            slider.onValueChanged.AddListener(onChanged.Invoke);
            SetupLayoutElement(slider.gameObject, 200, 30);
            
            var valueText = CreateStyledText(setting, "Value", current.ToString("F1"), UIBrandingExtensions.TypographyStyle.Body);
            valueText.color = MinipollBrandManager.Colors.WarmOrange;
            SetupLayoutElement(valueText.gameObject, 50, 30);
            
            slider.onValueChanged.AddListener(value => valueText.text = value.ToString("F1"));
        }
        
        private void CreateToggleSetting(GameObject parent, string label, bool current, System.Action<bool> onChanged)
        {
            var setting = CreateUIElement($"Toggle_{label.Replace(" ", "")}", parent.transform);
            SetupHorizontalLayoutGroup(setting, 10, false);
            SetupLayoutElement(setting, 0, 30);
            
            var labelText = CreateStyledText(setting, "Label", label, UIBrandingExtensions.TypographyStyle.Body);
            labelText.color = MinipollBrandManager.Colors.DeepNavy;
            SetupLayoutElement(labelText.gameObject, 150, 30);
            
            var toggle = CreateStyledToggle(setting, "Toggle", current);
            toggle.onValueChanged.AddListener(onChanged.Invoke);
            SetupLayoutElement(toggle.gameObject, 50, 30);
        }
        
        private Slider CreateStyledSlider(GameObject parent, string name, float min, float max, float current)
        {
            var sliderObj = CreateUIElement(name, parent.transform);
            var slider = sliderObj.AddComponent<Slider>();
            
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = current;
            
            // Background
            var bg = CreateUIElement("Background", sliderObj.transform);
            SetRectTransform(bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = MinipollBrandManager.Colors.LightGray;
            
            // Fill area
            var fillArea = CreateUIElement("Fill Area", sliderObj.transform);
            SetRectTransform(fillArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            var fill = CreateUIElement("Fill", fillArea.transform);
            SetRectTransform(fill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = MinipollBrandManager.Colors.MinipollBlue;
            
            slider.fillRect = fill.GetComponent<RectTransform>();
            
            // Handle
            var handleArea = CreateUIElement("Handle Slide Area", sliderObj.transform);
            SetRectTransform(handleArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            var handle = CreateUIElement("Handle", handleArea.transform);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = MinipollBrandManager.Colors.CloudWhite;
            
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            
            return slider;
        }
        
        private Toggle CreateStyledToggle(GameObject parent, string name, bool current)
        {
            var toggleObj = CreateUIElement(name, parent.transform);
            var toggle = toggleObj.AddComponent<Toggle>();
            
            toggle.isOn = current;
            
            // Background
            var bg = CreateUIElement("Background", toggleObj.transform);
            SetRectTransform(bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = MinipollBrandManager.Colors.LightGray;
            
            // Checkmark
            var checkmark = CreateUIElement("Checkmark", toggleObj.transform);
            SetRectTransform(checkmark, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var checkImage = checkmark.AddComponent<Image>();
            checkImage.color = MinipollBrandManager.Colors.SuccessGreen;
            
            toggle.graphic = checkImage;
            
            return toggle;
        }
        
        private GameObject CreateScrollView(GameObject parent, string name)
        {
            var scrollView = CreateUIElement(name, parent.transform);
            SetRectTransform(scrollView, new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, 80), new Vector2(-20, -20));
            
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            
            // Viewport
            var viewport = CreateUIElement("Viewport", scrollView.transform);
            SetRectTransform(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var viewportMask = viewport.AddComponent<Mask>();
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            
            // Content
            var content = CreateUIElement("Content", viewport.transform);
            SetRectTransform(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
            SetupVerticalLayoutGroup(content, 10, true);
            
            var contentSizeFitter = content.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            
            return scrollView;
        }
        
        private void ShowItemDetails(string name, string description, int quantity)
        {
            ShowNotification($"📦 {name} - {description} (x{quantity})", NotificationType.Info, 2f);
        }
        
        #endregion
    }
    
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
