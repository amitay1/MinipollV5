using UnityEngine;
using System;

namespace MinipollGame.Core
{
    /// <summary>
    /// Working MinipollCore - simplified version for CoreCreatureScene
    /// גרסה פשוטה ועובדת של MinipollCore לסצנת הליבה
    /// </summary>
    public class WorkingMinipollCore : MonoBehaviour
    {
        [Header("=== Minipoll Identity ===")]
        [SerializeField] private string minipollName = "WorkingMinipoll";
        [SerializeField] private string uniqueID;
        [SerializeField] private Color primaryColor = Color.cyan;
        [SerializeField] private float age = 0f;
        
        [Header("=== Core Stats ===")]
        [SerializeField] private float health = 100f;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float hunger = 75f;
        [SerializeField] private float thirst = 75f;
        [SerializeField] private float energy = 80f;
        [SerializeField] private float happiness = 60f;
        
        [Header("=== Movement ===")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private bool canMove = true;
        
        [Header("=== AI Behavior ===")]
        [SerializeField] private float seekFoodThreshold = 30f;
        [SerializeField] private float seekWaterThreshold = 25f;
        [SerializeField] private float restThreshold = 20f;
        
        // Components
        private Rigidbody rb;
        private Renderer meshRenderer;
        private Material material;
        
        // AI State
        private Vector3 targetPosition;
        private bool hasTarget = false;
        private string currentAction = "Idle";
        private GameObject currentTarget;
        
        // Properties
        public string Name => minipollName;
        public string ID => uniqueID;
        public float Health => health;
        public float Hunger => hunger;
        public float Thirst => thirst;
        public float Energy => energy;
        public float Happiness => happiness;
        public bool IsAlive => health > 0f;
        public string CurrentAction => currentAction;
        
        // Events
        public event Action<string> OnActionChanged;
        public event Action<float> OnHealthChanged;
        public event Action<string, float> OnNeedChanged;
        
        private void Start()
        {
            InitializeComponents();
            InitializeProperties();
            
            Debug.Log($"[WorkingMinipollCore] {minipollName} initialized and ready!");
        }
        
        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody>();
            meshRenderer = GetComponent<Renderer>();
            
            if (meshRenderer != null)
            {
                material = meshRenderer.material;
                UpdateVisualState();
            }
        }
        
        private void InitializeProperties()
        {
            // Generate unique ID if not set
            if (string.IsNullOrEmpty(uniqueID))
                uniqueID = System.Guid.NewGuid().ToString().Substring(0, 8);
                
            // Set gameobject name
            gameObject.name = $"WorkingMinipoll_{minipollName}_{uniqueID}";
            
            // Generate name if empty
            if (string.IsNullOrEmpty(minipollName) || minipollName == "WorkingMinipoll")
                minipollName = GenerateRandomName();
        }
        
        private void Update()
        {
            if (!IsAlive) return;
            
            // Update needs over time
            UpdateNeeds();
            
            // AI decision making
            MakeDecisions();
            
            // Execute current action
            ExecuteCurrentAction();
            
            // Update visuals
            UpdateVisualState();
        }
        
        private void UpdateNeeds()
        {
            float deltaTime = Time.deltaTime;
            
            // Decrease needs over time
            hunger = Mathf.Max(0, hunger - deltaTime * 3f);
            thirst = Mathf.Max(0, thirst - deltaTime * 4f);
            energy = Mathf.Max(0, energy - deltaTime * 1.5f);
            
            // Health effects from critical needs
            if (hunger <= 5f)
            {
                health = Mathf.Max(0, health - deltaTime * 8f);
                happiness = Mathf.Max(0, happiness - deltaTime * 10f);
            }
            
            if (thirst <= 5f)
            {
                health = Mathf.Max(0, health - deltaTime * 12f);
                happiness = Mathf.Max(0, happiness - deltaTime * 15f);
            }
            
            if (energy <= 5f)
            {
                moveSpeed = 1f; // Slower when tired
                happiness = Mathf.Max(0, happiness - deltaTime * 5f);
            }
            else
            {
                moveSpeed = 3f; // Normal speed
            }
            
            // Natural happiness recovery when needs are met
            if (hunger > 50f && thirst > 50f && energy > 50f)
            {
                happiness = Mathf.Min(100f, happiness + deltaTime * 2f);
            }
            
            // Check for death
            if (health <= 0f && IsAlive)
            {
                Die();
            }
        }
        
        private void MakeDecisions()
        {
            // Priority-based decision making
            
            // Emergency: Critical health
            if (health < 20f)
            {
                SetAction("Emergency");
                return;
            }
            
            // High priority: Critical needs
            if (hunger <= seekFoodThreshold)
            {
                SeekFood();
                return;
            }
            
            if (thirst <= seekWaterThreshold)
            {
                SeekWater();
                return;
            }
            
            if (energy <= restThreshold)
            {
                SeekRest();
                return;
            }
            
            // Low priority: Wander around
            if (!hasTarget || currentAction == "Idle")
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.01f) // 1% chance per frame
                {
                    WanderRandomly();
                }
            }
        }
        
        private void SeekFood()
        {
            GameObject food = FindClosest("FoodSource");
            if (food != null)
            {
                SetTarget(food.transform.position, "Seeking Food", food);
            }
            else
            {
                SetAction("Looking for Food");
                WanderRandomly();
            }
        }
        
        private void SeekWater()
        {
            GameObject water = FindClosest("WaterSource");
            if (water != null)
            {
                SetTarget(water.transform.position, "Seeking Water", water);
            }
            else
            {
                SetAction("Looking for Water");
                WanderRandomly();
            }
        }
        
        private void SeekRest()
        {
            GameObject rest = FindClosest("RestArea");
            if (rest != null)
            {
                SetTarget(rest.transform.position, "Seeking Rest", rest);
            }
            else
            {
                SetAction("Looking for Rest");
                WanderRandomly();
            }
        }
        
        private void WanderRandomly()
        {
            Vector3 randomPos = transform.position + new Vector3(
                UnityEngine.Random.Range(-5f, 5f),
                0,
                UnityEngine.Random.Range(-5f, 5f)
            );
            
            SetTarget(randomPos, "Wandering", null);
        }
        
        private void SetTarget(Vector3 position, string action, GameObject target)
        {
            targetPosition = position;
            targetPosition.y = transform.position.y; // Keep same height
            hasTarget = true;
            currentTarget = target;
            SetAction(action);
        }
        
        private void SetAction(string newAction)
        {
            if (currentAction != newAction)
            {
                currentAction = newAction;
                OnActionChanged?.Invoke(currentAction);
                Debug.Log($"[WorkingMinipollCore] {minipollName} is now: {currentAction}");
            }
        }
        
        private void ExecuteCurrentAction()
        {
            if (!hasTarget || !canMove || !IsAlive) return;
            
            // Move towards target
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb.AddForce(direction * moveSpeed, ForceMode.Force);
            
            // Check if reached target
            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance < 1.5f)
            {
                // Reached target - interact if it's a resource
                if (currentTarget != null)
                {
                    InteractWithTarget(currentTarget);
                }
                
                // Clear target
                hasTarget = false;
                currentTarget = null;
                SetAction("Idle");
                
                // Stop movement
                rb.linearVelocity = Vector3.zero;
            }
        }
        
        private void InteractWithTarget(GameObject target)
        {
            if (target.name.Contains("FoodSource"))
            {
                Feed(40f);
                Debug.Log($"[WorkingMinipollCore] {minipollName} ate food! Hunger: {hunger:F1}");
            }
            else if (target.name.Contains("WaterSource"))
            {
                Drink(45f);
                Debug.Log($"[WorkingMinipollCore] {minipollName} drank water! Thirst: {thirst:F1}");
            }
            else if (target.name.Contains("RestArea"))
            {
                Rest(35f);
                Debug.Log($"[WorkingMinipollCore] {minipollName} rested! Energy: {energy:F1}");
            }
        }
        
        private GameObject FindClosest(string objectName)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Untagged");
            GameObject closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (GameObject obj in objects)
            {
                if (obj.name.Contains(objectName))
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = obj;
                    }
                }
            }
            
            return closest;
        }
        
        private void UpdateVisualState()
        {
            if (material == null) return;
            
            if (!IsAlive)
            {
                material.color = Color.gray;
            }
            else if (health < 30f)
            {
                material.color = Color.red;
            }
            else if (hunger < 20f)
            {
                material.color = Color.orange;
            }
            else if (thirst < 20f)
            {
                material.color = Color.blue;
            }
            else if (energy < 20f)
            {
                material.color = Color.yellow;
            }
            else if (happiness > 70f)
            {
                material.color = Color.green;
            }
            else
            {
                material.color = primaryColor;
            }
        }
        
        // Public Methods
        public void Feed(float amount)
        {
            if (!IsAlive) return;
            
            hunger = Mathf.Min(100f, hunger + amount);
            happiness = Mathf.Min(100f, happiness + amount * 0.3f);
            OnNeedChanged?.Invoke("Hunger", hunger);
        }
        
        public void Drink(float amount)
        {
            if (!IsAlive) return;
            
            thirst = Mathf.Min(100f, thirst + amount);
            happiness = Mathf.Min(100f, happiness + amount * 0.25f);
            OnNeedChanged?.Invoke("Thirst", thirst);
        }
        
        public void Rest(float amount)
        {
            if (!IsAlive) return;
            
            energy = Mathf.Min(100f, energy + amount);
            happiness = Mathf.Min(100f, happiness + amount * 0.2f);
            OnNeedChanged?.Invoke("Energy", energy);
        }
        
        public void Heal(float amount)
        {
            if (!IsAlive) return;
            
            health = Mathf.Min(maxHealth, health + amount);
            OnHealthChanged?.Invoke(health);
        }
        
        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            
            health = Mathf.Max(0f, health - damage);
            happiness = Mathf.Max(0f, happiness - damage * 0.5f);
            OnHealthChanged?.Invoke(health);
            
            if (health <= 0f)
                Die();
        }
        
        private void Die()
        {
            Debug.Log($"[WorkingMinipollCore] {minipollName} has died");
            
            canMove = false;
            hasTarget = false;
            SetAction("Dead");
            
            if (rb != null)
                rb.isKinematic = true;
                
            UpdateVisualState();
        }
        
        private string GenerateRandomName()
        {
            string[] prefixes = { "Blob", "Puff", "Squish", "Bounce", "Wiggle", "Bubble", "Fuzzy", "Squishy", "Glow", "Spark" };
            string[] suffixes = { "y", "ie", "ster", "ling", "bert", "ina", "o", "a", "us", "ix" };
            
            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
            
            return prefix + suffix;
        }
        
        // Debug Visualization
        private void OnDrawGizmosSelected()
        {
            // Health bar (red)
            Vector3 pos = transform.position + Vector3.up * 3f;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + Vector3.right * (health / 100f) * 2f);
            
            // Hunger bar (orange)
            pos += Vector3.up * 0.2f;
            Gizmos.color = Color.orange;
            Gizmos.DrawLine(pos, pos + Vector3.right * (hunger / 100f) * 2f);
            
            // Thirst bar (blue)
            pos += Vector3.up * 0.2f;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pos, pos + Vector3.right * (thirst / 100f) * 2f);
            
            // Energy bar (yellow)
            pos += Vector3.up * 0.2f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos, pos + Vector3.right * (energy / 100f) * 2f);
            
            // Happiness bar (green)
            pos += Vector3.up * 0.2f;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos, pos + Vector3.right * (happiness / 100f) * 2f);
            
            // Target
            if (hasTarget)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(targetPosition, 0.5f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }
            
            // Info text
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 4.5f,
                $"{minipollName}\n{currentAction}\nH:{health:F0} Hu:{hunger:F0} T:{thirst:F0}\nE:{energy:F0} Ha:{happiness:F0}");
#endif
        }
    }
}
