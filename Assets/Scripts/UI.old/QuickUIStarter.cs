using UnityEngine;

/// <summary>
/// Quick UI Starter - Add this to any GameObject in your scene to get UI working
/// This is the simplest way to test the UI system
/// </summary>
public class QuickUIStarter : MonoBehaviour
{
    [Header("🎮 Quick UI Options")]
    [SerializeField] private bool startWithSimpleTest = true;
    [SerializeField] private bool startWithFullUI = false;
    [SerializeField] private bool showDebugInfo = true;
    
    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("🎮 QuickUIStarter: Starting UI system...");
        }
        
        if (startWithSimpleTest)
        {
            CreateSimpleTest();
        }
        else if (startWithFullUI)
        {
            CreateFullUI();
        }
    }
    
    void CreateSimpleTest()
    {
        Debug.Log("🧪 Creating Simple UI Test...");
        
        GameObject testObj = new GameObject("SimpleUITest");
        testObj.AddComponent<SimpleUITest>();
        
        if (showDebugInfo)
        {
            Debug.Log("✅ Simple UI Test created! You should see a blue background with text.");
        }
    }
    
    void CreateFullUI()
    {
        Debug.Log("🚀 Creating Full UI System...");
        
        GameObject uiSystemObj = new GameObject("ComprehensiveUISystem");
        uiSystemObj.AddComponent<MinipollGame.UI.ComprehensiveUISystem>();
        
        if (showDebugInfo)
        {
            Debug.Log("✨ Full UI System created!");
        }
    }
    
    void Update()
    {
        // SPACE key to create full UI
        if (InputHelper.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🎮 SPACE pressed - creating full UI!");
            CreateFullUI();
        }
        
        // Quick hotkeys
        if (InputHelper.GetKeyDown(KeyCode.F1))
        {
            CreateSimpleTest();
        }
        
        if (InputHelper.GetKeyDown(KeyCode.F2))
        {
            CreateFullUI();
        }
    }
    
    void OnGUI()
    {
        if (showDebugInfo)
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 100, 300, 100));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("🎮 Quick UI Starter");
            GUILayout.Label("F1 - Simple Test");
            GUILayout.Label("F2 - Full UI System");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
