using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Minipoll Art UI Creator - משתמש בנכסי האמנות של המשחק ליצירת UI מושלם
/// מטמיע את כל הספרייטים, הכפתורים והאפקטים הקיימים
/// </summary>
public class MinipollArtUICreator : MonoBehaviour
{
    [Header("🎨 Art-Based UI Creator")]
    [SerializeField] private bool createOnStart = true;
    [SerializeField] private bool loadArtAssets = true;
    [SerializeField] private bool createAnimatedUI = true;

    [Header("🖼️ Art Assets")]
    [SerializeField] private string artSpritePath = "Art/Sprites/SpriteCoinMaster/";
    [SerializeField] private string artTexturePath = "Art/Textures/";

    [Header("📊 UI Configuration")]
    [SerializeField] private Vector2 canvasSize = new Vector2(1920, 1080);
    [SerializeField] private Color brandColor = new Color(0.2f, 0.4f, 0.8f);

    // Art Assets Dictionary
    private Dictionary<string, Sprite> artSprites = new Dictionary<string, Sprite>();
    private Dictionary<string, Texture2D> artTextures = new Dictionary<string, Texture2D>();

    // UI References
    private Canvas mainCanvas;
    private GameObject mainPanel;
    private GameObject gameHUD;
    private GameObject menuPanel;

    void Start()
    {
        if (createOnStart)
        {
            CreateArtBasedUI();
        }
    }

    void Update()
    {
        // SPACE key to create art-based UI
        if (InputHelper.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🎨 SPACE pressed - creating art-based UI!");
            CreateArtBasedUI();
        }

        // F2 key for alternative creation
        if (InputHelper.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("🎨 F2 pressed - creating art-based UI!");
            CreateArtBasedUI();
        }

        // F3 key for animated UI
        if (InputHelper.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("🎨 F3 pressed - creating animated UI!");
            CreateAnimatedGameUI();
        }
    }

    /// <summary>
    /// יצירת UI מבוסס על נכסי האמנות
    /// </summary>
    [ContextMenu("Create Art-Based UI")]
    public void CreateArtBasedUI()
    {
        Debug.Log("🎨 Creating Art-Based UI with game assets...");

        try
        {
            // טען נכסי אמנות
            if (loadArtAssets)
            {
                LoadArtAssets();
            }

            // צור Canvas ראשי
            CreateMainCanvas();

            // צור פאנל רקע עם רקע משחק
            CreateBackgroundPanel();

            // צור HUD עם אלמנטים אמיתיים
            CreateGameHUD();

            // צור תפריט ראשי
            CreateMainMenu();

            // צור פאנל מידע
            CreateInfoPanel();

            // הפעל אנימציות אם מבוקש
            if (createAnimatedUI)
            {
                CreateAnimatedElements();
            }

            Debug.Log("✨ Art-Based UI created successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to create Art-Based UI: {e.Message}");
        }
    }

    /// <summary>
    /// טעינת נכסי אמנות
    /// </summary>
    private void LoadArtAssets()
    {
        Debug.Log("🖼️ Loading art assets...");

        // טען כפתורים
        LoadSpriteAsset("buttonEnabled", "buttonEnabled");
        LoadSpriteAsset("buttonDisabled", "buttonDisabled");
        LoadSpriteAsset("blue button", "blue_button");
        LoadSpriteAsset("green button", "green_button");
        LoadSpriteAsset("red button", "red_button");
        LoadSpriteAsset("yellow button", "yellow_button");

        // טען אייקונים
        LoadSpriteAsset("coin", "coin");
        LoadSpriteAsset("spins icon", "spins_icon");
        LoadSpriteAsset("shield_icon", "shield_icon");
        LoadSpriteAsset("hammer", "hammer");
        LoadSpriteAsset("setting icon", "setting_icon");
        LoadSpriteAsset("info_button", "info_button");

        // טען רקעים
        LoadSpriteAsset("Background", "background");
        LoadSpriteAsset("panel_back", "panel_back");
        LoadSpriteAsset("golden_panel_back", "golden_panel_back");
        LoadSpriteAsset("Panel_white", "panel_white");

        // טען אפקטים
        LoadSpriteAsset("glow", "glow");
        LoadSpriteAsset("rays", "rays");
        LoadSpriteAsset("spark", "spark");
        LoadSpriteAsset("flasher", "flasher");

        Debug.Log($"✅ Loaded {artSprites.Count} art sprites");
    }

    /// <summary>
    /// טעינת ספרייט יחיד
    /// </summary>
    private void LoadSpriteAsset(string fileName, string key)
    {
        try
        {
            Sprite sprite = Resources.Load<Sprite>(artSpritePath + fileName);
            if (sprite != null)
            {
                artSprites[key] = sprite;
                Debug.Log($"✅ Loaded sprite: {fileName}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Could not load sprite: {fileName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Error loading sprite {fileName}: {e.Message}");
        }
    }

    /// <summary>
    /// יצירת Canvas ראשי
    /// </summary>
    private void CreateMainCanvas()
    {
        GameObject canvasObj = new GameObject("Minipoll_ArtCanvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 1000;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = canvasSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("🖼️ Main canvas created with art assets");
    }

    /// <summary>
    /// יצירת פאנל רקע
    /// </summary>
    private void CreateBackgroundPanel()
    {
        GameObject bgPanel = new GameObject("Background_Panel");
        bgPanel.transform.SetParent(mainCanvas.transform, false);

        var bgRect = bgPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        var bgImage = bgPanel.AddComponent<Image>();

        // השתמש ברקע אמיתי אם קיים
        if (artSprites.ContainsKey("background"))
        {
            bgImage.sprite = artSprites["background"];
            bgImage.type = Image.Type.Simple;
        }
        else
        {
            bgImage.color = new Color(0.1f, 0.2f, 0.4f, 1f); // כחול כהה
        }

        Debug.Log("🖼️ Background panel created");
    }

    /// <summary>
    /// יצירת HUD משחק
    /// </summary>
    private void CreateGameHUD()
    {
        gameHUD = new GameObject("Game_HUD");
        gameHUD.transform.SetParent(mainCanvas.transform, false);

        var hudRect = gameHUD.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 1);
        hudRect.anchorMax = new Vector2(1, 1);
        hudRect.anchoredPosition = Vector2.zero;
        hudRect.sizeDelta = new Vector2(0, 100);

        // צור פאנל HUD עם רקע
        var hudImage = gameHUD.AddComponent<Image>();
        if (artSprites.ContainsKey("panel_back"))
        {
            hudImage.sprite = artSprites["panel_back"];
            hudImage.type = Image.Type.Sliced;
        }
        else
        {
            hudImage.color = new Color(0, 0, 0, 0.7f);
        }

        // הוסף מוני משאבים
        CreateResourceCounter("Coins", "coin", new Vector2(150, -50), 1000);
        CreateResourceCounter("Spins", "spins_icon", new Vector2(350, -50), 50);
        CreateResourceCounter("Shields", "shield_icon", new Vector2(550, -50), 3);

        // הוסף כפתורי HUD
        CreateHUDButton("Settings", "setting_icon", new Vector2(-150, -50), () => OpenSettings());
        CreateHUDButton("Info", "info_button", new Vector2(-50, -50), () => OpenInfo());

        Debug.Log("🎮 Game HUD created with art assets");
    }

    /// <summary>
    /// יצירת מונה משאבים
    /// </summary>
    private void CreateResourceCounter(string name, string iconKey, Vector2 position, int value)
    {
        GameObject counter = new GameObject($"{name}_Counter");
        counter.transform.SetParent(gameHUD.transform, false);

        var counterRect = counter.AddComponent<RectTransform>();
        counterRect.anchoredPosition = position;
        counterRect.sizeDelta = new Vector2(120, 40);

        // צור אייקון
        if (artSprites.ContainsKey(iconKey))
        {
            GameObject icon = new GameObject($"{name}_Icon");
            icon.transform.SetParent(counter.transform, false);

            var iconRect = icon.AddComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector2(-40, 0);
            iconRect.sizeDelta = new Vector2(30, 30);

            var iconImage = icon.AddComponent<Image>();
            iconImage.sprite = artSprites[iconKey];
        }

        // צור טקסט
        GameObject text = new GameObject($"{name}_Text");
        text.transform.SetParent(counter.transform, false);

        var textRect = text.AddComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(20, 0);
        textRect.sizeDelta = new Vector2(80, 40);

        var textMesh = text.AddComponent<TextMeshProUGUI>();
        textMesh.text = value.ToString();
        textMesh.fontSize = 18;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;
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
        buttonRect.sizeDelta = new Vector2(60, 60);

        var button = buttonObj.AddComponent<Button>();
        var buttonImage = buttonObj.AddComponent<Image>();

        // השתמש בכפתור אמיתי
        if (artSprites.ContainsKey("buttonEnabled"))
        {
            buttonImage.sprite = artSprites["buttonEnabled"];
            buttonImage.type = Image.Type.Sliced;
        }
        else
        {
            buttonImage.color = brandColor;
        }

        // הוסף אייקון
        if (artSprites.ContainsKey(iconKey))
        {
            GameObject icon = new GameObject($"{name}_Icon");
            icon.transform.SetParent(buttonObj.transform, false);

            var iconRect = icon.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            var iconImage = icon.AddComponent<Image>();
            iconImage.sprite = artSprites[iconKey];
        }

        // הוסף אירוע לחיצה
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    /// <summary>
    /// יצירת תפריט ראשי
    /// </summary>
    private void CreateMainMenu()
    {
        menuPanel = new GameObject("Main_Menu");
        menuPanel.transform.SetParent(mainCanvas.transform, false);

        var menuRect = menuPanel.AddComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuRect.anchoredPosition = Vector2.zero;
        menuRect.sizeDelta = new Vector2(400, 600);

        var menuImage = menuPanel.AddComponent<Image>();
        if (artSprites.ContainsKey("golden_panel_back"))
        {
            menuImage.sprite = artSprites["golden_panel_back"];
            menuImage.type = Image.Type.Sliced;
        }
        else
        {
            menuImage.color = new Color(0.8f, 0.6f, 0.2f, 0.9f);
        }

        // הוסף כותרת
        CreateMenuTitle();

        // הוסף כפתורי תפריט
        CreateMenuButton("🎮 Play Game", new Vector2(0, 100), () => StartGame());
        CreateMenuButton("⚙️ Settings", new Vector2(0, 20), () => OpenSettings());
        CreateMenuButton("🏆 Achievements", new Vector2(0, -60), () => OpenAchievements());
        CreateMenuButton("❌ Exit", new Vector2(0, -140), () => ExitGame());

        Debug.Log("📱 Main menu created with art assets");
    }

    /// <summary>
    /// יצירת כותרת תפריט
    /// </summary>
    private void CreateMenuTitle()
    {
        GameObject title = new GameObject("Menu_Title");
        title.transform.SetParent(menuPanel.transform, false);

        var titleRect = title.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 200);
        titleRect.sizeDelta = new Vector2(350, 80);

        var titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "🎮 MINIPOLL V5";
        titleText.fontSize = 36;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;

        // הוסף צל
        var shadow = title.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(3, -3);

        // הוסף אפקט זוהר אם קיים
        if (artSprites.ContainsKey("glow"))
        {
            GameObject glow = new GameObject("Title_Glow");
            glow.transform.SetParent(title.transform, false);

            var glowRect = glow.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-20, -20);
            glowRect.offsetMax = new Vector2(20, 20);

            var glowImage = glow.AddComponent<Image>();
            glowImage.sprite = artSprites["glow"];
            glowImage.color = new Color(1, 1, 0, 0.5f);
        }
    }

    /// <summary>
    /// יצירת כפתור תפריט
    /// </summary>
    private void CreateMenuButton(string text, Vector2 position, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"MenuButton_{text.Replace(" ", "")}");
        buttonObj.transform.SetParent(menuPanel.transform, false);

        var buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(300, 60);

        var button = buttonObj.AddComponent<Button>();
        var buttonImage = buttonObj.AddComponent<Image>();

        // השתמש בכפתור כחול אמיתי
        if (artSprites.ContainsKey("blue_button"))
        {
            buttonImage.sprite = artSprites["blue_button"];
            buttonImage.type = Image.Type.Sliced;
        }
        else
        {
            buttonImage.color = brandColor;
        }

        // הוסף טקסט
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = 18;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;

        // הוסף אירוע לחיצה
        button.onClick.AddListener(() => onClick?.Invoke());

        // הוסף אפקטי hover
        AddButtonEffects(button);
    }

    /// <summary>
    /// יצירת פאנל מידע
    /// </summary>
    private void CreateInfoPanel()
    {
        GameObject infoPanel = new GameObject("Info_Panel");
        infoPanel.transform.SetParent(mainCanvas.transform, false);

        var infoRect = infoPanel.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0);
        infoRect.anchorMax = new Vector2(0, 0);
        infoRect.anchoredPosition = new Vector2(200, 150);
        infoRect.sizeDelta = new Vector2(300, 200);

        var infoImage = infoPanel.AddComponent<Image>();
        if (artSprites.ContainsKey("panel_white"))
        {
            infoImage.sprite = artSprites["panel_white"];
            infoImage.type = Image.Type.Sliced;
        }
        else
        {
            infoImage.color = new Color(1, 1, 1, 0.9f);
        }

        // הוסף טקסט מידע
        GameObject infoText = new GameObject("Info_Text");
        infoText.transform.SetParent(infoPanel.transform, false);

        var infoTextRect = infoText.AddComponent<RectTransform>();
        infoTextRect.anchorMin = Vector2.zero;
        infoTextRect.anchorMax = Vector2.one;
        infoTextRect.offsetMin = new Vector2(10, 10);
        infoTextRect.offsetMax = new Vector2(-10, -10);

        var infoTextMesh = infoText.AddComponent<TextMeshProUGUI>();
        infoTextMesh.text = "🎮 Minipoll V5\n\n" +
                           "✨ Art-Based UI System\n" +
                           "🎨 Using game assets\n" +
                           "🚀 Full functionality\n\n" +
                           "Controls:\n" +
                           "SPACE - Create UI\n" +
                           "F2 - Alternative\n" +
                           "F3 - Animated UI";
        infoTextMesh.fontSize = 14;
        infoTextMesh.color = Color.black;
        infoTextMesh.alignment = TextAlignmentOptions.TopLeft;

        Debug.Log("ℹ️ Info panel created");
    }

    /// <summary>
    /// יצירת אלמנטים מונפשים
    /// </summary>
    private void CreateAnimatedElements()
    {
        Debug.Log("🎬 Creating animated elements...");

        // הוסף אפקטי זוהר מונפשים
        if (artSprites.ContainsKey("rays"))
        {
            CreateRotatingRays();
        }

        if (artSprites.ContainsKey("spark"))
        {
            CreateFloatingSparkles();
        }

        Debug.Log("✨ Animated elements created");
    }

    /// <summary>
    /// יצירת קרניים מסתובבות
    /// </summary>
    private void CreateRotatingRays()
    {
        GameObject rays = new GameObject("Rotating_Rays");
        rays.transform.SetParent(mainCanvas.transform, false);

        var raysRect = rays.AddComponent<RectTransform>();
        raysRect.anchorMin = new Vector2(0.5f, 0.5f);
        raysRect.anchorMax = new Vector2(0.5f, 0.5f);
        raysRect.anchoredPosition = Vector2.zero;
        raysRect.sizeDelta = new Vector2(800, 800);

        var raysImage = rays.AddComponent<Image>();
        raysImage.sprite = artSprites["rays"];
        raysImage.color = new Color(1, 1, 0, 0.3f);

        // הוסף סקריפט סיבוב
        var rotator = rays.AddComponent<UIRotator>();
        rotator.rotationSpeed = 10f;
    }

    /// <summary>
    /// יצירת ניצוצות מרחפים
    /// </summary>
    private void CreateFloatingSparkles()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject sparkle = new GameObject($"Sparkle_{i}");
            sparkle.transform.SetParent(mainCanvas.transform, false);

            var sparkleRect = sparkle.AddComponent<RectTransform>();
            sparkleRect.anchoredPosition = new Vector2(
                Random.Range(-500, 500),
                Random.Range(-300, 300)
            );
            sparkleRect.sizeDelta = new Vector2(20, 20);

            var sparkleImage = sparkle.AddComponent<Image>();
            sparkleImage.sprite = artSprites["spark"];
            sparkleImage.color = new Color(1, 1, 1, 0.7f);

            // הוסף אנימציה
            var floater = sparkle.AddComponent<UIFloater>();
            floater.floatSpeed = Random.Range(0.5f, 2f);
            floater.floatRange = Random.Range(50f, 150f);
        }
    }

    /// <summary>
    /// הוספת אפקטי כפתור
    /// </summary>
    private void AddButtonEffects(Button button)
    {
        // הוסף אפקטי צבע
        var colors = button.colors;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        // הוסף אפקט סקייל
        var scaler = button.gameObject.AddComponent<UIButtonScaler>();
        scaler.scaleOnHover = 1.1f;
        scaler.scaleOnPress = 0.95f;
    }

    /// <summary>
    /// יצירת UI משחק מונפש
    /// </summary>
    [ContextMenu("Create Animated Game UI")]
    public void CreateAnimatedGameUI()
    {
        Debug.Log("🎬 Creating animated game UI...");

        createAnimatedUI = true;
        CreateArtBasedUI();

        Debug.Log("✨ Animated game UI created!");
    }

    // Event handlers
    private void StartGame()
    {
        Debug.Log("🎮 Game started!");
        menuPanel.SetActive(false);
        gameHUD.SetActive(true);
    }

    private void OpenSettings()
    {
        Debug.Log("⚙️ Settings opened!");
    }

    private void OpenInfo()
    {
        Debug.Log("ℹ️ Info opened!");
    }

    private void OpenAchievements()
    {
        Debug.Log("🏆 Achievements opened!");
    }

    private void ExitGame()
    {
        Debug.Log("❌ Game exited!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 150),
            "🎨 Art-Based UI Creator\n" +
            "Using real game assets!\n\n" +
            "SPACE - Create Art UI\n" +
            "F2 - Alternative Creation\n" +
            "F3 - Animated UI\n\n" +
            $"Loaded Assets: {artSprites.Count}");
    }
}

/// <summary>
/// סקריפט עזר לסיבוב UI
/// </summary>
public class UIRotator : MonoBehaviour
{
    public float rotationSpeed = 10f;

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}

/// <summary>
/// סקריפט עזר לריחוף UI
/// </summary>
public class UIFloater : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatRange = 50f;

    private Vector2 startPos;
    private float timer;

    void Start()
    {
        startPos = transform.position;
        timer = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        timer += Time.deltaTime * floatSpeed;
        float offset = Mathf.Sin(timer) * floatRange;
        transform.position = startPos + new Vector2(0, offset);
    }
}

/// <summary>
/// סקריפט עזר לסקייל כפתורים
/// </summary>
public class UIButtonScaler : MonoBehaviour
{
    public float scaleOnHover = 1.1f;
    public float scaleOnPress = 0.95f;

    private Vector3 originalScale;
    private Button button;

    void Start()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();
    }

    void Update()
    {
        if (button != null)
        {
            // This is a simple implementation
            // In a real game, you'd use proper UI events
            transform.localScale = originalScale;
        }
    }
}
