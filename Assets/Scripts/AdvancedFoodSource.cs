using UnityEngine;

public class AdvancedFoodSource : MonoBehaviour
{
    [Header("Food Properties")]
    public float nutritionValue = 40f;
    public float feedRadius = 3f;
    public int maxFeedings = 5;
    public float regenerateTime = 10f;
    
    private int currentFeedings;
    private float lastFeedTime;
    private Material material;
    private bool isActive = true;
    
    private void Start()
    {
        material = GetComponent<Renderer>().material;
        currentFeedings = maxFeedings;
        gameObject.name = "AdvancedFoodSource";
        UpdateVisuals();
        
        Debug.Log($"[AdvancedFoodSource] Food source ready with {maxFeedings} feedings");
    }
    
    private void Update()
    {
        // Regenerate food over time
        if (!isActive && Time.time - lastFeedTime >= regenerateTime)
        {
            currentFeedings = maxFeedings;
            isActive = true;
            UpdateVisuals();
            Debug.Log("[AdvancedFoodSource] Food regenerated!");
        }
        
        // Auto-feed nearby creatures
        if (isActive)
        {
            AutoFeedNearbyCreatures();
        }
    }
    
    private void AutoFeedNearbyCreatures()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, feedRadius);
        
        foreach (Collider col in nearbyColliders)
        {
            AdvancedMinipollCore creature = col.GetComponent<AdvancedMinipollCore>();
            if (creature != null && creature.IsAlive && creature.hunger < 80f)
            {
                // Check if creature is close enough and looking for food
                float distance = Vector3.Distance(transform.position, creature.transform.position);
                if (distance < 1.5f && creature.CurrentAction.Contains("Food"))
                {
                    FeedCreature(creature);
                    break; // Feed one at a time
                }
            }
        }
    }
    
    private void FeedCreature(AdvancedMinipollCore creature)
    {
        if (!isActive || currentFeedings <= 0) return;
        
        creature.Feed(nutritionValue);
        currentFeedings--;
        lastFeedTime = Time.time;
        
        Debug.Log($"[AdvancedFoodSource] Fed {creature.name}! Feedings left: {currentFeedings}");
        
        // Visual feedback
        StartCoroutine(FlashFeedback());
        
        // Check if depleted
        if (currentFeedings <= 0)
        {
            isActive = false;
            UpdateVisuals();
            Debug.Log("[AdvancedFoodSource] Food source depleted! Regenerating...");
        }
        else
        {
            UpdateVisuals();
        }
    }
    
    private void UpdateVisuals()
    {
        if (material == null) return;
        
        if (!isActive)
        {
            material.color = Color.gray; // Depleted
        }
        else
        {
            float intensity = (float)currentFeedings / maxFeedings;
            material.color = Color.Lerp(Color.yellow, Color.green, intensity);
        }
    }
    
    private System.Collections.IEnumerator FlashFeedback()
    {
        Color originalColor = material.color;
        
        // Flash white
        material.color = Color.white;
        yield return new WaitForSeconds(0.15f);
        
        // Back to original
        material.color = originalColor;
    }
    
    // Manual feeding for testing
    private void OnMouseDown()
    {
        Debug.Log("[AdvancedFoodSource] Manual feed triggered");
        
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, feedRadius);
        foreach (Collider col in nearbyColliders)
        {
            AdvancedMinipollCore creature = col.GetComponent<AdvancedMinipollCore>();
            if (creature != null && creature.IsAlive)
            {
                FeedCreature(creature);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw feed radius
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, feedRadius);
        
        // Draw nutrition info
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f,
            $"Food: {currentFeedings}/{maxFeedings}\nActive: {isActive}\nNutrition: {nutritionValue}");
#endif
    }
}
