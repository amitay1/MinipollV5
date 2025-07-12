using UnityEngine;
using System.Collections.Generic;
using MinipollGame.Managers;

namespace MinipollGame.Effects
{
    /// <summary>
    /// Particle system pool manager for VFX management
    /// </summary>
    public class ParticlePoolManager : MonoBehaviour
    {
        [Header("Pool Settings")]
        public int poolSize = 30;
        public ParticleSystem particleSystemPrefab;
        
        [Header("Particle Effects Library")]
        public List<ParticleEffectData> particleEffects = new List<ParticleEffectData>();
        
        [Header("Settings")]
        public bool autoReturnToPool = true;
        public float maxParticleLifetime = 30f;
        public bool preWarmEffects = false;
        
        private Queue<ParticleSystem> availableParticleSystems = new Queue<ParticleSystem>();
        private List<ParticleSystem> activeParticleSystems = new List<ParticleSystem>();
        private Dictionary<string, ParticleSystem> effectPrefabs = new Dictionary<string, ParticleSystem>();
        // Remove ObjectPoolManager dependency for now
        // private ObjectPoolManager objectPoolManager;

        [System.Serializable]
        public class ParticleEffectData
        {
            public string name;
            public ParticleSystem prefab;
            public bool isLooping = false;
            public float customDuration = -1f; // -1 means use system's duration
            public bool attachToTransform = false;
            public Vector3 localOffset = Vector3.zero;
        }

        private void Awake()
        {
            // objectPoolManager = FindObjectOfType<ObjectPoolManager>();
            InitializePool();
            BuildEffectDictionary();
        }

        private void Start()
        {
            if (preWarmEffects)
                PreWarmEffects();
        }

        /// <summary>
        /// Initialize the particle system pool
        /// </summary>
        private void InitializePool()
        {
            Transform poolParent = transform;
            
            for (int i = 0; i < poolSize; i++)
            {
                ParticleSystem particleSystem;
                
                if (particleSystemPrefab != null)
                {
                    particleSystem = Instantiate(particleSystemPrefab, poolParent);
                }
                else
                {
                    // Create a basic particle system if no prefab is provided
                    GameObject particleObject = new GameObject($"Particle_{i}");
                    particleObject.transform.SetParent(poolParent);
                    particleSystem = particleObject.AddComponent<ParticleSystem>();
                    
                    // Setup default particle system settings
                    var main = particleSystem.main;
                    main.startLifetime = 2f;
                    main.startSpeed = 5f;
                    main.maxParticles = 100;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                }
                
                particleSystem.gameObject.SetActive(false);
                availableParticleSystems.Enqueue(particleSystem);
            }
        }

        /// <summary>
        /// Build dictionary for quick effect lookup
        /// </summary>
        private void BuildEffectDictionary()
        {
            effectPrefabs.Clear();
            
            foreach (var effectData in particleEffects)
            {
                if (effectData.prefab != null && !string.IsNullOrEmpty(effectData.name))
                {
                    if (!effectPrefabs.ContainsKey(effectData.name))
                    {
                        effectPrefabs.Add(effectData.name, effectData.prefab);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate particle effect name: {effectData.name}");
                    }
                }
            }
        }

        /// <summary>
        /// Pre-warm effects for better performance
        /// </summary>
        private void PreWarmEffects()
        {
            foreach (var effectData in particleEffects)
            {
                if (effectData.prefab != null)
                {
                    // Temporarily instantiate and pre-warm
                    ParticleSystem tempPS = Instantiate(effectData.prefab);
                    tempPS.Simulate(2f, true, true);
                    Destroy(tempPS.gameObject);
                }
            }
        }

        /// <summary>
        /// Play a particle effect by name
        /// </summary>
        public ParticleSystem PlayEffect(string effectName, Vector3 position, Quaternion rotation = default)
        {
            if (!effectPrefabs.ContainsKey(effectName))
            {
                Debug.LogWarning($"Particle effect '{effectName}' not found in library!");
                return null;
            }

            return PlayEffect(effectPrefabs[effectName], position, rotation);
        }

        /// <summary>
        /// Play a particle effect with a prefab
        /// </summary>
        public ParticleSystem PlayEffect(ParticleSystem prefab, Vector3 position, Quaternion rotation = default)
        {
            if (prefab == null)
            {
                Debug.LogWarning("Attempted to play null particle effect!");
                return null;
            }

            ParticleSystem particleSystem = GetParticleSystem();
            if (particleSystem == null)
            {
                Debug.LogWarning("No available particle systems in pool!");
                return null;
            }

            // Copy settings from prefab
            CopyParticleSystemSettings(prefab, particleSystem);
            
            // Set position and rotation
            particleSystem.transform.position = position;
            particleSystem.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            
            // Play the effect
            particleSystem.Play();

            // Handle auto-return to pool
            if (autoReturnToPool)
            {
                ParticleEffectData effectData = GetEffectData(prefab);
                float duration = effectData?.customDuration ?? GetParticleSystemDuration(particleSystem);
                StartCoroutine(ReturnToPoolAfterPlaying(particleSystem, duration));
            }

            return particleSystem;
        }

        /// <summary>
        /// Play effect attached to a transform
        /// </summary>
        public ParticleSystem PlayEffectAttached(string effectName, Transform parent, Vector3 localOffset = default)
        {
            ParticleSystem particleSystem = PlayEffect(effectName, parent.position + localOffset, parent.rotation);
            
            if (particleSystem != null)
            {
                particleSystem.transform.SetParent(parent);
                particleSystem.transform.localPosition = localOffset;
                
                ParticleEffectData effectData = GetEffectDataByName(effectName);
                if (effectData != null)
                {
                    particleSystem.transform.localPosition = effectData.localOffset;
                }
            }
            
            return particleSystem;
        }

        /// <summary>
        /// Stop a particle effect and return it to the pool
        /// </summary>
        public void StopAndReturnToPool(ParticleSystem particleSystem)
        {
            if (particleSystem != null)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ReturnToPool(particleSystem);
            }
        }

        /// <summary>
        /// Stop all active particle effects
        /// </summary>
        public void StopAllEffects()
        {
            foreach (var particleSystem in activeParticleSystems.ToArray())
            {
                StopAndReturnToPool(particleSystem);
            }
        }

        /// <summary>
        /// Pause all active particle effects
        /// </summary>
        public void PauseAllEffects()
        {
            foreach (var particleSystem in activeParticleSystems)
            {
                particleSystem.Pause();
            }
        }

        /// <summary>
        /// Resume all paused particle effects
        /// </summary>
        public void ResumeAllEffects()
        {
            foreach (var particleSystem in activeParticleSystems)
            {
                particleSystem.Play();
            }
        }

        /// <summary>
        /// Get an available particle system from the pool
        /// </summary>
        private ParticleSystem GetParticleSystem()
        {
            ParticleSystem particleSystem;

            if (availableParticleSystems.Count > 0)
            {
                particleSystem = availableParticleSystems.Dequeue();
            }
            else
            {
                // Pool exhausted, try to reclaim an inactive system
                particleSystem = FindInactiveParticleSystem();
                
                if (particleSystem == null)
                {
                    // Create a new one if absolutely necessary
                    return CreateNewParticleSystem();
                }
            }

            particleSystem.gameObject.SetActive(true);
            activeParticleSystems.Add(particleSystem);
            return particleSystem;
        }

        /// <summary>
        /// Return a particle system to the pool
        /// </summary>
        private void ReturnToPool(ParticleSystem particleSystem)
        {
            if (particleSystem == null) return;

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Clear();
            particleSystem.transform.SetParent(transform);
            particleSystem.gameObject.SetActive(false);
            
            if (activeParticleSystems.Contains(particleSystem))
            {
                activeParticleSystems.Remove(particleSystem);
                availableParticleSystems.Enqueue(particleSystem);
            }
        }

        /// <summary>
        /// Copy settings from one particle system to another
        /// </summary>
        private void CopyParticleSystemSettings(ParticleSystem source, ParticleSystem target)
        {
            // Copy main module
            var sourceMain = source.main;
            var targetMain = target.main;
            
            targetMain.startLifetime = sourceMain.startLifetime;
            targetMain.startSpeed = sourceMain.startSpeed;
            targetMain.startSize = sourceMain.startSize;
            targetMain.startColor = sourceMain.startColor;
            targetMain.startRotation = sourceMain.startRotation;
            targetMain.startDelay = sourceMain.startDelay;
            targetMain.loop = sourceMain.loop;
            targetMain.prewarm = sourceMain.prewarm;
            targetMain.duration = sourceMain.duration;
            targetMain.maxParticles = sourceMain.maxParticles;
            targetMain.simulationSpace = sourceMain.simulationSpace;
            targetMain.scalingMode = sourceMain.scalingMode;
            
            // Copy emission module
            if (source.emission.enabled)
            {
                var sourceEmission = source.emission;
                var targetEmission = target.emission;
                targetEmission.enabled = true;
                targetEmission.rateOverTime = sourceEmission.rateOverTime;
                targetEmission.rateOverDistance = sourceEmission.rateOverDistance;
            }
            
            // Copy shape module
            if (source.shape.enabled)
            {
                var sourceShape = source.shape;
                var targetShape = target.shape;
                targetShape.enabled = true;
                targetShape.shapeType = sourceShape.shapeType;
                targetShape.radius = sourceShape.radius;
                targetShape.angle = sourceShape.angle;
            }
            
            // Copy velocity over lifetime
            if (source.velocityOverLifetime.enabled)
            {
                var sourceVel = source.velocityOverLifetime;
                var targetVel = target.velocityOverLifetime;
                targetVel.enabled = true;
                targetVel.space = sourceVel.space;
                targetVel.x = sourceVel.x;
                targetVel.y = sourceVel.y;
                targetVel.z = sourceVel.z;
            }
            
            // Copy renderer module
            var sourceRenderer = source.GetComponent<ParticleSystemRenderer>();
            var targetRenderer = target.GetComponent<ParticleSystemRenderer>();
            if (sourceRenderer != null && targetRenderer != null)
            {
                targetRenderer.material = sourceRenderer.material;
                targetRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                targetRenderer.sortingOrder = sourceRenderer.sortingOrder;
            }
        }

        /// <summary>
        /// Get effect data by particle system prefab
        /// </summary>
        private ParticleEffectData GetEffectData(ParticleSystem prefab)
        {
            return particleEffects.Find(data => data.prefab == prefab);
        }

        /// <summary>
        /// Get effect data by name
        /// </summary>
        private ParticleEffectData GetEffectDataByName(string name)
        {
            return particleEffects.Find(data => data.name == name);
        }

        /// <summary>
        /// Calculate particle system duration
        /// </summary>
        private float GetParticleSystemDuration(ParticleSystem ps)
        {
            var main = ps.main;
            if (main.loop)
                return maxParticleLifetime;
            
            return main.duration + main.startLifetime.constantMax;
        }

        /// <summary>
        /// Find an inactive particle system for reuse
        /// </summary>
        private ParticleSystem FindInactiveParticleSystem()
        {
            for (int i = activeParticleSystems.Count - 1; i >= 0; i--)
            {
                var ps = activeParticleSystems[i];
                if (!ps.isPlaying && !ps.isPaused)
                {
                    activeParticleSystems.RemoveAt(i);
                    return ps;
                }
            }
            return null;
        }

        /// <summary>
        /// Create a new particle system when pool is exhausted
        /// </summary>
        private ParticleSystem CreateNewParticleSystem()
        {
            GameObject particleObject = new GameObject($"Particle_Extra_{Time.time}");
            particleObject.transform.SetParent(transform);
            ParticleSystem ps = particleObject.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 5f;
            main.maxParticles = 100;
            
            return ps;
        }

        /// <summary>
        /// Coroutine to return particle system to pool after playing
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolAfterPlaying(ParticleSystem particleSystem, float duration)
        {
            yield return new WaitForSeconds(duration);
            
            // Wait for all particles to finish
            while (particleSystem.IsAlive(true))
            {
                yield return null;
            }
            
            ReturnToPool(particleSystem);
        }

        /// <summary>
        /// Add a new effect to the library at runtime
        /// </summary>
        public void AddEffectToLibrary(string name, ParticleSystem prefab, bool isLooping = false)
        {
            ParticleEffectData newEffectData = new ParticleEffectData
            {
                name = name,
                prefab = prefab,
                isLooping = isLooping
            };
            
            particleEffects.Add(newEffectData);
            
            if (!effectPrefabs.ContainsKey(name))
            {
                effectPrefabs.Add(name, prefab);
            }
        }

        /// <summary>
        /// Get all active particle system count
        /// </summary>
        public int GetActiveParticleSystemCount()
        {
            return activeParticleSystems.Count;
        }

        /// <summary>
        /// Get available particle system count
        /// </summary>
        public int GetAvailableParticleSystemCount()
        {
            return availableParticleSystems.Count;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                BuildEffectDictionary();
            }
        }
    }
}
