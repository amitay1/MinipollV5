using UnityEngine;
using UnityEngine.UI;

namespace MinipollGame.UI
{
    /// <summary>
    /// Quick UI setup for existing panels - just configures colors and positions
    /// </summary>
    public class QuickUIConfig : MonoBehaviour
    {
        void Start()
        {
            ConfigureUI();
        }
        
        public void ConfigureUI()
        {
            Debug.Log("🎨 Configuring Minipoll UI...");
            
            // Configure canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                Debug.Log("📱 Canvas configured");
            }
            
            // Configure panels
            ConfigurePanel("TopPanel", 0, 1, 1, 1, 0, -80, 0, 0);      // Top
            ConfigurePanel("LeftPanel", 0, 0, 0, 1, 0, 80, 300, -80);   // Left
            ConfigurePanel("RightPanel", 1, 0, 1, 1, -300, 80, 0, -80); // Right
            ConfigurePanel("BottomPanel", 0, 0, 1, 0, 300, 0, -300, 80); // Bottom
            
            Debug.Log("✅ UI Configuration complete!");
        }
        
        private void ConfigurePanel(string panelName, float anchorMinX, float anchorMinY, 
                                  float anchorMaxX, float anchorMaxY,
                                  float offsetMinX, float offsetMinY, 
                                  float offsetMaxX, float offsetMaxY)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel != null)
            {
                RectTransform rect = panel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(anchorMinX, anchorMinY);
                    rect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
                    rect.offsetMin = new Vector2(offsetMinX, offsetMinY);
                    rect.offsetMax = new Vector2(offsetMaxX, offsetMaxY);
                }
                
                Image image = panel.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark gray with transparency
                }
                
                Debug.Log($"🔧 {panelName} configured");
            }
        }
        
        [ContextMenu("Configure UI")]
        public void ConfigureUIFromMenu()
        {
            ConfigureUI();
        }
    }
}
