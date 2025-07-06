using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MinipollGame.Core;
using MinipollGame.Systems.Core;
using MinipollGame.Social;
using MinipollCore.core;
using Unity.VisualScripting;


namespace MinipollGame.AI
{
    /// <summary>
    /// Utility AI system for decision making based on scoring different actions
    /// Each action gets a utility score based on current needs and context
    /// </summary>
    public class UtilityAIManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float decisionInterval = 0.5f;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private AnimationCurve defaultResponseCurve;
        
        [Header("Action Weights")]
        [SerializeField] private float needsWeight = 1f;
        [SerializeField] private float emotionalWeight = 0.5f;
        [SerializeField] private float socialWeight = 0.7f;
        [SerializeField] private float survivalWeight = 1.5f;

        // Available actions
        [Serializable]
        public class AIAction
        {
            public string actionName;
            public ActionType type;
            public float basePriority = 0.5f;
            public float energyCost = 10f;
            public float timeCost = 1f;
            public AnimationCurve responseCurve;
            public List<Consideration> considerations;
            
            [NonSerialized] public float lastExecutionTime;
            [NonSerialized] public float executionCount;
        }

        public enum ActionType
        {
            Survival,    // Eat, Drink, Sleep, Flee
            Social,      // Communicate, Share, Mate
            Exploration, // Wander, Investigate
            Work,        // Gather, Build, Craft
            Combat,      // Attack, Defend
            Emotional    // Express, Comfort
        }

        // Considerations for scoring
        [Serializable]
        public class Consideration
        {
            public string name;
            public ConsiderationType type;
            public float weight = 1f;
            public AnimationCurve curve;
            public float min = 0f;
            public float max = 1f;
        }

        public enum ConsiderationType
        {
            // Needs
            Hunger,
            Thirst,
            Energy,
            Safety,
            Social,
            Fun,
            
            // Environmental
            TimeOfDay,
            Weather,
            Temperature,
            NearbyThreats,
            NearbyResources,
            NearbyFriends,
            
            // Internal State
            Health,
            Age,
            Emotion,
            Memory,
            Skill,
            
            // Context
            DistanceToTarget,
            RelationshipStrength,
            GroupSize,
            ResourceAvailability
        }

        // Decision context
        public class DecisionContext
        {
            private MinipollCoreType agent;
            public GameObject target;
            public Vector3 targetPosition;
            public Dictionary<ConsiderationType, float> inputs;
            public float timestamp;

            public DecisionContext()
            {
                inputs = new Dictionary<ConsiderationType, float>();
                timestamp = Time.time;
            }
        }

        // Action execution result
        public class ActionResult
        {
            public bool success;
            public float utilityGained;
            public string failureReason;
            public Dictionary<string, object> data;

            public ActionResult()
            {
                data = new Dictionary<string, object>();
            }
        }

        // System data
        private Dictionary<MinipollCoreType, List<AIAction>> agentActions = new Dictionary<MinipollCoreType, List<AIAction>>();
        private Dictionary<MinipollCoreType, AIAction> currentActions = new Dictionary<MinipollCoreType, AIAction>();
        private Dictionary<MinipollCoreType, float> lastDecisionTimes = new Dictionary<MinipollCoreType, float>();
        private Dictionary<MinipollCoreType, DecisionContext> agentContexts = new Dictionary<MinipollCoreType, DecisionContext>();

        // Action definitions
        private List<AIAction> actionTemplates = new List<AIAction>();
        private float foodLevel;
        private IEnumerable<object> nearby;
        private object otherMinipoll;
        private float distance;

        private void Awake()
        {
            InitializeActionTemplates();
        }

        private void InitializeActionTemplates()
        {
            // Survival Actions
            actionTemplates.Add(new AIAction
            {
                actionName = "Eat",
                type = ActionType.Survival,
                basePriority = 0.9f,
                energyCost = 5f,
                timeCost = 2f,
                responseCurve = defaultResponseCurve,
                considerations = new List<Consideration>
                {
                    new Consideration { name = "Hunger", type = ConsiderationType.Hunger, weight = 2f },
                    new Consideration { name = "FoodNearby", type = ConsiderationType.NearbyResources, weight = 1f },
                    new Consideration { name = "Safety", type = ConsiderationType.Safety, weight = 0.5f }
                }
            });

            actionTemplates.Add(new AIAction
            {
                actionName = "Drink",
                type = ActionType.Survival,
                basePriority = 0.9f,
                energyCost = 3f,
                timeCost = 1f,
                responseCurve = defaultResponseCurve,
                considerations = new List<Consideration>
                {
                    new Consideration { name = "Thirst", type = ConsiderationType.Thirst, weight = 2f },
                    new Consideration { name = "WaterNearby", type = ConsiderationType.NearbyResources, weight = 1f }
                }
            });

            actionTemplates.Add(new AIAction
            {
                actionName = "Sleep",
                type = ActionType.Survival,
                basePriority = 0.7f,
                energyCost = 0f,
                timeCost = 10f,
                responseCurve = defaultResponseCurve,
                considerations = new List<Consideration>
                {
                    new Consideration { name = "Tiredness", type = ConsiderationType.Energy, weight = 2f },
                    new Consideration { name = "Safety", type = ConsiderationType.Safety, weight = 1.5f },
                    new Consideration { name = "NightTime", type = ConsiderationType.TimeOfDay, weight = 1f }
                }
            });

            actionTemplates.Add(new AIAction
            {
                actionName = "Flee",
                type = ActionType.Survival,
                basePriority = 1f,
                energyCost = 20f,
                timeCost = 0.5f,
                responseCurve = defaultResponseCurve,
                considerations = new List<Consideration>
                {
                    new Consideration { name = "Danger", type = ConsiderationType.NearbyThreats, weight = 3f },
                    new Consideration { name = "Health", type = ConsiderationType.Health, weight = 1f }
                }
            });

            // Social Actions
            actionTemplates.Add(new AIAction
            {
                actionName = "Communicate",
                type = ActionType.Social,
                basePriority = 0.6f,
                energyCost = 5f,
                timeCost = 1f,
                responseCurve = defaultResponseCurve,
                considerations = new List<Consideration>
                {
                    new Consideration { name = "SocialNeed", type = ConsiderationType.Social, weight = 2f },
                    new Consideration { name = "FriendsNearby", type = ConsiderationType.NearbyFriends, weight = 1f },
                    new Consideration { name = "GroupSize", type = ConsiderationType.GroupSize, weight = 0.5f }
                }
            });

            actionTemplates.Add(new AIAction
            {
                actionName = "Share",
                type = ActionType.Social,
                basePriority = 0.4f,
                energyCost = 10f,
                timeCost = 1f,
                responseCurve = defaultResponseCurve,
                considerations = new List<Consideration>
                {
                    new Consideration { name = "Relationship", type = ConsiderationType.RelationshipStrength, weight = 2f },
                    new Consideration { name = "ResourceAbundance", type = ConsiderationType.ResourceAvailability, weight = 1f }
                }
            });

            // Exploration Actions
            actionTemplates.Add(new AIAction
            {
                actionName = "Explore",
                type = ActionType.Exploration,
                basePriority = 0.5f,
                energyCost = 10f,
                timeCost = 5f,
                responseCurve = defaultResponseCurve,
                considerations = new List<Consideration>
                {
                    new Consideration { name = "Curiosity", type = ConsiderationType.Fun, weight = 1.5f },
                    new Consideration { name = "Energy", type = ConsiderationType.Energy, weight = 1f },
                    new Consideration { name = "Safety", type = ConsiderationType.Safety, weight = 0.8f }
                }
            });

            // Work Actions
            actionTemplates.Add(new AIAction
            {
                actionName = "Gather",
                type = ActionType.Work,
                basePriority = 0.6f,
                energyCost = 15f,
                timeCost = 3f,
                responseCurve = defaultResponseCurve,
                considerations = new List<Consideration>
                {
                    new Consideration { name = "ResourceNeed", type = ConsiderationType.Hunger, weight = 1f },
                    new Consideration { name = "ResourcesNearby", type = ConsiderationType.NearbyResources, weight = 2f },
                    new Consideration { name = "Energy", type = ConsiderationType.Energy, weight = 0.8f }
                }
            });

            // Combat Actions
            actionTemplates.Add(new AIAction
            {
                actionName = "Attack",
                type = ActionType.Combat,
                basePriority = 0.3f,
                energyCost = 25f,
                timeCost = 1f,
                responseCurve = defaultResponseCurve,
                considerations = new List<Consideration>
                {
                    new Consideration { name = "Aggression", type = ConsiderationType.Emotion, weight = 2f },
                    new Consideration { name = "TargetWeakness", type = ConsiderationType.Health, weight = 1f },
                    new Consideration { name = "Energy", type = ConsiderationType.Energy, weight = 0.5f }
                }
            });
        }

        #region Public API
        
        public void RegisterAgent(MinipollCoreType agent)
        {
            if (!agentActions.ContainsKey(agent))
            {
                agentActions[agent] = actionTemplates.Select(a => CloneAction(a)).ToList();
                lastDecisionTimes[agent] = 0f;
                // agentContexts[agent] = new DecisionContext { agent = agent };
            }
        }

        public void UnregisterAgent(MinipollCoreType agent)
        {
            agentActions.Remove(agent);
            currentActions.Remove(agent);
            lastDecisionTimes.Remove(agent);
            agentContexts.Remove(agent);
        }

        public AIAction GetBestAction(MinipollCoreType agent)
        {
            if (!agentActions.ContainsKey(agent))
                return null;

            // Check if it's time for a new decision
            if (Time.time - lastDecisionTimes.GetValueOrDefault(agent, 0f) < decisionInterval)
            {
                return currentActions.GetValueOrDefault(agent, null);
            }

            // Update context
            UpdateContext(agent);

            // Score all actions
            var context = agentContexts[agent];
            var actions = agentActions[agent];
            AIAction bestAction = null;
            float bestScore = float.MinValue;

            foreach (var action in actions)
            {
                float score = ScoreAction(action, context);
                
                if (debugMode)
                {
                    Debug.Log($"{agent.name} - Action: {action.actionName}, Score: {score}");
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAction = action;
                }
            }

            // Update current action
            if (bestAction != null)
            {
                currentActions[agent] = bestAction;
                lastDecisionTimes[agent] = Time.time;
            }

            return bestAction;
        }

        public ActionResult ExecuteAction(MinipollCoreType agent, AIAction action)
        {
            ActionResult result = new ActionResult();

            if (!CanExecuteAction(agent, action))
            {
                result.success = false;
                result.failureReason = "Cannot execute action - insufficient resources or conditions not met";
                return result;
            }

            // Execute based on action type
            switch (action.actionName)
            {
                case "Eat":
                    result = ExecuteEat(agent);
                    break;
                case "Drink":
                    result = ExecuteDrink(agent);
                    break;
                case "Sleep":
                    result = ExecuteSleep(agent);
                    break;
                case "Flee":
                    result = ExecuteFlee(agent);
                    break;
                case "Communicate":
                    result = ExecuteCommunicate(agent);
                    break;
                case "Share":
                    result = ExecuteShare(agent);
                    break;
                case "Explore":
                    result = ExecuteExplore(agent);
                    break;
                case "Gather":
                    result = ExecuteGather(agent);
                    break;
                case "Attack":
                    result = ExecuteAttack(agent);
                    break;
                default:
                    result.success = false;
                    result.failureReason = "Unknown action";
                    break;
            }

            if (result.success)
            {
                // This would need implementation in MinipollCore
                // agent.ConsumeEnergy(action.energyCost);
                action.lastExecutionTime = Time.time;
                action.executionCount++;
            }

            return result;
        }

        #endregion

        #region Context and Scoring

        private void UpdateContext(MinipollCoreType agent)
        {
            var context = agentContexts[agent];
            context.inputs.Clear();

            // Get components with proper casting
            var needs = agent.GetComponent<Core.MinipollNeedsSystem>() as Core.MinipollNeedsSystem;
            var emotions = agent.GetComponent<MinipollEmotionsSystem>() as MinipollEmotionsSystem;
            var social = agent.GetComponent<MinipollSocialRelations>() as MinipollSocialRelations;

            // Update needs inputs
            if (needs != null)
            {
                context.inputs[ConsiderationType.Hunger] = 1f - needs.GetNormalizedNeed("Food");
                context.inputs[ConsiderationType.Thirst] = 1f - needs.GetNormalizedNeed("Water");
                context.inputs[ConsiderationType.Energy] = 1f - needs.GetNormalizedNeed("Sleep");
                context.inputs[ConsiderationType.Safety] = needs.GetNormalizedNeed("Safety");
                context.inputs[ConsiderationType.Social] = 1f - needs.GetNormalizedNeed("Social");
                context.inputs[ConsiderationType.Fun] = 1f - needs.GetNormalizedNeed("Fun");
            }

            // Update environmental inputs
            context.inputs[ConsiderationType.TimeOfDay] = GetTimeOfDayValue();
            context.inputs[ConsiderationType.Weather] = GetWeatherValue();
            context.inputs[ConsiderationType.Temperature] = GetTemperatureValue();

            // Update proximity inputs
            UpdateProximityInputs(agent, context);

            // Update internal state - placeholder values since methods don't exist yet
            context.inputs[ConsiderationType.Health] = 1.0f; // agent.GetHealthPercentage();
            context.inputs[ConsiderationType.Age] = GetAgeValue(agent);

            // Update emotional state
            if (emotions != null)
            {
                var dominantEmotion = emotions.GetDominantEmotion();
                context.inputs[ConsiderationType.Emotion] = dominantEmotion.intensity;
            }

            // Update social context
            if (social != null && context.target != null)
            {
                var targetMinipoll = context.target.GetComponent<MinipollCoreType>();
                if (targetMinipoll != null)
                {
                    context.inputs[ConsiderationType.RelationshipStrength] = 
                        social.GetRelationshipValue(targetMinipoll.GetInstanceID());
                }
            }
        }

        private float ScoreAction(AIAction action, DecisionContext context)
        {
            float score = action.basePriority;

            // Apply consideration scores
            foreach (var consideration in action.considerations)
            {
                float input = context.inputs.GetValueOrDefault(consideration.type, 0.5f);
                float normalized = Mathf.InverseLerp(consideration.min, consideration.max, input);
                
                // Apply response curve
                float curveValue = consideration.curve != null ? 
                    consideration.curve.Evaluate(normalized) : 
                    normalized;
                
                // Apply weight
                score *= Mathf.Lerp(0.1f, 1f, curveValue * consideration.weight);
            }

            // Apply type weight
            score *= GetTypeWeight(action.type);

            // Apply recency penalty
            float timeSinceLastExecution = Time.time - action.lastExecutionTime;
            if (timeSinceLastExecution < action.timeCost * 2f)
            {
                score *= 0.5f; // Reduce score for recently executed actions
            }

            // Apply energy consideration - placeholder since HasEnergy doesn't exist
            // if (!context.agent.HasEnergy(action.energyCost))
            // {
            //     score *= 0.1f; // Heavily penalize if not enough energy
            // }

            return score;
        }

        private bool CanExecuteAction(MinipollCoreType agent, AIAction action)
        {
            // Check energy - placeholder since HasEnergy doesn't exist
            // if (!agent.HasEnergy(action.energyCost))
            //     return false;

            // Check cooldown
            if (Time.time - action.lastExecutionTime < action.timeCost)
                return false;

            // Additional checks based on action type
            switch (action.type)
            {
                case ActionType.Combat:
                    // Need a valid target
                    var context = agentContexts[agent];
                    if (context.target == null)
                        return false;
                    break;
            }

            return true;
        }

        #endregion

        #region Action Execution

        private ActionResult ExecuteEat(MinipollCoreType agent)
        {
            ActionResult result = new ActionResult();

            // Placeholder implementation - would use WorldResourceManager if it exists
            var needs = agent.GetComponent<Core.MinipollNeedsSystem>();
            if (needs != null)
            {
                // needs.FillNeed("Food", 0.5f);
                result.success = true;
                result.utilityGained = 0.5f;
            }
            else
            {
                result.success = false;
                result.failureReason = "No needs system found";
            }

            return result;
        }

        private ActionResult ExecuteDrink(MinipollCoreType agent)
        {
            ActionResult result = new ActionResult();

            var needs = agent.GetComponent<Core.MinipollNeedsSystem>();
            if (needs != null)
            {
                // needs.FillNeed("Water", 0.5f);
                result.success = true;
                result.utilityGained = 0.5f;
            }
            else
            {
                result.success = false;
                result.failureReason = "No needs system found";
            }

            return result;
        }

        private ActionResult ExecuteSleep(MinipollCoreType agent)
        {
            ActionResult result = new ActionResult();
            
            var needs = agent.GetComponent<Core.MinipollNeedsSystem>();
            if (needs != null)
            {
                // needs.FillNeed("Sleep", 0.1f);
                result.success = true;
                result.utilityGained = 0.1f;
            }

            return result;
        }

        private ActionResult ExecuteFlee(MinipollCoreType agent)
        {
            ActionResult result = new ActionResult();

            var context = agentContexts[agent];
            if (context.target != null)
            {
                // Move away from threat - placeholder since Move doesn't exist
                // Vector3 fleeDirection = (agent.transform.position - context.target.transform.position).normalized;
                // agent.Move(fleeDirection);

                result.success = true;
                result.utilityGained = 0.5f;
            }
            else
            {
                result.success = false;
                result.failureReason = "No threat to flee from";
            }

            return result;
        }

        private ActionResult ExecuteCommunicate(MinipollCoreType agent)
        {
            ActionResult result = new ActionResult();

            var context = agentContexts[agent];
            var social = agent.GetComponent<MinipollSocialRelations>();
            
            if (context.target != null && social != null)
            {
                var targetMinipoll = context.target.GetComponent<MinipollCoreType>();
                if (targetMinipoll != null)
                {
                    // Placeholder - would improve relationship
                    result.success = true;
                    result.utilityGained = 0.3f;
                }
            }
            else
            {
                result.success = false;
                result.failureReason = "No one to communicate with";
            }

            return result;
        }

        private ActionResult ExecuteShare(MinipollCoreType agent)
        {
            ActionResult result = new ActionResult();

            var context = agentContexts[agent];
            var needs = agent.GetComponent<Core.MinipollNeedsSystem>();
            
            if (context.target != null && needs != null)
            {
                // float foodLevel = needs.GetNormalizedNeed("Food");
                if (foodLevel > 0.7f)
                {
                    // Share food
                    // needs.FillNeed("Food", -0.2f);
                    
                    if (context.target.TryGetComponent<Core.MinipollNeedsSystem>(out var targetNeeds))
                    {
                        targetNeeds.FillNeed("Food", 0.2f);
                    }

                    result.success = true;
                    result.utilityGained = 0.4f;
                }
                else
                {
                    result.success = false;
                    result.failureReason = "Not enough resources to share";
                }
            }

            return result;
        }

        private ActionResult ExecuteExplore(MinipollCoreType agent)
        {
            ActionResult result = new ActionResult();

            // Random exploration - placeholder since Move doesn't exist
            Vector3 randomDirection = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                0,
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized;

            // agent.Move(randomDirection);

            result.success = true;
            result.utilityGained = 0.2f;

            return result;
        }

        private ActionResult ExecuteGather(MinipollCoreType agent)
        {
            ActionResult result = new ActionResult();

            // Placeholder implementation
            result.success = true;
            result.utilityGained = 0.3f;

            return result;
        }

        private ActionResult ExecuteAttack(MinipollCoreType agent)
        {
            ActionResult result = new ActionResult();

            var context = agentContexts[agent];
            if (context.target != null)
            {
                var targetMinipoll = context.target.GetComponent<MinipollCoreType>();
                if (targetMinipoll != null)
                {
                    float damage = 10f;
                    // targetMinipoll.TakeDamage(damage); // Placeholder

                    result.success = true;
                    result.utilityGained = 0.5f;
                    result.data["damage"] = damage;
                }
            }
            else
            {
                result.success = false;
                result.failureReason = "No valid target";
            }

            return result;
        }

        #endregion

        #region Helper Methods

        private void UpdateProximityInputs(MinipollCoreType agent, DecisionContext context)
        {
            float searchRadius = 15f;
            
            // Check for threats
            int threatCount = 0;
            GameObject closestThreat = null;
            float closestThreatDistance = float.MaxValue;

            // Check for friends
            int friendCount = 0;
            GameObject closestFriend = null;
            float closestFriendDistance = float.MaxValue;

            // Find all nearby entities
            // Collider[] nearby = Physics.OverlapSphere(agent.transform.position, searchRadius);
            
            foreach (var collider in nearby)
            {
                // if (collider.gameObject == agent.gameObject) continue;

                // var otherMinipoll = collider.GetComponent<MinipollCoreType>();
                if (otherMinipoll != null)
                {
                    // float distance = Vector3.Distance(agent.transform.position, otherMinipoll.transform.position);
                    
                    // Check if threat
                    if (IsThreat(agent, otherMinipoll))
                    {
                        threatCount++;
                        // if (Unity.VisualScripting.Vector2Distance < closestThreatDistance)
                        // {
                        //     closestThreatDistance = distance;
                        //     closestThreat = otherMinipoll.gameObject;
                        // }
                    }
                    // Check if friend
                    else
                    {
                        friendCount++;
                        if (distance < closestFriendDistance)
                        {
                            closestFriendDistance = distance;
                            // closestFriend = otherMinipoll.gameObject;
                        }
                    }
                }
            }

            // Update context inputs
            context.inputs[ConsiderationType.NearbyThreats] = threatCount > 0 ? 
                1f - (closestThreatDistance / searchRadius) : 0f;
            context.inputs[ConsiderationType.NearbyResources] = 0.5f; // Placeholder
            context.inputs[ConsiderationType.NearbyFriends] = Mathf.Clamp01(friendCount / 3f);
            context.inputs[ConsiderationType.GroupSize] = Mathf.Clamp01(friendCount / 10f);

            // Update target based on current action needs
            if (closestThreat != null && context.inputs[ConsiderationType.NearbyThreats] > 0.5f)
            {
                context.target = closestThreat;
            }
            else if (closestFriend != null)
            {
                context.target = closestFriend;
            }
        }

        private bool IsThreat(MinipollCoreType agent, object otherMinipoll)
        {
            throw new NotImplementedException();
        }

        private bool IsThreat(MinipollCoreType agent, MinipollCoreType other)
        {
            // Simple threat detection - placeholder implementation
            return false;
        }

        private float GetTimeOfDayValue()
        {
            // Placeholder implementation
            return 0.5f;
        }

        private float GetWeatherValue()
        {
            // Placeholder implementation
            return 0.5f;
        }

        private float GetTemperatureValue()
        {
            // Placeholder implementation
            return 0.5f;
        }

        private float GetAgeValue(MinipollCoreType agent)
        {
            // Placeholder implementation
            return 0.5f;
        }

        private float GetTypeWeight(ActionType type)
        {
            return type switch
            {
                ActionType.Survival => survivalWeight,
                ActionType.Social => socialWeight,
                ActionType.Emotional => emotionalWeight,
                _ => needsWeight
            };
        }

        private AIAction CloneAction(AIAction original)
        {
            return new AIAction
            {
                actionName = original.actionName,
                type = original.type,
                basePriority = original.basePriority,
                energyCost = original.energyCost,
                timeCost = original.timeCost,
                responseCurve = original.responseCurve,
                considerations = new List<Consideration>(original.considerations)
            };
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!debugMode || !Application.isPlaying) return;

            foreach (var kvp in agentContexts)
            {
                var agent = kvp.Key;
                var context = kvp.Value;

                if (agent == null) continue;

                // Draw target line
                if (context.target != null)
                {
                    Gizmos.color = Color.yellow;
                    // Gizmos.DrawLine(agent.transform.position, context.target.transform.position);
                }
            }
        }

        #endregion
    }

    public class MinipollCoreType
    {
        internal object name;
        internal object transform;

        internal object GetComponent<T>()
        {
            throw new NotImplementedException();
        }

        internal int GetInstanceID()
        {
            throw new NotImplementedException();
        }
    }
}