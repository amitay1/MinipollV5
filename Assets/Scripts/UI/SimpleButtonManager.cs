using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple Button Manager - Enhanced with custom animations
/// Always active, finds and setups buttons with beautiful click animations
/// This will test if the input system is working with visual feedback
/// </summary>
public class SimpleButtonManager : MonoBehaviour
{
    [Header("Settings")]
    public string gameSceneName = "02_GameScene";
    
    [Header("Animation Settings")]
    [Range(0.05f, 0.3f)]
    public float pressDownDuration = 0.1f;
    [Range(0.1f, 0.5f)]
    public float bounceBackDuration = 0.3f;
    [Range(0.1f, 0.4f)]
    public float settleDuration = 0.2f;
    
    [Range(0.8f, 0.98f)]
    public float pressedScale = 0.9f;
    [Range(1.02f, 1.2f)]
    public float bounceScale = 1.1f;
    
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
                    OnSettingsButtonClicked(settingsButton);
                });
                
                Debug.Log("⚙️ Settings button click handler setup complete!");
            }
        }
        
        Debug.Log("🧪 Button setup complete! Try clicking the buttons now.");
    }
    
    void OnPlayButtonClicked(Button button)
    {
        Debug.Log("🎮 Play button clicked! Starting custom animation...");
        Debug.Log($"🎯 Button name: {button.gameObject.name}");
        
        // Disable button to prevent double clicks
        button.interactable = false;
        
        // Always use our custom animation - much more reliable!
        Debug.Log("� Playing custom button click animation");
        CreateEnhancedButtonAnimation(button);
    }
    
    void OnSettingsButtonClicked(Button button)
    {
        Debug.Log("⚙️ Settings button clicked! Playing animation...");
        
        // Disable button to prevent double clicks
        button.interactable = false;
        
        // Play animation but don't load scene - just re-enable button
        StartCoroutine(SettingsButtonAnimation(button));
    }
    
    System.Collections.IEnumerator SettingsButtonAnimation(Button button)
    {
        Transform buttonTransform = button.transform;
        Image buttonImage = button.GetComponent<Image>();
        
        Vector3 originalScale = buttonTransform.localScale;
        Color originalColor = buttonImage != null ? buttonImage.color : Color.white;
        
        // Quick press and release animation (shorter than play button)
        float duration = 0.15f;
        Vector3 pressedScale = originalScale * 0.95f;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Simple press down and up
            float scaleMultiplier = progress < 0.5f ? 
                Mathf.Lerp(1f, 0.95f, progress * 2f) : 
                Mathf.Lerp(0.95f, 1f, (progress - 0.5f) * 2f);
                
            buttonTransform.localScale = originalScale * scaleMultiplier;
            
            yield return null;
        }
        
        // Restore original values
        buttonTransform.localScale = originalScale;
        
        // Re-enable the button
        button.interactable = true;
        
        Debug.Log("⚙️ Settings animation complete! (Settings functionality not implemented yet)");
    }
    
    void CreateEnhancedButtonAnimation(Button button)
    {
        Debug.Log($"� Creating enhanced click animation for button: {button.name}");
        
        // Start the enhanced animation sequence
        StartCoroutine(EnhancedButtonClickAnimation(button.transform, button.GetComponent<Image>()));
    }
    
    System.Collections.IEnumerator EnhancedButtonClickAnimation(Transform buttonTransform, Image buttonImage)
    {
        Vector3 originalScale = buttonTransform.localScale;
        Color originalColor = buttonImage != null ? buttonImage.color : Color.white;
        
        // Phase 1: Quick press down (configurable duration)
        Debug.Log("🎬 Phase 1: Press down effect");
        Vector3 pressedScaleVec = originalScale * pressedScale;
        Color pressedColor = originalColor * 0.8f; // Darker when pressed
        
        float elapsed = 0f;
        while (elapsed < pressDownDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / pressDownDuration;
            
            // Smooth scaling with easing
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            buttonTransform.localScale = Vector3.Lerp(originalScale, pressedScaleVec, easedProgress);
            
            if (buttonImage != null)
            {
                buttonImage.color = Color.Lerp(originalColor, pressedColor, easedProgress);
            }
            
            yield return null;
        }
        
        // Phase 2: Pop back up with bounce (configurable duration)
        Debug.Log("🎬 Phase 2: Bounce back effect");
        Vector3 bounceScaleVec = originalScale * bounceScale;
        Color brightColor = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * 1.2f);
        
        elapsed = 0f;
        while (elapsed < bounceBackDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / bounceBackDuration;
            
            // Bounce effect using sine wave
            float bounceProgress = Mathf.Sin(progress * Mathf.PI);
            Vector3 currentScale = Vector3.Lerp(pressedScaleVec, bounceScaleVec, bounceProgress);
            buttonTransform.localScale = currentScale;
            
            if (buttonImage != null)
            {
                Color currentColor = Color.Lerp(pressedColor, brightColor, bounceProgress);
                buttonImage.color = currentColor;
            }
            
            yield return null;
        }
        
        // Phase 3: Settle back to normal (configurable duration)
        Debug.Log("🎬 Phase 3: Settle to normal");
        
        elapsed = 0f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / settleDuration;
            
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            buttonTransform.localScale = Vector3.Lerp(bounceScaleVec, originalScale, easedProgress);
            
            if (buttonImage != null)
            {
                buttonImage.color = Color.Lerp(brightColor, originalColor, easedProgress);
            }
            
            yield return null;
        }
        
        // Ensure we end exactly at original values
        buttonTransform.localScale = originalScale;
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
        
        Debug.Log("🎉 Button animation complete! Loading scene...");
        
        // Wait a tiny bit more for visual effect, then load scene
        yield return new WaitForSeconds(0.1f);
        LoadGameScene();
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
