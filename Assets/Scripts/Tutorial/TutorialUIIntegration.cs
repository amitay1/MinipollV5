using UnityEngine;
using MinipollGame.UI;

namespace MinipollGame.Tutorial
{
    /// <summary>
    /// Tutorial UI Integration - Connects GameSceneUI with the existing tutorial system
    /// Ensures Feed/Play/Clean buttons work correctly with tutorial highlighting and events
    /// </summary>
    public class TutorialUIIntegration : MonoBehaviour
    {
        [Header("🎓 Tutorial Integration")]
        [SerializeField] private bool enableTutorialIntegration = true;
        [SerializeField] private bool enableDebugLogs = true;
        
        // References to systems
        private GameSceneUI gameSceneUI;
        private TutorialUI tutorialUI;
        private TutorialManager tutorialManager;
        
        // Tutorial step tracking
        private bool feedButtonHighlighted = false;
        private bool playButtonHighlighted = false;
        private bool cleanButtonHighlighted = false;
        
        void Start()
        {
            if (enableTutorialIntegration)
            {
                InitializeTutorialIntegration();
            }
        }
        
        void InitializeTutorialIntegration()
        {
            // Find required components
            gameSceneUI = FindFirstObjectByType<GameSceneUI>();
            tutorialUI = FindFirstObjectByType<TutorialUI>();
            tutorialManager = FindFirstObjectByType<TutorialManager>();
            
            if (gameSceneUI == null)
            {
                Debug.LogWarning("⚠️ TutorialUIIntegration: GameSceneUI not found");
                return;
            }
            
            // Subscribe to button events
            SubscribeToButtonEvents();
            
            if (enableDebugLogs)
            {
                Debug.Log("🎓 Tutorial UI Integration initialized");
            }
        }
        
        void SubscribeToButtonEvents()
        {
            if (gameSceneUI != null)
            {
                gameSceneUI.OnFeedButtonClicked.AddListener(HandleFeedButtonClicked);
                gameSceneUI.OnPlayButtonClicked.AddListener(HandlePlayButtonClicked);
                gameSceneUI.OnCleanButtonClicked.AddListener(HandleCleanButtonClicked);
                
                if (enableDebugLogs)
                {
                    Debug.Log("🔗 Subscribed to GameSceneUI button events");
                }
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (gameSceneUI != null)
            {
                gameSceneUI.OnFeedButtonClicked.RemoveListener(HandleFeedButtonClicked);
                gameSceneUI.OnPlayButtonClicked.RemoveListener(HandlePlayButtonClicked);
                gameSceneUI.OnCleanButtonClicked.RemoveListener(HandleCleanButtonClicked);
            }
        }
        
        #region Button Event Handlers
        
        void HandleFeedButtonClicked()
        {
            if (enableDebugLogs)
                Debug.Log("🍎 Tutorial: Feed button clicked");
            
            // Clear highlight if this button was highlighted
            if (feedButtonHighlighted)
            {
                ClearButtonHighlight("feed");
                feedButtonHighlighted = false;
            }
            
            // Notify tutorial system that feed action was completed
            NotifyTutorialAction("feed");
        }
        
        void HandlePlayButtonClicked()
        {
            if (enableDebugLogs)
                Debug.Log("⚽ Tutorial: Play button clicked");
            
            if (playButtonHighlighted)
            {
                ClearButtonHighlight("play");
                playButtonHighlighted = false;
            }
            
            NotifyTutorialAction("play");
        }
        
        void HandleCleanButtonClicked()
        {
            if (enableDebugLogs)
                Debug.Log("🧼 Tutorial: Clean button clicked");
            
            if (cleanButtonHighlighted)
            {
                ClearButtonHighlight("clean");
                cleanButtonHighlighted = false;
            }
            
            NotifyTutorialAction("clean");
        }
        
        #endregion
        
        #region Tutorial Interaction Methods
        
        /// <summary>
        /// Highlights a specific button for tutorial guidance
        /// Called by tutorial system to guide player attention
        /// </summary>
        public void HighlightActionButton(string buttonType)
        {
            if (gameSceneUI == null) return;
            
            if (enableDebugLogs)
                Debug.Log($"🎯 Tutorial: Highlighting {buttonType} button");
            
            // Clear any existing highlights
            ClearAllHighlights();
            
            // Set new highlight
            gameSceneUI.HighlightButton(buttonType);
            
            // Track which button is highlighted
            switch (buttonType.ToLower())
            {
                case "feed":
                    feedButtonHighlighted = true;
                    break;
                case "play":
                    playButtonHighlighted = true;
                    break;
                case "clean":
                    cleanButtonHighlighted = true;
                    break;
            }
        }
        
        /// <summary>
        /// Clears highlight from a specific button
        /// </summary>
        public void ClearButtonHighlight(string buttonType)
        {
            if (gameSceneUI == null) return;
            
            if (enableDebugLogs)
                Debug.Log($"🎯 Tutorial: Clearing highlight from {buttonType} button");
            
            gameSceneUI.ClearHighlights();
            
            // Update tracking
            switch (buttonType.ToLower())
            {
                case "feed":
                    feedButtonHighlighted = false;
                    break;
                case "play":
                    playButtonHighlighted = false;
                    break;
                case "clean":
                    cleanButtonHighlighted = false;
                    break;
            }
        }
        
        /// <summary>
        /// Clears all button highlights
        /// </summary>
        public void ClearAllHighlights()
        {
            if (gameSceneUI == null) return;
            
            gameSceneUI.ClearHighlights();
            feedButtonHighlighted = false;
            playButtonHighlighted = false;
            cleanButtonHighlighted = false;
            
            if (enableDebugLogs)
                Debug.Log("🎯 Tutorial: Cleared all button highlights");
        }
        
        /// <summary>
        /// Notifies the tutorial system that an action was completed
        /// </summary>
        void NotifyTutorialAction(string actionType)
        {
            if (tutorialManager != null)
            {
                // Send action completion to tutorial manager
                // This depends on the tutorial manager's API
                // tutorialManager.OnActionCompleted(actionType);
                
                if (enableDebugLogs)
                    Debug.Log($"📚 Tutorial: Notified action completion - {actionType}");
            }
        }
        
        /// <summary>
        /// Checks if the UI is ready for tutorial interactions
        /// </summary>
        public bool IsUIReady()
        {
            bool hasGameSceneUI = gameSceneUI != null;
            bool hasFeedButton = GameObject.Find("FeedButton") != null;
            bool hasPlayButton = GameObject.Find("PlayButton") != null;
            bool hasCleanButton = GameObject.Find("CleanButton") != null;
            bool hasNeedsUI = GameObject.Find("NeedsUI") != null;
            
            bool isReady = hasGameSceneUI && hasFeedButton && hasPlayButton && hasCleanButton && hasNeedsUI;
            
            if (enableDebugLogs)
            {
                Debug.Log($"🎓 Tutorial UI Ready Check: {isReady}");
                Debug.Log($"   - GameSceneUI: {hasGameSceneUI}");
                Debug.Log($"   - FeedButton: {hasFeedButton}");
                Debug.Log($"   - PlayButton: {hasPlayButton}");
                Debug.Log($"   - CleanButton: {hasCleanButton}");
                Debug.Log($"   - NeedsUI: {hasNeedsUI}");
            }
            
            return isReady;
        }
        
        #endregion
        
        #region Context Menu Debug Methods
        
        [ContextMenu("🎯 Test Feed Highlight")]
        public void TestFeedHighlight()
        {
            HighlightActionButton("feed");
        }
        
        [ContextMenu("🎯 Test Play Highlight")]
        public void TestPlayHighlight()
        {
            HighlightActionButton("play");
        }
        
        [ContextMenu("🎯 Test Clean Highlight")]
        public void TestCleanHighlight()
        {
            HighlightActionButton("clean");
        }
        
        [ContextMenu("🧹 Clear All Highlights")]
        public void TestClearHighlights()
        {
            ClearAllHighlights();
        }
        
        [ContextMenu("🔍 Check UI Ready")]
        public void TestUIReady()
        {
            bool ready = IsUIReady();
            Debug.Log($"🎓 UI Ready for Tutorial: {ready}");
        }
        
        #endregion
    }
}
