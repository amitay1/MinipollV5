using UnityEngine;
using UnityEngine.AI;

public class PenguinAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float roamingRange = 8f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    
    [Header("Interaction Settings")]
    public float interactionDistance = 2f;
    public LayerMask interactionLayers = -1;
    
    private NavMeshAgent agent;
    private Animator animator;
    private MinipollAnimationController animationController;
    private Vector3 targetPosition;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isInteracting = false;
    
    // מקומות המוצא
    private Vector3 spawnPoint;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // יהיה null אם אין Animator
        animationController = GetComponent<MinipollAnimationController>();
        
        // וודא שהמיניפול נמצא בגובה בטוח מעל הקרקע
        if (transform.position.y < 1.5f)
        {
            transform.position = new Vector3(transform.position.x, 2.0f, transform.position.z);
            Debug.Log("PenguinAI: תוקן מיקום המיניפול מעל הקרקע ל-Y=2.0");
        }
        
        spawnPoint = transform.position;
        
        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;
            
            Debug.Log("PenguinAI: איתחול הושלם בהצלחה!");
            ChooseNewDestination();
        }
        else
        {
            Debug.LogError("PenguinAI: לא נמצא NavMeshAgent!");
        }
    }
    
    void Update()
    {
        if (agent == null) return;
        
        // בדיקה מתמשכת שהמיניפול לא יירד מתחת לרצפה
        if (transform.position.y < 1.5f)
        {
            Vector3 fixedPos = transform.position;
            fixedPos.y = 2.5f;
            transform.position = fixedPos;
            Debug.LogWarning("PenguinAI: תוקן מיקום המיניפול במהלך Update - חזר ל-Y=2.5 מ-Y=" + transform.position.y.ToString("F3"));
        }
        
        // טיפול בזמן המתנה
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                agent.isStopped = false; // אפשר תנועה מחדש
                ChooseNewDestination();
            }
            return;
        }
        
        // בדיקה אם הגענו למטרה
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isInteracting)
            {
                // בדוק אם יש אובייקט אינטראקציה קרוב
                CheckForInteractions();
                
                // התחל המתנה לפני בחירת מטרה חדשה
                StartWaiting();
            }
        }
        
        // עדכון אנימציות
        UpdateAnimations();
    }
    
    void ChooseNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamingRange;
        randomDirection += spawnPoint;
        randomDirection.y = spawnPoint.y; // שמור על גובה קבוע
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamingRange, 1))
        {
            targetPosition = hit.position;
            // וודא שהמטרה לא תהיה מתחת לקרקע
            if (targetPosition.y < 1.5f)
            {
                targetPosition.y = 2.0f;
            }
            
            agent.SetDestination(targetPosition);
            agent.isStopped = false; // וודא שהסוכן יכול לזוז
            
            // שנה מהירות באופן אקראי
            agent.speed = Random.Range(walkSpeed, walkSpeed * 1.5f);
            
            Debug.Log($"PenguinAI: מטרה חדשה - {targetPosition}");
        }
        else
        {
            // אם לא מצאנו מקום חוקי, חזור למרכז
            Vector3 safeSpawn = spawnPoint;
            if (safeSpawn.y < 1.5f) safeSpawn.y = 2.0f;
            
            agent.SetDestination(safeSpawn);
            agent.isStopped = false;
            Debug.Log("PenguinAI: חוזר למרכז");
        }
    }
    
    void CheckForInteractions()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, interactionDistance, interactionLayers);
        
        foreach (Collider col in nearbyObjects)
        {
            if (col.gameObject.name.Contains("Food_Source") || 
                col.gameObject.name.Contains("Water_Source") || 
                col.gameObject.name.Contains("Sleep_Area"))
            {
                StartCoroutine(InteractWithObject(col.gameObject));
                return;
            }
        }
    }
    
    System.Collections.IEnumerator InteractWithObject(GameObject interactionObject)
    {
        isInteracting = true;
        agent.isStopped = true;
        
        // פנה לכיוון האובייקט
        Vector3 lookDirection = (interactionObject.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookDirection);
        
        // נגן אנימציית אינטראקציה אם קיימת
        if (animationController != null)
        {
            if (interactionObject.name.Contains("Sleep"))
            {
                animationController.PlaySleepAnimation();
                if (animator != null) animator.SetBool("IsSleeping", true);
            }
            else if (interactionObject.name.Contains("Food"))
            {
                animationController.PlayEatAnimation();
                if (animator != null) animator.SetBool("IsEating", true);
            }
            else if (interactionObject.name.Contains("Water"))
            {
                animationController.PlayEatAnimation(); // אין אנימציית שתייה נפרדת, נשתמש באכילה
                if (animator != null) animator.SetBool("IsEating", true);
            }
        }
        
        Debug.Log($"PenguinAI: מתקשר עם {interactionObject.name}");
        
        // החזק אינטראקציה למשך זמן
        yield return new WaitForSeconds(Random.Range(2f, 4f));
        
        // סיים את האינטראקציה
        if (animationController != null)
        {
            animationController.ReturnToNormalState();
        }
        
        if (animator != null)
        {
            animator.SetBool("IsSleeping", false);
            animator.SetBool("IsEating", false);
        }
        
        isInteracting = false;
        agent.isStopped = false;
        
        Debug.Log($"PenguinAI: סיים אינטראקציה עם {interactionObject.name}");
    }
    
    void StartWaiting()
    {
        isWaiting = true;
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
        agent.isStopped = true;
        
        // סיכוי קטן לאנימציה מיוחדת בזמן המתנה
        if (Random.Range(0f, 1f) < 0.3f && animationController != null)
        {
            // בחר אנימציה אקראית למתנה
            int randomAnim = Random.Range(0, 4);
            switch (randomAnim)
            {
                case 0:
                    if (animator != null) animator.SetTrigger("Idle_A");
                    break;
                case 1:
                    if (animator != null) animator.SetTrigger("Idle_B");
                    break;
                case 2:
                    if (animator != null) animator.SetTrigger("Idle_C");
                    break;
                case 3:
                    // אנימציית הסתכלות מסביב
                    break;
            }
        }
        
        Debug.Log($"PenguinAI: ממתין {waitTimer:F1} שניות");
    }
    
    // פונקציות עזר למצבים מיוחדים
    public void TriggerFearResponse()
    {
        if (animationController != null)
        {
            animationController.PlayFearAnimation();
        }
        if (animator != null)
        {
            animator.SetTrigger("Fear");
            animator.SetBool("IsAfraid", true);
        }
        
        // ברח למקום אקראי
        Vector3 fleeDirection = Random.insideUnitSphere * roamingRange;
        fleeDirection += spawnPoint;
        fleeDirection.y = spawnPoint.y;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeDirection, out hit, roamingRange, 1))
        {
            agent.SetDestination(hit.position);
            agent.speed = runSpeed; // רוץ מהר
        }
    }
    
    public void TriggerAttackResponse()
    {
        if (animationController != null)
        {
            animationController.PlayAttackAnimation();
        }
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    
    public void TriggerJump()
    {
        if (animationController != null)
        {
            animationController.PlayJumpAnimation();
        }
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
    }
    
    void UpdateAnimations()
    {
        if (animationController != null)
        {
            float speed = agent.velocity.magnitude;
            bool isMoving = speed > 0.1f;
            bool isRunning = speed > 3f;
            bool isWalking = speed > 0.1f && speed <= 3f;
            
            // תן ל-MinipollAnimationController לטפל באנימציות
            // הוא יבחר בין Animator ו-Animation Component לפי מה שזמין
            
            // עכשיו נקרא לפונקציות האנימציה בצורה נכונה
            if (isMoving)
            {
                if (isRunning)
                {
                    // אם רץ, בואו נוודא שהוא מנגן את אנימציית הריצה
                    Debug.Log("PenguinAI: רץ במהירות " + speed.ToString("F1"));
                    // אנימציית הריצה תופעל אוטומטית ב-UpdateLegacyAnimations
                }
                else
                {
                    // הליכה רגילה
                    Debug.Log("PenguinAI: הולך במהירות " + speed.ToString("F1"));
                    // אנימציית ההליכה תופעל אוטומטית ב-UpdateLegacyAnimations
                }
            }
            else
            {
                // עומד במקום
                if (isWaiting)
                {
                    // אנימציית המתנה - זה יופעל אוטומטית כשהמהירות 0
                    // animationController.PlayIdleAnimation(); // לא צריך - זה יקרה אוטומטית
                }
            }
            
            // עכשיו בואו נוסיף קריאה ישירה לפונקציות האנימציה כשצריך
            if (animator != null)
            {
                // אם יש Animator, נעדכן את הפרמטרים
                animator.SetFloat("Speed", speed);
                animator.SetBool("IsMoving", isMoving);
                animator.SetBool("IsWalking", isWalking);
                animator.SetBool("IsRunning", isRunning);
            }
        }
    }
    
    void LateUpdate()
    {
        // בדיקה אגרסיבית ב-LateUpdate לוודא שהמיניפול לא יירד
        if (transform.position.y < 1.5f)
        {
            float currentY = transform.position.y;
            transform.position = new Vector3(transform.position.x, 2.5f, transform.position.z);
            Debug.LogWarning($"PenguinAI LateUpdate: תוקן מיקום מ-Y={currentY:F3} ל-Y=2.5");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // הצג את טווח הנדידה
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnPoint, roamingRange);
        
        // הצג את טווח האינטראקציה
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        // הצג את המטרה הנוכחית
        if (targetPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.3f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
