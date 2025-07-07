using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple Button Manager - always active, finds and setups buttons
/// This will test if the input system is working
/// </summary>
public class SimpleButtonManager : MonoBehaviour
{
    [Header("Settings")]
    public string gameSceneName = "02_GameScene";
    
    void Start()
    {
        Debug.Log("🧪 SimpleButtonManager: Testing input system...");
        
        // Wait a frame to ensure everything is loaded
        Invoke("SetupButtons", 0.1f);
    }
    
    void SetupButtons()
    {
        Debug.Log("🧪 Looking for buttons in scene...");
        
        // Find PlayButton_Professional specifically
        GameObject playButtonObj = GameObject.Find("PlayButton_Professional");
        if (playButtonObj != null)
        {
            Button playButton = playButtonObj.GetComponent<Button>();
            if (playButton != null)
            {
                Debug.Log($"🎮 Found PlayButton_Professional! Setting up click handler...");
                
                // Clear any existing listeners
                playButton.onClick.RemoveAllListeners();
                
                // Add our test listener
                playButton.onClick.AddListener(() => {
                    Debug.Log("🎉 PLAY BUTTON CLICKED! INPUT SYSTEM WORKS!");
                    Debug.Log($"🎉 Button position: {playButton.transform.position}");
                    
                    OnPlayButtonClicked(playButton);
                });
                
                Debug.Log("🎮 Play button click handler setup complete!");
            }
            else
            {
                Debug.LogWarning("⚠️ PlayButton_Professional found but no Button component!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ PlayButton_Professional not found!");
        }
        
        // Also find SettingsButton_Professional
        GameObject settingsButtonObj = GameObject.Find("SettingsButton_Professional");
        if (settingsButtonObj != null)
        {
            Button settingsButton = settingsButtonObj.GetComponent<Button>();
            if (settingsButton != null)
            {
                Debug.Log($"⚙️ Found SettingsButton_Professional! Setting up click handler...");
                
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(() => {
                    Debug.Log("🎉 SETTINGS BUTTON CLICKED! INPUT SYSTEM WORKS!");
                });
                
                Debug.Log("⚙️ Settings button click handler setup complete!");
            }
        }
        
        Debug.Log("🧪 Button setup complete! Try clicking the buttons now.");
    }
    
    void OnPlayButtonClicked(Button button)
    {
        Debug.Log("🎮 Play button clicked! Checking for animation...");
        
        // Disable button to prevent double clicks
        button.interactable = false;
        
        // Check for animation
        Animation anim = button.GetComponent<Animation>();
        if (anim != null && anim.clip != null)
        {
            Debug.Log($"🎬 Playing animation: {anim.clip.name}");
            anim.Play();
            
            // Wait for animation then load scene
            Invoke("LoadGameScene", anim.clip.length);
        }
        else
        {
            Debug.Log("⚠️ No animation found, loading scene immediately");
            LoadGameScene();
        }
    }
    
    void LoadGameScene()
    {
        Debug.Log($"🎮 Loading game scene: {gameSceneName}");
        
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ Game scene name not set!");
        }
    }
}
