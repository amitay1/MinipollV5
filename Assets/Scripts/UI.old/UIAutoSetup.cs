using UnityEngine;

/// <summary>
/// UI Auto Setup - יוצר אוטומטית GameObject עם רכיב UI מצורף
/// סקריפט זה יוצר את כל מה שצריך ביוניטי עם הרכיבים המתאימים
/// </summary>
[System.Serializable]
public class UIAutoSetup : MonoBehaviour
{
    [Header("🎮 הגדרות UI אוטומטיות")]
    [Tooltip("צור אוטומטית את מערכת ה-UI כשהמשחק מתחיל")]
    public bool createUIOnStart = true;
    
    [Tooltip("השתמש במערכת UI פשוטה (ללא תלויות חיצוניות)")]
    public bool useSimpleUI = true;
    
    [Tooltip("הצג מידע דיבוג בקונסולה")]
    public bool showDebugInfo = true;
    
    [Tooltip("צור גם בקרי בדיקה")]
    public bool createTestControls = true;
    
    [Header("📋 מצב המערכת")]
    [SerializeField, Tooltip("מצב נוכחי של מערכת ה-UI")]
    private string systemStatus = "לא הופעל";
    
    [SerializeField, Tooltip("רכיבי UI שנוצרו")]
    private string[] createdComponents = new string[0];
    
    void Start()
    {
        if (createUIOnStart)
        {
            CreateCompleteUISystem();
        }
    }
    
    /// <summary>
    /// יצירת מערכת UI מלאה עם כל הרכיבים
    /// </summary>
    [ContextMenu("צור מערכת UI מלאה")]
    public void CreateCompleteUISystem()
    {
        systemStatus = "יוצר מערכת UI...";
        
        if (showDebugInfo)
        {
            Debug.Log("🎮 UIAutoSetup: יוצר מערכת UI מלאה...");
        }
        
        // צור GameObject ראשי למערכת UI
        GameObject uiSystemRoot = CreateUISystemRoot();
        
        // הוסף רכיבי UI
        AddUIComponents(uiSystemRoot);
        
        // צור בקרי בדיקה אם מבוקש
        if (createTestControls)
        {
            CreateTestingControls(uiSystemRoot);
        }
        
        systemStatus = "✅ מערכת UI פעילה";
        
        if (showDebugInfo)
        {
            Debug.Log("✨ מערכת UI נוצרה בהצלחה! כל הרכיבים מחוברים.");
        }
        
        // הצג הודעת הצלחה
        ShowSuccessMessage();
    }
    
    /// <summary>
    /// יצירת GameObject ראשי למערכת UI
    /// </summary>
    private GameObject CreateUISystemRoot()
    {
        // בדוק אם כבר קיים
        GameObject existing = GameObject.Find("MinipollUI_System");
        if (existing != null)
        {
            if (showDebugInfo)
            {
                Debug.Log("🔄 מערכת UI כבר קיימת, מעדכן...");
            }
            DestroyImmediate(existing);
        }
        
        // צור GameObject חדש
        GameObject uiRoot = new GameObject("MinipollUI_System");
        uiRoot.transform.SetParent(this.transform);
        
        // הוסף תג לזיהוי
        uiRoot.tag = "UISystem";
        
        if (showDebugInfo)
        {
            Debug.Log("🏗️ נוצר GameObject ראשי: " + uiRoot.name);
        }
        
        return uiRoot;
    }
    
    /// <summary>
    /// הוספת רכיבי UI לאובייקט
    /// </summary>
    private void AddUIComponents(GameObject uiRoot)
    {
        var createdList = new System.Collections.Generic.List<string>();
        
        if (useSimpleUI)
        {
            // נסה להוסיף מערכת UI פשוטה
            try
            {
                var simpleUI = uiRoot.AddComponent<MinipollGame.UI.BasicUISystem>();
                createdList.Add("BasicUISystem - מערכת UI פשוטה");
                
                if (showDebugInfo)
                {
                    Debug.Log("✅ נוסף רכיב: BasicUISystem");
                }
            }
            catch
            {
                // fallback לרכיב SimpleUITest
                var simpleTest = uiRoot.AddComponent<SimpleUITest>();
                createdList.Add("SimpleUITest - מערכת UI פשוטה (fallback)");
                
                if (showDebugInfo)
                {
                    Debug.Log("✅ נוסף רכיב: SimpleUITest (fallback)");
                }
            }
        }
        else
        {
            // הוסף מערכת UI מתקדמת
            var comprehensiveUI = uiRoot.AddComponent<MinipollGame.UI.ComprehensiveUISystem>();
            createdList.Add("ComprehensiveUISystem - מערכת UI מתקדמת");
            
            if (showDebugInfo)
            {
                Debug.Log("✅ נוסף רכיב: ComprehensiveUISystem");
            }
        }
        
        // הוסף מנהל UI
        GameObject managerObj = new GameObject("UI_Manager");
        managerObj.transform.SetParent(uiRoot.transform);
        var uiManager = managerObj.AddComponent<MinipollGame.UI.GameUIManager>();
        createdList.Add("GameUIManager - מנהל UI");
        
        if (showDebugInfo)
        {
            Debug.Log("✅ נוסף רכיב: GameUIManager");
        }
        
        // עדכן רשימת רכיבים
        createdComponents = createdList.ToArray();
    }
    
    /// <summary>
    /// יצירת בקרי בדיקה
    /// </summary>
    private void CreateTestingControls(GameObject uiRoot)
    {
        // צור אובייקט לבקרי בדיקה
        GameObject testObj = new GameObject("UI_TestControls");
        testObj.transform.SetParent(uiRoot.transform);
        
        // הוסף בקר בדיקה פשוט
        var simpleTest = testObj.AddComponent<SimpleUITest>();
        
        // הוסף בקר התחלה מהירה
        var quickStarter = testObj.AddComponent<QuickUIStarter>();
        
        if (showDebugInfo)
        {
            Debug.Log("🧪 נוצרו בקרי בדיקה");
        }
    }
    
    /// <summary>
    /// הצגת הודעת הצלחה
    /// </summary>
    private void ShowSuccessMessage()
    {
        if (showDebugInfo)
        {
            Debug.Log("🎉 SUCCESS! מערכת UI של Minipoll V5 מוכנה לשימוש!");
            Debug.Log("📋 רכיבים שנוצרו:");
            foreach (var component in createdComponents)
            {
                Debug.Log("  • " + component);
            }
            Debug.Log("🎮 לחץ Play כדי לראות את ה-UI בפעולה!");
        }
    }
    
    /// <summary>
    /// יצירת UI פשוט במיוחד לבדיקה מהירה
    /// </summary>
    [ContextMenu("צור UI פשוט לבדיקה")]
    public void CreateSimpleTestUI()
    {
        GameObject testObj = new GameObject("SimpleUI_Test");
        testObj.transform.SetParent(this.transform);
        testObj.AddComponent<SimpleUITest>();
        
        systemStatus = "✅ UI בדיקה פעיל";
        
        Debug.Log("🧪 נוצר UI פשוט לבדיקה - לחץ Play!");
    }
    
    /// <summary>
    /// ניקוי מערכת UI
    /// </summary>
    [ContextMenu("נקה מערכת UI")]
    public void CleanupUISystem()
    {
        GameObject uiSystem = GameObject.Find("MinipollUI_System");
        if (uiSystem != null)
        {
            DestroyImmediate(uiSystem);
            systemStatus = "נוקה";
            createdComponents = new string[0];
            
            if (showDebugInfo)
            {
                Debug.Log("🧹 מערכת UI נוקתה");
            }
        }
    }
    
    void Update()
    {
        // קיצורי מקלדת מהירים
        if (InputHelper.GetKeyDown(KeyCode.F9))
        {
            CreateCompleteUISystem();
        }
        
        if (InputHelper.GetKeyDown(KeyCode.F10))
        {
            CreateSimpleTestUI();
        }
        
        if (InputHelper.GetKeyDown(KeyCode.F11))
        {
            CleanupUISystem();
        }
    }
    
    void OnGUI()
    {
        if (showDebugInfo)
        {
            GUILayout.BeginArea(new Rect(10, 10, 350, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("🎮 Minipoll V5 - UI Auto Setup");
            GUILayout.Label($"מצב: {systemStatus}");
            GUILayout.Label($"רכיבים: {createdComponents.Length}");
            
            GUILayout.Space(10);
            GUILayout.Label("קיצורי מקלדת:");
            GUILayout.Label("F9 - צור מערכת UI מלאה");
            GUILayout.Label("F10 - צור UI בדיקה פשוט");
            GUILayout.Label("F11 - נקה מערכת UI");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
    
    /// <summary>
    /// מתודה ליצירה אוטומטית בזמן ההפעלה
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void AutoCreateUISystem()
    {
        // בדוק אם כבר קיים UIAutoSetup
        UIAutoSetup existing = FindFirstObjectByType<UIAutoSetup>();
        if (existing == null)
        {
            // צור אוטומטית אם לא קיים
            GameObject autoSetup = new GameObject("UIAutoSetup");
            autoSetup.AddComponent<UIAutoSetup>();
        }
    }
}
