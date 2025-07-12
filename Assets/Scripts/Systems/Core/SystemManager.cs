using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minipoll.Core.Events;
using Minipoll.Core.Events.Types;
using Minipoll.Core.Architecture;

namespace Minipoll.Core.Management
{
    /// <summary>
    /// System Manager - Coordinates initialization and shutdown of all game systems
    /// Follows the 5-level dependency hierarchy for proper system initialization order
    /// Level 1: Foundation (MINIPOLL PREFAB ARCHITECTURE, EVENT SYSTEM)
    /// Level 2: Infrastructure (SCENE LOADING FLOW, SAVE/LOAD SYSTEM, OPTIMIZATION SYSTEM)
    /// Level 3: Services (AUDIO REQUEST SOURCES, LOCALIZATION SYSTEM, DEVELOPER TOOLS & DEBUG)
    /// Level 4: Interaction (INTERACTION SYSTEM, WEATHER SYSTEM, INVENTORY ACTIONS)
    /// Level 5: Content (DIALOGUE & QUEST SYSTEM, MULTIPLAYER SYSTEM, MODDING SYSTEM)
    /// </summary>
    public class SystemManager : MonoBehaviour
    {
        [Header("System Initialization")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private float systemInitializationDelay = 0.1f;
        
        [Header("System References")]
        [SerializeField] private MinipollPrefabArchitecture prefabArchitecture;
        
        private static SystemManager instance;
        private Dictionary<string, IGameSystem> registeredSystems = new Dictionary<string, IGameSystem>();
        private Dictionary<int, List<IGameSystem>> systemsByLevel = new Dictionary<int, List<IGameSystem>>();
        private bool isInitialized = false;
        private bool isShuttingDown = false;
        
        public static SystemManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<SystemManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SystemManager");
                        instance = go.AddComponent<SystemManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        public bool IsInitialized => isInitialized;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSystemManager();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (instance == this)
            {
                StartCoroutine(InitializeAllSystems());
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            PublishSystemEvent("ApplicationPause", pauseStatus);
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            PublishSystemEvent("ApplicationFocus", hasFocus);
        }
        
        private void OnApplicationQuit()
        {
            ShutdownAllSystems();
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                ShutdownAllSystems();
            }
        }
        
        #endregion
        
        #region System Manager Initialization
        
        private void InitializeSystemManager()
        {
            try
            {
                // Initialize system level collections
                for (int i = 1; i <= 5; i++)
                {
                    systemsByLevel[i] = new List<IGameSystem>();
                }
                
                // Register for system events
                EventSystem.Subscribe<SystemInitializedEvent>(OnSystemInitialized);
                EventSystem.Subscribe<SystemShutdownEvent>(OnSystemShutdown);
                
                if (enableDebugLogging)
                    Debug.Log("[SystemManager] System Manager initialized");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SystemManager] Failed to initialize System Manager: {ex.Message}");
            }
        }
        
        #endregion
        
        #region System Registration
        
        /// <summary>
        /// Register a system with the manager
        /// </summary>
        public void RegisterSystem(IGameSystem system, int level)
        {
            if (system == null)
            {
                Debug.LogError("[SystemManager] Cannot register null system");
                return;
            }
            
            string systemName = system.SystemName;
            
            if (registeredSystems.ContainsKey(systemName))
            {
                Debug.LogWarning($"[SystemManager] System '{systemName}' already registered. Updating reference.");
            }
            
            registeredSystems[systemName] = system;
            
            if (level < 1 || level > 5)
            {
                Debug.LogWarning($"[SystemManager] Invalid system level {level} for '{systemName}'. Defaulting to level 3.");
                level = 3;
            }
            
            systemsByLevel[level].Add(system);
            
            if (enableDebugLogging)
                Debug.Log($"[SystemManager] Registered system '{systemName}' at level {level}");
        }
        
        /// <summary>
        /// Unregister a system from the manager
        /// </summary>
        public void UnregisterSystem(string systemName)
        {
            if (registeredSystems.ContainsKey(systemName))
            {
                var system = registeredSystems[systemName];
                registeredSystems.Remove(systemName);
                
                // Remove from level lists
                foreach (var levelList in systemsByLevel.Values)
                {
                    levelList.Remove(system);
                }
                
                if (enableDebugLogging)
                    Debug.Log($"[SystemManager] Unregistered system '{systemName}'");
            }
        }
        
        #endregion
        
        #region System Initialization
        
        private IEnumerator InitializeAllSystems()
        {
            if (isInitialized)
                yield break;
                
            isShuttingDown = false;
            
            if (enableDebugLogging)
                Debug.Log("[SystemManager] Starting system initialization sequence");
            
            // Initialize systems level by level
            for (int level = 1; level <= 5; level++)
            {
                yield return StartCoroutine(InitializeSystemsAtLevel(level));
            }
            
            isInitialized = true;
            
            EventSystem.Publish(new SystemInitializedEvent 
            { 
                SystemName = "SystemManager",
                Success = true,
                Message = $"All systems initialized. Total: {registeredSystems.Count}"
            });
            
            if (enableDebugLogging)
                Debug.Log($"[SystemManager] All systems initialized successfully. Total: {registeredSystems.Count}");
        }
        
        private IEnumerator InitializeSystemsAtLevel(int level)
        {
            if (!systemsByLevel.ContainsKey(level))
                yield break;
                
            var systems = systemsByLevel[level];
            
            if (systems.Count == 0)
            {
                if (enableDebugLogging)
                    Debug.Log($"[SystemManager] No systems to initialize at level {level}");
                yield break;
            }
            
            if (enableDebugLogging)
                Debug.Log($"[SystemManager] Initializing {systems.Count} systems at level {level}");
            
            foreach (var system in systems)
            {
                if (system != null && !isShuttingDown)
                {
                    // Initialize system - error handling moved to InitializeSystem method
                    yield return StartCoroutine(InitializeSystem(system));
                    
                    // Small delay between system initializations
                    yield return new WaitForSeconds(systemInitializationDelay);
                }
            }
            
            if (enableDebugLogging)
                Debug.Log($"[SystemManager] Completed initialization of level {level} systems");
        }
        
        private IEnumerator InitializeSystem(IGameSystem system)
        {
            if (enableDebugLogging)
                Debug.Log($"[SystemManager] Initializing system: {system.SystemName}");
            
            // Initialize the system using a safe coroutine approach
            yield return StartCoroutine(SafeInitializeSystem(system));
            
            if (enableDebugLogging)
                Debug.Log($"[SystemManager] System '{system.SystemName}' initialization attempt complete");
        }
        
        private IEnumerator SafeInitializeSystem(IGameSystem system)
        {
            // This method handles the actual initialization without try-catch around yield
            try
            {
                // Validate system before initialization
                if (system == null)
                {
                    Debug.LogError("[SystemManager] Cannot initialize null system");
                    yield break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SystemManager] Pre-initialization validation failed for '{system?.SystemName ?? "Unknown"}': {ex.Message}");
                yield break;
            }
            
            // Perform the actual initialization - exceptions will be caught by Unity's coroutine system
            yield return StartCoroutine(system.InitializeAsync());
        }
        
        #endregion
        
        #region System Shutdown
        
        private void ShutdownAllSystems()
        {
            if (isShuttingDown)
                return;
                
            isShuttingDown = true;
            isInitialized = false;
            
            if (enableDebugLogging)
                Debug.Log("[SystemManager] Starting system shutdown sequence");
            
            // Shutdown systems in reverse order (level 5 to 1)
            for (int level = 5; level >= 1; level--)
            {
                ShutdownSystemsAtLevel(level);
            }
            
            EventSystem.Publish(new SystemShutdownEvent { SystemName = "SystemManager" });
            
            if (enableDebugLogging)
                Debug.Log("[SystemManager] All systems shutdown complete");
        }
        
        private void ShutdownSystemsAtLevel(int level)
        {
            if (!systemsByLevel.ContainsKey(level))
                return;
                
            var systems = systemsByLevel[level];
            
            if (enableDebugLogging && systems.Count > 0)
                Debug.Log($"[SystemManager] Shutting down {systems.Count} systems at level {level}");
            
            foreach (var system in systems)
            {
                if (system != null)
                {
                    try
                    {
                        system.Shutdown();
                        
                        if (enableDebugLogging)
                            Debug.Log($"[SystemManager] Shutdown system: {system.SystemName}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SystemManager] Failed to shutdown system '{system.SystemName}': {ex.Message}");
                    }
                }
            }
        }
        
        #endregion
        
        #region System Queries
        
        /// <summary>
        /// Get a registered system by name
        /// </summary>
        public T GetSystem<T>(string systemName) where T : class, IGameSystem
        {
            return registeredSystems.ContainsKey(systemName) ? registeredSystems[systemName] as T : null;
        }
        
        /// <summary>
        /// Check if a system is registered
        /// </summary>
        public bool IsSystemRegistered(string systemName)
        {
            return registeredSystems.ContainsKey(systemName);
        }
        
        /// <summary>
        /// Get all systems at a specific level
        /// </summary>
        public List<IGameSystem> GetSystemsAtLevel(int level)
        {
            return systemsByLevel.ContainsKey(level) ? new List<IGameSystem>(systemsByLevel[level]) : new List<IGameSystem>();
        }
        
        /// <summary>
        /// Get all registered system names
        /// </summary>
        public List<string> GetAllSystemNames()
        {
            return new List<string>(registeredSystems.Keys);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSystemInitialized(SystemInitializedEvent eventData)
        {
            if (enableDebugLogging)
            {
                string status = eventData.Success ? "SUCCESS" : "FAILED";
                Debug.Log($"[SystemManager] System '{eventData.SystemName}' initialization: {status} - {eventData.Message}");
            }
        }
        
        private void OnSystemShutdown(SystemShutdownEvent eventData)
        {
            if (enableDebugLogging)
                Debug.Log($"[SystemManager] System '{eventData.SystemName}' shutdown");
        }
        
        private void PublishSystemEvent(string eventName, object data)
        {
            EventSystem.Publish(eventName, data);
        }
        
        #endregion
        
        #region Debug & Utilities
        
        /// <summary>
        /// Get debug information about all registered systems
        /// </summary>
        public string GetDebugInfo()
        {
            var info = "[System Manager Debug Info]\n";
            info += $"Initialized: {isInitialized}\n";
            info += $"Shutting Down: {isShuttingDown}\n";
            info += $"Total Systems: {registeredSystems.Count}\n\n";
            
            for (int level = 1; level <= 5; level++)
            {
                var systems = systemsByLevel[level];
                info += $"Level {level}: {systems.Count} systems\n";
                
                foreach (var system in systems)
                {
                    if (system != null)
                        info += $"  - {system.SystemName} (Status: {system.Status})\n";
                }
                info += "\n";
            }
            
            return info;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug Info")]
        private void LogDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        
        [ContextMenu("Force Initialize Systems")]
        private void ForceInitializeSystems()
        {
            StartCoroutine(InitializeAllSystems());
        }
        
        [ContextMenu("Force Shutdown Systems")]
        private void ForceShutdownSystems()
        {
            ShutdownAllSystems();
        }
        #endif
        
        #endregion
    }
    
    /// <summary>
    /// Interface that all game systems must implement
    /// </summary>
    public interface IGameSystem
    {
        string SystemName { get; }
        SystemStatus Status { get; }
        
        IEnumerator InitializeAsync();
        void Shutdown();
    }
    
    /// <summary>
    /// Status of a game system
    /// </summary>
    public enum SystemStatus
    {
        NotInitialized,
        Initializing,
        Running,
        Error,
        ShuttingDown,
        Shutdown
    }
}
