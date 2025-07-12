using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Minipoll Auto Launcher - מפעיל אוטומטית את כל מערכות המשחק
/// הסקריפט הזה מוודא שכל הרכיבים והמערכות פעילים ומחוברים כמו שצריך
/// </summary>
[CreateAssetMenu(fileName = "MinipollAutoLauncher", menuName = "Minipoll/Auto Launcher")]
public class MinipollAutoLauncher : ScriptableObject
{
    [Header("🚀 הגדרות הפעלה אוטומטית")]
    [Tooltip("הפעל את המערכת אוטומטית כשהמשחק מתחיל")]
    public bool launchOnStart = true;
    
    [Tooltip("צור חסרים אם לא קיימים")]
    public bool createMissingComponents = true;
    
    [Tooltip("הצג הודעות דיבוג מפורטות")]
    public bool verboseDebug = true;
    
    [Header("📋 רכיבים לבדיקה")]
    [Tooltip("רשימת סקריפטים שחייבים להיות במערכת")]
    public string[] requiredScripts = {
        "UIAutoSetup",
        "GameObjectAutoAttacher", 
        "SimpleUITest",
        "QuickUIStarter",
        "BasicUISystem"
    };
    
    [Header("📊 מצב המערכת")]
    [SerializeField, Tooltip("מספר רכיבים שנמצאו")]
    private int foundComponents = 0;
    
    [SerializeField, Tooltip("מספר רכיבים שנוצרו")]
    private int createdComponents = 0;
    
    [SerializeField, Tooltip("האם המערכת מוכנה")]
    private bool systemReady = false;
    
    /// <summary>
    /// הפעלה אוטומטית של המערכת
    /// נקראת אוטומטית כשהמשחק מתחיל
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void AutoLaunchSystem()
    {
        // חפש אם יש כבר Launcher פעיל
        MinipollAutoLauncherRunner existing = FindFirstObjectByType<MinipollAutoLauncherRunner>();
        if (existing == null)
        {
            // צור Launcher חדש
            GameObject launcherObj = new GameObject("MinipollAutoLauncher");
            var runner = launcherObj.AddComponent<MinipollAutoLauncherRunner>();
            
            // נסה לטעון הגדרות קיימות
            MinipollAutoLauncher settings = Resources.Load<MinipollAutoLauncher>("MinipollAutoLauncher");
            if (settings != null)
            {
                runner.settings = settings;
            }
        }
    }
    
    /// <summary>
    /// בדיקת מצב המערכת הנוכחי
    /// </summary>
    public void CheckSystemStatus()
    {
        foundComponents = 0;
        systemReady = false;
        
        if (verboseDebug)
        {
            Debug.Log("🔍 בודק מצב מערכת Minipoll...");
        }
        
        foreach (string scriptName in requiredScripts)
        {
            if (FindScriptInScene(scriptName))
            {
                foundComponents++;
                if (verboseDebug)
                {
                    Debug.Log($"✅ נמצא: {scriptName}");
                }
            }
            else
            {
                if (verboseDebug)
                {
                    Debug.Log($"❌ חסר: {scriptName}");
                }
            }
        }
        
        systemReady = (foundComponents == requiredScripts.Length);
        
        if (verboseDebug)
        {
            Debug.Log($"📊 מצב המערכת: {foundComponents}/{requiredScripts.Length} רכיבים פעילים");
            Debug.Log($"🎮 המערכת מוכנה: {(systemReady ? "כן ✅" : "לא ❌")}");
        }
    }
    
    /// <summary>
    /// יצירת רכיבים חסרים
    /// </summary>
    public void CreateMissingComponents()
    {
        if (verboseDebug)
        {
            Debug.Log("🏗️ יוצר רכיבים חסרים...");
        }
        
        createdComponents = 0;
        
        // צור אובייקט ראשי אם לא קיים
        GameObject mainSystem = GameObject.Find("MinipollMainSystem");
        if (mainSystem == null)
        {
            mainSystem = new GameObject("MinipollMainSystem");
            if (verboseDebug)
            {
                Debug.Log("🏗️ נוצר אובייקט ראשי: MinipollMainSystem");
            }
        }
        
        // הוסף רכיבים חסרים
        EnsureComponentExists<MinipollAutoLauncherRunner>(mainSystem, "MinipollAutoLauncherRunner");
        
        // צור אובייקט UI אם לא קיים
        GameObject uiSystem = GameObject.Find("MinipollUISystem");
        if (uiSystem == null)
        {
            uiSystem = new GameObject("MinipollUISystem");
            uiSystem.transform.SetParent(mainSystem.transform);
            if (verboseDebug)
            {
                Debug.Log("🖼️ נוצר אובייקט UI: MinipollUISystem");
            }
        }
        
        // הוסף רכיבי UI
        EnsureComponentExists<UIAutoSetup>(uiSystem, "UIAutoSetup");
        EnsureComponentExists<GameObjectAutoAttacher>(uiSystem, "GameObjectAutoAttacher");
        EnsureComponentExists<SimpleUITest>(uiSystem, "SimpleUITest");
        EnsureComponentExists<QuickUIStarter>(uiSystem, "QuickUIStarter");
        
        // נסה להוסיף BasicUISystem
        try
        {
            if (uiSystem.GetComponent<MinipollGame.UI.BasicUISystem>() == null)
            {
                uiSystem.AddComponent<MinipollGame.UI.BasicUISystem>();
                createdComponents++;
                if (verboseDebug)
                {
                    Debug.Log("✅ נוסף רכיב: BasicUISystem ל-" + uiSystem.name);
                }
            }
        }
        catch
        {
            if (verboseDebug)
            {
                Debug.Log("⚠️ BasicUISystem לא זמין - משתמש ברכיבים בסיסיים");
            }
        }
        
        if (verboseDebug)
        {
            Debug.Log($"🎉 נוצרו {createdComponents} רכיבים חדשים!");
        }
    }
    
    /// <summary>
    /// וידוא שרכיב קיים באובייקט
    /// </summary>
    private void EnsureComponentExists<T>(GameObject target, string componentName) where T : Component
    {
        if (target.GetComponent<T>() == null)
        {
            target.AddComponent<T>();
            createdComponents++;
            if (verboseDebug)
            {
                Debug.Log($"✅ נוסף רכיב: {componentName} ל-{target.name}");
            }
        }
    }
    
    /// <summary>
    /// חיפוש סקריפט בסצנה
    /// </summary>
    private bool FindScriptInScene(string scriptName)
    {
        // חפש לפי שם הסקריפט
        MonoBehaviour[] allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour script in allScripts)
        {
            if (script.GetType().Name == scriptName)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// הפעלה מלאה של המערכת
    /// </summary>
    public void FullSystemLaunch()
    {
        if (verboseDebug)
        {
            Debug.Log("🚀 מתחיל הפעלה מלאה של מערכת Minipoll V5...");
        }
        
        // בדוק מצב נוכחי
        CheckSystemStatus();
        
        // צור רכיבים חסרים אם נדרש
        if (createMissingComponents && !systemReady)
        {
            CreateMissingComponents();
        }
        
        // בדוק שוב אחרי היצירה
        CheckSystemStatus();
        
        // הפעל מערכות UI
        LaunchUISystem();
        
        // הודעת סיום
        if (systemReady)
        {
            Debug.Log("🎉 מערכת Minipoll V5 הופעלה בהצלחה!");
            Debug.Log("🎮 המשחק מוכן לשימוש!");
        }
        else
        {
            Debug.LogWarning("⚠️ יש בעיות במערכת - בדוק את הקונסולה לפרטים");
        }
    }
    
    /// <summary>
    /// הפעלת מערכת UI
    /// </summary>
    private void LaunchUISystem()
    {
        // חפש UIAutoSetup ושה הפעל
        UIAutoSetup uiSetup = FindFirstObjectByType<UIAutoSetup>();
        if (uiSetup != null)
        {
            uiSetup.CreateCompleteUISystem();
            if (verboseDebug)
            {
                Debug.Log("🖼️ מערכת UI הופעלה");
            }
        }
        
        // חפש GameObjectAutoAttacher והפעל
        GameObjectAutoAttacher autoAttacher = FindFirstObjectByType<GameObjectAutoAttacher>();
        if (autoAttacher != null)
        {
            autoAttacher.PerformAutoAttachment();
            if (verboseDebug)
            {
                Debug.Log("🔗 Auto Attacher הופעל");
            }
        }
    }
}

/// <summary>
/// רכיב עזר להפעלת ה-Launcher
/// </summary>
public class MinipollAutoLauncherRunner : MonoBehaviour
{
    [Header("🚀 Minipoll Auto Launcher")]
    public MinipollAutoLauncher settings;
    
    [Header("📊 מצב")]
    [SerializeField] private bool launched = false;
    [SerializeField] private float launchTime = 0f;
    
    void Start()
    {
        if (settings != null && settings.launchOnStart && !launched)
        {
            launchTime = Time.time;
            settings.FullSystemLaunch();
            launched = true;
        }
        else if (settings == null)
        {
            // צור הגדרות ברירת מחדל
            CreateDefaultSettings();
        }
    }
    
    /// <summary>
    /// יצירת הגדרות ברירת מחדל
    /// </summary>
    private void CreateDefaultSettings()
    {
        settings = ScriptableObject.CreateInstance<MinipollAutoLauncher>();
        settings.launchOnStart = true;
        settings.createMissingComponents = true;
        settings.verboseDebug = true;
        
        // הפעל עם הגדרות ברירת מחדל
        settings.FullSystemLaunch();
        launched = true;
        
        Debug.Log("⚙️ נוצרו הגדרות ברירת מחדל ל-Auto Launcher");
    }
    
    void Update()
    {
        // קיצור מקלדת להפעלה ידנית
        if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame)
        {
            if (settings != null)
            {
                settings.FullSystemLaunch();
                launched = true;
            }
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 300, 350, 100));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🚀 Minipoll Auto Launcher");
        GUILayout.Label($"הופעל: {(launched ? "כן ✅" : "לא ❌")}");
        if (launched)
        {
            GUILayout.Label($"זמן הפעלה: {(Time.time - launchTime):F1} שניות");
        }
        
        GUILayout.Space(5);
        GUILayout.Label("F12 - הפעלה ידנית מלאה");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
