using UnityEngine;

public class AdvancedMinipollCore : MonoBehaviour
{
    [Header("=== Identity ===")]
    public string minipollName = "AdvancedMinipoll";
    public Color primaryColor = Color.cyan;
    
    [Header("=== Core Stats ===")]
    public float health = 100f;
    public float hunger = 75f;
    public float thirst = 75f;
    public float energy = 80f;
    public float happiness = 60f;
    
    [Header("=== AI Settings ===")]
    public float seekFoodThreshold = 30f;
    public float seekWaterThreshold = 25f;
    public float restThreshold = 20f;
    public float moveSpeed = 3f;
    
    private Rigidbody rb;
    private Material material;
    private Vector3 targetPosition;
    private bool hasTarget = false;
    private string currentAction = "Idle";
    private GameObject currentTarget;
    private bool isAlive = true;
    
    public string CurrentAction => currentAction;
    public bool IsAlive => isAlive && health > 0f;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
            material.color = primaryColor;
        }
        
        gameObject.name = minipollName;
        Debug.Log($"[AdvancedMinipollCore] {minipollName} initialized!");
    }
    
    private void Update()
    {
        if (!IsAlive) return;
        
        UpdateNeeds();
        MakeDecisions();
        ExecuteMovement();
        UpdateVisuals();
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
        
        // Natural happiness recovery when needs are met
        if (hunger > 50f && thirst > 50f && energy > 50f)
        {
            happiness = Mathf.Min(100f, happiness + deltaTime * 2f);
        }
        
        // Check for death
        if (health <= 0f && isAlive)
        {
            Die();
        }
    }
    
    private void MakeDecisions()
    {
        // Priority-based AI decisions
        if (hunger <= seekFoodThreshold)
        {
            SeekFood();
        }
        else if (thirst <= seekWaterThreshold)
        {
            SeekWater();
        }
        else if (energy <= restThreshold)
        {
            SeekRest();
        }
        else if (!hasTarget && Random.Range(0f, 1f) < 0.01f)
        {
            WanderRandomly();
        }
    }
    
    private void SeekFood()
    {
        GameObject food = FindClosestByName("FoodSource");
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
        GameObject water = FindClosestByName("WaterSource");
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
        GameObject rest = FindClosestByName("RestArea");
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
            Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        SetTarget(randomPos, "Wandering", null);
    }
    
    private void SetTarget(Vector3 position, string action, GameObject target)
    {
        targetPosition = position;
        targetPosition.y = transform.position.y;
        hasTarget = true;
        currentTarget = target;
        SetAction(action);
    }
    
    private void SetAction(string newAction)
    {
        if (currentAction != newAction)
        {
            currentAction = newAction;
            Debug.Log($"[AdvancedMinipollCore] {minipollName}: {currentAction}");
        }
    }
    
    private void ExecuteMovement()
    {
        if (!hasTarget || rb == null) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.AddForce(direction * moveSpeed, ForceMode.Force);
        
        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 1.5f)
        {
            if (currentTarget != null)
            {
                InteractWithTarget(currentTarget);
            }
            
            hasTarget = false;
            currentTarget = null;
            SetAction("Idle");
            rb.linearVelocity = Vector3.zero;
        }
    }
    
    private void InteractWithTarget(GameObject target)
    {
        if (target.name.Contains("FoodSource"))
        {
            Feed(40f);
            Debug.Log($"{minipollName} ate! Hunger: {hunger:F1}");
        }
        else if (target.name.Contains("WaterSource"))
        {
            Drink(45f);
            Debug.Log($"{minipollName} drank! Thirst: {thirst:F1}");
        }
        else if (target.name.Contains("RestArea"))
        {
            Rest(35f);
            Debug.Log($"{minipollName} rested! Energy: {energy:F1}");
        }
    }
    
    private GameObject FindClosestByName(string nameContains)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        GameObject closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(nameContains))
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
    
    private void UpdateVisuals()
    {
        if (material == null) return;
        
        if (!IsAlive)
            material.color = Color.gray;
        else if (health < 30f)
            material.color = Color.red;
        else if (hunger < 20f)
            material.color = Color.orange;
        else if (thirst < 20f)
            material.color = Color.blue;
        else if (energy < 20f)
            material.color = Color.yellow;
        else if (happiness > 70f)
            material.color = Color.green;
        else
            material.color = primaryColor;
    }
    
    public void Feed(float amount)
    {
        hunger = Mathf.Min(100f, hunger + amount);
        happiness = Mathf.Min(100f, happiness + amount * 0.3f);
    }
    
    public void Drink(float amount)
    {
        thirst = Mathf.Min(100f, thirst + amount);
        happiness = Mathf.Min(100f, happiness + amount * 0.25f);
    }
    
    public void Rest(float amount)
    {
        energy = Mathf.Min(100f, energy + amount);
        happiness = Mathf.Min(100f, happiness + amount * 0.2f);
    }
    
    private void Die()
    {
        isAlive = false;
        SetAction("Dead");
        if (rb != null) rb.isKinematic = true;
        Debug.Log($"[AdvancedMinipollCore] {minipollName} has died");
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw stats above creature
        Vector3 pos = transform.position + Vector3.up * 3f;
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector3.right * (health / 100f) * 2f);
        pos += Vector3.up * 0.2f;
        
        Gizmos.color = Color.orange;
        Gizmos.DrawLine(pos, pos + Vector3.right * (hunger / 100f) * 2f);
        pos += Vector3.up * 0.2f;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + Vector3.right * (thirst / 100f) * 2f);
        pos += Vector3.up * 0.2f;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + Vector3.right * (energy / 100f) * 2f);
        
        if (hasTarget)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }
    }
}
