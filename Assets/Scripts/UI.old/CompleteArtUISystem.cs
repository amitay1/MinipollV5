using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.InputSystem;

/// <summary>
/// Complete Art UI System V2 - מערכת UI מתקדמת עם אנימציות ועיצוב מודרני
/// פיצ'רים: ניהול state מתקדם, אנימציות חלקות, עיצוב responsive, מערכת אירועים
/// </summary>
public class CompleteArtUISystem : MonoBehaviour
{
    /// <summary>
    /// מצבי UI האפשריים
    /// </summary>
    public enum UIState
    {
        None,
        Loading,
        MainMenu,
        GameHUD,
        Gameplay,
        Inventory,
        Settings,
        Paused,
        CreatureStats,
        WorldMap,
        Shop
    }

    /// <summary>
    /// אירועי UI
    /// </summary>
    public static event Action<UIState> OnUIStateChanged;
    public static event Action<string, object> OnUIEvent;

    [Header("🎨 Complete Art UI System V2")]
    [SerializeField] private bool createOnStart = true;
    
    [Header("Manual Testing")]
    [SerializeField] private bool enableTestButton = true;
    [SerializeField] private bool useArtAssets = true;
    [SerializeField] private bool createAnimations = true;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool enableResponsiveDesign = true;

    [Header("🎮 Game Configuration")]
    [SerializeField] private string gameTitle = "🎮 MINIPOLL V5";
    [SerializeField] private Color primaryColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color secondaryColor = new Color(0.1f, 0.8f, 0.4f);
    [SerializeField] private Color accentColor = new Color(1f, 0.7f, 0.2f);
    [SerializeField] private Color warningColor = new Color(1f, 0.4f, 0.2f);

    [Header("🌟 UI Animation Settings")]
    [SerializeField] private float transitionSpeed = 0.3f;
    [SerializeField] private float fadeSpeed = 0.25f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("🎨 Art Assets")]
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite panelSprite;
    [SerializeField] private Sprite coinIcon;
    [SerializeField] private Sprite settingsIcon;
    [SerializeField] private Sprite inventoryIcon;
    [SerializeField] private Sprite mapIcon;
    [SerializeField] private Sprite shopIcon;
    [SerializeField] private Sprite exitIcon;
    [SerializeField] private int coins = 10000;
    [SerializeField] private int level = 25;
    [SerializeField] private float health = 100f;
    [SerializeField] private float energy = 85f;
    [SerializeField] private float happiness = 90f;

    // State Management
    private UIState currentState = UIState.Loading;
    private UIState previousState = UIState.Loading;
    private Dictionary<UIState, GameObject> uiPanels = new Dictionary<UIState, GameObject>();
    
    // References
    private MinipollArtAssetManager assetManager;
    private Canvas mainCanvas;
    private CanvasScaler canvasScaler;
    private GameObject uiRoot;
    
    // UI Panels
    private GameObject loadingPanel;
    private GameObject gameHUD;
    private GameObject mainMenu;
    private GameObject gameplayUI;
    private GameObject settingsPanel;
    private GameObject inventoryPanel;
    private GameObject creatureStatsPanel;
    private GameObject worldMapPanel;
    private GameObject shopPanel;

    // UI Components - HUD
    private TextMeshProUGUI coinsText;
    private TextMeshProUGUI levelText;
    private Slider healthBar;
    private Slider energyBar;
    private Slider happinessBar;
    
    // UI Components - Interactive
    private Button menuButton;
    private Button inventoryButton;
    private Button settingsButton;
    private Button statsButton;
    private Button mapButton;
    private Button shopButton;
    
    // Animation and Effects
    private Coroutine currentTransition;
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    void Start()
    {
        // Debug logging removed to reduce console spam
        
        if (createOnStart)
        {
            // Debug logging removed to reduce console spam
            InitializeCompleteUI();
        }
        else
        {
            // Debug logging removed to reduce console spam
        }
    }
    
    /// <summary>
    /// פונקציה להפעלה ידנית של המערכת לצורכי בדיקה
    /// </summary>
    [ContextMenu("🚀 Initialize UI System Manually")]
    public void ForceInitializeUI()
    {
        Debug.Log("🚀 MANUAL INITIALIZATION - Force Initialize UI System called!");
        
        try
        {
            InitializeCompleteUI();
            Debug.Log("✅ MANUAL INITIALIZATION - UI System initialized successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ MANUAL INITIALIZATION - Failed to initialize UI System: {e.Message}");
        }
    }
    
    /// <summary>
    /// פונקציה לבדיקת מצב המערכת
    /// </summary>
    [ContextMenu("📊 Check UI System Status")]
    public void CheckUISystemStatus()
    {
        // Debug logging removed to reduce console spam - status check still functional
    }

    void Update()
    {
        // Multiple key combinations for UI creation
        if (InputHelper.GetKeyDown(KeyCode.Space) ||
            InputHelper.GetKeyDown(KeyCode.F2) ||
            InputHelper.GetKeyDown(KeyCode.F3))
        {
            // Debug logging removed to reduce console spam
            InitializeCompleteUI();
        }

        // F6 key for gameplay mode
        if (InputHelper.GetKeyDown(KeyCode.F6))
        {
            // Debug logging removed to reduce console spam
            TransitionToState(UIState.Gameplay);
        }

        // F7 key for menu mode
        if (InputHelper.GetKeyDown(KeyCode.F7))
        {
            // Debug logging removed to reduce console spam
            TransitionToState(UIState.MainMenu);
        }

        // F8 key for settings
        if (InputHelper.GetKeyDown(KeyCode.F8))
        {
            // Debug logging removed to reduce console spam
            TransitionToState(UIState.Settings);
        }

        // F9 key for inventory
        if (InputHelper.GetKeyDown(KeyCode.F9))
        {
            // Debug logging removed to reduce console spam
            TransitionToState(UIState.Inventory);
        }

        // F10 key for creature stats
        if (InputHelper.GetKeyDown(KeyCode.F10))
        {
            // Debug logging removed to reduce console spam
            TransitionToState(UIState.CreatureStats);
        }

        // F11 key for world map
        if (InputHelper.GetKeyDown(KeyCode.F11))
        {
            // Debug logging removed to reduce console spam
            TransitionToState(UIState.WorldMap);
        }

        // F12 key for shop
        if (InputHelper.GetKeyDown(KeyCode.F12))
        {
            // Debug logging removed to reduce console spam
            TransitionToState(UIState.Shop);
        }

        // ESC key to go back or close panels
        if (InputHelper.GetKeyDown(KeyCode.Escape))
        {
            // Debug logging removed to reduce console spam
            HandleEscapeKey();
        }
    }

    /// <summary>
    /// טיפול במקש ESC
    /// </summary>
    private void HandleEscapeKey()
    {
        switch (currentState)
        {
            case UIState.MainMenu:
                // במקום לצאת מהמשחק, עבור למצב GameHUD
                TransitionToState(UIState.GameHUD);
                break;
            case UIState.GameHUD:
                // אפשר לעבור למצב MainMenu
                TransitionToState(UIState.MainMenu);
                break;
            case UIState.Settings:
            case UIState.Inventory:
            case UIState.CreatureStats:
            case UIState.WorldMap:
            case UIState.Shop:
                // חזור למצב הקודם או לGameplay
                TransitionToState(previousState != UIState.None ? previousState : UIState.Gameplay);
                break;
            case UIState.Gameplay:
                // עבור לתפריט ראשי
                TransitionToState(UIState.MainMenu);
                break;
            default:
                TransitionToState(UIState.MainMenu);
                break;
        }
    }

    /// <summary>
    /// אתחול מערכת UI מלאה
    /// </summary>
    [ContextMenu("Initialize Complete UI")]
    public void InitializeCompleteUI()
    {
        Debug.Log("🎨 Initializing Complete Art UI System...");

        try
        {
            // אתחל מנהל נכסים
            InitializeAssetManager();

            // צור Canvas ראשי
            CreateMainCanvas();

            // צור UI Root
            CreateUIRoot();

            // צור HUD משחק
            CreateGameHUD();

            // צור תפריט ראשי
            CreateMainMenu();

            // צור UI משחק
            CreateGameplayUI();

            // צור פאנל הגדרות
            CreateSettingsPanel();

            // צור את כל הפאנלים החדשים
            CreateAllPanels();

            // התחל במצב משחק - ללא UI
            TransitionToState(UIState.Gameplay);

            Debug.Log("✅ Complete Art UI System initialized successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to initialize Complete Art UI System: {e.Message}");
        }
    }

    /// <summary>
    /// אתחול מנהל נכסים וטעינת נכסי אמנות
    /// </summary>
    private void InitializeAssetManager()
    {
        // חפש מנהל נכסים קיים
        assetManager = FindFirstObjectByType<MinipollArtAssetManager>();

        if (assetManager == null)
        {
            // צור מנהל נכסים חדש
            GameObject assetManagerObj = new GameObject("ArtAssetManager");
            assetManager = assetManagerObj.AddComponent<MinipollArtAssetManager>();
            assetManager.LoadAllAssets();

            Debug.Log("📦 Created new Asset Manager");
        }
        else
        {
            Debug.Log("📦 Using existing Asset Manager");
        }

        // טען נכסי UI מתיקיית Art
        LoadUIAssets();
    }

    /// <summary>
    /// טעינת נכסי UI מתיקיית Art
    /// </summary>
    private void LoadUIAssets()
    {
        try
        {
            // טען sprites מתיקיית SpriteCoinMaster
            buttonSprite = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/buttonEnabled");
            if (buttonSprite == null) buttonSprite = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/green button");
            
            panelSprite = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/Panel_white");
            if (panelSprite == null) panelSprite = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/main_bg_slice");
            
            coinIcon = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/coin");
            if (coinIcon == null) coinIcon = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/coins_1");
            
            settingsIcon = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/setting icon");
            inventoryIcon = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/box");
            mapIcon = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/map_icon");
            shopIcon = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/shop icon");
            exitIcon = Resources.Load<Sprite>("Art/Sprites/SpriteCoinMaster/exit_button");

            Debug.Log("🎨 UI Assets loaded successfully!");
            Debug.Log($"Button: {(buttonSprite != null ? "✅" : "❌")}, Panel: {(panelSprite != null ? "✅" : "❌")}, Coin: {(coinIcon != null ? "✅" : "❌")}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Failed to load some UI assets: {e.Message}");
        }
    }

    /// <summary>
    /// יצירת Canvas ראשי
    /// </summary>
    private void CreateMainCanvas()
    {
        GameObject canvasObj = new GameObject("CompleteArtUI_Canvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 1000;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // הוסף רקע
        CreateCanvasBackground();

        // Debug logging removed to reduce console spam
    }

    /// <summary>
    /// יצירת רקע Canvas
    /// </summary>
    private void CreateCanvasBackground()
    {
        GameObject bgObj = new GameObject("Canvas_Background");
        bgObj.transform.SetParent(mainCanvas.transform, false);

        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        var bgImage = bgObj.AddComponent<Image>();

        // נסה להשתמש ברקע אמיתי
        if (useArtAssets && assetManager != null)
        {
            var background = assetManager.GetBackground("background");
            if (background != null)
            {
                bgImage.sprite = background;
                bgImage.type = Image.Type.Simple;
                Debug.Log("✅ Using art background");
            }
            else
            {
                bgImage.color = new Color(0.1f, 0.15f, 0.25f, 1f);
                Debug.Log("🎨 Using gradient background");
            }
        }
        else
        {
            bgImage.color = new Color(0.1f, 0.15f, 0.25f, 1f);
        }
    }

    /// <summary>
    /// יצירת UI Root
    /// </summary>
    private void CreateUIRoot()
    {
        uiRoot = new GameObject("UI_Root");
        uiRoot.transform.SetParent(mainCanvas.transform, false);

        var rootRect = uiRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // Debug logging removed to reduce console spam
    }

    /// <summary>
    /// יצירת HUD משחק
    /// </summary>
    private void CreateGameHUD()
    {
        gameHUD = new GameObject("Game_HUD");
        gameHUD.transform.SetParent(uiRoot.transform, false);

        var hudRect = gameHUD.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 1);
        hudRect.anchorMax = new Vector2(1, 1);
        hudRect.anchoredPosition = Vector2.zero;
        hudRect.sizeDelta = new Vector2(0, 120);

        // רקע HUD
        var hudImage = gameHUD.AddComponent<Image>();
        if (useArtAssets && assetManager != null)
        {
            var panelSprite = assetManager.GetBackground("panel_back");
            if (panelSprite != null)
            {
                hudImage.sprite = panelSprite;
                hudImage.type = Image.Type.Sliced;
            }
            else
            {
                hudImage.color = new Color(0, 0, 0, 0.7f);
            }
        }
        else
        {
            hudImage.color = new Color(0, 0, 0, 0.7f);
        }

        // צור משאבי HUD
        CreateHUDResources();

        // צור כפתורי HUD
        CreateHUDButtons();

        // Debug logging removed to reduce console spam
    }

    /// <summary>
    /// יצירת משאבי HUD
    /// </summary>
    private void CreateHUDResources()
    {
        // מוני משאבים
        coinsText = CreateResourceDisplay("Coins", "coin", new Vector2(150, -60), coins);
        levelText = CreateResourceDisplay("Level", "star", new Vector2(350, -60), level);

        // פרוגרס בר אם יש
        CreateProgressBar();
    }

    /// <summary>
    /// יצירת תצוגת משאב
    /// </summary>
    private TextMeshProUGUI CreateResourceDisplay(string name, string iconKey, Vector2 position, int value)
    {
        GameObject resourceObj = new GameObject($"{name}_Display");
        resourceObj.transform.SetParent(gameHUD.transform, false);

        var resourceRect = resourceObj.AddComponent<RectTransform>();
        resourceRect.anchoredPosition = position;
        resourceRect.sizeDelta = new Vector2(140, 50);

        // רקע
        var resourceBg = resourceObj.AddComponent<Image>();
        if (useArtAssets && assetManager != null)
        {
            var panelSprite = assetManager.GetBackground("panel_white");
            if (panelSprite != null)
            {
                resourceBg.sprite = panelSprite;
                resourceBg.type = Image.Type.Sliced;
                resourceBg.color = new Color(1, 1, 1, 0.2f);
            }
            else
            {
                resourceBg.color = new Color(1, 1, 1, 0.1f);
            }
        }
        else
        {
            resourceBg.color = new Color(1, 1, 1, 0.1f);
        }

        // אייקון
        if (useArtAssets && assetManager != null)
        {
            var iconSprite = assetManager.GetIcon(iconKey);
            if (iconSprite != null)
            {
                GameObject iconObj = new GameObject($"{name}_Icon");
                iconObj.transform.SetParent(resourceObj.transform, false);

                var iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchoredPosition = new Vector2(-45, 0);
                iconRect.sizeDelta = new Vector2(35, 35);

                var iconImage = iconObj.AddComponent<Image>();
                iconImage.sprite = iconSprite;
            }
        }

        // טקסט
        GameObject textObj = new GameObject($"{name}_Text");
        textObj.transform.SetParent(resourceObj.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(20, 0);
        textRect.sizeDelta = new Vector2(90, 50);

        var textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = value.ToString("N0");
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;

        // הוסף צל
        var shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(1, -1);

        return textMesh;
    }

    /// <summary>
    /// יצירת פרוגרס בר
    /// </summary>
    private void CreateProgressBar()
    {
        GameObject progressObj = new GameObject("Progress_Bar");
        progressObj.transform.SetParent(gameHUD.transform, false);

        var progressRect = progressObj.AddComponent<RectTransform>();
        progressRect.anchoredPosition = new Vector2(0, -25);
        progressRect.sizeDelta = new Vector2(400, 20);

        // רקע פרוגרס
        var progressBg = progressObj.AddComponent<Image>();
        progressBg.color = new Color(0, 0, 0, 0.5f);

        // מילוי פרוגרס
        GameObject fillObj = new GameObject("Progress_Fill");
        fillObj.transform.SetParent(progressObj.transform, false);

        var fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        var fillImage = fillObj.AddComponent<Image>();
        fillImage.color = accentColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 0.7f; // 70% מלא

        // Debug logging removed to reduce console spam
    }

    /// <summary>
    /// יצירת כפתורי HUD
    /// </summary>
    private void CreateHUDButtons()
    {
        // כפתור הגדרות עם פונקציונליות אמיתית
        var settingsBtn = CreateButton(gameHUD, "SettingsButton", "⚙️", OpenSettings, settingsIcon);
        var settingsRect = settingsBtn.GetComponent<RectTransform>();
        settingsRect.anchoredPosition = new Vector2(-100, -60);
        settingsRect.sizeDelta = new Vector2(60, 60);

        // כפתור מלאי
        var inventoryBtn = CreateButton(gameHUD, "InventoryButton", "🎒", OpenInventory, inventoryIcon);
        var invRect = inventoryBtn.GetComponent<RectTransform>();
        invRect.anchoredPosition = new Vector2(-170, -60);
        invRect.sizeDelta = new Vector2(60, 60);

        // כפתור סטטיסטיקות
        var statsBtn = CreateButton(gameHUD, "StatsButton", "📊", OpenCreatureStats);
        var statsRect = statsBtn.GetComponent<RectTransform>();
        statsRect.anchoredPosition = new Vector2(-240, -60);
        statsRect.sizeDelta = new Vector2(60, 60);

        // כפתור מפה
        var mapBtn = CreateButton(gameHUD, "MapButton", "🗺️", OpenWorldMap, mapIcon);
        var mapRect = mapBtn.GetComponent<RectTransform>();
        mapRect.anchoredPosition = new Vector2(-310, -60);
        mapRect.sizeDelta = new Vector2(60, 60);

        // כפתור חנות
        var shopBtn = CreateButton(gameHUD, "ShopButton", "🛍️", OpenShop, shopIcon);
        var shopRect = shopBtn.GetComponent<RectTransform>();
        shopRect.anchoredPosition = new Vector2(-380, -60);
        shopRect.sizeDelta = new Vector2(60, 60);

        // Debug logging removed to reduce console spam
    }

    /// <summary>
    /// יצירת כפתור HUD
    /// </summary>
    private void CreateHUDButton(string name, string iconKey, Vector2 position, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"{name}_Button");
        buttonObj.transform.SetParent(gameHUD.transform, false);

        var buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(70, 70);

        var button = buttonObj.AddComponent<Button>();
        var buttonImage = buttonObj.AddComponent<Image>();

        // השתמש בכפתור אמיתי
        if (useArtAssets && assetManager != null)
        {
            var buttonSprite = assetManager.GetButton("buttonenabled");
            if (buttonSprite != null)
            {
                buttonImage.sprite = buttonSprite;
                buttonImage.type = Image.Type.Sliced;
            }
            else
            {
                buttonImage.color = primaryColor;
            }
        }
        else
        {
            buttonImage.color = primaryColor;
        }

        // הוסף אייקון
        if (useArtAssets && assetManager != null)
        {
            var iconSprite = assetManager.GetIcon(iconKey);
            if (iconSprite != null)
            {
                GameObject iconObj = new GameObject($"{name}_Icon");
                iconObj.transform.SetParent(buttonObj.transform, false);

                var iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(10, 10);
                iconRect.offsetMax = new Vector2(-10, -10);

                var iconImage = iconObj.AddComponent<Image>();
                iconImage.sprite = iconSprite;
            }
        }

        // אירוע לחיצה
        button.onClick.AddListener(() => onClick?.Invoke());

        // אפקטי hover
        AddButtonHoverEffect(button);
    }

    /// <summary>
    /// יצירת תפריט ראשי
    /// </summary>
    private void CreateMainMenu()
    {
        mainMenu = new GameObject("Main_Menu");
        mainMenu.transform.SetParent(uiRoot.transform, false);

        var menuRect = mainMenu.AddComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuRect.anchoredPosition = Vector2.zero;
        menuRect.sizeDelta = new Vector2(500, 700);

        // רקע תפריט
        var menuImage = mainMenu.AddComponent<Image>();
        if (useArtAssets && assetManager != null)
        {
            var panelSprite = assetManager.GetBackground("golden_panel_back");
            if (panelSprite != null)
            {
                menuImage.sprite = panelSprite;
                menuImage.type = Image.Type.Sliced;
            }
            else
            {
                menuImage.color = new Color(0.2f, 0.2f, 0.3f, 0.95f);
            }
        }
        else
        {
            menuImage.color = new Color(0.2f, 0.2f, 0.3f, 0.95f);
        }

        // כותרת
        CreateMenuTitle();

        // כפתורי תפריט
        CreateMenuButtons();

        Debug.Log("📱 Main menu created");
    }

    /// <summary>
    /// יצירת כותרת תפריט
    /// </summary>
    private void CreateMenuTitle()
    {
        GameObject titleObj = new GameObject("Menu_Title");
        titleObj.transform.SetParent(mainMenu.transform, false);

        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 250);
        titleRect.sizeDelta = new Vector2(450, 100);

        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = gameTitle;
        titleText.fontSize = 42;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;

        // הוסף צל
        var shadow = titleObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(3, -3);

        // הוסף אפקט זוהר
        if (useArtAssets && assetManager != null)
        {
            var glowSprite = assetManager.GetEffect("glow");
            if (glowSprite != null)
            {
                GameObject glowObj = new GameObject("Title_Glow");
                glowObj.transform.SetParent(titleObj.transform, false);

                var glowRect = glowObj.AddComponent<RectTransform>();
                glowRect.anchorMin = Vector2.zero;
                glowRect.anchorMax = Vector2.one;
                glowRect.offsetMin = new Vector2(-30, -30);
                glowRect.offsetMax = new Vector2(30, 30);

                var glowImage = glowObj.AddComponent<Image>();
                glowImage.sprite = glowSprite;
                glowImage.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.6f);

                // הוסף סיבוב
                if (createAnimations)
                {
                    var rotator = glowObj.AddComponent<UIElementRotator>();
                    // Rotation speed will use default value (30f)
                }
            }
        }
    }

    /// <summary>
    /// יצירת כפתורי תפריט
    /// </summary>
    private void CreateMenuButtons()
    {
        Vector2[] buttonPositions = {
            new Vector2(0, 120),
            new Vector2(0, 40),
            new Vector2(0, -40),
            new Vector2(0, -120),
            new Vector2(0, -200)
        };

        string[] buttonTexts = {
            "🎮 PLAY GAME",
            "🏆 ACHIEVEMENTS",
            "⚙️ SETTINGS",
            "📊 STATISTICS",
            "❌ EXIT"
        };

        System.Action[] buttonActions = {
            () => StartGame(),
            () => OpenAchievements(),
            () => ToggleSettings(),
            () => ShowStatistics(),
            () => ExitGame()
        };

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            CreateMenuButton(buttonTexts[i], buttonPositions[i], buttonActions[i]);
        }
    }

    /// <summary>
    /// יצירת כפתור תפריט
    /// </summary>
    private void CreateMenuButton(string text, Vector2 position, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"MenuButton_{text.Replace(" ", "")}");
        buttonObj.transform.SetParent(mainMenu.transform, false);

        var buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(400, 70);

        var button = buttonObj.AddComponent<Button>();
        var buttonImage = buttonObj.AddComponent<Image>();

        // השתמש בכפתור אמיתי
        if (useArtAssets && assetManager != null)
        {
            var buttonSprite = assetManager.GetButton("blue_button");
            if (buttonSprite != null)
            {
                buttonImage.sprite = buttonSprite;
                buttonImage.type = Image.Type.Sliced;
            }
            else
            {
                buttonImage.color = primaryColor;
            }
        }
        else
        {
            buttonImage.color = primaryColor;
        }

        // טקסט
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = 22;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;

        // אירוע לחיצה
        button.onClick.AddListener(() => onClick?.Invoke());

        // אפקטי hover
        AddButtonHoverEffect(button);
    }

    /// <summary>
    /// יצירת UI משחק
    /// </summary>
    private void CreateGameplayUI()
    {
        gameplayUI = new GameObject("Gameplay_UI");
        gameplayUI.transform.SetParent(uiRoot.transform, false);

        var gameplayRect = gameplayUI.AddComponent<RectTransform>();
        gameplayRect.anchorMin = Vector2.zero;
        gameplayRect.anchorMax = Vector2.one;
        gameplayRect.offsetMin = Vector2.zero;
        gameplayRect.offsetMax = Vector2.zero;

        // צור כפתורי משחק
        CreateGameplayButtons();

        Debug.Log("🎮 Gameplay UI created");
    }

    /// <summary>
    /// יצירת כפתורי משחק
    /// </summary>
    private void CreateGameplayButtons()
    {
        // כפתור הגדרות בלבד - קטן ובפינה
        CreateMinimalSettingsButton();
    }
    
    /// <summary>
    /// יצירת כפתור הגדרות מינימלי למשחק
    /// </summary>
    private void CreateMinimalSettingsButton()
    {
        var settingsBtn = CreateButton(gameplayUI, "MinimalSettingsButton", "⚙️", () => {
            Debug.Log("🎮 Opening Settings from Gameplay");
            OpenSettings();
        }, settingsIcon);
        
        // מיקום בפינה העליונה ימנית
        var buttonRect = settingsBtn.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.anchoredPosition = new Vector2(-60, -60);
        buttonRect.sizeDelta = new Vector2(50, 50);
        
        Debug.Log("⚙️ Minimal settings button created for gameplay");
    }

    /// <summary>
    /// יצירת כפתור משחק
    /// </summary>
    private void CreateGameplayButton(string text, Vector2 position, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"GameplayButton_{text.Replace(" ", "")}");
        buttonObj.transform.SetParent(gameplayUI.transform, false);

        var buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(150, 50);

        var button = buttonObj.AddComponent<Button>();
        var buttonImage = buttonObj.AddComponent<Image>();

        // השתמש בכפתור אמיתי
        if (useArtAssets && assetManager != null)
        {
            var buttonSprite = assetManager.GetButton("green_button");
            if (buttonSprite != null)
            {
                buttonImage.sprite = buttonSprite;
                buttonImage.type = Image.Type.Sliced;
            }
            else
            {
                buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 1f);
            }
        }
        else
        {
            buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 1f);
        }

        // טקסט
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = 16;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;

        // אירוע לחיצה
        button.onClick.AddListener(() => onClick?.Invoke());

        // אפקטי hover
        AddButtonHoverEffect(button);
    }

    /// <summary>
    /// יצירת פאנל הגדרות
    /// </summary>
    private void CreateSettingsPanel()
    {
        settingsPanel = new GameObject("Settings_Panel");
        settingsPanel.transform.SetParent(uiRoot.transform, false);

        var settingsRect = settingsPanel.AddComponent<RectTransform>();
        settingsRect.anchorMin = new Vector2(0.5f, 0.5f);
        settingsRect.anchorMax = new Vector2(0.5f, 0.5f);
        settingsRect.anchoredPosition = Vector2.zero;
        settingsRect.sizeDelta = new Vector2(600, 500);

        // רקע
        var settingsImage = settingsPanel.AddComponent<Image>();
        if (useArtAssets && assetManager != null)
        {
            var panelSprite = assetManager.GetBackground("panel_white");
            if (panelSprite != null)
            {
                settingsImage.sprite = panelSprite;
                settingsImage.type = Image.Type.Sliced;
                settingsImage.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            }
            else
            {
                settingsImage.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            }
        }
        else
        {
            settingsImage.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
        }

        // כותרת הגדרות
        CreateSettingsTitle();

        // אפשרויות הגדרות
        CreateSettingsOptions();

        // כפתור סגירה
        CreateSettingsCloseButton();

        // התחל כסמוי
        settingsPanel.SetActive(false);

        Debug.Log("⚙️ Settings panel created");
    }

    /// <summary>
    /// יצירת כותרת הגדרות
    /// </summary>
    private void CreateSettingsTitle()
    {
        GameObject titleObj = new GameObject("Settings_Title");
        titleObj.transform.SetParent(settingsPanel.transform, false);

        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 200);
        titleRect.sizeDelta = new Vector2(500, 60);

        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "⚙️ SETTINGS";
        titleText.fontSize = 32;
        titleText.color = Color.black;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
    }

    /// <summary>
    /// יצירת אפשרויות הגדרות
    /// </summary>
    private void CreateSettingsOptions()
    {
        // סליידר קול
        CreateSettingsSlider("🔊 Sound Volume", new Vector2(0, 100), 0.8f);

        // סליידר מוזיקה
        CreateSettingsSlider("🎵 Music Volume", new Vector2(0, 40), 0.6f);

        // טוגל אפקטים
        CreateSettingsToggle("✨ Visual Effects", new Vector2(0, -20), true);

        // טוגל רטט
        CreateSettingsToggle("📳 Vibration", new Vector2(0, -80), true);
    }

    /// <summary>
    /// יצירת סליידר הגדרות
    /// </summary>
    private void CreateSettingsSlider(string label, Vector2 position, float defaultValue)
    {
        GameObject sliderObj = new GameObject($"Settings_{label.Replace(" ", "")}");
        sliderObj.transform.SetParent(settingsPanel.transform, false);

        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchoredPosition = position;
        sliderRect.sizeDelta = new Vector2(400, 40);

        // תווית
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sliderObj.transform, false);

        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(-150, 0);
        labelRect.sizeDelta = new Vector2(200, 40);

        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 16;
        labelText.color = Color.black;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;

        // סליידר
        GameObject sliderControl = new GameObject("Slider");
        sliderControl.transform.SetParent(sliderObj.transform, false);

        var sliderControlRect = sliderControl.AddComponent<RectTransform>();
        sliderControlRect.anchoredPosition = new Vector2(100, 0);
        sliderControlRect.sizeDelta = new Vector2(200, 30);

        var slider = sliderControl.AddComponent<Slider>();
        slider.value = defaultValue;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        // רקע סליידר
        var sliderImage = sliderControl.AddComponent<Image>();
        sliderImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        // יצירת handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(sliderControl.transform, false);

        var handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 30);

        var handleImage = handleObj.AddComponent<Image>();
        handleImage.color = primaryColor;

        slider.targetGraphic = handleImage;
        slider.handleRect = handleRect;
    }

    /// <summary>
    /// יצירת טוגל הגדרות
    /// </summary>
    private void CreateSettingsToggle(string label, Vector2 position, bool defaultValue)
    {
        GameObject toggleObj = new GameObject($"Settings_{label.Replace(" ", "")}");
        toggleObj.transform.SetParent(settingsPanel.transform, false);

        var toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.anchoredPosition = position;
        toggleRect.sizeDelta = new Vector2(400, 40);

        // תווית
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);

        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(-150, 0);
        labelRect.sizeDelta = new Vector2(200, 40);

        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 16;
        labelText.color = Color.black;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;

        // טוגל
        GameObject toggleControl = new GameObject("Toggle");
        toggleControl.transform.SetParent(toggleObj.transform, false);

        var toggleControlRect = toggleControl.AddComponent<RectTransform>();
        toggleControlRect.anchoredPosition = new Vector2(100, 0);
        toggleControlRect.sizeDelta = new Vector2(50, 30);

        var toggle = toggleControl.AddComponent<Toggle>();
        toggle.isOn = defaultValue;

        // רקע טוגל
        var toggleImage = toggleControl.AddComponent<Image>();
        toggleImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        // יצירת checkmark
        GameObject checkmarkObj = new GameObject("Checkmark");
        checkmarkObj.transform.SetParent(toggleControl.transform, false);

        var checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
        checkmarkRect.anchorMin = Vector2.zero;
        checkmarkRect.anchorMax = Vector2.one;
        checkmarkRect.offsetMin = Vector2.zero;
        checkmarkRect.offsetMax = Vector2.zero;

        var checkmarkImage = checkmarkObj.AddComponent<Image>();
        checkmarkImage.color = primaryColor;

        toggle.targetGraphic = toggleImage;
        toggle.graphic = checkmarkImage;
    }

    /// <summary>
    /// יצירת כפתור סגירת הגדרות
    /// </summary>
    private void CreateSettingsCloseButton()
    {
        // כפתור סגירה ראשי - חזרה למשחק
        var closeButton = CreateButton(settingsPanel, "CloseButton", "🔙 Back to Game", CloseSettings, exitIcon);
        var closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchoredPosition = new Vector2(-150, -200);
        closeRect.sizeDelta = new Vector2(200, 50);

        // כפתור חזרה לתפריט ראשי
        var mainMenuButton = CreateButton(settingsPanel, "MainMenuButton", "📱 Main Menu", QuitToMainMenu);
        var menuRect = mainMenuButton.GetComponent<RectTransform>();
        menuRect.anchoredPosition = new Vector2(150, -200);
        menuRect.sizeDelta = new Vector2(200, 50);
        
        // כפתורי נווט נוספים
        var inventoryButton = CreateButton(settingsPanel, "InventoryButton", "🎒 Inventory", OpenInventory, inventoryIcon);
        var invRect = inventoryButton.GetComponent<RectTransform>();
        invRect.anchoredPosition = new Vector2(-150, -140);
        invRect.sizeDelta = new Vector2(120, 40);
        
        var statsButton = CreateButton(settingsPanel, "StatsButton", "📊 Stats", OpenCreatureStats);
        var statsRect = statsButton.GetComponent<RectTransform>();
        statsRect.anchoredPosition = new Vector2(0, -140);
        statsRect.sizeDelta = new Vector2(120, 40);
        
        var mapButton = CreateButton(settingsPanel, "MapButton", "🗺️ Map", OpenWorldMap, mapIcon);
        var mapRect = mapButton.GetComponent<RectTransform>();
        mapRect.anchoredPosition = new Vector2(150, -140);
        mapRect.sizeDelta = new Vector2(120, 40);
    }

    /// <summary>
    /// הוספת אפקט hover לכפתור
    /// </summary>
    private void AddButtonHoverEffect(Button button)
    {
        var colors = button.colors;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;
    }

    // Mode switching methods
    public void SwitchToMenuMode()
    {
        mainMenu.SetActive(true);
        gameplayUI.SetActive(false);
        gameHUD.SetActive(false);

        // Debug logging removed to reduce console spam
    }

    public void SwitchToGameplayMode()
    {
        mainMenu.SetActive(false);
        gameplayUI.SetActive(true);
        gameHUD.SetActive(true);

        // Debug logging removed to reduce console spam
    }

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        // Debug logging removed to reduce console spam
    }

    // Event handlers
    private void StartGame()
    {
        Debug.Log("🎮 Starting game...");
        SwitchToGameplayMode();
    }

    private void OpenAchievements()
    {
        Debug.Log("🏆 Opening achievements...");
    }

    private void ShowStatistics()
    {
        Debug.Log("📊 Showing statistics...");
    }

    private void ExitGame()
    {
        Debug.Log("❌ Exiting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void ShowHelp()
    {
        Debug.Log("❓ Showing help");
    }

    private void ShowInfo()
    {
        Debug.Log("ℹ️ Showing info");
    }

    // Resource update methods
    public void UpdateCoins(int newCoins)
    {
        coins = newCoins;
        if (coinsText != null)
        {
            coinsText.text = coins.ToString("N0");
        }
    }

    public void UpdateLevel(int newLevel)
    {
        level = newLevel;
        if (levelText != null)
        {
            levelText.text = level.ToString("N0");
        }
    }

    #region State Management V2

    /// <summary>
    /// מעבר למצב UI חדש
    /// </summary>
    public void TransitionToState(UIState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;

        Debug.Log($"🔄 Transitioning from {previousState} to {currentState}");

        // הסתר את הפאנל הקודם
        HideCurrentPanel();

        // הצג את הפאנל החדש
        ShowPanelForState(newState);

        // הפעל אירוע
        OnUIStateChanged?.Invoke(currentState);
    }

    /// <summary>
    /// יצירת כל הפאנלים
    /// </summary>
    private void CreateAllPanels()
    {
        CreateGameHUD();
        CreateMainMenu();
        CreateGameplayUI();
        CreateSettingsPanel();
        CreateInventoryPanel();
        CreateCreatureStatsPanel();
        CreateWorldMapPanel();
        CreateShopPanel();
    }

    /// <summary>
    /// הסתרת הפאנל הנוכחי
    /// </summary>
    private void HideCurrentPanel()
    {
        foreach (var panel in uiPanels.Values)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    /// <summary>
    /// הצגת פאנל לפי מצב
    /// </summary>
    private void ShowPanelForState(UIState state)
    {
        if (uiPanels.ContainsKey(state) && uiPanels[state] != null)
        {
            uiPanels[state].SetActive(true);
        }
    }

    #endregion

    #region Functional UI Actions

    /// <summary>
    /// פונקציות כפתורים פונקציונליות
    /// </summary>
    
    public void OpenSettings()
    {
        Debug.Log("⚙️ Opening Settings Panel");
        TransitionToState(UIState.Settings);
    }
    
    public void CloseSettings()
    {
        Debug.Log("🔙 Closing Settings Panel");
        TransitionToState(UIState.Gameplay);
    }
    
    public void OpenInventory()
    {
        Debug.Log("🎒 Opening Inventory Panel");
        TransitionToState(UIState.Inventory);
    }
    
    public void CloseInventory()
    {
        Debug.Log("🔙 Closing Inventory Panel");
        TransitionToState(UIState.Gameplay);
    }
    
    public void OpenCreatureStats()
    {
        Debug.Log("📊 Opening Creature Stats Panel");
        TransitionToState(UIState.CreatureStats);
    }
    
    public void CloseCreatureStats()
    {
        Debug.Log("🔙 Closing Creature Stats Panel");
        TransitionToState(UIState.Gameplay);
    }
    
    public void OpenWorldMap()
    {
        Debug.Log("🗺️ Opening World Map Panel");
        TransitionToState(UIState.WorldMap);
    }
    
    public void CloseWorldMap()
    {
        Debug.Log("🔙 Closing World Map Panel");
        TransitionToState(UIState.Gameplay);
    }
    
    public void OpenShop()
    {
        Debug.Log("🛍️ Opening Shop Panel");
        TransitionToState(UIState.Shop);
    }
    
    public void CloseShop()
    {
        Debug.Log("🔙 Closing Shop Panel");
        TransitionToState(UIState.Gameplay);
    }
    
    public void ReturnToGame()
    {
        Debug.Log("🎮 Returning to Game");
        TransitionToState(UIState.Gameplay);
    }
    
    public void PauseGame()
    {
        Debug.Log("⏸️ Pausing Game");
        Time.timeScale = 0f;
        TransitionToState(UIState.Paused);
    }
    
    public void ResumeGame()
    {
        Debug.Log("▶️ Resuming Game");
        Time.timeScale = 1f;
        TransitionToState(UIState.Gameplay);
    }
    
    public void QuitToMainMenu()
    {
        Debug.Log("📱 Returning to Main Menu");
        Time.timeScale = 1f;
        TransitionToState(UIState.MainMenu);
    }

    #endregion

    #region New Panel Creation

    /// <summary>
    /// יצירת פאנל מלאי
    /// </summary>
    private void CreateInventoryPanel()
    {
        inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(uiRoot.transform, false);
        
        var rectTransform = inventoryPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var image = inventoryPanel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.7f);

        // הוסף תוכן לפאנל
        CreateInventoryContent(inventoryPanel);
        
        uiPanels[UIState.Inventory] = inventoryPanel;
        inventoryPanel.SetActive(false);
    }

    /// <summary>
    /// יצירת פאנל סטטיסטיקות יצור
    /// </summary>
    private void CreateCreatureStatsPanel()
    {
        creatureStatsPanel = new GameObject("CreatureStatsPanel");
        creatureStatsPanel.transform.SetParent(uiRoot.transform, false);
        
        var rectTransform = creatureStatsPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var image = creatureStatsPanel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);

        // הוסף תוכן לפאנל
        CreateCreatureStatsContent(creatureStatsPanel);
        
        uiPanels[UIState.CreatureStats] = creatureStatsPanel;
        creatureStatsPanel.SetActive(false);
    }

    /// <summary>
    /// יצירת פאנל מפת העולם
    /// </summary>
    private void CreateWorldMapPanel()
    {
        worldMapPanel = new GameObject("WorldMapPanel");
        worldMapPanel.transform.SetParent(uiRoot.transform, false);
        
        var rectTransform = worldMapPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var image = worldMapPanel.AddComponent<Image>();
        image.color = new Color(0.05f, 0.3f, 0.2f, 0.95f);

        // הוסף תוכן לפאנל
        CreateWorldMapContent(worldMapPanel);
        
        uiPanels[UIState.WorldMap] = worldMapPanel;
        worldMapPanel.SetActive(false);
    }

    /// <summary>
    /// יצירת פאנל חנות
    /// </summary>
    private void CreateShopPanel()
    {
        shopPanel = new GameObject("ShopPanel");
        shopPanel.transform.SetParent(uiRoot.transform, false);
        
        var rectTransform = shopPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var image = shopPanel.AddComponent<Image>();
        image.color = new Color(0.3f, 0.1f, 0.4f, 0.9f);

        // הוסף תוכן לפאנל
        CreateShopContent(shopPanel);
        
        uiPanels[UIState.Shop] = shopPanel;
        shopPanel.SetActive(false);
    }

    #endregion

    #region Panel Content Creation

    private void CreateInventoryContent(GameObject parent)
    {
        // כותרת
        var title = CreateText(parent, "InventoryTitle", "🎒 INVENTORY", 32);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        title.color = accentColor;

        // כפתור חזרה
        var backBtn = CreateButton(parent, "BackButton", "🔙 Back to Game", CloseInventory, exitIcon);
        var backRect = backBtn.GetComponent<RectTransform>();
        backRect.sizeDelta = new Vector2(200, 60);
        backRect.anchorMin = new Vector2(0.1f, 0.1f);
        backRect.anchorMax = new Vector2(0.1f, 0.1f);
        backRect.anchoredPosition = Vector2.zero;

        // רשימת פריטים (דמה)
        string[] items = {"🗡️ Iron Sword", "🛡️ Shield", "🏹 Bow", "💎 Diamond", "🧪 Health Potion", "🔥 Fire Scroll"};
        for (int i = 0; i < items.Length; i++)
        {
            var item = CreateButton(parent, $"Item{i}", items[i], () => Debug.Log($"Used item: {items[i]}"));
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(200, 60);
            itemRect.anchorMin = new Vector2(0.2f + (i % 3) * 0.3f, 0.7f - (i / 3) * 0.2f);
            itemRect.anchorMax = new Vector2(0.2f + (i % 3) * 0.3f, 0.7f - (i / 3) * 0.2f);
            itemRect.anchoredPosition = Vector2.zero;
        }
    }

    private void CreateCreatureStatsContent(GameObject parent)
    {
        // כותרת
        var title = CreateText(parent, "StatsTitle", "📊 CREATURE STATS", 32);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        title.color = primaryColor;

        // כפתור חזרה
        var backBtn = CreateButton(parent, "BackButton", "🔙 Back to Game", CloseCreatureStats, exitIcon);
        var backRect = backBtn.GetComponent<RectTransform>();
        backRect.sizeDelta = new Vector2(200, 60);
        backRect.anchorMin = new Vector2(0.1f, 0.1f);
        backRect.anchorMax = new Vector2(0.1f, 0.1f);
        backRect.anchoredPosition = Vector2.zero;

        // בריאות בר
        var healthLabel = CreateText(parent, "HealthLabel", "❤️ Health", 20);
        var healthLabelRect = healthLabel.GetComponent<RectTransform>();
        healthLabelRect.anchorMin = new Vector2(0.1f, 0.7f);
        healthLabelRect.anchorMax = new Vector2(0.1f, 0.7f);
        healthLabelRect.anchoredPosition = Vector2.zero;

        healthBar = CreateProgressBar(parent, "HealthBar", health / 100f, Color.red);
        var healthBarRect = healthBar.GetComponent<RectTransform>();
        healthBarRect.sizeDelta = new Vector2(300, 30);
        healthBarRect.anchorMin = new Vector2(0.3f, 0.7f);
        healthBarRect.anchorMax = new Vector2(0.3f, 0.7f);
        healthBarRect.anchoredPosition = Vector2.zero;

        // אנרגיה בר
        var energyLabel = CreateText(parent, "EnergyLabel", "⚡ Energy", 20);
        var energyLabelRect = energyLabel.GetComponent<RectTransform>();
        energyLabelRect.anchorMin = new Vector2(0.1f, 0.6f);
        energyLabelRect.anchorMax = new Vector2(0.1f, 0.6f);
        energyLabelRect.anchoredPosition = Vector2.zero;

        energyBar = CreateProgressBar(parent, "EnergyBar", energy / 100f, Color.yellow);
        var energyBarRect = energyBar.GetComponent<RectTransform>();
        energyBarRect.sizeDelta = new Vector2(300, 30);
        energyBarRect.anchorMin = new Vector2(0.3f, 0.6f);
        energyBarRect.anchorMax = new Vector2(0.3f, 0.6f);
        energyBarRect.anchoredPosition = Vector2.zero;

        // אושר בר
        var happinessLabel = CreateText(parent, "HappinessLabel", "😊 Happiness", 20);
        var happinessLabelRect = happinessLabel.GetComponent<RectTransform>();
        happinessLabelRect.anchorMin = new Vector2(0.1f, 0.5f);
        happinessLabelRect.anchorMax = new Vector2(0.1f, 0.5f);
        happinessLabelRect.anchoredPosition = Vector2.zero;

        happinessBar = CreateProgressBar(parent, "HappinessBar", happiness / 100f, Color.green);
        var happinessBarRect = happinessBar.GetComponent<RectTransform>();
        happinessBarRect.sizeDelta = new Vector2(300, 30);
        happinessBarRect.anchorMin = new Vector2(0.3f, 0.5f);
        happinessBarRect.anchorMax = new Vector2(0.3f, 0.5f);
        happinessBarRect.anchoredPosition = Vector2.zero;
    }

    private void CreateWorldMapContent(GameObject parent)
    {
        // כותרת
        var title = CreateText(parent, "MapTitle", "🗺️ WORLD MAP", 32);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        title.color = secondaryColor;

        // אזורים (דמה)
        string[] areas = {"🏠 Home", "🌲 Forest", "🏔️ Mountains", "🏖️ Beach", "🏜️ Desert", "❄️ Tundra"};
        for (int i = 0; i < areas.Length; i++)
        {
            var areaBtn = CreateButton(parent, $"Area{i}", areas[i], () => Debug.Log($"Clicked {areas[i]}"));
            var areaRect = areaBtn.GetComponent<RectTransform>();
            areaRect.sizeDelta = new Vector2(150, 50);
            areaRect.anchorMin = new Vector2(0.2f + (i % 3) * 0.3f, 0.6f - (i / 3) * 0.2f);
            areaRect.anchorMax = new Vector2(0.2f + (i % 3) * 0.3f, 0.6f - (i / 3) * 0.2f);
            areaRect.anchoredPosition = Vector2.zero;
        }
    }

    private void CreateShopContent(GameObject parent)
    {
        // כותרת
        var title = CreateText(parent, "ShopTitle", "🛍️ SHOP", 32);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        title.color = warningColor;

        // פריטים למכירה (דמה)
        string[] items = {"🍎 Food - 100", "🛡️ Shield - 500", "⚡ Energy - 250", "💎 Gem - 1000"};
        for (int i = 0; i < items.Length; i++)
        {
            var itemBtn = CreateButton(parent, $"ShopItem{i}", items[i], () => Debug.Log($"Purchased {items[i]}"));
            var itemRect = itemBtn.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(250, 60);
            itemRect.anchorMin = new Vector2(0.5f, 0.7f - i * 0.15f);
            itemRect.anchorMax = new Vector2(0.5f, 0.7f - i * 0.15f);
            itemRect.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// יצירת בר התקדמות
    /// </summary>
    private Slider CreateProgressBar(GameObject parent, string name, float value, Color color)
    {
        var sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent.transform, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        
        var slider = sliderObj.AddComponent<Slider>();
        slider.value = value;
        
        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        var bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Fill
        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        var fillImage = fill.AddComponent<Image>();
        fillImage.color = color;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        slider.fillRect = fillRect;
        
        return slider;
    }

    /// <summary>
    /// יצירת טקסט UI
    /// </summary>
    private TextMeshProUGUI CreateText(GameObject parent, string name, string text, int fontSize)
    {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        textObj.AddComponent<RectTransform>();
        
        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontStyle = FontStyles.Bold;
        
        return textComponent;
    }

    /// <summary>
    /// יצירת כפתור UI מתקדם עם נכסי אמנות
    /// </summary>
    private Button CreateButton(GameObject parent, string name, string text, System.Action onClick, Sprite icon = null, Sprite buttonBg = null)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);
        buttonObj.AddComponent<RectTransform>();
        
        var button = buttonObj.AddComponent<Button>();
        var image = buttonObj.AddComponent<Image>();
        
        // השתמש בנכס אמנות אם זמין
        if (buttonBg != null)
            image.sprite = buttonBg;
        else if (buttonSprite != null)
            image.sprite = buttonSprite;
        else
            image.color = primaryColor;
        
        // הגדר כפתור כ-9-slice אם יש sprite
        if (image.sprite != null)
        {
            image.type = Image.Type.Sliced;
        }
        
        // יצירת טקסט הכפתור
        var buttonText = CreateText(buttonObj, "Text", text, 18);
        var textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyles.Bold;
        
        // הוסף אייקון לכפתור אם זמין
        if (icon != null)
        {
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buttonObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            var iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
            
            // מקם את האייקון בצד שמאל
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.sizeDelta = new Vector2(32, 32);
            iconRect.anchoredPosition = new Vector2(40, 0);
            
            // הזז את הטקסט ימינה
            textRect.offsetMin = new Vector2(80, 5);
        }
        
        // הוסף אפקטי צליל וחזותיים
        AddButtonEffects(button);
        
        // הוספת פעולה לכפתור
        if (onClick != null)
            button.onClick.AddListener(() => {
                Debug.Log($"🔘 Button clicked: {name}");
                onClick();
            });
        
        return button;
    }

    /// <summary>
    /// הוספת אפקטים לכפתור
    /// </summary>
    private void AddButtonEffects(Button button)
    {
        // הוסף ColorBlock מותאם אישית
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.selectedColor = accentColor;
        colors.colorMultiplier = 1.2f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        
        // הוסף Navigation
        var nav = button.navigation;
        nav.mode = Navigation.Mode.Automatic;
        button.navigation = nav;
    }

    #endregion

    void OnGUI()
    {
        GUI.Label(new Rect(10, 400, 600, 400),
            "🎨 Complete Art UI System - Advanced State Management\n" +
            $"Asset Manager: {(assetManager != null ? "✅ Active" : "❌ Inactive")}\n" +
            $"Art Assets: {(useArtAssets ? "✅ Enabled" : "❌ Disabled")}\n" +
            $"Animations: {(createAnimations ? "✅ Enabled" : "❌ Disabled")}\n" +
            $"Current State: 🔄 {currentState}\n" +
            $"Previous State: ⏮️ {previousState}\n\n" +
            "🎮 State Controls:\n" +
            "SPACE/F2/F3 - Create UI\n" +
            "F6 - GameHUD Mode ⚡\n" +
            "F7 - MainMenu Mode 📱\n" +
            "F8 - Settings ⚙️\n" +
            "F9 - Inventory 🎒\n" +
            "F10 - Creature Stats 📊\n" +
            "F11 - World Map 🗺️\n" +
            "F12 - Shop �️\n" +
            "ESC - Go Back 🔙\n\n" +
            "�📊 Resources:\n" +
            $"💰 Coins: {coins:N0}\n" +
            $"⭐ Level: {level:N0}\n" +
            $"❤️ Health: {health:F1}\n" +
            $"⚡ Energy: {energy:F1}\n" +
            $"😊 Happiness: {happiness:F1}");
    }
}

/// <summary>
/// סקריפט עזר לסיבוב אלמנטי UI
/// </summary>
public class UIElementRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private bool rotateClockwise = true;

    void Update()
    {
        float speed = rotateClockwise ? rotationSpeed : -rotationSpeed;
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
