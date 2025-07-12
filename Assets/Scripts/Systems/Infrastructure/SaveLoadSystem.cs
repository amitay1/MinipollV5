using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Minipoll.Core.Events;
using Minipoll.Core.Events.Types;
using Minipoll.Core.Management;

namespace Minipoll.Core.Infrastructure
{
    /// <summary>
    /// Save/Load System - Level 2 Infrastructure System
    /// Manages persistent game state, creature data, and player progress
    /// Depends on: MinipollPrefabArchitecture, EventSystem, SceneLoadingFlow
    /// </summary>
    public class SaveLoadSystem : MonoBehaviour, IGameSystem
    {
        [Header("Save Configuration")]
        [SerializeField] private string saveDirectory = "MinipollSaves";
        [SerializeField] private string saveFileExtension = ".minipoll";
        [SerializeField] private int maxSaveSlots = 10;
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        
        [Header("Save Data Settings")]
        [SerializeField] private bool compressData = true;
        [SerializeField] private bool encryptData = false;
        [SerializeField] private bool enableBackups = true;
        [SerializeField] private int maxBackups = 3;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool logSaveOperations = true;
        
        private static SaveLoadSystem instance;
        private Dictionary<string, ISaveable> saveableObjects = new Dictionary<string, ISaveable>();
        private Dictionary<string, SaveSlotInfo> saveSlots = new Dictionary<string, SaveSlotInfo>();
        private SystemStatus status = SystemStatus.NotInitialized;
        private string currentSaveSlot = null;
        private float lastAutoSaveTime = 0f;
        private Coroutine autoSaveCoroutine;
        
        public static SaveLoadSystem Instance => instance;
        public string SystemName => "SaveLoadSystem";
        public SystemStatus Status => status;
        public string CurrentSaveSlot => currentSaveSlot;
        public Dictionary<string, SaveSlotInfo> SaveSlots => new Dictionary<string, SaveSlotInfo>(saveSlots);
        
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
                // Create save directory if it doesn't exist
                CreateSaveDirectory();
                
                // Scan for existing save files
                ScanSaveSlots();
                
                // Register for events
                RegisterEvents();
                
                // Start auto-save if enabled
                if (enableAutoSave)
                {
                    StartAutoSave();
                }
                
                status = SystemStatus.Running;
            }
            catch (Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
                status = SystemStatus.Error;
            }
            
            if (success)
            {
                EventSystem.Publish(new SystemInitializedEvent
                {
                    SystemName = SystemName,
                    Success = true,
                    Message = $"Save/Load System initialized with {saveSlots.Count} save slots found"
                });
                
                if (enableDebugLogging)
                    Debug.Log($"[SaveLoadSystem] System initialized successfully. Save directory: {GetSaveDirectoryPath()}");
            }
            else
            {
                EventSystem.Publish(new SystemInitializedEvent
                {
                    SystemName = SystemName,
                    Success = false,
                    Message = errorMessage
                });
                
                Debug.LogError($"[SaveLoadSystem] Initialization failed: {errorMessage}");
            }
            
            yield return null;
        }
        
        public void Shutdown()
        {
            status = SystemStatus.ShuttingDown;
            
            try
            {
                // Stop auto-save
                StopAutoSave();
                
                // Perform final auto-save if enabled
                if (enableAutoSave && !string.IsNullOrEmpty(currentSaveSlot))
                {
                    SaveGame(currentSaveSlot, true);
                }
                
                // Unregister events
                UnregisterEvents();
                
                // Clear data
                saveableObjects.Clear();
                saveSlots.Clear();
                
                status = SystemStatus.Shutdown;
                
                EventSystem.Publish(new SystemShutdownEvent { SystemName = SystemName });
                
                if (enableDebugLogging)
                    Debug.Log($"[SaveLoadSystem] System shutdown complete");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadSystem] Shutdown error: {ex.Message}");
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
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                Shutdown();
            }
        }
        
        #endregion
        
        #region Save Operations
        
        /// <summary>
        /// Save the current game state to a specific slot
        /// </summary>
        public bool SaveGame(string slotName, bool isAutoSave = false)
        {
            if (status != SystemStatus.Running)
            {
                Debug.LogWarning("[SaveLoadSystem] Cannot save - system not running");
                return false;
            }
            
            try
            {
                if (logSaveOperations)
                    Debug.Log($"[SaveLoadSystem] Starting save operation for slot '{slotName}' (AutoSave: {isAutoSave})");
                
                // Create save data
                GameSaveData saveData = CreateSaveData();
                
                // Get save file path
                string filePath = GetSaveFilePath(slotName);
                
                // Create backup if enabled
                if (enableBackups && File.Exists(filePath))
                {
                    CreateBackup(filePath);
                }
                
                // Serialize and write data
                string jsonData = JsonUtility.ToJson(saveData, true);
                
                if (compressData)
                {
                    jsonData = CompressString(jsonData);
                }
                
                if (encryptData)
                {
                    jsonData = EncryptString(jsonData);
                }
                
                File.WriteAllText(filePath, jsonData);
                
                // Update save slot info
                UpdateSaveSlotInfo(slotName, saveData);
                
                // Update current save slot
                if (!isAutoSave)
                {
                    currentSaveSlot = slotName;
                }
                
                // Publish save event
                EventSystem.Publish(new GameSaveEvent 
                { 
                    SaveSlot = slotName, 
                    AutoSave = isAutoSave 
                });
                
                if (logSaveOperations)
                    Debug.Log($"[SaveLoadSystem] Save completed successfully for slot '{slotName}'");
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadSystem] Failed to save game to slot '{slotName}': {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Load game state from a specific slot
        /// </summary>
        public bool LoadGame(string slotName)
        {
            if (status != SystemStatus.Running)
            {
                Debug.LogWarning("[SaveLoadSystem] Cannot load - system not running");
                return false;
            }
            
            try
            {
                string filePath = GetSaveFilePath(slotName);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"[SaveLoadSystem] Save file not found: {filePath}");
                    return false;
                }
                
                if (logSaveOperations)
                    Debug.Log($"[SaveLoadSystem] Starting load operation for slot '{slotName}'");
                
                // Read and deserialize data
                string jsonData = File.ReadAllText(filePath);
                
                if (encryptData)
                {
                    jsonData = DecryptString(jsonData);
                }
                
                if (compressData)
                {
                    jsonData = DecompressString(jsonData);
                }
                
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                // Apply save data
                ApplySaveData(saveData);
                
                // Update current save slot
                currentSaveSlot = slotName;
                
                // Publish load event
                EventSystem.Publish(new GameLoadEvent 
                { 
                    SaveSlot = slotName, 
                    Success = true 
                });
                
                if (logSaveOperations)
                    Debug.Log($"[SaveLoadSystem] Load completed successfully for slot '{slotName}'");
                
                return true;
            }
            catch (Exception ex)
            {
                EventSystem.Publish(new GameLoadEvent 
                { 
                    SaveSlot = slotName, 
                    Success = false 
                });
                
                Debug.LogError($"[SaveLoadSystem] Failed to load game from slot '{slotName}': {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Delete a save slot
        /// </summary>
        public bool DeleteSave(string slotName)
        {
            try
            {
                string filePath = GetSaveFilePath(slotName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    
                    // Remove from save slots
                    saveSlots.Remove(slotName);
                    
                    // Clear current save slot if it was deleted
                    if (currentSaveSlot == slotName)
                    {
                        currentSaveSlot = null;
                    }
                    
                    if (logSaveOperations)
                        Debug.Log($"[SaveLoadSystem] Deleted save slot '{slotName}'");
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadSystem] Failed to delete save slot '{slotName}': {ex.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Saveable Object Management
        
        /// <summary>
        /// Register an object that can be saved/loaded
        /// </summary>
        public void RegisterSaveable(string id, ISaveable saveable)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[SaveLoadSystem] Cannot register saveable with empty ID");
                return;
            }
            
            if (saveable == null)
            {
                Debug.LogError($"[SaveLoadSystem] Cannot register null saveable with ID '{id}'");
                return;
            }
            
            saveableObjects[id] = saveable;
            
            if (enableDebugLogging)
                Debug.Log($"[SaveLoadSystem] Registered saveable object: {id}");
        }
        
        /// <summary>
        /// Unregister a saveable object
        /// </summary>
        public void UnregisterSaveable(string id)
        {
            if (saveableObjects.ContainsKey(id))
            {
                saveableObjects.Remove(id);
                
                if (enableDebugLogging)
                    Debug.Log($"[SaveLoadSystem] Unregistered saveable object: {id}");
            }
        }
        
        #endregion
        
        #region Save Data Creation and Application
        
        private GameSaveData CreateSaveData()
        {
            var saveData = new GameSaveData
            {
                version = Application.version,
                timestamp = DateTime.Now,
                currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                playerData = new Dictionary<string, object>(),
                objectData = new Dictionary<string, object>()
            };
            
            // Collect data from all saveable objects
            foreach (var kvp in saveableObjects)
            {
                try
                {
                    var data = kvp.Value.GetSaveData();
                    if (data != null)
                    {
                        saveData.objectData[kvp.Key] = data;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveLoadSystem] Error getting save data from '{kvp.Key}': {ex.Message}");
                }
            }
            
            return saveData;
        }
        
        private void ApplySaveData(GameSaveData saveData)
        {
            // Apply data to all saveable objects
            foreach (var kvp in saveData.objectData)
            {
                if (saveableObjects.ContainsKey(kvp.Key))
                {
                    try
                    {
                        saveableObjects[kvp.Key].LoadSaveData(kvp.Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SaveLoadSystem] Error applying save data to '{kvp.Key}': {ex.Message}");
                    }
                }
            }
            
            // Load scene if different from current
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(saveData.currentScene) && saveData.currentScene != currentSceneName)
            {
                if (SceneLoadingFlow.Instance != null)
                {
                    SceneLoadingFlow.Instance.LoadScene(saveData.currentScene);
                }
            }
        }
        
        #endregion
        
        #region Auto Save
        
        private void StartAutoSave()
        {
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
            }
            
            autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
        }
        
        private void StopAutoSave()
        {
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
                autoSaveCoroutine = null;
            }
        }
        
        private IEnumerator AutoSaveCoroutine()
        {
            while (enableAutoSave && status == SystemStatus.Running)
            {
                yield return new WaitForSeconds(autoSaveInterval);
                
                if (!string.IsNullOrEmpty(currentSaveSlot))
                {
                    SaveGame($"{currentSaveSlot}_autosave", true);
                }
            }
        }
        
        #endregion
        
        #region File Operations
        
        private void CreateSaveDirectory()
        {
            string path = GetSaveDirectoryPath();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        
        private string GetSaveDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, saveDirectory);
        }
        
        private string GetSaveFilePath(string slotName)
        {
            return Path.Combine(GetSaveDirectoryPath(), slotName + saveFileExtension);
        }
        
        private void ScanSaveSlots()
        {
            saveSlots.Clear();
            
            string saveDir = GetSaveDirectoryPath();
            if (!Directory.Exists(saveDir))
                return;
            
            string[] saveFiles = Directory.GetFiles(saveDir, "*" + saveFileExtension);
            
            foreach (string filePath in saveFiles)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    FileInfo fileInfo = new FileInfo(filePath);
                    
                    var slotInfo = new SaveSlotInfo
                    {
                        slotName = fileName,
                        filePath = filePath,
                        lastModified = fileInfo.LastWriteTime,
                        fileSize = fileInfo.Length
                    };
                    
                    saveSlots[fileName] = slotInfo;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SaveLoadSystem] Error reading save file '{filePath}': {ex.Message}");
                }
            }
        }
        
        private void UpdateSaveSlotInfo(string slotName, GameSaveData saveData)
        {
            string filePath = GetSaveFilePath(slotName);
            FileInfo fileInfo = new FileInfo(filePath);
            
            saveSlots[slotName] = new SaveSlotInfo
            {
                slotName = slotName,
                filePath = filePath,
                lastModified = fileInfo.LastWriteTime,
                fileSize = fileInfo.Length,
                gameVersion = saveData.version,
                currentScene = saveData.currentScene,
                timestamp = saveData.timestamp
            };
        }
        
        #endregion
        
        #region Backup System
        
        private void CreateBackup(string originalFilePath)
        {
            try
            {
                string backupDir = Path.Combine(GetSaveDirectoryPath(), "Backups");
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }
                
                string fileName = Path.GetFileNameWithoutExtension(originalFilePath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"{fileName}_backup_{timestamp}{saveFileExtension}";
                string backupFilePath = Path.Combine(backupDir, backupFileName);
                
                File.Copy(originalFilePath, backupFilePath);
                
                // Clean up old backups
                CleanupOldBackups(backupDir, fileName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveLoadSystem] Failed to create backup: {ex.Message}");
            }
        }
        
        private void CleanupOldBackups(string backupDir, string fileName)
        {
            try
            {
                string[] backupFiles = Directory.GetFiles(backupDir, $"{fileName}_backup_*{saveFileExtension}");
                
                if (backupFiles.Length > maxBackups)
                {
                    Array.Sort(backupFiles, (x, y) => File.GetLastWriteTime(x).CompareTo(File.GetLastWriteTime(y)));
                    
                    int filesToDelete = backupFiles.Length - maxBackups;
                    for (int i = 0; i < filesToDelete; i++)
                    {
                        File.Delete(backupFiles[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveLoadSystem] Failed to cleanup old backups: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Compression and Encryption (Placeholder)
        
        private string CompressString(string input)
        {
            // Placeholder for compression implementation
            // Could use System.IO.Compression.GZipStream
            return input;
        }
        
        private string DecompressString(string input)
        {
            // Placeholder for decompression implementation
            return input;
        }
        
        private string EncryptString(string input)
        {
            // Placeholder for encryption implementation
            // Could use System.Security.Cryptography
            return input;
        }
        
        private string DecryptString(string input)
        {
            // Placeholder for decryption implementation
            return input;
        }
        
        #endregion
        
        #region Event Management
        
        private void RegisterEvents()
        {
            EventSystem.Subscribe<SceneLoadCompleteEvent>(OnSceneLoadComplete);
            EventSystem.Subscribe<MinipollSpawnedEvent>(OnMinipollSpawned);
        }
        
        private void UnregisterEvents()
        {
            EventSystem.Unsubscribe<SceneLoadCompleteEvent>(OnSceneLoadComplete);
            EventSystem.Unsubscribe<MinipollSpawnedEvent>(OnMinipollSpawned);
        }
        
        private void OnSceneLoadComplete(SceneLoadCompleteEvent eventData)
        {
            if (eventData.Success)
            {
                // Scene loaded successfully, update save data if needed
                if (enableDebugLogging)
                    Debug.Log($"[SaveLoadSystem] Scene '{eventData.SceneName}' loaded, save system ready");
            }
        }
        
        private void OnMinipollSpawned(MinipollSpawnedEvent eventData)
        {
            // Automatically register new Minipoll creatures as saveable
            var saveable = eventData.MinipollObject?.GetComponent<ISaveable>();
            if (saveable != null)
            {
                RegisterSaveable(eventData.MinipollId, saveable);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Check if a save slot exists
        /// </summary>
        public bool SaveSlotExists(string slotName)
        {
            return saveSlots.ContainsKey(slotName);
        }
        
        /// <summary>
        /// Get available save slot names
        /// </summary>
        public List<string> GetAvailableSaveSlots()
        {
            return new List<string>(saveSlots.Keys);
        }
        
        /// <summary>
        /// Get information about a save slot
        /// </summary>
        public SaveSlotInfo GetSaveSlotInfo(string slotName)
        {
            return saveSlots.ContainsKey(slotName) ? saveSlots[slotName] : null;
        }
        
        #endregion
        
        #region Debug & Utilities
        
        public string GetDebugInfo()
        {
            var info = "[Save/Load System Debug Info]\n";
            info += $"Status: {status}\n";
            info += $"Current Save Slot: {currentSaveSlot ?? "None"}\n";
            info += $"Auto Save Enabled: {enableAutoSave}\n";
            info += $"Save Directory: {GetSaveDirectoryPath()}\n";
            info += $"Registered Saveable Objects: {saveableObjects.Count}\n";
            info += $"Available Save Slots: {saveSlots.Count}\n\n";
            
            foreach (var kvp in saveSlots)
            {
                info += $"  {kvp.Key}: {kvp.Value.lastModified:yyyy-MM-dd HH:mm:ss}\n";
            }
            
            return info;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug Info")]
        private void LogDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        
        [ContextMenu("Scan Save Slots")]
        private void ScanSaveSlotsFromMenu()
        {
            ScanSaveSlots();
            Debug.Log($"[SaveLoadSystem] Found {saveSlots.Count} save slots");
        }
        #endif
        
        #endregion
    }
    
    /// <summary>
    /// Interface for objects that can be saved and loaded
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Get data that should be saved
        /// </summary>
        object GetSaveData();
        
        /// <summary>
        /// Load data from save
        /// </summary>
        void LoadSaveData(object saveData);
    }
    
    /// <summary>
    /// Game save data structure
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public string version;
        public DateTime timestamp;
        public string currentScene;
        public Dictionary<string, object> playerData;
        public Dictionary<string, object> objectData;
    }
    
    /// <summary>
    /// Information about a save slot
    /// </summary>
    [Serializable]
    public class SaveSlotInfo
    {
        public string slotName;
        public string filePath;
        public DateTime lastModified;
        public long fileSize;
        public string gameVersion;
        public string currentScene;
        public DateTime timestamp;
    }
}
