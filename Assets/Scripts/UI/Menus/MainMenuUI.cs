using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MinipollGame.Managers;

namespace MinipollGame.UI.Menus
{
    /// <summary>
    /// Main menu UI controller
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        public Button newGameButton;
        public Button loadGameButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;
        
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject loadGamePanel;
        public GameObject creditsPanel;
        
        [Header("Title")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI versionText;
        
        [Header("Animation")]
        public CanvasGroup mainCanvasGroup;
        public float fadeInDuration = 1f;
        
        private GameManager gameManager;
        private UIManager uiManager;
        private bool isInitialized = false;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            uiManager = FindObjectOfType<UIManager>();
            
            InitializeUI();
        }

        private void Start()
        {
            if (!isInitialized)
                InitializeUI();
                
            ShowMainMenu();
            
            // Fade in the main menu
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = 0f;
                // TODO: Add fade animation
                mainCanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Initialize UI components and button listeners
        /// </summary>
        private void InitializeUI()
        {
            if (isInitialized) return;
            
            // Set up title and version
            if (titleText != null)
                titleText.text = "MINIPOLL V5";
                
            if (versionText != null)
                versionText.text = $"Version {Application.version}";
            
            // Set up button listeners
            SetupButtons();
            
            // Initialize panels
            ShowMainMenu();
            
            isInitialized = true;
        }

        /// <summary>
        /// Setup button click listeners
        /// </summary>
        private void SetupButtons()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(StartNewGame);
                
            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(ShowLoadGame);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ShowSettings);
                
            if (creditsButton != null)
                creditsButton.onClick.AddListener(ShowCredits);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartNewGame()
        {
            if (gameManager != null)
            {
                // TODO: Implement StartNewGame in GameManager
                // gameManager.StartNewGame();
            }
            else
            {
                Debug.LogWarning("GameManager not found! Loading scene directly.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MinipollGame");
            }
        }

        /// <summary>
        /// Show load game panel
        /// </summary>
        public void ShowLoadGame()
        {
            ShowPanel(loadGamePanel);
        }

        /// <summary>
        /// Show settings menu
        /// </summary>
        public void ShowSettings()
        {
            if (uiManager != null)
            {
                // TODO: Implement ShowSettingsMenu in UIManager
                // uiManager.ShowSettingsMenu();
            }
            else
            {
                Debug.LogWarning("UIManager not found! Cannot show settings.");
            }
        }

        /// <summary>
        /// Show credits panel
        /// </summary>
        public void ShowCredits()
        {
            ShowPanel(creditsPanel);
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Show main menu panel
        /// </summary>
        public void ShowMainMenu()
        {
            ShowPanel(mainMenuPanel);
        }

        /// <summary>
        /// Show a specific panel and hide others
        /// </summary>
        private void ShowPanel(GameObject panelToShow)
        {
            // Hide all panels first
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (loadGamePanel != null) loadGamePanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            
            // Show the requested panel
            if (panelToShow != null)
                panelToShow.SetActive(true);
        }

        /// <summary>
        /// Load a saved game
        /// </summary>
        public void LoadGame(string saveFileName)
        {
            if (gameManager != null)
            {
                // TODO: Implement LoadGame in GameManager  
                // gameManager.LoadGame(saveFileName);
            }
            else
            {
                Debug.LogWarning("GameManager not found! Cannot load game.");
            }
        }

        /// <summary>
        /// Back button functionality for sub-panels
        /// </summary>
        public void GoBack()
        {
            ShowMainMenu();
        }

        /// <summary>
        /// Enable or disable the main menu
        /// </summary>
        public void SetMenuActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// Update button states based on game state
        /// </summary>
        public void UpdateButtonStates()
        {
            // Check if there are any save files
            // TODO: Implement HasSaveFiles in GameManager
            bool hasSaveFiles = false; // gameManager != null && gameManager.HasSaveFiles();
            
            if (loadGameButton != null)
                loadGameButton.interactable = hasSaveFiles;
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (newGameButton != null) newGameButton.onClick.RemoveAllListeners();
            if (loadGameButton != null) loadGameButton.onClick.RemoveAllListeners();
            if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
            if (creditsButton != null) creditsButton.onClick.RemoveAllListeners();
            if (quitButton != null) quitButton.onClick.RemoveAllListeners();
        }
    }
}
