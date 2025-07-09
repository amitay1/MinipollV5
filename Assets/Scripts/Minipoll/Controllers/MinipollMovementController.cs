using UnityEngine;
using UnityEngine.AI;
using System;
using MinipollGame.Core;
using System.Collections;

namespace MinipollGame.Controllers
{
    public enum MinipollActionState
    {
        Idle,
        Roaming,
        SeekingResource,
        Escaping,
        Socializing,
        Resting
    }

    /// <summary>
    /// MinipollMovementController – מערכת תנועה משופרת:
    /// 1. תומך בשימוש אופציונלי ב־NavMeshAgent (useNavMesh).
    /// 2. מנהל States (Idle, Roaming וכו’).
    /// 3. קורא ל־Animator.SetBool(“IsWalking”/“IsRunning”) או כל פרמטר רצוי אחר.
    /// 4. מפעיל event OnDestinationReached כאשר מגיעים ליעד.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MinipollMovementController : MonoBehaviour
    {
        private global::MinipollGame.Core.MinipollBrain brain;

        [Header("Movement Settings")]
        public float moveSpeed = 2f;         // מהירות בסיסית
        public float rotationSpeed = 180f;   // מהירות סיבוב (ללא navmesh)
        public bool useNavMesh = true;       // כמעט תמיד נרצה NavMesh - זה המצב המועדף!

        [Header("NavMesh")]
        public NavMeshAgent agent;           // נשים פה את ה־NavMeshAgent
        public float arrivalThreshold = 1f;  // מרחק הגדרה של “הגענו ליעד”

        [Header("Animation / Animator")]
        [SerializeField] private Animator animator;
        // נניח שיש לנו Bool "IsWalking", "IsRunning" ב־Animator
        // או שנשתמש ב־animator.SetFloat("Speed", speed)

        [Header("States")]
        public MinipollActionState currentState = MinipollActionState.Idle;
        public bool allowRoaming = true; // האם בכלל מותר להיכנס ל־Roaming
        public float roamDuration = 5f;  // זמן מקס להסתובב לפני חזרה ל־Idle
        private float roamTimer = 0f;

        public Vector3 targetPosition;   // יעד נוכחי (למצב Roaming / SeekingResource וכו’)
        private float escapeDistance = 10f; // מרחק בריחה
        internal bool isMovingToTarget;

        // Event שמעיד על הגעה ליעד (כמו בגו’סטיק קווסט וכו’)
        public event Action OnDestinationReached;

        void Awake()
        {
            Debug.Log($"[Movement] Initializing MinipollMovementController for {gameObject.name}");
            // מאתרים NavMeshAgent
            if (!agent)
            {
                agent = GetComponent<NavMeshAgent>();
                if (!agent)
                {
                    agent = gameObject.AddComponent<NavMeshAgent>();
                    Debug.Log($"[Movement] NavMeshAgent found/added to {gameObject.name}");
                }

            }
            agent.speed = moveSpeed;
            agent.angularSpeed = rotationSpeed * 2f; // אפשר לכוון כרצונך

            // מאתרים Animator אם לא הוגדר ידנית
            if (!animator)
            {
                animator = GetComponentInChildren<Animator>();
                Debug.Log($"[Movement] Animator found/added to {gameObject.name}");
            }
        }

        public void InitSystem(global::MinipollGame.Core.MinipollBrain ownerBrain)
        {
            brain = ownerBrain;
            if (agent && useNavMesh)
            {
                agent.speed = moveSpeed;
            }
        }
void Start()
{
    // Force start roaming after initialization
    StartCoroutine(StartInitialBehavior());
}

private IEnumerator StartInitialBehavior()
{
    yield return new WaitForSeconds(2f); // Wait 2 seconds for everything to initialize
    
    Debug.Log($"[Movement] Starting initial behavior for {gameObject.name}");
    
    if (allowRoaming && currentState == MinipollActionState.Idle)
    {
        Debug.Log("[Movement] Starting automatic roaming...");
        StartRoaming();
    }
    else
    {
        Debug.Log($"[Movement] Not starting roaming. AllowRoaming: {allowRoaming}, CurrentState: {currentState}");
    }
}
        public void UpdateMovement(float deltaTime)
        {        // בדיקת אפשרות "Escaping" (אם מערכת חברתית אומרת שיש איום)
            // TODO: Implement escape logic when social system is available
            // if (brain?.GetSocialSystem() != null)
            // {
            //     // Check for threats and start escape if needed
            // }

            // עדכון לוגיקת State:
            switch (currentState)
            {
                case MinipollActionState.Idle:
                    HandleIdle(deltaTime);
                    break;
                case MinipollActionState.Roaming:
                    HandleRoaming(deltaTime);
                    break;
                case MinipollActionState.SeekingResource:
                    HandleSeekingResource(deltaTime);
                    break;
                case MinipollActionState.Escaping:
                    HandleEscaping(deltaTime);
                    break;
                case MinipollActionState.Socializing:
                    HandleSocializing(deltaTime);
                    break;
                case MinipollActionState.Resting:
                    HandleResting(deltaTime);
                    break;
            }

            // עדכון Animator לפי מהירות / סטייט
            UpdateAnimator();
        }

        private void StartEscape(MinipollBrain threat)
        {
            throw new NotImplementedException();
        }

        #region Idle
        private void HandleIdle(float deltaTime)
        {
            // סתם עומדים. לפעמים נתחיל לשוטט אם allowRoaming
            if (allowRoaming && UnityEngine.Random.value < 0.01f * deltaTime)
            {
                StartRoaming();
            }
        }
        #endregion

        #region Roaming
        private void StartRoaming()
        {
            currentState = MinipollActionState.Roaming;
            roamTimer = 0f;
            targetPosition = FindRandomPoint(10f); // רדיוס 10 בסביבה
            if (useNavMesh && agent)
            {
                agent.SetDestination(targetPosition);
            }
        }

        private void HandleRoaming(float deltaTime)
        {
            roamTimer += deltaTime;
            if (roamTimer >= roamDuration)
            {
                // חזרנו ל־Idle
                currentState = MinipollActionState.Idle;
                StopMovement();
                return;
            }

            float dist = Vector3.Distance(transform.position, targetPosition);
            if (dist < arrivalThreshold)
            {
                // נבחר יעד חדש?
                targetPosition = FindRandomPoint(10f);
                if (useNavMesh && agent)
                    agent.SetDestination(targetPosition);
            }
            else
            {
                // נעים לעבר היעד
                if (!useNavMesh)
                {
                    MoveTowards(targetPosition, deltaTime);
                }
                else
                {
                    CheckArrival();
                }
            }
        }
        #endregion

        #region SeekingResource
        public void StartSeekingResource(Vector3 resourcePos)
        {
            currentState = MinipollActionState.SeekingResource;
            targetPosition = resourcePos;
            if (useNavMesh && agent)
            {
                agent.SetDestination(targetPosition);
            }
        }

        private void HandleSeekingResource(float deltaTime)
        {
            float dist = Vector3.Distance(transform.position, targetPosition);
            if (dist < arrivalThreshold)
            {
                currentState = MinipollActionState.Idle;
                StopMovement();
                OnDestinationReached?.Invoke(); // הגענו
            }
            else
            {
                if (!useNavMesh)
                {
                    MoveTowards(targetPosition, deltaTime);
                }
                else
                {
                    CheckArrival();
                }
            }
        }
        #endregion

        #region Escaping
        // private void StartEscape(MinipollBrain threat)
        // {
        //     currentState = MinipollActionState.Escaping;
        //     if (threat != null)
        //     {
        //         Vector3 awayDir = (transform.position - threat.transform.position).normalized;
        //         targetPosition = transform.position + awayDir * escapeDistance;
        //         if (useNavMesh && agent)
        //             agent.SetDestination(targetPosition);
        //     }
        //     else
        //     {
        //         // אם לא ידוע ממי בורחים => נקודה רנדומלית
        //         targetPosition = FindRandomPoint(escapeDistance);
        //         if (useNavMesh && agent)
        //             agent.SetDestination(targetPosition);
        //     }
        // }

        private void HandleEscaping(float deltaTime)
        {
            float dist = Vector3.Distance(transform.position, targetPosition);
            if (dist < arrivalThreshold)
            {
                currentState = MinipollActionState.Idle;
                StopMovement();
            }
            else
            {
                if (!useNavMesh)
                    MoveTowards(targetPosition, deltaTime, 1.5f); // ריצה מהירה
                else
                    CheckArrival();
            }
        }
        #endregion

        #region Socializing
        public void StartSocializing(Vector3 friendPos)
        {
            currentState = MinipollActionState.Socializing;
            targetPosition = friendPos;
            if (useNavMesh && agent)
                agent.SetDestination(friendPos);
        }

        private void HandleSocializing(float deltaTime)
        {
            float dist = Vector3.Distance(transform.position, targetPosition);
            if (dist < arrivalThreshold)
            {
                currentState = MinipollActionState.Idle;
                StopMovement();
                OnDestinationReached?.Invoke();
            }
            else
            {
                if (!useNavMesh)
                    MoveTowards(targetPosition, deltaTime);
                else
                    CheckArrival();
            }
        }
        #endregion

        #region Resting
        public void StartResting()
        {
            currentState = MinipollActionState.Resting;
            StopMovement();
        }        private void HandleResting(float deltaTime)
        {
            // מנוחה => לא זזים
            // אפשר למלא Need: Energy + 3f * deltaTime וכו'
            if (brain && brain.GetNeedsSystem() != null)
            {
                var needsSystem = brain.GetNeedsSystem() as MinipollGame.Core.MinipollNeedsSystem;
                needsSystem?.FillNeed("Energy", 3f * deltaTime);
            }
            // אם או כאשר מגיעים למקס אנרגיה => currentState = Idle
        }
        #endregion

        #region Public Interface Methods
        public void StopMovement()
        {
            if (useNavMesh && agent)
            {
                agent.ResetPath();
            }
            // אפשר גם currentState = Idle; אם תרצה
        }

        /// <summary>
        /// Move to a specific target position. This is a general movement method.
        /// </summary>
        /// <param name="targetPos">The target position to move to</param>
        public void MoveTo(Vector3 targetPos)
        {
            targetPosition = targetPos;
            isMovingToTarget = true;

            if (useNavMesh && agent)
            {
                agent.SetDestination(targetPosition);
            }

            // Set appropriate state if not already in a specific action
            if (currentState == MinipollActionState.Idle)
            {
                currentState = MinipollActionState.SeekingResource;
            }
        }

        // אם תרצה לשמור API למשהו כמו "StartWandering()"
        // עבור BehaviorTree, אפשר פשוט:
        public void StartWandering()
        {
            StartRoaming();
        }
        #endregion

        #region Utility Functions
        void CheckArrival()
        {
            if (!agent.pathPending && agent.remainingDistance <= arrivalThreshold)
            {
                OnDestinationReached?.Invoke();
            }
        }

        void MoveTowards(Vector3 pos, float deltaTime, float runMultiplier = 1f)
        {
            Vector3 dir = (pos - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * deltaTime);

            float speed = moveSpeed * runMultiplier;
            transform.position += transform.forward * speed * deltaTime;
        }

        Vector3 FindRandomPoint(float radius)
        {
            Vector2 r = UnityEngine.Random.insideUnitCircle * radius;
            return transform.position + new Vector3(r.x, 0f, r.y);
        }

        void UpdateAnimator()
        {
            if (animator == null) return;
            
            float velocity = 0f;
            
            // עבוד עם NavMeshAgent - זה המצב המועדף!
            if (useNavMesh && agent)
            {
                velocity = agent.velocity.magnitude;
                Debug.Log($"[Movement] NavMesh velocity: {velocity:F2} for {gameObject.name}");
            }
            else
            {
                // אם לא משתמשים ב-NavMesh, נגדיר ערך לפי state (מצב fallback)
                if (currentState == MinipollActionState.Roaming ||
                    currentState == MinipollActionState.SeekingResource ||
                    currentState == MinipollActionState.Socializing)
                    velocity = moveSpeed;
                else if (currentState == MinipollActionState.Escaping)
                    velocity = moveSpeed * 1.5f;
                else
                    velocity = 0f;
                    
                Debug.Log($"[Movement] Manual velocity: {velocity:F2} for {gameObject.name}");
            }

            // עדכן את ה-Animator עם פרמטר Speed - זה הכי חשוב!
            animator.SetFloat("Speed", velocity);
            
            // פרמטרי Bool נוספים (רק אם הם קיימים)
            try 
            {
                bool isWalking = (velocity > 0.1f && velocity < 3f);
                bool isRunning = (velocity >= 3f);

                animator.SetBool("IsWalking", isWalking);
                animator.SetBool("IsRunning", isRunning);
                
                Debug.Log($"[Movement] Animation state: Walking={isWalking}, Running={isRunning}");
            }
            catch (System.Exception)
            {
                // אם הפרמטרים לא קיימים ב-Controller, זה בסדר
                // נמשיך לעבוד רק עם Speed
            }
        }
        #endregion

        #region Age and Speed Management
        private float speedMultiplier = 1f;

        /// <summary>
        /// Set speed multiplier for movement
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;

            // Update NavMeshAgent speed if using NavMesh
            if (useNavMesh && agent && agent.enabled)
            {
                agent.speed = moveSpeed * speedMultiplier;
            }
        }

        /// <summary>
        /// Update movement parameters based on age stage
        /// </summary>
        // public void UpdateForAgeStage(AgeStage ageStage)g
        // {
        //     switch (AgeStage)
        //     {
        //         case AgeStage.Baby:
        //             SetSpeedMultiplier(0.5f); // Babies move slowly
        //             break;
        //         case AgeStage.Child:
        //             SetSpeedMultiplier(0.8f); // Children move moderately
        //             break;
        //         case AgeStage.Adult:
        //             SetSpeedMultiplier(1.0f); // Adults at normal speed
        //             break;
        //         case AgeStage.Elder:
        //             SetSpeedMultiplier(0.7f); // Elders move more slowly
        //             break;
        //     }
        // }

        internal void UpdateForAgeStage(Core.MinipollCore.AgeStage newStage)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
