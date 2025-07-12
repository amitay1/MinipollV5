using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace MinipollGame.UI
{
    /// <summary>
    /// Basic UI System without DOTween - Fallback version if DOTween isn't available
    /// Simple, functional UI system that works with standard Unity components
    /// </summary>
    public class BasicUISystem : MonoBehaviour
    {
        [Header("🎨 UI Configuration")]
        [SerializeField] private bool createMainMenu = true;
        [SerializeField] private bool showDebugInfo = true;
        
        // UI References
        private Canvas mainCanvas;
        private GameObject mainMenuPanel;
        private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();
        
        void Start()
        {
            InitializeBasicUI();
        }
        
        private void InitializeBasicUI()
        {
            if (showDebugInfo)
            {
                Debug.Log("🎨 BasicUISystem: Initializing...");
            }
            
            CreateMainCanvas();
            
            if (createMainMenu)
            {
                CreateMainMenu();
            }
            
            if (showDebugInfo)
            {
                Debug.Log("✅ BasicUISystem: Initialization complete!");
            }
        }
        
        private void CreateMainCanvas()
        {
            GameObject canvasObj = new GameObject("BasicUI_Canvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 1000;
            
            // Canvas Scaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            if (showDebugInfo)
            {
                Debug.Log("📱 Main Canvas created successfully");
            }
        }
        
        private void CreateMainMenu()
        {
            mainMenuPanel = CreatePanel("MainMenu");
            
            // Background
            GameObject background = CreateUIElement("Background", mainMenuPanel.transform);
            SetRectTransform(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.2f, 0.4f, 0.95f); // Dark blue background
            
            // Title
            GameObject titleObj = CreateUIElement("Title", mainMenuPanel.transform);
            SetRectTransform(titleObj, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(-300, -50), new Vector2(300, 50));
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "🐧 MINIPOLL V5";
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            // Subtitle
            GameObject subtitleObj = CreateUIElement("Subtitle", mainMenuPanel.transform);
            SetRectTransform(subtitleObj, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(-300, -25), new Vector2(300, 25));
            
            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "Life Simulation Adventure";
            subtitleText.fontSize = 24;
            subtitleText.color = new Color(1f, 0.8f, 0.8f, 1f);
            subtitleText.alignment = TextAlignmentOptions.Center;
            
            // Buttons
            CreateMenuButton(mainMenuPanel, "🎮 PLAY GAME", new Vector2(0.5f, 0.5f), StartGame);
            CreateMenuButton(mainMenuPanel, "🏆 ACHIEVEMENTS", new Vector2(0.5f, 0.4f), ShowAchievements);
            CreateMenuButton(mainMenuPanel, "⚙️ SETTINGS", new Vector2(0.5f, 0.3f), ShowSettings);
            CreateMenuButton(mainMenuPanel, "🚪 EXIT", new Vector2(0.5f, 0.2f), QuitGame);
            
            // Instructions
            GameObject instructionObj = CreateUIElement("Instructions", mainMenuPanel.transform);
            SetRectTransform(instructionObj, new Vector2(0.5f, 0.1f), new Vector2(0.5f, 0.1f), new Vector2(-300, -30), new Vector2(300, 30));
            
            TextMeshProUGUI instructionText = instructionObj.AddComponent<TextMeshProUGUI>();
            instructionText.text = "Press ESC to pause • Use mouse to click buttons";
            instructionText.fontSize = 16;
            instructionText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            instructionText.alignment = TextAlignmentOptions.Center;
            
            uiPanels.Add("MainMenu", mainMenuPanel);
            
            if (showDebugInfo)
            {
                Debug.Log("🏠 Main Menu created successfully");
            }
        }
        
        private void CreateMenuButton(GameObject parent, string text, Vector2 position, System.Action onClick)
        {
            GameObject buttonObj = CreateUIElement($"Button_{text.Replace(" ", "")}", parent.transform);
            SetRectTransform(buttonObj, position, position, new Vector2(-150, -25), new Vector2(150, 25));
            
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green button
            
            // Button text
            GameObject textObj = CreateUIElement("Text", buttonObj.transform);
            SetRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
            
            // Click event
            button.onClick.AddListener(() => {
                if (showDebugInfo)
                {
                    Debug.Log($"🎯 Button clicked: {text}");
                }
                onClick?.Invoke();
            });
            
            // Simple hover effect
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
            colors.pressedColor = new Color(0.1f, 0.4f, 0.1f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            button.colors = colors;
        }
        
        private GameObject CreatePanel(string name)
        {
            GameObject panel = CreateUIElement(name, mainCanvas.transform);
            SetRectTransform(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            return panel;
        }
        
        private GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }
        
        private void SetRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
        
        // Button Actions
        private void StartGame()
        {
            if (showDebugInfo)
            {
                Debug.Log("🎮 Starting Game...");
            }
            
            // Create simple game message
            ShowMessage("🎮 Game Started! Welcome to Minipoll V5!");
        }
        
        private void ShowAchievements()
        {
            if (showDebugInfo)
            {
                Debug.Log("🏆 Showing Achievements...");
            }
            
            ShowMessage("🏆 Achievements system coming soon!");
        }
        
        private void ShowSettings()
        {
            if (showDebugInfo)
            {
                Debug.Log("⚙️ Showing Settings...");
            }
            
            ShowMessage("⚙️ Settings panel coming soon!");
        }
        
        private void QuitGame()
        {
            if (showDebugInfo)
            {
                Debug.Log("🚪 Quitting Game...");
            }
            
            ShowMessage("👋 Thanks for playing Minipoll V5!");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        private void ShowMessage(string message)
        {
            StartCoroutine(ShowMessageCoroutine(message));
        }
        
        private IEnumerator ShowMessageCoroutine(string message)
        {
            // Create temporary message panel
            GameObject messagePanel = CreateUIElement("MessagePanel", mainCanvas.transform);
            SetRectTransform(messagePanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            // Semi-transparent background
            Image bgImage = messagePanel.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);
            
            // Message text
            GameObject textObj = CreateUIElement("MessageText", messagePanel.transform);
            SetRectTransform(textObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300, -50), new Vector2(300, 50));
            
            TextMeshProUGUI messageText = textObj.AddComponent<TextMeshProUGUI>();
            messageText.text = message;
            messageText.fontSize = 24;
            messageText.color = Color.white;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.fontStyle = FontStyles.Bold;
            
            // Wait and then destroy
            yield return new WaitForSeconds(2f);
            Destroy(messagePanel);
        }
        
        void Update()
        {
            // ESC key handling
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (showDebugInfo)
                {
                    Debug.Log("🔙 ESC pressed");
                }
                
                ShowMessage("⏸️ ESC pressed - Game paused");
            }
        }
    }
}
