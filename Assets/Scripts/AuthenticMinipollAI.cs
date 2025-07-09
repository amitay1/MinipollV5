using UnityEngine;
using UnityEngine.AI;
using NEEDSIM;

public class AuthenticMinipollAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float wanderRadius = 10f;
    public float wanderTimer = 3f;
    public float interactionDistance = 2f;
    public float runSpeed = 3.5f;
    public float walkSpeed = 1.5f;
    
    [Header("Animation Clips")]
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip runClip;
    public AnimationClip sleepClip;
    public AnimationClip eatClip;
    
    private NavMeshAgent agent;
    private Animation animationComponent;
    private NEEDSIMNode needsimNode;
    private Vector3 homePosition;
    private float timer;
    private string currentAnimation = "";
    private bool isInteracting = false;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animationComponent = GetComponent<Animation>();
        needsimNode = GetComponent<NEEDSIMNode>();
        
        if (animationComponent == null)
        {
            animationComponent = gameObject.AddComponent<Animation>();
        }
        
        homePosition = transform.position;
        timer = wanderTimer;
        
        // טען אנימציות
        LoadAnimations();
        
        // הגדרות NavMeshAgent
        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;
        }
        
        Debug.Log("AuthenticMinipollAI: המיניפול האמיתי מוכן לפעולה!");
        
        // התחל עם אנימציית Idle
        PlayAnimation("Idle");
    }
    
    void LoadAnimations()
    {
        // טען את כל האנימציות מתיקיית biped
        LoadAnimationFromAsset("Animation_Idle_frame_rate_60", "Idle");
        LoadAnimationFromAsset("Animation_Walking_frame_rate_60", "Walk");
        LoadAnimationFromAsset("Animation_Running_frame_rate_60", "Run");
        LoadAnimationFromAsset("Animation_Sleep_Normally_frame_rate_60", "Sleep");
        LoadAnimationFromAsset("Animation_Collect_Object_frame_rate_60", "Eat");
    }
    
    void LoadAnimationFromAsset(string assetName, string clipName)
    {
        // טען אנימציה מתיקיית biped
        GameObject animObject = Resources.Load<GameObject>($"biped/{assetName}");
        if (animObject != null)
        {
            Animation anim = animObject.GetComponent<Animation>();
            if (anim != null && anim.clip != null)
            {
                animationComponent.AddClip(anim.clip, clipName);
                Debug.Log($"נטענה אנימציה: {clipName} מ-{assetName}");
            }
        }
        else
        {
            Debug.LogWarning($"לא נמצאה אנימציה: {assetName}");
        }
    }
    
    void Update()
    {
        if (agent == null || isInteracting) return;
        
        timer += Time.deltaTime;
        
        // בדוק אם צריך לבחור יעד חדש
        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(homePosition, wanderRadius);
            if (newPos != Vector3.zero)
            {
                agent.SetDestination(newPos);
                timer = 0f;
            }
        }
        
        // בדוק אינטראקציות
        CheckForInteractions();
        
        // עדכן אנימציות
        UpdateAnimations();
    }
    
    void CheckForInteractions()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, interactionDistance);
        
        foreach (Collider col in nearbyObjects)
        {
            if (col.gameObject != gameObject)
            {
                string objectName = col.gameObject.name.ToLower();
                
                if (objectName.Contains("water") || objectName.Contains("drink"))
                {
                    StartCoroutine(InteractWithObject("Drink", col.transform));
                    return;
                }
                else if (objectName.Contains("food") || objectName.Contains("eat"))
                {
                    StartCoroutine(InteractWithObject("EatFern", col.transform));
                    return;
                }
                else if (objectName.Contains("sleep") || objectName.Contains("bed"))
                {
                    StartCoroutine(InteractWithObject("SleepInRabbitHole", col.transform));
                    return;
                }
            }
        }
    }
    
    System.Collections.IEnumerator InteractWithObject(string interactionType, Transform target)
    {
        if (isInteracting) yield break;
        
        isInteracting = true;
        agent.SetDestination(target.position);
        
        // חכה עד שמגיע לאובייקט
        while (Vector3.Distance(transform.position, target.position) > agent.stoppingDistance + 0.5f)
        {
            yield return null;
        }
        
        agent.isStopped = true;
        
        // פנה לכיוון האובייקט
        Vector3 lookDirection = (target.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        
        // נגן אנימציה מתאימה
        if (interactionType == "EatFern")
        {
            PlayAnimation("Eat");
        }
        else if (interactionType == "SleepInRabbitHole")
        {
            PlayAnimation("Sleep");
        }
        else
        {
            PlayAnimation("Idle");
        }
        
        // בצע אינטראקציה עם NEEDSIM - כרגע רק מדמה
        Debug.Log($"מיניפול מבצע: {interactionType} (עם {target.name})");
        
        // חכה מעט לפני חזרה לפעילות רגילה
        yield return new WaitForSeconds(2f);
        
        agent.isStopped = false;
        isInteracting = false;
        PlayAnimation("Idle");
    }
    
    void UpdateAnimations()
    {
        if (animationComponent == null || agent == null) return;
        
        float speed = agent.velocity.magnitude;
        string targetAnimation = "";
        
        if (isInteracting)
        {
            return; // שמור על האנימציה הנוכחית בזמן אינטראקציה
        }
        
        if (speed > 2.5f)
        {
            targetAnimation = "Run";
            agent.speed = runSpeed;
        }
        else if (speed > 0.1f)
        {
            targetAnimation = "Walk";
            agent.speed = walkSpeed;
        }
        else
        {
            targetAnimation = "Idle";
        }
        
        if (targetAnimation != currentAnimation)
        {
            PlayAnimation(targetAnimation);
        }
    }
    
    void PlayAnimation(string animationName)
    {
        if (animationComponent != null && currentAnimation != animationName)
        {
            if (animationComponent[animationName] != null)
            {
                animationComponent.CrossFade(animationName, 0.3f);
                currentAnimation = animationName;
                Debug.Log($"מנגן אנימציה: {animationName}");
            }
            else
            {
                Debug.LogWarning($"אנימציה לא נמצאה: {animationName}");
            }
        }
    }
    
    public static Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randDirection, out navHit, dist, -1))
        {
            return navHit.position;
        }
        
        return Vector3.zero;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(homePosition, wanderRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
