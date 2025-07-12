using UnityEngine;

/// <summary>
/// ONE-CLICK UI CREATOR - פתרון אולטימטיבי ליצירת UI
/// פשוט גרור את הסקריפט הזה לכל GameObject ותקבל UI מלא מיד!
/// </summary>
public class OneClickUICreator : MonoBehaviour
{
    [Header("🎮 ONE-CLICK UI CREATOR")]
    [Tooltip("לחץ כאן כדי ליצור UI מלא מיד!")]
    public bool clickToCreateUI = false;
    
    [Space(10)]
    [Header("🎨 מה ייווצר:")]
    [SerializeField] private string[] whatWillBeCreated = {
        "✅ Canvas מלא עם UI מתקדם",
        "✅ כפתורי משחק (Play/Pause/Stop)", 
        "✅ פאנל מידע ונתונים",
        "✅ מערכת דיבוג ובקרה",
        "✅ עיצוב Minipoll מלא",
        "✅ אנימציות וטווינים",
        "✅ מערכת ניהול UI"
    };
    
    [Space(10)]
    [Header("📊 מצב")]
    [SerializeField] private bool uiCreated = false;
    [SerializeField] private string status = "לחץ על הכפתור או SPACE/F9/F2 ליצירה";
    
    void Start()
    {
        // הצג הוראות למשתמש
        Debug.Log("🎮 ONE-CLICK UI CREATOR מוכן!");
        Debug.Log("🔹 לחץ SPACE/F9/F2 ליצירת UI מלא מיד");
        Debug.Log("🔹 או סמן 'Click To Create UI' ב-Inspector");
        Debug.Log("🔹 או לחץ על 'Create UI Now' בתפריט הימני");
    }
    
    void Update()
    {
        // בדוק אם המשתמש לחץ על הכפתור ב-Inspector
        if (clickToCreateUI && !uiCreated)
        {
            CreateCompleteUIInstantly();
            clickToCreateUI = false; // איפוס הכפתור
        }
        
        // SPACE key to create UI
        if (InputHelper.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🎮 SPACE pressed - creating UI instantly!");
            CreateCompleteUIInstantly();
        }
        
        // קיצור מקלדת
        if (InputHelper.GetKeyDown(KeyCode.F9))
        {
            CreateCompleteUIInstantly();
        }
        
        // F2 as alternative
        if (InputHelper.GetKeyDown(KeyCode.F2))
        {
            CreateCompleteUIInstantly();
        }
    }
    
    /// <summary>
    /// יצירת UI מלא מיידית - הפונקציה הראשית!
    /// </summary>
    [ContextMenu("Create UI Now! (יצור UI עכשיו!)")]
    public void CreateCompleteUIInstantly()
    {
        if (uiCreated)
        {
            Debug.Log("🔄 UI כבר קיים - מעדכן...");
            CleanupExistingUI();
        }
        
        status = "יוצר UI מלא...";
        Debug.Log("🚀 ONE-CLICK UI: מתחיל יצירת UI מלא...");
        
        // שלב 1: צור Canvas ראשי
        GameObject mainCanvas = CreateMainCanvas();
        
        // שלב 2: צור פאנלים ראשיים
        CreateMainPanels(mainCanvas);
        
        // שלב 3: הוסף רכיבי ניהול
        AddManagementComponents();
        
        // שלב 4: הוסף בקרים ובדיקות
        AddTestingComponents();
        
        // שלב 5: הפעל מערכות
        ActivateAllSystems();
        
        // סיום
        uiCreated = true;
        status = "✅ UI מלא נוצר בהצלחה!";
        
        ShowSuccessMessage();
    }
    
    /// <summary>
    /// יצירת Canvas ראשי
    /// </summary>
    private GameObject CreateMainCanvas()
    {
        Debug.Log("🖼️ יוצר Canvas ראשי...");
        
        GameObject canvasObj = new GameObject("Minipoll_MainCanvas");
        
        // הוסף Canvas
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        // הוסף CanvasScaler לתמיכה ברזולוציות שונות
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // הוסף GraphicRaycaster לאינטראקציה
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        Debug.Log("✅ Canvas ראשי נוצר");
        return canvasObj;
    }
    
    /// <summary>
    /// יצירת פאנלים ראשיים
    /// </summary>
    private void CreateMainPanels(GameObject canvas)
    {
        Debug.Log("📱 יוצר פאנלים ראשיים...");
        
        // פאנל ראשי
        CreatePanel(canvas, "MainPanel", new Vector2(400, 300), new Vector2(0, 100));
        
        // פאנל בקרה
        CreatePanel(canvas, "ControlPanel", new Vector2(200, 100), new Vector2(-300, -200));
        
        // פאנל מידע
        CreatePanel(canvas, "InfoPanel", new Vector2(300, 200), new Vector2(300, -150));
        
        Debug.Log("✅ פאנלים נוצרו");
    }
    
    /// <summary>
    /// יצירת פאנל יחיד
    /// </summary>
    private GameObject CreatePanel(GameObject parent, string name, Vector2 size, Vector2 position)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);
        
        // הוסף RectTransform
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        // הוסף Image (רקע)
        var image = panel.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.2f, 0.3f, 0.8f, 0.8f); // כחול Minipoll
        
        // הוסף טקסט
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panel.transform, false);
        
        var text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = $"Minipoll V5\n{name}";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return panel;
    }
    
    /// <summary>
    /// הוספת רכיבי ניהול
    /// </summary>
    private void AddManagementComponents()
    {
        Debug.Log("🎮 מוסיף רכיבי ניהול...");
        
        // צור אובייקט מנהל
        GameObject manager = new GameObject("UIManager");
        manager.transform.SetParent(this.transform);
        
        // הוסף רכיבים
        manager.AddComponent<UIAutoSetup>();
        manager.AddComponent<GameObjectAutoAttacher>();
        
        // נסה להוסיף GameUIManager אם קיים
        try
        {
            var gameUIManager = manager.AddComponent<MinipollGame.UI.GameUIManager>();
            Debug.Log("✅ GameUIManager נוסף");
        }
        catch
        {
            Debug.Log("⚠️ GameUIManager לא זמין - משתמש ברכיבים בסיסיים");
        }
        
        Debug.Log("✅ רכיבי ניהול נוספו");
    }
    
    /// <summary>
    /// הוספת רכיבי בדיקה
    /// </summary>
    private void AddTestingComponents()
    {
        Debug.Log("🧪 מוסיף רכיבי בדיקה...");
        
        // צור אובייקט בדיקה
        GameObject tester = new GameObject("UITester");
        tester.transform.SetParent(this.transform);
        
        // הוסף רכיבי בדיקה
        tester.AddComponent<SimpleUITest>();
        tester.AddComponent<QuickUIStarter>();
        
        // נסה להוסיף BasicUISystem
        try
        {
            tester.AddComponent<MinipollGame.UI.BasicUISystem>();
        }
        catch
        {
            Debug.Log("⚠️ BasicUISystem לא זמין - משתמש ברכיבים בסיסיים");
        }
        
        Debug.Log("✅ רכיבי בדיקה נוספו");
    }
    
    /// <summary>
    /// הפעלת כל המערכות
    /// </summary>
    private void ActivateAllSystems()
    {
        Debug.Log("⚡ מפעיל מערכות...");
        
        // הפעל UIAutoSetup אם קיים
        UIAutoSetup uiSetup = FindFirstObjectByType<UIAutoSetup>();
        if (uiSetup != null)
        {
            uiSetup.CreateCompleteUISystem();
        }
        
        // הפעל AutoAttacher אם קיים
        GameObjectAutoAttacher attacher = FindFirstObjectByType<GameObjectAutoAttacher>();
        if (attacher != null)
        {
            attacher.PerformAutoAttachment();
        }
        
        Debug.Log("✅ מערכות הופעלו");
    }
    
    /// <summary>
    /// ניקוי UI קיים
    /// </summary>
    private void CleanupExistingUI()
    {
        // מחק Canvas קיימים של Minipoll
        GameObject[] canvases = GameObject.FindGameObjectsWithTag("UICanvas");
        foreach (GameObject canvas in canvases)
        {
            if (canvas.name.Contains("Minipoll"))
            {
                DestroyImmediate(canvas);
            }
        }
        
        // מחק אובייקטים של UI קיימים
        GameObject existing = GameObject.Find("Minipoll_MainCanvas");
        if (existing != null) DestroyImmediate(existing);
        
        existing = GameObject.Find("MinipollUI_System");
        if (existing != null) DestroyImmediate(existing);
        
        existing = GameObject.Find("MinipollUISystem");
        if (existing != null) DestroyImmediate(existing);
    }
    
    /// <summary>
    /// הצגת הודעת הצלחה
    /// </summary>
    private void ShowSuccessMessage()
    {
        Debug.Log("🎉 ONE-CLICK UI SUCCESS!");
        Debug.Log("================================");
        Debug.Log("✨ נוצר UI מלא עבור Minipoll V5!");
        Debug.Log("📱 Canvas עם פאנלים ראשיים");
        Debug.Log("🎮 מערכת ניהול UI מלאה");
        Debug.Log("🧪 כלי בדיקה ופיתוח");
        Debug.Log("🎨 עיצוב מותאם למותג");
        Debug.Log("================================");
        Debug.Log("🎮 המשחק מוכן! תנו משחק!");
    }
    
    /// <summary>
    /// מחיקת UI
    /// </summary>
    [ContextMenu("Delete UI (מחק UI)")]
    public void DeleteUI()
    {
        CleanupExistingUI();
        uiCreated = false;
        status = "UI נמחק - לחץ F9 ליצירה חדשה";
        Debug.Log("🗑️ UI נמחק");
    }
    
    void OnGUI()
    {
        // הצג מידע על המערכת
        GUILayout.BeginArea(new Rect(10, 410, 400, 120));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🎮 ONE-CLICK UI CREATOR");
        GUILayout.Label($"מצב: {status}");
        GUILayout.Label($"UI נוצר: {(uiCreated ? "כן ✅" : "לא ❌")}");
        
        GUILayout.Space(5);
        if (GUILayout.Button("🚀 יצור UI מלא עכשיו!"))
        {
            CreateCompleteUIInstantly();
        }
        
        if (uiCreated && GUILayout.Button("🗑️ מחק UI"))
        {
            DeleteUI();
        }
        
        GUILayout.Space(5);
        GUILayout.Label("SPACE/F9/F2 - יצירה מהירה");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
