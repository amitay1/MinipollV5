using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the splash screen sequence from HeartCode Studios to Minipoll logo to main menu
/// </summary>
public class SplashScreenManager : MonoBehaviour
{
    [Header("Splash Panels")]
    [SerializeField] private GameObject studioSplashPanel;
    [SerializeField] private GameObject gameLogoPanel;
    [SerializeField] private GameObject loadingPanel;
    
    [Header("Studio Splash Elements")]
    [SerializeField] private TextMeshProUGUI studioLogoText;
    [SerializeField] private TextMeshProUGUI studioTaglineText;
    
    [Header("Game Logo Elements")]
    [SerializeField] private TextMeshProUGUI gameLogoText;
    [SerializeField] private Image gameLogoIcon;
    
    [Header("Loading Elements")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    
    [Header("Timing Settings")]
    [SerializeField] private float studioSplashDuration = 2.5f;
    [SerializeField] private float gameLogoDuration = 3.0f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Next Scene")]
    [SerializeField] private string mainMenuSceneName = "01_SplashMenu";
    
    private bool skipRequested = false;
    
    private void Start()
    {
        // Apply TASK001 branding to text elements
        ApplyBrandingToUI();
        
        // Start the splash sequence
        StartCoroutine(SplashSequence());
    }
    
    private void Update()
    {
        // Allow skipping with any key press or mouse click
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            skipRequested = true;
        }
    }
    
    private void ApplyBrandingToUI()
    {
        // Apply branding if Brand Manager is available
        if (MinipollBrandManager.Instance != null)
        {
            // Studio splash styling
            if (studioLogoText != null)
                MinipollBrandManager.StyleText(studioLogoText, TypographyStyle.H1, Color.white);
            
            if (studioTaglineText != null)
                MinipollBrandManager.StyleText(studioTaglineText, TypographyStyle.Body, new Color(1, 1, 1, 0.8f));
            
            // Game logo styling
            if (gameLogoText != null)
            {
                MinipollBrandManager.StyleText(gameLogoText, TypographyStyle.Logo, Color.white);
                gameLogoText.fontSize = 72; // Extra large for impact
            }
            
            // Loading text styling
            if (loadingText != null)
                MinipollBrandManager.StyleText(loadingText, TypographyStyle.Body, Color.white);
        }
    }
    
    private IEnumerator SplashSequence()
    {
        // Ensure only studio splash is visible at start
        SetPanelVisibility(studioSplashPanel, true);
        SetPanelVisibility(gameLogoPanel, false);
        SetPanelVisibility(loadingPanel, false);
        
        // Studio splash sequence
        yield return StartCoroutine(ShowStudioSplash());
        
        if (skipRequested)
        {
            LoadMainMenu();
            yield break;
        }
        
        // Game logo sequence
        yield return StartCoroutine(ShowGameLogo());
        
        if (skipRequested)
        {
            LoadMainMenu();
            yield break;
        }
        
        // Loading sequence
        yield return StartCoroutine(ShowLoadingScreen());
        
        // Load main menu
        LoadMainMenu();
    }
    
    private IEnumerator ShowStudioSplash()
    {
        // Fade in studio splash
        yield return StartCoroutine(FadePanel(studioSplashPanel, true, fadeInDuration));
        
        // Wait for duration or skip
        float elapsed = 0f;
        while (elapsed < studioSplashDuration && !skipRequested)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Fade out studio splash
        yield return StartCoroutine(FadePanel(studioSplashPanel, false, fadeOutDuration));
    }
    
    private IEnumerator ShowGameLogo()
    {
        // Switch to game logo panel
        SetPanelVisibility(studioSplashPanel, false);
        SetPanelVisibility(gameLogoPanel, true);
        
        // Fade in game logo
        yield return StartCoroutine(FadePanel(gameLogoPanel, true, fadeInDuration));
        
        // Animate logo entrance (scale effect)
        if (gameLogoText != null)
        {
            yield return StartCoroutine(AnimateLogoEntrance());
        }
        
        // Wait for duration or skip
        float elapsed = 0f;
        while (elapsed < gameLogoDuration && !skipRequested)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Fade out game logo
        yield return StartCoroutine(FadePanel(gameLogoPanel, false, fadeOutDuration));
    }
    
    private IEnumerator ShowLoadingScreen()
    {
        // Switch to loading panel
        SetPanelVisibility(gameLogoPanel, false);
        SetPanelVisibility(loadingPanel, true);
        
        // Fade in loading screen
        yield return StartCoroutine(FadePanel(loadingPanel, true, fadeInDuration));
        
        // Simulate loading with progress bar
        yield return StartCoroutine(SimulateLoading());
    }
    
    private IEnumerator FadePanel(GameObject panel, bool fadeIn, float duration)
    {
        if (panel == null) yield break;
        
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        canvasGroup.alpha = startAlpha;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
    
    private IEnumerator AnimateLogoEntrance()
    {
        if (gameLogoText == null) yield break;
        
        Transform logoTransform = gameLogoText.transform;
        Vector3 originalScale = logoTransform.localScale;
        
        // Start small and scale up with bounce effect
        logoTransform.localScale = Vector3.zero;
        
        float duration = 0.8f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Bounce easing
            float scale = EaseOutBounce(t);
            logoTransform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        logoTransform.localScale = originalScale;
    }
    
    private IEnumerator SimulateLoading()
    {
        if (loadingBar == null || loadingText == null) yield break;
        
        string[] loadingMessages = {
            "Preparing your Minipoll companion...",
            "Loading emotional AI systems...",
            "Initializing creature environment...",
            "Ready to begin your journey!"
        };
        
        loadingBar.value = 0f;
        
        for (int i = 0; i < loadingMessages.Length; i++)
        {
            if (skipRequested) break;
            
            loadingText.text = loadingMessages[i];
            
            float targetProgress = (i + 1) / (float)loadingMessages.Length;
            float currentProgress = loadingBar.value;
            
            float stepDuration = 0.8f;
            float elapsed = 0f;
            
            while (elapsed < stepDuration && !skipRequested)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / stepDuration;
                loadingBar.value = Mathf.Lerp(currentProgress, targetProgress, t);
                yield return null;
            }
            
            yield return new WaitForSeconds(0.3f); // Brief pause between steps
        }
        
        loadingBar.value = 1f;
        loadingText.text = "Complete!";
        yield return new WaitForSeconds(0.5f);
    }
    
    private void SetPanelVisibility(GameObject panel, bool visible)
    {
        if (panel != null)
        {
            panel.SetActive(visible);
        }
    }
    
    private void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    // Bounce easing function for logo animation
    private float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }
}
