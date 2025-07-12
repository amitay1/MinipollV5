using UnityEngine;
using MinipollGame.UI;
using MinipollGame.Testing;
using MinipollGame.Demo;

/// <summary>
/// Complete UI Showcase - Add this component to demonstrate the full UI system
/// Shows all features, panels, and capabilities of the comprehensive UI
/// Perfect for testing, demos, and showcasing to stakeholders
/// </summary>
public class CompleteUIShowcase : MonoBehaviour
{
    [Header("🎨 UI Showcase Configuration")]
    [Tooltip("Automatically initialize UI system on start")]
    public bool autoInitialize = true;
    
    [Tooltip("Show the UI demo sequence")]
    public bool runDemoSequence = false;
    
    [Tooltip("Enable test controls for manual testing")]
    public bool enableTestControls = true;
    
    [Tooltip("Enable all debugging features")]
    public bool enableDebugMode = true;
    
    [Header("🎮 Demo Settings")]
    [Tooltip("Time between demo steps in seconds")]
    [Range(1f, 10f)]
    public float demoStepDuration = 3f;
    
    [Tooltip("Starting panel for the demo")]
    public string startingPanel = "MainMenu";
    
    private GameUIManager uiManager;
    private UIDemo demoController;
    private UITestController testController;
    private SceneUIInitializer sceneInitializer;
    
    [Header("🎯 Current Status")]
    [SerializeField, Tooltip("Current UI system status")]
    private string systemStatus = "Not Initialized";
    
    [SerializeField, Tooltip("Active panel name")]
    private string activePanel = "None";
    
    [SerializeField, Tooltip("Number of notifications shown")]
    private int notificationCount = 0;
    
    void Start()
    {
        if (autoInitialize)
        {
            InitializeCompleteUISystem();
        }
    }
    
    /// <summary>
    /// Initialize the complete UI system with all components
    /// </summary>
    public void InitializeCompleteUISystem()
    {
        Debug.Log("🎨 CompleteUIShowcase: Initializing full UI system...");
        systemStatus = "Initializing...";
        
        // 1. Create Scene UI Initializer
        CreateSceneInitializer();
        
        // 2. Get or create UI Manager
        SetupUIManager();
        
        // 3. Create demo controller if requested
        if (runDemoSequence)
        {
            CreateDemoController();
        }
        
        // 4. Create test controller if requested
        if (enableTestControls)
        {
            CreateTestController();
        }
        
        // 5. Show initial welcome
        ShowWelcomeMessage();
        
        systemStatus = "✅ Fully Initialized";
        Debug.Log("✨ Complete UI System ready! All features available.");
    }
    
    private void CreateSceneInitializer()
    {
        GameObject initializerObj = new GameObject("SceneUIInitializer");
        initializerObj.transform.SetParent(transform);
        sceneInitializer = initializerObj.AddComponent<SceneUIInitializer>();
        
        // Configure the initializer
        var initType = typeof(SceneUIInitializer);
        var defaultPanelField = initType.GetField("defaultPanelToShow", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var welcomeField = initType.GetField("showWelcomeNotification", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var debugField = initType.GetField("enableDebugMode", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (defaultPanelField != null) defaultPanelField.SetValue(sceneInitializer, startingPanel);
        if (welcomeField != null) welcomeField.SetValue(sceneInitializer, true);
        if (debugField != null) debugField.SetValue(sceneInitializer, enableDebugMode);
        
        Debug.Log("🎬 Scene UI Initializer created and configured");
    }
    
    private void SetupUIManager()
    {
        uiManager = GameUIManager.Instance;
        if (uiManager == null)
        {
            GameObject uiManagerObj = new GameObject("GameUIManager");
            uiManagerObj.transform.SetParent(transform);
            uiManager = uiManagerObj.AddComponent<GameUIManager>();
            Debug.Log("🎮 UI Manager created");
        }
        else
        {
            Debug.Log("🎮 UI Manager found");
        }
    }
    
    private void CreateDemoController()
    {
        GameObject demoObj = new GameObject("UIDemo");
        demoObj.transform.SetParent(transform);
        demoController = demoObj.AddComponent<UIDemo>();
        
        // Configure demo settings
        var demoType = typeof(UIDemo);
        var autoStartField = demoType.GetField("autoStartDemo", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pauseTimeField = demoType.GetField("demoPauseTime", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (autoStartField != null) autoStartField.SetValue(demoController, runDemoSequence);
        if (pauseTimeField != null) pauseTimeField.SetValue(demoController, demoStepDuration);
        
        Debug.Log("🎬 Demo controller created and configured");
    }
    
    private void CreateTestController()
    {
        GameObject testObj = new GameObject("UITestController");
        testObj.transform.SetParent(transform);
        testController = testObj.AddComponent<MinipollGame.Testing.UITestController>();
        
        Debug.Log("🧪 Test controller created");
    }
    
    private void ShowWelcomeMessage()
    {
        if (uiManager != null)
        {
            uiManager.ShowNotification("🎨 Complete UI System Loaded!", NotificationType.Success);
            notificationCount++;
            
            // Show feature summary
            StartCoroutine(ShowFeatureSummary());
        }
    }
    
    private System.Collections.IEnumerator ShowFeatureSummary()
    {
        yield return new WaitForSeconds(2f);
        
        if (uiManager != null)
        {
            uiManager.ShowNotification("✨ All UI features ready: Menu, HUD, Settings, Achievements, Inventory", NotificationType.Info);
            notificationCount++;
            
            yield return new WaitForSeconds(3f);
            
            string controls = enableTestControls ? "Test controls enabled!" : "Press keys: ESC(pause), I(inventory), H(achievements)";
            uiManager.ShowNotification($"🎮 {controls}", NotificationType.Info);
            notificationCount++;
        }
    }
    
    void Update()
    {
        UpdateSystemStatus();
        HandleShowcaseInput();
    }
    
    private void UpdateSystemStatus()
    {
        if (uiManager != null)
        {
            // Update status information
            var uiSystem = uiManager.GetUISystem();
            if (uiSystem != null)
            {
                systemStatus = "✅ Running";
                // activePanel would need to be exposed from ComprehensiveUISystem
                activePanel = "Available"; // Placeholder
            }
        }
    }
    
    private void HandleShowcaseInput()
    {
        // Showcase-specific controls
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ShowSystemInfo();
        }
        
        if (Input.GetKeyDown(KeyCode.F11))
        {
            DemonstratePanels();
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            DemonstrateNotifications();
        }
    }
    
    /// <summary>
    /// Show detailed system information
    /// </summary>
    public void ShowSystemInfo()
    {
        if (uiManager != null)
        {
            string info = $"🎯 UI System Info:\n" +
                         $"• Status: {systemStatus}\n" +
                         $"• Notifications: {notificationCount}\n" +
                         $"• Demo: {(runDemoSequence ? "Active" : "Inactive")}\n" +
                         $"• Test Controls: {(enableTestControls ? "Enabled" : "Disabled")}\n" +
                         $"• Debug Mode: {(enableDebugMode ? "On" : "Off")}";
            
            Debug.Log(info);
            uiManager.ShowNotification("📊 System info logged to console", NotificationType.Info);
            notificationCount++;
        }
    }
    
    /// <summary>
    /// Demonstrate all available panels
    /// </summary>
    public void DemonstratePanels()
    {
        if (uiManager != null)
        {
            StartCoroutine(CycleThroughPanels());
        }
    }
    
    private System.Collections.IEnumerator CycleThroughPanels()
    {
        string[] panels = { "MainMenu", "GameHUD", "PauseMenu", "Settings", "Achievements", "Inventory" };
        
        uiManager.ShowNotification("🎨 Panel demonstration starting...", NotificationType.Info);
        notificationCount++;
        
        foreach (string panel in panels)
        {
            yield return new WaitForSeconds(2f);
            uiManager.ShowPanel(panel);
            uiManager.ShowNotification($"📱 Showing: {panel}", NotificationType.Info);
            notificationCount++;
        }
        
        yield return new WaitForSeconds(2f);
        uiManager.ShowPanel("MainMenu");
        uiManager.ShowNotification("✅ Panel demonstration complete!", NotificationType.Success);
        notificationCount++;
    }
    
    /// <summary>
    /// Demonstrate all notification types
    /// </summary>
    public void DemonstrateNotifications()
    {
        if (uiManager != null)
        {
            StartCoroutine(ShowAllNotificationTypes());
        }
    }
    
    private System.Collections.IEnumerator ShowAllNotificationTypes()
    {
        NotificationType[] types = { NotificationType.Success, NotificationType.Info, NotificationType.Warning, NotificationType.Error };
        string[] messages = { 
            "✅ Success notification example", 
            "ℹ️ Information notification example", 
            "⚠️ Warning notification example", 
            "❌ Error notification example" 
        };
        
        uiManager.ShowNotification("🔔 Notification demonstration starting...", NotificationType.Info);
        notificationCount++;
        
        for (int i = 0; i < types.Length; i++)
        {
            yield return new WaitForSeconds(1.5f);
            uiManager.ShowNotification(messages[i], types[i]);
            notificationCount++;
        }
        
        yield return new WaitForSeconds(2f);
        uiManager.ShowNotification("✨ All notification types demonstrated!", NotificationType.Success);
        notificationCount++;
    }
    
    void OnGUI()
    {
        // Showcase information panel
        GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🎨 Complete UI Showcase");
        GUILayout.Space(5);
        
        GUILayout.Label($"Status: {systemStatus}");
        GUILayout.Label($"Active Panel: {activePanel}");
        GUILayout.Label($"Notifications: {notificationCount}");
        
        GUILayout.Space(10);
        GUILayout.Label("Showcase Controls:");
        GUILayout.Label("F10 - System Info");
        GUILayout.Label("F11 - Demo Panels");
        GUILayout.Label("F12 - Demo Notifications");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Public method to restart the complete showcase
    /// </summary>
    public void RestartShowcase()
    {
        Debug.Log("🔄 Restarting Complete UI Showcase...");
        
        // Reset counters
        notificationCount = 0;
        systemStatus = "Restarting...";
        
        // Restart demo if active
        if (demoController != null)
        {
            demoController.StartDemo();
        }
        
        // Show restart message
        if (uiManager != null)
        {
            uiManager.ShowNotification("🔄 Showcase restarted!", NotificationType.Success);
            notificationCount++;
        }
    }
}
