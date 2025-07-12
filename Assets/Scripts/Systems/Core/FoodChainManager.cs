using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MinipollGame.Core;
using Unity.VisualScripting;
using UnityEngine;

namespace MinipollGame.Systems.Core
{
    // Event classes for food chain system
    public class EntityBornEvent
    {
        public GameObject entity;
        public string species;
    }

    public class EntityDiedEvent
    {
        public GameObject entity;
    }

    public class EntityAteEvent
    {
        public GameObject predator;
        public GameObject prey;
        public float energy;
    }

    public class PlantGrownEvent
    {
        public GameObject plant;
        public string species;
        public Vector3 position;
    }

    /// <summary>
    /// Manages the food chain ecosystem - predator/prey relationships, population balance, and ecological dynamics
    /// </summary>
    public class FoodChainManager : MonoBehaviour
    {
        private static FoodChainManager _instance;

        public static FoodChainManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<FoodChainManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("FoodChainManager");
                        _instance = go.AddComponent<FoodChainManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Food Chain Configuration")]
        [SerializeField] private bool enableFoodChain = true;
        [SerializeField] private float updateInterval = 2f;
        [SerializeField] private float populationCheckInterval = 10f;

        [Header("Trophic Levels")]
        [SerializeField] private List<TrophicLevel> trophicLevels = new List<TrophicLevel>();

        [Header("Population Balance")]
        [SerializeField] private AnimationCurve populationGrowthCurve;
        [SerializeField] private float carryingCapacityMultiplier = 1.5f;
        [SerializeField] private float starvationThreshold = 0.3f;
        [SerializeField] private float overpopulationThreshold = 0.8f;

        [Header("Hunting & Foraging")]
        [SerializeField] private float huntingSuccessBaseRate = 0.4f;
        [SerializeField] private float foragingSuccessBaseRate = 0.7f;
        [SerializeField] private float energyTransferEfficiency = 0.1f; // 10% energy transfer between levels

        [Header("Ecosystem Health")]
        [SerializeField] private float ecosystemStability = 1f;
        [SerializeField] private float minimumPopulationRatio = 0.1f;
        [SerializeField] private float maximumPopulationRatio = 10f;

        // Trophic level definition
        [Serializable]
        public class TrophicLevel
        {
            public string levelName;
            public int level; // 0 = producers, 1 = primary consumers, 2 = secondary, etc.
            public List<SpeciesConfig> species;
            public float totalBiomass;
            public float carryingCapacity;
            public Color debugColor;
        }

        [Serializable]
        public class SpeciesConfig
        {
            public string speciesName;
            public GameObject prefab;
            public FoodChainRole role;
            public float baseEnergyValue;
            public float reproductionRate;
            public float metabolicRate;
            public List<string> preySpecies;
            public List<string> predatorSpecies;
            public float optimalPopulationDensity;
        }

        public enum FoodChainRole
        {
            Producer,        // Plants, fruits
            Herbivore,       // Primary consumers
            Omnivore,        // Eats plants and meat
            Carnivore,       // Secondary/tertiary consumers
            Decomposer,      // Breaks down dead matter
            Apex            // Top predator
        }

        // Population tracking
        [Serializable]
        public class SpeciesPopulation
        {
            public string speciesName;
            public List<GameObject> individuals;
            public float totalBiomass;
            public float growthRate;
            public float deathRate;
            public float averageHealth;
            public int births;
            public int deaths;
            public float lastPopulationCheck;
        }

        // Predation event
        [Serializable]
        public class PredationEvent
        {
            public int predatorId;
            public int preyId;
            public string predatorSpecies;
            public string preySpecies;
            public float timestamp;
            public bool successful;
            public Vector3 location;
        }

        // System data
        private Dictionary<string, SpeciesPopulation> populations = new Dictionary<string, SpeciesPopulation>();
        private List<PredationEvent> recentPredations = new List<PredationEvent>();
        private Dictionary<string, List<string>> foodWeb = new Dictionary<string, List<string>>();
        private Dictionary<int, string> entitySpeciesMap = new Dictionary<int, string>();

        // Ecosystem metrics
        private float totalSystemBiomass;
        private float systemEnergyFlow;
        private Dictionary<int, float> trophicEfficiencies = new Dictionary<int, float>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeFoodWeb();
        }

        [Obsolete]
        private void Start()
        {
            // Subscribe to events using the correct generic API
            if (EventSystem.Instance != null)
            {
                EventSystem.Instance.Subscribe<EntityBornEvent>(OnEntityBorn);
                EventSystem.Instance.Subscribe<EntityDiedEvent>(OnEntityDied);
                EventSystem.Instance.Subscribe<EntityAteEvent>(OnEntityAte);
                EventSystem.Instance.Subscribe<PlantGrownEvent>(OnPlantGrown);
            }

            InvokeRepeating(nameof(UpdateFoodChain), updateInterval, updateInterval);
            InvokeRepeating(nameof(CheckPopulationBalance), populationCheckInterval, populationCheckInterval);
            InvokeRepeating(nameof(CalculateEcosystemHealth), 30f, 30f);
        }

        private void InitializeFoodWeb()
        {
            // Build food web relationships
            foreach (var level in trophicLevels)
            {
                foreach (var species in level.species)
                {
                    foodWeb[species.speciesName] = new List<string>(species.preySpecies);

                    // Initialize population tracking
                    populations[species.speciesName] = new SpeciesPopulation
                    {
                        speciesName = species.speciesName,
                        individuals = new List<GameObject>(),
                        lastPopulationCheck = Time.time
                    };
                }
            }
        }

        #region Population Management

        public void RegisterEntity(GameObject entity, string speciesName)
        {
            if (!populations.ContainsKey(speciesName))
            {
                Debug.LogWarning($"Unknown species: {speciesName}");
                return;
            }

            int entityId = entity.GetInstanceID();
            entitySpeciesMap[entityId] = speciesName;
            populations[speciesName].individuals.Add(entity);

            UpdatePopulationMetrics(speciesName);
        }

        public void UnregisterEntity(GameObject entity)
        {
            int entityId = entity.GetInstanceID();
            if (!entitySpeciesMap.ContainsKey(entityId)) return;

            string species = entitySpeciesMap[entityId];
            entitySpeciesMap.Remove(entityId);

            if (populations.ContainsKey(species))
            {
                populations[species].individuals.Remove(entity);
                populations[species].deaths++;
                UpdatePopulationMetrics(species);
            }
        }

        private void UpdatePopulationMetrics(string speciesName)
        {
            if (!populations.ContainsKey(speciesName)) return;

            var population = populations[speciesName];
            var speciesConfig = GetSpeciesConfig(speciesName);
            if (speciesConfig == null) return;

            // Calculate biomass
            population.totalBiomass = population.individuals.Count * speciesConfig.baseEnergyValue;

            // Calculate average health
            float totalHealth = 0f;
            int healthyCount = 0;

            foreach (var individual in population.individuals)
            {
                if (individual != null)
                {
                    var needs = individual.GetComponent<MinipollNeedsSystem>();
                    if (needs != null)
                    {
                        // Use GetNormalizedNeed instead of non-existent GetOverallSatisfaction
                        float overallSatisfaction = (
                            needs.GetNormalizedNeed("Energy") +
                            needs.GetNormalizedNeed("Hunger") +
                            needs.GetNormalizedNeed("Social") +
                            needs.GetNormalizedNeed("Fun") +
                            needs.GetNormalizedNeed("Hygiene")
                        ) / 5f;
                        totalHealth += overallSatisfaction;
                        healthyCount++;
                    }
                }
            }

            population.averageHealth = healthyCount > 0 ? totalHealth / healthyCount : 0f;
        }

        private SpeciesConfig GetSpeciesConfig(string speciesName)
        {
            foreach (var level in trophicLevels)
            {
                foreach (var species in level.species)
                {
                    if (species.speciesName == speciesName)
                        return species;
                }
            }
            return null;
        }

        #endregion

        #region Food Chain Logic

        private void UpdateFoodChain()
        {
            if (!enableFoodChain) return;

            ProcessHunting();
            ProcessForaging();
            ProcessStarvation();
        }

        private void ProcessHunting()
        {
            // For each carnivore/omnivore species, check for hunting opportunities
            foreach (var level in trophicLevels.Where(l => l.level > 0))
            {
                foreach (var species in level.species.Where(s => s.role == FoodChainRole.Carnivore || s.role == FoodChainRole.Omnivore))
                {
                    if (!populations.ContainsKey(species.speciesName)) continue;

                    var predators = populations[species.speciesName].individuals;
                    foreach (var predator in predators)
                    {
                        if (predator == null) continue;
                        AttemptHunt(predator, species);
                    }
                }
            }
        }

        private void AttemptHunt(GameObject predator, SpeciesConfig predatorSpecies)
        {
            // Find nearby prey
            Collider[] nearbyEntities = Physics.OverlapSphere(predator.transform.position, 10f);

            foreach (var collider in nearbyEntities)
            {
                if (collider.gameObject == predator) continue;

                // Check if it's a plant (for now we'll check InteractableObject)
                var plant = collider.GetComponent<InteractableObject>();
                if (plant != null && predatorSpecies.preySpecies.Contains(plant.name)) // Use name instead of objectName
                {
                    // Attempt to eat plant
                    float successRate = foragingSuccessBaseRate;
                    if (UnityEngine.Random.value < successRate)
                    {
                        ConsumePrey(predator, collider.gameObject, predatorSpecies);
                    }
                    continue;
                }

                // Check if it's another entity
                var minipoll = collider.GetComponent<MinipollGame.Core.MinipollCore>(); // Use MinipollCore instead of Minipoll
                if (minipoll != null)
                {
                    string preySpecies = minipoll.Species;
                    if (predatorSpecies.preySpecies.Contains(preySpecies))
                    {
                        // Calculate hunting success
                        float successRate = CalculateHuntingSuccess(predator, collider.gameObject);
                        if (UnityEngine.Random.value < successRate)
                        {
                            ConsumePrey(predator, collider.gameObject, predatorSpecies);
                        }
                    }
                }
            }
        }
        private float CalculateHuntingSuccess(GameObject predator, GameObject prey)
        {
            float baseRate = huntingSuccessBaseRate;

            // Factor in predator health
            if (predator.TryGetComponent<MinipollNeedsSystem>(out var predatorNeeds))
            {
                float overallSatisfaction = (
                    predatorNeeds.GetNormalizedNeed("Energy") +
                    predatorNeeds.GetNormalizedNeed("Hunger") +
                    predatorNeeds.GetNormalizedNeed("Social") +
                    predatorNeeds.GetNormalizedNeed("Fun") +
                    predatorNeeds.GetNormalizedNeed("Hygiene")
                ) / 5f;
                float predatorHealth = overallSatisfaction;
                baseRate *= (0.5f + predatorHealth * 0.5f);
            }

            // Factor in prey weakness
            var preyNeeds = prey.GetComponent<MinipollNeedsSystem>();
            if (preyNeeds != null)
            {
                float overallSatisfaction = (
                    preyNeeds.GetNormalizedNeed("Energy") +
                    preyNeeds.GetNormalizedNeed("Hunger") +
                    preyNeeds.GetNormalizedNeed("Social") +
                    preyNeeds.GetNormalizedNeed("Fun") +
                    preyNeeds.GetNormalizedNeed("Hygiene")
                ) / 5f;
                float weakness = 1f - overallSatisfaction;
                baseRate *= (0.7f + weakness * 0.3f);
            }

            // Environmental factors
            if (WorldManager.Instance != null)
            {
                // Check if it's night - use a simple time check instead of non-existent IsNight method
                float sunlightFactor = WorldManager.Instance.timeSystem.GetSunlightFactor();
                bool isNight = sunlightFactor < 0.3f;

                // Check weather - use a simple weather check instead of non-existent GetCurrentWeather method
                var currentWeather = WorldManager.Instance.weatherSystem.currentWeather;
                bool isFoggy = currentWeather == WorldManager.WeatherSystem.WeatherType.Foggy;

                if (isNight || isFoggy)
                {
                    baseRate *= 1.2f; // Predators are more successful in low visibility
                }
            }

            return Mathf.Clamp01(baseRate);
        }

        private void ProcessForaging()
        {
            // Similar to hunting but for herbivores eating plants
            foreach (var level in trophicLevels.Where(l => l.level > 0))
            {
                foreach (var species in level.species.Where(s => s.role == FoodChainRole.Herbivore || s.role == FoodChainRole.Omnivore))
                {
                    if (!populations.ContainsKey(species.speciesName)) continue;

                    var foragers = populations[species.speciesName].individuals;
                    foreach (var forager in foragers)
                    {
                        if (forager == null) continue;
                        // Foraging logic would go here
                    }
                }
            }
        }

        private void ProcessStarvation()
        {
            // Check for entities that are starving and apply effects
            foreach (var population in populations.Values)
            {
                foreach (var individual in population.individuals)
                {
                    if (individual == null) continue;

                    var needs = individual.GetComponent<MinipollNeedsSystem>();
                    if (needs != null)
                    {
                        // Check hunger instead of non-existent methods
                        float hungerLevel = needs.GetNormalizedNeed("Hunger");
                        if (hungerLevel < starvationThreshold)
                        {
                            // Apply starvation effects
                            var brain = individual.GetComponent<Core.MinipollCore>();
                            if (brain != null)
                            {
                                // Apply starvation damage
                                needs.FillNeed("Energy", -0.1f * Time.deltaTime);
                            }
                        }
                    }
                }
            }
        }
        private void ConsumePrey(GameObject predator, GameObject prey, SpeciesConfig predatorSpecies)
        {            // Remove prey from ecosystem
            var preyMinipoll = prey.GetComponent<MinipollGame.Core.MinipollCore>(); // Use MinipollCore instead of Minipoll
            if (preyMinipoll != null)
            {
                UnregisterEntity(prey);

                // Record predation event
                var predationEvent = new PredationEvent
                {
                    predatorId = predator.GetInstanceID(),
                    preyId = prey.GetInstanceID(),
                    predatorSpecies = predatorSpecies.speciesName,
                    preySpecies = preyMinipoll.Species,
                    timestamp = Time.time,
                    successful = true,
                    location = prey.transform.position
                };
                recentPredations.Add(predationEvent);

                // Publish event using generic API
                EventSystem.Instance?.Publish<EntityAteEvent>(new EntityAteEvent
                {
                    predator = predator,
                    prey = prey,
                    energy = predatorSpecies.baseEnergyValue
                }, new Dictionary<string, object>());

                // No MemoryManager or EmotionContagionSystem - remove those references

                // Transfer energy to predator
                var predatorNeeds = predator.GetComponent<MinipollNeedsSystem>();
                if (predatorNeeds != null)
                {
                    float energyValue = predatorSpecies.baseEnergyValue * energyTransferEfficiency;
                    predatorNeeds.FillNeed("Hunger", energyValue); // Use FillNeed instead of ModifyNeed
                }

                Destroy(prey);
            }
        }

        #endregion

        #region Population Balance

        private void CheckPopulationBalance()
        {
            foreach (var population in populations.Values)
            {
                var speciesConfig = GetSpeciesConfig(population.speciesName);
                if (speciesConfig == null) continue;

                int currentPopulation = population.individuals.Count;
                float targetPopulation = speciesConfig.optimalPopulationDensity;

                if (currentPopulation > targetPopulation * overpopulationThreshold)
                {
                    // Overpopulation detected
                    TriggerOverpopulationResponse(population.speciesName);
                }
                else if (currentPopulation < targetPopulation * minimumPopulationRatio)
                {
                    // Endangered species
                    TriggerEndangeredResponse(population.speciesName);
                }
            }
        }

        private void TriggerOverpopulationResponse(string species)
        {
            // Increase disease/starvation effects
            if (populations.ContainsKey(species))
            {
                foreach (var individual in populations[species].individuals)
                {
                    if (individual != null)
                    {
                        var needs = individual.GetComponent<MinipollNeedsSystem>();
                        if (needs != null)
                        {
                            float decompositionRate = 0.1f; // 10% per check
                            needs.FillNeed("Hunger", -decompositionRate); // Use FillNeed instead of ModifyNeed
                        }
                    }
                }
            }

            // Publish event using generic API
            EventSystem.Instance?.Publish(new Dictionary<string, object>
            {
                ["species"] = species,
                ["populationCount"] = populations[species].individuals.Count
            });
        }

        private void TriggerEndangeredResponse(string species)
        {
            // Boost reproduction, reduce predation
            EventSystem.Instance?.Publish(new Dictionary<string, object>
            {
                ["species"] = species,
                ["populationCount"] = populations[species].individuals.Count
            });
        }

        private void CalculateEcosystemHealth()
        {
            // Calculate ecosystem stability based on population ratios
            float totalBiomass = 0f;
            float balanceScore = 1f;

            foreach (var level in trophicLevels)
            {
                float levelBiomass = 0f;
                foreach (var species in level.species)
                {
                    if (populations.ContainsKey(species.speciesName))
                    {
                        levelBiomass += populations[species.speciesName].totalBiomass;
                    }
                }
                level.totalBiomass = levelBiomass;
                totalBiomass += levelBiomass;
            }

            // Check for ecosystem collapse conditions
            int extinctSpecies = populations.Count(p => p.Value.individuals.Count == 0);
            if (extinctSpecies > trophicLevels.Sum(l => l.species.Count) * 0.5f)
            {
                // Over 50% species extinct
                balanceScore = 0.1f;
                EventSystem.Instance?.Publish(new Dictionary<string, object>
                {
                    ["severity"] = "critical",
                    ["extinctSpecies"] = extinctSpecies
                });
            }

            ecosystemStability = balanceScore;
            totalSystemBiomass = totalBiomass;
        }

        #endregion

        #region Event Handlers

        private void OnEntityBorn(EntityBornEvent eventData)
        {
            if (eventData != null && eventData.entity != null && !string.IsNullOrEmpty(eventData.species))
            {
                RegisterEntity(eventData.entity, eventData.species);

                if (populations.ContainsKey(eventData.species))
                {
                    populations[eventData.species].births++;
                }
            }
        }

        private void OnEntityDied(EntityDiedEvent eventData)
        {
            if (eventData != null && eventData.entity != null)
            {
                UnregisterEntity(eventData.entity);

                // Could spawn decomposer activity here
                StartCoroutine(DecompositionProcess(eventData.entity.transform.position));
            }
        }

        private void OnEntityAte(EntityAteEvent eventData)
        {
            // Track feeding events for ecosystem metrics
            if (eventData != null)
            {
                systemEnergyFlow += eventData.energy;
            }
        }

        private void OnPlantGrown(PlantGrownEvent eventData)
        {
            // Increase producer biomass
            if (eventData != null && eventData.plant != null && !string.IsNullOrEmpty(eventData.species))
            {
                RegisterEntity(eventData.plant, eventData.species);
            }
        }

        private IEnumerator DecompositionProcess(Vector3 location)
        {
            // Wait before decomposition starts
            yield return new WaitForSeconds(10f);

            // Return nutrients to soil/environment
            // This could trigger plant growth nearby
            EventSystem.Instance?.Publish(new Dictionary<string, object>
            {
                ["location"] = location,
                ["amount"] = 1f
            });
        }

        #endregion

        #region Public API

        public bool IsSpeciesPredator(string species)
        {
            var config = GetSpeciesConfig(species);
            return config != null &&
                   (config.role == FoodChainRole.Carnivore ||
                    config.role == FoodChainRole.Omnivore ||
                    config.role == FoodChainRole.Apex);
        }

        public bool IsSpeciesPrey(string species)
        {
            var config = GetSpeciesConfig(species);
            return config != null &&
                   (config.role == FoodChainRole.Herbivore ||
                    config.role == FoodChainRole.Omnivore);
        }

        public List<string> GetPredatorsOf(string species)
        {
            var predators = new List<string>();
            var config = GetSpeciesConfig(species);
            if (config != null)
            {
                predators.AddRange(config.predatorSpecies);
            }
            return predators;
        }

        public List<string> GetPreyOf(string species)
        {
            var config = GetSpeciesConfig(species);
            return config != null ? new List<string>(config.preySpecies) : new List<string>();
        }

        public float GetSpeciesPopulationHealth(string species)
        {
            return populations.ContainsKey(species) ? populations[species].averageHealth : 0f;
        }

        public int GetSpeciesCount(string species)
        {
            return populations.ContainsKey(species) ? populations[species].individuals.Count : 0;
        }

        public List<PredationEvent> GetRecentPredations(float timeWindow = 60f)
        {
            float currentTime = Time.time;
            return recentPredations
                .Where(p => currentTime - p.timestamp <= timeWindow)
                .ToList();
        }

        public float GetEcosystemStability()
        {
            return ecosystemStability;
        }

        public Dictionary<string, object> GetEcosystemStats()
        {
            return new Dictionary<string, object>
            {
                ["TotalBiomass"] = totalSystemBiomass,
                ["EcosystemStability"] = ecosystemStability,
                ["TotalSpecies"] = populations.Count,
                ["ActivePopulations"] = populations.Count(p => p.Value.individuals.Count > 0),
                ["RecentPredations"] = recentPredations.Count,
                ["EnergyFlow"] = systemEnergyFlow
            };
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            if (!enableFoodChain) return;

            // Draw recent predation events
            foreach (var predation in recentPredations.TakeLast(10))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(predation.location, 0.5f);
            }

            // Draw population centers
            foreach (var level in trophicLevels)
            {
                Gizmos.color = level.debugColor;
                foreach (var species in level.species)
                {
                    if (populations.ContainsKey(species.speciesName))
                    {
                        foreach (var individual in populations[species.speciesName].individuals)
                        {
                            if (individual != null)
                            {
                                Gizmos.DrawWireCube(individual.transform.position, Vector3.one * 0.2f);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Unsubscribe from events using the correct generic API
            if (EventSystem.Instance != null)
            {

                EventSystem.Instance.Unsubscribe<EntityBornEvent>(OnEntityBorn);
                EventSystem.Instance.Unsubscribe<EntityDiedEvent>(OnEntityDied);
                EventSystem.Instance.Unsubscribe<EntityAteEvent>(OnEntityAte);
                EventSystem.Instance.Unsubscribe<PlantGrownEvent>(OnPlantGrown);
            }
        }


    }

    internal class MinipollCore
    {
        public static object MinipollManager { get; internal set; }
    }

    internal class MinipollNeedsSystem
    {
        internal object energy;
        internal object hunger;

        internal void FillNeed(string v, float energyValue)
        {
            throw new NotImplementedException();
        }

        internal int GetNormalizedNeed(string v)
        {
            throw new NotImplementedException();
        }

        internal void InitSystem(MinipollBrain minipollBrain)
        {
            throw new NotImplementedException();
        }

        internal void UpdateNeeds(float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}
