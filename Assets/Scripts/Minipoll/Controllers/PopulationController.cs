using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MinipollGame.Core;
using MinipollGame.Systems.Core;
using MinipollGame.Managers;

/// <summary>
/// PopulationController - מנהל את כל אוכלוסיית המיניפולים במשחק
/// אחראי על spawn, despawn, ניהול מספרים, רבייה, מחזור חיים ואיזון
/// </summary>
namespace MinipollGame.Controllers
{
    public class PopulationController : MonoBehaviour
    {
        #region Singleton
        public static PopulationController Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeController();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Population Settings
        [Header("=== Population Limits ===")]
        [SerializeField] private int maxPopulation = 100;
        [SerializeField] private int startingPopulation = 10;
        [SerializeField] private int minPopulation = 5;
        [SerializeField] private int targetPopulation = 50;

        [Header("=== Spawn Settings ===")]
        [SerializeField] private GameObject minipollPrefab;
        [SerializeField] private Transform spawnParent;
        [SerializeField] private LayerMask spawnLayerMask = -1;
        [SerializeField] private float spawnRadius = 50f;
        [SerializeField] private Vector3 worldCenter = Vector3.zero;

        [Header("=== Population Control ===")]
        [Range(0f, 1f)]
        [SerializeField] private float reproductionRate = 0.1f;
        [Range(0f, 1f)]
        [SerializeField] private float naturalDeathRate = 0.02f;
        [Range(0f, 1f)]
        [SerializeField] private float migrationRate = 0.05f;

        [Header("=== Balancing ===")]
        [SerializeField] private bool autoBalance = true;
        [SerializeField] private float balanceCheckInterval = 30f;
        [SerializeField] private bool allowNaturalGrowth = true;
        [SerializeField] private bool allowNaturalDeath = true;

        [Header("=== Debug ===")]
        [SerializeField] private bool verboseLogging = false;
        [SerializeField] private bool showGizmos = true;
        #endregion

        #region Population Data
        private List<MinipollBrain> allMinipolls = new List<MinipollBrain>();
        private Dictionary<AgeStage, List<MinipollBrain>> populationByAge = new Dictionary<AgeStage, List<MinipollBrain>>();
        private Dictionary<Gender, List<MinipollBrain>> populationByGender = new Dictionary<Gender, List<MinipollBrain>>();
        
        private float nextBalanceCheck;
        private float nextReproductionCheck;
        private float nextMigrationCheck;
        
        // Statistics
        private int totalBirths = 0;
        private int totalDeaths = 0;
        private int totalMigrations = 0;
        #endregion

        #region Events
        public static event Action<MinipollBrain> OnMinipollSpawned;
        public static event Action<MinipollBrain> OnMinipollDied;
        public static event Action<MinipollBrain, MinipollBrain> OnMinipollReproduced;
        public static event Action<int> OnPopulationChanged;
        public static event Action<PopulationStats> OnPopulationStatsUpdated;
        #endregion

        #region Initialization
        private void InitializeController()
        {
            // Initialize dictionaries
            foreach (AgeStage age in Enum.GetValues(typeof(AgeStage)))
            {
                populationByAge[age] = new List<MinipollBrain>();
            }
            
            foreach (Gender gender in Enum.GetValues(typeof(Gender)))
            {
                if (gender != Gender.Random)
                    populationByGender[gender] = new List<MinipollBrain>();
            }

            // Set initial timing
            nextBalanceCheck = Time.time + balanceCheckInterval;
            nextReproductionCheck = Time.time + UnityEngine.Random.Range(5f, 15f);
            nextMigrationCheck = Time.time + UnityEngine.Random.Range(20f, 40f);

            if (verboseLogging)
                Debug.Log("[PopulationController] Initialized successfully");
        }

        private void Start()
        {
            // Find existing minipolls in scene
            // RegisterExistingMinipolls();
            
            // Spawn initial population if needed
            if (allMinipolls.Count < startingPopulation)
            {
                SpawnInitialPopulation();
            }
        }
        #endregion

        #region Update & Management
        private void Update()
        {
            float currentTime = Time.time;

            // Population balance check
            if (autoBalance && currentTime >= nextBalanceCheck)
            {
                PerformBalanceCheck();
                nextBalanceCheck = currentTime + balanceCheckInterval;
            }

            // Natural reproduction
            if (allowNaturalGrowth && currentTime >= nextReproductionCheck)
            {
                CheckForNaturalReproduction();
                nextReproductionCheck = currentTime + UnityEngine.Random.Range(10f, 30f);
            }

            // Migration events
            if (currentTime >= nextMigrationCheck)
            {
                CheckForMigration();
                nextMigrationCheck = currentTime + UnityEngine.Random.Range(60f, 120f);
            }

            // Clean up dead references
            CleanupDeadReferences();
        }

        private void PerformBalanceCheck()
        {
            int currentPopulation = GetAliveCount();
            
            if (verboseLogging)
                Debug.Log($"[PopulationController] Balance check: {currentPopulation}/{targetPopulation}");

            // Too few - spawn more
            if (currentPopulation < minPopulation)
            {
                int toSpawn = minPopulation - currentPopulation;
                for (int i = 0; i < toSpawn; i++)
                {
                    SpawnRandomMinipoll();
                }
                
                if (verboseLogging)
                    Debug.Log($"[PopulationController] Emergency spawn: {toSpawn} minipolls");
            }
            // Too many - natural selection
            else if (currentPopulation > maxPopulation && allowNaturalDeath)
            {
                PerformNaturalSelection();
            }

            UpdatePopulationStats();
        }

        private void CleanupDeadReferences()
        {
            // Remove dead or null references
            allMinipolls.RemoveAll(m => m == null || !m.IsAlive);
            
            foreach (var ageGroup in populationByAge.Values)
            {
                ageGroup.RemoveAll(m => m == null || !m.IsAlive);
            }
            
            foreach (var genderGroup in populationByGender.Values)
            {
                genderGroup.RemoveAll(m => m == null || !m.IsAlive);
            }
        }
        #endregion

        #region Population Management
        public MinipollBrain SpawnRandomMinipoll()
        {
            if (minipollPrefab == null)
            {
                Debug.LogError("[PopulationController] No minipoll prefab assigned!");
                return null;
            }

            Vector3 spawnPosition = GetRandomSpawnPosition();
            return SpawnMinipoll(spawnPosition);
        }

        public MinipollBrain SpawnMinipoll(Vector3 position)
        {
            return SpawnMinipoll(position, GetRandomGender(), AgeStage.Adult);
        }

        public MinipollBrain SpawnMinipoll(Vector3 position, Gender gender, AgeStage age)
        {
            if (allMinipolls.Count >= maxPopulation)
            {
                if (verboseLogging)
                    Debug.LogWarning("[PopulationController] Population limit reached!");
                return null;
            }

            // Instantiate
            GameObject minipollObj = Instantiate(minipollPrefab, position, Quaternion.identity, spawnParent);
            MinipollBrain newMinipoll = minipollObj.GetComponent<MinipollBrain>();
            
            if (newMinipoll == null)
            {
                Debug.LogError("[PopulationController] Spawned minipoll has no MinipollBrain component!");
                Destroy(minipollObj);
                return null;
            }

            // Register with NEEDSIM system if NEEDSIMNode component exists
            // var needsimNode = minipollObj.GetComponent<NEEDSIM.NEEDSIMNode>();
            // if (needsimNode != null && NEEDSIM.NEEDSIMRoot.Instance != null)
            // {
            //     NEEDSIM.NEEDSIMRoot.Instance.AddNEEDSIMNode(needsimNode);
            //     needsimNode.Setup();
                
            //     if (verboseLogging)
            //         Debug.Log($"[PopulationController] Registered minipoll with NEEDSIM system: {newMinipoll.Name}");
            // }

            // Configure
            Core.MinipollCore core = newMinipoll.GetCore();
            if (core != null)
            {
                // Set properties (simplified for now)
                core.SetGender(gender);
                core.SetAgeStage(age);
                core.SetRandomName();
            }

            // Register
            RegisterMinipoll(newMinipoll);
            
            totalBirths++;
            
            if (verboseLogging)
                Debug.Log($"[PopulationController] Spawned {gender} {age} minipoll at {position}");

            // Events
            OnMinipollSpawned?.Invoke(newMinipoll);
            OnPopulationChanged?.Invoke(GetAliveCount());

            return newMinipoll;
        }

        public void RegisterMinipoll(MinipollBrain minipoll)
        {
            if (minipoll == null || allMinipolls.Contains(minipoll))
                return;

            allMinipolls.Add(minipoll);            // Subscribe to events
            var core = minipoll.GetCore();
            if (core != null)
            {
                MinipollGame.Core.MinipollCore.OnMinipollDied += HandleMinipollDeath;
                core.OnAgeStageChanged += (oldAge, newAge) => UpdateAgeCategories(minipoll);
            }

            UpdateCategories(minipoll);
            
            if (verboseLogging)
                Debug.Log($"[PopulationController] Registered minipoll: {minipoll.Name}");
        }

        public void UnregisterMinipoll(MinipollBrain minipoll)
        {
            if (minipoll == null)
                return;

            allMinipolls.Remove(minipoll);
            
            // Remove from categories
            foreach (var ageGroup in populationByAge.Values)
                ageGroup.Remove(minipoll);
            
            foreach (var genderGroup in populationByGender.Values)
                genderGroup.Remove(minipoll);            // Unsubscribe from events
            var core = minipoll.GetCore();
            if (core != null)
            {
                MinipollGame.Core.MinipollCore.OnMinipollDied -= HandleMinipollDeath;
            }

            if (verboseLogging)
                Debug.Log($"[PopulationController] Unregistered minipoll: {minipoll.Name}");

            OnPopulationChanged?.Invoke(GetAliveCount());
        }        private void HandleMinipollDeath(Core.MinipollCore core)
        {
            // Find the minipoll brain associated with this core
            var deadMinipoll = allMinipolls.FirstOrDefault(m => m.GetCore() == core);
            if (deadMinipoll != null)
            {
                UnregisterMinipoll(deadMinipoll);
                totalDeaths++;
                
                if (verboseLogging)
                    Debug.Log($"[PopulationController] {deadMinipoll.Name} has died");
                
                OnPopulationChanged?.Invoke(GetAliveCount());
            }
        }
        #endregion

        #region Reproduction & Death
        private void CheckForNaturalReproduction()
        {
            if (GetAliveCount() >= maxPopulation)
                return;

            var adults = GetMinipollsByAge(AgeStage.Adult);
            var availableMales = adults.Where(m => m.GetCore()?.Gender == Gender.Male).ToList();
            var availableFemales = adults.Where(m => m.GetCore()?.Gender == Gender.Female).ToList();

            if (availableMales.Count == 0 || availableFemales.Count == 0)
                return;

            // Check for reproduction opportunities
            foreach (var female in availableFemales)
            {
                if (UnityEngine.Random.value < reproductionRate)
                {
                    var nearbyMale = FindNearbyMale(female, availableMales);
                    if (nearbyMale != null)
                    {
                        AttemptReproduction(nearbyMale, female);
                    }
                }
            }
        }

        private MinipollBrain FindNearbyMale(MinipollBrain female, List<MinipollBrain> males)
        {
            const float maxDistance = 20f;
            
            return males
                .Where(male => Vector3.Distance(male.transform.position, female.transform.position) <= maxDistance)
                .OrderBy(male => Vector3.Distance(male.transform.position, female.transform.position))
                .FirstOrDefault();
        }

        private void AttemptReproduction(MinipollBrain male, MinipollBrain female)
        {
            if (GetAliveCount() >= maxPopulation)
                return;

            // Create offspring
            Vector3 birthPosition = Vector3.Lerp(male.transform.position, female.transform.position, 0.5f);
            Gender babyGender = UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;
            
            var baby = SpawnMinipoll(birthPosition, babyGender, AgeStage.Baby);
            
            if (baby != null)
            {
                if (verboseLogging)
                    Debug.Log($"[PopulationController] {male.Name} and {female.Name} had a baby!");

                OnMinipollReproduced?.Invoke(male, female);
            }
        }

        private void PerformNaturalSelection()
        {
            var aliveMinipolls = allMinipolls.Where(m => m != null && m.IsAlive).ToList();
            int excessCount = aliveMinipolls.Count - maxPopulation;
            
            if (excessCount <= 0)
                return;            // Prioritize older, weaker minipolls for natural death
            var candidates = aliveMinipolls
                .Where(static m => 
                {
                    var core = m.GetCore();
                    return core?.CurrentAgeStage.ToString() == "Elder" || m.Health < 30f;
                })
                .OrderBy(m => m.Health)
                .Take(excessCount)
                .ToList();

            foreach (var candidate in candidates)
            {
                if (UnityEngine.Random.value < naturalDeathRate)
                {
                    candidate.TakeDamage(candidate.Health); // Natural death
                    
                    if (verboseLogging)
                        Debug.Log($"[PopulationController] {candidate.Name} died from natural causes");
                }
            }
        }

        private void HandleMinipollDeath(MinipollBrain deadMinipoll)
        {
            totalDeaths++;
            UnregisterMinipoll(deadMinipoll);
            
            if (verboseLogging)
                Debug.Log($"[PopulationController] {deadMinipoll.Name} has died. Population: {GetAliveCount()}");

            OnMinipollDied?.Invoke(deadMinipoll);
        }
        #endregion

        #region Migration & Events
        private void CheckForMigration()
        {
            if (UnityEngine.Random.value > migrationRate)
                return;

            bool isImmigration = UnityEngine.Random.value > 0.5f;
            
            if (isImmigration && GetAliveCount() < maxPopulation)
            {
                // Immigration - new minipoll arrives
                SpawnRandomMinipoll();
                totalMigrations++;
                
                if (verboseLogging)
                    Debug.Log("[PopulationController] A new minipoll immigrated to the area");
            }
            else if (!isImmigration && GetAliveCount() > minPopulation)
            {
                // Emigration - random minipoll leaves
                var randomMinipoll = allMinipolls.Where(m => m != null && m.IsAlive).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                if (randomMinipoll != null)
                {
                    EmigateMinipoll(randomMinipoll);
                }
            }
        }

        private void EmigateMinipoll(MinipollBrain minipoll)
        {
            totalMigrations++;
            
            if (verboseLogging)
                Debug.Log($"[PopulationController] {minipoll.Name} emigrated from the area");

            UnregisterMinipoll(minipoll);
            Destroy(minipoll.gameObject);
        }
        #endregion

        #region Utility Methods
        // private void RegisterExistingMinipolls()
        // {
        //     MinipollBrain[] existingMinipolls = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None);
        //     foreach (MinipollBrain minipoll in existingMinipolls)
        //     {
        //         if (minipoll.IsAlive)
        //         {
        //             // Register with NEEDSIM system if NEEDSIMNode component exists
        //             var needsimNode = minipoll.GetComponent<NEEDSIM.NEEDSIMNode>();
        //             if (needsimNode != null && NEEDSIM.NEEDSIMRoot.Instance != null)
        //             {
        //                 // Check if not already registered (AffordanceTreeNode is null)
        //                 if (needsimNode.AffordanceTreeNode == null)
        //                 {
        //                     NEEDSIM.NEEDSIMRoot.Instance.AddNEEDSIMNode(needsimNode);
        //                     needsimNode.Setup();
                            
        //                     if (verboseLogging)
        //                         Debug.Log($"[PopulationController] Registered existing minipoll with NEEDSIM system: {minipoll.Name}");
        //                 }
        //             }
                    
        //             RegisterMinipoll(minipoll);
        //         }
        //     }
            
        //     if (verboseLogging)
        //         Debug.Log($"[PopulationController] Found {existingMinipolls.Length} existing minipolls");
        // }

        private void SpawnInitialPopulation()
        {
            int toSpawn = startingPopulation - allMinipolls.Count;
            
            for (int i = 0; i < toSpawn; i++)
            {
                SpawnRandomMinipoll();
            }
            
            if (verboseLogging)
                Debug.Log($"[PopulationController] Spawned {toSpawn} initial minipolls");
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0; // Keep on ground level
            
            Vector3 spawnPos = worldCenter + randomOffset;
            
            // Raycast to ground
            if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, spawnLayerMask))
            {
                spawnPos.y = hit.point.y;
            }
            
            return spawnPos;
        }

        private Gender GetRandomGender()
        {
            return UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;
        }

        private void UpdateCategories(MinipollBrain minipoll)
        {
            UpdateAgeCategories(minipoll);
            UpdateGenderCategories(minipoll);
        }        private void UpdateAgeCategories(MinipollBrain minipoll)
        {
            // Remove from all age groups
            foreach (var ageGroup in populationByAge.Values)
                ageGroup.Remove(minipoll);
            
            // Add to correct age group
            var core = minipoll.GetCore();
            if (core != null)
            {
                // Convert from MinipollCore.AgeStage to Systems.Core.AgeStage
                var coreAgeStage = core.CurrentAgeStage;
                AgeStage systemsAgeStage = ConvertCoreAgeStageToSystems(coreAgeStage);
                populationByAge[systemsAgeStage].Add(minipoll);
            }
        }

        private void UpdateGenderCategories(MinipollBrain minipoll)
        {
            // Remove from all gender groups
            foreach (var genderGroup in populationByGender.Values)
                genderGroup.Remove(minipoll);
            
            // Add to correct gender group
            var core = minipoll.GetCore();
            if (core != null && core.Gender != Gender.Random)
            {
                populationByGender[core.Gender].Add(minipoll);
            }
        }

        private void UpdatePopulationStats()
        {
            var stats = new PopulationStats
            {
                totalPopulation = GetAliveCount(),
                babies = GetMinipollsByAge(AgeStage.Baby).Count,
                children = GetMinipollsByAge(AgeStage.Child).Count,
                adults = GetMinipollsByAge(AgeStage.Adult).Count,
                elders = GetMinipollsByAge(AgeStage.Elder).Count,
                males = GetMinipollsByGender(Gender.Male).Count,
                females = GetMinipollsByGender(Gender.Female).Count,
                totalBirths = this.totalBirths,
                totalDeaths = this.totalDeaths,
                totalMigrations = this.totalMigrations
            };

            OnPopulationStatsUpdated?.Invoke(stats);
        }        /// <summary>
        /// Convert from MinipollCore's AgeStage to Systems.Core.AgeStage
        /// </summary>
        private AgeStage ConvertCoreAgeStageToSystems(object coreAgeStage)
        {
            // Since we have namespace conflicts, convert by string comparison
            string ageStageString = coreAgeStage.ToString();
            return ageStageString switch
            {
                "Baby" => AgeStage.Baby,
                "Child" => AgeStage.Child,
                "Adult" => AgeStage.Adult,
                "Elder" => AgeStage.Elder,
                _ => AgeStage.Adult
            };
        }
        #endregion

        #region Public API
        public int GetAliveCount() => allMinipolls.Count(m => m != null && m.IsAlive);
        public List<MinipollBrain> GetAllMinipolls() => new List<MinipollBrain>(allMinipolls);
        public List<MinipollBrain> GetAliveMinipolls() => allMinipolls.Where(m => m != null && m.IsAlive).ToList();
        
        public List<MinipollBrain> GetMinipollsByAge(AgeStage age)
        {
            return populationByAge.ContainsKey(age) ? 
                populationByAge[age].Where(m => m != null && m.IsAlive).ToList() : 
                new List<MinipollBrain>();
        }
        
        public List<MinipollBrain> GetMinipollsByGender(Gender gender)
        {
            return populationByGender.ContainsKey(gender) ? 
                populationByGender[gender].Where(m => m != null && m.IsAlive).ToList() : 
                new List<MinipollBrain>();
        }

        public void SetPopulationLimits(int min, int target, int max)
        {
            minPopulation = min;
            targetPopulation = target;
            maxPopulation = max;
            
            if (verboseLogging)
                Debug.Log($"[PopulationController] Population limits set: {min}/{target}/{max}");
        }

        public void ForceSpawn(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnRandomMinipoll();
            }
        }

        public void ForceBalance()
        {
            PerformBalanceCheck();
        }

        public PopulationStats GetCurrentStats()
        {
            var stats = new PopulationStats
            {
                totalPopulation = GetAliveCount(),
                babies = GetMinipollsByAge(AgeStage.Baby).Count,
                children = GetMinipollsByAge(AgeStage.Child).Count,
                adults = GetMinipollsByAge(AgeStage.Adult).Count,
                elders = GetMinipollsByAge(AgeStage.Elder).Count,
                males = GetMinipollsByGender(Gender.Male).Count,
                females = GetMinipollsByGender(Gender.Female).Count,
                totalBirths = totalBirths,
                totalDeaths = totalDeaths,
                totalMigrations = totalMigrations
            };
            
            return stats;
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            // Draw spawn area
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(worldCenter, spawnRadius);
            
            // Draw population count
            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(worldCenter + Vector3.up * 10f, 
                    $"Population: {GetAliveCount()}/{maxPopulation}\n" +
                    $"Births: {totalBirths} | Deaths: {totalDeaths}");
#endif
            }
        }
        #endregion
    }

    #region Data Structures
    [System.Serializable]
    public struct PopulationStats
    {
        public int totalPopulation;
        public int babies;
        public int children;
        public int adults;
        public int elders;
        public int males;
        public int females;
        public int totalBirths;
        public int totalDeaths;
        public int totalMigrations;
    }
    #endregion
}