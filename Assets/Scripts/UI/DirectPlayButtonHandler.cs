using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Direct Play Button Handler - attaches directly to the Play button
/// This bypasses the MenuButtonsAnimationController dependency
/// </summary>
[RequireComponent(typeof(Button))]
public class DirectPlayButtonHandler : MonoBehaviour
{
    [Header("Settings")]
    public string gameSceneName = "02_GameScene";
    public bool showDebugMessages = true;
    
    private Button button;
    private Animation animationComponent;
    
    void Start()
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎮 DirectPlayButtonHandler: Setting up button {gameObject.name}");
        }
        
        // Get the button component
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("❌ No Button component found!");
            return;
        }
        
        // Get animation component if available
        animationComponent = GetComponent<Animation>();
        if (animationComponent != null && animationComponent.clip != null)
        {
            if (showDebugMessages)
            {
                Debug.Log($"🎬 Animation found: {animationComponent.clip.name}");
            }
        }
        
        // Setup click event
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnPlayButtonClicked);
        
        if (showDebugMessages)
        {
            Debug.Log($"🎮 Direct play button setup complete! Ready for clicks.");
        }
    }
    
    void OnPlayButtonClicked()
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎉 PLAY BUTTON CLICKED! Input system is working! Button: {gameObject.name}");
        }
        
        // Disable button to prevent double clicks
        if (button != null)
        {
            button.interactable = false;
        }
        
        // Play animation if available
        if (animationComponent != null && animationComponent.clip != null)
        {
            if (showDebugMessages)
            {
                Debug.Log($"🎬 Playing animation: {animationComponent.clip.name}");
            }
            
            animationComponent.Play();
            
            // Wait for animation to complete, then load scene
            Invoke("LoadGameScene", animationComponent.clip.length);
        }
        else
        {
            if (showDebugMessages)
            {
                Debug.Log("⚠️ No animation - loading scene immediately");
            }
            
            // No animation, load immediately
            LoadGameScene();
        }
    }
    
    void LoadGameScene()
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎮 Loading game scene: {gameSceneName}");
        }
        
        // Try to use the entrance manager for smooth transition
        SimpleEntranceManager entranceManager = FindFirstObjectByType<SimpleEntranceManager>();
        if (entranceManager != null)
        {
            StartCoroutine(entranceManager.FadeOutVideoAndTransition(gameSceneName));
        }
        else
        {
            // Fallback: direct scene load
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
}
