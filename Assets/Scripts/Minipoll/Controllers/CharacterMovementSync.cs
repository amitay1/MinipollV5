using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CharacterMovementSync : MonoBehaviour
{
    [Header("Movement Sync Settings")]
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float runThreshold = 3.0f;
    
    private NavMeshAgent agent;
    private Animator animator;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // וודא שיש לנו את הפרמטרים הנדרשים ב-Animator Controller
        Debug.Log($"[CharacterMovement] Initialized for {gameObject.name}");
    }
    
    void Update()
    {
        // קבל את המהירות מה-NavMeshAgent
        float speed = agent.velocity.magnitude;
        
        // עדכן את ה-Animator עם המהירות הנוכחית
        animator.SetFloat("Speed", speed);
        
        // Debug info (optional)
        if (speed > walkThreshold)
        {
            Debug.Log($"[CharacterMovement] {gameObject.name} moving at speed: {speed:F2}");
        }
    }
    
    // פונקציה להזיז את הדמות ליעד מסוים
    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
            Debug.Log($"[CharacterMovement] {gameObject.name} moving to {destination}");
        }
    }
    
    // פונקציה לעצירת התנועה
    public void StopMovement()
    {
        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
            Debug.Log($"[CharacterMovement] {gameObject.name} stopped moving");
        }
    }
    
    // פונקציה לבדיקה אם הדמות הגיעה ליעד
    public bool HasReachedDestination()
    {
        if (agent != null && !agent.pathPending)
        {
            return agent.remainingDistance < 0.5f;
        }
        return false;
    }
    
    // פונקציה לקבלת המהירות הנוכחית
    public float GetCurrentSpeed()
    {
        return agent != null ? agent.velocity.magnitude : 0f;
    }
    
    // פונקציה לקביעת מהירות הבסיס של ה-Agent
    public void SetSpeed(float newSpeed)
    {
        if (agent != null)
        {
            agent.speed = newSpeed;
        }
    }
}
