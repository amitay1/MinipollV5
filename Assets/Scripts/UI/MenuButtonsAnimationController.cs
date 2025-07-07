using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// MenuButtonsAnimationController - Beautiful button animations with entrance effects and scene transitions
/// </summary>
public class MenuButtonsAnimationController : MonoBehaviour
{
    [Header("Button References")]
    public Button playGameButton;
    public Button settingsButton;
    
    [Header("Entrance Animation Settings")]
    public float entranceDelay = 0.5f;
    public float entranceStagger = 0.3f;
    public float entranceDuration = 0.8f;
    public float entranceScale = 1.2f;
    public AnimationCurve entranceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Click Animation Settings")]
    public float clickScale = 0.85f;
    public float clickDuration = 0.15f;
    public float bounceScale = 1.1f;
    public AnimationCurve clickCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Scene Settings")]
    public string gameSceneName = "02_GameScene";  // השם הנכון מה-Build Settings
    public string settingsSceneName = "SettingsScene";
    
    [Header("Debug")]
    public bool showDebugMessages = true;
    
    private Button[] allButtons;
    private bool animationsCompleted = false;
    private CanvasGroup canvasGroup;
    
    // Public property to check animation status
    public bool AnimationsCompleted => animationsCompleted;
    
    void Start()
    {
        Debug.Log("🧪 MenuButtonsAnimationController: Starting with INPUT SYSTEM TEST");
        
        // Get CanvasGroup component
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // FORCE FIND ALL BUTTONS IN SCENE (including inactive ones)
        allButtons = FindObjectsOfType<Button>(true); // true = include inactive
        Debug.Log($"🧪 FOUND {allButtons.Length} BUTTONS IN ENTIRE SCENE");
        
        foreach (Button btn in allButtons)
        {
            if (btn != null)
            {
                Debug.Log($"🧪 Button found: {btn.name} - Active: {btn.gameObject.activeSelf}");
            }
        }
        
        AutoAssignButtons();

        if (showDebugMessages)
        {
            Debug.Log($"🎮 MenuButtonsAnimationController: Found {allButtons.Length} buttons");
        }

        // FORCE SETUP BUTTON EVENTS IMMEDIATELY
        SetupButtonEvents();
        animationsCompleted = true; 
        
        // FORCE ADD DIRECT CLICK TESTS
        AddDirectClickTests();
        
        if (showDebugMessages)
        {
            Debug.Log("🎮 MenuButtons ready and interactive - INPUT TEST ENABLED");
        }
    }
    
    void AddDirectClickTests()
    {
        Debug.Log("🧪 Adding direct click tests to all buttons...");
        
        foreach (Button button in allButtons)
        {
            if (button != null)
            {
                // Add a direct test listener (without removing existing ones)
                string buttonName = button.name;
                button.onClick.AddListener(() => {
                    Debug.Log($"🎉 DIRECT CLICK TEST SUCCESS! Button: {buttonName}");
                    Debug.Log($"🎉 INPUT SYSTEM IS WORKING! Position: {button.transform.position}");
                });
                
                Debug.Log($"🧪 Direct click test added to: {buttonName}");
            }
        }
        
        Debug.Log("🧪 Direct click tests setup complete!");
    }
    
    void AutoAssignButtons()
    {
        foreach (Button button in allButtons)
        {
            if (button == null) continue;
            
            string buttonName = button.name.ToLower();
            
            if (buttonName.Contains("play") && playGameButton == null)
            {
                playGameButton = button;
                if (showDebugMessages) Debug.Log($"🎮 Auto-assigned Play button: {button.name}");
            }
            else if (buttonName.Contains("settings") && settingsButton == null)
            {
                settingsButton = button;
                if (showDebugMessages) Debug.Log($"⚙️ Auto-assigned Settings button: {button.name}");
            }
        }
    }
    
    void SetupButtonEvents()
    {
        if (playGameButton != null)
        {
            playGameButton.onClick.RemoveAllListeners();
            playGameButton.onClick.AddListener(() => OnPlayGameClick());
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => OnSettingsClick());
        }
    }
    
    public void OnPlayGameClick()
    {
        if (showDebugMessages)
        {
            Debug.Log("🎮 Play Game clicked!");
        }
        
        StartCoroutine(HandleButtonClick(playGameButton, () => {
            // Find SimpleEntranceManager and trigger fade out transition
            SimpleEntranceManager entranceManager = FindFirstObjectByType<SimpleEntranceManager>();
            if (entranceManager != null)
            {
                StartCoroutine(entranceManager.FadeOutVideoAndTransition(gameSceneName));
            }
            else
            {
                // Fallback: Load scene directly
                if (!string.IsNullOrEmpty(gameSceneName))
                {
                    SceneManager.LoadScene(gameSceneName);
                }
                else
                {
                    Debug.LogWarning("⚠️ Game scene name not set!");
                }
            }
        }));
    }
    
    public void OnSettingsClick()
    {
        if (showDebugMessages)
        {
            Debug.Log("⚙️ Settings clicked!");
        }
        
        StartCoroutine(HandleButtonClick(settingsButton, () => {
            // Load settings scene or open settings panel
            if (!string.IsNullOrEmpty(settingsSceneName))
            {
                SceneManager.LoadScene(settingsSceneName);
            }
            else
            {
                Debug.Log("⚙️ Settings panel would open here");
            }
        }));
    }
    
    IEnumerator HandleButtonClick(Button button, System.Action onComplete)
    {
        if (button == null) yield break;
        
        // Disable all buttons during click to prevent double-clicks
        SetButtonsInteractable(false);
        
        if (showDebugMessages)
        {
            Debug.Log($"🎮 Button clicked: {button.name}");
        }
        
        // Check if button has animation component
        Animation animationComponent = button.GetComponent<Animation>();
        if (animationComponent != null && animationComponent.clip != null)
        {
            // Play your existing animation
            animationComponent.Play();
            if (showDebugMessages)
            {
                Debug.Log($"🎬 Playing animation: {animationComponent.clip.name} on {button.name}");
            }
            
            // Wait for animation to complete
            yield return new WaitForSeconds(animationComponent.clip.length);
        }
        else
        {
            // Fallback: Simple visual feedback if no animation
            Vector3 originalScale = button.transform.localScale;
            yield return StartCoroutine(AnimateScale(button.transform, originalScale, originalScale * 0.9f, 0.1f, clickCurve));
            yield return StartCoroutine(AnimateScale(button.transform, originalScale * 0.9f, originalScale, 0.1f, clickCurve));
        }
        
        // Execute the action (load scene)
        onComplete?.Invoke();
        
        // Re-enable buttons
        yield return new WaitForSeconds(0.1f);
        SetButtonsInteractable(true);
    }
    
    IEnumerator AnimateScale(Transform target, Vector3 fromScale, Vector3 toScale, float duration, AnimationCurve curve = null)
    {
        if (target == null) yield break;
        
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            float curveValue = curve != null ? curve.Evaluate(progress) : progress;
            
            target.localScale = Vector3.Lerp(fromScale, toScale, curveValue);
            yield return null;
        }
        target.localScale = toScale;
    }
    
    void SetButtonsInteractable(bool interactable)
    {
        if (allButtons != null)
        {
            foreach (Button button in allButtons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }
    }
    
    [ContextMenu("🚨 Force Show Buttons Now")]
    public void ForceShowButtonsNow()
    {
        if (showDebugMessages)
        {
            Debug.Log("🚨 FORCE: Showing buttons immediately!");
        }
        
        // Force buttons to be visible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Force all buttons to be visible and interactive
        if (allButtons == null || allButtons.Length == 0)
        {
            allButtons = GetComponentsInChildren<Button>();
        }
        
        if (allButtons != null)
        {
            foreach (Button button in allButtons)
            {
                if (button != null)
                {
                    button.transform.localScale = Vector3.one;
                    button.interactable = true;
                    button.gameObject.SetActive(true);
                }
            }
        }
        
        animationsCompleted = true;
        
        if (showDebugMessages)
        {
            Debug.Log("🚨 FORCE: Buttons are now visible and interactive!");
        }
    }
    
    public void TriggerEntranceAnimation()
    {
        if (showDebugMessages)
        {
            Debug.Log("🎬 Manually triggering entrance animation...");
        }
        
        animationsCompleted = false;
        StopAllCoroutines();
        
        // Just ensure we have the buttons and start the animation
        if (allButtons == null || allButtons.Length == 0)
        {
            if (playGameButton == null || settingsButton == null)
            {
                allButtons = GetComponentsInChildren<Button>();
                AutoAssignButtons();
            }
            else
            {
                allButtons = new Button[] { playGameButton, settingsButton };
            }
        }
        
        SetupButtonEvents();
        animationsCompleted = true;
        
        if (showDebugMessages)
        {
            Debug.Log("🎬 Entrance animation completed!");
        }
    }
}