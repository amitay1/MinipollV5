using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// בדיקה מהירה לוודא שהסנכרון NavMeshAgent + Animator עובד
/// </summary>
public class QuickMovementTest : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        if (agent != null && animator != null)
        {
            Debug.Log($"✅ [QuickTest] רכיבים נמצאו על {gameObject.name}");
            
            // תתחיל לזוז כעבור שנייה
            Invoke(nameof(StartMovementTest), 1f);
        }
        else
        {
            Debug.LogError($"❌ [QuickTest] חסרים רכיבים: Agent={agent != null}, Animator={animator != null}");
        }
    }
    
    void StartMovementTest()
    {
        // זוז לנקודה אקראית
        Vector3 randomTarget = transform.position + new Vector3(
            Random.Range(-5f, 5f),
            0f,
            Random.Range(-5f, 5f)
        );
        
        agent.SetDestination(randomTarget);
        Debug.Log($"🎯 [QuickTest] {gameObject.name} נע לעבר {randomTarget}");
    }
    
    void Update()
    {
        if (agent != null && animator != null)
        {
            // סנכרון פשוט - עיקרון הזהב!
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
            
            // לוג רק כשיש תנועה
            if (speed > 0.1f)
            {
                Debug.Log($"🏃 [QuickTest] {gameObject.name} מהירות: {speed:F2}");
            }
        }
    }
    
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 200, 300, 100));
        GUILayout.Label($"🧪 בדיקת תנועה: {gameObject.name}");
        
        if (agent != null)
        {
            GUILayout.Label($"מהירות: {agent.velocity.magnitude:F2}");
            GUILayout.Label($"יעד: {(agent.hasPath ? "✅" : "❌")}");
        }
        
        if (GUILayout.Button("🎯 זוז לנקודה אקראית"))
        {
            StartMovementTest();
        }
        
        GUILayout.EndArea();
    }
}
