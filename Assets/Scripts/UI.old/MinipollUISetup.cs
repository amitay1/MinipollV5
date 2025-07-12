using UnityEngine;
using UnityEngine.UI;

namespace MinipollGame.UI
{
    /// <summary>
    /// Simple UI setup script for Minipoll game panels
    /// This script will position and configure the main UI panels
    /// </summary>
    public class MinipollUISetup : MonoBehaviour
    {
        [Header("UI Configuration")]
        public GameObject topPanel;
        public GameObject leftPanel;
        public GameObject rightPanel;
        public GameObject bottomPanel;
        
        [Header("Panel Colors")]
        public Color panelColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        public Color accentColor = new Color(0.3f, 0.7f, 1f, 1f);
        
        void Start()
        {
            SetupUI();
        }
        
        public void SetupUI()
        {
            Debug.Log("🎨 Setting up Minipoll UI panels...");
            
            // Find panels automatically if not assigned
            if (topPanel == null) topPanel = GameObject.Find("TopPanel");
            if (leftPanel == null) leftPanel = GameObject.Find("LeftPanel");
            if (rightPanel == null) rightPanel = GameObject.Find("RightPanel");
            if (bottomPanel == null) bottomPanel = GameObject.Find("BottomPanel");
            
            // Setup each panel
            if (topPanel != null) SetupTopPanel();
            if (leftPanel != null) SetupLeftPanel();
            if (rightPanel != null) SetupRightPanel();
            if (bottomPanel != null) SetupBottomPanel();
            
            Debug.Log("✅ UI Setup complete!");
        }
        
        private void SetupTopPanel()
        {
            RectTransform rect = topPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Position at top of screen
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = new Vector2(0, -80);
                rect.offsetMax = new Vector2(0, 0);
                
                // Set background color
                Image image = topPanel.GetComponent<Image>();
                if (image != null)
                {
                    image.color = panelColor;
                }
                
                // Add title text
                AddTitleText(topPanel, "🐧 MINIPOLL SIMULATION V5");
                
                Debug.Log("⬆️ Top Panel configured");
            }
        }
        
        private void SetupLeftPanel()
        {
            RectTransform rect = leftPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Position at left side
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 1);
                rect.offsetMin = new Vector2(0, 80);
                rect.offsetMax = new Vector2(300, -80);
                
                // Set background color
                Image image = leftPanel.GetComponent<Image>();
                if (image != null)
                {
                    image.color = panelColor;
                }
                
                // Add population info
                AddInfoText(leftPanel, "📊 POPULATION STATUS", "Population: 1 Minipoll");
                
                Debug.Log("⬅️ Left Panel configured");
            }
        }
        
        private void SetupRightPanel()
        {
            RectTransform rect = rightPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Position at right side
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = new Vector2(-300, 80);
                rect.offsetMax = new Vector2(0, -80);
                
                // Set background color
                Image image = rightPanel.GetComponent<Image>();
                if (image != null)
                {
                    image.color = panelColor;
                }
                
                // Add selected minipoll info
                AddInfoText(rightPanel, "🎯 SELECTED MINIPOLL", "Name: Bubbleina");
                
                Debug.Log("➡️ Right Panel configured");
            }
        }
        
        private void SetupBottomPanel()
        {
            RectTransform rect = bottomPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Position at bottom
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.offsetMin = new Vector2(300, 0);
                rect.offsetMax = new Vector2(-300, 80);
                
                // Set background color
                Image image = bottomPanel.GetComponent<Image>();
                if (image != null)
                {
                    image.color = panelColor;
                }
                
                // Add system status
                AddInfoText(bottomPanel, "⚙️ SYSTEM STATUS", "All Systems: ✅ Active");
                
                Debug.Log("⬇️ Bottom Panel configured");
            }
        }
        
        private void AddTitleText(GameObject parent, string text)
        {
            GameObject textObj = new GameObject("TitleText");
            textObj.transform.SetParent(parent.transform, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 20;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.fontStyle = FontStyle.Bold;
        }
        
        private void AddInfoText(GameObject parent, string title, string info)
        {
            // Create vertical layout
            GameObject container = new GameObject("InfoContainer");
            container.transform.SetParent(parent.transform, false);
            
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.offsetMin = new Vector2(10, -100);
            containerRect.offsetMax = new Vector2(-10, -10);
            
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            
            // Add title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(container.transform, false);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 14;
            titleText.color = accentColor;
            titleText.fontStyle = FontStyle.Bold;
            
            // Add info
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(container.transform, false);
            
            Text infoText = infoObj.AddComponent<Text>();
            infoText.text = info;
            infoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            infoText.fontSize = 12;
            infoText.color = Color.white;
        }
        
        [ContextMenu("Setup UI Now")]
        public void SetupUIFromEditor()
        {
            SetupUI();
        }
        
        [ContextMenu("Reset Panel Positions")]
        public void ResetPanelPositions()
        {
            SetupUI();
        }
    }
}
