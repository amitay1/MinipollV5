using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using MinipollGame.Systems.Core;
using MinipollGame.Core;
using UnityEngine;

namespace MinipollCore.Systems.World
{
    /// <summary>
    /// Manages plant growth, reproduction, and lifecycle in the ecosystem
    /// </summary>
    public class PlantGrowthSystem : MonoBehaviour
    {
        private static PlantGrowthSystem _instance;
        public static PlantGrowthSystem Instance
        {
            get
            {                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PlantGrowthSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PlantGrowthSystem");
                        _instance = go.AddComponent<PlantGrowthSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Growth Configuration")]
        [SerializeField] private bool enablePlantGrowth = true;
        [SerializeField] private float growthUpdateInterval = 5f;
        [SerializeField] private float baseGrowthRate = 0.1f;
        [SerializeField] private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Environmental Factors")]
        [SerializeField] private float optimalTemperature = 20f;
        [SerializeField] private float temperatureTolerance = 15f;
        [SerializeField] private float minWaterRequirement = 0.3f;
        [SerializeField] private float sunlightImportance = 0.8f;
        [SerializeField] private float soilQualityImpact = 0.5f;

        [Header("Reproduction")]
        [SerializeField] private float seedingChance = 0.1f;
        [SerializeField] private float seedDispersalRadius = 10f;
        [SerializeField] private float pollinationRadius = 5f;
        [SerializeField] private int maxSeedsPerPlant = 3;
        [SerializeField] private float germinationTime = 30f;

        [Header("Competition & Ecology")]
        [SerializeField] private float competitionRadius = 2f;
        [SerializeField] private float competitionStrength = 0.5f;
        [SerializeField] private float symbiosisBonus = 1.2f;
        [SerializeField] private float allelopathyRange = 3f;

        [Header("Seasons & Lifecycle")]
        [SerializeField] private bool enableSeasonalGrowth = true;
        [SerializeField] private float dormancyThreshold = 0.2f;
        [SerializeField] private float lifespanVariation = 0.2f;

        // Plant types configuration
        [Serializable]
        public class PlantSpecies
        {
            public string speciesName;
            public PlantType type;
            public float maxHeight = 5f;
            public float maxAge = 100f;
            public float growthSpeed = 1f;
            public float waterNeed = 0.5f;
            public float sunlightNeed = 0.7f;
            public float nutrientNeed = 0.4f;
            public float seedProductionAge = 10f;
            public float fruitProductionRate = 0f;
            public List<string> symbioticSpecies;
            public List<string> competitiveSpecies;
            public GameObject[] growthStagePrefabs;
            public Color healthyColor = Color.green;
            public Color unhealthyColor = Color.yellow;
        }

        public enum PlantType
        {
            Grass,
            Shrub,
            Tree,
            Flower,
            Crop,
            Vine,
            Fungus,
            Aquatic
        }

        // Individual plant data
        [Serializable]
        public class Plant
        {
            public string plantId;
            public PlantSpecies species;
            public Vector3 position;
            public float age;
            public float height;
            public float health = 1f;
            public GrowthStage stage;
            public float growthProgress;
            public bool isDormant;
            public float lastGrowthTime;
            public float waterLevel = 1f;
            public float nutrientLevel = 1f;
            public GameObject gameObject;
            public List<string> nearbyPlants;
            public PlantGenes genes;

            public Plant()
            {
                plantId = Guid.NewGuid().ToString();
                nearbyPlants = new List<string>();
                genes = new PlantGenes();
            }
        }

        public enum GrowthStage
        {
            Seed,
            Sprout,
            Young,
            Mature,
            Flowering,
            Fruiting,
            Declining,
            Dead
        }

        // Genetic traits for variation
        [Serializable]
        public class PlantGenes
        {
            public float growthRateModifier = 1f;
            public float heightModifier = 1f;
            public float waterEfficiency = 1f;
            public float diseaseResistance = 1f;
            public float seedProduction = 1f;
            public Color flowerColor = Color.white;
        }

        // Seed waiting to germinate
        [Serializable]
        public class Seed
        {
            public string seedId;
            public PlantSpecies species;
            public Vector3 position;
            public float plantTime;
            public float viability = 1f;
            public PlantGenes parentGenes;
            public string parentId;
        }

        // Environmental conditions at location
        [Serializable]
        public class EnvironmentalConditions
        {
            public float temperature;
            public float waterAvailability;
            public float sunlight;
            public float soilQuality;
            public float nutrients;
            public bool isSheltered;
        }

        // System data
        [SerializeField] private List<PlantSpecies> plantSpecies = new List<PlantSpecies>();
        private Dictionary<string, Plant> plants = new Dictionary<string, Plant>();
        private List<Seed> seeds = new List<Seed>();
        private Dictionary<Vector2Int, List<string>> spatialPlantIndex = new Dictionary<Vector2Int, List<string>>();
        private Dictionary<PlantType, int> plantCounts = new Dictionary<PlantType, int>();
        
        // Soil quality grid
        private float[,] soilQualityGrid;
        private int gridResolution = 100;
        private float gridCellSize = 5f;

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
        }        private void Start()
        {
            // Subscribe to events
            if (EventSystem.Instance != null)
            {
                EventSystem.Instance.Subscribe<object>("WeatherChanged", OnWeatherChanged);
                EventSystem.Instance.Subscribe<object>("SeasonChanged", OnSeasonChanged);
                EventSystem.Instance.Subscribe<object>("PlantEaten", OnPlantEaten);
                EventSystem.Instance.Subscribe<object>("SoilEnriched", OnSoilEnriched);
            }

            InvokeRepeating(nameof(UpdatePlantGrowth), growthUpdateInterval, growthUpdateInterval);
            InvokeRepeating(nameof(ProcessSeeds), germinationTime / 3f, germinationTime / 3f);
            InvokeRepeating(nameof(UpdateSoilQuality), 30f, 30f);
        }

        private void InitializeSystem()
        {
            // Initialize plant type counts
            foreach (PlantType type in Enum.GetValues(typeof(PlantType)))
            {
                plantCounts[type] = 0;
            }

            // Initialize soil quality grid
            soilQualityGrid = new float[gridResolution, gridResolution];
            for (int x = 0; x < gridResolution; x++)
            {
                for (int z = 0; z < gridResolution; z++)
                {
                    // Random initial soil quality with some noise
                    soilQualityGrid[x, z] = Mathf.PerlinNoise(x * 0.1f, z * 0.1f);
                }
            }
        }

        #region Plant Creation and Management

        public Plant CreatePlant(PlantSpecies species, Vector3 position, GrowthStage initialStage = GrowthStage.Seed, PlantGenes parentGenes = null)
        {
            Plant plant = new Plant
            {
                species = species,
                position = position,
                stage = initialStage,
                age = initialStage == GrowthStage.Seed ? 0f : UnityEngine.Random.Range(1f, 10f),
                genes = parentGenes != null ? MutateGenes(parentGenes) : GenerateRandomGenes()
            };

            // Create visual representation
            if (species.growthStagePrefabs != null && species.growthStagePrefabs.Length > 0)
            {
                int prefabIndex = Mathf.Min((int)initialStage, species.growthStagePrefabs.Length - 1);
                if (species.growthStagePrefabs[prefabIndex] != null)
                {
                    plant.gameObject = Instantiate(species.growthStagePrefabs[prefabIndex], position, 
                        Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0));
                    plant.gameObject.name = $"{species.speciesName}_{plant.plantId.Substring(0, 8)}";
                }
            }

            plants[plant.plantId] = plant;
            plantCounts[species.type]++;
            
            // Add to spatial index
            AddToSpatialIndex(plant);
            
            // Find nearby plants
            UpdateNearbyPlants(plant);

            // Register as resource if it produces food
            if (species.fruitProductionRate > 0 && WorldResourceManager.Instance != null)
            {
                var resourceType = new WorldResourceManager.ResourceType
                {
                    resourceName = $"{species.speciesName}_Fruit",
                    category = WorldResourceManager.ResourceCategory.Food,
                    baseValue = species.fruitProductionRate,
                    isRenewable = true                };
                WorldResourceManager.Instance.CreateResourceNode(resourceType, position, 0f, plant.gameObject);
            }            
            var eventSystem = FindFirstObjectByType<EventSystem>();
            eventSystem?.Publish(new PlantGrownEvent
            {
                plant = plant.gameObject,
                species = species.speciesName,
                position = position
            });

            return plant;
        }

        public void RemovePlant(string plantId)
        {
            if (!plants.ContainsKey(plantId)) return;

            var plant = plants[plantId];
            
            // Clean up visual
            if (plant.gameObject != null)
            {
                Destroy(plant.gameObject);
            }

            // Update counts
            plantCounts[plant.species.type]--;
            
            // Remove from spatial index
            RemoveFromSpatialIndex(plant);
            
            // Remove from nearby lists
            foreach (string nearbyId in plant.nearbyPlants)
            {
                if (plants.ContainsKey(nearbyId))
                {
                    plants[nearbyId].nearbyPlants.Remove(plantId);
                }
            }

            plants.Remove(plantId);

            // Enrich soil where plant died
            EnrichSoilAtPosition(plant.position, 0.2f);
        }

        #endregion

        #region Growth System

        private void UpdatePlantGrowth()
        {
            if (!enablePlantGrowth) return;

            List<string> plantsToRemove = new List<string>();

            foreach (var plant in plants.Values)
            {
                if (plant.stage == GrowthStage.Dead)
                {
                    plantsToRemove.Add(plant.plantId);
                    continue;
                }

                UpdateIndividualPlant(plant);
            }

            // Remove dead plants
            foreach (string plantId in plantsToRemove)
            {
                RemovePlant(plantId);
            }
        }

        private void UpdateIndividualPlant(Plant plant)
        {
            // Get environmental conditions
            EnvironmentalConditions conditions = GetEnvironmentalConditions(plant.position);
            
            // Calculate growth factors
            float growthMultiplier = CalculateGrowthMultiplier(plant, conditions);
            
            // Update resources
            UpdatePlantResources(plant, conditions);
            
            // Check dormancy
            if (enableSeasonalGrowth)
            {
                UpdateDormancy(plant, conditions);
            }

            if (!plant.isDormant && plant.health > 0)
            {
                // Age the plant
                plant.age += growthUpdateInterval * Time.timeScale;
                
                // Growth based on stage
                switch (plant.stage)
                {
                    case GrowthStage.Seed:
                        // Seeds don't grow, they germinate (handled separately)
                        break;
                        
                    case GrowthStage.Sprout:
                    case GrowthStage.Young:
                    case GrowthStage.Mature:
                        GrowPlant(plant, growthMultiplier);
                        break;
                        
                    case GrowthStage.Flowering:
                        HandleFlowering(plant);
                        break;
                        
                    case GrowthStage.Fruiting:
                        HandleFruiting(plant);
                        break;
                        
                    case GrowthStage.Declining:
                        plant.health -= 0.01f * growthUpdateInterval;
                        break;
                }
                
                // Check stage transitions
                CheckStageTransition(plant);
            }
            
            // Update visuals
            UpdatePlantVisuals(plant);
            
            // Check death
            if (plant.health <= 0 || plant.age > plant.species.maxAge * (1f + lifespanVariation))
            {
                plant.stage = GrowthStage.Dead;
            }
        }

        private float CalculateGrowthMultiplier(Plant plant, EnvironmentalConditions conditions)
        {
            float multiplier = baseGrowthRate * plant.species.growthSpeed * plant.genes.growthRateModifier;
            
            // Temperature factor
            float tempDiff = Mathf.Abs(conditions.temperature - optimalTemperature);
            float tempFactor = 1f - Mathf.Clamp01(tempDiff / temperatureTolerance);
            multiplier *= tempFactor;
            
            // Water factor
            float waterFactor = Mathf.Clamp01(plant.waterLevel / plant.species.waterNeed);
            multiplier *= waterFactor;
            
            // Sunlight factor
            float sunFactor = conditions.sunlight / plant.species.sunlightNeed;
            multiplier *= Mathf.Lerp(1f, sunFactor, sunlightImportance);
            
            // Soil quality
            multiplier *= Mathf.Lerp(1f, conditions.soilQuality, soilQualityImpact);
            
            // Competition
            float competitionFactor = CalculateCompetition(plant);
            multiplier *= competitionFactor;
            
            // Symbiosis
            float symbiosisFactor = CalculateSymbiosis(plant);
            multiplier *= symbiosisFactor;
            
            // Health
            multiplier *= plant.health;
            
            return Mathf.Clamp(multiplier, 0f, 2f);
        }

        private void GrowPlant(Plant plant, float growthMultiplier)
        {
            // Increase height
            float heightGrowth = growthMultiplier * growthUpdateInterval * plant.genes.heightModifier;
            plant.height = Mathf.Min(plant.height + heightGrowth, plant.species.maxHeight);
            
            // Update growth progress
            plant.growthProgress += growthMultiplier * growthUpdateInterval;
            
            // Scale visual
            if (plant.gameObject != null)
            {
                float targetScale = (plant.height / plant.species.maxHeight) * plant.genes.heightModifier;
                plant.gameObject.transform.localScale = Vector3.one * targetScale;
            }
        }

        #endregion

        #region Plant Resources

        private void UpdatePlantResources(Plant plant, EnvironmentalConditions conditions)
        {
            // Water uptake
            float waterUptake = conditions.waterAvailability * plant.genes.waterEfficiency;
            plant.waterLevel = Mathf.Clamp01(plant.waterLevel + waterUptake - plant.species.waterNeed * growthUpdateInterval);
            
            // Nutrient uptake
            float nutrientUptake = conditions.nutrients * conditions.soilQuality;
            plant.nutrientLevel = Mathf.Clamp01(plant.nutrientLevel + nutrientUptake - plant.species.nutrientNeed * growthUpdateInterval);
            
            // Health impact from resource deficiency
            if (plant.waterLevel < minWaterRequirement)
            {
                plant.health -= 0.1f * growthUpdateInterval;
            }
            
            if (plant.nutrientLevel < 0.2f)
            {
                plant.health -= 0.05f * growthUpdateInterval;
            }
            
            // Natural health recovery
            if (plant.waterLevel > 0.7f && plant.nutrientLevel > 0.5f)
            {
                plant.health = Mathf.Min(plant.health + 0.02f * growthUpdateInterval, 1f);
            }
        }        private EnvironmentalConditions GetEnvironmentalConditions(Vector3 position)
        {
            EnvironmentalConditions conditions = new EnvironmentalConditions();
            
            // Temperature from world manager
            if (WorldManager.Instance != null)
            {
                conditions.temperature = WorldManager.Instance.GetTemperature();
                conditions.sunlight = WorldManager.Instance.IsNight() ? 0.1f : 1f;
                
                // Weather affects water
                var weather = (WorldManager.WeatherSystem.WeatherType)WorldManager.Instance.GetCurrentWeather();
                conditions.waterAvailability = weather == WorldManager.WeatherSystem.WeatherType.Rain ? 1f : 0.5f;
            }
            else
            {
                conditions.temperature = optimalTemperature;
                conditions.sunlight = 1f;
                conditions.waterAvailability = 0.5f;
            }
            
            // Soil quality from grid
            Vector2Int gridPos = WorldToGridPosition(position);
            if (gridPos.x >= 0 && gridPos.x < gridResolution && gridPos.y >= 0 && gridPos.y < gridResolution)
            {
                conditions.soilQuality = soilQualityGrid[gridPos.x, gridPos.y];
                conditions.nutrients = conditions.soilQuality; // Simplified
            }
            else
            {
                conditions.soilQuality = 0.5f;
                conditions.nutrients = 0.5f;
            }
            
            // Check if sheltered (under trees, etc)
            conditions.isSheltered = Physics.Raycast(position + Vector3.up, Vector3.up, 10f);
            if (conditions.isSheltered)
            {
                conditions.sunlight *= 0.5f;
                conditions.waterAvailability *= 0.8f;
            }
            
            return conditions;
        }

        #endregion

        #region Competition and Symbiosis

        private float CalculateCompetition(Plant plant)
        {
            float competitionFactor = 1f;
            
            foreach (string nearbyId in plant.nearbyPlants)
            {
                if (!plants.ContainsKey(nearbyId)) continue;
                
                Plant nearbyPlant = plants[nearbyId];
                float distance = Vector3.Distance(plant.position, nearbyPlant.position);
                
                if (distance < competitionRadius)
                {
                    // Check if competitive species
                    bool isCompetitive = plant.species.competitiveSpecies.Contains(nearbyPlant.species.speciesName) ||
                                       plant.species.type == nearbyPlant.species.type;
                    
                    if (isCompetitive)
                    {
                        // Larger plants have advantage
                        float sizeRatio = nearbyPlant.height / Mathf.Max(0.1f, plant.height);
                        float distanceFactor = 1f - (distance / competitionRadius);
                        
                        competitionFactor -= competitionStrength * distanceFactor * Mathf.Min(sizeRatio, 2f);
                    }
                }
            }
            
            return Mathf.Clamp(competitionFactor, 0.1f, 1f);
        }

        private float CalculateSymbiosis(Plant plant)
        {
            float symbiosisFactor = 1f;
            
            foreach (string nearbyId in plant.nearbyPlants)
            {
                if (!plants.ContainsKey(nearbyId)) continue;
                
                Plant nearbyPlant = plants[nearbyId];
                
                // Check if symbiotic species
                if (plant.species.symbioticSpecies.Contains(nearbyPlant.species.speciesName))
                {
                    float distance = Vector3.Distance(plant.position, nearbyPlant.position);
                    if (distance < competitionRadius * 2f)
                    {
                        symbiosisFactor = symbiosisBonus;
                        break; // Only need one symbiotic partner
                    }
                }
            }
            
            return symbiosisFactor;
        }

        private void UpdateNearbyPlants(Plant plant)
        {
            plant.nearbyPlants.Clear();
            
            // Check surrounding grid cells
            Vector2Int centerGrid = GetGridPosition(plant.position);
            int searchRadius = Mathf.CeilToInt(competitionRadius * 2f / gridCellSize);
            
            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int z = -searchRadius; z <= searchRadius; z++)
                {
                    Vector2Int gridPos = centerGrid + new Vector2Int(x, z);
                    
                    if (spatialPlantIndex.ContainsKey(gridPos))
                    {
                        foreach (string otherId in spatialPlantIndex[gridPos])
                        {
                            if (otherId != plant.plantId && plants.ContainsKey(otherId))
                            {
                                Plant other = plants[otherId];
                                float distance = Vector3.Distance(plant.position, other.position);
                                
                                if (distance <= competitionRadius * 2f)
                                {
                                    plant.nearbyPlants.Add(otherId);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Lifecycle and Stages

        private void CheckStageTransition(Plant plant)
        {
            switch (plant.stage)
            {
                case GrowthStage.Sprout:
                    if (plant.growthProgress > 10f)
                    {
                        plant.stage = GrowthStage.Young;
                        UpdatePlantPrefab(plant, GrowthStage.Young);
                    }
                    break;
                    
                case GrowthStage.Young:
                    if (plant.growthProgress > 30f)
                    {
                        plant.stage = GrowthStage.Mature;
                        UpdatePlantPrefab(plant, GrowthStage.Mature);
                    }
                    break;
                    
                case GrowthStage.Mature:
                    if (plant.age > plant.species.seedProductionAge && !plant.isDormant)
                    {
                        plant.stage = GrowthStage.Flowering;
                        UpdatePlantPrefab(plant, GrowthStage.Flowering);
                    }
                    break;
                    
                case GrowthStage.Flowering:
                    // Transition handled in HandleFlowering
                    break;
                    
                case GrowthStage.Fruiting:
                    // Return to mature after fruiting
                    if (plant.species.fruitProductionRate <= 0)
                    {
                        plant.stage = GrowthStage.Mature;
                    }
                    break;
                    
                case GrowthStage.Declining:
                    // Natural progression
                    break;
            }
            
            // Check for decline
            if (plant.age > plant.species.maxAge * 0.8f && plant.stage != GrowthStage.Declining)
            {
                plant.stage = GrowthStage.Declining;
            }
        }

        private void UpdatePlantPrefab(Plant plant, GrowthStage newStage)
        {
            if (plant.species.growthStagePrefabs == null || plant.species.growthStagePrefabs.Length == 0)
                return;
                
            int prefabIndex = Mathf.Min((int)newStage, plant.species.growthStagePrefabs.Length - 1);
            
            if (plant.species.growthStagePrefabs[prefabIndex] != null && plant.gameObject != null)
            {
                Vector3 position = plant.gameObject.transform.position;
                Quaternion rotation = plant.gameObject.transform.rotation;
                Vector3 scale = plant.gameObject.transform.localScale;
                
                Destroy(plant.gameObject);
                
                plant.gameObject = Instantiate(plant.species.growthStagePrefabs[prefabIndex], position, rotation);
                plant.gameObject.transform.localScale = scale;
                plant.gameObject.name = $"{plant.species.speciesName}_{plant.plantId.Substring(0, 8)}";
            }
        }

        #endregion

        #region Reproduction

        private void HandleFlowering(Plant plant)
        {
            // Check for pollination
            bool pollinated = false;
            
            foreach (string nearbyId in plant.nearbyPlants)
            {
                if (!plants.ContainsKey(nearbyId)) continue;
                
                Plant nearbyPlant = plants[nearbyId];
                
                // Same species and also flowering
                if (nearbyPlant.species.speciesName == plant.species.speciesName && 
                    nearbyPlant.stage == GrowthStage.Flowering)
                {
                    float distance = Vector3.Distance(plant.position, nearbyPlant.position);
                    if (distance <= pollinationRadius)
                    {
                        pollinated = true;
                        break;
                    }
                }
            }
            
            if (pollinated)
            {
                plant.stage = GrowthStage.Fruiting;
                UpdatePlantPrefab(plant, GrowthStage.Fruiting);
            }
        }

        private void HandleFruiting(Plant plant)
        {
            // Produce seeds
            if (UnityEngine.Random.value < seedingChance * plant.genes.seedProduction)
            {
                int seedCount = UnityEngine.Random.Range(1, maxSeedsPerPlant + 1);
                
                for (int i = 0; i < seedCount; i++)
                {
                    ProduceSeed(plant);
                }
                
                // Return to mature stage
                plant.stage = GrowthStage.Mature;
                UpdatePlantPrefab(plant, GrowthStage.Mature);
            }
              // Update fruit resource
            if (plant.species.fruitProductionRate > 0 && WorldResourceManager.Instance != null)
            {
                // This would update the resource node associated with this plant
            }
        }

        private void ProduceSeed(Plant parent)
        {
            // Calculate dispersal
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(1f, seedDispersalRadius);
            
            Vector3 seedPosition = parent.position + new Vector3(
                Mathf.Cos(angle) * distance,
                0,
                Mathf.Sin(angle) * distance
            );
            
            // Adjust to terrain
            if (Physics.Raycast(seedPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                seedPosition = hit.point;
            }
            
            Seed seed = new Seed
            {
                seedId = Guid.NewGuid().ToString(),
                species = parent.species,
                position = seedPosition,
                plantTime = Time.time,
                viability = 1f,
                parentGenes = parent.genes,
                parentId = parent.plantId
            };
            
            seeds.Add(seed);
        }

        private void ProcessSeeds()
        {
            List<Seed> seedsToRemove = new List<Seed>();
            
            foreach (var seed in seeds)
            {
                // Check germination conditions
                EnvironmentalConditions conditions = GetEnvironmentalConditions(seed.position);
                
                // Reduce viability over time
                seed.viability -= 0.1f * (germinationTime / 3f) / germinationTime;
                
                // Check if should germinate
                float germinationChance = seed.viability * conditions.waterAvailability * conditions.soilQuality;
                
                if (Time.time - seed.plantTime > germinationTime && UnityEngine.Random.value < germinationChance)
                {
                    // Germinate!
                    Plant newPlant = CreatePlant(seed.species, seed.position, GrowthStage.Sprout, seed.parentGenes);
                      // Create memory of parent
                    // if (MemoryManager.Instance != null && !string.IsNullOrEmpty(seed.parentId))
                    // {
                    //     // Could track plant lineages
                    // }
                    
                    seedsToRemove.Add(seed);
                }
                else if (seed.viability <= 0)
                {
                    seedsToRemove.Add(seed);
                }
            }
            
            // Remove germinated or dead seeds
            foreach (var seed in seedsToRemove)
            {
                seeds.Remove(seed);
            }
        }

        #endregion

        #region Genetics

        private PlantGenes GenerateRandomGenes()
        {
            return new PlantGenes
            {
                growthRateModifier = UnityEngine.Random.Range(0.8f, 1.2f),
                heightModifier = UnityEngine.Random.Range(0.9f, 1.1f),
                waterEfficiency = UnityEngine.Random.Range(0.8f, 1.2f),
                diseaseResistance = UnityEngine.Random.Range(0.5f, 1f),
                seedProduction = UnityEngine.Random.Range(0.8f, 1.2f),
                flowerColor = new Color(
                    UnityEngine.Random.Range(0.5f, 1f),
                    UnityEngine.Random.Range(0.5f, 1f),
                    UnityEngine.Random.Range(0.5f, 1f)
                )
            };
        }

        private PlantGenes MutateGenes(PlantGenes parentGenes)
        {
            float mutationRate = 0.1f;
            float mutationStrength = 0.2f;
            
            PlantGenes newGenes = new PlantGenes
            {
                growthRateModifier = MutateValue(parentGenes.growthRateModifier, mutationRate, mutationStrength),
                heightModifier = MutateValue(parentGenes.heightModifier, mutationRate, mutationStrength),
                waterEfficiency = MutateValue(parentGenes.waterEfficiency, mutationRate, mutationStrength),
                diseaseResistance = MutateValue(parentGenes.diseaseResistance, mutationRate, mutationStrength),
                seedProduction = MutateValue(parentGenes.seedProduction, mutationRate, mutationStrength),
                flowerColor = MutateColor(parentGenes.flowerColor, mutationRate, mutationStrength)
            };
            
            return newGenes;
        }

        private float MutateValue(float original, float mutationRate, float mutationStrength)
        {
            if (UnityEngine.Random.value < mutationRate)
            {
                float mutation = UnityEngine.Random.Range(-mutationStrength, mutationStrength);
                return Mathf.Clamp(original + mutation, 0.1f, 2f);
            }
            return original;
        }

        private Color MutateColor(Color original, float mutationRate, float mutationStrength)
        {
            if (UnityEngine.Random.value < mutationRate)
            {
                return new Color(
                    Mathf.Clamp01(original.r + UnityEngine.Random.Range(-mutationStrength, mutationStrength)),
                    Mathf.Clamp01(original.g + UnityEngine.Random.Range(-mutationStrength, mutationStrength)),
                    Mathf.Clamp01(original.b + UnityEngine.Random.Range(-mutationStrength, mutationStrength))
                );
            }
            return original;
        }

        #endregion

        #region Environmental Effects

        private void UpdateDormancy(Plant plant, EnvironmentalConditions conditions)
        {            if (WorldManager.Instance != null)
            {
                var season = WorldManager.Instance.timeSystem.currentSeason;
                
                // Enter dormancy in winter
                if (season == WorldManager.TimeSystem.Season.Winter && conditions.temperature < 5f)
                {
                    plant.isDormant = true;
                }
                // Exit dormancy in spring
                else if (season == WorldManager.TimeSystem.Season.Spring && conditions.temperature > 10f)
                {
                    plant.isDormant = false;
                }
            }
        }

        private void UpdatePlantVisuals(Plant plant)
        {
            if (plant.gameObject == null) return;
            
            // Color based on health
            Renderer renderer = plant.gameObject.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                Color targetColor = Color.Lerp(plant.species.unhealthyColor, plant.species.healthyColor, plant.health);
                  // Add seasonal color changes
                if (enableSeasonalGrowth && WorldManager.Instance != null)
                {
                    var season = WorldManager.Instance.timeSystem.currentSeason;
                    if (season == WorldManager.TimeSystem.Season.Autumn && plant.species.type == PlantType.Tree)
                    {
                        targetColor = Color.Lerp(targetColor, new Color(1f, 0.5f, 0f), 0.7f); // Orange/red
                    }
                }
                
                renderer.material.color = targetColor;
            }
            
            // Wilting animation for low water
            if (plant.waterLevel < 0.3f)
            {
                plant.gameObject.transform.localRotation = Quaternion.Euler(
                    Mathf.Sin(Time.time * 2f) * 5f * (1f - plant.waterLevel),
                    0,
                    Mathf.Cos(Time.time * 2f) * 5f * (1f - plant.waterLevel)
                );
            }
        }

        #endregion

        #region Soil Management

        private void UpdateSoilQuality()
        {
            // Natural soil recovery and degradation
            for (int x = 0; x < gridResolution; x++)
            {
                for (int z = 0; z < gridResolution; z++)
                {
                    // Slowly trend toward baseline
                    float current = soilQualityGrid[x, z];
                    float baseline = 0.5f;
                    
                    soilQualityGrid[x, z] = Mathf.Lerp(current, baseline, 0.01f);
                }
            }
            
            // Apply plant effects on soil
            foreach (var plant in plants.Values)
            {
                Vector2Int gridPos = WorldToGridPosition(plant.position);
                
                if (IsValidGridPosition(gridPos))
                {
                    // Some plants enrich soil (nitrogen fixers)
                    if (plant.species.type == PlantType.Crop || plant.species.symbioticSpecies.Count > 0)
                    {
                        soilQualityGrid[gridPos.x, gridPos.y] = 
                            Mathf.Min(soilQualityGrid[gridPos.x, gridPos.y] + 0.001f, 1f);
                    }
                }
            }
        }

        private void EnrichSoilAtPosition(Vector3 position, float amount)
        {
            Vector2Int gridPos = WorldToGridPosition(position);
            
            if (IsValidGridPosition(gridPos))
            {
                soilQualityGrid[gridPos.x, gridPos.y] = 
                    Mathf.Clamp01(soilQualityGrid[gridPos.x, gridPos.y] + amount);
                
                // Spread to nearby cells
                for (int x = -1; x <= 1; x++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Vector2Int nearbyPos = gridPos + new Vector2Int(x, z);
                        if (IsValidGridPosition(nearbyPos))
                        {
                            soilQualityGrid[nearbyPos.x, nearbyPos.y] = 
                                Mathf.Clamp01(soilQualityGrid[nearbyPos.x, nearbyPos.y] + amount * 0.5f);
                        }
                    }
                }
            }
        }

        #endregion

        #region Spatial Indexing

        private void AddToSpatialIndex(Plant plant)
        {
            Vector2Int gridPos = GetGridPosition(plant.position);
            
            if (!spatialPlantIndex.ContainsKey(gridPos))
            {
                spatialPlantIndex[gridPos] = new List<string>();
            }
            
            spatialPlantIndex[gridPos].Add(plant.plantId);
        }

        private void RemoveFromSpatialIndex(Plant plant)
        {
            Vector2Int gridPos = GetGridPosition(plant.position);
            
            if (spatialPlantIndex.ContainsKey(gridPos))
            {
                spatialPlantIndex[gridPos].Remove(plant.plantId);
                
                if (spatialPlantIndex[gridPos].Count == 0)
                {
                    spatialPlantIndex.Remove(gridPos);
                }
            }
        }

        private Vector2Int GetGridPosition(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / gridCellSize),
                Mathf.FloorToInt(worldPos.z / gridCellSize)
            );
        }

        private Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt((worldPos.x + gridResolution * gridCellSize * 0.5f) / gridCellSize),
                Mathf.FloorToInt((worldPos.z + gridResolution * gridCellSize * 0.5f) / gridCellSize)
            );
        }

        private bool IsValidGridPosition(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < gridResolution && 
                   gridPos.y >= 0 && gridPos.y < gridResolution;
        }

        #endregion

        #region Event Handlers

        private void OnWeatherChanged(object data)
        {
            // Weather changes affect all plants immediately
            // Handled in environmental conditions checks
        }

        private void OnSeasonChanged(object data)
        {
            if (!enableSeasonalGrowth) return;
            
            // Trigger seasonal behaviors
            foreach (var plant in plants.Values)
            {
                EnvironmentalConditions conditions = GetEnvironmentalConditions(plant.position);
                UpdateDormancy(plant, conditions);
                  // Some plants die in winter
                if (WorldManager.Instance != null && WorldManager.Instance.timeSystem.currentSeason == WorldManager.TimeSystem.Season.Winter)
                {
                    if (plant.species.type == PlantType.Flower || plant.species.type == PlantType.Crop)
                    {
                        plant.health -= 0.5f;
                    }
                }
            }
        }

        private void OnPlantEaten(object data)
        {
            if (data is Dictionary<string, object> eventData)
            {
                if (eventData.ContainsKey("plant"))
                {
                    GameObject plantObj = eventData["plant"] as GameObject;
                    
                    // Find plant by GameObject
                    var plant = plants.Values.FirstOrDefault(p => p.gameObject == plantObj);
                    if (plant != null)
                    {
                        plant.health -= 0.3f;
                        
                        // Trigger regrowth if not dead
                        if (plant.health > 0 && plant.stage == GrowthStage.Mature)
                        {
                            plant.stage = GrowthStage.Young;
                            plant.height *= 0.7f;
                        }
                    }
                }
            }
        }

        private void OnSoilEnriched(object data)
        {
            if (data is Dictionary<string, object> eventData)
            {
                if (eventData.ContainsKey("position") && eventData.ContainsKey("amount"))
                {
                    Vector3 position = (Vector3)eventData["position"];
                    float amount = (float)eventData["amount"];
                    
                    EnrichSoilAtPosition(position, amount);
                }
            }
        }

        #endregion

        #region Public API

        public Plant GetNearestPlant(Vector3 position, PlantType? type = null, float maxDistance = 50f)
        {
            Plant nearest = null;
            float nearestDistance = maxDistance;
            
            foreach (var plant in plants.Values)
            {
                if (type.HasValue && plant.species.type != type.Value) continue;
                
                float distance = Vector3.Distance(position, plant.position);
                if (distance < nearestDistance)
                {
                    nearest = plant;
                    nearestDistance = distance;
                }
            }
            
            return nearest;
        }

        public List<Plant> GetPlantsInRadius(Vector3 center, float radius, PlantType? type = null)
        {
            List<Plant> result = new List<Plant>();
            
            foreach (var plant in plants.Values)
            {
                if (type.HasValue && plant.species.type != type.Value) continue;
                
                if (Vector3.Distance(center, plant.position) <= radius)
                {
                    result.Add(plant);
                }
            }
            
            return result;
        }

        public Plant GetPlant(string plantId)
        {
            return plants.ContainsKey(plantId) ? plants[plantId] : null;
        }

        public int GetPlantCount(PlantType? type = null)
        {
            if (type.HasValue)
            {
                return plantCounts.ContainsKey(type.Value) ? plantCounts[type.Value] : 0;
            }
            return plants.Count;
        }

        public float GetSoilQuality(Vector3 position)
        {
            Vector2Int gridPos = WorldToGridPosition(position);
            
            if (IsValidGridPosition(gridPos))
            {
                return soilQualityGrid[gridPos.x, gridPos.y];
            }
            
            return 0.5f; // Default
        }

        public Dictionary<string, object> GetEcosystemStats()
        {
            var stats = new Dictionary<string, object>
            {
                ["TotalPlants"] = plants.Count,
                ["TotalSeeds"] = seeds.Count,
                ["PlantTypes"] = plantCounts,
                ["AverageHealth"] = plants.Count > 0 ? plants.Values.Average(p => p.health) : 0f,
                ["AverageSoilQuality"] = CalculateAverageSoilQuality()
            };
            
            // Growth stage distribution
            Dictionary<GrowthStage, int> stageCount = new Dictionary<GrowthStage, int>();
            foreach (GrowthStage stage in Enum.GetValues(typeof(GrowthStage)))
            {
                stageCount[stage] = plants.Values.Count(p => p.stage == stage);
            }
            stats["StageDistribution"] = stageCount;
            
            return stats;
        }

        private float CalculateAverageSoilQuality()
        {
            float total = 0f;
            for (int x = 0; x < gridResolution; x++)
            {
                for (int z = 0; z < gridResolution; z++)
                {
                    total += soilQualityGrid[x, z];
                }
            }
            return total / (gridResolution * gridResolution);
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Draw plants
            foreach (var plant in plants.Values)
            {
                if (plant.gameObject == null)
                {
                    // Color by health
                    Gizmos.color = Color.Lerp(Color.red, Color.green, plant.health);
                    Gizmos.DrawWireSphere(plant.position, plant.height * 0.5f);
                    
                    // Draw stage indicator
                    Gizmos.color = GetStageColor(plant.stage);
                    Gizmos.DrawCube(plant.position + Vector3.up * plant.height, Vector3.one * 0.2f);
                }
            }
            
            // Draw seeds
            Gizmos.color = Color.yellow * 0.5f;
            foreach (var seed in seeds)
            {
                Gizmos.DrawSphere(seed.position, 0.1f);
            }
            
            // Draw soil quality (sampled)
            if (Input.GetKey(KeyCode.F1)) // Only when key held
            {
                for (int x = 0; x < gridResolution; x += 5)
                {
                    for (int z = 0; z < gridResolution; z += 5)
                    {
                        float quality = soilQualityGrid[x, z];
                        Vector3 worldPos = new Vector3(
                            (x - gridResolution * 0.5f) * gridCellSize,
                            0,
                            (z - gridResolution * 0.5f) * gridCellSize
                        );
                        
                        Gizmos.color = new Color(quality, quality * 0.5f, 0, 0.3f);
                        Gizmos.DrawCube(worldPos, Vector3.one * gridCellSize);
                    }
                }
            }
        }

        private Color GetStageColor(GrowthStage stage)
        {
            return stage switch
            {
                GrowthStage.Seed => Color.brown,
                GrowthStage.Sprout => Color.yellow,
                GrowthStage.Young => Color.green,
                GrowthStage.Mature => Color.blue,
                GrowthStage.Flowering => Color.magenta,
                GrowthStage.Fruiting => Color.red,
                GrowthStage.Declining => Color.gray,
                GrowthStage.Dead => Color.black,
                _ => Color.white
            };        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (EventSystem.Instance != null)
            {
                EventSystem.Instance.Unsubscribe<object>("WeatherChanged", OnWeatherChanged);
                EventSystem.Instance.Unsubscribe<object>("SeasonChanged", OnSeasonChanged);
                EventSystem.Instance.Unsubscribe<object>("PlantEaten", OnPlantEaten);
                EventSystem.Instance.Unsubscribe<object>("SoilEnriched", OnSoilEnriched);
            }
        }

        #endregion
    }
}