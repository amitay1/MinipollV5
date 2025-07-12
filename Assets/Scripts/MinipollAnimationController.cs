using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MinipollAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip eatAnimation;
    public AnimationClip sleepAnimation;
    
    [Header("Animation Paths - Biped Assets")]
    public string idleAnimationPath = "Assets/biped/Animation_Idle_frame_rate_60.fbx";
    public string walkAnimationPath = "Assets/biped/Animation_Walk_Slowly_and_Look_Around_frame_rate_60.fbx";
    public string runAnimationPath = "Assets/biped/Animation_Running_frame_rate_60.fbx";
    public string sleepAnimationPath = "Assets/biped/Animation_Sleep_Normally_frame_rate_60.fbx";
    
    private Animator animator;
    private Animation animationComponent;
    private UnityEngine.AI.NavMeshAgent agent;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        animationComponent = GetComponent<Animation>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        
        // נעדיף לעבוד עם Animation Component במקום Animator
        if (animationComponent == null)
        {
            animationComponent = gameObject.AddComponent<Animation>();
        }
        
        // טען אנימציות מתיקיית biped או צור אנימציות בסיסיות
        LoadAnimationsFromBiped();
        
        Debug.Log("MinipollAnimationController: מוכן לפעולה! GameObject: " + gameObject.name);
    }
    
    void SetupAnimatorParameters()
    {
        // הפרמטרים שנדרשים ל-Animator Controller
        if (animator != null)
        {
            // וודא שיש פרמטרים מתאימים
            Debug.Log("MinipollAnimationController: עובד עם Animator Controller");
        }
    }
    
    void LoadAnimationsFromBiped()
    {
        // וודא שיש Animation component
        if (animationComponent == null)
        {
            animationComponent = gameObject.AddComponent<Animation>();
        }
        
#if UNITY_EDITOR
        // טען אנימציות מתיקיית biped באמצעות AssetDatabase
        try 
        {
            // טען Idle Animation
            if (idleAnimation == null)
            {
                LoadSingleAnimationFromAsset(idleAnimationPath, "Idle", ref idleAnimation);
            }
            
            // טען Walk Animation
            if (walkAnimation == null)
            {
                LoadSingleAnimationFromAsset(walkAnimationPath, "Walk", ref walkAnimation);
            }
            
            // טען Run Animation
            if (runAnimation == null)
            {
                LoadSingleAnimationFromAsset(runAnimationPath, "Run", ref runAnimation);
            }
            
            // טען Sleep Animation
            if (sleepAnimation == null)
            {
                LoadSingleAnimationFromAsset(sleepAnimationPath, "Sleep", ref sleepAnimation);
            }
            
            // הגדר אנימציית ברירת מחדל
            if (idleAnimation != null && animationComponent.GetClip("Idle") != null)
            {
                animationComponent.clip = idleAnimation;
                animationComponent.Play("Idle");
                Debug.Log("MinipollAnimationController: Started playing Idle animation");
            }
            else
            {
                Debug.LogWarning("MinipollAnimationController: Could not load Idle animation - creating fallback");
                CreateBasicAnimations();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("MinipollAnimationController: Error loading animations: " + e.Message);
            CreateBasicAnimations();
        }
#else
        // בזמן ריצה - ניצור אנימציות בסיסיות
        CreateBasicAnimations();
#endif
    }
    
#if UNITY_EDITOR
    void LoadSingleAnimationFromAsset(string assetPath, string clipName, ref AnimationClip targetClip)
    {
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        
        foreach (Object asset in allAssets)
        {
            if (asset is AnimationClip clip)
            {
                targetClip = clip;
                animationComponent.AddClip(targetClip, clipName);
                Debug.Log($"MinipollAnimationController: Loaded {clipName} animation successfully");
                return;
            }
        }
        Debug.LogWarning($"MinipollAnimationController: Could not find AnimationClip in {assetPath} for {clipName}");
    }
#endif
    
    void CreateBasicAnimations()
    {
        Debug.Log("MinipollAnimationController: Creating basic working animations...");
        
        // אם לא הצלחנו לטעון אנימציות, ניצור אנימציות פשוטות שעובדות
        if (animationComponent == null)
        {
            animationComponent = gameObject.AddComponent<Animation>();
        }
        
        // יצירת אנימציית Idle פשוטה שעובדת
        AnimationClip idleClip = CreateBasicIdleAnimation();
        animationComponent.AddClip(idleClip, "Idle");
        idleAnimation = idleClip;
        
        // יצירת אנימציית Walk פשוטה
        AnimationClip walkClip = CreateBasicWalkAnimation();
        animationComponent.AddClip(walkClip, "Walk");
        walkAnimation = walkClip;
        
        // יצירת אנימציית Run פשוטה
        AnimationClip runClip = CreateBasicRunAnimation();
        animationComponent.AddClip(runClip, "Run");
        runAnimation = runClip;
        
        // הגדרת הקליפ הברירת מחדל
        animationComponent.clip = idleClip;
        
        Debug.Log("MinipollAnimationController: Basic working animations created successfully");
    }
    
    AnimationClip CreateBasicIdleAnimation()
    {
        AnimationClip clip = new AnimationClip();
        clip.name = "BasicIdle";
        clip.legacy = true;
        
        // יצירת אנימציית נשימה פשוטה
        AnimationCurve breathingCurve = new AnimationCurve();
        breathingCurve.AddKey(0f, 0f);
        breathingCurve.AddKey(1f, 0.02f);
        breathingCurve.AddKey(2f, 0f);
        
        clip.SetCurve("", typeof(Transform), "localPosition.y", breathingCurve);
        clip.wrapMode = WrapMode.Loop;
        
        return clip;
    }
    
    AnimationClip CreateBasicWalkAnimation()
    {
        AnimationClip clip = new AnimationClip();
        clip.name = "BasicWalk";
        clip.legacy = true;
        
        // יצירת אנימציית הליכה פשוטה - תזוזה קלה למעלה ולמטה
        AnimationCurve walkCurve = new AnimationCurve();
        walkCurve.AddKey(0f, 0f);
        walkCurve.AddKey(0.25f, 0.05f);
        walkCurve.AddKey(0.5f, 0f);
        walkCurve.AddKey(0.75f, 0.05f);
        walkCurve.AddKey(1f, 0f);
        
        clip.SetCurve("", typeof(Transform), "localPosition.y", walkCurve);
        clip.wrapMode = WrapMode.Loop;
        
        return clip;
    }
    
    AnimationClip CreateBasicRunAnimation()
    {
        AnimationClip clip = new AnimationClip();
        clip.name = "BasicRun";
        clip.legacy = true;
        
        // יצירת אנימציית ריצה - תזוזה יותר גדולה ומהירה
        AnimationCurve runCurve = new AnimationCurve();
        runCurve.AddKey(0f, 0f);
        runCurve.AddKey(0.2f, 0.1f);
        runCurve.AddKey(0.4f, 0f);
        runCurve.AddKey(0.6f, 0.1f);
        runCurve.AddKey(0.8f, 0f);
        runCurve.AddKey(1f, 0f);
        
        clip.SetCurve("", typeof(Transform), "localPosition.y", runCurve);
        clip.wrapMode = WrapMode.Loop;
        
        return clip;
    }
    
    void Update()
    {
        if (agent == null) return;
        
        // קבל מהירות מה-NavMeshAgent - זה הדבר הכי חשוב!
        float speed = agent.velocity.magnitude;
        
        // עבוד עם Animator אם קיים - זה המצב המועדף
        if (animator != null)
        {
            UpdateAnimatorParameters(speed);
        }
        else if (animationComponent != null)
        {
            // fallback למצב Animation Component (מצב ישן)
            UpdateLegacyAnimations(speed);
        }
        
        // Debug info להבנה מה קורה
        if (speed > 0.1f)
        {
            Debug.Log($"MinipollAnimation: {gameObject.name} speed={speed:F2}, using {(animator != null ? "Animator" : "Animation Component")}");
        }
    }
    
    void UpdateAnimatorParameters(float speed)
    {
        // עדכן פרמטר Speed - זה הפרמטר העיקרי שצריך לעבוד!
        animator.SetFloat("Speed", speed);
        
        // פרמטרי Bool נוספים (רק אם הם קיימים ב-Animator Controller)
        bool isMoving = speed > 0.1f;
        bool isRunning = speed > 3f;
        bool isWalking = speed > 0.1f && speed <= 3f;
        
        // נעדכן רק פרמטרים שקיימים כדי למנוע שגיאות
        try 
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsRunning", isRunning);
        }
        catch (System.Exception)
        {
            // אם הפרמטרים לא קיימים ב-Controller, זה בסדר
            // המשיכו לעבוד רק עם Speed
        }
        
        // Debug info
        if (speed > 0.1f)
        {
            Debug.Log($"Animator Update: Speed={speed:F2}, Walking={isWalking}, Running={isRunning}");
        }
    }
    
    void UpdateLegacyAnimations(float speed)
    {
        // עכשיו נפעיל את האנימציות שוב
        if (animationComponent == null) return;
        
        // השיטה עם Animation Component
        if (speed > 3f && runAnimation != null && animationComponent.GetClip("Run") != null)
        {
            if (!animationComponent.IsPlaying("Run"))
            {
                animationComponent.CrossFade("Run", 0.3f);
                Debug.Log("MinipollAnimationController: Playing Run animation");
            }
        }
        else if (speed > 0.1f && walkAnimation != null && animationComponent.GetClip("Walk") != null)
        {
            if (!animationComponent.IsPlaying("Walk"))
            {
                animationComponent.CrossFade("Walk", 0.3f);
                Debug.Log("MinipollAnimationController: Playing Walk animation");
            }
        }
        else if (speed <= 0.1f && idleAnimation != null && animationComponent.GetClip("Idle") != null)
        {
            if (!animationComponent.IsPlaying("Idle"))
            {
                animationComponent.CrossFade("Idle", 0.3f);
                Debug.Log("MinipollAnimationController: Playing Idle animation");
            }
        }
        else if (speed <= 0.1f)
        {
            // fallback - אם אין אנימציית Idle, נעצור את כל האנימציות
            if (animationComponent.isPlaying)
            {
                animationComponent.Stop();
                Debug.Log("MinipollAnimationController: Stopped all animations (idle fallback)");
            }
        }
    }
    
    public void PlayEatAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Eat");
        }
        else if (eatAnimation != null && animationComponent != null)
        {
            animationComponent.CrossFade("Eat", 0.5f);
        }
    }
    
    public void PlaySleepAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Sleep");
        }
        else if (sleepAnimation != null && animationComponent != null && animationComponent.GetClip("Sleep") != null)
        {
            animationComponent.CrossFade("Sleep", 0.5f);
        }
        else
        {
            Debug.LogWarning("MinipollAnimationController: Cannot play Sleep animation - not loaded");
        }
    }
    
    public void PlayIdleAnimation()
    {
        if (animator != null)
        {
            // במצב Animator, זה יקרה אוטומטית כשהמהירות 0
            animator.SetBool("IsMoving", false);
        }
        else if (idleAnimation != null && animationComponent != null)
        {
            animationComponent.CrossFade("Idle", 0.5f);
        }
    }
    
    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    
    public void PlayJumpAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
    }
    
    public void PlayFearAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Fear");
        }
    }
    
    // פונקציה לחזרה למצב רגיל מאנימציות מיוחדות
    public void ReturnToNormalState()
    {
        if (animator != null)
        {
            animator.SetBool("IsEating", false);
            animator.SetBool("IsSleeping", false);
            animator.SetBool("IsAfraid", false);
        }
    }
}
