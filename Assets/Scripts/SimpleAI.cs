using UnityEngine;
using UnityEngine.AI;

public class SimpleAI : MonoBehaviour
{
    public float moveRange = 5f;
    public float moveSpeed = 2f;
    
    private Vector3 targetPosition;
    private float timer;
    
    void Start()
    {
        Debug.Log("Simple AI started for " + gameObject.name);
        ChooseNewTarget();
    }
    
    void Update()
    {
        // תנועה פשוטה ללא NavMesh
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        timer += Time.deltaTime;
        
        // אם הגיע ליעד או עבר זמן מספיק
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f || timer > 3f)
        {
            ChooseNewTarget();
            timer = 0f;
        }
        
        // מסובב לכיוון התנועה
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // דיווח מדי פעם
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"Penguin moving to: {targetPosition}, Current: {transform.position}");
        }
    }
    
    void ChooseNewTarget()
    {
        // בוחר מיקום אקראי בטווח
        Vector3 randomDirection = new Vector3(
            Random.Range(-moveRange, moveRange),
            0.5f, // גובה קבועה
            Random.Range(-moveRange, moveRange)
        );
        
        targetPosition = randomDirection;
        Debug.Log($"New target chosen: {targetPosition}");
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, moveRange);
    }
}