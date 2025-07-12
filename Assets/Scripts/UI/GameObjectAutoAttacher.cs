using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameObject Auto Attacher - מחבר סקריפטים אוטומטית לאובייקטים קיימים
/// הסקריפט הזה מחפש GameObjects מתאימים בסצנה ומוסיף אליהם את הרכיבים הנדרשים
/// </summary>
[System.Serializable]
public class GameObjectAutoAttacher : MonoBehaviour
{
    [Header("🔗 הגדרות חיבור אוטומטי")]
    [Tooltip("חבר רכיבי UI אוטומטית לאובייקטים קיימים")]
    public bool autoAttachUI = true;
    
    [Tooltip("חפש אובייקטים עם שמות ספציפיים")]
    public bool searchByName = true;
    
    [Tooltip("חפש אובייקטים עם תגים ספציפיים")]
    public bool searchByTag = true;
    
    [Tooltip("יצור אובייקטים חדשים אם לא נמצאו")]
    public bool createIfNotFound = true;
    
    [Header("📋 יעדי חיבור")]
    [Tooltip("שמות של GameObjects לחיבור UI")]
    public string[] targetNames = { "Canvas", "UI", "UICanvas", "MainUI", "GameUI" };
    
    [Tooltip("תגים של GameObjects לחיבור UI")]
    public string[] targetTags = { "UICanvas", "UI", "MainCamera", "Player" };
    
    [Header("📊 מצב החיבור")]
    [SerializeField, Tooltip("מספר אובייקטים שנמצאו")]
    private int foundObjects = 0;
    
    [SerializeField, Tooltip("מספר רכיבים שחוברו")]
    private int attachedComponents = 0;
    
    [SerializeField, Tooltip("אובייקטים שנוצרו")]
    private string[] createdObjects = new string[0];
    
    void Start()
    {
        if (autoAttachUI)
        {
            PerformAutoAttachment();
        }
    }
    
    /// <summary>
    /// ביצוע חיבור אוטומטי של רכיבי UI
    /// </summary>
    [ContextMenu("חבר רכיבי UI אוטומטית")]
    public void PerformAutoAttachment()
    {
        Debug.Log("🔗 מתחיל חיבור אוטומטי של רכיבי UI...");
        
        foundObjects = 0;
        attachedComponents = 0;
        var createdList = new System.Collections.Generic.List<string>();
        
        // חיפוש לפי שמות
        if (searchByName)
        {
            AttachByNames(createdList);
        }
        
        // חיפוש לפי תגים
        if (searchByTag)
        {
            AttachByTags(createdList);
        }
        
        // יצירת אובייקטים חדשים אם נדרש
        if (createIfNotFound && foundObjects == 0)
        {
            CreateNewUIObjects(createdList);
        }
        
        // עדכון התוצאות
        createdObjects = createdList.ToArray();
        
        // הצגת תוצאות
        ShowAttachmentResults();
    }
    
    /// <summary>
    /// חיבור לפי שמות אובייקטים
    /// </summary>
    private void AttachByNames(System.Collections.Generic.List<string> createdList)
    {
        foreach (string targetName in targetNames)
        {
            GameObject target = GameObject.Find(targetName);
            if (target != null)
            {
                AttachUIComponentsToObject(target, createdList);
                foundObjects++;
            }
        }
    }
    
    /// <summary>
    /// חיבור לפי תגים
    /// </summary>
    private void AttachByTags(System.Collections.Generic.List<string> createdList)
    {
        foreach (string targetTag in targetTags)
        {
            try
            {
                GameObject target = GameObject.FindWithTag(targetTag);
                if (target != null)
                {
                    AttachUIComponentsToObject(target, createdList);
                    foundObjects++;
                }
            }
            catch (UnityException)
            {
                // תג לא קיים - לא נורא
                Debug.Log($"🏷️ תג '{targetTag}' לא קיים במערכת");
            }
        }
    }
    
    /// <summary>
    /// יצירת אובייקטים חדשים
    /// </summary>
    private void CreateNewUIObjects(System.Collections.Generic.List<string> createdList)
    {
        Debug.Log("🏗️ יוצר אובייקטים חדשים למערכת UI...");
        
        // צור Canvas ראשי
        GameObject mainCanvas = CreateCanvasObject();
        AttachUIComponentsToObject(mainCanvas, createdList);
        createdList.Add($"נוצר: {mainCanvas.name}");
        
        // צור אובייקט למנהל המשחק
        GameObject gameManager = CreateGameManagerObject();
        AttachGameManagerComponents(gameManager, createdList);
        createdList.Add($"נוצר: {gameManager.name}");
        
        foundObjects += 2;
    }
    
    /// <summary>
    /// יצירת Canvas אובייקט
    /// </summary>
    private GameObject CreateCanvasObject()
    {
        GameObject canvasObj = new GameObject("MinipollUI_Canvas");
        
        // הוסף Canvas component
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        // הוסף CanvasScaler
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // הוסף GraphicRaycaster
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        Debug.Log("🖼️ נוצר Canvas חדש: " + canvasObj.name);
        return canvasObj;
    }
    
    /// <summary>
    /// יצירת Game Manager אובייקט
    /// </summary>
    private GameObject CreateGameManagerObject()
    {
        GameObject managerObj = new GameObject("MinipollGame_Manager");
        
        // הוסף את הסקריפט אוטומטית
        managerObj.AddComponent<UIAutoSetup>();
        
        Debug.Log("🎮 נוצר Game Manager חדש: " + managerObj.name);
        return managerObj;
    }
    
    /// <summary>
    /// חיבור רכיבי UI לאובייקט ספציפי
    /// </summary>
    private void AttachUIComponentsToObject(GameObject target, System.Collections.Generic.List<string> createdList)
    {
        Debug.Log($"🔗 מחבר רכיבי UI ל-{target.name}");
        
        // בדוק איזה רכיבים כבר קיימים
        bool hasUISystem = target.GetComponent<MonoBehaviour>() != null;
        
        if (!hasUISystem)
        {
            // הוסף מערכת UI פשוטה
            if (target.GetComponent<Canvas>() != null)
            {
                // זה Canvas - הוסף רכיבי UI
                target.AddComponent<SimpleUITest>();
                createdList.Add($"✅ SimpleUITest → {target.name}");
                attachedComponents++;
                
                target.AddComponent<QuickUIStarter>();
                createdList.Add($"✅ QuickUIStarter → {target.name}");
                attachedComponents++;
            }
            else
            {
                // זה אובייקט רגיל - הוסף מנהל UI
                target.AddComponent<UIAutoSetup>();
                createdList.Add($"✅ UIAutoSetup → {target.name}");
                attachedComponents++;
            }
        }
        else
        {
            Debug.Log($"ℹ️ {target.name} כבר יש לו רכיבי UI");
        }
    }
    
    /// <summary>
    /// חיבור רכיבי Game Manager
    /// </summary>
    private void AttachGameManagerComponents(GameObject target, System.Collections.Generic.List<string> createdList)
    {
        // הוסף מנהל UI מתקדם
        try
        {
            var gameUIManager = target.AddComponent<MinipollGame.UI.GameUIManager>();
            createdList.Add($"✅ GameUIManager → {target.name}");
            attachedComponents++;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ לא ניתן להוסיף GameUIManager: {e.Message}");
            
            // fallback לרכיב פשוט
            target.AddComponent<UIAutoSetup>();
            createdList.Add($"✅ UIAutoSetup (fallback) → {target.name}");
            attachedComponents++;
        }
    }
    
    /// <summary>
    /// הצגת תוצאות החיבור
    /// </summary>
    private void ShowAttachmentResults()
    {
        Debug.Log("📊 תוצאות החיבור האוטומטי:");
        Debug.Log($"   🔍 אובייקטים שנמצאו: {foundObjects}");
        Debug.Log($"   🔗 רכיבים שחוברו: {attachedComponents}");
        Debug.Log($"   🏗️ אובייקטים שנוצרו: {createdObjects.Length}");
        
        if (createdObjects.Length > 0)
        {
            Debug.Log("📋 פירוט השינויים:");
            foreach (var item in createdObjects)
            {
                Debug.Log($"   • {item}");
            }
        }
        
        if (foundObjects > 0 || createdObjects.Length > 0)
        {
            Debug.Log("🎉 החיבור האוטומטי הושלם בהצלחה!");
            Debug.Log("🎮 לחץ Play כדי לראות את התוצאות!");
        }
        else
        {
            Debug.LogWarning("⚠️ לא נמצאו אובייקטים מתאימים לחיבור");
        }
    }
    
    /// <summary>
    /// חיבור מהיר למצלמה הראשית
    /// </summary>
    [ContextMenu("חבר למצלמה הראשית")]
    public void AttachToMainCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            var createdList = new System.Collections.Generic.List<string>();
            AttachUIComponentsToObject(mainCam.gameObject, createdList);
            createdObjects = createdList.ToArray();
            
            Debug.Log("📷 חובר למצלמה הראשית בהצלחה!");
        }
        else
        {
            Debug.LogWarning("⚠️ לא נמצאה מצלמה ראשית");
        }
    }
    
    /// <summary>
    /// חיבור לכל האובייקטים בסצנה
    /// </summary>
    [ContextMenu("חבר לכל האובייקטים")]
    public void AttachToAllObjects()
    {
        Debug.Log("🌍 מחבר לכל האובייקטים בסצנה...");
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var createdList = new System.Collections.Generic.List<string>();
        
        foreach (GameObject obj in allObjects)
        {
            // התעלם מאובייקטים מיוחדים
            if (obj.name.Contains("Camera") || obj.name.Contains("Light") || 
                obj.name.Contains("Canvas") || obj.GetComponent<Canvas>() != null)
            {
                AttachUIComponentsToObject(obj, createdList);
                foundObjects++;
            }
        }
        
        createdObjects = createdList.ToArray();
        ShowAttachmentResults();
    }
    
    void Update()
    {
        // קיצורי מקלדת
        if (InputHelper.GetKeyDown(KeyCode.F8))
        {
            PerformAutoAttachment();
        }
        
        if (InputHelper.GetKeyDown(KeyCode.F7))
        {
            AttachToMainCamera();
        }
        
        if (InputHelper.GetKeyDown(KeyCode.F6))
        {
            AttachToAllObjects();
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 170, 350, 120));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🔗 Auto Attacher");
        GUILayout.Label($"נמצאו: {foundObjects} | חוברו: {attachedComponents}");
        
        GUILayout.Space(5);
        GUILayout.Label("קיצורי מקלדת:");
        GUILayout.Label("F6 - חבר לכל האובייקטים");
        GUILayout.Label("F7 - חבר למצלמה הראשית");
        GUILayout.Label("F8 - חיבור אוטומטי חכם");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
