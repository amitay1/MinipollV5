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
        StartCoroutine(RunEntranceSequence());
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
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(false);
        }
        
        if (menuButtons != null)
        {
            menuButtons.SetActive(true);
            Log("🎭 Menu shown");
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
}
