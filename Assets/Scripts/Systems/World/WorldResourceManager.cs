using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using MinipollEventSystem = MinipollGame.Core.EventSystem;

namespace MinipollGame.Systems.Core
{
    // Event types for the resource system
    public class ResourceHarvestedEvent 
    { 
        public int entityId; 
        public string nodeId; 
        public float amount; 
        public string resourceType; 
    }
      public class ResourceDepletedEvent 
    { 
        public WorldResourceManager.ResourceNode node; 
    }
    
    public class WeatherChangedEvent 
    { 
        public WorldManager.WeatherSystem.WeatherType weather; 
    }
    
    public class SeasonChangedEvent 
    { 
        public WorldManager.TimeSystem.Season season; 
    }
    
    public class ResourceScarcityEvent 
    { 
        public WorldResourceManager.ResourceCategory category; 
        public float scarcityLevel; 
    }
    
    public class ResourceTransferredEvent 
    { 
        public int from; 
        public int to; 
        public string resource; 
        public float amount; 
    }
    
    public class ResourceRegeneratedEvent
    {
        public string nodeId;
        public string resourceType;
        public Vector3 position;
    }

    /// <summary>
    /// Manages world resources - water, food, shelter, materials and their distribution
    /// </summary>
    public class WorldResourceManager : MonoBehaviour
    {
        private static WorldResourceManager _instance;
        public static WorldResourceManager Instance
        {
            get
            {                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<WorldResourceManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("WorldResourceManager");
                        _instance = go.AddComponent<WorldResourceManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Resource Configuration")]
        [SerializeField] private List<ResourceType> resourceTypes = new List<ResourceType>();
        [SerializeField] private float resourceUpdateInterval = 1f;
        [SerializeField] private float resourceDistributionRadius = 50f;

        [Header("Resource Generation")]
        [SerializeField] private bool enableDynamicGeneration = true;
        [SerializeField] private float baseGenerationRate = 1f;
        [SerializeField] private AnimationCurve scarcityImpactCurve;

        [Header("Resource Depletion")]
        [SerializeField] private bool enableDepletion = true;
        [SerializeField] private float depletionRate = 0.1f;
        [SerializeField] private float regenerationDelay = 30f;

        [Header("Environmental Impact")]
        [SerializeField] private bool weatherAffectsResources = true;
        [SerializeField] private bool seasonAffectsResources = true;
        [SerializeField] private float pollutionImpact = 0.8f;

        // Resource type definition
        [Serializable]
        public class ResourceType
        {
            public string resourceName;
            public ResourceCategory category;
            public float baseValue;
            public float maxCapacity;
            public float regenerationRate;
            public float qualityRange = 1f;
            public bool isRenewable = true;
            public bool affectedByWeather = true;
            public bool affectedBySeason = true;
            public Sprite icon;
            public Color mapColor = Color.white;
        }

        public enum ResourceCategory
        {
            Water,
            Food,
            Shelter,
            Material,
            Energy,
            Social,
            Knowledge
        }

        // Resource node in world
        [Serializable]
        public class ResourceNode
        {
            public string nodeId;
            public ResourceType resourceType;
            public Vector3 position;
            public float currentAmount;
            public float maxAmount;
            public float quality = 1f;
            public bool isDepleted;
            public float lastHarvestTime;
            public List<int> claimedBy;
            public GameObject worldObject;

            public ResourceNode()
            {
                nodeId = Guid.NewGuid().ToString();
                claimedBy = new List<int>();
            }
        }

        // Resource cluster for efficient spatial queries
        [Serializable]
        public class ResourceCluster
        {
            public Vector3 center;
            public float radius;
            public ResourceCategory primaryResource;
            public List<ResourceNode> nodes;
            public float totalValue;
            public float averageQuality;

            public ResourceCluster()
            {
                nodes = new List<ResourceNode>();
            }
        }

        // Resource claim/ownership
        [Serializable]
        public class ResourceClaim
        {
            public int claimantId;
            public string nodeId;
            public float claimTime;
            public float claimStrength;
            public bool isExclusive;
        }

        // Resource flow tracking
        [Serializable]
        public class ResourceFlow
        {
            public string resourceName;
            public float consumed;
            public float produced;
            public float wasted;
            public float transferred;
            public Dictionary<int, float> consumerData;

            public ResourceFlow()
            {
                consumerData = new Dictionary<int, float>();
            }
        }

        // System data
        private Dictionary<string, ResourceNode> resourceNodes = new Dictionary<string, ResourceNode>();
        private Dictionary<ResourceCategory, List<ResourceCluster>> resourceClusters = new Dictionary<ResourceCategory, List<ResourceCluster>>();
        private Dictionary<int, List<ResourceClaim>> entityClaims = new Dictionary<int, List<ResourceClaim>>();
        private Dictionary<string, ResourceFlow> resourceFlows = new Dictionary<string, ResourceFlow>();
        
        // Spatial indexing for performance
        private Dictionary<Vector2Int, List<string>> spatialIndex = new Dictionary<Vector2Int, List<string>>();
        private float gridCellSize = 10f;

        // Resource scarcity tracking
        private Dictionary<ResourceCategory, float> scarcityLevels = new Dictionary<ResourceCategory, float>();
        private Dictionary<ResourceCategory, float> demandLevels = new Dictionary<ResourceCategory, float>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeResourceSystem();
        }        private void Start()
        {
            // Subscribe to events using the correct generic API            if (MinipollEventSystem.Instance != null)
            {
                MinipollEventSystem.Instance.Subscribe<ResourceHarvestedEvent>(OnResourceHarvested);
                MinipollEventSystem.Instance.Subscribe<ResourceDepletedEvent>(OnResourceDepleted);
                MinipollEventSystem.Instance.Subscribe<WeatherChangedEvent>(OnWeatherChanged);
                MinipollEventSystem.Instance.Subscribe<SeasonChangedEvent>(OnSeasonChanged);
            }

            InvokeRepeating(nameof(UpdateResources), resourceUpdateInterval, resourceUpdateInterval);
            InvokeRepeating(nameof(RegenerateResources), regenerationDelay, regenerationDelay);
            InvokeRepeating(nameof(CalculateScarcity), 5f, 5f);
        }

        private void InitializeResourceSystem()
        {
            // Initialize resource categories
            foreach (ResourceCategory category in Enum.GetValues(typeof(ResourceCategory)))
            {
                resourceClusters[category] = new List<ResourceCluster>();
                scarcityLevels[category] = 0f;
                demandLevels[category] = 0f;
            }

            // Initialize resource flows
            foreach (var resourceType in resourceTypes)
            {
                resourceFlows[resourceType.resourceName] = new ResourceFlow
                {
                    resourceName = resourceType.resourceName
                };
            }
        }

        #region Resource Node Management

        public ResourceNode CreateResourceNode(ResourceType type, Vector3 position, float amount = -1f, GameObject worldObject = null)
        {
            ResourceNode node = new ResourceNode
            {
                resourceType = type,
                position = position,
                currentAmount = amount > 0 ? amount : type.baseValue,
                maxAmount = type.maxCapacity,
                quality = UnityEngine.Random.Range(0.5f, 1f) * type.qualityRange,
                worldObject = worldObject
            };

            resourceNodes[node.nodeId] = node;
            
            // Add to spatial index
            AddToSpatialIndex(node);
            
            // Update clusters
            UpdateResourceClusters(type.category);

            return node;
        }

        public void RemoveResourceNode(string nodeId)
        {
            if (resourceNodes.ContainsKey(nodeId))
            {
                var node = resourceNodes[nodeId];
                
                // Remove from spatial index
                RemoveFromSpatialIndex(node);
                
                // Remove claims
                foreach (var claimList in entityClaims.Values)
                {
                    claimList.RemoveAll(c => c.nodeId == nodeId);
                }
                
                resourceNodes.Remove(nodeId);
                
                // Update clusters
                UpdateResourceClusters(node.resourceType.category);
            }
        }

        private void AddToSpatialIndex(ResourceNode node)
        {
            Vector2Int gridPos = GetGridPosition(node.position);
            
            if (!spatialIndex.ContainsKey(gridPos))
            {
                spatialIndex[gridPos] = new List<string>();
            }
            
            spatialIndex[gridPos].Add(node.nodeId);
        }

        private void RemoveFromSpatialIndex(ResourceNode node)
        {
            Vector2Int gridPos = GetGridPosition(node.position);
            
            if (spatialIndex.ContainsKey(gridPos))
            {
                spatialIndex[gridPos].Remove(node.nodeId);
                
                if (spatialIndex[gridPos].Count == 0)
                {
                    spatialIndex.Remove(gridPos);
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

        #endregion

        #region Resource Discovery

        public List<ResourceNode> FindResourcesNearPosition(Vector3 position, float radius, ResourceCategory? category = null)
        {
            List<ResourceNode> foundResources = new List<ResourceNode>();
            
            // Calculate grid cells to check
            Vector2Int centerGrid = GetGridPosition(position);
            int cellRadius = Mathf.CeilToInt(radius / gridCellSize);
            
            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int z = -cellRadius; z <= cellRadius; z++)
                {
                    Vector2Int gridPos = centerGrid + new Vector2Int(x, z);
                    
                    if (spatialIndex.ContainsKey(gridPos))
                    {
                        foreach (string nodeId in spatialIndex[gridPos])
                        {
                            if (resourceNodes.ContainsKey(nodeId))
                            {
                                var node = resourceNodes[nodeId];
                                float distance = Vector3.Distance(position, node.position);
                                
                                if (distance <= radius && !node.isDepleted)
                                {
                                    if (!category.HasValue || node.resourceType.category == category.Value)
                                    {
                                        foundResources.Add(node);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return foundResources.OrderBy(n => Vector3.Distance(position, n.position)).ToList();
        }

        public ResourceNode FindNearestResource(Vector3 position, ResourceCategory category, float maxDistance = 100f)
        {
            var resources = FindResourcesNearPosition(position, maxDistance, category);
            return resources.FirstOrDefault();
        }

        public ResourceCluster FindNearestCluster(Vector3 position, ResourceCategory category)
        {
            if (!resourceClusters.ContainsKey(category)) return null;
            
            return resourceClusters[category]
                .OrderBy(c => Vector3.Distance(position, c.center))
                .FirstOrDefault();
        }

        public List<ResourceNode> GetUnclaimedResources(ResourceCategory category, float maxDistance, Vector3 position)
        {
            return FindResourcesNearPosition(position, maxDistance, category)
                .Where(n => n.claimedBy.Count == 0)
                .ToList();
        }

        #endregion

        #region Resource Harvesting

        public float HarvestResource(int entityId, string nodeId, float requestedAmount)
        {
            if (!resourceNodes.ContainsKey(nodeId)) return 0f;
            
            var node = resourceNodes[nodeId];
            if (node.isDepleted) return 0f;
            
            // Calculate actual harvest amount
            float harvestAmount = Mathf.Min(requestedAmount, node.currentAmount);
            harvestAmount *= node.quality; // Quality affects yield
            
            // Apply environmental modifiers
            harvestAmount *= GetEnvironmentalModifier(node);
            
            // Deduct from node
            node.currentAmount -= harvestAmount;
            node.lastHarvestTime = Time.time;
            
            // Track flow
            if (resourceFlows.ContainsKey(node.resourceType.resourceName))
            {
                var flow = resourceFlows[node.resourceType.resourceName];
                flow.consumed += harvestAmount;
                
                if (!flow.consumerData.ContainsKey(entityId))
                {
                    flow.consumerData[entityId] = 0f;
                }
                flow.consumerData[entityId] += harvestAmount;
            }
              // Check depletion
            if (node.currentAmount <= 0)
            {
                node.isDepleted = true;
                MinipollEventSystem.Instance?.Publish(new ResourceDepletedEvent { node = node });
            }
            
            // Update demand tracking
            demandLevels[node.resourceType.category] += harvestAmount / node.resourceType.baseValue;
              // Broadcast harvest event
            MinipollEventSystem.Instance?.Publish(new ResourceHarvestedEvent
            {
                entityId = entityId,
                nodeId = nodeId,
                amount = harvestAmount,
                resourceType = node.resourceType.resourceName
            });
            
            return harvestAmount;
        }        private float GetEnvironmentalModifier(ResourceNode node)
        {
            float modifier = 1f;
            
            if (weatherAffectsResources && node.resourceType.affectedByWeather && WorldManager.Instance != null)
            {
                var weather = WorldManager.Instance.weatherSystem.currentWeather;
                
                switch (node.resourceType.category)
                {
                    case ResourceCategory.Water:
                        if (weather == WorldManager.WeatherSystem.WeatherType.Rain) modifier = 1.5f;
                        else if (weather == WorldManager.WeatherSystem.WeatherType.Storm) modifier = 1.3f;
                        break;
                        
                    case ResourceCategory.Food:
                        if (weather == WorldManager.WeatherSystem.WeatherType.Clear) modifier = 1.2f;
                        else if (weather == WorldManager.WeatherSystem.WeatherType.Storm) modifier = 0.5f;
                        break;
                }
            }
            
            if (seasonAffectsResources && node.resourceType.affectedBySeason && WorldManager.Instance != null)
            {
                var season = WorldManager.Instance.timeSystem.currentSeason;
                
                switch (node.resourceType.category)
                {
                    case ResourceCategory.Food:
                        if (season == WorldManager.TimeSystem.Season.Summer) modifier *= 1.3f;
                        else if (season == WorldManager.TimeSystem.Season.Winter) modifier *= 0.4f;
                        break;
                }
            }
            
            return modifier;
        }

        #endregion

        #region Resource Claims

        public bool ClaimResource(int entityId, string nodeId, bool exclusive = false)
        {
            if (!resourceNodes.ContainsKey(nodeId)) return false;
            
            var node = resourceNodes[nodeId];
            
            // Check if already claimed exclusively
            if (node.claimedBy.Count > 0 && HasExclusiveClaim(nodeId))
            {
                return false;
            }
            
            // Create claim
            ResourceClaim claim = new ResourceClaim
            {
                claimantId = entityId,
                nodeId = nodeId,
                claimTime = Time.time,
                claimStrength = 1f,
                isExclusive = exclusive
            };
            
            // Add to entity claims
            if (!entityClaims.ContainsKey(entityId))
            {
                entityClaims[entityId] = new List<ResourceClaim>();
            }
            entityClaims[entityId].Add(claim);
            
            // Add to node claims
            if (!node.claimedBy.Contains(entityId))
            {
                node.claimedBy.Add(entityId);
            }
            
            return true;
        }

        public void ReleaseResource(int entityId, string nodeId)
        {
            if (resourceNodes.ContainsKey(nodeId))
            {
                resourceNodes[nodeId].claimedBy.Remove(entityId);
            }
            
            if (entityClaims.ContainsKey(entityId))
            {
                entityClaims[entityId].RemoveAll(c => c.nodeId == nodeId);
            }
        }

        public void ReleaseAllClaims(int entityId)
        {
            if (entityClaims.ContainsKey(entityId))
            {
                foreach (var claim in entityClaims[entityId])
                {
                    if (resourceNodes.ContainsKey(claim.nodeId))
                    {
                        resourceNodes[claim.nodeId].claimedBy.Remove(entityId);
                    }
                }
                entityClaims.Remove(entityId);
            }
        }

        private bool HasExclusiveClaim(string nodeId)
        {
            foreach (var claimList in entityClaims.Values)
            {
                var claim = claimList.FirstOrDefault(c => c.nodeId == nodeId && c.isExclusive);
                if (claim != null) return true;
            }
            return false;
        }

        #endregion

        #region Resource Updates

        private void UpdateResources()
        {
            if (!enableDynamicGeneration) return;
            
            // Update resource flows
            foreach (var flow in resourceFlows.Values)
            {
                // Reset flow tracking
                flow.consumed = 0;
                flow.produced = 0;
                flow.wasted = 0;
                flow.consumerData.Clear();
            }
            
            // Check for new resource generation
            GenerateNewResources();
            
            // Update existing resources
            foreach (var node in resourceNodes.Values)
            {
                UpdateResourceNode(node);
            }
        }

        private void UpdateResourceNode(ResourceNode node)
        {
            // Apply environmental effects
            if (weatherAffectsResources || seasonAffectsResources)
            {
                float envModifier = GetEnvironmentalModifier(node);
                node.quality = Mathf.Clamp(node.quality * envModifier, 0.1f, node.resourceType.qualityRange);
            }
            
            // Apply pollution
            if (pollutionImpact < 1f)
            {
                // Could check pollution levels in area
                node.quality *= pollutionImpact;
            }
        }

        private void GenerateNewResources()
        {
            foreach (var resourceType in resourceTypes)
            {
                float scarcity = scarcityLevels.ContainsKey(resourceType.category) ? 
                    scarcityLevels[resourceType.category] : 0f;
                
                // Higher scarcity = higher generation rate (to balance)
                float generationChance = baseGenerationRate * scarcityImpactCurve.Evaluate(scarcity);
                
                if (UnityEngine.Random.value < generationChance * Time.deltaTime)
                {
                    // Find suitable location
                    Vector3 spawnPosition = FindSuitableSpawnLocation(resourceType);
                    
                    if (spawnPosition != Vector3.zero)
                    {
                        CreateResourceNode(resourceType, spawnPosition);
                    }
                }
            }
        }

        private Vector3 FindSuitableSpawnLocation(ResourceType resourceType)
        {
            // Simple random spawn for now
            // Could be improved with biome/terrain checks
            float x = UnityEngine.Random.Range(-resourceDistributionRadius, resourceDistributionRadius);
            float z = UnityEngine.Random.Range(-resourceDistributionRadius, resourceDistributionRadius);
            
            Vector3 position = new Vector3(x, 0, z);
            
            // Adjust Y to terrain height
            if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f))
            {
                position.y = hit.point.y;
            }
            
            return position;
        }

        #endregion

        #region Resource Regeneration

        private void RegenerateResources()
        {
            foreach (var node in resourceNodes.Values)
            {
                if (node.isDepleted && node.resourceType.isRenewable)
                {
                    float timeSinceDepletion = Time.time - node.lastHarvestTime;
                    
                    if (timeSinceDepletion >= regenerationDelay)
                    {
                        RegenerateNode(node);
                    }
                }
                else if (node.currentAmount < node.maxAmount && node.resourceType.isRenewable)
                {
                    // Gradual regeneration
                    float regenAmount = node.resourceType.regenerationRate * regenerationDelay;
                    regenAmount *= GetEnvironmentalModifier(node);
                    
                    node.currentAmount = Mathf.Min(node.currentAmount + regenAmount, node.maxAmount);
                    
                    // Track production
                    if (resourceFlows.ContainsKey(node.resourceType.resourceName))
                    {
                        resourceFlows[node.resourceType.resourceName].produced += regenAmount;
                    }
                }
            }
        }

        private void RegenerateNode(ResourceNode node)
        {
            node.isDepleted = false;
            node.currentAmount = node.resourceType.baseValue * UnityEngine.Random.Range(0.5f, 1f);
            node.quality = UnityEngine.Random.Range(0.7f, 1f) * node.resourceType.qualityRange;
            
            // Visual feedback
            if (node.worldObject != null)
            {
                // Could trigger regrowth animation
                node.worldObject.SetActive(true);
            }            
            MinipollEventSystem.Instance?.Publish(new ResourceRegeneratedEvent
            {
                nodeId = node.nodeId,
                resourceType = node.resourceType.resourceName,
                position = node.position
            });
        }

        #endregion

        #region Resource Clustering

        private void UpdateResourceClusters(ResourceCategory category)
        {
            var nodes = resourceNodes.Values
                .Where(n => n.resourceType.category == category && !n.isDepleted)
                .ToList();
            
            // Simple clustering algorithm
            List<ResourceCluster> clusters = new List<ResourceCluster>();
            HashSet<string> processedNodes = new HashSet<string>();
            
            foreach (var node in nodes)
            {
                if (processedNodes.Contains(node.nodeId)) continue;
                
                ResourceCluster cluster = new ResourceCluster
                {
                    center = node.position,
                    primaryResource = category,
                    radius = 20f // Base cluster radius
                };
                
                // Find nearby nodes
                var nearbyNodes = nodes
                    .Where(n => !processedNodes.Contains(n.nodeId) && 
                           Vector3.Distance(node.position, n.position) <= cluster.radius)
                    .ToList();
                
                foreach (var nearbyNode in nearbyNodes)
                {
                    cluster.nodes.Add(nearbyNode);
                    processedNodes.Add(nearbyNode.nodeId);
                }
                
                if (cluster.nodes.Count > 0)
                {
                    // Calculate cluster properties
                    cluster.center = cluster.nodes.Aggregate(Vector3.zero, (sum, n) => sum + n.position) / cluster.nodes.Count;
                    cluster.totalValue = cluster.nodes.Sum(n => n.currentAmount * n.quality);
                    cluster.averageQuality = cluster.nodes.Average(n => n.quality);
                    
                    clusters.Add(cluster);
                }
            }
            
            resourceClusters[category] = clusters;
        }

        #endregion

        #region Scarcity Calculation

        private void CalculateScarcity()
        {
            foreach (ResourceCategory category in Enum.GetValues(typeof(ResourceCategory)))
            {
                float totalAvailable = 0f;
                float totalCapacity = 0f;
                
                var categoryNodes = resourceNodes.Values
                    .Where(n => n.resourceType.category == category)
                    .ToList();
                
                foreach (var node in categoryNodes)
                {
                    totalAvailable += node.currentAmount * node.quality;
                    totalCapacity += node.maxAmount;
                }
                
                // Calculate scarcity (0 = abundant, 1 = scarce)
                float availability = totalCapacity > 0 ? totalAvailable / totalCapacity : 0f;
                scarcityLevels[category] = 1f - availability;
                
                // Factor in demand
                float demand = demandLevels.ContainsKey(category) ? demandLevels[category] : 0f;
                scarcityLevels[category] = Mathf.Clamp01(scarcityLevels[category] + demand * 0.1f);
                  // Trigger scarcity events
                if (scarcityLevels[category] > 0.8f)
                {
                    MinipollEventSystem.Instance?.Publish(new ResourceScarcityEvent
                    {
                        category = category,
                        scarcityLevel = scarcityLevels[category]
                    });
                }
            }
            
            // Reset demand tracking
            foreach (var key in demandLevels.Keys.ToList())
            {
                demandLevels[key] *= 0.9f; // Gradual decay
            }
        }

        #endregion

        #region Trading and Sharing

        public bool TransferResource(int fromEntity, int toEntity, string resourceName, float amount)
        {
            // Track resource transfer
            if (resourceFlows.ContainsKey(resourceName))
            {
                resourceFlows[resourceName].transferred += amount;
            }
              // Could implement actual inventory transfer here
            MinipollEventSystem.Instance?.Publish(new ResourceTransferredEvent
            {
                from = fromEntity,
                to = toEntity,
                resource = resourceName,
                amount = amount
            });
            
            return true;
        }

        public float CalculateTradeFairness(string offeredResource, float offeredAmount, string requestedResource, float requestedAmount)
        {
            // Calculate based on scarcity and value
            var offeredType = resourceTypes.FirstOrDefault(r => r.resourceName == offeredResource);
            var requestedType = resourceTypes.FirstOrDefault(r => r.resourceName == requestedResource);
            
            if (offeredType == null || requestedType == null) return 0f;
            
            float offeredValue = offeredAmount * offeredType.baseValue * 
                (1f + scarcityLevels.GetValueOrDefault(offeredType.category, 0f));
            
            float requestedValue = requestedAmount * requestedType.baseValue * 
                (1f + scarcityLevels.GetValueOrDefault(requestedType.category, 0f));
            
            return offeredValue / Mathf.Max(0.01f, requestedValue);        }

        #endregion

        #region Event Handlers

        private void OnResourceHarvested(ResourceHarvestedEvent eventData)
        {
            // Already handled in HarvestResource method
        }

        private void OnResourceDepleted(ResourceDepletedEvent eventData)
        {
            var node = eventData.node;
            if (node != null)
            {
                // Visual feedback
                if (node.worldObject != null)
                {
                    // Could trigger depletion effect
                    node.worldObject.SetActive(false);
                }
                
                // Update clusters
                UpdateResourceClusters(node.resourceType.category);
            }
        }

        private void OnWeatherChanged(WeatherChangedEvent eventData)
        {
            if (!weatherAffectsResources) return;
            
            // Update all resource qualities based on new weather
            foreach (var node in resourceNodes.Values)
            {
                if (node.resourceType.affectedByWeather)
                {
                    UpdateResourceNode(node);
                }
            }
        }

        private void OnSeasonChanged(SeasonChangedEvent eventData)
        {
            if (!seasonAffectsResources) return;
            
            // Major seasonal resource update
            foreach (var node in resourceNodes.Values)
            {
                if (node.resourceType.affectedBySeason)
                {
                    UpdateResourceNode(node);
                      // Seasonal regeneration changes
                    if (WorldManager.Instance != null)
                    {
                        var season = WorldManager.Instance.timeSystem.currentSeason;
                        
                        if (season == WorldManager.TimeSystem.Season.Spring && node.resourceType.category == ResourceCategory.Food)
                        {
                            // Boost food resources in spring
                            node.currentAmount = Mathf.Min(node.currentAmount * 1.5f, node.maxAmount);
                        }
                        else if (season == WorldManager.TimeSystem.Season.Winter)
                        {
                            // Reduce most resources in winter
                            node.currentAmount *= 0.7f;
                        }
                    }
                }
            }
            
            // Update all clusters
            foreach (ResourceCategory category in Enum.GetValues(typeof(ResourceCategory)))
            {
                UpdateResourceClusters(category);
            }
        }

        #endregion

        #region Public API

        public float GetResourceAvailability(ResourceCategory category)
        {
            return 1f - scarcityLevels.GetValueOrDefault(category, 0f);
        }

        public float GetResourceScarcity(ResourceCategory category)
        {
            return scarcityLevels.GetValueOrDefault(category, 0f);
        }

        public List<ResourceNode> GetResourceNodes(ResourceCategory category)
        {
            return resourceNodes.Values
                .Where(n => n.resourceType.category == category)
                .ToList();
        }

        public ResourceNode GetResourceNode(string nodeId)
        {
            return resourceNodes.ContainsKey(nodeId) ? resourceNodes[nodeId] : null;
        }

        public List<ResourceClaim> GetEntityClaims(int entityId)
        {
            return entityClaims.ContainsKey(entityId) ? 
                new List<ResourceClaim>(entityClaims[entityId]) : 
                new List<ResourceClaim>();
        }

        public Dictionary<string, float> GetResourceFlowStats(string resourceName)
        {
            if (resourceFlows.ContainsKey(resourceName))
            {
                var flow = resourceFlows[resourceName];
                return new Dictionary<string, float>
                {
                    ["consumed"] = flow.consumed,
                    ["produced"] = flow.produced,
                    ["wasted"] = flow.wasted,
                    ["transferred"] = flow.transferred,
                    ["netFlow"] = flow.produced - flow.consumed
                };
            }
            return new Dictionary<string, float>();
        }

        public Dictionary<string, object> GetResourceStats()
        {
            return new Dictionary<string, object>
            {
                ["TotalNodes"] = resourceNodes.Count,
                ["ActiveNodes"] = resourceNodes.Count(n => !n.Value.isDepleted),
                ["TotalClusters"] = resourceClusters.Sum(kvp => kvp.Value.Count),
                ["ActiveClaims"] = entityClaims.Sum(kvp => kvp.Value.Count),
                ["AverageScarcity"] = scarcityLevels.Values.Average()
            };
        }

        #endregion

        #region Debug Visualization

        // private void OnDrawGizmos()
        // {
        //     if (!Application.isPlaying) return;

        //     // Draw resource nodes
        //     foreach (var node in resourceNodes.Values)
        //     {
        //         if (node.worldObject == null)
        //         {
        //             Gizmos.color = node.isDepleted ? Color.gray : node.resourceType.mapColor;
        //             Gizmos.DrawSphere(node.position, 0.5f + node.currentAmount / node.maxAmount);
        //         }
        //     }

        //     // Draw resource clusters
        //     foreach (var categoryCluster in resourceClusters)
        //     {
        //         Gizmos.color = Color.yellow * 0.3f;
        //         foreach (var cluster in categoryCluster.Value)
        //         {
        //             Gizmos.DrawWireSphere(cluster.center, cluster.radius);
        //         }
        //     }

        //     // Draw claims
        //     Gizmos.color = Color.cyan * 0.5f;
        //     foreach (var node in resourceNodes.Values)
        //     {                if (node.claimedBy.Count > 0)
        //         {
        //             foreach (int claimant in node.claimedBy)
        //             {                        var minipoll = MinipollCore.MinipollManager.Instance?.FindMinipollByName($"Minipoll_{claimant}");
        //                 if (minipoll != null && minipoll.gameObject is GameObject go)
        //                 {
        //                     Gizmos.DrawLine(node.position, go.transform.position);
        //                 }
        //             }
        //         }
        //     }
        // }

        #endregion

        private void OnDestroy()
        {
            if (MinipollEventSystem.Instance != null)
            {                MinipollEventSystem.Instance.Unsubscribe<ResourceHarvestedEvent>(OnResourceHarvested);
                MinipollEventSystem.Instance.Unsubscribe<ResourceDepletedEvent>(OnResourceDepleted);
                MinipollEventSystem.Instance.Unsubscribe<WeatherChangedEvent>(OnWeatherChanged);
                MinipollEventSystem.Instance.Unsubscribe<SeasonChangedEvent>(OnSeasonChanged);
            }

            // Release all claims
            foreach (var entityId in entityClaims.Keys.ToList())
            {
                ReleaseAllClaims(entityId);
            }
        }
    }
}