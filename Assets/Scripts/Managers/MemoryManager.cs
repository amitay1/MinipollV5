using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using MinipollGame.Core;


    /// <summary>
    /// Global memory management system for optimizing and sharing memories between Minipolls
    /// Implements memory pools, shared knowledge, and collective memory concepts
    /// </summary>
    namespace  MinipollGame.Managers
    {
    public class MemoryManager : MonoBehaviour
    {

        private static MemoryManager _instance;

        [Obsolete]
        public static MemoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MemoryManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MemoryManager");
                        _instance = go.AddComponent<MemoryManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Memory Configuration")]
        [SerializeField] private int maxGlobalMemories = 1000;
        [SerializeField] private int maxSharedMemoriesPerType = 100;
        [SerializeField] private float memoryDecayRate = 0.1f;
        [SerializeField] private float memoryShareRadius = 10f;
        [SerializeField] private bool enableCollectiveMemory = true;

        [Header("Memory Pools")]
        [SerializeField] private int memoryPoolSize = 500;

        // Memory data structures
        private Dictionary<string, List<SharedMemory>> sharedMemories = new Dictionary<string, List<SharedMemory>>();
        private Dictionary<int, MinipollMemoryProfile> minipollMemories = new Dictionary<int, MinipollMemoryProfile>();
        private Queue<Memory> memoryPool = new Queue<Memory>();
        private List<GlobalEvent> globalEvents = new List<GlobalEvent>();

        // Memory types
        public enum MemoryType
        {
            Personal,
            Shared,
            Collective,
            Instinctual,
            Danger,    // Added to fix CS0117
            Knowledge, // Added to fix CS0117
            Social     // Added to fix CS0117
        }

        // Memory importance levels
        public enum MemoryImportance
        {
            Trivial,
            Minor,
            Normal,
            Important,
            Critical
        }

        [Serializable]
        public class Memory
        {
            public string id;
            public MemoryType type;
            public MemoryImportance importance;
            public string subject;
            public string details;
            public Vector3 location;
            public float timestamp;
            public float strength;
            public Dictionary<string, object> metadata;
            public List<int> involvedMinipollIds;

            public Memory()
            {
                id = Guid.NewGuid().ToString();
                metadata = new Dictionary<string, object>();
                involvedMinipollIds = new List<int>();
                strength = 1f;
            }

            public void Decay(float decayRate)
            {
                strength -= decayRate * Time.deltaTime;
                strength = Mathf.Clamp01(strength);
            }
        }

        [Serializable]
        public class SharedMemory : Memory
        {
            public int originMinipollId;
            public int shareCount;
            public float reliability;
        }

        [Serializable]
        public class GlobalEvent
        {
            public string eventName;
            public float timestamp;
            public Vector3 location;
            public List<int> witnesses;
            public Dictionary<string, object> eventData;
        }

        [Serializable]
        public class MinipollMemoryProfile
        {
            public int minipollId;
            public int maxMemories = 50;
            public List<Memory> memories;
            public float memoryCapacity = 1f;
            public float learningRate = 1f;

            public MinipollMemoryProfile(int id)
            {
                minipollId = id;
                memories = new List<Memory>();
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMemoryPool();
        }        [Obsolete]
        private void Start()
        {
            // Subscribe to global events
            // Note: EventBus is commented out in MinipollEventBus.cs
            // If you need event subscriptions, uncomment the EventBus code or use alternative event system
            /*
            if (GameEventBus.Instance != null)
            {
                GameEventBus.Subscribe<CriticalEvent>(HandleCriticalEvent);
                GameEventBus.Subscribe<ForgottenEvent>(HandleForgottenEvent);
                GameEventBus.Subscribe<SocialEvent>(HandleSocialEvent);
            }
            */

            InvokeRepeating(nameof(ProcessMemoryDecay), 1f, 1f);
            InvokeRepeating(nameof(CleanupOldMemories), 10f, 10f);
        }        private void InitializeMemoryPool()
        {
            for (int i = 0; i < memoryPoolSize; i++)
            {
                memoryPool.Enqueue(new Memory());
            }
        }

        #region Memory Creation and Storage

        public Memory CreateMemory(int minipollId, string subject, string details,
            MemoryType type = MemoryType.Personal, MemoryImportance importance = MemoryImportance.Normal)
        {
            Memory memory = GetPooledMemory();
            memory.subject = subject;
            memory.details = details;
            memory.type = type;
            memory.importance = importance;
            memory.timestamp = Time.time;
            memory.involvedMinipollIds.Clear();
            memory.involvedMinipollIds.Add(minipollId);

            // Get location from minipoll
            GameObject minipoll = MinipollManager.Instance?.GetMinipollById(minipollId);
            if (minipoll != null)
            {
                memory.location = minipoll.transform.position;
            }

            StoreMemory(minipollId, memory);

            // Check if this should be shared
            if (type == MemoryType.Shared || importance >= MemoryImportance.Important)
            {
                ShareMemoryWithNearby(minipollId, memory);
            }

            return memory;
        }

        private Memory GetPooledMemory()
        {
            if (memoryPool.Count > 0)
            {
                Memory memory = memoryPool.Dequeue();
                memory.strength = 1f;
                memory.metadata.Clear();
                return memory;
            }
            return new Memory();
        }

        private void StoreMemory(int minipollId, Memory memory)
        {
            if (!minipollMemories.ContainsKey(minipollId))
            {
                minipollMemories[minipollId] = new MinipollMemoryProfile(minipollId);
            }

            var profile = minipollMemories[minipollId];

            // Check capacity
            if (profile.memories.Count >= profile.maxMemories)
            {
                // Remove weakest memory
                var weakest = profile.memories.OrderBy(m => m.strength * (int)m.importance).FirstOrDefault();
                if (weakest != null)
                {
                    profile.memories.Remove(weakest);
                    ReturnToPool(weakest);
                }
            }

            profile.memories.Add(memory);

            // Store in shared memories if applicable
            if (memory.type == MemoryType.Shared || memory.type == MemoryType.Collective)
            {
                if (!sharedMemories.ContainsKey(memory.subject))
                {
                    sharedMemories[memory.subject] = new List<SharedMemory>();
                }

                var shared = new SharedMemory
                {
                    id = memory.id,
                    type = memory.type,
                    importance = memory.importance,
                    subject = memory.subject,
                    details = memory.details,
                    location = memory.location,
                    timestamp = memory.timestamp,
                    strength = memory.strength,
                    originMinipollId = minipollId,
                    shareCount = 0,
                    reliability = 1f
                };

                sharedMemories[memory.subject].Add(shared);
            }
        }

        #endregion

        #region Memory Sharing and Collective Memory

        private void ShareMemoryWithNearby(int originId, Memory memory)
        {
            if (!enableCollectiveMemory) return;

            GameObject origin = MinipollManager.Instance?.GetMinipollById(originId);
            if (origin == null) return;

            // Find nearby minipolls
            Collider[] nearbyColliders = Physics.OverlapSphere(origin.transform.position, memoryShareRadius);

            foreach (var collider in nearbyColliders)
            {
                var nearbyMinipoll = collider.GetComponent<MinipollGame.Core.MinipollCore>();
                if (nearbyMinipoll != null && nearbyMinipoll.gameObject != origin)
                {
                    // Check if they can receive the memory (based on relationship, etc.)
                    if (CanShareMemory(originId, nearbyMinipoll.GetInstanceID(), memory))
                    {
                        // Create a shared version of the memory
                        Memory sharedVersion = CreateMemory(
                            nearbyMinipoll.GetInstanceID(),
                            memory.subject,
                            $"[Heard from another]: {memory.details}",
                            MemoryType.Shared,
                            (MemoryImportance)Mathf.Max(0, (int)memory.importance - 1)
                        );

                        sharedVersion.metadata["originalSource"] = originId;
                        sharedVersion.metadata["reliability"] = 0.8f; // Reduced reliability for second-hand info
                    }
                }
            }
        }

        private bool CanShareMemory(int senderId, int receiverId, Memory memory)
        {
            // Check social relationship
            Social.MinipollSocialRelations senderMinipoll = MinipollManager.Instance?.GetMinipollById(senderId)?.GetComponent<MinipollGame.Social.MinipollSocialRelations>();
            if (senderMinipoll != null)
            {
                float relationship = senderMinipoll.GetRelationshipValue(receiverId);

                // More likely to share with friends
                if (relationship > 0.5f) return true;

                // Still share critical information even with non-friends
                if (memory.importance >= MemoryImportance.Critical) return true;

                // Random chance based on importance
                return UnityEngine.Random.value < 0.3f + (int)memory.importance * 0.1f;
            }

            return false;
        }

        public void CreateCollectiveMemory(string subject, string details, Vector3 location, List<int> witnesses)
        {
            if (!enableCollectiveMemory) return;

            GlobalEvent globalEvent = new GlobalEvent
            {
                eventName = subject,
                timestamp = Time.time,
                location = location,
                witnesses = new List<int>(witnesses),
                eventData = new Dictionary<string, object>()
            };

            globalEvents.Add(globalEvent);

            // Create memory for all witnesses
            foreach (int witnessId in witnesses)
            {
                CreateMemory(witnessId, subject, details, MemoryType.Collective, MemoryImportance.Important);
            }

            // Propagate to nearby non-witnesses
            PropagateCollectiveMemory(globalEvent);
        }

        private void PropagateCollectiveMemory(GlobalEvent globalEvent)
        {
            // Find all minipolls in a larger radius
            Collider[] allNearby = Physics.OverlapSphere(globalEvent.location, memoryShareRadius * 2f);            foreach (var collider in allNearby)
            {
                var minipoll = collider.GetComponent<MinipollGame.Core.MinipollCore>();
                if (minipoll != null && !globalEvent.witnesses.Contains(minipoll.GetInstanceID()))
                {
                    // Delayed information spread
                    float distance = Vector3.Distance(globalEvent.location, minipoll.transform.position);
                    float delay = distance / 10f; // Information travels at 10 units/second

                    StartCoroutine(DelayedMemoryCreation(
                        minipoll.GetInstanceID(),
                        globalEvent.eventName,
                        $"[Collective knowledge]: {globalEvent.eventName}",
                        delay
                    ));
                }
            }
        }

        private System.Collections.IEnumerator DelayedMemoryCreation(int minipollId, string subject, string details, float delay)
        {
            yield return new WaitForSeconds(delay);
            CreateMemory(minipollId, subject, details, MemoryType.Collective, MemoryImportance.Normal);
        }

        #endregion

        #region Memory Retrieval and Queries

        public List<Memory> GetMemories(int minipollId, string subject = null, MemoryType? type = null)
        {
            if (!minipollMemories.ContainsKey(minipollId))
                return new List<Memory>();

            var memories = minipollMemories[minipollId].memories;

            if (!string.IsNullOrEmpty(subject))
            {
                memories = memories.Where(m => m.subject.Contains(subject)).ToList();
            }

            if (type.HasValue)
            {
                memories = memories.Where(m => m.type == type.Value).ToList();
            }

            return memories.OrderByDescending(m => m.strength * (int)m.importance).ToList();
        }

        public Memory GetStrongestMemory(int minipollId, string subject)
        {
            var memories = GetMemories(minipollId, subject);
            return memories.FirstOrDefault();
        }

        public List<SharedMemory> GetSharedMemories(string subject)
        {
            if (sharedMemories.ContainsKey(subject))
            {
                return sharedMemories[subject].Where(m => m.strength > 0.1f).ToList();
            }
            return new List<SharedMemory>();
        }

        public bool HasMemory(int minipollId, string subject)
        {
            return GetMemories(minipollId, subject).Any();
        }

        public float GetMemoryStrength(int minipollId, string subject)
        {
            var memory = GetStrongestMemory(minipollId, subject);
            return memory?.strength ?? 0f;
        }

        #endregion

        #region Memory Modification

        public void ReinforceMemory(int minipollId, string subject, float reinforcement = 0.2f)
        {
            var memories = GetMemories(minipollId, subject);
            foreach (var memory in memories)
            {
                memory.strength = Mathf.Clamp01(memory.strength + reinforcement);
                memory.timestamp = Time.time; // Update last access time
            }
        }

        public void WeakenMemory(int minipollId, string subject, float weakening = 0.3f)
        {
            var memories = GetMemories(minipollId, subject);
            foreach (var memory in memories)
            {
                memory.strength = Mathf.Clamp01(memory.strength - weakening);
            }
        }

        public void ForgetMemory(int minipollId, string memoryId)
        {
            if (minipollMemories.ContainsKey(minipollId))
            {
                var memory = minipollMemories[minipollId].memories.FirstOrDefault(m => m.id == memoryId);
                if (memory != null)
                {
                    minipollMemories[minipollId].memories.Remove(memory);
                    ReturnToPool(memory);
                }
            }
        }

        #endregion

        #region Memory Maintenance

        private void ProcessMemoryDecay()
        {
            float decayMultiplier = memoryDecayRate * Time.deltaTime;

            foreach (var profile in minipollMemories.Values)
            {
                for (int i = profile.memories.Count - 1; i >= 0; i--)
                {
                    var memory = profile.memories[i];

                    // Decay based on importance
                    float importanceMultiplier = 1f - ((int)memory.importance * 0.2f);
                    memory.Decay(decayMultiplier * importanceMultiplier);

                    // Remove if too weak
                    if (memory.strength < 0.01f)
                    {
                        profile.memories.RemoveAt(i);
                        ReturnToPool(memory);
                    }
                }
            }

            // Decay shared memories
            foreach (var memoryList in sharedMemories.Values)
            {
                for (int i = memoryList.Count - 1; i >= 0; i--)
                {
                    memoryList[i].Decay(decayMultiplier * 0.5f); // Shared memories decay slower
                    if (memoryList[i].strength < 0.01f)
                    {
                        memoryList.RemoveAt(i);
                    }
                }
            }
        }

        private void CleanupOldMemories()
        {
            // Remove very old global events
            globalEvents.RemoveAll(e => Time.time - e.timestamp > 3600f); // Remove after 1 hour

            // Clean up empty shared memory categories
            var emptyCategories = sharedMemories.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList();
            foreach (var category in emptyCategories)
            {
                sharedMemories.Remove(category);
            }

            // Check total memory usage
            int totalMemories = minipollMemories.Sum(kvp => kvp.Value.memories.Count);
            if (totalMemories > maxGlobalMemories)
            {
                // Force cleanup of weakest memories
                var allMemories = minipollMemories.SelectMany(kvp =>
                    kvp.Value.memories.Select(m => new { MinipollId = kvp.Key, Memory = m }))
                    .OrderBy(x => x.Memory.strength * (int)x.Memory.importance)
                    .Take(totalMemories - maxGlobalMemories);

                foreach (var item in allMemories)
                {
                    minipollMemories[item.MinipollId].memories.Remove(item.Memory);
                    ReturnToPool(item.Memory);
                }
            }
        }

        private void ReturnToPool(Memory memory)
        {
            memory.metadata.Clear();
            memory.involvedMinipollIds.Clear();
            memoryPool.Enqueue(memory);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// טיפול באירועים קריטיים - יצירת זיכרונות חשובים
        /// </summary>
        private void HandleCriticalEvent(CriticalEvent evt)
        {
            if (evt == null || evt.minipoll == null) return;

            // יצירת זיכרון מהאירוע הקריטי
            var memory = new MemoryItem(
                evt.description,
                MemoryType.Danger,
                evt.severity // חשיבות על בסיס חומרת האירוע
            );

            memory.location = evt.location;
            memory.emotionalContext = "Fear";
            memory.tags = new string[] { "Critical", evt.eventType, "Important" };

            // הוספת הזיכרון למיניפול
            var memorySystem = evt.minipoll.GetMemorySystem();
            if (memorySystem != null)
            {
                // Cast to the proper memory system type that has AddMemory method
                if (memorySystem is IMemorySystem typedMemorySystem)
                {
                    typedMemorySystem.AddMemory(
                        memory.description,
                        evt.minipoll.gameObject,  // המרה ל-GameObject
                        false,  // אירוע קריטי = שלילי
                        evt.severity
                    );
                }
                else
                {
                    // Alternative: Use reflection or try dynamic casting
                    var addMemoryMethod = memorySystem.GetType().GetMethod("AddMemory");
                    if (addMemoryMethod != null)
                    {
                        addMemoryMethod.Invoke(memorySystem, new object[] 
                        { 
                            memory.description, 
                            evt.minipoll.gameObject, 
                            false, 
                            evt.severity 
                        });
                    }
                }
            }

            // רישום בזיכרון הגלובלי
            if (!globalMemories.ContainsKey(evt.minipoll))
            {
                globalMemories[evt.minipoll] = new List<MemoryItem>();
            }
            globalMemories[evt.minipoll].Add(memory);

            if (debugMode)
            {
                Debug.Log($"Critical Event Memory Created: {evt.description} for {evt.minipoll.name}");
            }
        }

        /// <summary>
        /// טיפול בזיכרונות שנשכחו
        /// </summary>
        private void HandleForgottenEvent(ForgottenEvent evt)
        {
            if (evt == null || evt.minipoll == null || evt.forgottenMemory == null) return;

            // הסרת הזיכרון מהרשימה הגלובלית
            if (globalMemories.ContainsKey(evt.minipoll))
            {
                globalMemories[evt.minipoll].Remove(evt.forgottenMemory);

                // אם אין יותר זיכרונות, הסר את המפתח
                if (globalMemories[evt.minipoll].Count == 0)
                {
                    globalMemories.Remove(evt.minipoll);
                }
            }

            // עדכון סטטיסטיקות
            totalForgottenMemories++;

            // יצירת זיכרון "רפאים" - זיכרון חלקי שנשאר
            if (evt.forgottenMemory.importance > 0.7f && UnityEngine.Random.value < 0.3f)
            {                var ghostMemory = new MemoryItem(
                    "Vague memory of " + evt.forgottenMemory.description,
                    MemoryType.Knowledge,
                    evt.forgottenMemory.importance * 0.3f
                );

                ghostMemory.strength = 20f;
                ghostMemory.decayRate = 0.01f; // דעיכה איטית מאוד

                if (!vagueMemories.ContainsKey(evt.minipoll))
                {
                    vagueMemories[evt.minipoll] = new List<MemoryItem>();
                }
                vagueMemories[evt.minipoll].Add(ghostMemory);
            }

            if (debugMode)
            {
                Debug.Log($"Memory Forgotten: {evt.forgottenMemory.description} by {evt.minipoll.name} (Reason: {evt.reason})");
            }
        }

        /// <summary>
        /// טיפול באירועים חברתיים - יצירת זיכרונות חברתיים
        /// </summary>
        private void HandleSocialEvent(SocialEvent evt)
        {
            if (evt == null || evt.initiator == null) return;

            // יצירת זיכרון עבור המתחיל
            var initiatorMemory = new MemoryItem(
                $"{evt.eventType} with {evt.target?.name ?? "unknown"}",
                MemoryType.Social,
                Mathf.Abs(evt.socialImpact) * 0.5f
            );

            initiatorMemory.location = evt.location;
            initiatorMemory.relatedMinipoll = evt.target;
            initiatorMemory.emotionalContext = evt.socialImpact > 0 ? "Positive" : "Negative";
            initiatorMemory.tags = new string[] { "Social", evt.eventType, evt.target?.name ?? "unknown" };

            // הוספת זיכרון למתחיל
            var initiatorMemorySystem = evt.initiator.GetMemorySystem();
            if (initiatorMemorySystem != null)
            {
                if (initiatorMemorySystem is IMemorySystem typedInitiatorSystem)
                {
                    typedInitiatorSystem.AddMemory(
                        initiatorMemory.description,
                        evt.target?.gameObject,  // GameObject במקום MinipולBrain
                        evt.socialImpact > 0,    // חיובי או שלילי
                        Mathf.Abs(evt.socialImpact) * 0.5f
                    );
                }
                else
                {
                    var addMemoryMethod = initiatorMemorySystem.GetType().GetMethod("AddMemory");
                    if (addMemoryMethod != null)
                    {
                        addMemoryMethod.Invoke(initiatorMemorySystem, new object[] 
                        { 
                            initiatorMemory.description, 
                            evt.target?.gameObject, 
                            evt.socialImpact > 0, 
                            Mathf.Abs(evt.socialImpact) * 0.5f 
                        });
                    }
                }
            }

            // יצירת זיכרון עבור היעד
            if (evt.target != null)
            {                var targetMemory = new MemoryItem(
                    $"{evt.eventType} from {evt.initiator.name}",
                    MemoryType.Social,
                    Mathf.Abs(evt.socialImpact) * 0.5f
                );

                targetMemory.location = evt.location;
                targetMemory.relatedMinipoll = evt.initiator;
                targetMemory.emotionalContext = evt.socialImpact > 0 ? "Positive" : "Negative";
                targetMemory.tags = new string[] { "Social", evt.eventType, evt.initiator.name };

                var targetMemorySystem = evt.target.GetMemorySystem();
                if (targetMemorySystem != null)
                {
                    if (targetMemorySystem is IMemorySystem typedTargetSystem)
                    {
                        typedTargetSystem.AddMemory(
                            targetMemory.description,
                            evt.initiator.gameObject,  // GameObject במקום MinipולBrain
                            evt.socialImpact > 0,       // חיובי או שלילי
                            Mathf.Abs(evt.socialImpact) * 0.5f
                        );
                    }
                    else
                    {
                        var addMemoryMethod = targetMemorySystem.GetType().GetMethod("AddMemory");
                        if (addMemoryMethod != null)
                        {
                            addMemoryMethod.Invoke(targetMemorySystem, new object[] 
                            { 
                                targetMemory.description, 
                                evt.initiator.gameObject, 
                                evt.socialImpact > 0, 
                                Mathf.Abs(evt.socialImpact) * 0.5f 
                            });
                        }
                    }
                }
            }

            // עדכון מערכות יחסים
            UpdateRelationshipMemories(evt.initiator, evt.target, evt.socialImpact);

            if (debugMode)
            {
                Debug.Log($"Social Event Memory: {evt.eventType} between {evt.initiator.name} and {evt.target?.name ?? "unknown"}");
            }
        }        /// <summary>
        /// עדכון זיכרונות יחסים
        /// </summary>
        private void UpdateRelationshipMemories(MinipollBrain initiator, MinipollBrain target, float impact)
        {
            if (initiator == null || target == null) return;

            string relationshipKey = GetRelationshipKey(initiator, target);

            if (!relationshipMemories.ContainsKey(relationshipKey))
            {
                relationshipMemories[relationshipKey] = new RelationshipMemory
                {
                    minipoll1 = initiator,
                    minipoll2 = target,
                    totalInteractions = 0,
                    positiveInteractions = 0,
                    negativeInteractions = 0,
                    lastInteractionTime = Time.time
                };
            }

            var relMemory = relationshipMemories[relationshipKey];
            relMemory.totalInteractions++;
            relMemory.lastInteractionTime = Time.time;

            if (impact > 0)
                relMemory.positiveInteractions++;
            else if (impact < 0)
                relMemory.negativeInteractions++;
        }

        private string GetRelationshipKey(MinipollBrain m1, MinipollBrain m2)
        {
            // יצירת מפתח ייחודי לזוג, ללא תלות בסדר
            int id1 = m1.GetInstanceID();
            int id2 = m2.GetInstanceID();
            return id1 < id2 ? $"{id1}_{id2}" : $"{id2}_{id1}";        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        public class RelationshipMemory
        {
            public MinipollBrain minipoll1;
            public MinipollBrain minipoll2;
            public int totalInteractions;
            public int positiveInteractions;
            public int negativeInteractions;
            public float lastInteractionTime;

            public float GetRelationshipQuality()
            {
                if (totalInteractions == 0) return 0f;
                return (positiveInteractions - negativeInteractions) / (float)totalInteractions;
            }
        }

        #endregion

        #region Fields to Add to MemoryManager

        // הוסף את השדות האלה אם הם לא קיימים:
        private Dictionary<MinipollBrain, List<MemoryItem>> globalMemories = new Dictionary<MinipollBrain, List<MemoryItem>>();
        private Dictionary<MinipollBrain, List<MemoryItem>> vagueMemories = new Dictionary<MinipollBrain, List<MemoryItem>>();
        private Dictionary<string, RelationshipMemory> relationshipMemories = new Dictionary<string, RelationshipMemory>();
        private int totalForgottenMemories = 0;        private bool debugMode = true; // או false, תלוי בהעדפה שלך

        #endregion

        public Dictionary<string, object> GetMemoryStats()
        {
            return new Dictionary<string, object>
            {
                ["TotalMinipolls"] = minipollMemories.Count,
                ["TotalMemories"] = minipollMemories.Sum(kvp => kvp.Value.memories.Count),
                ["SharedMemoryCategories"] = sharedMemories.Count,
                ["GlobalEvents"] = globalEvents.Count,
                ["MemoryPoolAvailable"] = memoryPool.Count
            };
        }

        public void VisualizeMemoryConnections(int minipollId)
        {
            // This could be used to show memory connections in editor or runtime
            if (!minipollMemories.ContainsKey(minipollId)) return;

            var memories = minipollMemories[minipollId].memories;
            foreach (var memory in memories)
            {
                if (memory.type == MemoryType.Shared)
                {
                    // Draw connections to other minipolls who share this memory
                    var sharedWith = GetMinipollsWithMemory(memory.subject);
                    // Visualization code here
                }
            }
        }

        private List<int> GetMinipollsWithMemory(string subject)
        {
            return minipollMemories
                .Where(kvp => kvp.Value.memories.Any(m => m.subject == subject))
                .Select(kvp => kvp.Key)
                .ToList();
        }
    }// Event Classes שחסרים
    [System.Serializable]
    public class CriticalEvent
    {
        public MinipollBrain minipoll;
        public string eventType;
        public string description;
        public float severity;
        public Vector3 location;
        public float timestamp;

        public CriticalEvent(MinipollBrain brain, string type, string desc, float sev = 1f)
        {
            minipoll = brain;
            eventType = type;
            description = desc;
            severity = sev;
            location = brain ? brain.transform.position : Vector3.zero;
            timestamp = Time.time;
        }
    }

    [System.Serializable]
    public class ForgottenEvent
    {
        public MinipollBrain minipoll;
        public MemoryItem forgottenMemory;
        public float timeForgotten;
        public string reason;

        public ForgottenEvent(MinipollBrain brain, MemoryItem memory, string forgotReason = "Natural decay")
        {
            minipoll = brain;
            forgottenMemory = memory;
            timeForgotten = Time.time;
            reason = forgotReason;
        }
    }

    [System.Serializable]
    public class SocialEvent
    {
        public MinipollBrain initiator;
        public MinipollBrain target;
        public string eventType;
        public float socialImpact;
        public Vector3 location;
        public float timestamp;

        public SocialEvent(MinipollBrain init, MinipollBrain targ, string type, float impact = 0f)
        {
            initiator = init;
            target = targ;
            eventType = type;
            socialImpact = impact;
            location = init ? init.transform.position : Vector3.zero;
            timestamp = Time.time;
        }
    }

    internal interface IMemorySystem
    {
        void AddMemory(string description, GameObject gameObject, bool v1, float v2);
    }
}