using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MinipollGame.Managers;

namespace MinipollGame.UI.Menus
{
    /// <summary>
    /// Pause menu UI controller
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        public Button resumeButton;
        public Button saveGameButton;
        public Button loadGameButton;
        public Button settingsButton;
        public Button mainMenuButton;
        public Button quitButton;
        
        [Header("Panels")]
        public GameObject pauseMenuPanel;
        public GameObject confirmationPanel;
        
        [Header("Confirmation Dialog")]
        public TextMeshProUGUI confirmationText;
        public Button confirmYesButton;
        public Button confirmNoButton;
        
        [Header("Animation")]
        public CanvasGroup pauseCanvasGroup;
        public float fadeInDuration = 0.3f;
        
        private GameManager gameManager;
        private UIManager uiManager;
        private bool isPaused = false;
        private System.Action pendingConfirmationAction;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            uiManager = FindObjectOfType<UIManager>();
            
            SetupButtons();
        }

        private void Start()
        {
            // Initially hide the pause menu
            gameObject.SetActive(false);
        }

        private void Update()
        {
            // Check for pause input (ESC key)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }
        }

        /// <summary>
        /// Setup button click listeners
        /// </summary>
        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);
                
            if (saveGameButton != null)
                saveGameButton.onClick.AddListener(SaveGame);
                
            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(LoadGame);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ShowSettings);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => ShowConfirmation("Return to Main Menu?", ReturnToMainMenu));
                
            if (quitButton != null)
                quitButton.onClick.AddListener(() => ShowConfirmation("Quit Game?", QuitGame));
                
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(ConfirmAction);
                
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(CancelConfirmation);
        }

        /// <summary>
        /// Pause the game and show pause menu
        /// </summary>
        public void PauseGame()
        {
            if (isPaused) return;
            
            isPaused = true;
            Time.timeScale = 0f;
            
            gameObject.SetActive(true);
            ShowPanel(pauseMenuPanel);
            
            // Fade in animation
            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.alpha = 0f;
                // TODO: Add proper fade animation
                pauseCanvasGroup.alpha = 1f;
            }
            
            // Notify other systems about pause state
            if (gameManager != null)
                gameManager.PauseGame();
        }

        /// <summary>
        /// Resume the game and hide pause menu
        /// </summary>
        public void ResumeGame()
        {
            if (!isPaused) return;
            
            isPaused = false;
            Time.timeScale = 1f;
            
            gameObject.SetActive(false);
            
            // Notify other systems about resume state
            if (gameManager != null)
                gameManager.ResumeGame();
        }

        /// <summary>
        /// Save the current game
        /// </summary>
        public void SaveGame()
        {
            if (gameManager != null)
            {
                gameManager.SavePlayerData(); // Use available method
                ShowNotification("Game Saved!");
            }
            else
            {
                ShowNotification("Save failed - GameManager not found!");
            }
        }

        /// <summary>
        /// Load a saved game
        /// </summary>
        public void LoadGame()
        {
            if (uiManager != null)
            {
                // TODO: Implement ShowLoadGameMenu in UIManager
                // uiManager.ShowLoadGameMenu();
            }
            else
            {
                Debug.LogWarning("UIManager not found! Cannot show load game menu.");
            }
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
        /// Return to main menu with confirmation
        /// </summary>
        public void ReturnToMainMenu()
        {
            ResumeGame();
            
            if (gameManager != null)
            {
                // TODO: Implement ReturnToMainMenu in GameManager  
                // gameManager.ReturnToMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        /// <summary>
        /// Quit the game with confirmation
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
        /// Show confirmation dialog
        /// </summary>
        private void ShowConfirmation(string message, System.Action confirmAction)
        {
            pendingConfirmationAction = confirmAction;
            
            if (confirmationText != null)
                confirmationText.text = message;
            
            ShowPanel(confirmationPanel);
        }

        /// <summary>
        /// Confirm the pending action
        /// </summary>
        private void ConfirmAction()
        {
            pendingConfirmationAction?.Invoke();
            pendingConfirmationAction = null;
            ShowPanel(pauseMenuPanel);
        }

        /// <summary>
        /// Cancel the confirmation
        /// </summary>
        private void CancelConfirmation()
        {
            pendingConfirmationAction = null;
            ShowPanel(pauseMenuPanel);
        }

        /// <summary>
        /// Show a specific panel and hide others
        /// </summary>
        private void ShowPanel(GameObject panelToShow)
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (confirmationPanel != null) confirmationPanel.SetActive(false);
            
            if (panelToShow != null)
                panelToShow.SetActive(true);
        }

        /// <summary>
        /// Show a notification message
        /// </summary>
        private void ShowNotification(string message)
        {
            if (uiManager != null)
            {
                // TODO: Implement ShowNotification in UIManager
                // uiManager.ShowNotification(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        /// Get the current pause state
        /// </summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// Force set pause state (for external control)
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (paused)
                PauseGame();
            else
                ResumeGame();
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
            if (saveGameButton != null) saveGameButton.onClick.RemoveAllListeners();
            if (loadGameButton != null) loadGameButton.onClick.RemoveAllListeners();
            if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
            if (quitButton != null) quitButton.onClick.RemoveAllListeners();
            if (confirmYesButton != null) confirmYesButton.onClick.RemoveAllListeners();
            if (confirmNoButton != null) confirmNoButton.onClick.RemoveAllListeners();
        }
    }
}
