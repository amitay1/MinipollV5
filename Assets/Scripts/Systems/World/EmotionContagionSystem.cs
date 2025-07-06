using System;
using System.Collections.Generic;
using System.Linq;
using MinipollCore.core;
using MinipollGame.Managers;
using MinipollGame.Social;
using MinipollGame.Core;
using UnityEngine;

namespace MinipollCore.Systems.Social
{
    /// <summary>
    /// Manages emotional contagion between Minipolls - how emotions spread through groups
    /// Based on research in emotional contagion and crowd psychology
    /// </summary>
    public class EmotionContagionSystem : MonoBehaviour
    {
        private static EmotionContagionSystem _instance;
        public static EmotionContagionSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<EmotionContagionSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("EmotionContagionSystem");
                        _instance = go.AddComponent<EmotionContagionSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Contagion Settings")]
        [SerializeField] private float baseContagionRadius = 5f;
        [SerializeField] private float contagionUpdateInterval = 0.5f;
        [SerializeField] private AnimationCurve distanceFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Emotion Spread Parameters")]
        [SerializeField] private float emotionTransferRate = 0.3f;
        [SerializeField] private float emotionDecayRate = 0.1f;
        [SerializeField] private float minEmotionThreshold = 0.1f;
        [SerializeField] private float crowdAmplificationFactor = 1.2f;
        
        [Header("Social Factors")]
        [SerializeField] private float relationshipInfluence = 0.5f;
        [SerializeField] private float personalityResistance = 0.3f;
        [SerializeField] private float groupIdentityMultiplier = 1.5f;
        
        [Header("Visual Effects")]
        [SerializeField] private bool showEmotionWaves = true;
        [SerializeField] private GameObject emotionWavePrefab;
        [SerializeField] private Gradient emotionColorGradient;

        // Emotion types that can spread
        public enum ContagiousEmotion
        {
            Joy,
            Fear,
            Anger,
            Sadness,
            Excitement,
            Calm,
            Panic,
            Love,
            Disgust,
            Curiosity
        }

        // Emotion wave data
        [Serializable]
        public class EmotionWave
        {
            public ContagiousEmotion emotion;
            public Vector3 origin;
            public float strength;
            public float radius;
            public int sourceMinipollId;
            public float timestamp;
            public List<int> affectedMinipollIds;

            public EmotionWave()
            {
                affectedMinipollIds = new List<int>();
            }
        }

        // Emotional state tracking
        [Serializable]
        public class EmotionalInfluence
        {
            public int influencerId;
            public ContagiousEmotion emotion;
            public float strength;
            public float timestamp;
        }

        // Group emotion dynamics
        [Serializable]
        public class EmotionalGroup
        {
            public string groupId;
            public List<int> memberIds;
            public Dictionary<ContagiousEmotion, float> dominantEmotions;
            public Vector3 center;
            public float cohesion;

            public EmotionalGroup()
            {
                memberIds = new List<int>();
                dominantEmotions = new Dictionary<ContagiousEmotion, float>();
            }
        }

        // System data
        private Dictionary<int, List<EmotionalInfluence>> activeInfluences = new Dictionary<int, List<EmotionalInfluence>>();
        private List<EmotionWave> activeWaves = new List<EmotionWave>();
        private Dictionary<string, EmotionalGroup> emotionalGroups = new Dictionary<string, EmotionalGroup>();
        private Dictionary<int, Dictionary<ContagiousEmotion, float>> minipollEmotionalStates = new Dictionary<int, Dictionary<ContagiousEmotion, float>>();
          // Performance optimization
        private float lastUpdateTime;
        private HashSet<int> minipollsToUpdate = new HashSet<int>();
        private ObjectPool<EmotionWave> wavePool;
        private List<MinipollGame.Core.MinipollCore> allMinipolls = new List<MinipollGame.Core.MinipollCore>();

        // Emotion interaction rules
        private readonly Dictionary<ContagiousEmotion, List<ContagiousEmotion>> emotionOpposites = new Dictionary<ContagiousEmotion, List<ContagiousEmotion>>
        {
            { ContagiousEmotion.Joy, new List<ContagiousEmotion> { ContagiousEmotion.Sadness, ContagiousEmotion.Anger } },
            { ContagiousEmotion.Fear, new List<ContagiousEmotion> { ContagiousEmotion.Calm, ContagiousEmotion.Joy } },
            { ContagiousEmotion.Anger, new List<ContagiousEmotion> { ContagiousEmotion.Calm, ContagiousEmotion.Love } },
            { ContagiousEmotion.Calm, new List<ContagiousEmotion> { ContagiousEmotion.Panic, ContagiousEmotion.Fear } },
            { ContagiousEmotion.Love, new List<ContagiousEmotion> { ContagiousEmotion.Disgust, ContagiousEmotion.Anger } }
        };

        private readonly Dictionary<ContagiousEmotion, float> emotionContagionStrength = new Dictionary<ContagiousEmotion, float>
        {
            { ContagiousEmotion.Panic, 1.5f },      // Panic spreads very quickly
            { ContagiousEmotion.Fear, 1.3f },       // Fear spreads quickly
            { ContagiousEmotion.Excitement, 1.2f }, // Excitement is contagious
            { ContagiousEmotion.Joy, 1.0f },        // Joy spreads normally
            { ContagiousEmotion.Anger, 0.9f },      // Anger spreads moderately
            { ContagiousEmotion.Sadness, 0.7f },    // Sadness spreads slowly
            { ContagiousEmotion.Calm, 0.6f },       // Calm spreads very slowly
            { ContagiousEmotion.Love, 0.8f },       // Love spreads moderately
            { ContagiousEmotion.Disgust, 0.8f },    // Disgust spreads moderately
            { ContagiousEmotion.Curiosity, 1.1f }   // Curiosity is fairly contagious
        };

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSystem();
        }

        [Obsolete]
        private void Start()
        {
            // Subscribe to events
            if (EventSystem.Instance != null)
            {

                EventSystem.Instance.Subscribe<GameObject>("MinipollEmotionChanged", OnMinipollEmotionChanged);
                EventSystem.Instance.Subscribe<GameObject>("MinipollDied", OnMinipollDied);
                EventSystem.Instance.Subscribe<GameObject>("GroupFormed", OnGroupFormed);
            }

            InvokeRepeating(nameof(UpdateEmotionalContagion), contagionUpdateInterval, contagionUpdateInterval);
            InvokeRepeating(nameof(UpdateEmotionalGroups), 2f, 2f);
        }

        private void InitializeSystem()
        {
            wavePool = new ObjectPool<EmotionWave>(() => new EmotionWave(), 50);
        }

        #region Emotion Broadcasting

        public void BroadcastEmotion(int minipollId, ContagiousEmotion emotion, float intensity, float customRadius = -1f)
        {
            GameObject source = MinipollManager.Instance?.GetMinipollById(minipollId);
            if (source == null) return;

            // Get or create emotional state
            if (!minipollEmotionalStates.ContainsKey(minipollId))
            {
                minipollEmotionalStates[minipollId] = new Dictionary<ContagiousEmotion, float>();
            }

            // Update source's emotional state
            minipollEmotionalStates[minipollId][emotion] = Mathf.Clamp01(intensity);

            // Create emotion wave
            EmotionWave wave = wavePool.Get();
            wave.emotion = emotion;
            wave.origin = source.transform.position;
            wave.strength = intensity * GetEmotionContagionMultiplier(emotion);
            wave.radius = customRadius > 0 ? customRadius : baseContagionRadius * intensity;
            wave.sourceMinipollId = minipollId;
            wave.timestamp = Time.time;
            wave.affectedMinipollIds.Clear();

            activeWaves.Add(wave);

            // Visual effect
            if (showEmotionWaves && emotionWavePrefab != null)
            {
                CreateEmotionWaveVisual(wave);
            }

            // Immediate nearby check for strong emotions
            if (intensity > 0.7f)
            {
                PropagateEmotionImmediate(wave);
            }
        }

        private void PropagateEmotionImmediate(EmotionWave wave)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(wave.origin, wave.radius);
              foreach (var collider in nearbyColliders)
            {
                var minipoll = collider.GetComponent<MinipollGame.Core.MinipollCore>();
                if (minipoll != null && minipoll.GetInstanceID() != wave.sourceMinipollId)
                {
                    ApplyEmotionalInfluence(minipoll.GetInstanceID(), wave);
                }
            }
        }

        #endregion

        #region Contagion Mechanics

        private void UpdateEmotionalContagion()
        {
            // Update existing waves
            for (int i = activeWaves.Count - 1; i >= 0; i--)
            {
                var wave = activeWaves[i];
                
                // Decay wave strength
                wave.strength -= emotionDecayRate * contagionUpdateInterval;
                
                // Remove weak waves
                if (wave.strength < minEmotionThreshold)
                {
                    activeWaves.RemoveAt(i);
                    wavePool.Return(wave);
                    continue;
                }

                // Expand wave radius slightly over time
                wave.radius += 0.5f * contagionUpdateInterval;

                // Find minipolls in range
                Collider[] inRange = Physics.OverlapSphere(wave.origin, wave.radius);
                  foreach (var collider in inRange)
                {
                    var minipoll = collider.GetComponent<MinipollGame.Core.MinipollCore>();
                    if (minipoll != null)
                    {
                        int targetId = minipoll.GetInstanceID();
                        
                        // Skip if already affected by this wave
                        if (wave.affectedMinipollIds.Contains(targetId)) continue;
                        
                        ApplyEmotionalInfluence(targetId, wave);
                        wave.affectedMinipollIds.Add(targetId);
                    }
                }
            }

            // Process queued updates
            ProcessQueuedEmotionalUpdates();
        }

        private void ApplyEmotionalInfluence(int targetId, EmotionWave wave)
        {
            GameObject target = MinipollManager.Instance?.GetMinipollById(targetId);
            if (target == null) return;

            // Calculate influence strength
            float distance = Vector3.Distance(wave.origin, target.transform.position);
            float distanceNormalized = distance / wave.radius;
            float distanceInfluence = distanceFalloff.Evaluate(distanceNormalized);
            
            // Get social factors
            float socialMultiplier = CalculateSocialInfluence(wave.sourceMinipollId, targetId);
            
            // Get personality resistance
            float resistance = CalculatePersonalityResistance(targetId, wave.emotion);
            
            // Calculate final influence
            float influence = wave.strength * distanceInfluence * socialMultiplier * (1f - resistance) * emotionTransferRate;
            
            if (influence > minEmotionThreshold)
            {
                // Add to influences
                if (!activeInfluences.ContainsKey(targetId))
                {
                    activeInfluences[targetId] = new List<EmotionalInfluence>();
                }

                activeInfluences[targetId].Add(new EmotionalInfluence
                {
                    influencerId = wave.sourceMinipollId,
                    emotion = wave.emotion,
                    strength = influence,
                    timestamp = Time.time
                });

                minipollsToUpdate.Add(targetId);
            }
        }

        private float CalculateSocialInfluence(int sourceId, int targetId)
        {
            float baseInfluence = 1f;

            // Check relationship
            GameObject source = MinipollManager.Instance?.GetMinipollById(sourceId);
            if (source != null)
            {
                var socialRelations = source.GetComponent<MinipollSocialRelations>();
                if (socialRelations != null)
                {
                    float relationship = socialRelations.GetRelationshipValue(targetId);
                    baseInfluence += relationship * relationshipInfluence;
                }
            }

            // Check if in same group/tribe
            string sourceGroup = GetMinipollGroup(sourceId);
            string targetGroup = GetMinipollGroup(targetId);
            
            if (!string.IsNullOrEmpty(sourceGroup) && sourceGroup == targetGroup)
            {
                baseInfluence *= groupIdentityMultiplier;
            }

            return Mathf.Clamp(baseInfluence, 0.1f, 3f);
        }

        private float CalculatePersonalityResistance(int minipollId, ContagiousEmotion emotion)
        {
            // Base resistance
            float resistance = personalityResistance;

            // Could check personality traits here
            // For example, stubborn minipolls resist more, empathetic ones resist less
            
            // Check if minipoll has opposing emotions
            if (minipollEmotionalStates.ContainsKey(minipollId))
            {
                var currentEmotions = minipollEmotionalStates[minipollId];
                
                if (emotionOpposites.ContainsKey(emotion))
                {
                    foreach (var opposite in emotionOpposites[emotion])
                    {
                        if (currentEmotions.ContainsKey(opposite))
                        {
                            resistance += currentEmotions[opposite] * 0.5f;
                        }
                    }
                }
            }

            return Mathf.Clamp01(resistance);
        }

        private void ProcessQueuedEmotionalUpdates()
        {
            foreach (int minipollId in minipollsToUpdate)
            {
                if (!activeInfluences.ContainsKey(minipollId)) continue;

                var influences = activeInfluences[minipollId];
                
                // Remove old influences
                influences.RemoveAll(i => Time.time - i.timestamp > 5f);

                // Aggregate influences by emotion
                Dictionary<ContagiousEmotion, float> aggregatedInfluences = new Dictionary<ContagiousEmotion, float>();
                
                foreach (var influence in influences)
                {
                    if (!aggregatedInfluences.ContainsKey(influence.emotion))
                    {
                        aggregatedInfluences[influence.emotion] = 0f;
                    }
                    aggregatedInfluences[influence.emotion] += influence.strength;
                }

                // Apply to minipoll's emotional system
                ApplyEmotionalChanges(minipollId, aggregatedInfluences);
            }

            minipollsToUpdate.Clear();
        }

        private void ApplyEmotionalChanges(int minipollId, Dictionary<ContagiousEmotion, float> influences)
        {
            GameObject minipoll = MinipollManager.Instance?.GetMinipollById(minipollId);
            if (minipoll == null) return;

            var emotionSystem = minipoll.GetComponent<MinipollEmotionsSystem>();
            if (emotionSystem == null) return;

            // Get or create emotional state tracking
            if (!minipollEmotionalStates.ContainsKey(minipollId))
            {
                minipollEmotionalStates[minipollId] = new Dictionary<ContagiousEmotion, float>();
            }

            // Apply influences
            foreach (var kvp in influences)
            {
                ContagiousEmotion emotion = kvp.Key;
                float influence = kvp.Value;

                // Check for crowd amplification
                int nearbyWithSameEmotion = CountNearbyWithEmotion(minipoll.transform.position, emotion, baseContagionRadius);
                if (nearbyWithSameEmotion > 3)
                {
                    influence *= crowdAmplificationFactor;
                }

                // Update tracked emotional state
                if (!minipollEmotionalStates[minipollId].ContainsKey(emotion))
                {
                    minipollEmotionalStates[minipollId][emotion] = 0f;
                }
                
                float currentLevel = minipollEmotionalStates[minipollId][emotion];
                float newLevel = Mathf.Clamp01(currentLevel + influence);
                minipollEmotionalStates[minipollId][emotion] = newLevel;

                // Map to emotion system emotions
                switch (emotion)
                {
                    case ContagiousEmotion.Joy:
                        emotionSystem.AddEmotion(MinipollEmotionsSystem.EmotionType.Happy, influence);
                        break;
                    case ContagiousEmotion.Fear:
                        emotionSystem.AddEmotion(MinipollEmotionsSystem.EmotionType.Scared, influence);
                        break;
                    case ContagiousEmotion.Anger:
                        emotionSystem.AddEmotion(MinipollEmotionsSystem.EmotionType.Angry, influence);
                        break;
                    case ContagiousEmotion.Sadness:
                        emotionSystem.AddEmotion(MinipollEmotionsSystem.EmotionType.Sad, influence);
                        break;
                    case ContagiousEmotion.Love:
                        emotionSystem.AddEmotion(MinipollEmotionsSystem.EmotionType.Loving, influence);
                        break;
                    case ContagiousEmotion.Curiosity:
                        emotionSystem.AddEmotion(MinipollEmotionsSystem.EmotionType.Curious, influence);
                        break;
                }

                // Trigger memory of emotional event if strong enough
                // Commented out as MemoryManager is undefined
                // if (newLevel > 0.7f)
                // {
                //     // Memory logging removed
                // }
            }
        }

        #endregion        #region Group Emotions

        private void UpdateEmotionalGroups()
        {
            // Find clusters of minipolls with similar emotions
            var allMinipolls = FindObjectsByType<MinipollGame.Core.MinipollCore>(FindObjectsSortMode.None);
            if (allMinipolls == null || allMinipolls.Length == 0) return;
            
            // Clear old groups
            emotionalGroups.Clear();

            // Simple clustering based on proximity and dominant emotion
            HashSet<int> processed = new HashSet<int>();
            
            foreach (MinipollGame.Core.MinipollCore minipoll in allMinipolls)
            {
                if (minipoll == null || minipoll.transform == null) continue;
                
                int id = minipoll.GetInstanceID();
                if (processed.Contains(id)) continue;

                // Find dominant emotion
                var dominantEmotion = GetDominantEmotion(id);
                if (!dominantEmotion.HasValue || dominantEmotion.Value.Item2 < 0.3f) continue;

                // Create group
                string groupId = $"EmotionGroup_{dominantEmotion.Value.Item1}_{Time.time}";
                EmotionalGroup group = new EmotionalGroup
                {
                    groupId = groupId,
                    center = minipoll.transform.position
                };
                
                // Find nearby with similar emotion
                Collider[] nearby = Physics.OverlapSphere(minipoll.transform.position, baseContagionRadius * 2f);
                
                foreach (Collider collider in nearby)
                {
                    MinipollGame.Core.MinipollCore nearbyMinipoll = collider.GetComponent<MinipollGame.Core.MinipollCore>();
                    if (nearbyMinipoll != null)
                    {
                        int nearbyId = nearbyMinipoll.GetInstanceID();
                        var nearbyEmotion = GetDominantEmotion(nearbyId);
                        
                        if (nearbyEmotion.HasValue && nearbyEmotion.Value.Item1 == dominantEmotion.Value.Item1)
                        {
                            group.memberIds.Add(nearbyId);
                            processed.Add(nearbyId);
                        }
                    }
                }

                if (group.memberIds.Count >= 3)
                {
                    // Calculate group properties
                    group.dominantEmotions[dominantEmotion.Value.Item1] = dominantEmotion.Value.Item2;
                    group.cohesion = CalculateGroupCohesion(group);
                    emotionalGroups[groupId] = group;

                    // Amplify emotions within group
                    AmplifyGroupEmotions(group);
                }
            }
        }

        private (ContagiousEmotion, float)? GetDominantEmotion(int minipollId)
        {
            if (!minipollEmotionalStates.ContainsKey(minipollId))
                return null;

            var emotions = minipollEmotionalStates[minipollId];
            if (emotions.Count == 0) return null;

            var dominant = emotions.OrderByDescending(kvp => kvp.Value).First();
            return (dominant.Key, dominant.Value);
        }

        private float CalculateGroupCohesion(EmotionalGroup group)
        {
            if (group.memberIds.Count < 2) return 0f;

            float totalSimilarity = 0f;
            int comparisons = 0;

            for (int i = 0; i < group.memberIds.Count - 1; i++)
            {
                for (int j = i + 1; j < group.memberIds.Count; j++)
                {
                    float similarity = CalculateEmotionalSimilarity(group.memberIds[i], group.memberIds[j]);
                    totalSimilarity += similarity;
                    comparisons++;
                }
            }

            return comparisons > 0 ? totalSimilarity / comparisons : 0f;
        }

        private float CalculateEmotionalSimilarity(int minipoll1, int minipoll2)
        {
            if (!minipollEmotionalStates.ContainsKey(minipoll1) || !minipollEmotionalStates.ContainsKey(minipoll2))
                return 0f;

            var emotions1 = minipollEmotionalStates[minipoll1];
            var emotions2 = minipollEmotionalStates[minipoll2];

            float similarity = 0f;
            int sharedEmotions = 0;

            foreach (var emotion in emotions1.Keys)
            {
                if (emotions2.ContainsKey(emotion))
                {
                    float diff = Mathf.Abs(emotions1[emotion] - emotions2[emotion]);
                    similarity += 1f - diff;
                    sharedEmotions++;
                }
            }

            return sharedEmotions > 0 ? similarity / sharedEmotions : 0f;
        }

        private void AmplifyGroupEmotions(EmotionalGroup group)
        {
            foreach (int memberId in group.memberIds)
            {
                foreach (var emotion in group.dominantEmotions)
                {
                    // Reinforce the group's dominant emotion
                    if (minipollEmotionalStates.ContainsKey(memberId))
                    {
                        float current = minipollEmotionalStates[memberId].ContainsKey(emotion.Key) ? 
                            minipollEmotionalStates[memberId][emotion.Key] : 0f;
                        
                        minipollEmotionalStates[memberId][emotion.Key] = 
                            Mathf.Clamp01(current + group.cohesion * 0.1f);
                    }
                }
            }
        }

        #region Utility Methods

        private float GetEmotionContagionMultiplier(ContagiousEmotion emotion)
        {
            return emotionContagionStrength.ContainsKey(emotion) ? emotionContagionStrength[emotion] : 1f;
        }

        private int CountNearbyWithEmotion(Vector3 position, ContagiousEmotion emotion, float radius)
        {
            int count = 0;
            Collider[] nearby = Physics.OverlapSphere(position, radius);
            
            foreach (var collider in nearby)
            {
                if (collider.TryGetComponent< MinipollGame.Core.MinipollCore>(out MinipollGame.Core.MinipollCore minipoll))
                {
                    int id = minipoll.GetInstanceID();
                    if (minipollEmotionalStates.ContainsKey(id) && 
                        minipollEmotionalStates[id].ContainsKey(emotion) &&
                        minipollEmotionalStates[id][emotion] > 0.3f)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private string GetMinipollGroup(int minipollId)
        {
            foreach (var group in emotionalGroups.Values)
            {
                if (group.memberIds.Contains(minipollId))
                    return group.groupId;
            }
            return null;
        }

        #endregion

        #region Visual Effects

        private void CreateEmotionWaveVisual(EmotionWave wave)
        {
            if (emotionWavePrefab == null) return;

            GameObject visual = Instantiate(emotionWavePrefab, wave.origin, Quaternion.identity);
            
            // Set color based on emotion
            Color emotionColor = GetEmotionColor(wave.emotion);
            var renderer = visual.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = emotionColor;
            }

            // Animate expansion
            StartCoroutine(AnimateEmotionWave(visual, wave));
        }

        private Color GetEmotionColor(ContagiousEmotion emotion)
        {
            // Map emotions to color gradient positions
            float gradientPosition = emotion switch
            {
                ContagiousEmotion.Joy => 0.9f,        // Yellow
                ContagiousEmotion.Love => 0.8f,       // Pink
                ContagiousEmotion.Excitement => 0.7f, // Orange
                ContagiousEmotion.Calm => 0.5f,       // Green
                ContagiousEmotion.Curiosity => 0.4f,  // Cyan
                ContagiousEmotion.Sadness => 0.3f,    // Blue
                ContagiousEmotion.Fear => 0.2f,       // Purple
                ContagiousEmotion.Disgust => 0.15f,   // Brown
                ContagiousEmotion.Anger => 0.1f,      // Red
                ContagiousEmotion.Panic => 0.0f,      // Dark Red
                _ => 0.5f
            };

            return emotionColorGradient.Evaluate(gradientPosition);
        }

        private System.Collections.IEnumerator AnimateEmotionWave(GameObject visual, EmotionWave wave)
        {
            float duration = 2f;
            float elapsed = 0f;
            Vector3 initialScale = visual.transform.localScale;
            Vector3 targetScale = initialScale * (wave.radius / baseContagionRadius);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Expand and fade
                visual.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
                
                var renderer = visual.GetComponentInChildren<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    Color color = renderer.material.color;
                    color.a = Mathf.Lerp(0.5f, 0f, t);
                    renderer.material.color = color;
                }

                yield return null;
            }

            Destroy(visual);
        }

        #endregion

        #region Event Handlers

        private void OnMinipollEmotionChanged(object data)
        {
            if (data is Dictionary<string, object> eventData)
            {
                if (eventData.ContainsKey("minipollId") && eventData.ContainsKey("emotion") && eventData.ContainsKey("intensity"))
                {
                    int minipollId = (int)eventData["minipollId"];
                    string emotionName = eventData["emotion"].ToString();
                    float intensity = (float)eventData["intensity"];

                    // Map to contagious emotion
                    ContagiousEmotion? contagiousEmotion = emotionName switch
                    {
                        "Happy" => ContagiousEmotion.Joy,
                        "Scared" => ContagiousEmotion.Fear,
                        "Angry" => ContagiousEmotion.Anger,
                        "Sad" => ContagiousEmotion.Sadness,
                        "Loving" => ContagiousEmotion.Love,
                        "Curious" => ContagiousEmotion.Curiosity,
                        _ => null
                    };

                    if (contagiousEmotion.HasValue && intensity > 0.5f)
                    {
                        BroadcastEmotion(minipollId, contagiousEmotion.Value, intensity);
                    }
                }
            }
        }

        private void OnMinipollDied(object data)
        {
            if (data is GameObject minipoll)
            {
                int id = minipoll.GetInstanceID();
                
                // Broadcast fear/sadness from death location
                BroadcastEmotion(id, ContagiousEmotion.Fear, 0.8f, baseContagionRadius * 1.5f);
                
                // Clean up emotional data
                minipollEmotionalStates.Remove(id);
                activeInfluences.Remove(id);
            }
        }

        private void OnGroupFormed(object data)
        {
            // Handle when minipolls form groups/tribes
            // Could boost emotional contagion within groups
        }

        #endregion

        #region Public API

        public float GetEmotionalInfluence(int minipollId, ContagiousEmotion emotion)
        {
            if (minipollEmotionalStates.ContainsKey(minipollId) && 
                minipollEmotionalStates[minipollId].ContainsKey(emotion))
            {
                return minipollEmotionalStates[minipollId][emotion];
            }
            return 0f;
        }

        public List<ContagiousEmotion> GetActiveEmotions(int minipollId)
        {
            if (minipollEmotionalStates.ContainsKey(minipollId))
            {
                return minipollEmotionalStates[minipollId]
                    .Where(kvp => kvp.Value > minEmotionThreshold)
                    .Select(kvp => kvp.Key)
                    .ToList();
            }
            return new List<ContagiousEmotion>();
        }

        public EmotionalGroup GetMinipollEmotionalGroup(int minipollId)
        {
            string groupId = GetMinipollGroup(minipollId);
            return !string.IsNullOrEmpty(groupId) && emotionalGroups.ContainsKey(groupId) ? 
                emotionalGroups[groupId] : null;
        }

        public void TriggerMassEmotion(Vector3 center, float radius, ContagiousEmotion emotion, float intensity)
        {
            // Create a powerful emotion wave from a location
            EmotionWave wave = wavePool.Get();
            wave.emotion = emotion;
            wave.origin = center;
            wave.strength = intensity * 2f; // Double strength for mass events
            wave.radius = radius;
            wave.sourceMinipollId = -1; // No specific source
            wave.timestamp = Time.time;
            wave.affectedMinipollIds.Clear();

            activeWaves.Add(wave);
            
            // Immediate propagation
            PropagateEmotionImmediate(wave);
        }

        #endregion

        #region Debug

        public Dictionary<string, object> GetSystemStats()
        {
            return new Dictionary<string, object>
            {
                ["ActiveWaves"] = activeWaves.Count,
                ["TrackedMinipolls"] = minipollEmotionalStates.Count,
                ["EmotionalGroups"] = emotionalGroups.Count,
                ["ActiveInfluences"] = activeInfluences.Sum(kvp => kvp.Value.Count)
            };
        }

        private void OnDrawGizmos()
        {
            if (!showEmotionWaves) return;

            // Draw emotion waves
            foreach (var wave in activeWaves)
            {
                Gizmos.color = GetEmotionColor(wave.emotion) * new Color(1, 1, 1, wave.strength);
                Gizmos.DrawWireSphere(wave.origin, wave.radius);
            }

            // Draw emotional groups
            foreach (var group in emotionalGroups.Values)
            {
                if (group.memberIds.Count < 3) continue;
                
                Gizmos.color = Color.cyan * 0.3f;
                foreach (int memberId in group.memberIds)
                {
                    GameObject member = MinipollManager.Instance?.GetMinipollById(memberId);
                    if (member != null)
                    {
                        Gizmos.DrawLine(group.center, member.transform.position);
                    }
                }
            }
        }

        #endregion

        [Obsolete]
        private void OnDestroy()
        {
            if (EventSystem.Instance != null)
            {
                EventSystem.Instance.Unsubscribe<GameObject>("MinipollEmotionChanged", OnMinipollEmotionChanged);
                EventSystem.Instance.Unsubscribe<GameObject>("MinipollDied", OnMinipollDied);
                EventSystem.Instance.Unsubscribe<GameObject>("GroupFormed", OnGroupFormed);
            
            }
        }
    }

    // Simple object pool implementation
    public class ObjectPool<T> where T : class, new()
    {
        private Queue<T> pool;
        private Func<T> createFunc;
        private int maxSize;

        public ObjectPool(Func<T> createFunc, int maxSize = 100)
        {
            this.createFunc = createFunc;
            this.maxSize = maxSize;
            pool = new Queue<T>();
        }

        public T Get()
        {
            return pool.Count > 0 ? pool.Dequeue() : createFunc();
        }

        public void Return(T item)
        {
            if (pool.Count < maxSize)
            {
                pool.Enqueue(item);
            }
        }
    }
}