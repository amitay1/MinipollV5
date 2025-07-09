using UnityEngine;
using UnityEngine.AI;

public class FreeRoamingAI : MonoBehaviour
{
    [Header("Free Roaming Settings")]
    public float roamingRange = 10f;
    public float waitTime = 2f;
    public float walkSpeed = 2f;
    
    private NavMeshAgent agent;
    private Vector3 startPosition;
    private float waitTimer;
    private bool isWaiting;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        
        if (agent != null)
        {
            agent.speed = walkSpeed;
            Debug.Log("Free roaming AI started for " + gameObject.name);
            ChooseNewDestination();
        }
        else
        {
            Debug.LogError("NavMeshAgent not found on " + gameObject.name);
        }
    }
    
    void Update()
    {
        if (agent == null) return;
        
        // אם הגיע ליעד או אם אין יעד
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = waitTime;
                Debug.Log("Penguin reached destination, waiting...");
            }
        }
        
        // ממתין ואז בוחר יעד חדש
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                ChooseNewDestination();
            }
        }
        
        // דיווח על מצב
        if (Time.frameCount % 120 == 0) // כל שתי שניות
        {
            Debug.Log($"Penguin AI Status: Position={transform.position}, HasPath={agent.hasPath}, IsWaiting={isWaiting}");
        }
    }
    
    void ChooseNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamingRange;
        randomDirection += startPosition;
        randomDirection.y = startPosition.y; // שומר על אותה גובה
        
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        
        // מחפש נקודה חוקית על ה-NavMesh
        if (NavMesh.SamplePosition(randomDirection, out hit, roamingRange, 1))
        {
            finalPosition = hit.position;
        }
        else
        {
            // אם לא מצא, נשאר במקום הנוכחי
            finalPosition = transform.position;
        }
        
        agent.SetDestination(finalPosition);
        Debug.Log($"Penguin moving to new destination: {finalPosition}");
    }
    
    void OnDrawGizmosSelected()
    {
        // מציג את טווח השיטוט בעורך
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPosition, roamingRange);
        
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, agent.destination);
        }
    }
}