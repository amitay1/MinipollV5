using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enhanced tutorial system for Minipoll creature care
/// Creates emotional bonding through gentle, story-driven guidance
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("🎓 Tutorial Configuration")]
    public TutorialData tutorialData;
    public TutorialUI tutorialUI;
    public TutorialHighlight highlightSystem;
    
    [Header("⚙️ Settings")]
    public bool autoStartTutorial = true;
    public bool useComfortableSteps = true;
    public bool enableEmotionalFeedback = true;
    
    [Header("� Game Integration")]
    public GameObject minipollCreature;
    public Button feedButton;
    public Button playButton;
    public Button cleanButton;
    public GameObject needsUI;
    
    // Tutorial state
    private List<ComfortableTutorialStep> currentSteps;
    private int currentStepIndex = 0;
    private bool isTutorialActive = false;
    private bool waitingForUserAction = false;
    
    // Events for other systems to listen to
    public System.Action OnTutorialStarted;
    public System.Action OnTutorialCompleted;
    public System.Action<int> OnStepCompleted;
    
    void Start()
    {
        InitializeTutorial();
        
        if (autoStartTutorial && IsFirstTimePlayer())
        {
            StartTutorial();
        }
    }
    
    void InitializeTutorial()
    {
        // Load tutorial steps
        if (useComfortableSteps)
        {
            currentSteps = HebrewTutorialSteps.GetDefaultSteps();
        }
        else if (tutorialData != null)
        {
            // Convert TutorialData to ComfortableTutorialStep format
            // This would be implemented based on your data structure needs
        }
        
        // Setup UI references
        if (tutorialUI == null)
        {
            tutorialUI = FindFirstObjectByType<TutorialUI>();
        }
        
        if (highlightSystem == null)
        {
            highlightSystem = FindFirstObjectByType<TutorialHighlight>();
        }
        
        // Auto-find game components if not assigned
        AutoFindGameComponents();
    }
    
    void AutoFindGameComponents()
    {
        if (minipollCreature == null)
        {
            // Try to find existing Minipoll prefab
            GameObject prefabObj = GameObject.Find("Meshy_Model_20250527_193740.glb");
            if (prefabObj != null)
            {
                minipollCreature = prefabObj;
            }
            else
            {
                // Try to find by tag
                minipollCreature = GameObject.FindWithTag("Minipoll");
                if (minipollCreature == null)
                {
                    // Try to find by name variations
                    minipollCreature = GameObject.Find("Minipoll") ?? 
                                      GameObject.Find("MinipollCreature") ??
                                      GameObject.Find("Capsule"); // Fallback to temporary capsule
                }
            }
        }
        
        // Find UI buttons from existing system
        if (feedButton == null)
        {
            feedButton = GameObject.Find("FeedButton")?.GetComponent<Button>() ??
                        GameObject.Find("Button_Feed")?.GetComponent<Button>() ??
                        GameObject.Find("BtnFeed")?.GetComponent<Button>();
        }
        
        if (playButton == null)
        {
            playButton = GameObject.Find("PlayButton")?.GetComponent<Button>() ??
                        GameObject.Find("Button_Play")?.GetComponent<Button>() ??
                        GameObject.Find("BtnPlay")?.GetComponent<Button>();
        }
        
        if (cleanButton == null)
        {
            cleanButton = GameObject.Find("CleanButton")?.GetComponent<Button>() ??
                         GameObject.Find("Button_Clean")?.GetComponent<Button>() ??
                         GameObject.Find("BtnClean")?.GetComponent<Button>();
        }
        
        if (needsUI == null)
        {
            needsUI = GameObject.Find("NeedsUI") ?? 
                     GameObject.Find("NeedsPanel") ??
                     GameObject.Find("UI_Needs");
        }
    }
    
    bool IsFirstTimePlayer()
    {
        return !PlayerPrefs.HasKey("CompletedTutorial");
    }
    
    public void StartTutorial()
    {
        if (isTutorialActive) return;
        
        isTutorialActive = true;
        currentStepIndex = 0;
        waitingForUserAction = false;
        
        // Notify other systems
        OnTutorialStarted?.Invoke();
        
        // Show tutorial UI
        if (tutorialUI != null)
        {
            tutorialUI.ShowTutorial();
        }
        
        // Start first step
        StartCoroutine(ExecuteStep(currentSteps[0]));
    }
    
    IEnumerator ExecuteStep(ComfortableTutorialStep step)
    {
        Debug.Log($"Tutorial Step: {step.title}");
        
        // Setup highlights if needed
        SetupStepHighlights(step);
        
        // Display the step in UI
        if (tutorialUI != null)
        {
            tutorialUI.DisplayStep(step, currentStepIndex + 1, currentSteps.Count);
        }
        
        // If this step requires user action, wait for it
        if (step.isActionRequired)
        {
            waitingForUserAction = true;
            yield return StartCoroutine(WaitForUserAction(step));
            waitingForUserAction = false;
            
            // Show positive reinforcement
            if (step.showEncouragement && tutorialUI != null)
            {
                tutorialUI.ShowPositiveReinforcement(step.positiveReinforcement);
            }
        }
        
        // Wait minimum time for step
        yield return new WaitForSeconds(step.minimumTime);
        
        // Mark step as completed
        OnStepCompleted?.Invoke(currentStepIndex);
    }
    
    void SetupStepHighlights(ComfortableTutorialStep step)
    {
        if (highlightSystem == null) return;
        
        // Hide any existing highlights
        highlightSystem.HideHighlight();
        
        // Determine what to highlight based on step content
        if (step.isActionRequired)
        {
            if (step.actionInstructions.Contains("אוכל") || step.actionInstructions.Contains("האכיל"))
            {
                if (feedButton != null)
                {
                    highlightSystem.ShowHighlight(feedButton.GetComponent<RectTransform>());
                }
            }
            // Add more highlight conditions based on action types
        }
    }
    
    IEnumerator WaitForUserAction(ComfortableTutorialStep step)
    {
        // Wait for specific actions based on step requirements
        if (step.actionInstructions.Contains("אוכל") || step.actionInstructions.Contains("האכיל"))
        {
            yield return WaitForFeedingAction();
        }
        else if (step.actionInstructions.Contains("משחק"))
        {
            yield return WaitForPlayAction();
        }
        else
        {
            // Default: wait for any input
            yield return WaitForAnyInput();
        }
    }
    
    IEnumerator WaitForFeedingAction()
    {
        bool actionCompleted = false;
        
        while (!actionCompleted && waitingForUserAction)
        {
            // Check if feed button was clicked
            if (feedButton != null && Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                RectTransform buttonRect = feedButton.GetComponent<RectTransform>();
                
                if (RectTransformUtility.RectangleContainsScreenPoint(buttonRect, mousePos))
                {
                    actionCompleted = true;
                }
            }
            
            // Alternative: Check for keyboard shortcut
            if (Input.GetKeyDown(KeyCode.F))
            {
                actionCompleted = true;
            }
            
            yield return null;
        }
    }
    
    IEnumerator WaitForPlayAction()
    {
        bool actionCompleted = false;
        
        while (!actionCompleted && waitingForUserAction)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                actionCompleted = true;
            }
            
            yield return null;
        }
    }
    
    IEnumerator WaitForAnyInput()
    {
        bool actionCompleted = false;
        
        while (!actionCompleted && waitingForUserAction)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                actionCompleted = true;
            }
            
            yield return null;
        }
    }
    
    public void NextStep()
    {
        if (!isTutorialActive || waitingForUserAction) return;
        
        currentStepIndex++;
        
        if (currentStepIndex < currentSteps.Count)
        {
            StartCoroutine(ExecuteStep(currentSteps[currentStepIndex]));
        }
        else
        {
            CompleteTutorial();
        }
    }
    
    public void SkipTutorial()
    {
        if (isTutorialActive)
        {
            CompleteTutorial();
        }
    }
    
    void CompleteTutorial()
    {
        isTutorialActive = false;
        waitingForUserAction = false;
        
        // Hide all tutorial UI
        if (tutorialUI != null)
        {
            tutorialUI.CompleteTutorial();
        }
        
        if (highlightSystem != null)
        {
            highlightSystem.HideHighlight();
        }
        
        // Mark as completed
        PlayerPrefs.SetInt("CompletedTutorial", 1);
        PlayerPrefs.Save();
        
        // Notify systems
        OnTutorialCompleted?.Invoke();
        
        Debug.Log("🎉 Tutorial completed! Player is ready to care for Minipoll!");
        
        // Enable full game functionality
        EnableFullGameplay();
    }
    
    void EnableFullGameplay()
    {
        // Enable all game systems that were limited during tutorial
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            // Enable full AI, save system, etc.
        }
        
        // Enable all interaction buttons
        if (feedButton != null) feedButton.interactable = true;
        if (playButton != null) playButton.interactable = true;
        if (cleanButton != null) cleanButton.interactable = true;
    }
    
    // Public methods for external systems
    public void RestartTutorial()
    {
        PlayerPrefs.DeleteKey("CompletedTutorial");
        currentStepIndex = 0;
        StartTutorial();
    }
    
    public void TriggerTutorialAction(string actionType)
    {
        if (isTutorialActive && waitingForUserAction)
        {
            var currentStep = currentSteps[currentStepIndex];
            
            if (currentStep.actionInstructions.Contains(actionType))
            {
                // This action matches what we're waiting for
                waitingForUserAction = false;
            }
        }
    }
    
    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }
    
    public bool IsWaitingForAction()
    {
        return waitingForUserAction;
    }
}
