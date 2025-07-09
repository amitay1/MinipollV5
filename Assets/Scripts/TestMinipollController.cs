using UnityEngine;

namespace MinipollGame.Test
{
    public class TestMinipollController : MonoBehaviour
{
    [Header("Basic Stats")]
    public string creatureName = "TestMinipoll";
    public float health = 100f;
    public float hunger = 50f;
    public float thirst = 50f;
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    
    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool hasTarget = false;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameObject.name = creatureName;
        
        // Apply a nice color
        GetComponent<MeshRenderer>().material.color = Color.cyan;
        
        Debug.Log($"[TestMinipollController] {creatureName} initialized!");
    }
    
    private void Update()
    {
        // Update needs over time
        hunger -= Time.deltaTime * 5f;
        thirst -= Time.deltaTime * 8f;
        
        hunger = Mathf.Max(0, hunger);
        thirst = Mathf.Max(0, thirst);
        
        // Check for critical needs
        if (hunger < 20f)
        {
            GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else if (thirst < 20f)
        {
            GetComponent<MeshRenderer>().material.color = Color.blue;
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.cyan;
        }
        
        // Simple movement towards target
        if (hasTarget && rb != null)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb.AddForce(direction * moveSpeed, ForceMode.Force);
            
            // Stop when close enough
            if (Vector3.Distance(transform.position, targetPosition) < 1f)
            {
                hasTarget = false;
                rb.linearVelocity = Vector3.zero;
            }
        }
        
        // Random movement if no target
        if (!hasTarget && Random.Range(0f, 1f) < 0.01f)
        {
            SetRandomTarget();
        }
    }
    
    private void SetRandomTarget()
    {
        targetPosition = new Vector3(
            Random.Range(-8f, 8f),
            1f,
            Random.Range(-8f, 8f)
        );
        hasTarget = true;
        
        Debug.Log($"[TestMinipollController] {creatureName} moving to {targetPosition}");
    }
    
    public void Feed(float amount)
    {
        hunger = Mathf.Min(100f, hunger + amount);
        Debug.Log($"[TestMinipollController] {creatureName} fed! Hunger: {hunger}");
    }
    
    public void GiveWater(float amount)
    {
        thirst = Mathf.Min(100f, thirst + amount);
        Debug.Log($"[TestMinipollController] {creatureName} drank! Thirst: {thirst}");
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw stats above creature
        Vector3 pos = transform.position + Vector3.up * 3f;
        
        // Health bar (red)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector3.right * (health / 100f) * 2f);
        
        // Hunger bar (orange)
        pos += Vector3.up * 0.3f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + Vector3.right * (hunger / 100f) * 2f);
        
        // Thirst bar (blue)
        pos += Vector3.up * 0.3f;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + Vector3.right * (thirst / 100f) * 2f);
        
        // Target position
        if (hasTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
}
