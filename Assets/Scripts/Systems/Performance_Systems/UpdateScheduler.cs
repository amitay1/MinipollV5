using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UpdateScheduler - מנהל את כל ה-updates של המערכות בפרויקט
/// מפזר את העומס על פני פריימים כדי למנוע lag
/// </summary>
namespace MinipollGame.Performance
{
    public class UpdateScheduler : MonoBehaviour
    {
        #region Singleton
        public static UpdateScheduler Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeScheduler();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Settings
        [Header("=== Performance Settings ===")]
        [SerializeField] private int maxUpdatesPerFrame = 20;
        [SerializeField] private float baseUpdateInterval = 0.1f;
        [SerializeField] private bool enableDistanceOptimization = true;
        [SerializeField] private float optimizationDistance = 50f;

        [Header("=== Monitoring ===")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool logPerformanceWarnings = true;
        [SerializeField] private float performanceWarningThreshold = 16.67f; // 60 FPS
        #endregion

        #region Data Structures
        [System.Serializable]
        public class ScheduledUpdate
        {
            public Action updateAction;
            public float interval;
            public float nextUpdateTime;
            public bool isPaused;
            public Transform targetTransform; // For distance optimization
            public UpdatePriority priority;
            public string debugName;

            public ScheduledUpdate(Action action, float updateInterval, UpdatePriority updatePriority = UpdatePriority.Normal, Transform target = null, string name = "Unknown")
            {
                updateAction = action;
                interval = updateInterval;
                nextUpdateTime = Time.time + UnityEngine.Random.Range(0f, updateInterval); // Spread initial updates
                isPaused = false;
                targetTransform = target;
                priority = updatePriority;
                debugName = name;
            }
        }

        public enum UpdatePriority
        {
            Critical,   // Always runs (core systems)
            High,       // Important (AI, player interactions)
            Normal,     // Standard (most systems)
            Low,        // Background (analytics, cleanup)
            VeryLow     // Optional (distant objects)
        }
        #endregion

        #region Internal Data
        private List<ScheduledUpdate> scheduledUpdates = new List<ScheduledUpdate>();
        private List<ScheduledUpdate> updateQueue = new List<ScheduledUpdate>();
        private Camera playerCamera;
        private int updateIndex = 0;

        // Performance tracking
        private float frameStartTime;
        private int updatesThisFrame;
        private float totalUpdateTime;
        private int totalUpdatesPerSecond;
        #endregion

        #region Events
        public static event Action<float> OnPerformanceWarning; // Frame time exceeded
        public static event Action<PerformanceStats> OnPerformanceStats;
        #endregion

        #region Initialization
        private void InitializeScheduler()
        {
            playerCamera = Camera.main;
            
            if (showDebugInfo)
                Debug.Log("[UpdateScheduler] Initialized successfully");
        }

        private void Start()
        {
            // Schedule performance monitoring
            ScheduleUpdate(UpdatePerformanceStats, 1f, UpdatePriority.Low, null, "PerformanceMonitor");
        }
        #endregion

        #region Update Management
        private void Update()
        {
            frameStartTime = Time.realtimeSinceStartup;
            updatesThisFrame = 0;

            // Build update queue for this frame
            BuildUpdateQueue();

            // Execute updates with performance limits
            ExecuteScheduledUpdates();

            // Performance monitoring
            MonitorFramePerformance();
        }

        private void BuildUpdateQueue()
        {
            updateQueue.Clear();
            float currentTime = Time.time;

            // Find all updates that should run this frame
            foreach (var update in scheduledUpdates)
            {
                if (!update.isPaused && currentTime >= update.nextUpdateTime)
                {
                    updateQueue.Add(update);
                }
            }

            // Sort by priority and distance
            updateQueue.Sort((a, b) =>
            {
                // First by priority
                int priorityComparison = a.priority.CompareTo(b.priority);
                if (priorityComparison != 0) return priorityComparison;

                // Then by distance (closer objects first)
                if (enableDistanceOptimization && playerCamera != null)
                {
                    float distanceA = GetDistanceToCamera(a.targetTransform);
                    float distanceB = GetDistanceToCamera(b.targetTransform);
                    return distanceA.CompareTo(distanceB);
                }

                return 0;
            });
        }

        private void ExecuteScheduledUpdates()
        {
            int maxUpdates = GetMaxUpdatesForCurrentPerformance();
            
            for (int i = 0; i < updateQueue.Count && updatesThisFrame < maxUpdates; i++)
            {
                var update = updateQueue[i];
                
                // Skip distant updates if performance optimization is enabled
                if (ShouldSkipUpdate(update))
                    continue;

                try
                {
                    float updateStartTime = Time.realtimeSinceStartup;
                    update.updateAction?.Invoke();
                    float updateDuration = Time.realtimeSinceStartup - updateStartTime;

                    // Update timing
                    update.nextUpdateTime = Time.time + update.interval;
                    updatesThisFrame++;
                    totalUpdateTime += updateDuration;

                    // Log slow updates
                    if (logPerformanceWarnings && updateDuration > 0.01f) // 10ms
                    {
                        Debug.LogWarning($"[UpdateScheduler] Slow update detected: {update.debugName} took {updateDuration * 1000:F2}ms");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UpdateScheduler] Error in scheduled update '{update.debugName}': {e.Message}");
                }
            }
        }

        private bool ShouldSkipUpdate(ScheduledUpdate update)
        {
            if (!enableDistanceOptimization || playerCamera == null || update.targetTransform == null)
                return false;

            float distance = GetDistanceToCamera(update.targetTransform);
            
            // Always update critical and high priority
            if (update.priority <= UpdatePriority.High)
                return false;

            // Skip distant low priority updates with increasing probability
            if (distance > optimizationDistance)
            {
                float skipChance = Mathf.Clamp01((distance - optimizationDistance) / optimizationDistance);
                
                switch (update.priority)
                {
                    case UpdatePriority.Normal:
                        return UnityEngine.Random.value < skipChance * 0.3f;
                    case UpdatePriority.Low:
                        return UnityEngine.Random.value < skipChance * 0.6f;
                    case UpdatePriority.VeryLow:
                        return UnityEngine.Random.value < skipChance * 0.9f;
                }
            }

            return false;
        }

        private float GetDistanceToCamera(Transform target)
        {
            if (target == null || playerCamera == null)
                return 0f;
            
            return Vector3.Distance(playerCamera.transform.position, target.position);
        }

        private int GetMaxUpdatesForCurrentPerformance()
        {
            float frameTime = Time.deltaTime * 1000f; // Convert to milliseconds
            
            // Reduce max updates if frame time is high
            if (frameTime > 25f) // Below 40 FPS
                return Mathf.Max(5, maxUpdatesPerFrame / 4);
            else if (frameTime > 20f) // Below 50 FPS
                return Mathf.Max(10, maxUpdatesPerFrame / 2);
            else if (frameTime > 16.67f) // Below 60 FPS
                return Mathf.Max(15, (maxUpdatesPerFrame * 3) / 4);
            
            return maxUpdatesPerFrame;
        }
        #endregion

        #region Public API
        public void ScheduleUpdate(Action updateAction, float interval, UpdatePriority priority = UpdatePriority.Normal, Transform target = null, string debugName = "Unknown")
        {
            if (updateAction == null)
            {
                Debug.LogError("[UpdateScheduler] Cannot schedule null update action");
                return;
            }

            var scheduledUpdate = new ScheduledUpdate(updateAction, interval, priority, target, debugName);
            scheduledUpdates.Add(scheduledUpdate);

            if (showDebugInfo)
                Debug.Log($"[UpdateScheduler] Scheduled update: {debugName} (Priority: {priority}, Interval: {interval}s)");
        }

        public void UnscheduleUpdate(Action updateAction)
        {
            scheduledUpdates.RemoveAll(update => update.updateAction == updateAction);
        }

        public void PauseUpdate(Action updateAction, bool pause = true)
        {
            var update = scheduledUpdates.Find(u => u.updateAction == updateAction);
            if (update != null)
            {
                update.isPaused = pause;
                
                if (showDebugInfo)
                    Debug.Log($"[UpdateScheduler] {(pause ? "Paused" : "Resumed")} update: {update.debugName}");
            }
        }

        public void PauseAllUpdates(bool pause = true)
        {
            foreach (var update in scheduledUpdates)
            {
                update.isPaused = pause;
            }
            
            if (showDebugInfo)
                Debug.Log($"[UpdateScheduler] {(pause ? "Paused" : "Resumed")} all updates");
        }

        public void SetUpdateInterval(Action updateAction, float newInterval)
        {
            var update = scheduledUpdates.Find(u => u.updateAction == updateAction);
            if (update != null)
            {
                update.interval = newInterval;
                
                if (showDebugInfo)
                    Debug.Log($"[UpdateScheduler] Changed interval for {update.debugName} to {newInterval}s");
            }
        }

        public void ChangeUpdatePriority(Action updateAction, UpdatePriority newPriority)
        {
            var update = scheduledUpdates.Find(u => u.updateAction == updateAction);
            if (update != null)
            {
                update.priority = newPriority;
                
                if (showDebugInfo)
                    Debug.Log($"[UpdateScheduler] Changed priority for {update.debugName} to {newPriority}");
            }
        }

        public void ClearAllUpdates()
        {
            scheduledUpdates.Clear();
            updateQueue.Clear();
            
            if (showDebugInfo)
                Debug.Log("[UpdateScheduler] Cleared all scheduled updates");
        }
        #endregion

        #region Performance Monitoring
        private void MonitorFramePerformance()
        {
            float frameTime = (Time.realtimeSinceStartup - frameStartTime) * 1000f;
            
            // Check for performance warnings
            if (logPerformanceWarnings && frameTime > performanceWarningThreshold)
            {
                OnPerformanceWarning?.Invoke(frameTime);
                Debug.LogWarning($"[UpdateScheduler] Frame time exceeded threshold: {frameTime:F2}ms (Target: {performanceWarningThreshold:F2}ms)");
            }
        }

        private void UpdatePerformanceStats()
        {
            var stats = new PerformanceStats
            {
                scheduledUpdatesCount = scheduledUpdates.Count,
                updatesExecutedThisSecond = totalUpdatesPerSecond,
                averageFrameTime = Time.deltaTime * 1000f,
                currentFPS = 1f / Time.deltaTime
            };

            OnPerformanceStats?.Invoke(stats);
            
            // Reset counters
            totalUpdatesPerSecond = 0;
            totalUpdateTime = 0f;
        }
        #endregion

        #region Utility Methods
        public int GetScheduledUpdatesCount() => scheduledUpdates.Count;
        public int GetActiveUpdatesCount() => scheduledUpdates.FindAll(u => !u.isPaused).Count;
        
        public List<string> GetUpdateDebugInfo()
        {
            var debugInfo = new List<string>();
            foreach (var update in scheduledUpdates)
            {
                string status = update.isPaused ? "PAUSED" : "ACTIVE";
                float timeUntilNext = Mathf.Max(0, update.nextUpdateTime - Time.time);
                debugInfo.Add($"{update.debugName} [{update.priority}] - {status} (Next: {timeUntilNext:F1}s)");
            }
            return debugInfo;
        }

        public void SetOptimizationSettings(bool enableOptimization, float distance, int maxUpdates)
        {
            enableDistanceOptimization = enableOptimization;
            optimizationDistance = distance;
            maxUpdatesPerFrame = maxUpdates;
            
            if (showDebugInfo)
                Debug.Log($"[UpdateScheduler] Updated optimization settings: Enable={enableOptimization}, Distance={distance}, MaxUpdates={maxUpdates}");
        }
        #endregion

        #region Debug Visualization
        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"<b>UpdateScheduler Debug</b>");
            GUILayout.Label($"Scheduled Updates: {scheduledUpdates.Count}");
            GUILayout.Label($"Updates This Frame: {updatesThisFrame}");
            GUILayout.Label($"Frame Time: {Time.deltaTime * 1000f:F1}ms");
            GUILayout.Label($"FPS: {1f / Time.deltaTime:F0}");
            
            if (playerCamera != null)
                GUILayout.Label($"Camera Distance Optimization: {enableDistanceOptimization}");
            
            GUILayout.EndArea();
        }
        #endregion
    }

    #region Data Structures
    [System.Serializable]
    public struct PerformanceStats
    {
        public int scheduledUpdatesCount;
        public int updatesExecutedThisSecond;
        public float averageFrameTime;
        public float currentFPS;
    }
    #endregion
}