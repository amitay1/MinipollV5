using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MinipollGame.Systems.Core;
/// <summary>
/// MinipollBrain - המוח המרכזי של המיניפול
/// עובד בשיתוף פעולה עם MinipollCore
/// מנהל את ה-AI, קבלת החלטות, ומתאם בין המערכות
/// </summary>
namespace MinipollGame.Core
{
    public class MinipollBrain : MonoBehaviour
    {
        #region Core References
        private MinipollCore coreComponent;

        // Quick access properties
        public bool IsAlive => coreComponent != null && coreComponent.IsAlive;
        public float Health => coreComponent != null && coreComponent.Health != null ? coreComponent.Health.CurrentHealth : 0f;
        public string Name => coreComponent != null ? coreComponent.Name : "Unknown";
        #endregion

        #region Goals and Decision Making
        [System.Serializable]
        public class Goal
        {
            public string name;
            public GoalPriority priority;
            public float urgency; // 0-1
            public Vector3 targetPosition;
            public GameObject targetObject;
            public System.Action<MinipollBrain> executeAction;

            public Goal(string goalName, GoalPriority goalPriority = GoalPriority.Medium)
            {
                name = goalName;
                priority = goalPriority;
                urgency = 0.5f;
                targetPosition = Vector3.zero;
                targetObject = null;
                executeAction = null;
            }
        }

        [Header("=== Brain Settings ===")]
        public bool verboseLogging = false;
        
        [Header("=== Decision Making ===")]
        public float decisionInterval = 1f; // How often to make decisions
        public float urgentDecisionMultiplier = 0.2f; // Faster decisions when urgent
        
        private List<Goal> availableGoals = new List<Goal>();
        private Goal currentGoal;
        private float nextDecisionTime;
        private bool hasUrgentGoal = false;
        private float moodInfluence = 1f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Get references
            coreComponent = GetComponent<MinipollCore>();
            
            if (coreComponent == null)
            {
                Debug.LogError($"[MinipollBrain] No MinipollCore found on {gameObject.name}!");
            }
        }

        private void Start()
        {
            InitializeDefaultGoals();
            
            if (verboseLogging)
            {
                Debug.Log($"[Brain] {Name} brain initialized");
            }
        }

        private void Update()
        {
            if (!IsAlive) return;

            float deltaTime = Time.deltaTime;
            
            // Decision timing logic
            float currentInterval = hasUrgentGoal ? 
                decisionInterval * urgentDecisionMultiplier : 
                decisionInterval;

            if (Time.time >= nextDecisionTime)
            {
                UpdateGoalUrgencies();
                MakeDecision();
                nextDecisionTime = Time.time + currentInterval;
                hasUrgentGoal = false;
            }

            // Execute current goal
            if (currentGoal != null)
            {
                ExecuteGoal(currentGoal);
            }
        }
        #endregion

        #region System Initialization
        public void InitializeDefaultGoals()
        {
            availableGoals.Clear();
            
            // Basic survival goals
            availableGoals.Add(new Goal("FindFood", GoalPriority.High)
            {
                executeAction = (brain) => brain.ExecuteFindFood()
            });
            
            availableGoals.Add(new Goal("FindWater", GoalPriority.High)
            {
                executeAction = (brain) => brain.ExecuteFindWater()
            });
            
            availableGoals.Add(new Goal("Rest", GoalPriority.Medium)
            {
                executeAction = (brain) => brain.ExecuteRest()
            });
            
            availableGoals.Add(new Goal("Socialize", GoalPriority.Medium)
            {
                executeAction = (brain) => brain.ExecuteSocialize()
            });
            
            availableGoals.Add(new Goal("Explore", GoalPriority.Low)
            {
                executeAction = (brain) => brain.ExecuteExplore()
            });
            
            availableGoals.Add(new Goal("Escape", GoalPriority.Critical)
            {
                executeAction = (brain) => brain.ExecuteEscape()
            });
        }

        private void MakeDecision()
        {
            Goal bestGoal = null;
            float highestScore = 0f;

            foreach (var goal in availableGoals)
            {
                float score = CalculateGoalScore(goal);
                
                if (score > highestScore)
                {
                    highestScore = score;
                    bestGoal = goal;
                }
            }

            if (bestGoal != null && bestGoal != currentGoal)
            {
                currentGoal = bestGoal;
                
                if (verboseLogging)
                {
                    Debug.Log($"[Brain] {Name} switching to goal: {currentGoal.name} (score: {highestScore:F2})");
                }
            }
        }

        private float CalculateGoalScore(Goal goal)
        {
            float baseScore = (float)goal.priority / 10f;
            float urgencyScore = goal.urgency;
            float totalScore = (baseScore + urgencyScore) * moodInfluence;
            
            return totalScore;
        }

        private void UpdateGoalUrgencies()
        {
            // This is a simplified version - in a real implementation you'd check actual systems
            SetGoalUrgency("FindFood", UnityEngine.Random.Range(0.3f, 0.9f));
            SetGoalUrgency("FindWater", UnityEngine.Random.Range(0.2f, 0.8f));
            SetGoalUrgency("Rest", UnityEngine.Random.Range(0.1f, 0.7f));
            SetGoalUrgency("Socialize", UnityEngine.Random.Range(0.2f, 0.6f));
            
            SetGoalPriority("Explore", GoalPriority.Low);
        }
        #endregion

        #region Goal Execution
        private void ExecuteFindFood()
        {
            if (verboseLogging)
                Debug.Log($"[Brain] {Name} looking for food");
        }

        private void ExecuteFindWater()
        {
            if (verboseLogging)
                Debug.Log($"[Brain] {Name} looking for water");
        }

        private void ExecuteRest()
        {
            if (verboseLogging)
                Debug.Log($"[Brain] {Name} resting");
        }

        private void ExecuteSocialize()
        {
            if (verboseLogging)
                Debug.Log($"[Brain] {Name} socializing");
        }

        private void ExecuteExplore()
        {
            if (verboseLogging)
                Debug.Log($"[Brain] {Name} exploring");
        }

        private void ExecuteEscape()
        {
            if (verboseLogging)
                Debug.Log($"[Brain] {Name} escaping from danger");
        }

        private void ExecuteGoal(Goal goal)
        {
            goal.executeAction?.Invoke(this);
        }
        #endregion

        #region Goal Management
        public Goal GetGoal(string goalName)
        {
            return availableGoals.Find(g => g.name == goalName);
        }

        public void SetGoalUrgency(string goalName, float urgency)
        {
            var goal = GetGoal(goalName);
            if (goal != null)
            {
                goal.urgency = Mathf.Clamp01(urgency);
                
                if (goal.urgency > 0.8f)
                {
                    hasUrgentGoal = true;
                }
            }
        }

        public void ModifyGoalUrgency(string goalName, float delta)
        {
            var goal = GetGoal(goalName);
            if (goal != null)
            {
                goal.urgency = Mathf.Clamp01(goal.urgency + delta);
            }
        }

        public void SetGoalPriority(string goalName, GoalPriority newPriority)
        {
            var goal = GetGoal(goalName);
            if (goal != null)
            {
                goal.priority = newPriority;
            }
        }

        public void SetUrgentGoal(string goalName)
        {
            var goal = GetGoal(goalName);
            if (goal != null)
            {
                currentGoal = goal;
                hasUrgentGoal = true;
            }
        }

        public void ForceGoal(string goalName)
        {
            var goal = GetGoal(goalName);
            if (goal != null)
            {
                currentGoal = goal;
                if (verboseLogging)
                    Debug.Log($"[Brain] {Name} forced to goal: {goalName}");
            }
        }
        #endregion

        #region System Access (for other scripts)
        public MinipollCore GetCore() => coreComponent;
        #endregion

        #region Debug
#if UNITY_EDITOR
        [Header("=== Debug ===")]
        public bool showGoalGizmos = true;
        internal float health;
        internal float happiness;
        internal float energy;
        internal int currentState;
        internal float hunger;
        internal float thirst;

        private void OnDrawGizmosSelected()
        {
            if (!showGoalGizmos || currentGoal == null) return;

            Gizmos.color = Color.green;
            if (currentGoal.targetObject != null)
            {
                Gizmos.DrawWireSphere(currentGoal.targetObject.transform.position, 0.5f);
                Gizmos.DrawLine(transform.position, currentGoal.targetObject.transform.position);
            }
            else if (currentGoal.targetPosition != Vector3.zero)
            {
                Gizmos.DrawWireSphere(currentGoal.targetPosition, 0.5f);
                Gizmos.DrawLine(transform.position, currentGoal.targetPosition);
            }
        }

        internal object GetSocialSystem()
        {
            throw new NotImplementedException();
        }

        internal void TakeDamage(float v)
        {
            throw new NotImplementedException();
        }

        internal object GetEmotionsSystem()
        {
            throw new NotImplementedException();
        }

        internal void SetMoodInfluence(float v)
        {
            throw new NotImplementedException();
        }

        internal void SetUrgentGoal(NeedType needType)
        {
            throw new NotImplementedException();
        }

        internal object GetNeedsSystem()
        {
            throw new NotImplementedException();
        }

        internal object GetMemorySystem()
        {
            throw new NotImplementedException();
        }        internal void ApplyHealthPower(float power)
        {
            throw new NotImplementedException();
        }        internal object GetMovementController()
        {
            throw new NotImplementedException();
        }
#endif
        #endregion
    }
}



