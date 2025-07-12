using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple UI Test - Creates a basic UI to verify Unity UI is working
/// This will help debug why the comprehensive UI isn't showing
/// </summary>
public class SimpleUITest : MonoBehaviour
{
    void Start()
    {
        CreateSimpleUI();
    }
    
    void CreateSimpleUI()
    {
        Debug.Log("🧪 Creating Simple UI Test...");
        
        // Create Canvas
        GameObject canvasObj = new GameObject("TestCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        // Add Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create a simple colored background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(canvasObj.transform, false);
        
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.3f, 0.5f, 0.8f); // Blue background
        
        RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        // Create a simple text element
        GameObject textObj = new GameObject("TestText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "🎮 MINIPOLL V5 UI TEST\n\nIf you can see this, Unity UI is working!\n\nPress SPACE to test the full UI system\nPress F2 as alternative\nPress ESC to quit\n\n👆 Click this button to launch UI";
        text.fontSize = 36;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(50, 50);
        textRect.offsetMax = new Vector2(-50, -50);
        
        // Create a simple button
        GameObject buttonObj = new GameObject("TestButton");
        buttonObj.transform.SetParent(canvasObj.transform, false);
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green button
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.3f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.3f);
        buttonRect.sizeDelta = new Vector2(300, 60);
        
        // Button text
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "🚀 LAUNCH FULL UI SYSTEM";
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        // Add button click event
        button.onClick.AddListener(LaunchFullUISystem);
        
        Debug.Log("✅ Simple UI Test created successfully!");
    }
    
    void LaunchFullUISystem()
    {
        Debug.Log("🚀 Launching Full UI System...");
        
        try
        {
            // Destroy this test UI
            Canvas testCanvas = FindFirstObjectByType<Canvas>();
            if (testCanvas != null && testCanvas.name == "TestCanvas")
            {
                Debug.Log("🗑️ Destroying test canvas...");
                Destroy(testCanvas.gameObject);
            }
            
            // Try multiple approaches to create comprehensive UI
            bool uiCreated = false;
            
            // Approach 1: Try ComprehensiveUISystem
            try
            {
                Debug.Log("🎯 Trying ComprehensiveUISystem...");
                GameObject uiSystemObj = new GameObject("ComprehensiveUISystem");
                uiSystemObj.AddComponent<MinipollGame.UI.ComprehensiveUISystem>();
                uiCreated = true;
                Debug.Log("✅ ComprehensiveUISystem created successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ ComprehensiveUISystem failed: {e.Message}");
            }
            
            // Approach 2: Try OneClickUICreator if ComprehensiveUISystem failed
            if (!uiCreated)
            {
                try
                {
                    Debug.Log("🎯 Trying OneClickUICreator...");
                    GameObject oneClickObj = new GameObject("OneClickUICreator");
                    var oneClick = oneClickObj.AddComponent<OneClickUICreator>();
                    oneClick.CreateCompleteUIInstantly();
                    uiCreated = true;
                    Debug.Log("✅ OneClickUICreator created successfully!");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠️ OneClickUICreator failed: {e.Message}");
                }
            }
            
            // Approach 3: Try GameUIManager if others failed
            if (!uiCreated)
            {
                try
                {
                    Debug.Log("🎯 Trying GameUIManager...");
                    GameObject gameUIObj = new GameObject("GameUIManager");
                    gameUIObj.AddComponent<MinipollGame.UI.GameUIManager>();
                    uiCreated = true;
                    Debug.Log("✅ GameUIManager created successfully!");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠️ GameUIManager failed: {e.Message}");
                }
            }
            
            // Approach 4: Fallback to UIAutoSetup
            if (!uiCreated)
            {
                try
                {
                    Debug.Log("🎯 Trying UIAutoSetup as fallback...");
                    GameObject autoSetupObj = new GameObject("UIAutoSetup");
                    var autoSetup = autoSetupObj.AddComponent<UIAutoSetup>();
                    autoSetup.CreateCompleteUISystem();
                    uiCreated = true;
                    Debug.Log("✅ UIAutoSetup created successfully!");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠️ UIAutoSetup failed: {e.Message}");
                }
            }
            
            if (uiCreated)
            {
                Debug.Log("✨ Full UI System launched successfully!");
                
                // Add a UI test controller for additional testing
                try
                {
                    GameObject testControllerObj = new GameObject("UITestController");
                    testControllerObj.AddComponent<MinipollGame.Testing.UITestController>();
                    Debug.Log("🧪 UITestController added for testing!");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠️ UITestController failed: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("❌ Failed to create any UI system!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Critical error in LaunchFullUISystem: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }
    
    void Update()
    {
        // Space key to launch full UI
        if (InputHelper.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🎮 SPACE key pressed - launching full UI system!");
            LaunchFullUISystem();
        }
        
        // F2 key for alternative launch
        if (InputHelper.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("🎮 F2 key pressed - launching full UI system!");
            LaunchFullUISystem();
        }
        
        // ESC key to quit
        if (InputHelper.GetKeyDown(KeyCode.Escape))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    void OnGUI()
    {
        // Create a styled GUI box
        GUI.Box(new Rect(10, 10, 450, 120), "");
        
        // Title
        GUI.Label(new Rect(20, 20, 430, 30), "🎮 MINIPOLL V5 - Simple UI Test");
        
        // Instructions
        GUI.Label(new Rect(20, 50, 430, 80), 
            "🧪 Controls:\n" +
            "SPACE - Launch Full UI System\n" +
            "F2 - Alternative Launch\n" +
            "ESC - Quit Test\n" +
            "👆 Or click the button above");
        
        // Status indicator
        if (GUI.Button(new Rect(20, 100, 200, 25), "🚀 Launch Full UI Now!"))
        {
            Debug.Log("🎮 GUI Button clicked - launching full UI system!");
            LaunchFullUISystem();
        }
    }
}
