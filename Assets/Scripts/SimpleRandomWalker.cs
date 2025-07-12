using UnityEngine;
using UnityEngine.AI;
using MinipollGame.Core;
using MinipollGame.Controllers;
using MinipollGame.Systems.Core;

/// <summary>
/// SimpleRandomWalker Enhanced - אינטגרציה חכמה עם MinipollCore
/// תומך בכל המערכות הקיימות שלך והופך לחלק מהמערכת
/// </summary>
public class SimpleRandomWalker : MonoBehaviour
{
    [Header("🎯 Random Walking Settings")]
    public float walkRadius = 10f;
    public float waitTime = 2f;
    public float energyDrain = 0.5f; // כמה אנרגיה הליכה צורכת
    
    [Header("🤖 Smart Behavior")]
    public bool useMinipollIntegration = true; // האם להשתמש באינטגרציה חכמה
    public float restWhenEnergyBelow = 20f; // מתי להפסיק לטייל
    public bool respectNeedsSystem = true; // האם לכבד מערכת הצרכים
    
    // Enums
    public enum WalkingState { Idle, Walking, Resting, Seeking }
    
    // Core Components
    private NavMeshAgent agent;
    private CharacterMovementSync movementSync;
    private MinipollGame.Core.MinipollCore minipollCore;
    private MinipollMovementController movementController;
    private MinipollGame.Systems.Core.MinipollNeedsSystem needsSystem;
    
    // State
    private Vector3 startPosition;
    private float timer;
    private bool isResting = false;
    private WalkingState currentState = WalkingState.Idle;
    
    void Start()
    {
        // אתחול רכיבים בסיסיים
        InitializeBasicComponents();
        
        // אתחול אינטגרציה חכמה (אם פעיל)
        if (useMinipollIntegration)
        {
            InitializeMinipollIntegration();
        }
        
        // התחלת התנהגות
        startPosition = transform.position;
        if (agent != null)
        {
            agent.speed = 3.5f;
            // Debug logging removed to reduce console spam
            
            // התחל בהתנהגות חכמה
            StartSmartWalking();
        }
        else
        {
            Debug.LogError("SimpleRandomWalker: לא נמצא NavMeshAgent!");
        }
    }
    
    /// <summary>
    /// אתחול רכיבים בסיסיים
    /// </summary>
    private void InitializeBasicComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        movementSync = GetComponent<CharacterMovementSync>();
        movementController = GetComponent<MinipollMovementController>();
    }
    
    /// <summary>
    /// אתחול אינטגרציה עם מערכות MinipollCore
    /// </summary>
    private void InitializeMinipollIntegration()
    {
        minipollCore = GetComponent<MinipollGame.Core.MinipollCore>();
        needsSystem = GetComponent<MinipollGame.Systems.Core.MinipollNeedsSystem>();
        
        if (minipollCore != null)
        {
            // Debug logging removed to reduce console spam
        }
        
        if (needsSystem != null)
        {
            // Debug logging removed to reduce console spam
        }
    }
    
    /// <summary>
    /// התחלת הליכה חכמה
    /// </summary>
    private void StartSmartWalking()
    {
        if (ShouldWalk())
        {
            ChooseRandomDestination();
            currentState = WalkingState.Walking;
        }
        else
        {
            StartResting();
        }
    }
    
    void Update()
    {
        if (agent == null) return;
        
        // עדכון התנהגות על פי מצב
        switch (currentState)
        {
            case WalkingState.Idle:
                HandleIdle();
                break;
            case WalkingState.Walking:
                HandleWalking();
                break;
            case WalkingState.Resting:
                HandleResting();
                break;
            case WalkingState.Seeking:
                HandleSeeking();
                break;
        }
        
        // צריכת אנרגיה בזמן הליכה
        if (currentState == WalkingState.Walking && agent.velocity.magnitude > 0.1f)
        {
            ConsumeEnergy();
        }
    }
    
    /// <summary>
    /// טיפול במצב Idle
    /// </summary>
    private void HandleIdle()
    {
        timer += Time.deltaTime;
        
        if (timer >= waitTime)
        {
            if (ShouldWalk())
            {
                ChooseRandomDestination();
                currentState = WalkingState.Walking;
            }
            else if (ShouldRest())
            {
                StartResting();
            }
            timer = 0f;
        }
    }
    
    /// <summary>
    /// טיפול במצב Walking
    /// </summary>
    private void HandleWalking()
    {
        // בדיקה אם הגענו ליעד או שאין לנו יעד
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            timer += Time.deltaTime;
            
            if (timer >= waitTime)
            {
                // החלט מה לעשות הלאה
                if (ShouldContinueWalking())
                {
                    ChooseRandomDestination();
                }
                else
                {
                    currentState = WalkingState.Idle;
                }
                timer = 0f;
            }
        }
    }
    
    /// <summary>
    /// טיפול במצב Resting
    /// </summary>
    private void HandleResting()
    {
        timer += Time.deltaTime;
        
        // מנוחה מחזירה אנרגיה
        if (minipollCore != null && timer > 1f)
        {
            minipollCore.Heal(1f); // מחזיר קצת בריאות/אנרגיה
            timer = 0f;
        }
        
        // בדיקה אם סיימנו לנוח
        if (CanStopResting())
        {
            StopResting();
        }
    }
    
    /// <summary>
    /// טיפול במצב Seeking (חיפוש משאבים)
    /// </summary>
    private void HandleSeeking()
    {
        // אם הגענו ליעד - חזור להליכה רגילה
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            currentState = WalkingState.Idle;
            // Debug logging removed to reduce console spam
        }
    }
    
    void ChooseRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += startPosition;
        randomDirection.y = startPosition.y; // שמור על גובה קבוע
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1))
        {
            // השתמש במערכת המתקדמת אם זמינה
            if (movementController != null && useMinipollIntegration)
            {
                movementController.MoveTo(hit.position);
                // Debug logging removed to reduce console spam
            }
            else if (movementSync != null)
            {
                movementSync.MoveTo(hit.position);
                // Debug logging removed to reduce console spam
            }
            else if (agent != null)
            {
                agent.SetDestination(hit.position);
                // Debug logging removed to reduce console spam
            }
        }
        else
        {
            // Debug logging removed to reduce console spam
        }
    }
    
    #region Smart Decision Making
    
    /// <summary>
    /// האם כדאי ללכת עכשיו?
    /// </summary>
    private bool ShouldWalk()
    {
        if (!useMinipollIntegration) return true;
        
        // בדיקת אנרגיה (גרסה פשוטה)
        if (minipollCore != null && minipollCore.Health != null)
        {
            float healthPercent = minipollCore.Health.CurrentHealth / minipollCore.Health.MaxHealth;
            if (healthPercent < 0.5f)
            {
                Debug.Log($"💤 {gameObject.name} עייף מדי ללכת (בריאות: {healthPercent:P})");
                return false;
            }
        }
        
        // בדיקת בריאות
        if (minipollCore != null && minipollCore.Health != null)
        {
            float healthPercent = minipollCore.Health.CurrentHealth / minipollCore.Health.MaxHealth;
            if (healthPercent < 0.3f)
            {
                Debug.Log($"❤️ {gameObject.name} חלש מדי ללכת");
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// האם צריך לנוח?
    /// </summary>
    private bool ShouldRest()
    {
        if (!useMinipollIntegration || minipollCore == null) return false;
        
        if (minipollCore.Health != null)
        {
            float healthPercent = minipollCore.Health.CurrentHealth / minipollCore.Health.MaxHealth;
            return healthPercent < 0.3f;
        }
        
        return false;
    }
    
    /// <summary>
    /// האם להמשיך ללכת?
    /// </summary>
    private bool ShouldContinueWalking()
    {
        if (!respectNeedsSystem) return Random.value > 0.3f; // 70% סיכוי להמשיך
        
        return ShouldWalk() && Random.value > 0.2f; // 80% סיכוי אם יש אנרגיה
    }
    
    /// <summary>
    /// האם אפשר להפסיק לנוח?
    /// </summary>
    private bool CanStopResting()
    {
        if (!useMinipollIntegration || minipollCore == null) return true;
        
        if (minipollCore.Health != null)
        {
            float healthPercent = minipollCore.Health.CurrentHealth / minipollCore.Health.MaxHealth;
            return healthPercent > 0.6f; // רק אם יש מספיק בריאות
        }
        
        return true;
    }
    
    /// <summary>
    /// התחלת מנוחה
    /// </summary>
    private void StartResting()
    {
        currentState = WalkingState.Resting;
        isResting = true;
        
        // עצור תנועה
        if (agent != null)
            agent.ResetPath();
            
        // Debug logging removed to reduce console spam
    }
    
    /// <summary>
    /// סיום מנוחה
    /// </summary>
    private void StopResting()
    {
        currentState = WalkingState.Idle;
        isResting = false;
        timer = 0f;
        
        // Debug logging removed to reduce console spam
    }
    
    /// <summary>
    /// צריכת אנרגיה
    /// </summary>
    private void ConsumeEnergy()
    {
        if (!useMinipollIntegration || minipollCore == null) return;
        
        float energyToConsume = energyDrain * Time.deltaTime;
        minipollCore.ConsumeEnergy(energyToConsume);
    }
    
    #endregion
    
    #region Public Interface
    
    /// <summary>
    /// חיפוש משאב ספציפי
    /// </summary>
    public void SeekResource(Vector3 resourcePosition)
    {
        currentState = WalkingState.Seeking;
        
        if (movementController != null)
        {
            movementController.MoveTo(resourcePosition);
        }
        else if (agent != null)
        {
            agent.SetDestination(resourcePosition);
        }
        
        // Debug logging removed to reduce console spam
    }
    
    /// <summary>
    /// עצירת הליכה חירום
    /// </summary>
    public void StopWalking()
    {
        currentState = WalkingState.Idle;
        if (agent != null)
            agent.ResetPath();
            
        // Debug logging removed to reduce console spam
    }
    
    /// <summary>
    /// קבלת מצב נוכחי
    /// </summary>
    public string GetCurrentStatus()
    {
        string status = $"מצב: {currentState}";
        
        if (minipollCore != null && minipollCore.Health != null)
        {
            float healthPercent = minipollCore.Health.CurrentHealth / minipollCore.Health.MaxHealth;
            status += $", בריאות: {healthPercent:P}";
        }
        
        return status;
    }    
    #endregion

    void OnDrawGizmosSelected()
    {
        // ציור רדיוס ההליכה
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, walkRadius);
        
        // ציור מצב נוכחי
        if (currentState == WalkingState.Walking)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else if (currentState == WalkingState.Resting)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else if (currentState == WalkingState.Seeking)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        
        // הצגת מידע במצב Debug
        #if UNITY_EDITOR
        if (minipollCore != null)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
                $"{gameObject.name}\n{GetCurrentStatus()}");
        }
        #endif
    }
}
