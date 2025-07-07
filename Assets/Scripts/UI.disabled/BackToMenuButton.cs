using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// DISABLED SCRIPT - Simple script to handle the Back to Menu button functionality
/// Connects to the existing GameManager system
/// Created for TASK007 - Core Game Scene Setup
/// </summary>
/*
public class BackToMenuButton : MonoBehaviour
{
    [Header("Scene Management")]
    [Tooltip("Name of the main menu scene to return to")]
    public string mainMenuSceneName = "01_SplashMenu";
    
    private Button backButton;
    
    void Start()
    {
        // Get the button component
        backButton = GetComponent<Button>();
        
        if (backButton != null)
        {
            // Clear existing listeners and add our function
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ReturnToMainMenu);
            
            Debug.Log("✅ BackToMenuButton: Configured successfully");
            Debug.Log($"   - Will return to scene: {mainMenuSceneName}");
        }
        else
        {
            Debug.LogError("❌ BackToMenuButton: No Button component found!");
        }
    }
    
    /// <summary>
    /// Returns to the main menu scene
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("🔙 BackToMenuButton: Returning to main menu...");
        Debug.Log($"   - Current scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"   - Target scene: {mainMenuSceneName}");
        
        try
        {
            // Ensure time scale is normal before scene change
            Time.timeScale = 1f;
            
            // If GameManager exists, we can use it to handle the transition
            if (GameManager.Instance != null)
            {
                Debug.Log("🎮 Using GameManager for scene transition");
                GameManager.Instance.SetGameState(GameManager.GameState.Initializing);
            }
            
            // Load the main menu scene
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log("✅ Scene change initiated successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to load main menu scene: {e.Message}");
            Debug.LogError($"   Check that '{mainMenuSceneName}' exists in Build Settings");
            
            // Fallback - try to load the first scene in build settings
            if (SceneManager.sceneCountInBuildSettings > 0)
            {
                Debug.Log("🔄 Attempting fallback to first scene in build settings");
                SceneManager.LoadScene(0);
            }
        }
    }
    
    void Update()
    {
        // Handle escape key for quick return to menu (desktop)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("⌨️ Escape key pressed - returning to main menu");
            ReturnToMainMenu();
        }
        
        // Handle Android back button
        if (Input.GetKeyDown(KeyCode.Escape) && Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("📱 Android back button pressed - returning to main menu");
            ReturnToMainMenu();
        }
    }
}
*/