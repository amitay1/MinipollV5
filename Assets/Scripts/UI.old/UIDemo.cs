using UnityEngine;
using MinipollGame.UI;

namespace MinipollGame.Demo
{
    /// <summary>
    /// Demonstration script that shows off the comprehensive UI system
    /// Creates a complete demo scenario with all UI features
    /// </summary>
    public class UIDemo : MonoBehaviour
    {
        [Header("🎬 Demo Configuration")]
        [SerializeField] private bool autoStartDemo = true;
        [SerializeField] private float demoPauseTime = 3f;
        
        private GameUIManager uiManager;
        private int demoStep = 0;
        private float demoTimer = 0f;
        
        void Start()
        {
            if (autoStartDemo)
            {
                StartDemo();
            }
        }
        
        public void StartDemo()
        {
            Debug.Log("🎬 Starting UI Demo...");
            
            // Get or create UI manager
            uiManager = GameUIManager.Instance;
            if (uiManager == null)
            {
                GameObject uiManagerObj = new GameObject("GameUIManager");
                uiManager = uiManagerObj.AddComponent<GameUIManager>();
            }
            
            // Start demo sequence
            demoStep = 0;
            demoTimer = 0f;
            
            // Show welcome message
            uiManager.ShowNotification("🎮 Welcome to the UI Demo!", NotificationType.Success);
        }
        
        void Update()
        {
            if (autoStartDemo)
            {
                RunDemoSequence();
            }
            
            // Manual demo controls
            HandleDemoInput();
        }
        
        private void RunDemoSequence()
        {
            demoTimer += Time.deltaTime;
            
            if (demoTimer >= demoPauseTime)
            {
                ExecuteDemoStep();
                demoTimer = 0f;
                demoStep++;
                
                // Reset demo after showing all features
                if (demoStep > 15)
                {
                    demoStep = 0;
                    demoTimer = -demoPauseTime; // Extra pause before restart
                }
            }
        }
        
        private void ExecuteDemoStep()
        {
            if (uiManager == null) return;
            
            switch (demoStep)
            {
                case 0:
                    uiManager.ShowPanel("MainMenu");
                    uiManager.ShowNotification("📱 Main Menu - Your game's front door", NotificationType.Info);
                    break;
                    
                case 1:
                    uiManager.ShowNotification("🎮 Click PLAY GAME to start your adventure!", NotificationType.Info);
                    break;
                    
                case 2:
                    uiManager.ShowPanel("GameHUD");
                    uiManager.ShowNotification("🎯 Game HUD - Real-time game interface", NotificationType.Success);
                    break;
                    
                case 3:
                    uiManager.ShowNotification("📊 Track resources, population, and game speed", NotificationType.Info);
                    break;
                    
                case 4:
                    uiManager.ShowPanel("PauseMenu");
                    uiManager.ShowNotification("⏸️ Pause Menu - Game controls at your fingertips", NotificationType.Warning);
                    break;
                    
                case 5:
                    uiManager.ShowNotification("💾 Save, resume, or return to main menu", NotificationType.Info);
                    break;
                    
                case 6:
                    uiManager.ShowPanel("Settings");
                    uiManager.ShowNotification("⚙️ Settings - Customize your experience", NotificationType.Info);
                    break;
                    
                case 7:
                    uiManager.ShowNotification("🎨 Graphics, audio, and gameplay options", NotificationType.Info);
                    break;
                    
                case 8:
                    uiManager.ShowPanel("Achievements");
                    uiManager.ShowNotification("🏆 Achievements - Track your progress", NotificationType.Success);
                    break;
                    
                case 9:
                    uiManager.ShowNotification("🌟 Unlock rewards and show off your skills", NotificationType.Info);
                    break;
                    
                case 10:
                    uiManager.ShowPanel("Inventory");
                    uiManager.ShowNotification("🎒 Inventory - Manage your items", NotificationType.Info);
                    break;
                    
                case 11:
                    uiManager.ShowNotification("📦 Organize resources and equipment", NotificationType.Info);
                    break;
                    
                case 12:
                    uiManager.ShowNotification("✅ Success notification example", NotificationType.Success);
                    break;
                    
                case 13:
                    uiManager.ShowNotification("⚠️ Warning notification example", NotificationType.Warning);
                    break;
                    
                case 14:
                    uiManager.ShowNotification("❌ Error notification example", NotificationType.Error);
                    break;
                    
                case 15:
                    uiManager.ShowPanel("MainMenu");
                    uiManager.ShowNotification("🎉 Demo complete! Professional UI ready for your game!", NotificationType.Success);
                    break;
            }
        }
        
        private void HandleDemoInput()
        {
            // Manual demo controls
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ExecuteDemoStep();
                demoStep++;
                demoTimer = 0f;
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartDemo();
            }
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                autoStartDemo = !autoStartDemo;
                uiManager?.ShowNotification($"Auto demo: {(autoStartDemo ? "ON" : "OFF")}", NotificationType.Info);
            }
        }
        
        void OnGUI()
        {
            // Demo controls
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 300, 120));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("🎬 UI Demo Controls:");
            GUILayout.Label("SPACE - Next demo step");
            GUILayout.Label("R - Restart demo");
            GUILayout.Label("P - Toggle auto demo");
            GUILayout.Label($"Auto Demo: {(autoStartDemo ? "ON" : "OFF")}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
            
            // Title
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 24;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(0, 50, Screen.width, 30), "🎨 MinipollV5 Comprehensive UI System Demo", titleStyle);
        }
    }
}
