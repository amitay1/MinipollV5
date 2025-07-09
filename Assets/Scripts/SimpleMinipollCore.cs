using UnityEngine;

namespace MinipollGame.Core
{
    /// <summary>
    /// Simple Minipoll Controller for testing basic functionality
    /// סקריפט פשוט לבדיקת פונקציונליות בסיסית של מיניפול
    /// </summary>
    public class SimpleMinipollCore : MonoBehaviour
    {
        [Header("=== Basic Properties ===")]
        [SerializeField] private string minipollName = "TestMinipoll";
        [SerializeField] private float health = 100f;
        [SerializeField] private float hunger = 50f;
        [SerializeField] private float thirst = 50f;
        [SerializeField] private float energy = 75f;
        [SerializeField] private Color primaryColor = Color.cyan;
        
        [Header("=== Movement ===")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private bool canMove = true;
        
        private Vector3 targetPosition;
        private bool isMoving = false;
        
        public string Name => minipollName;
        public float Health => health;
        public float Hunger => hunger;
        public float Thirst => thirst;
        public float Energy => energy;
        public Color PrimaryColor => primaryColor;
        public bool IsAlive => health > 0f;
        
        private void Start()
        {
            // Set random name if empty
            if (string.IsNullOrEmpty(minipollName))
                minipollName = GenerateRandomName();
                
            // Set gameobject name
            gameObject.name = $"SimpleMinipoll_{minipollName}";
            
            // Apply color to renderer if available
            ApplyPrimaryColor();
            
            Debug.Log($"[SimpleMinipollCore] {minipollName} initialized with health: {health}");
        }
        
        private void Update()
        {
            if (!IsAlive) return;
            
            // Update needs over time
            UpdateNeeds();
            
            // Handle movement
            HandleMovement();
            
            // Handle input for testing
            HandleInput();
        }
        
        private void UpdateNeeds()
        {
            // Gradually decrease needs over time
            float decreaseRate = Time.deltaTime * 2f; // Adjust as needed
            
            hunger = Mathf.Max(0, hunger - decreaseRate);
            thirst = Mathf.Max(0, thirst - decreaseRate);
            energy = Mathf.Max(0, energy - decreaseRate * 0.5f);
            
            // Check for critical needs
            CheckCriticalNeeds();
        }
        
        private void CheckCriticalNeeds()
        {
            if (hunger <= 10f)
            {
                Debug.LogWarning($"[SimpleMinipollCore] {minipollName} is starving!");
                // Lose health when starving
                health = Mathf.Max(0, health - Time.deltaTime * 5f);
            }
            
            if (thirst <= 10f)
            {
                Debug.LogWarning($"[SimpleMinipollCore] {minipollName} is dehydrated!");
                // Lose health when dehydrated
                health = Mathf.Max(0, health - Time.deltaTime * 8f);
            }
            
            if (energy <= 5f)
            {
                Debug.LogWarning($"[SimpleMinipollCore] {minipollName} is exhausted!");
                moveSpeed = 0.5f; // Move slower when tired
            }
            else
            {
                moveSpeed = 2f; // Normal speed
            }
            
            // Check for death
            if (health <= 0f)
            {
                Die();
            }
        }
        
        private void HandleMovement()
        {
            if (!canMove || !IsAlive) return;
            
            if (isMoving)
            {
                // Move towards target
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                
                // Check if reached target
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    isMoving = false;
                    Debug.Log($"[SimpleMinipollCore] {minipollName} reached destination");
                }
            }
        }
        
        private void HandleInput()
        {
            // For testing: click to move
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    MoveTo(hit.point);
                }
            }
        }
        
        public void MoveTo(Vector3 position)
        {
            if (!canMove || !IsAlive) return;
            
            targetPosition = position;
            targetPosition.y = transform.position.y; // Keep same height
            isMoving = true;
            
            Debug.Log($"[SimpleMinipollCore] {minipollName} moving to {position}");
        }
        
        public void Feed(float amount)
        {
            if (!IsAlive) return;
            
            hunger = Mathf.Min(100f, hunger + amount);
            Debug.Log($"[SimpleMinipollCore] {minipollName} was fed. Hunger: {hunger:F1}");
        }
        
        public void GiveWater(float amount)
        {
            if (!IsAlive) return;
            
            thirst = Mathf.Min(100f, thirst + amount);
            Debug.Log($"[SimpleMinipollCore] {minipollName} drank water. Thirst: {thirst:F1}");
        }
        
        public void Rest(float amount)
        {
            if (!IsAlive) return;
            
            energy = Mathf.Min(100f, energy + amount);
            Debug.Log($"[SimpleMinipollCore] {minipollName} rested. Energy: {energy:F1}");
        }
        
        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            
            health = Mathf.Max(0f, health - damage);
            Debug.Log($"[SimpleMinipollCore] {minipollName} took {damage} damage. Health: {health:F1}");
            
            if (health <= 0f)
                Die();
        }
        
        public void Heal(float amount)
        {
            if (!IsAlive) return;
            
            health = Mathf.Min(100f, health + amount);
            Debug.Log($"[SimpleMinipollCore] {minipollName} healed {amount}. Health: {health:F1}");
        }
        
        private void Die()
        {
            if (!IsAlive) return;
            
            Debug.Log($"[SimpleMinipollCore] {minipollName} has died");
            
            // Change color to indicate death
            ApplyColor(Color.gray);
            
            // Stop movement
            canMove = false;
            isMoving = false;
        }
        
        private void ApplyPrimaryColor()
        {
            ApplyColor(primaryColor);
        }
        
        private void ApplyColor(Color color)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
            
            // Also try to apply to children
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in childRenderers)
            {
                childRenderer.material.color = color;
            }
        }
        
        private string GenerateRandomName()
        {
            string[] prefixes = { "Blob", "Puff", "Squish", "Bounce", "Wiggle", "Bubble", "Fuzzy", "Squishy" };
            string[] suffixes = { "y", "ie", "ster", "ling", "bert", "ina", "o", "a" };
            
            string prefix = prefixes[Random.Range(0, prefixes.Length)];
            string suffix = suffixes[Random.Range(0, suffixes.Length)];
            
            return prefix + suffix;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw health bar
            Gizmos.color = Color.red;
            Vector3 healthBarPos = transform.position + Vector3.up * 2f;
            Gizmos.DrawLine(healthBarPos, healthBarPos + Vector3.right * (health / 100f));
            
            // Draw hunger bar
            Gizmos.color = Color.orange;
            Vector3 hungerBarPos = transform.position + Vector3.up * 2.2f;
            Gizmos.DrawLine(hungerBarPos, hungerBarPos + Vector3.right * (hunger / 100f));
            
            // Draw thirst bar
            Gizmos.color = Color.blue;
            Vector3 thirstBarPos = transform.position + Vector3.up * 2.4f;
            Gizmos.DrawLine(thirstBarPos, thirstBarPos + Vector3.right * (thirst / 100f));
            
            // Draw energy bar
            Gizmos.color = Color.yellow;
            Vector3 energyBarPos = transform.position + Vector3.up * 2.6f;
            Gizmos.DrawLine(energyBarPos, energyBarPos + Vector3.right * (energy / 100f));
            
            // Draw target position if moving
            if (isMoving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(targetPosition, 0.5f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }
        }
    }
}
