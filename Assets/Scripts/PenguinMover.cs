using UnityEngine;

public class PenguinMover : MonoBehaviour
{
    public float speed = 2f;
    public float range = 5f;
    
    private Vector3 targetPosition;
    private float waitTime = 0f;
    
    void Start()
    {
        Debug.Log("PenguinMover started!");
        ChooseNewTarget();
    }
    
    void Update()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            return;
        }
        
        // זוז לכיוון המטרה
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // אם הגענו למטרה, בחר מטרה חדשה
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            ChooseNewTarget();
            waitTime = Random.Range(1f, 3f);
        }
    }
    
    void ChooseNewTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * range;
        randomDirection.y = 0.5f; // שמור על גובה קבוע
        targetPosition = transform.position + randomDirection;
        
        Debug.Log($"New target: {targetPosition}");
    }
}
