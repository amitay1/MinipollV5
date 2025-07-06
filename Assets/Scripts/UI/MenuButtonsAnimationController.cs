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
    public string gameSceneName = "GameScene";
    public string settingsSceneName = "SettingsScene";
    
    [Header("Debug")]
    public bool showDebugMessages = true;
    
    private Button[] allButtons;
    private bool animationsCompleted = false;
    private CanvasGroup canvasGroup;
    
    void Start()
    {
        // Get CanvasGroup component
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Get all buttons if specific ones aren't assigned
        if (playGameButton == null || settingsButton == null)
        {
            allButtons = GetComponentsInChildren<Button>();
            AutoAssignButtons();
        }
        else
        {
            allButtons = new Button[] { playGameButton, settingsButton };
        }
        
        if (showDebugMessages)
        {
            Debug.Log($"🎮 MenuButtonsAnimationController: Found {allButtons.Length} buttons");
        }
        
        InitializeButtons();
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
    
    void InitializeButtons()
    {
        // Hide buttons using CanvasGroup for smooth fade
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        // Also hide individual buttons as backup
        foreach (Button button in allButtons)
        {
            if (button != null)
            {
                button.transform.localScale = Vector3.zero;
                button.interactable = false;
            }
        }
        
        SetupButtonEvents();
        StartCoroutine(BeautifulEntranceSequence());
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
    
    IEnumerator BeautifulEntranceSequence()
    {
        if (showDebugMessages)
        {
            Debug.Log("✨ Starting beautiful entrance sequence...");
        }
        
        yield return new WaitForSeconds(entranceDelay);
        
        // Fade in the entire button group first
        if (canvasGroup != null)
        {
            yield return StartCoroutine(AnimateCanvasGroupAlpha(0f, 1f, entranceDuration * 0.3f));
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Then animate each button with staggered timing
        for (int i = 0; i < allButtons.Length; i++)
        {
            if (allButtons[i] != null)
            {
                StartCoroutine(AnimateButtonEntrance(allButtons[i], i * entranceStagger));
            }
        }
        
        // Wait for all animations to complete
        yield return new WaitForSeconds(entranceDuration + (allButtons.Length * entranceStagger));
        
        animationsCompleted = true;
        
        if (showDebugMessages)
        {
            Debug.Log("🎉 Entrance sequence completed! Buttons are ready.");
        }
    }
    
    IEnumerator AnimateButtonEntrance(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Enable interaction
        button.interactable = true;
        
        // Beautiful entrance animation with overshoot and rotation
        yield return StartCoroutine(AnimateScale(button.transform, Vector3.zero, Vector3.one * entranceScale, entranceDuration * 0.6f, entranceCurve));
        yield return StartCoroutine(AnimateScale(button.transform, Vector3.one * entranceScale, Vector3.one, entranceDuration * 0.4f, clickCurve));
        
        // Add a subtle rotation effect
        StartCoroutine(AnimateRotation(button.transform, Vector3.zero, new Vector3(0, 0, 10), entranceDuration * 0.3f));
        yield return new WaitForSeconds(entranceDuration * 0.1f);
        StartCoroutine(AnimateRotation(button.transform, new Vector3(0, 0, 10), Vector3.zero, entranceDuration * 0.2f));
    }
    
    public void OnPlayGameClick()
    {
        if (!animationsCompleted) return;
        
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
        if (!animationsCompleted) return;
        
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
        // Disable all buttons during animation
        SetButtonsInteractable(false);
        
        // Beautiful click animation sequence
        yield return StartCoroutine(AnimateScale(button.transform, Vector3.one, Vector3.one * clickScale, clickDuration * 0.5f, clickCurve));
        yield return StartCoroutine(AnimateScale(button.transform, Vector3.one * clickScale, Vector3.one * bounceScale, clickDuration * 0.3f, clickCurve));
        yield return StartCoroutine(AnimateScale(button.transform, Vector3.one * bounceScale, Vector3.one, clickDuration * 0.2f, clickCurve));
        
        // Execute the action
        onComplete?.Invoke();
        
        // Re-enable buttons (if we're still in this scene)
        yield return new WaitForSeconds(0.1f);
        SetButtonsInteractable(true);
    }
    
    IEnumerator AnimateCanvasGroupAlpha(float fromAlpha, float toAlpha, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
            }
            yield return null;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = toAlpha;
        }
    }
    
    IEnumerator AnimateScale(Transform target, Vector3 fromScale, Vector3 toScale, float duration, AnimationCurve curve)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            float curveValue = curve.Evaluate(progress);
            
            target.localScale = Vector3.Lerp(fromScale, toScale, curveValue);
            yield return null;
        }
        target.localScale = toScale;
    }
    
    IEnumerator AnimateRotation(Transform target, Vector3 fromRotation, Vector3 toRotation, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            
            target.localEulerAngles = Vector3.Lerp(fromRotation, toRotation, progress);
            yield return null;
        }
        target.localEulerAngles = toRotation;
    }
    
    void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in allButtons)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
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
        InitializeButtons();
    }
}
