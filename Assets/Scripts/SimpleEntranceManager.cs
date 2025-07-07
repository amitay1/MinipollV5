using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Simple and clean entrance sequence - Logo video once, then Minipoll video loop, then menu
/// </summary>
public class SimpleEntranceManager : MonoBehaviour
{
    [Header("🎬 Video Components")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    
    [Header("🎯 Video Files")]
    public VideoClip logoVideo;
    public VideoClip minipollVideo;
    
    [Header("🎨 Menu")]
    public GameObject menuButtons;
    
    [Header("⚙️ Settings")]
    public bool enableDebugLogs = true;
    public bool skipVideosForTesting = false;
    
    // Private state
    private bool isSequenceRunning = false;
      void Start()
    {
        // Auto-assign components if not set
        if (videoPlayer == null)
        {
            videoPlayer = FindFirstObjectByType<VideoPlayer>();
            if (videoPlayer != null)
            {
                Log("📹 Auto-assigned VideoPlayer component");
            }
        }

        if (videoDisplay == null)
        {
            videoDisplay = FindFirstObjectByType<RawImage>();
            if (videoDisplay != null && videoDisplay.name.Contains("VideoDisplay"))
            {
                Log("📺 Auto-assigned VideoDisplay component");
            }
        }

        if (menuButtons == null)
        {
            GameObject menuButtonsObj = GameObject.Find("MenuButtons");
            if (menuButtonsObj != null)
            {
                menuButtons = menuButtonsObj;
                Log("🎮 Auto-assigned MenuButtons GameObject");
            }
        }

        // QUICK FIX: Skip videos and show menu immediately for testing
        if (skipVideosForTesting)
        {
            Log("⏭️ Skipping entrance sequence - showing menu immediately");
            ShowMenu();
        }
        else
        {
            StartCoroutine(RunEntranceSequence());
        }
    }
    
    IEnumerator RunEntranceSequence()
    {
        if (isSequenceRunning) yield break;
        isSequenceRunning = true;
        
        Log("🎬 Starting entrance sequence...");
        
        // Step 1: Setup
        SetupVideoSystem();
        HideMenu();
        
        if (skipVideosForTesting)
        {
            Log("⏭️ Skipping videos for testing");
            ShowMenu();
            yield break;
        }
        
        // Step 2: Play Logo Video (once)
        Log("🎯 Phase 1: Logo video");
        yield return StartCoroutine(PlayVideoOnce(logoVideo));
        
        // Step 3: Play Minipoll Video (infinite loop until user clicks Play)
        Log("🎯 Phase 2: Minipoll infinite loop");
        yield return StartCoroutine(PlayMinipollInfiniteLoop());
        
        // Step 3.5: Wait 2 seconds after Minipoll video starts before showing menu
        Log("⏳ Waiting 2 seconds before showing menu...");
        yield return new WaitForSeconds(2f);
        
        // Step 4: Show Menu (show menu while video is still playing)
        Log("🎯 Phase 3: Show menu over video");
        ShowMenu();
        
        Log("✅ Entrance sequence complete! Video continues until Play is clicked.");
    }
    
    void SetupVideoSystem()
    {
        if (videoPlayer == null)
        {
            Log("❌ VideoPlayer not assigned!");
            return;
        }
        
        // Auto-load video clips from Resources if not assigned
        if (logoVideo == null)
        {
            logoVideo = Resources.Load<VideoClip>("Videos/LogoV");
            if (logoVideo != null)
            {
                Log("📹 Auto-loaded Logo video from Resources");
            }
            else
            {
                Log("⚠️ Logo video clip not found in Resources/Videos/LogoV");
            }
        }
        
        if (minipollVideo == null)
        {
            minipollVideo = Resources.Load<VideoClip>("Videos/MinipollV");
            if (minipollVideo != null)
            {
                Log("📹 Auto-loaded Minipoll video from Resources");
            }
            else
            {
                Log("⚠️ Minipoll video clip not found in Resources/Videos/MinipollV");
            }
        }
        
        // Basic video player setup
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.Stretch; // מלא את כל המסך
        
        // Create render texture that matches screen size
        if (videoPlayer.targetTexture == null)
        {
            // יצירת RenderTexture שמתאים לגודל המסך
            int screenWidth = Screen.width > 0 ? Screen.width : 1920;
            int screenHeight = Screen.height > 0 ? Screen.height : 1080;
            
            RenderTexture rt = new RenderTexture(screenWidth, screenHeight, 0, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 1; // למניעת בעיות ביצועים
            rt.Create();
            videoPlayer.targetTexture = rt;
            
            Log($"📺 Created RenderTexture: {screenWidth}x{screenHeight}");
        }
        
        // Setup video display to fill entire screen
        if (videoDisplay != null)
        {
            videoDisplay.texture = videoPlayer.targetTexture;
            
            // וידוא שה-RawImage ממלא את כל המסך
            RectTransform rectTransform = videoDisplay.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }
            
            videoDisplay.gameObject.SetActive(true);
            Log("📺 VideoDisplay configured to fill screen");
        }
        else
        {
            Log("⚠️ VideoDisplay not assigned!");
        }
        
        Log("📺 Video system setup complete");
    }
    
    IEnumerator PlayVideoOnce(VideoClip clip)
    {
        if (clip == null)
        {
            Log("⚠️ Video clip is null");
            yield break;
        }
        
        Log($"▶️ Playing {clip.name} once");
        
        // Setup and play
        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.Prepare();
        
        // Wait for preparation
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        
        videoPlayer.Play();
        
        // Wait for completion
        yield return new WaitUntil(() => !videoPlayer.isPlaying);
        
        Log($"✅ {clip.name} completed");
    }
    
    IEnumerator PlayMinipollInfiniteLoop()
    {
        if (minipollVideo == null)
        {
            Log("⚠️ Minipoll video clip is null");
            yield break;
        }
        
        Log($"♾️ Playing {minipollVideo.name} in infinite loop");
        
        // Setup and play with infinite loop
        videoPlayer.clip = minipollVideo;
        videoPlayer.isLooping = true;
        videoPlayer.Prepare();
        
        // Wait for preparation
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        
        videoPlayer.Play();
        
        Log($"✅ {minipollVideo.name} infinite loop started");
    }
    
    public IEnumerator FadeOutVideoAndTransition(string sceneName)
    {
        Log("🌙 Starting fade out and scene transition...");
        
        if (videoDisplay != null)
        {
            CanvasGroup canvasGroup = videoDisplay.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = videoDisplay.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Fade out over 1 second
            float fadeDuration = 1f;
            float elapsed = 0f;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                canvasGroup.alpha = alpha;
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
        }
        
        // Stop video
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        
        Log("🌙 Fade out complete, loading scene...");
        
        // Load scene
        if (!string.IsNullOrEmpty(sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
    
    IEnumerator PlayVideoLoop(VideoClip clip, float maxTime)
    {
        if (clip == null)
        {
            Log("⚠️ Video clip is null");
            yield break;
        }
        
        Log($"🔄 Playing {clip.name} in loop for {maxTime}s");
        
        // Setup and play with loop
        videoPlayer.clip = clip;
        videoPlayer.isLooping = true;
        videoPlayer.Prepare();
        
        // Wait for preparation
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        
        videoPlayer.Play();
        
        // Wait for max time or manual skip
        float elapsed = 0f;
        while (elapsed < maxTime && videoPlayer.isPlaying)
        {
            elapsed += Time.deltaTime;
            
            // Allow skipping with any key/click using new Input System
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame ||
                Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Log("⏭️ User skipped video loop");
                break;
            }
            
            yield return null;
        }
        
        videoPlayer.Stop();
        Log($"✅ {clip.name} loop finished");
    }
    
    void HideMenu()
    {
        if (menuButtons != null)
        {
            menuButtons.SetActive(false);
            Log("🙈 Menu hidden");
        }
    }
    
    void ShowMenu()
    {
        // DON'T stop the video - keep it playing in the background!
        // The video should continue looping while menu is visible
        
        // Keep VideoDisplay active so video shows in background
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);
            Log("📺 Video kept playing in background");
        }
        
        if (menuButtons != null)
        {
            menuButtons.SetActive(true);
            
            // MenuButtons are now in a separate Canvas with higher sort order
            // No need to adjust Z position - the Canvas sort order handles it
            Log("🎯 MenuButtons are in MenuCanvas (Sort Order 10) - above video!");
            
            // Trigger button entrance animations
            MenuButtonsAnimationController buttonController = menuButtons.GetComponent<MenuButtonsAnimationController>();
            if (buttonController != null)
            {
                buttonController.TriggerEntranceAnimation();
                Log("✨ Triggered button entrance animations");
            }
            
            Log("🎭 Menu shown over video");
        }
    }
    
    void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
    
    // Public methods for testing
    [ContextMenu("🎬 Test Sequence")]
    public void TestSequence()
    {
        StartCoroutine(RunEntranceSequence());
    }
    
    [ContextMenu("⏭️ Skip to Menu")]
    public void SkipToMenu()
    {
        StopAllCoroutines();
        ShowMenu();
    }
    
    [ContextMenu("📹 Test Video System")]
    public void TestVideoSystem()
    {
        SetupVideoSystem();
        if (minipollVideo != null && videoPlayer != null)
        {
            StartCoroutine(PlayMinipollInfiniteLoop());
            Log("🎬 Started Minipoll video test");
        }
        else
        {
            Log("❌ Cannot test video - missing components");
        }
    }
    
    [ContextMenu("👁️ Show Video Display")]
    public void ShowVideoDisplay()
    {
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);
            Log("👁️ VideoDisplay is now visible");
        }
        else
        {
            Log("❌ VideoDisplay not found");
        }
    }
    
    [ContextMenu("🎯 Force Show Menu Now")]
    public void ForceShowMenuNow()
    {
        Log("🚀 Force showing menu immediately...");
        ShowMenu();
    }
    
    [ContextMenu("🧪 Test Button Visibility")]
    public void TestButtonVisibility()
    {
        if (menuButtons != null)
        {
            Log($"📊 MenuButtons Status:");
            Log($"   - Active: {menuButtons.activeSelf}");
            Log($"   - ActiveInHierarchy: {menuButtons.activeInHierarchy}");
            Log($"   - Position: {menuButtons.transform.localPosition}");
            
            MenuButtonsAnimationController controller = menuButtons.GetComponent<MenuButtonsAnimationController>();
            if (controller != null)
            {
                Log($"   - AnimationsCompleted: {controller.AnimationsCompleted}");
            }
        }
        else
        {
            Log("❌ MenuButtons not found");
        }
    }
    
    [ContextMenu("🎮 Load Game Scene")]
    public void LoadGameScene()
    {
        Log("🎮 Loading game scene: 02_GameScene");
        StartCoroutine(FadeOutVideoAndTransition("02_GameScene"));
    }
    
    [ContextMenu("🚀 Emergency Button Test")]
    public void EmergencyButtonTest()
    {
        Log("🚨 EMERGENCY: Forcing buttons to show immediately!");
        
        // Force find MenuButtons 
        if (menuButtons == null)
        {
            menuButtons = GameObject.Find("MenuButtons");
        }
        
        if (menuButtons != null)
        {
            // Force active
            menuButtons.SetActive(true);
            
            // Force position in front
            RectTransform rect = menuButtons.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector3 pos = rect.localPosition;
                pos.z = 200f; // WAY in front
                rect.localPosition = pos;
                Log($"🎯 FORCED MenuButtons to Z={pos.z}");
            }
            
            // Find all buttons manually
            Button[] buttons = menuButtons.GetComponentsInChildren<Button>(true);
            Log($"🎮 Found {buttons.Length} buttons in MenuButtons");
            
            foreach (Button btn in buttons)
            {
                if (btn != null)
                {
                    btn.gameObject.SetActive(true);
                    btn.transform.localScale = Vector3.one;
                    btn.interactable = true;
                    Log($"✅ Activated button: {btn.name}");
                }
            }
            
            Log("🚨 EMERGENCY ACTIVATION COMPLETE!");
        }
        else
        {
            Log("❌ EMERGENCY FAILED - MenuButtons still null!");
        }
    }
}
