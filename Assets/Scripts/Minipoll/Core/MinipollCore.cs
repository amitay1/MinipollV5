using UnityEngine;
using System;
using System.Collections;
using MinipollGame.Systems.Core;
using MinipollGame.AI;
using MinipollGame.Social;
using MinipollGame.Controllers;
using MinipollGame.Managers;


namespace MinipollGame.Core

{


    /// <summary>
    /// הסקריפט הראשי של Minipoll - מנהל ומחבר את כל המערכות
    /// חייב להיות על ה-Root GameObject של ה-Prefab
    /// </summary>
    [RequireComponent(typeof(MinipollHealth))]
    [RequireComponent(typeof(MinipollStats))]
    [SelectionBase] // גורם לבחירה של האובייקט הראשי ב-Scene
    public class MinipollCore : MonoBehaviour
    {
        #region Singleton for Testing (Optional)
        public static MinipollCore SelectedMinipoll { get; private set; }
        #endregion

        #region Core Properties
        [Header("=== Minipoll Identity ===")]
        [SerializeField] private string minipollName;
        [SerializeField] private string uniqueID;
        [SerializeField] private float age = 0f;
        [SerializeField] private Color primaryColor = Color.cyan;

        // Public Properties
        public string Name => minipollName;
        public string ID => uniqueID;
        [field: SerializeField]
        public Gender Gender { get; private set; } = Gender.Random;
        [field: SerializeField]
        public AgeStage CurrentAgeStage { get; private set; } = AgeStage.Baby;
        public float Age => age;
        public Color PrimaryColor => primaryColor;
        public bool IsAlive { get; private set; } = true;
        public bool IsSelected => MinipollCore.SelectedMinipoll == this;

        [Header("=== Core Properties ===")]
        [SerializeField] private string species = "Minipoll";

        // Public Accessors for Core Properties
        public string Species => species;

        #endregion

        #region Component References
        [Header("=== Core Components ===")]
        private MinipollHealth health;
        [SerializeField] private MinipollStats stats;
        [SerializeField] private MinipollSelectable selectable;

        [Header("=== AI Components ===")]
        [SerializeField] private MinipollBrain brain;
        [SerializeField] private MinipollMemorySystem memory;
        [SerializeField] private MinipollAIPlanningSystem planner;

        [Header("=== Needs & Emotions ===")]
        [SerializeField] private MinipollNeedsSystem needs;
        [SerializeField] private MinipollEmotionsSystem emotions;

        [Header("=== Social Components ===")]
        [SerializeField] private MinipollSocialRelations socialRelations;
        [SerializeField] private MinipollTribeSystem tribeSystem;

        [Header("=== Controllers ===")]
        [SerializeField] private MinipollMovementController movement;
        [SerializeField] private MinipollVisualController visualController;
        [SerializeField] private MinipollBlinkController blinkController;
        [SerializeField] private MinipollWorldInteraction worldInteraction;

        // Public Accessors
        public MinipollHealth Health => health;
        public MinipollStats Stats => stats;
        public MinipollSelectable Selectable => selectable;
        public MinipollBrain Brain => brain;
        public MinipollMemorySystem Memory => memory;
        public MinipollNeedsSystem Needs => needs;
        public MinipollEmotionsSystem Emotions => emotions;

        // Aliases for backward compatibility
        public MinipollEmotionsSystem EmotionsSystem => emotions;
        public MinipollNeedsSystem NeedsSystem => needs;
        public MinipollMemorySystem MemorySystem => memory;

        public MinipollSocialRelations SocialRelations => socialRelations;
        public MinipollTribeSystem TribeSystem => tribeSystem;
        public MinipollMovementController Movement => movement;
        public MinipollVisualController VisualController => visualController;
        public MinipollWorldInteraction WorldInteraction => worldInteraction;

        #endregion

        #region Events
        public static event Action<MinipollCore> OnMinipollBorn;
        public static event Action<MinipollCore> OnMinipollDied;
        public static event Action<MinipollCore> OnMinipollSelected;
        public static event Action<MinipollCore> OnMinipollDeselected;

        public event Action<AgeStage, AgeStage> OnAgeStageChanged;
        public event Action<string, string> OnNameChanged;
        public event Action<float> OnAgeIncreased;
        #endregion

        #region Age Configuration
        [Header("=== Age Settings ===")]
        [SerializeField] private float ageProgressionSpeed = 1f; // כמה מהר מתבגרים
        [SerializeField] private float babyDuration = 100f;
        [SerializeField] private float childDuration = 400f;
        [SerializeField] private float adultDuration = 1500f;
        [SerializeField] private float maxAge = 2500f;

        private float nextAgeUpdateTime;
        private AgeStage ageStage;
        private const float AGE_UPDATE_INTERVAL = 1f; // עדכון גיל כל שנייה
        #endregion

        #region Initialization
        private void Awake()
        {
            // Generate unique ID if not set
            if (string.IsNullOrEmpty(uniqueID))
                uniqueID = System.Guid.NewGuid().ToString();

            // Initialize components
            ValidateAndInitializeComponents();

            // Set random properties if needed
            InitializeRandomProperties();

            // Register to manager
            if (MinipollManager.Instance != null)
                MinipollManager.Instance.RegisterMinipoll(this);
        }

        private void Start()
        {
            health = GetComponent<MinipollHealth>();
            // Subscribe to events
            RegisterEventListeners();

            // Initialize all systems
            InitializeSystems();

            // Announce birth
            OnMinipollBorn?.Invoke(this);

            Debug.Log($"[MinipollCore] {minipollName} was born! ID: {uniqueID}, Gender: {Gender}, Age Stage: {CurrentAgeStage}");
        }

        private void ValidateAndInitializeComponents()
        {
            // Core Components - חייבים להיות
            if (!health) health = GetComponent<MinipollHealth>();
            if (!stats) stats = GetComponent<MinipollStats>();
            if (!selectable) selectable = GetComponent<MinipollSelectable>();

            if (!health || !stats || !selectable)
            {
                Debug.LogError($"[MinipollCore] Missing CRITICAL components on {gameObject.name}!");
                enabled = false;
                return;
            }

            // Try to find components in children if not assigned
            if (!brain) brain = GetComponentInChildren<MinipollBrain>();
            if (!memory) memory = GetComponentInChildren<MinipollMemorySystem>();
            if (!planner) planner = GetComponentInChildren<MinipollAIPlanningSystem>();
            if (!needs) needs = GetComponentInChildren<MinipollNeedsSystem>();
            if (!emotions) emotions = GetComponentInChildren<MinipollEmotionsSystem>();
            if (!socialRelations) socialRelations = GetComponentInChildren<MinipollSocialRelations>();
            if (!tribeSystem) tribeSystem = GetComponentInChildren<MinipollTribeSystem>();
            if (!movement) movement = GetComponentInChildren<MinipollMovementController>();
            if (!visualController) visualController = GetComponentInChildren<MinipollVisualController>();
            if (!blinkController) blinkController = GetComponentInChildren<MinipollBlinkController>();
            if (!worldInteraction) worldInteraction = GetComponentInChildren<MinipollWorldInteraction>();

            // Log warnings for missing non-critical components
            LogMissingComponents();
        }

        private void LogMissingComponents()
        {
            if (!brain) Debug.LogWarning($"[MinipollCore] {name} missing Brain component");
            if (!memory) Debug.LogWarning($"[MinipollCore] {name} missing Memory component");
            if (!needs) Debug.LogWarning($"[MinipollCore] {name} missing Needs component");
            if (!emotions) Debug.LogWarning($"[MinipollCore] {name} missing Emotions component");
            if (!movement) Debug.LogWarning($"[MinipollCore] {name} missing Movement component");
        }

        private void InitializeRandomProperties()
        {
            // Random gender if needed
            if (Gender == Gender.Random)
                Gender = UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;

            // Generate name if empty
            if (string.IsNullOrEmpty(minipollName))
                minipollName = GenerateRandomName();

            // Set gameobject name
            gameObject.name = $"Minipoll_{minipollName}_{uniqueID.Substring(0, 8)}";
        }

        private void InitializeSystems()
        {
            try
            {
                // Debug logging removed to reduce console spam

                // אתחול מערכת הצרכים
                InitializeNeedsSystem();

                // אתחול מערכת הרגשות
                InitializeEmotionsSystem();

                // אתחול מערכת הזיכרון
                InitializeMemorySystem();

                // אתחול מערכת התנועה
                InitializeMovementSystem();

                // אתחול מערכת החזותיים
                InitializeVisualSystem();

                // אתחול מערכת ה-AI
                InitializeAISystem();

                // Debug logging removed to reduce console spam
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MinipollCore] Failed to initialize systems: {e.Message}");
            }
        }
        private void InitializeNeedsSystem()
        {
            var needsSystem = GetComponent<MinipollNeedsSystem>();
            if (needsSystem == null)
            {
                needsSystem = gameObject.AddComponent<MinipollNeedsSystem>();
            }
            // Debug logging removed to reduce console spam
        }

        /// <summary>
        ///אתחול מערכת רגשות
        /// </summary>
        private void InitializeEmotionsSystem()
        {
            var emotionsSystem = GetComponent<MinipollEmotionsSystem>();
            if (emotionsSystem == null)
            {
                emotionsSystem = gameObject.AddComponent<MinipollEmotionsSystem>();
            }
            // Debug logging removed to reduce console spam
        }

        /// <summary>
        ///אתחול מערכת זיכרון
        /// </summary>
        private void InitializeMemorySystem()
        {
            var memorySystem = GetComponent<MinipollMemorySystem>();
            if (memorySystem == null)
            {
                memorySystem = gameObject.AddComponent<MinipollMemorySystem>();
            }
            // Debug logging removed to reduce console spam
        }

        /// <summary>
        ///אתחול מערכת תנועה
        /// </summary>
        private void InitializeMovementSystem()
        {
            var movementController = GetComponent<MinipollMovementController>();
            if (movementController == null)
            {
                movementController = gameObject.AddComponent<MinipollMovementController>();
            }
            // Debug logging removed to reduce console spam
        }

        /// <summary>
        ///אתחול מערכת חזותיים
        /// </summary>
        private void InitializeVisualSystem()
        {
            var visualController = GetComponent<MinipollVisualController>();
            if (visualController == null)
            {
                visualController = gameObject.AddComponent<MinipollVisualController>();
            }
            // Debug logging removed to reduce console spam
        }

        /// <summary>
        ///אתחול מערכת AI
        /// </summary>
        private void InitializeAISystem()
        {
            // בדיקה אם יש מערכת AI
            var aiComponent = GetComponent<MonoBehaviour>(); // כל רכיב AI
            if (aiComponent == null)
            {
                // Debug logging removed to reduce console spam
                return;
            }
            // Debug logging removed to reduce console spam
        }

        #region Event Management
        private void RegisterEventListeners()
        {
            // Health events
            if (health)
            {
                health.OnDeath += HandleDeath;
                health.OnDamaged += HandleDamage;
                health.OnHealed += HandleHeal;
                health.OnHealthChanged += HandleHealthChanged;
            }

            // Needs events
            if (needs)
            {
                needs.OnNeedCritical += HandleCriticalNeed;
                needs.OnNeedSatisfied += HandleNeedSatisfied;
            }

            // Emotion events
            if (emotions)
            {
                emotions.OnEmotionChanged += HandleEmotionChange;
                emotions.OnMoodChanged += HandleMoodChange;
            }

            // Selection events
            if (selectable)
            {
                selectable.OnSelected += HandleSelected;
                selectable.OnDeselected += HandleDeselected;
            }
        }
        #endregion
        private void UnregisterEventListeners()
        {
            if (health)
            {
                health.OnDeath -= HandleDeath;
                health.OnDamaged -= HandleDamage;
                health.OnHealed -= HandleHeal;
                health.OnHealthChanged -= HandleHealthChanged;
            }

            if (needs)
            {
                needs.OnNeedCritical -= HandleCriticalNeed;
                needs.OnNeedSatisfied -= HandleNeedSatisfied;
            }

            if (emotions)
            {
                emotions.OnEmotionChanged -= HandleEmotionChange;
                emotions.OnMoodChanged -= HandleMoodChange;
            }

            if (selectable)
            {
                selectable.OnSelected -= HandleSelected;
                selectable.OnDeselected -= HandleDeselected;
            }
        }
        #endregion

        #region Update Loop
        private void Update()
        {
            if (!IsAlive) return;

            // Update age
            if (Time.time >= nextAgeUpdateTime)
            {
                UpdateAge();
                nextAgeUpdateTime = Time.time + AGE_UPDATE_INTERVAL;
            }
        }

        private void UpdateAge()
        {
            // Increase age
            float ageIncrease = AGE_UPDATE_INTERVAL * ageProgressionSpeed;
            age += ageIncrease;
            OnAgeIncreased?.Invoke(age);

            // Check for age stage transition
            AgeStage newStage = CalculateAgeStage();
            if (newStage != CurrentAgeStage)
            {
                TransitionToAgeStage(newStage);
            }

            // Check for natural death from old age
            if (age >= maxAge)
            {
                HandleNaturalDeath();
            }
        }

        private AgeStage CalculateAgeStage()
        {
            if (age < babyDuration) return AgeStage.Baby;
            if (age < babyDuration + childDuration) return AgeStage.Child;
            if (age < babyDuration + childDuration + adultDuration) return AgeStage.Adult;
            return AgeStage.Elder;
        }

        private void TransitionToAgeStage(AgeStage newStage)
        {
            AgeStage oldStage = CurrentAgeStage;
            CurrentAgeStage = newStage; Debug.Log($"[MinipollCore] {minipollName} grew from {oldStage} to {newStage}!");

            // Update systems
            if (stats) stats.TransitionToAgeStage(ConvertToSystemsAgeStage(newStage));
            if (visualController) visualController.UpdateForAgeStage(ConvertToSystemsAgeStage(newStage));
            if (movement) movement.UpdateForAgeStage(newStage);

            // Fire event
            OnAgeStageChanged?.Invoke(oldStage, newStage);

            // Special handling for transitions
            if (newStage == AgeStage.Adult && socialRelations)
            {
                socialRelations.EnableMating(true);
            }
        }
        #endregion

        #region Event Handlers
        private void HandleDeath()
        {
            if (!IsAlive) return;

            IsAlive = false;
            Debug.Log($"[MinipollCore] {minipollName} has died at age {age:F0}");

            // Stop all systems
            DisableAllSystems();

            // Play death effects
            if (visualController) visualController.PlayDeathAnimation();

            // Notify manager and events
            OnMinipollDied?.Invoke(this);

            if (MinipollManager.Instance)
                MinipollManager.Instance.UnregisterMinipoll(this);

            // Move to corpse pool after delay
            StartCoroutine(MoveToCorpsePoolRoutine());
        }

        private void HandleNaturalDeath()
        {
            Debug.Log($"[MinipollCore] {minipollName} died of old age at {age:F0}");
            if (health) health.Kill();
        }

        private void HandleDamage(float damage, GameObject source)
        {
            // Emotional response
            if (emotions)
            {
                emotions.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Afraid, 0.5f, source);
                if (damage > stats.MaxHealth * 0.3f) // Heavy damage
                    emotions.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Angry, 0.7f, source);
            }

            // Memory of attacker
            if (memory && source)
            {
                MinipollCore attacker = source.GetComponent<MinipollCore>();
                if (attacker)
                {
                    // Use correct overload: MinipollMemorySystem.RememberInteraction(MinipollGame.Core.MinipollCore, InteractionType, float)
                    memory.RememberInteraction(attacker, InteractionType.Attacked, -1f);
                    if (socialRelations)
                        socialRelations.ModifyRelationship(attacker.Brain, -0.5f);
                }
            }

            // Visual/Audio feedback
            if (visualController) visualController.PlayHurtAnimation();
        }
        private void HandleHeal(float amount, GameObject source)
        {
            if (emotions)
                emotions.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Happy, 0.3f, source);

            if (memory && source)
            {
                MinipollCore healer = source.GetComponent<MinipollCore>();
                if (healer)
                {
                    memory.RememberInteraction(healer, InteractionType.Helped, 0.5f);
                    if (socialRelations)
                        socialRelations.ModifyRelationship(healer, 0.3f);
                }
            }
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            // Update UI if selected
            if (IsSelected && UIManager.Instance)
            {
                UIManager.Instance.UpdateSelectedMinipollHealth(currentHealth / maxHealth);
            }
        }

        private void HandleCriticalNeed(NeedType needType)
        {
            Debug.LogWarning($"[MinipollCore] {minipollName} has critical {needType}!");

            // Tell brain to prioritize this need
            if (brain)
                brain.SetUrgentGoal(needType); // MinipollBrain.SetUrgentGoal(NeedType)
            // Emotional response
            if (emotions)
            {
                switch (needType)
                {
                    case NeedType.Hunger:
                    case NeedType.Thirst:
                        emotions.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Sad, 0.7f);
                        break;
                    case NeedType.Sleep:
                        emotions.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Sad, 0.9f);
                        break;
                    case NeedType.Social:
                        emotions.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Sad, 0.5f);
                        break;
                }
            }
        }
        private void HandleNeedSatisfied(NeedType needType)
        {
            // Debug logging removed to reduce console spam

            if (emotions)
                emotions.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Happy, 0.5f);
        }
        private void HandleEmotionChange(MinipollEmotionsSystem.EmotionType emotion, float intensity)
        {
            // Update visuals
            if (visualController)
            {
                // Cast to be more explicit about the method call
                visualController.SetEmotionalState((MinipollEmotionsSystem.EmotionType)emotion);
            }

            // Update movement based on emotion
            if (movement)
            {
                switch (emotion)
                {
                    case MinipollEmotionsSystem.EmotionType.Happy:
                        movement.SetSpeedMultiplier(1.2f);
                        break;
                    case MinipollEmotionsSystem.EmotionType.Scared:
                        movement.SetSpeedMultiplier(0.6f);
                        break;
                    case MinipollEmotionsSystem.EmotionType.Afraid:
                        movement.SetSpeedMultiplier(1.5f);
                        break;
                    default:
                        movement.SetSpeedMultiplier(1f);
                        break;
                }
            }
        }
        private void HandleMoodChange(MinipollEmotionsSystem.EmotionType mood)
        {
            // Mood affects general behavior
            if (brain)
                brain.SetMoodInfluence(0.5f); // Use a default mood influence value
        }

        private void HandleSelected()
        {
            SelectedMinipoll = this;
            OnMinipollSelected?.Invoke(this);

            // Update UI
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMinipollPanel(this);
        }

        private void HandleDeselected()
        {
            if (SelectedMinipoll == this)
                SelectedMinipoll = null;

            OnMinipollDeselected?.Invoke(this);

            // Update UI
            if (UIManager.Instance)
                UIManager.Instance.HideMinipollPanel();
        }
        #endregion

        #region Public Methods
        public void SetName(string newName)
        {
            string oldName = minipollName;
            minipollName = newName;
            gameObject.name = $"Minipoll_{newName}_{uniqueID.Substring(0, 8)}";
            OnNameChanged?.Invoke(oldName, newName);
        }

        public void SetPrimaryColor(Color color)
        {
            primaryColor = color;
            if (visualController)
                visualController.SetPrimaryColor(color);
        }
        // public void Feed(FoodItem food)
        // {
        //     if (!IsAlive || !food) return;

        //     if (needs)
        //         needs.Eat(food.NutritionValue);

        //     if (emotions)
        //         emotions.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Happy, 0.4f);

        //     Debug.Log($"[MinipollCore] {minipollName} ate {food.FoodName}");
        // }

        public void GiveWater(float amount)
        {
            if (!IsAlive) return;

            if (needs)
                needs.Drink(amount);
        }

        public void Sleep(float quality)
        {
            if (!IsAlive) return;

            if (needs)
                needs.Sleep(quality);

            if (visualController)
                visualController.PlaySleepAnimation();
        }

        public void InteractWith(MinipollCore other)
        {
            if (!IsAlive || !other || !other.IsAlive || other == this) return;

            // Memory
            if (memory)
                memory.RememberInteraction(other, InteractionType.Friendly, 0.1f);

            // Social
            if (socialRelations)
                socialRelations.ProcessSocialInteraction(other);

            // Needs
            if (needs)
                needs.Socialize(20f);

            // Debug logging removed to reduce console spam
        }
        public void Breed(MinipollCore partner)
        {
            if (!CanBreed() || !partner || !partner.CanBreed()) return;

            // TODO: Implement reproduction system
            Debug.Log($"[MinipollCore] {minipollName} is attempting to breed with {partner.minipollName}");

            // Placeholder for breeding logic
            // if (MinipollReproductionSystemEvents.Instance != null)
            // {
            //     MinipollReproductionSystemEvents.Instance.AttemptBreeding(this, partner);
            // }
        }

        public bool CanBreed()
        {
            return IsAlive &&
                   CurrentAgeStage == AgeStage.Adult &&
                   health.CurrentHealth > health.MaxHealth * 0.5f &&
                   needs.GetNeedValue(NeedType.Hunger) > 50f;
        }
        #endregion

        #region Helper Methods
        private void DisableAllSystems()
        {
            if (brain) brain.enabled = false;
            if (movement) movement.enabled = false;
            if (needs) needs.enabled = false;
            if (emotions) emotions.enabled = false;
            if (worldInteraction) worldInteraction.enabled = false;
            if (blinkController) blinkController.enabled = false;

            // Disable physics
            Rigidbody rb = GetComponentInChildren<Rigidbody>();
            if (rb) rb.isKinematic = true;

            // Disable colliders except trigger for selection
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                    col.enabled = false;
            }
        }

        private IEnumerator MoveToCorpsePoolRoutine()
        {
            yield return new WaitForSeconds(3f); // Wait for death animation

            // Move to corpse pool or destroy
            if (MinipollManager.Instance && MinipollManager.Instance.UseCorpsePool)
            {
                MinipollManager.Instance.MoveToCorpsePool(this);
            }
            else
            {
                Destroy(gameObject, 5f); // Destroy after 5 more seconds
            }
        }

        private string GenerateRandomName()
        {
            string[] prefixes = { "Blob", "Puff", "Squish", "Bounce", "Wiggle", "Bubble", "Fuzzy", "Squishy" };
            string[] suffixes = { "y", "ie", "ster", "ling", "bert", "ina", "o", "a" };

            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];

            // Add number if name exists
            string baseName = prefix + suffix;
            int counter = 1;
            string finalName = baseName;

            // Check if name exists in scene
            MinipollCore[] allMinipolls = FindObjectsByType<MinipollCore>(FindObjectsSortMode.None);
            bool nameExists = true;

            while (nameExists)
            {
                nameExists = false;
                foreach (var m in allMinipolls)
                {
                    if (m != this && m.minipollName == finalName)
                    {
                        nameExists = true;
                        counter++;
                        finalName = baseName + counter;
                        break;
                    }
                }
            }

            return finalName;
        }
        #endregion

        #region Debug
        private void OnDestroy()
        {
            UnregisterEventListeners();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw info
            Gizmos.color = IsAlive ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // Draw detection range
            if (worldInteraction)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 5f);
            }

            // Draw social connections
            if (socialRelations && socialRelations.GetFriends() != null)
            {
                Gizmos.color = Color.cyan; foreach (var friend in socialRelations.GetFriends())
                {
                    if (friend != null)
                        Gizmos.DrawLine(transform.position, friend.transform.position);
                }
            }

            // Draw name
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
                $"{minipollName}\n{CurrentAgeStage} ({age:F0})\n{(IsAlive ? "Alive" : "Dead")}");
#endif
        }
        internal void ConsumeEnergy(float energyCost)
        {
            if (needs != null)
            {
                // Use the needs system to consume energy
                needs.UpdateNeeds(energyCost);
            }

            if (health != null)
            {
                // Alternative: reduce health slightly when energy is consumed
                health.TakeDamage(energyCost * 0.1f);
            }
        }
        internal void Heal(float healAmount)
        {
            if (health != null)
            {
                health.Heal(healAmount);

                // Trigger happiness if healing is significant
                if (emotions != null && healAmount > 5f)
                {
                    emotions.OnEmotionChanged?.Invoke(MinipollEmotionsSystem.EmotionType.Happy, 0.3f);
                }
            }
        }

        internal void SetGender(Gender gender)
        {
            Gender = gender;

            // Update visual representation if needed
            if (visualController != null)
            {
                // Could update colors or models based on gender
                visualController.SetGender(gender);
            }

            // Update genetics if available
            MinipollGeneticsSystem genetics = GetComponentInChildren<MinipollGeneticsSystem>();
            if (genetics != null)
            {
                // Gender affects some genetic traits
                genetics.SetGender(gender);
            }
        }

        public void SetAgeStage(AgeStage newAge)
        {
            AgeStage oldAge = AgeStage.Adult;
            ageStage = newAge;
            OnAgeStageChanged?.Invoke(oldAge, newAge);
        }
        internal void SetRandomName()
        {
            minipollName = GenerateRandomName();
            gameObject.name = $"Minipoll_{minipollName}_{uniqueID.Substring(0, 8)}";
        }

        #endregion

        /// <summary>
        /// Convert from core AgeStage to Systems.Core.AgeStage
        /// Note: Using the proper AgeStage enum from MinipollEnums.cs
        /// </summary>
        private MinipollGame.Systems.Core.AgeStage ConvertToSystemsAgeStage(AgeStage coreAgeStage)
        {
            // Direct enum conversion - both enums have the same values
            switch (coreAgeStage)
            {
                case AgeStage.Baby:
                    return MinipollGame.Systems.Core.AgeStage.Baby;
                case AgeStage.Child:
                    return MinipollGame.Systems.Core.AgeStage.Child;
                case AgeStage.Adult:
                    return MinipollGame.Systems.Core.AgeStage.Adult;
                case AgeStage.Elder:
                    return MinipollGame.Systems.Core.AgeStage.Elder;
                default:
                    return MinipollGame.Systems.Core.AgeStage.Adult;
            }
        }
    }

}