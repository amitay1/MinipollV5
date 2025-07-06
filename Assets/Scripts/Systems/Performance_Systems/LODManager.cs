using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LODManager - מנהל Level of Detail לכל האובייקטים במשחק
/// מפחית פרטים של אובייקטים רחוקים לביצועים טובים יותר
/// </summary>
namespace MinipollGame.Performance
{
    public class LODManager : MonoBehaviour
    {
        #region Singleton
        public static LODManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLODManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Settings
        [Header("=== LOD Distance Settings ===")]
        [SerializeField] private float[] lodDistances = { 15f, 30f, 60f, 100f };
        [SerializeField] private float updateInterval = 0.2f;
        [SerializeField] private int maxLODUpdatesPerFrame = 20;

        [Header("=== Quality Settings ===")]
        [SerializeField] private bool enableAnimationLOD = true;
        [SerializeField] private bool enableAudioLOD = true;
        [SerializeField] private bool enableParticleLOD = true;
        [SerializeField] private bool enableShadowLOD = true;

        [Header("=== Camera Settings ===")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool useMainCameraIfNull = true;
        [SerializeField] private float cameraVelocityFactor = 1f;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool showLODGizmos = false;
        [SerializeField] private bool logLODChanges = false;
        #endregion

        #region Data Structures
        public enum LODLevel
        {
            LOD0 = 0,   // Highest quality (closest)
            LOD1 = 1,   // High quality  
            LOD2 = 2,   // Medium quality
            LOD3 = 3,   // Low quality
            LOD4 = 4,   // Lowest quality (furthest)
            Culled = 5  // Not visible/disabled
        }

        [System.Serializable]
        public class LODObject
        {
            public GameObject gameObject;
            public Transform transform;
            public float distance;
            public LODLevel currentLOD;
            public LODLevel targetLOD;
            public LODSettings settings;
            public bool isVisible;
            public float lastUpdateTime;

            // Components for optimization
            public Renderer[] renderers;
            public Animator animator;
            public AudioSource audioSource;
            public ParticleSystem[] particleSystems;
            public MinipollGame.Core.MinipollBrain minipollBrain; // Special handling for Minipolls

            public LODObject(GameObject obj, LODSettings lodSettings)
            {
                gameObject = obj;
                transform = obj.transform;
                settings = lodSettings;
                currentLOD = LODLevel.LOD0;
                targetLOD = LODLevel.LOD0;
                isVisible = true;
                lastUpdateTime = Time.time;

                // Cache components
                CacheComponents();
            }

            private void CacheComponents()
            {
                renderers = gameObject.GetComponentsInChildren<Renderer>();
                animator = gameObject.GetComponent<Animator>();
                audioSource = gameObject.GetComponent<AudioSource>();
                particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
                minipollBrain = gameObject.GetComponent<MinipollGame.Core.MinipollBrain>();
            }

            public void UpdateLOD(LODLevel newLOD, LODManager manager)
            {
                if (currentLOD == newLOD) return;

                var oldLOD = currentLOD;
                currentLOD = newLOD;
                lastUpdateTime = Time.time;

                // Apply LOD changes
                ApplyVisibilityLOD(manager);
                ApplyAnimationLOD(manager);
                ApplyAudioLOD(manager);
                ApplyParticleLOD(manager);
                ApplyShadowLOD(manager);
                ApplyMinipollSpecificLOD(manager);

                if (manager.logLODChanges)
                {
                    Debug.Log($"[LODManager] {gameObject.name}: LOD {oldLOD} → {newLOD} (Distance: {distance:F1}m)");
                }
            }

            private void ApplyVisibilityLOD(LODManager manager)
            {
                bool shouldBeVisible = currentLOD != LODLevel.Culled;
                
                if (isVisible != shouldBeVisible)
                {
                    isVisible = shouldBeVisible;
                    
                    foreach (var renderer in renderers)
                    {
                        if (renderer != null)
                            renderer.enabled = shouldBeVisible;
                    }
                }
            }

            private void ApplyAnimationLOD(LODManager manager)
            {
                if (!manager.enableAnimationLOD || animator == null) return;

                switch (currentLOD)
                {
                    case LODLevel.LOD0:
                    case LODLevel.LOD1:
                        animator.enabled = true;
                        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                        break;
                    case LODLevel.LOD2:
                        animator.enabled = true;
                        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                        break;
                    case LODLevel.LOD3:
                        animator.enabled = true;
                        animator.cullingMode = AnimatorCullingMode.CullCompletely;
                        break;
                    case LODLevel.LOD4:
                    case LODLevel.Culled:
                        animator.enabled = false;
                        break;
                }
            }

            private void ApplyAudioLOD(LODManager manager)
            {
                if (!manager.enableAudioLOD || audioSource == null) return;

                switch (currentLOD)
                {
                    case LODLevel.LOD0:
                        audioSource.volume = 1f;
                        audioSource.enabled = true;
                        break;
                    case LODLevel.LOD1:
                        audioSource.volume = 0.8f;
                        audioSource.enabled = true;
                        break;
                    case LODLevel.LOD2:
                        audioSource.volume = 0.5f;
                        audioSource.enabled = true;
                        break;
                    case LODLevel.LOD3:
                        audioSource.volume = 0.2f;
                        audioSource.enabled = true;
                        break;
                    case LODLevel.LOD4:
                    case LODLevel.Culled:
                        audioSource.enabled = false;
                        break;
                }
            }

            private void ApplyParticleLOD(LODManager manager)
            {
                if (!manager.enableParticleLOD) return;

                foreach (var particles in particleSystems)
                {
                    if (particles == null) continue;

                    var main = particles.main;
                    
                    switch (currentLOD)
                    {
                        case LODLevel.LOD0:
                            main.maxParticles = (int)(settings.maxParticles * 1f);
                            particles.gameObject.SetActive(true);
                            break;
                        case LODLevel.LOD1:
                            main.maxParticles = (int)(settings.maxParticles * 0.7f);
                            particles.gameObject.SetActive(true);
                            break;
                        case LODLevel.LOD2:
                            main.maxParticles = (int)(settings.maxParticles * 0.4f);
                            particles.gameObject.SetActive(true);
                            break;
                        case LODLevel.LOD3:
                            main.maxParticles = (int)(settings.maxParticles * 0.2f);
                            particles.gameObject.SetActive(true);
                            break;
                        case LODLevel.LOD4:
                        case LODLevel.Culled:
                            particles.gameObject.SetActive(false);
                            break;
                    }
                }
            }

            private void ApplyShadowLOD(LODManager manager)
            {
                if (!manager.enableShadowLOD) return;

                foreach (var renderer in renderers)
                {
                    if (renderer == null) continue;

                    switch (currentLOD)
                    {
                        case LODLevel.LOD0:
                        case LODLevel.LOD1:
                            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                            renderer.receiveShadows = true;
                            break;
                        case LODLevel.LOD2:
                            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                            renderer.receiveShadows = false;
                            break;
                        case LODLevel.LOD3:
                            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                            renderer.receiveShadows = false;
                            break;
                        case LODLevel.LOD4:
                        case LODLevel.Culled:
                            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            renderer.receiveShadows = false;
                            break;
                    }
                }
            }

            private void ApplyMinipollSpecificLOD(LODManager manager)
            {
                if (minipollBrain == null) return;

                // Special handling for Minipoll behavior based on LOD
                switch (currentLOD)
                {
                    case LODLevel.LOD0:
                    case LODLevel.LOD1:
                        // Full AI and behavior
                        break;
                    case LODLevel.LOD2:
                        // Reduced AI update frequency (handled by UpdateScheduler)
                        break;
                    case LODLevel.LOD3:
                        // Minimal AI updates
                        break;
                    case LODLevel.LOD4:
                    case LODLevel.Culled:
                        // Pause AI completely
                        break;
                }
            }
        }

        [System.Serializable]
        public class LODSettings
        {
            public bool enableLOD = true;
            public float cullDistance = 150f;
            public float biasMultiplier = 1f;
            public int maxParticles = 100;
            public bool forceHighQuality = false; // For important objects

            public static LODSettings Default => new LODSettings();
            
            public static LODSettings Minipoll => new LODSettings
            {
                enableLOD = true,
                cullDistance = 100f,
                biasMultiplier = 0.8f, // Keep visible longer
                maxParticles = 50
            };

            public static LODSettings Environment => new LODSettings
            {
                enableLOD = true,
                cullDistance = 200f,
                biasMultiplier = 1.2f,
                maxParticles = 20
            };

            public static LODSettings Effects => new LODSettings
            {
                enableLOD = true,
                cullDistance = 80f,
                biasMultiplier = 1.5f,
                maxParticles = 30
            };
        }
        #endregion

        #region Internal Data
        private List<LODObject> lodObjects = new List<LODObject>();
        private Dictionary<GameObject, LODObject> objectToLOD = new Dictionary<GameObject, LODObject>();
        
        private int updateIndex = 0;
        private float lastUpdateTime;
        
        // Statistics
        private int[] lodCounts = new int[6]; // Count for each LOD level
        private int totalLODChanges = 0;
        #endregion

        #region Events
        public static event Action<GameObject, LODLevel, LODLevel> OnLODChanged;
        public static event Action<LODStatistics> OnLODStatisticsUpdated;
        #endregion

        #region Initialization
        private void InitializeLODManager()
        {
            // Get camera reference
            if (targetCamera == null && useMainCameraIfNull)
                targetCamera = Camera.main;

            if (targetCamera == null)
                Debug.LogWarning("[LODManager] No camera assigned! LOD calculations may not work correctly.");

            // Register existing objects
            RegisterExistingObjects();

            // Schedule statistics updates
            if (UpdateScheduler.Instance != null)
            {
                UpdateScheduler.Instance.ScheduleUpdate(UpdateStatistics, 2f, UpdateScheduler.UpdatePriority.Low, null, "LODStatistics");
            }

            if (showDebugInfo)
                Debug.Log($"[LODManager] Initialized with {lodObjects.Count} LOD objects");
        }

        private void RegisterExistingObjects()
        {
            // Auto-register Minipolls
            var minipolls = FindObjectsByType<MinipollGame.Core.MinipollBrain>(FindObjectsSortMode.None);
            foreach (var minipoll in minipolls)
            {
                RegisterObject(minipoll.gameObject, LODSettings.Minipoll);
            }

            // Auto-register objects with specific tags
            RegisterObjectsByTag("Environment", LODSettings.Environment);
            RegisterObjectsByTag("Effects", LODSettings.Effects);

            if (showDebugInfo)
                Debug.Log($"[LODManager] Auto-registered {minipolls.Length} Minipolls and tagged objects");
        }        private void RegisterObjectsByTag(string tag, LODSettings settings)
        {
            try
            {
                var objects = GameObject.FindGameObjectsWithTag(tag);
                foreach (var obj in objects)
                {
                    RegisterObject(obj, settings);
                }
                
                if (showDebugInfo && objects.Length > 0)
                    Debug.Log($"[LODManager] Registered {objects.Length} objects with tag '{tag}'");
            }
            catch (UnityException ex)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[LODManager] Tag '{tag}' is not defined in Tag Manager. Skipping auto-registration. Exception: {ex.Message}");
            }
        }
        #endregion

        #region Update Loop
        private void Update()
        {
            if (Time.time - lastUpdateTime < updateInterval) return;

            UpdateLODObjects();
            lastUpdateTime = Time.time;
        }

        private void UpdateLODObjects()
        {
            if (targetCamera == null || lodObjects.Count == 0) return;

            Vector3 cameraPosition = targetCamera.transform.position;
            int objectsToUpdate = Mathf.Min(maxLODUpdatesPerFrame, lodObjects.Count);

            for (int i = 0; i < objectsToUpdate; i++)
            {
                int index = (updateIndex + i) % lodObjects.Count;
                var lodObject = lodObjects[index];

                if (lodObject.gameObject == null)
                {
                    // Remove destroyed objects
                    lodObjects.RemoveAt(index);
                    continue;
                }

                // Calculate distance with bias
                float distance = Vector3.Distance(cameraPosition, lodObject.transform.position);
                distance *= lodObject.settings.biasMultiplier;
                lodObject.distance = distance;

                // Determine target LOD level
                LODLevel targetLOD = CalculateLODLevel(distance, lodObject.settings);

                // Update LOD if changed
                if (lodObject.currentLOD != targetLOD)
                {
                    var oldLOD = lodObject.currentLOD;
                    lodObject.UpdateLOD(targetLOD, this);
                    OnLODChanged?.Invoke(lodObject.gameObject, oldLOD, targetLOD);
                    totalLODChanges++;
                }
            }

            updateIndex = (updateIndex + objectsToUpdate) % lodObjects.Count;
        }

        private LODLevel CalculateLODLevel(float distance, LODSettings settings)
        {
            if (!settings.enableLOD || settings.forceHighQuality)
                return LODLevel.LOD0;

            if (distance >= settings.cullDistance)
                return LODLevel.Culled;

            // Calculate LOD based on distance thresholds
            for (int i = 0; i < lodDistances.Length; i++)
            {
                if (distance <= lodDistances[i])
                    return (LODLevel)i;
            }

            return LODLevel.LOD4; // Furthest visible LOD
        }
        #endregion

        #region Public API
        public void RegisterObject(GameObject obj, LODSettings settings = null)
        {
            if (obj == null || objectToLOD.ContainsKey(obj)) return;

            if (settings == null) settings = LODSettings.Default;

            var lodObject = new LODObject(obj, settings);
            lodObjects.Add(lodObject);
            objectToLOD[obj] = lodObject;

            if (showDebugInfo)
                Debug.Log($"[LODManager] Registered LOD object: {obj.name}");
        }

        public void UnregisterObject(GameObject obj)
        {
            if (obj == null || !objectToLOD.ContainsKey(obj)) return;

            var lodObject = objectToLOD[obj];
            lodObjects.Remove(lodObject);
            objectToLOD.Remove(obj);

            if (showDebugInfo)
                Debug.Log($"[LODManager] Unregistered LOD object: {obj.name}");
        }

        public void SetObjectLODSettings(GameObject obj, LODSettings settings)
        {
            if (objectToLOD.ContainsKey(obj))
            {
                objectToLOD[obj].settings = settings;
            }
        }

        public void ForceObjectLOD(GameObject obj, LODLevel lodLevel)
        {
            if (objectToLOD.ContainsKey(obj))
            {
                objectToLOD[obj].UpdateLOD(lodLevel, this);
            }
        }

        public LODLevel GetObjectLOD(GameObject obj)
        {
            return objectToLOD.ContainsKey(obj) ? objectToLOD[obj].currentLOD : LODLevel.LOD0;
        }

        public void SetLODDistances(float[] distances)
        {
            if (distances != null && distances.Length > 0)
            {
                lodDistances = distances;
                if (showDebugInfo)
                    Debug.Log($"[LODManager] Updated LOD distances: [{string.Join(", ", distances)}]");
            }
        }

        public void SetCamera(Camera newCamera)
        {
            targetCamera = newCamera;
            if (showDebugInfo)
                Debug.Log($"[LODManager] Camera changed to: {(newCamera ? newCamera.name : "null")}");
        }
        #endregion

        #region Statistics
        private void UpdateStatistics()
        {
            // Reset counters
            for (int i = 0; i < lodCounts.Length; i++)
                lodCounts[i] = 0;

            // Count objects at each LOD level
            foreach (var lodObject in lodObjects)
            {
                if (lodObject.gameObject != null)
                    lodCounts[(int)lodObject.currentLOD]++;
            }

            var stats = new LODStatistics
            {
                totalObjects = lodObjects.Count,
                lod0Count = lodCounts[0],
                lod1Count = lodCounts[1],
                lod2Count = lodCounts[2],
                lod3Count = lodCounts[3],
                lod4Count = lodCounts[4],
                culledCount = lodCounts[5],
                totalLODChanges = totalLODChanges
            };

            OnLODStatisticsUpdated?.Invoke(stats);
        }

        public LODStatistics GetCurrentStatistics()
        {
            UpdateStatistics();
            return new LODStatistics
            {
                totalObjects = lodObjects.Count,
                lod0Count = lodCounts[0],
                lod1Count = lodCounts[1],
                lod2Count = lodCounts[2],
                lod3Count = lodCounts[3],
                lod4Count = lodCounts[4],
                culledCount = lodCounts[5],
                totalLODChanges = totalLODChanges
            };
        }
        #endregion

        #region Debug Visualization
        private void OnDrawGizmosSelected()
        {
            if (!showLODGizmos || targetCamera == null) return;

            Vector3 cameraPos = targetCamera.transform.position;

            // Draw LOD distance rings
            for (int i = 0; i < lodDistances.Length; i++)
            {
                Gizmos.color = GetLODColor((LODLevel)i);
                Gizmos.DrawWireSphere(cameraPos, lodDistances[i]);
            }

            // Draw LOD objects
            foreach (var lodObject in lodObjects)
            {
                if (lodObject.gameObject == null) continue;

                Gizmos.color = GetLODColor(lodObject.currentLOD);
                Gizmos.DrawWireCube(lodObject.transform.position, Vector3.one * 2f);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(lodObject.transform.position + Vector3.up * 3f,
                    $"LOD{(int)lodObject.currentLOD}\n{lodObject.distance:F1}m");
#endif
            }
        }

        private Color GetLODColor(LODLevel lod)
        {
            return lod switch
            {
                LODLevel.LOD0 => Color.green,
                LODLevel.LOD1 => Color.yellow,
                LODLevel.LOD2 => Color.orange,
                LODLevel.LOD3 => Color.red,
                LODLevel.LOD4 => Color.magenta,
                LODLevel.Culled => Color.gray,
                _ => Color.white
            };
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - 250, Screen.height - 200, 240, 190));
            GUILayout.Label("<b>LOD Manager Debug</b>");
            GUILayout.Label($"Total Objects: {lodObjects.Count}");
            
            for (int i = 0; i < lodCounts.Length - 1; i++)
            {
                GUILayout.Label($"LOD{i}: {lodCounts[i]}");
            }
            GUILayout.Label($"Culled: {lodCounts[5]}");
            GUILayout.Label($"LOD Changes: {totalLODChanges}");
            
            GUILayout.EndArea();
        }
        #endregion
    }

    #region Data Structures
    [System.Serializable]
    public struct LODStatistics
    {
        public int totalObjects;
        public int lod0Count;
        public int lod1Count;
        public int lod2Count;
        public int lod3Count;
        public int lod4Count;
        public int culledCount;
        public int totalLODChanges;
    }
    #endregion
}