using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ObjectPoolManager - מנהל pools של אובייקטים לביצועים טובים יותר
/// מחזר אובייקטים במקום ליצור ולמחוק אותם כל הזמן
/// </summary>
namespace MinipollGame.Performance
{
    public class ObjectPoolManager : MonoBehaviour
    {
        #region Singleton
        public static ObjectPoolManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePools();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Settings
        [Header("=== Pool Settings ===")]
        [SerializeField] private int defaultPoolSize = 20;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private bool enableAutoCleanup = true;
        [SerializeField] private float cleanupInterval = 30f;

        [Header("=== Pre-configured Pools ===")]
        [SerializeField] private PoolConfig[] preConfiguredPools;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool logPoolActivity = false;
        #endregion

        #region Data Structures
        [System.Serializable]
        public class PoolConfig
        {
            public string poolName;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 50;
            public bool allowGrowth = true;
            public PoolType poolType = PoolType.GameObject;
        }

        public enum PoolType
        {
            GameObject,     // Regular GameObjects
            ParticleSystem, // VFX particles
            AudioSource,    // Audio clips
            UIElement,      // UI components
            FoodItem,       // Food/Resource items
            MemoryItem      // Memory system objects
        }

        public class ObjectPool
        {
            public string name;
            public GameObject prefab;
            public Queue<GameObject> availableObjects;
            public HashSet<GameObject> activeObjects;
            public Transform parentTransform;
            public int maxSize;
            public bool allowGrowth;
            public PoolType poolType;
            public float lastCleanupTime;

            // Statistics
            public int totalCreated;
            public int currentActive;
            public int peakUsage;

            public ObjectPool(string poolName, GameObject poolPrefab, int initialSize, int maximumSize, bool canGrow, PoolType type, Transform parent)
            {
                name = poolName;
                prefab = poolPrefab;
                availableObjects = new Queue<GameObject>();
                activeObjects = new HashSet<GameObject>();
                parentTransform = parent;
                maxSize = maximumSize;
                allowGrowth = canGrow;
                poolType = type;
                lastCleanupTime = Time.time;

                // Pre-populate pool
                for (int i = 0; i < initialSize; i++)
                {
                    CreateNewObject();
                }
            }

            private GameObject CreateNewObject()
            {
                GameObject newObj = Instantiate(prefab, parentTransform);
                newObj.SetActive(false);
                availableObjects.Enqueue(newObj);
                totalCreated++;
                return newObj;
            }

            public GameObject GetObject()
            {
                GameObject obj = null;

                if (availableObjects.Count > 0)
                {
                    obj = availableObjects.Dequeue();
                }
                else if (allowGrowth && totalCreated < maxSize)
                {
                    obj = CreateNewObject();
                    availableObjects.Dequeue(); // Remove from available since we're returning it
                }
                else if (allowGrowth && activeObjects.Count > 0)
                {
                    // Force return oldest active object
                    foreach (var activeObj in activeObjects)
                    {
                        ReturnObject(activeObj);
                        obj = availableObjects.Dequeue();
                        break;
                    }
                }

                if (obj != null)
                {
                    activeObjects.Add(obj);
                    obj.SetActive(true);
                    currentActive = activeObjects.Count;
                    peakUsage = Mathf.Max(peakUsage, currentActive);
                }

                return obj;
            }

            public void ReturnObject(GameObject obj)
            {
                if (obj == null || !activeObjects.Contains(obj)) return;

                // Reset object state
                ResetObject(obj);

                obj.SetActive(false);
                obj.transform.SetParent(parentTransform);
                activeObjects.Remove(obj);
                availableObjects.Enqueue(obj);
                currentActive = activeObjects.Count;
            }

            private void ResetObject(GameObject obj)
            {
                // Reset transform
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;

                // Type-specific resets
                switch (poolType)
                {
                    case PoolType.ParticleSystem:
                        var particles = obj.GetComponent<ParticleSystem>();
                        if (particles != null) particles.Clear();
                        break;

                    case PoolType.AudioSource:
                        var audioSource = obj.GetComponent<AudioSource>();
                        if (audioSource != null) audioSource.Stop();
                        break;

                    case PoolType.UIElement:
                        var canvasGroup = obj.GetComponent<CanvasGroup>();
                        if (canvasGroup != null) canvasGroup.alpha = 1f;
                        break;
                }

                // Reset any IPoolable components
                var poolable = obj.GetComponent<IPoolable>();
                poolable?.OnReturnToPool();
            }

            public void Cleanup()
            {
                if (Time.time - lastCleanupTime < 60f) return; // Cleanup max once per minute

                int targetSize = Mathf.Max(5, currentActive); // Keep at least 5 or current active count
                
                while (availableObjects.Count > targetSize)
                {
                    var obj = availableObjects.Dequeue();
                    if (obj != null)
                    {
                        DestroyImmediate(obj);
                        totalCreated--;
                    }
                }

                lastCleanupTime = Time.time;
            }
        }
        #endregion

        #region Internal Data
        private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();
        private Dictionary<GameObject, string> objectToPoolMap = new Dictionary<GameObject, string>();
        private Transform poolParent;
        #endregion

        #region Events
        public static event Action<string, int> OnPoolCreated;
        public static event Action<string, GameObject> OnObjectSpawned;
        public static event Action<string, GameObject> OnObjectReturned;
        #endregion

        #region Initialization
        private void InitializePools()
        {
            // Create parent object for pools
            poolParent = new GameObject("Object Pools").transform;
            poolParent.SetParent(transform);

            // Create pre-configured pools
            if (preConfiguredPools != null)
            {
                foreach (var config in preConfiguredPools)
                {
                    CreatePool(config.poolName, config.prefab, config.initialSize, config.maxSize, config.allowGrowth, config.poolType);
                }
            }

            // Schedule automatic cleanup
            if (enableAutoCleanup)
            {
                InvokeRepeating(nameof(PerformCleanup), cleanupInterval, cleanupInterval);
            }

            if (showDebugInfo)
                Debug.Log($"[ObjectPoolManager] Initialized with {pools.Count} pools");
        }
        #endregion

        #region Pool Management
        public bool CreatePool(string poolName, GameObject prefab, int initialSize = -1, int maxSize = -1, bool allowGrowth = true, PoolType poolType = PoolType.GameObject)
        {
            if (pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool '{poolName}' already exists!");
                return false;
            }

            if (prefab == null)
            {
                Debug.LogError($"[ObjectPoolManager] Cannot create pool '{poolName}' with null prefab!");
                return false;
            }

            // Use defaults if not specified
            if (initialSize < 0) initialSize = defaultPoolSize;
            if (maxSize < 0) maxSize = maxPoolSize;

            // Create pool parent
            Transform poolTransform = new GameObject($"Pool_{poolName}").transform;
            poolTransform.SetParent(poolParent);

            // Create pool
            var pool = new ObjectPool(poolName, prefab, initialSize, maxSize, allowGrowth, poolType, poolTransform);
            pools[poolName] = pool;

            OnPoolCreated?.Invoke(poolName, initialSize);

            if (logPoolActivity)
                Debug.Log($"[ObjectPoolManager] Created pool '{poolName}' with {initialSize} objects (Max: {maxSize})");

            return true;
        }

        public void DestroyPool(string poolName)
        {
            if (!pools.ContainsKey(poolName)) return;

            var pool = pools[poolName];
            
            // Return all active objects
            var activeObjects = new List<GameObject>(pool.activeObjects);
            foreach (var obj in activeObjects)
            {
                ReturnToPool(obj);
            }

            // Destroy pool parent and all objects
            if (pool.parentTransform != null)
                DestroyImmediate(pool.parentTransform.gameObject);

            pools.Remove(poolName);

            if (logPoolActivity)
                Debug.Log($"[ObjectPoolManager] Destroyed pool '{poolName}'");
        }
        #endregion

        #region Object Management
        public GameObject SpawnFromPool(string poolName, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (!pools.ContainsKey(poolName))
            {
                Debug.LogError($"[ObjectPoolManager] Pool '{poolName}' not found!");
                return null;
            }

            var pool = pools[poolName];
            var obj = pool.GetObject();

            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                
                if (parent != null)
                    obj.transform.SetParent(parent);

                objectToPoolMap[obj] = poolName;

                // Call IPoolable interface
                var poolable = obj.GetComponent<IPoolable>();
                poolable?.OnSpawnFromPool();

                OnObjectSpawned?.Invoke(poolName, obj);

                if (logPoolActivity)
                    Debug.Log($"[ObjectPoolManager] Spawned object from pool '{poolName}'");
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool '{poolName}' is full and cannot spawn more objects!");
            }

            return obj;
        }

        public T SpawnFromPool<T>(string poolName, Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component
        {
            var obj = SpawnFromPool(poolName, position, rotation, parent);
            return obj?.GetComponent<T>();
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null || !objectToPoolMap.ContainsKey(obj)) return;

            string poolName = objectToPoolMap[obj];
            if (pools.ContainsKey(poolName))
            {
                pools[poolName].ReturnObject(obj);
                objectToPoolMap.Remove(obj);

                OnObjectReturned?.Invoke(poolName, obj);

                if (logPoolActivity)
                    Debug.Log($"[ObjectPoolManager] Returned object to pool '{poolName}'");
            }
        }

        public void ReturnToPoolAfterDelay(GameObject obj, float delay)
        {
            if (obj != null)
            {
                StartCoroutine(ReturnAfterDelayCoroutine(obj, delay));
            }
        }

        private System.Collections.IEnumerator ReturnAfterDelayCoroutine(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(obj);
        }
        #endregion

        #region Convenience Methods
        // Emotion particles
        public GameObject SpawnEmotionEffect(string emotionType, Vector3 position, Transform parent = null)
        {
            return SpawnFromPool($"Emotion_{emotionType}", position, Quaternion.identity, parent);
        }

        // Food items
        public GameObject SpawnFoodItem(string foodType, Vector3 position)
        {
            return SpawnFromPool($"Food_{foodType}", position);
        }

        // Audio sources
        public AudioSource SpawnAudioSource(Vector3 position)
        {
            return SpawnFromPool<AudioSource>("AudioSource", position);
        }

        // UI notifications
        public GameObject SpawnUINotification(string notificationType, Transform parent)
        {
            return SpawnFromPool($"UI_{notificationType}", Vector3.zero, Quaternion.identity, parent);
        }

        // Memory items
        public GameObject SpawnMemoryItem(string memoryType)
        {
            return SpawnFromPool($"Memory_{memoryType}");
        }
        #endregion

        #region Cleanup & Maintenance
        private void PerformCleanup()
        {
            foreach (var pool in pools.Values)
            {
                pool.Cleanup();
            }

            if (showDebugInfo)
                Debug.Log("[ObjectPoolManager] Performed automatic cleanup");
        }

        public void ManualCleanup()
        {
            PerformCleanup();
        }

        public void ReturnAllObjects()
        {
            var objectsToReturn = new List<GameObject>(objectToPoolMap.Keys);
            foreach (var obj in objectsToReturn)
            {
                ReturnToPool(obj);
            }

            if (showDebugInfo)
                Debug.Log("[ObjectPoolManager] Returned all active objects to pools");
        }
        #endregion

        #region Statistics & Debugging
        public PoolStatistics GetPoolStatistics(string poolName)
        {
            if (!pools.ContainsKey(poolName)) return default;

            var pool = pools[poolName];
            return new PoolStatistics
            {
                poolName = poolName,
                totalObjects = pool.totalCreated,
                activeObjects = pool.currentActive,
                availableObjects = pool.availableObjects.Count,
                peakUsage = pool.peakUsage,
                poolType = pool.poolType
            };
        }

        public List<PoolStatistics> GetAllPoolStatistics()
        {
            var stats = new List<PoolStatistics>();
            foreach (var poolName in pools.Keys)
            {
                stats.Add(GetPoolStatistics(poolName));
            }
            return stats;
        }

        public bool HasPool(string poolName) => pools.ContainsKey(poolName);
        public int GetPoolCount() => pools.Count;
        public int GetTotalActiveObjects() => objectToPoolMap.Count;
        #endregion

        #region Debug Visualization
        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 310, 300));
            GUILayout.Label("<b>Object Pool Manager</b>");
            GUILayout.Label($"Active Pools: {pools.Count}");
            GUILayout.Label($"Total Active Objects: {objectToPoolMap.Count}");
            GUILayout.Space(10);

            foreach (var pool in pools.Values)
            {
                GUILayout.Label($"{pool.name}: {pool.currentActive}/{pool.totalCreated} (Peak: {pool.peakUsage})");
            }

            GUILayout.EndArea();
        }
        #endregion
    }

    #region Interfaces & Data Structures
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }

    [System.Serializable]
    public struct PoolStatistics
    {
        public string poolName;
        public int totalObjects;
        public int activeObjects;
        public int availableObjects;
        public int peakUsage;
        public ObjectPoolManager.PoolType poolType;
    }
    #endregion
}