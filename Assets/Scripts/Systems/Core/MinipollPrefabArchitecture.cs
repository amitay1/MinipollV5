using System;
using System.Collections.Generic;
using UnityEngine;
using Minipoll.Core.Events;
using Minipoll.Core.Events.Types;

namespace Minipoll.Core.Architecture
{
    /// <summary>
    /// Minipoll Prefab Architecture - Level 1 Foundation System
    /// Manages the creation, registration, and lifecycle of all prefabs in the game
    /// Provides centralized prefab management with event integration
    /// </summary>
    public class MinipollPrefabArchitecture : MonoBehaviour
    {
        [Header("Prefab Registry")]
        [SerializeField] private PrefabRegistryData prefabRegistry;
        
        [Header("Runtime Management")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private Transform dynamicObjectsParent;
        
        private static MinipollPrefabArchitecture instance;
        private Dictionary<string, GameObject> registeredPrefabs = new Dictionary<string, GameObject>();
        private Dictionary<string, List<GameObject>> instantiatedObjects = new Dictionary<string, List<GameObject>>();
        private Dictionary<GameObject, PrefabMetadata> objectMetadata = new Dictionary<GameObject, PrefabMetadata>();
        
        public static MinipollPrefabArchitecture Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<MinipollPrefabArchitecture>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("MinipollPrefabArchitecture");
                        instance = go.AddComponent<MinipollPrefabArchitecture>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePrefabArchitecture();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (dynamicObjectsParent == null)
            {
                var dynamicParent = new GameObject("DynamicObjects");
                dynamicObjectsParent = dynamicParent.transform;
                DontDestroyOnLoad(dynamicParent);
            }
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                EventSystem.Publish(new SystemShutdownEvent { SystemName = "MinipollPrefabArchitecture" });
            }
        }
        
        #endregion
        
        #region System Initialization
        
        private void InitializePrefabArchitecture()
        {
            try
            {
                LoadPrefabRegistry();
                RegisterSystemEvents();
                
                EventSystem.Publish(new SystemInitializedEvent 
                { 
                    SystemName = "MinipollPrefabArchitecture",
                    Success = true,
                    Message = $"Initialized with {registeredPrefabs.Count} prefabs"
                });
                
                if (enableDebugLogging)
                    Debug.Log($"[PrefabArchitecture] System initialized successfully with {registeredPrefabs.Count} registered prefabs");
            }
            catch (Exception ex)
            {
                EventSystem.Publish(new SystemInitializedEvent 
                { 
                    SystemName = "MinipollPrefabArchitecture",
                    Success = false,
                    Message = ex.Message
                });
                
                Debug.LogError($"[PrefabArchitecture] Initialization failed: {ex.Message}");
            }
        }
        
        private void LoadPrefabRegistry()
        {
            if (prefabRegistry != null)
            {
                foreach (var entry in prefabRegistry.PrefabEntries)
                {
                    if (entry.Prefab != null && !string.IsNullOrEmpty(entry.PrefabId))
                    {
                        RegisterPrefab(entry.PrefabId, entry.Prefab, entry.Category, entry.Tags);
                    }
                }
            }
        }
        
        private void RegisterSystemEvents()
        {
            EventSystem.Subscribe<SceneLoadCompleteEvent>(OnSceneLoadComplete);
        }
        
        #endregion
        
        #region Prefab Registration
        
        /// <summary>
        /// Register a prefab with the architecture system
        /// </summary>
        public void RegisterPrefab(string prefabId, GameObject prefab, string category = "", List<string> tags = null)
        {
            if (string.IsNullOrEmpty(prefabId))
            {
                Debug.LogError("[PrefabArchitecture] Cannot register prefab with empty ID");
                return;
            }
            
            if (prefab == null)
            {
                Debug.LogError($"[PrefabArchitecture] Cannot register null prefab for ID: {prefabId}");
                return;
            }
            
            if (registeredPrefabs.ContainsKey(prefabId))
            {
                Debug.LogWarning($"[PrefabArchitecture] Prefab ID '{prefabId}' already registered. Updating reference.");
            }
            
            registeredPrefabs[prefabId] = prefab;
            
            if (!instantiatedObjects.ContainsKey(prefabId))
                instantiatedObjects[prefabId] = new List<GameObject>();
            
            if (enableDebugLogging)
                Debug.Log($"[PrefabArchitecture] Registered prefab '{prefabId}' in category '{category}'");
        }
        
        /// <summary>
        /// Unregister a prefab from the architecture system
        /// </summary>
        public void UnregisterPrefab(string prefabId)
        {
            if (registeredPrefabs.ContainsKey(prefabId))
            {
                registeredPrefabs.Remove(prefabId);
                
                // Clean up any instantiated objects
                if (instantiatedObjects.ContainsKey(prefabId))
                {
                    foreach (var obj in instantiatedObjects[prefabId])
                    {
                        if (obj != null)
                        {
                            objectMetadata.Remove(obj);
                            Destroy(obj);
                        }
                    }
                    instantiatedObjects.Remove(prefabId);
                }
                
                if (enableDebugLogging)
                    Debug.Log($"[PrefabArchitecture] Unregistered prefab '{prefabId}'");
            }
        }
        
        #endregion
        
        #region Object Instantiation
        
        /// <summary>
        /// Create an instance of a registered prefab
        /// </summary>
        public GameObject CreateInstance(string prefabId, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (!registeredPrefabs.ContainsKey(prefabId))
            {
                Debug.LogError($"[PrefabArchitecture] Prefab '{prefabId}' not registered");
                return null;
            }
            
            try
            {
                GameObject prefab = registeredPrefabs[prefabId];
                
                if (parent == null)
                    parent = dynamicObjectsParent;
                
                GameObject instance = Instantiate(prefab, position, rotation, parent);
                instance.name = $"{prefab.name}_{Guid.NewGuid().ToString("N")[..8]}";
                
                // Store metadata
                var metadata = new PrefabMetadata
                {
                    PrefabId = prefabId,
                    InstanceId = instance.GetInstanceID().ToString(),
                    CreationTime = DateTime.Now,
                    Position = position,
                    Rotation = rotation
                };
                
                objectMetadata[instance] = metadata;
                instantiatedObjects[prefabId].Add(instance);
                
                // Add PrefabInstance component for identification
                var prefabInstance = instance.GetComponent<PrefabInstance>();
                if (prefabInstance == null)
                    prefabInstance = instance.AddComponent<PrefabInstance>();
                
                prefabInstance.Initialize(prefabId, metadata);
                
                if (enableDebugLogging)
                    Debug.Log($"[PrefabArchitecture] Created instance of '{prefabId}' at {position}");
                
                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PrefabArchitecture] Failed to create instance of '{prefabId}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Destroy an instance and clean up metadata
        /// </summary>
        public void DestroyInstance(GameObject instance)
        {
            if (instance == null) return;
            
            if (objectMetadata.ContainsKey(instance))
            {
                var metadata = objectMetadata[instance];
                objectMetadata.Remove(instance);
                
                if (instantiatedObjects.ContainsKey(metadata.PrefabId))
                {
                    instantiatedObjects[metadata.PrefabId].Remove(instance);
                }
                
                if (enableDebugLogging)
                    Debug.Log($"[PrefabArchitecture] Destroyed instance of '{metadata.PrefabId}'");
            }
            
            Destroy(instance);
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// Get all instances of a specific prefab
        /// </summary>
        public List<GameObject> GetInstances(string prefabId)
        {
            return instantiatedObjects.ContainsKey(prefabId) 
                ? new List<GameObject>(instantiatedObjects[prefabId]) 
                : new List<GameObject>();
        }
        
        /// <summary>
        /// Get metadata for a specific instance
        /// </summary>
        public PrefabMetadata GetMetadata(GameObject instance)
        {
            return objectMetadata.ContainsKey(instance) ? objectMetadata[instance] : null;
        }
        
        /// <summary>
        /// Check if a prefab is registered
        /// </summary>
        public bool IsPrefabRegistered(string prefabId)
        {
            return registeredPrefabs.ContainsKey(prefabId);
        }
        
        /// <summary>
        /// Get all registered prefab IDs
        /// </summary>
        public List<string> GetRegisteredPrefabIds()
        {
            return new List<string>(registeredPrefabs.Keys);
        }
        
        /// <summary>
        /// Get total count of instances for a prefab
        /// </summary>
        public int GetInstanceCount(string prefabId)
        {
            return instantiatedObjects.ContainsKey(prefabId) ? instantiatedObjects[prefabId].Count : 0;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSceneLoadComplete(SceneLoadCompleteEvent eventData)
        {
            if (eventData.Success)
            {
                // Clean up destroyed objects from tracking
                CleanupDestroyedObjects();
                
                if (enableDebugLogging)
                    Debug.Log($"[PrefabArchitecture] Scene '{eventData.SceneName}' loaded, cleaned up destroyed objects");
            }
        }
        
        private void CleanupDestroyedObjects()
        {
            var keysToCheck = new List<string>(instantiatedObjects.Keys);
            
            foreach (var prefabId in keysToCheck)
            {
                instantiatedObjects[prefabId].RemoveAll(obj => obj == null);
            }
            
            var metadataKeysToRemove = new List<GameObject>();
            foreach (var kvp in objectMetadata)
            {
                if (kvp.Key == null)
                    metadataKeysToRemove.Add(kvp.Key);
            }
            
            foreach (var key in metadataKeysToRemove)
            {
                objectMetadata.Remove(key);
            }
        }
        
        #endregion
        
        #region Debug & Utilities
        
        /// <summary>
        /// Get debug information about the prefab architecture
        /// </summary>
        public string GetDebugInfo()
        {
            var info = "[Minipoll Prefab Architecture Debug Info]\n";
            info += $"Registered Prefabs: {registeredPrefabs.Count}\n";
            info += $"Total Instances: {objectMetadata.Count}\n\n";
            
            foreach (var kvp in instantiatedObjects)
            {
                info += $"  {kvp.Key}: {kvp.Value.Count} instances\n";
            }
            
            return info;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug Info")]
        private void LogDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        #endif
        
        #endregion
    }
    
    /// <summary>
    /// Metadata stored for each prefab instance
    /// </summary>
    [Serializable]
    public class PrefabMetadata
    {
        public string PrefabId;
        public string InstanceId;
        public DateTime CreationTime;
        public Vector3 Position;
        public Quaternion Rotation;
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Component attached to prefab instances for identification
    /// </summary>
    public class PrefabInstance : MonoBehaviour
    {
        [SerializeField] private string prefabId;
        [SerializeField] private string instanceId;
        [SerializeField] private PrefabMetadata metadata;
        
        public string PrefabId => prefabId;
        public string InstanceId => instanceId;
        public PrefabMetadata Metadata => metadata;
        
        public void Initialize(string id, PrefabMetadata meta)
        {
            prefabId = id;
            instanceId = meta.InstanceId;
            metadata = meta;
        }
        
        private void OnDestroy()
        {
            // Notify architecture system of destruction
            if (MinipollPrefabArchitecture.Instance != null)
            {
                // This will be handled by the architecture system's cleanup
            }
        }
    }
}
