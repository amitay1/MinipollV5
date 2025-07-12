using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Minipoll.Core.Events;
using Minipoll.Core.Events.Types;
using Minipoll.Core.Management;

namespace Minipoll.Core.Infrastructure
{
    /// <summary>
    /// Scene Loading Flow - Level 2 Infrastructure System
    /// Manages dynamic scene loading, transitions, and scene state
    /// Depends on: MinipollPrefabArchitecture, EventSystem
    /// </summary>
    public class SceneLoadingFlow : MonoBehaviour, IGameSystem
    {
        [Header("Scene Management")]
        [SerializeField] private List<SceneInfo> availableScenes = new List<SceneInfo>();
        [SerializeField] private string defaultSceneName = "02_GameScene";
        [SerializeField] private bool preloadScenes = false;
        
        [Header("Loading Configuration")]
        [SerializeField] private float minLoadingTime = 1f;
        [SerializeField] private bool showLoadingProgress = true;
        [SerializeField] private bool enableDebugLogging = true;
        
        [Header("Scene Transition")]
        [SerializeField] private GameObject loadingScreenPrefab;
        [SerializeField] private float transitionFadeTime = 0.5f;
        
        private static SceneLoadingFlow instance;
        private Dictionary<string, SceneInfo> sceneRegistry = new Dictionary<string, SceneInfo>();
        private Dictionary<string, AsyncOperation> preloadedScenes = new Dictionary<string, AsyncOperation>();
        private SceneInfo currentScene;
        private bool isLoading = false;
        private SystemStatus status = SystemStatus.NotInitialized;
        
        public static SceneLoadingFlow Instance => instance;
        public string SystemName => "SceneLoadingFlow";
        public SystemStatus Status => status;
        public SceneInfo CurrentScene => currentScene;
        public bool IsLoading => isLoading;
        
        #region IGameSystem Implementation
        
        public IEnumerator InitializeAsync()
        {
            status = SystemStatus.Initializing;
            
            return InitializeAsyncInternal();
        }
        
        private IEnumerator InitializeAsyncInternal()
        {
            bool success = true;
            string errorMessage = "";
            
            try
            {
                // Initialize scene registry
                InitializeSceneRegistry();
                
                // Register for events
                RegisterEvents();
                
                // Set current scene info
                UpdateCurrentSceneInfo();
                
                status = SystemStatus.Running;
            }
            catch (Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
                status = SystemStatus.Error;
            }
            
            if (!success)
            {
                EventSystem.Publish(new SystemInitializedEvent
                {
                    SystemName = SystemName,
                    Success = false,
                    Message = errorMessage
                });
                
                Debug.LogError($"[SceneLoadingFlow] Initialization failed: {errorMessage}");
                yield break;
            }
            
            // Preload scenes if enabled (outside try-catch to allow yield)
            if (preloadScenes)
            {
                yield return StartCoroutine(PreloadScenes());
            }
            
            EventSystem.Publish(new SystemInitializedEvent
            {
                SystemName = SystemName,
                Success = true,
                Message = $"Scene Loading Flow initialized with {sceneRegistry.Count} scenes"
            });
            
            if (enableDebugLogging)
                Debug.Log($"[SceneLoadingFlow] System initialized successfully");
            
            yield return null;
        }
        
        public void Shutdown()
        {
            status = SystemStatus.ShuttingDown;
            
            try
            {
                // Stop any ongoing operations
                StopAllCoroutines();
                
                // Unregister events
                UnregisterEvents();
                
                // Clear preloaded scenes
                foreach (var preloadedScene in preloadedScenes.Values)
                {
                    if (preloadedScene != null && !preloadedScene.isDone)
                    {
                        // Can't cancel AsyncOperation, but we can stop tracking it
                    }
                }
                preloadedScenes.Clear();
                
                status = SystemStatus.Shutdown;
                
                EventSystem.Publish(new SystemShutdownEvent { SystemName = SystemName });
                
                if (enableDebugLogging)
                    Debug.Log($"[SceneLoadingFlow] System shutdown complete");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneLoadingFlow] Shutdown error: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Register with System Manager
                if (SystemManager.Instance != null)
                {
                    SystemManager.Instance.RegisterSystem(this, 2); // Level 2 Infrastructure
                }
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Initialization will be handled by SystemManager
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                Shutdown();
            }
        }
        
        #endregion
        
        #region Scene Management
        
        /// <summary>
        /// Load a scene by name
        /// </summary>
        public void LoadScene(string sceneName, bool additive = false)
        {
            if (isLoading)
            {
                Debug.LogWarning($"[SceneLoadingFlow] Cannot load scene '{sceneName}' - already loading");
                return;
            }
            
            if (!sceneRegistry.ContainsKey(sceneName))
            {
                Debug.LogError($"[SceneLoadingFlow] Scene '{sceneName}' not found in registry");
                return;
            }
            
            StartCoroutine(LoadSceneAsync(sceneName, additive));
        }
        
        /// <summary>
        /// Load a scene asynchronously
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName, bool additive = false)
        {
            isLoading = true;
            
            var loadOperation = LoadSceneAsyncInternal(sceneName, additive);
            yield return StartCoroutine(loadOperation);
            
            isLoading = false;
        }
        
        private IEnumerator LoadSceneAsyncInternal(string sceneName, bool additive)
        {
            float startTime = Time.time;
            GameObject loadingScreen = null;
            AsyncOperation asyncLoad = null;
            bool success = true;
            string errorMessage = "";
            
            try
            {
                // Publish scene load start event
                EventSystem.Publish(new SceneLoadStartEvent { SceneName = sceneName });
                
                if (enableDebugLogging)
                    Debug.Log($"[SceneLoadingFlow] Starting to load scene: {sceneName}");
                
                // Show loading screen if available
                if (loadingScreenPrefab != null)
                {
                    loadingScreen = Instantiate(loadingScreenPrefab);
                    DontDestroyOnLoad(loadingScreen);
                }
                
                // Load the scene
                if (preloadedScenes.ContainsKey(sceneName))
                {
                    // Use preloaded scene
                    asyncLoad = preloadedScenes[sceneName];
                    asyncLoad.allowSceneActivation = true;
                    preloadedScenes.Remove(sceneName);
                }
                else
                {
                    // Load scene normally
                    LoadSceneMode loadMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
                    asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadMode);
                }
            }
            catch (Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
            }
            
            if (!success)
            {
                EventSystem.Publish(new SceneLoadCompleteEvent 
                { 
                    SceneName = sceneName, 
                    Success = false 
                });
                
                Debug.LogError($"[SceneLoadingFlow] Failed to load scene '{sceneName}': {errorMessage}");
                yield break;
            }
            
            // Wait for scene to load
            while (asyncLoad != null && !asyncLoad.isDone)
            {
                if (showLoadingProgress)
                {
                    float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                    // Update loading screen progress here
                }
                
                yield return null;
            }
            
            // Ensure minimum loading time
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minLoadingTime)
            {
                yield return new WaitForSeconds(minLoadingTime - elapsedTime);
            }
            
            // Update current scene info
            UpdateCurrentSceneInfo();
            
            // Hide loading screen
            if (loadingScreen != null)
            {
                yield return new WaitForSeconds(transitionFadeTime);
                Destroy(loadingScreen);
            }
            
            // Publish scene load complete event
            EventSystem.Publish(new SceneLoadCompleteEvent 
            { 
                SceneName = sceneName, 
                Success = true 
            });
            
            if (enableDebugLogging)
                Debug.Log($"[SceneLoadingFlow] Scene '{sceneName}' loaded successfully");
        }
        
        /// <summary>
        /// Unload a scene by name
        /// </summary>
        public void UnloadScene(string sceneName)
        {
            if (isLoading)
            {
                Debug.LogWarning($"[SceneLoadingFlow] Cannot unload scene '{sceneName}' - currently loading");
                return;
            }
            
            StartCoroutine(UnloadSceneAsync(sceneName));
        }
        
        private IEnumerator UnloadSceneAsync(string sceneName)
        {
            AsyncOperation asyncUnload = null;
            bool success = true;
            string errorMessage = "";
            
            try
            {
                if (enableDebugLogging)
                    Debug.Log($"[SceneLoadingFlow] Unloading scene: {sceneName}");
                
                asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
            }
            catch (Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
            }
            
            if (!success)
            {
                Debug.LogError($"[SceneLoadingFlow] Failed to unload scene '{sceneName}': {errorMessage}");
                yield break;
            }
            
            while (asyncUnload != null && !asyncUnload.isDone)
            {
                yield return null;
            }
            
            if (enableDebugLogging)
                Debug.Log($"[SceneLoadingFlow] Scene '{sceneName}' unloaded successfully");
        }
        
        #endregion
        
        #region Scene Preloading
        
        private IEnumerator PreloadScenes()
        {
            foreach (var sceneInfo in availableScenes)
            {
                if (sceneInfo.preloadScene && !preloadedScenes.ContainsKey(sceneInfo.sceneName))
                {
                    yield return StartCoroutine(PreloadScene(sceneInfo.sceneName));
                }
            }
        }
        
        private IEnumerator PreloadScene(string sceneName)
        {
            AsyncOperation asyncLoad = null;
            bool success = true;
            string errorMessage = "";
            
            try
            {
                if (enableDebugLogging)
                    Debug.Log($"[SceneLoadingFlow] Preloading scene: {sceneName}");
                
                asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                asyncLoad.allowSceneActivation = false;
                
                preloadedScenes[sceneName] = asyncLoad;
            }
            catch (Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
            }
            
            if (!success)
            {
                Debug.LogError($"[SceneLoadingFlow] Failed to preload scene '{sceneName}': {errorMessage}");
                yield break;
            }
            
            while (asyncLoad != null && asyncLoad.progress < 0.9f)
            {
                yield return null;
            }
            
            if (enableDebugLogging)
                Debug.Log($"[SceneLoadingFlow] Scene '{sceneName}' preloaded successfully");
        }
        
        #endregion
        
        #region Scene Registry
        
        private void InitializeSceneRegistry()
        {
            sceneRegistry.Clear();
            
            foreach (var sceneInfo in availableScenes)
            {
                if (!string.IsNullOrEmpty(sceneInfo.sceneName))
                {
                    sceneRegistry[sceneInfo.sceneName] = sceneInfo;
                }
            }
            
            // Add default scene if not in registry
            if (!sceneRegistry.ContainsKey(defaultSceneName))
            {
                sceneRegistry[defaultSceneName] = new SceneInfo
                {
                    sceneName = defaultSceneName,
                    displayName = "Default Game Scene",
                    sceneType = SceneType.Gameplay,
                    preloadScene = false
                };
            }
        }
        
        private void UpdateCurrentSceneInfo()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (sceneRegistry.ContainsKey(activeScene.name))
            {
                currentScene = sceneRegistry[activeScene.name];
            }
            else
            {
                currentScene = new SceneInfo
                {
                    sceneName = activeScene.name,
                    displayName = activeScene.name,
                    sceneType = SceneType.Unknown,
                    preloadScene = false
                };
            }
        }
        
        #endregion
        
        #region Event Management
        
        private void RegisterEvents()
        {
            // Listen for system events that might affect scene loading
            EventSystem.Subscribe<SystemInitializedEvent>(OnSystemInitialized);
        }
        
        private void UnregisterEvents()
        {
            EventSystem.Unsubscribe<SystemInitializedEvent>(OnSystemInitialized);
        }
        
        private void OnSystemInitialized(SystemInitializedEvent eventData)
        {
            if (eventData.SystemName == "MinipollPrefabArchitecture" && eventData.Success)
            {
                // Prefab architecture is ready, we can now safely load scenes with prefabs
                if (enableDebugLogging)
                    Debug.Log("[SceneLoadingFlow] Prefab architecture ready - scene loading fully enabled");
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get information about a registered scene
        /// </summary>
        public SceneInfo GetSceneInfo(string sceneName)
        {
            return sceneRegistry.ContainsKey(sceneName) ? sceneRegistry[sceneName] : null;
        }
        
        /// <summary>
        /// Get all available scenes
        /// </summary>
        public List<SceneInfo> GetAvailableScenes()
        {
            return new List<SceneInfo>(sceneRegistry.Values);
        }
        
        /// <summary>
        /// Check if a scene is registered
        /// </summary>
        public bool IsSceneRegistered(string sceneName)
        {
            return sceneRegistry.ContainsKey(sceneName);
        }
        
        /// <summary>
        /// Check if a scene is preloaded
        /// </summary>
        public bool IsScenePreloaded(string sceneName)
        {
            return preloadedScenes.ContainsKey(sceneName);
        }
        
        #endregion
        
        #region Debug & Utilities
        
        public string GetDebugInfo()
        {
            var info = "[Scene Loading Flow Debug Info]\n";
            info += $"Status: {status}\n";
            info += $"Current Scene: {(currentScene != null ? currentScene.sceneName : "None")}\n";
            info += $"Is Loading: {isLoading}\n";
            info += $"Registered Scenes: {sceneRegistry.Count}\n";
            info += $"Preloaded Scenes: {preloadedScenes.Count}\n\n";
            
            foreach (var kvp in sceneRegistry)
            {
                info += $"  {kvp.Key}: {kvp.Value.sceneType}\n";
            }
            
            return info;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug Info")]
        private void LogDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        
        [ContextMenu("Load Default Scene")]
        private void LoadDefaultScene()
        {
            LoadScene(defaultSceneName);
        }
        #endif
        
        #endregion
    }
    
    /// <summary>
    /// Information about a scene
    /// </summary>
    [Serializable]
    public class SceneInfo
    {
        [Header("Scene Identity")]
        public string sceneName;
        public string displayName;
        public SceneType sceneType;
        
        [Header("Loading Configuration")]
        public bool preloadScene = false;
        public float minLoadingTime = 1f;
        public bool showLoadingScreen = true;
        
        [Header("Dependencies")]
        public List<string> requiredSystems = new List<string>();
        public List<string> requiredPrefabs = new List<string>();
        
        [Header("Metadata")]
        [TextArea(2, 4)]
        public string description;
    }
    
    /// <summary>
    /// Types of scenes in the game
    /// </summary>
    public enum SceneType
    {
        Unknown,
        MainMenu,
        Gameplay,
        Loading,
        Settings,
        Credits,
        Tutorial,
        Cutscene
    }
}
