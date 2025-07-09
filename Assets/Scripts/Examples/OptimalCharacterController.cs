using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// דוגמה מלאה לסנכרון נכון בין NavMeshAgent ו-Animator
/// זה המדריך הטוב ביותר לעבודה עם אנימציות ב-Unity!
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class OptimalCharacterController : MonoBehaviour
{
    [Header("📋 הגדרות תנועה")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    
    [Header("📋 הגדרות אנימציה")]
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float runThreshold = 3.5f;
    
    [Header("🎯 יעדים לבדיקה")]
    [SerializeField] private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    
    private NavMeshAgent agent;
    private Animator animator;
    
    void Start()
    {
        // אתחול הרכיבים
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // הגדרת מהירות NavMeshAgent
        agent.speed = walkSpeed;
        
        // ⚠️ חשוב: כבה את Apply Root Motion אם האנימציות כוללות תנועה
        animator.applyRootMotion = false;
        
        Debug.Log($"✅ [OptimalController] מוכן לפעולה: {gameObject.name}");
        
        // התחל תנועה ליעד ראשון (לבדיקה)
        if (waypoints.Length > 0)
        {
            MoveToNextWaypoint();
        }
    }
    
    void Update()
    {
        // 🔄 העיקרון הזהב: NavMeshAgent שולט בתנועה, Animator מסתנכרן עם המהירות
        UpdateAnimationBasedOnVelocity();
        
        // בדוק אם הגענו ליעד (לדוגמה)
        CheckIfReachedDestination();
    }
    
    /// <summary>
    /// 🎯 זוהי הפונקציה החשובה ביותר - סנכרון מושלם!
    /// </summary>
    void UpdateAnimationBasedOnVelocity()
    {
        // קבל את המהירות האמיתית מה-NavMeshAgent
        float currentSpeed = agent.velocity.magnitude;
        
        // עדכן את פרמטר Speed ב-Animator
        animator.SetFloat("Speed", currentSpeed);
        
        // Debug להבנה (ניתן למחוק בפרודקשן)
        if (currentSpeed > walkThreshold)
        {
            string state = currentSpeed > runThreshold ? "🏃 ריצה" : "🚶 הליכה";
            Debug.Log($"[Controller] {gameObject.name}: {state} במהירות {currentSpeed:F2}");
        }
    }
    
    /// <summary>
    /// 🎯 הזז את הדמות ליעד - פשוט ויעיל!
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
            Debug.Log($"🎯 [Controller] {gameObject.name} נע אל {destination}");
        }
    }
    
    /// <summary>
    /// 🏃 החלף למצב ריצה
    /// </summary>
    public void SetRunning(bool shouldRun)
    {
        agent.speed = shouldRun ? runSpeed : walkSpeed;
        Debug.Log($"⚡ [Controller] {gameObject.name} {(shouldRun ? "רץ" : "הולך")} במהירות {agent.speed}");
    }
    
    /// <summary>
    /// ⏹️ עצור תנועה
    /// </summary>
    public void Stop()
    {
        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
            Debug.Log($"⏹️ [Controller] {gameObject.name} נעצר");
        }
    }
    
    /// <summary>
    /// 🎯 דוגמה לתנועה בין נקודות (לבדיקה)
    /// </summary>
    void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        
        MoveTo(waypoints[currentWaypointIndex].position);
        
        // עבור לנקודה הבאה
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }
    
    void CheckIfReachedDestination()
    {
        if (agent.hasPath && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Debug.Log($"🎯 [Controller] {gameObject.name} הגיע ליעד!");
            
            // המתן שנייה ואז עבור לנקודה הבאה (לדוגמה)
            if (waypoints.Length > 0)
            {
                Invoke(nameof(MoveToNextWaypoint), 2f);
            }
        }
    }
    
    /// <summary>
    /// 🎮 פונקציות נוספות לשליטה
    /// </summary>
    public float GetCurrentSpeed() => agent.velocity.magnitude;
    public bool IsMoving() => GetCurrentSpeed() > walkThreshold;
    public bool IsRunning() => GetCurrentSpeed() > runThreshold;
    
    /// <summary>
    /// 🎯 הצג מידע חיוני ב-Inspector
    /// </summary>
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label($"🎮 {gameObject.name} - בקרת דמות");
        GUILayout.Label($"מהירות נוכחית: {GetCurrentSpeed():F2}");
        GUILayout.Label($"סטטוס: {(IsRunning() ? "🏃 רץ" : IsMoving() ? "🚶 הולך" : "⏸️ עומד")}");
        GUILayout.Label($"יעד: {(agent.hasPath ? "✅ יש" : "❌ אין")}");
        
        if (GUILayout.Button("🎯 תנועה אקראית"))
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 10f;
            randomPoint.y = transform.position.y;
            MoveTo(randomPoint);
        }
        
        if (GUILayout.Button(IsRunning() ? "🚶 עבור להליכה" : "🏃 עבור לריצה"))
        {
            SetRunning(!IsRunning());
        }
        
        if (GUILayout.Button("⏹️ עצור"))
        {
            Stop();
        }
        
        GUILayout.EndArea();
    }
}

/* 
📋 הנחיות Animator Controller:

1. צור פרמטר "Speed" מסוג Float
2. צור states: Idle, Walk, Run
3. צור transitions:
   - Idle → Walk: Speed > 0.1
   - Walk → Idle: Speed ≤ 0.1  
   - Walk → Run: Speed > 3.5
   - Run → Walk: Speed ≤ 3.5

⚠️ חשוב: כבה את "Apply Root Motion" ב-Animator!

✅ התוצאה: סנכרון מושלם בין תנועה ואנימציה!
*/
