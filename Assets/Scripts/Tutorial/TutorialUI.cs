using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Manages the visual presentation of tutorial steps
/// Creates warm, welcoming UI that builds emotional connection
/// </summary>
public class TutorialUI : MonoBehaviour
{
    [Header("🎨 Main Tutorial Panel")]
    public GameObject tutorialPanel;
    public CanvasGroup tutorialCanvasGroup;
    public Image backgroundImage;
    public Color warmBackgroundColor = new Color(0.2f, 0.3f, 0.8f, 0.9f);
    
    [Header("📝 Text Display")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI instructionText;
    
    [Header("🎭 Visual Elements")]
    public Image tutorialImage;
    public GameObject minipollIndicator;
    public ParticleSystem celebrationEffect;
    
    [Header("🎮 Interaction")]
    public Button nextButton;
    public Button skipButton;
    public Slider progressSlider;
    
    [Header("🔊 Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClick;
    public AudioClip stepComplete;
    public AudioClip tutorialComplete;
    
    [Header("⚙️ Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.3f;
    public float typewriterSpeed = 0.05f;
    
    private bool isVisible = false;
    private bool isTyping = false;
    private Coroutine currentTypewriterCoroutine;
    
    void Start()
    {
        InitializeUI();
    }
    
    void InitializeUI()
    {
        // Setup initial state
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0f;
        }
        
        // Setup button listeners
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(() => {
                PlaySound(buttonClick);
                OnNextButtonClicked();
            });
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(() => {
                PlaySound(buttonClick);
                OnSkipButtonClicked();
            });
        }
        
        // Setup background color
        if (backgroundImage != null)
        {
            backgroundImage.color = warmBackgroundColor;
        }
    }
    
    public void ShowTutorial()
    {
        StartCoroutine(ShowTutorialCoroutine());
    }
    
    public void HideTutorial()
    {
        StartCoroutine(HideTutorialCoroutine());
    }
    
    IEnumerator ShowTutorialCoroutine()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
        
        if (tutorialCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                tutorialCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            tutorialCanvasGroup.alpha = 1f;
        }
        
        isVisible = true;
    }
    
    IEnumerator HideTutorialCoroutine()
    {
        if (tutorialCanvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = tutorialCanvasGroup.alpha;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                tutorialCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            tutorialCanvasGroup.alpha = 0f;
        }
        
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        isVisible = false;
    }
    
    public void DisplayStep(ComfortableTutorialStep step, int currentStep, int totalSteps)
    {
        StartCoroutine(DisplayStepCoroutine(step, currentStep, totalSteps));
    }
    
    IEnumerator DisplayStepCoroutine(ComfortableTutorialStep step, int currentStep, int totalSteps)
    {
        // Update progress
        if (progressSlider != null)
        {
            float progress = (float)currentStep / totalSteps;
            progressSlider.value = progress;
        }
        
        // Clear previous content
        if (messageText != null)
        {
            messageText.text = "";
        }
        
        // Set title with gentle animation
        if (titleText != null)
        {
            titleText.text = step.title;
            StartCoroutine(AnimateTitle());
        }
        
        // Set instruction text if this step requires action
        if (instructionText != null)
        {
            if (step.isActionRequired)
            {
                instructionText.text = step.actionInstructions;
                instructionText.gameObject.SetActive(true);
                StartCoroutine(BlinkInstruction());
            }
            else
            {
                instructionText.gameObject.SetActive(false);
            }
        }
        
        // Setup tutorial image if available
        if (tutorialImage != null && step.minipollAnimation != "")
        {
            // Here you would load the appropriate sprite for the step
            // tutorialImage.sprite = GetSpriteForAnimation(step.minipollAnimation);
        }
        
        // Show Minipoll indicator with appropriate emotion
        ShowMinipollEmotion(step.minipollEmotion);
        
        // Type out the message
        if (currentTypewriterCoroutine != null)
        {
            StopCoroutine(currentTypewriterCoroutine);
        }
        currentTypewriterCoroutine = StartCoroutine(TypewriteMessage(step.message));
        
        // Wait for typewriting to complete
        yield return currentTypewriterCoroutine;
        
        // Wait minimum time for this step
        yield return new WaitForSeconds(step.minimumTime);
        
        // Enable next button if not waiting for user action
        if (nextButton != null)
        {
            nextButton.interactable = !step.isActionRequired;
        }
        
        // If this step doesn't require action and auto-advances
        if (!step.isActionRequired && !step.waitForPlayerReady)
        {
            yield return new WaitForSeconds(1f);
            OnNextButtonClicked();
        }
    }
    
    IEnumerator TypewriteMessage(string message)
    {
        isTyping = true;
        
        if (messageText != null)
        {
            messageText.text = "";
            
            for (int i = 0; i <= message.Length; i++)
            {
                messageText.text = message.Substring(0, i);
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }
        
        isTyping = false;
    }
    
    IEnumerator AnimateTitle()
    {
        if (titleText != null)
        {
            // Gentle scale animation for title
            Vector3 originalScale = titleText.transform.localScale;
            Vector3 targetScale = originalScale * 1.1f;
            
            float elapsed = 0f;
            float duration = 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                titleText.transform.localScale = Vector3.Lerp(originalScale, targetScale, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            
            titleText.transform.localScale = originalScale;
        }
    }
    
    IEnumerator BlinkInstruction()
    {
        if (instructionText != null)
        {
            Color originalColor = instructionText.color;
            Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            
            while (instructionText.gameObject.activeInHierarchy)
            {
                // Fade out
                float elapsed = 0f;
                float duration = 0.8f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    instructionText.color = Color.Lerp(originalColor, targetColor, elapsed / duration);
                    yield return null;
                }
                
                // Fade in
                elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    instructionText.color = Color.Lerp(targetColor, originalColor, elapsed / duration);
                    yield return null;
                }
            }
        }
    }
    
    void ShowMinipollEmotion(string emotion)
    {
        if (minipollIndicator != null)
        {
            // Here you would set up the Minipoll visual based on emotion
            // For now, just ensure it's visible
            minipollIndicator.SetActive(true);
            
            // Example: Change color based on emotion
            Image minipollImage = minipollIndicator.GetComponent<Image>();
            if (minipollImage != null)
            {
                switch (emotion)
                {
                    case "happy":
                        minipollImage.color = Color.yellow;
                        break;
                    case "sad":
                        minipollImage.color = Color.blue;
                        break;
                    case "excited":
                        minipollImage.color = Color.red;
                        break;
                    case "hungry":
                        minipollImage.color = Color.orange;
                        break;
                    default:
                        minipollImage.color = Color.white;
                        break;
                }
            }
        }
    }
    
    public void ShowPositiveReinforcement(string message)
    {
        StartCoroutine(ShowPositiveReinforcementCoroutine(message));
    }
    
    IEnumerator ShowPositiveReinforcementCoroutine(string message)
    {
        // Play celebration effect
        if (celebrationEffect != null)
        {
            celebrationEffect.Play();
        }
        
        // Play positive sound
        PlaySound(stepComplete);
        
        // Show temporary message
        if (instructionText != null)
        {
            string originalText = instructionText.text;
            Color originalColor = instructionText.color;
            
            instructionText.text = message;
            instructionText.color = Color.green;
            instructionText.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(2f);
            
            instructionText.text = originalText;
            instructionText.color = originalColor;
        }
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    void OnNextButtonClicked()
    {
        if (!isTyping)
        {
            // Notify tutorial manager
            var tutorialManager = FindFirstObjectByType<TutorialManager>();
            if (tutorialManager != null)
            {
                tutorialManager.NextStep();
            }
        }
        else
        {
            // Skip typewriter effect
            if (currentTypewriterCoroutine != null)
            {
                StopCoroutine(currentTypewriterCoroutine);
                isTyping = false;
                
                // Show full message immediately
                var tutorialManager = FindFirstObjectByType<TutorialManager>();
                if (tutorialManager != null && messageText != null)
                {
                    // Get current step message and display it fully
                    // This would require access to current step data
                }
            }
        }
    }
    
    void OnSkipButtonClicked()
    {
        var tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager != null)
        {
            tutorialManager.SkipTutorial();
        }
    }
    
    public void CompleteTutorial()
    {
        StartCoroutine(CompleteTutorialCoroutine());
    }
    
    IEnumerator CompleteTutorialCoroutine()
    {
        // Play completion sound
        PlaySound(tutorialComplete);
        
        // Show final celebration
        if (celebrationEffect != null)
        {
            celebrationEffect.Play();
        }
        
        // Wait a bit for celebration
        yield return new WaitForSeconds(2f);
        
        // Hide tutorial
        HideTutorial();
    }
}
