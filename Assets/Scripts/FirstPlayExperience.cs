using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FirstPlayExperience : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas instructionsCanvas;
    public Text welcomeText;
    public Text instructionText;
    
    [Header("Experience Settings")]
    public float welcomeDisplayTime = 3f;
    public bool showWelcomeMessage = true;
    
    private bool hasStarted = false;
    
    void Start()
    {
        if (showWelcomeMessage)
        {
            ShowWelcomeMessage();
        }
        
        SetupFirstPlayEnvironment();
    }
    
    void ShowWelcomeMessage()
    {
        // יצירת Canvas אוטומטית אם לא קיים
        if (instructionsCanvas == null)
        {
            CreateWelcomeUI();
        }
        
        // הצגת הודעת ברוכים הבאים
        if (welcomeText != null)
        {
            welcomeText.text = "ברוכים הבאים למיניפול!\nזה הזמן שלך להתחיל לשחק...";
        }
        
        if (instructionText != null)
        {
            instructionText.text = "המיניפול שלך מוכן לפעולה!\n• הוא יחפש אוכל ומים\n• ינוח כשהוא עייף\n• תוכל לראות איך הוא לומד להישרד\n\nפשוט תצפה וצפה איך הוא מתפתח!";
        }
        
        // הסתר הודעה אחרי זמן מסוים
        Invoke(nameof(HideWelcomeMessage), welcomeDisplayTime);
    }
    
    void CreateWelcomeUI()
    {
        // יצירת Canvas
        GameObject canvasGO = new GameObject("FirstPlayCanvas");
        instructionsCanvas = canvasGO.AddComponent<Canvas>();
        instructionsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // יצירת Panel רקע
        GameObject panelGO = new GameObject("WelcomePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.3f);
        panelRect.anchorMax = new Vector2(0.9f, 0.7f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // יצירת טקסט כותרת
        GameObject welcomeGO = new GameObject("WelcomeText");
        welcomeGO.transform.SetParent(panelGO.transform, false);
        welcomeText = welcomeGO.AddComponent<Text>();
        welcomeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        welcomeText.fontSize = 28;
        welcomeText.color = Color.white;
        welcomeText.alignment = TextAnchor.MiddleCenter;
        welcomeText.fontStyle = FontStyle.Bold;
        
        RectTransform welcomeRect = welcomeGO.GetComponent<RectTransform>();
        welcomeRect.anchorMin = new Vector2(0.05f, 0.6f);
        welcomeRect.anchorMax = new Vector2(0.95f, 0.95f);
        welcomeRect.offsetMin = Vector2.zero;
        welcomeRect.offsetMax = Vector2.zero;
        
        // יצירת טקסט הוראות
        GameObject instructionGO = new GameObject("InstructionText");
        instructionGO.transform.SetParent(panelGO.transform, false);
        instructionText = instructionGO.AddComponent<Text>();
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.fontSize = 18;
        instructionText.color = Color.cyan;
        instructionText.alignment = TextAnchor.MiddleRight;
        
        RectTransform instructionRect = instructionGO.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.05f, 0.05f);
        instructionRect.anchorMax = new Vector2(0.95f, 0.55f);
        instructionRect.offsetMin = Vector2.zero;
        instructionRect.offsetMax = Vector2.zero;
    }
    
    void HideWelcomeMessage()
    {
        if (instructionsCanvas != null)
        {
            instructionsCanvas.gameObject.SetActive(false);
        }
        hasStarted = true;
    }
    
    void SetupFirstPlayEnvironment()
    {
        // וודא שהסביבה מוכנה לחוויה ראשונה
        
        // מצא את המיניפול הפעיל
        GameObject activePoll = GameObject.Find("Authentic_Minipoll_Character");
        if (activePoll == null) activePoll = GameObject.Find("Penguin_Minipoll");
        
        if (activePoll != null)
        {
            // וודא שהמיניפול במרכז הסצנה
            Vector3 centerPosition = new Vector3(0, 1, 0);
            activePoll.transform.position = centerPosition;
            
            Debug.Log("FirstPlayExperience: המיניפול מוכן לחוויה ראשונה!");
        }
        
        // כוון קמרה לחוויה אופטימלית
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0, 4, -6);
            mainCamera.transform.LookAt(Vector3.zero);
        }
        
        // הוסף תאורה נעימה
        Light mainLight = FindFirstObjectByType<Light>();
        if (mainLight != null)
        {
            mainLight.intensity = 1.2f;
            mainLight.color = new Color(1f, 0.95f, 0.8f); // תאורה חמה ונעימה
        }
    }
    
    void Update()
    {
        // אם השחקן לוחץ על כל מקש, הסתר הודעה - עם Input System החדש
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame && 
            instructionsCanvas != null && instructionsCanvas.gameObject.activeSelf)
        {
            HideWelcomeMessage();
        }
    }
}
