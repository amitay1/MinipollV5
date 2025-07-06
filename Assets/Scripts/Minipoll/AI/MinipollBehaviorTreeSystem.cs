using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MinipollGame.Core;
using MinipollGame.Systems.Core;
using MinipollGame.Managers;

/// <summary>
/// BehaviorTreeManager - מנהל את מערכת ה-Behavior Trees לכל המיניפולים
/// יוצר, מנהל ומריץ behavior trees מותאמות אישית לכל מיניפול
/// </summary>
namespace MinipollGame.AI
{
    public class BehaviorTreeManager : MonoBehaviour
    {
        #region Singleton
        public static BehaviorTreeManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Settings
        [Header("=== Behavior Tree Settings ===")]
        [SerializeField] private bool enableBehaviorTrees = true;
        [SerializeField] private float updateInterval = 0.1f; // עדכון כל 0.1 שניות
        [SerializeField] private int maxTreesPerFrame = 10; // מקסימום עצים לעדכן בפריים
        
        [Header("=== Performance ===")]
        [SerializeField] private bool usePerformanceOptimization = true;
        [SerializeField] private float distanceBasedOptimization = 50f;
        [SerializeField] private bool pauseDistantTrees = true;
        
        [Header("=== Debug ===")]
        [SerializeField] private bool verboseLogging = false;
        [SerializeField] private bool showTreeGizmos = false;
        [SerializeField] private MinipollBrain debugTarget;
        #endregion

        #region Data
        private Dictionary<MinipollBrain, BehaviorTree> activeTrees = new Dictionary<MinipollBrain, BehaviorTree>();
        private Dictionary<string, BehaviorTreeTemplate> treeTemplates = new Dictionary<string, BehaviorTreeTemplate>();
        private List<BehaviorTree> treesToUpdate = new List<BehaviorTree>();
        
        private float nextUpdateTime;
        private int updateIndex = 0;
        private Camera playerCamera;
        #endregion

        #region Events
        public static event Action<MinipollBrain, BehaviorTree> OnTreeCreated;
        public static event Action<MinipollBrain, BehaviorTree> OnTreeRemoved;
        public static event Action<BehaviorTree, NodeResult> OnTreeExecuted;
        #endregion

        #region Initialization
        private void InitializeManager()
        {
            nextUpdateTime = Time.time + updateInterval;
            playerCamera = Camera.main;
            
            CreateDefaultTemplates();
            
            if (verboseLogging)
                Debug.Log("[BehaviorTreeManager] Initialized successfully");
        }

        private void Start()
        {
            // רשום עצים עבור מיניפולים קיימים
            RegisterExistingMinipolls();
        }

        private void CreateDefaultTemplates()
        {
            // Basic Adult Template
            var adultTemplate = new BehaviorTreeTemplate("Adult")
            {
                rootNode = CreateAdultBehaviorTree()
            };
            treeTemplates["Adult"] = adultTemplate;

            // Baby Template
            var babyTemplate = new BehaviorTreeTemplate("Baby")
            {
                rootNode = CreateBabyBehaviorTree()
            };
            treeTemplates["Baby"] = babyTemplate;

            // Child Template
            var childTemplate = new BehaviorTreeTemplate("Child")
            {
                rootNode = CreateChildBehaviorTree()
            };
            treeTemplates["Child"] = childTemplate;

            // Elder Template
            var elderTemplate = new BehaviorTreeTemplate("Elder")
            {
                rootNode = CreateElderBehaviorTree()
            };
            treeTemplates["Elder"] = elderTemplate;

            if (verboseLogging)
                Debug.Log($"[BehaviorTreeManager] Created {treeTemplates.Count} behavior tree templates");
        }
        #endregion

        #region Update System
        private void Update()
        {
            if (!enableBehaviorTrees || Time.time < nextUpdateTime)
                return;

            UpdateBehaviorTrees();
            nextUpdateTime = Time.time + updateInterval;
        }

        private void UpdateBehaviorTrees()
        {
            if (activeTrees.Count == 0)
                return;

            // עדכן רק חלק מהעצים כל פריים לביצועים טובים יותר
            int treesToUpdateThisFrame = usePerformanceOptimization ? 
                Mathf.Min(maxTreesPerFrame, activeTrees.Count) : 
                activeTrees.Count;

            var allTrees = activeTrees.Values.ToList();
            
            for (int i = 0; i < treesToUpdateThisFrame; i++)
            {
                int treeIndex = (updateIndex + i) % allTrees.Count;
                var tree = allTrees[treeIndex];
                
                if (ShouldUpdateTree(tree))
                {
                    var result = tree.Execute();
                    OnTreeExecuted?.Invoke(tree, result);
                    
                    if (verboseLogging && debugTarget != null && tree.owner == debugTarget)
                    {
                        Debug.Log($"[BehaviorTree] {tree.owner.Name}: {result}");
                    }
                }
            }

            updateIndex = (updateIndex + treesToUpdateThisFrame) % activeTrees.Count;
        }

        private bool ShouldUpdateTree(BehaviorTree tree)
        {
            if (!tree.isActive || tree.owner == null || !tree.owner.IsAlive)
                return false;

            // אופטימיזציה מבוססת מרחק
            if (usePerformanceOptimization && pauseDistantTrees && playerCamera != null)
            {
                float distance = Vector3.Distance(playerCamera.transform.position, tree.owner.transform.position);
                if (distance > distanceBasedOptimization)
                {
                    return UnityEngine.Random.value < 0.1f; // עדכן 10% מהעצים הרחוקים
                }
            }

            return true;
        }
        #endregion

        #region Tree Management
        public BehaviorTree CreateBehaviorTree(MinipollBrain minipoll)
        {
            if (minipoll == null || activeTrees.ContainsKey(minipoll))
                return null;

            // בחר תבנית לפי גיל
            string templateName = GetTemplateNameForMinipoll(minipoll);
            
            if (!treeTemplates.ContainsKey(templateName))
            {
                Debug.LogError($"[BehaviorTreeManager] No template found for: {templateName}");
                templateName = "Adult"; // fallback
            }

            var template = treeTemplates[templateName];
            var behaviorTree = new BehaviorTree(minipoll, template.rootNode.Clone());
            
            activeTrees[minipoll] = behaviorTree;
            
            if (verboseLogging)
                Debug.Log($"[BehaviorTreeManager] Created {templateName} behavior tree for {minipoll.Name}");

            OnTreeCreated?.Invoke(minipoll, behaviorTree);
            return behaviorTree;
        }

        public void RemoveBehaviorTree(MinipollBrain minipoll)
        {
            if (minipoll == null || !activeTrees.ContainsKey(minipoll))
                return;

            var tree = activeTrees[minipoll];
            activeTrees.Remove(minipoll);
            
            if (verboseLogging)
                Debug.Log($"[BehaviorTreeManager] Removed behavior tree for {minipoll.Name}");

            OnTreeRemoved?.Invoke(minipoll, tree);
        }

        public BehaviorTree GetBehaviorTree(MinipollBrain minipoll)
        {
            return activeTrees.ContainsKey(minipoll) ? activeTrees[minipoll] : null;
        }

        public void RefreshBehaviorTree(MinipollBrain minipoll)
        {
            if (minipoll == null)
                return;

            RemoveBehaviorTree(minipoll);
            CreateBehaviorTree(minipoll);
        }

       private string GetTemplateNameForMinipoll(MinipollBrain minipoll)
{
    Core.MinipollCore core = minipoll.GetCore();            if (core == null)
                return "Adult";

            // Convert by string comparison to avoid namespace conflicts
            return core.CurrentAgeStage.ToString() switch
            {
                "Baby" => "Baby",
                "Child" => "Child", 
                "Adult" => "Adult",
                "Elder" => "Elder",
                _ => "Adult"
            };
}
        private void RegisterExistingMinipolls()
        {
            var allMinipolls = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None);
            foreach (var minipoll in allMinipolls)
            {
                if (minipoll.IsAlive)
                {
                    CreateBehaviorTree(minipoll);
                }
            }
            
            if (verboseLogging)
                Debug.Log($"[BehaviorTreeManager] Registered {allMinipolls.Length} existing minipolls");
        }
        #endregion

        #region Template Creation
        private BehaviorNode CreateAdultBehaviorTree()
        {
            // עץ התנהגות למבוגר: Selector שבוחר בין צרכים שונים
            return new SelectorNode("Adult_Root", new List<BehaviorNode>
            {
                // בדיקת סכנה (עדיפות גבוהה)
                new SequenceNode("Danger_Check", new List<BehaviorNode>
                {
                    new ConditionNode("Is_In_Danger", (brain) => CheckForDanger(brain)),
                    new ActionNode("Escape", (brain) => ExecuteEscape(brain))
                }),

                // צרכים בסיסיים
                new SequenceNode("Basic_Needs", new List<BehaviorNode>
                {
                    new ConditionNode("Is_Hungry", (brain) => CheckHunger(brain)),
                    new ActionNode("Find_Food", (brain) => ExecuteFindFood(brain))
                }),

                new SequenceNode("Thirst_Need", new List<BehaviorNode>
                {
                    new ConditionNode("Is_Thirsty", (brain) => CheckThirst(brain)),
                    new ActionNode("Find_Water", (brain) => ExecuteFindWater(brain))
                }),

                new SequenceNode("Energy_Need", new List<BehaviorNode>
                {
                    new ConditionNode("Is_Tired", (brain) => CheckTiredness(brain)),
                    new ActionNode("Rest", (brain) => ExecuteRest(brain))
                }),

                // פעילויות חברתיות
                new SequenceNode("Social_Activities", new List<BehaviorNode>
                {
                    new ConditionNode("Wants_Social", (brain) => CheckSocialNeed(brain)),
                    new ActionNode("Socialize", (brain) => ExecuteSocialize(brain))
                }),

                // ברירת מחדל - חקור
                new ActionNode("Explore", (brain) => ExecuteExplore(brain))
            });
        }

        private BehaviorNode CreateBabyBehaviorTree()
        {
            // עץ פשוט יותר לתינוקות - בעיקר צרכים בסיסיים
            return new SelectorNode("Baby_Root", new List<BehaviorNode>
            {
                new SequenceNode("Cry_For_Help", new List<BehaviorNode>
                {
                    new ConditionNode("Needs_Help", (brain) => CheckBabyNeeds(brain)),
                    new ActionNode("Cry", (brain) => ExecuteCry(brain))
                }),

                new SequenceNode("Sleep_Need", new List<BehaviorNode>
                {
                    new ConditionNode("Very_Tired", (brain) => CheckBabyTiredness(brain)),
                    new ActionNode("Sleep", (brain) => ExecuteSleep(brain))
                }),

                new ActionNode("Play", (brain) => ExecutePlay(brain))
            });
        }

        private BehaviorNode CreateChildBehaviorTree()
        {
            // עץ לילדים - יותר משחק וחקירה
            return new SelectorNode("Child_Root", new List<BehaviorNode>
            {
                new SequenceNode("Basic_Needs", new List<BehaviorNode>
                {
                    new ConditionNode("Is_Hungry", (brain) => CheckHunger(brain)),
                    new ActionNode("Find_Food", (brain) => ExecuteFindFood(brain))
                }),

                new SequenceNode("Play_Activities", new List<BehaviorNode>
                {
                    new ConditionNode("Wants_To_Play", (brain) => CheckPlayMood(brain)),
                    new SelectorNode("Play_Options", new List<BehaviorNode>
                    {
                        new ActionNode("Play_With_Others", (brain) => ExecutePlayWithOthers(brain)),
                        new ActionNode("Explore_Curiously", (brain) => ExecuteCuriousExplore(brain))
                    })
                }),

                new ActionNode("Follow_Adults", (brain) => ExecuteFollowAdults(brain))
            });
        }

        private BehaviorNode CreateElderBehaviorTree()
        {
            // עץ לזקנים - יותר מנוחה ופחות פעילות
            return new SelectorNode("Elder_Root", new List<BehaviorNode>
            {
                new SequenceNode("Health_Priority", new List<BehaviorNode>
                {
                    new ConditionNode("Poor_Health", (brain) => CheckElderHealth(brain)),
                    new ActionNode("Seek_Care", (brain) => ExecuteSeekCare(brain))
                }),

                new SequenceNode("Rest_Often", new List<BehaviorNode>
                {
                    new ConditionNode("Needs_Rest", (brain) => CheckElderRest(brain)),
                    new ActionNode("Rest_Long", (brain) => ExecuteLongRest(brain))
                }),

                new SequenceNode("Share_Wisdom", new List<BehaviorNode>
                {
                    new ConditionNode("Others_Nearby", (brain) => CheckOthersNearby(brain)),
                    new ActionNode("Teach", (brain) => ExecuteTeach(brain))
                }),

                new ActionNode("Peaceful_Wander", (brain) => ExecutePeacefulWander(brain))
            });
        }
        #endregion

        #region Condition Methods
       public bool CheckForDanger(MinipollGame.Core.MinipollBrain brain)
{
    if (brain == null)
        return false;
    
    // בדיקות בסיסיות לסכנה
    bool lowHealth = brain.health < 25f;
    bool criticalEnergy = brain.energy < 10f;
    bool veryHungry = brain.hunger > 90f;
    bool veryThirsty = brain.thirst > 90f;
    
    // החזרת true אם יש סכנה כלשהי
    return lowHealth || criticalEnergy || veryHungry || veryThirsty;
}        private bool CheckHunger(MinipollBrain brain)
        {
            var needsObj = brain.GetNeedsSystem();
            var needs = needsObj as MinipollGame.Core.MinipollNeedsSystem;
            return needs != null && needs.GetNeedValue(NeedType.Hunger) < 30f;
        }        private bool CheckThirst(MinipollBrain brain)
        {
            var needsObj = brain.GetNeedsSystem();
            var needs = needsObj as MinipollGame.Core.MinipollNeedsSystem;
            return needs != null && needs.GetNeedValue(NeedType.Thirst) < 40f;
        }        private bool CheckTiredness(MinipollBrain brain)
        {
            var needsObj = brain.GetNeedsSystem();
            var needs = needsObj as MinipollGame.Core.MinipollNeedsSystem;
            return needs != null && needs.GetNeedValue(NeedType.Sleep) < 25f;
        }        private bool CheckSocialNeed(MinipollBrain brain)
        {
            var needsObj = brain.GetNeedsSystem();
            var needs = needsObj as MinipollGame.Core.MinipollNeedsSystem;
            return needs != null && needs.GetNeedValue(NeedType.Social) < 50f;
        }        private bool CheckBabyNeeds(MinipollBrain brain)
        {
            var needsObj = brain.GetNeedsSystem();
            var needs = needsObj as MinipollGame.Core.MinipollNeedsSystem;
            return needs != null && (
                needs.GetNeedValue(NeedType.Hunger) < 50f || 
                needs.GetNeedValue(NeedType.Comfort) < 30f
            );
        }        private bool CheckBabyTiredness(MinipollBrain brain)
        {
            var needsObj = brain.GetNeedsSystem();
            var needs = needsObj as MinipollGame.Core.MinipollNeedsSystem;
            return needs != null && needs.GetNeedValue(NeedType.Sleep) < 20f;
        }

        private bool CheckPlayMood(MinipollBrain brain)
        {
            var emotions = brain.GetEmotionsSystem();
            return emotions != null && UnityEngine.Random.value < 0.7f; // ילדים אוהבים לשחק
        }

        private bool CheckElderHealth(MinipollBrain brain)
        {
            return brain.Health < 50f;
        }        private bool CheckElderRest(MinipollBrain brain)
        {
            var needsObj = brain.GetNeedsSystem();
            var needs = needsObj as MinipollGame.Core.MinipollNeedsSystem;
            return needs != null && needs.GetNeedValue(NeedType.Sleep) < 60f; // זקנים צריכים יותר מנוחה
        }

        private bool CheckOthersNearby(MinipollBrain brain)
        {
            var nearbyMinipolls = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None)
                .Where(m => m != brain && Vector3.Distance(m.transform.position, brain.transform.position) < 10f);
            return nearbyMinipolls.Any();
        }
        #endregion

        #region Action Methods
        private NodeResult ExecuteEscape(MinipollBrain brain)
        {
            brain.ForceGoal("Escape");
            return NodeResult.Success;
        }

        private NodeResult ExecuteFindFood(MinipollBrain brain)
        {
            brain.ForceGoal("FindFood");
            return NodeResult.Success;
        }

        private NodeResult ExecuteFindWater(MinipollBrain brain)
        {
            brain.ForceGoal("FindWater");
            return NodeResult.Success;
        }

        private NodeResult ExecuteRest(MinipollBrain brain)
        {
            brain.ForceGoal("Rest");
            return NodeResult.Success;
        }

        private NodeResult ExecuteSocialize(MinipollBrain brain)
        {
            brain.ForceGoal("Socialize");
            return NodeResult.Success;
        }

        private NodeResult ExecuteExplore(MinipollBrain brain)
        {
            brain.ForceGoal("Explore");
            return NodeResult.Success;
        }

        private NodeResult ExecuteCry(MinipollBrain brain)
        {
            // תינוק בוכה
            if (verboseLogging)
                Debug.Log($"[BehaviorTree] {brain.Name} is crying for help!");
            return NodeResult.Success;
        }

        private NodeResult ExecuteSleep(MinipollBrain brain)
        {
            brain.ForceGoal("Rest");
            return NodeResult.Success;
        }

        private NodeResult ExecutePlay(MinipollBrain brain)
        {
            if (verboseLogging)
                Debug.Log($"[BehaviorTree] {brain.Name} is playing!");
            return NodeResult.Success;
        }

        private NodeResult ExecutePlayWithOthers(MinipollBrain brain)
        {
            brain.ForceGoal("Socialize");
            return NodeResult.Success;
        }

        private NodeResult ExecuteCuriousExplore(MinipollBrain brain)
        {
            brain.ForceGoal("Explore");
            return NodeResult.Success;
        }

        private NodeResult ExecuteFollowAdults(MinipollBrain brain)
        {
            // ילד עוקב אחרי מבוגרים
            brain.ForceGoal("Socialize");
            return NodeResult.Success;
        }

        private NodeResult ExecuteSeekCare(MinipollBrain brain)
        {
            brain.ForceGoal("Socialize"); // חפש עזרה מאחרים
            return NodeResult.Success;
        }

        private NodeResult ExecuteLongRest(MinipollBrain brain)
        {
            brain.ForceGoal("Rest");
            return NodeResult.Success;
        }

        private NodeResult ExecuteTeach(MinipollBrain brain)
        {
            brain.ForceGoal("Socialize"); // שתף חוכמה
            return NodeResult.Success;
        }

        private NodeResult ExecutePeacefulWander(MinipollBrain brain)
        {
            brain.ForceGoal("Explore");
            return NodeResult.Success;
        }
        #endregion

        #region Public API
        public void RegisterMinipoll(MinipollBrain minipoll)
        {
            if (minipoll != null && !activeTrees.ContainsKey(minipoll))
            {
                CreateBehaviorTree(minipoll);
            }
        }

        public void UnregisterMinipoll(MinipollBrain minipoll)
        {
            RemoveBehaviorTree(minipoll);
        }

        public void SetEnabled(bool enabled)
        {
            enableBehaviorTrees = enabled;
        }

        public void PauseTree(MinipollBrain minipoll)
        {
            var tree = GetBehaviorTree(minipoll);
            if (tree != null)
                tree.isActive = false;
        }

        public void ResumeTree(MinipollBrain minipoll)
        {
            var tree = GetBehaviorTree(minipoll);
            if (tree != null)
                tree.isActive = true;
        }

        public int GetActiveTreesCount()
        {
            return activeTrees.Count;
        }

        public List<BehaviorTree> GetAllActiveTrees()
        {
            return activeTrees.Values.ToList();
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            if (!showTreeGizmos || !Application.isPlaying)
                return;

            foreach (var tree in activeTrees.Values)
            {
                if (tree.owner != null)
                {
                    Gizmos.color = tree.isActive ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(tree.owner.transform.position + Vector3.up * 2f, 0.5f);

#if UNITY_EDITOR
                    UnityEditor.Handles.Label(tree.owner.transform.position + Vector3.up * 3f,
                        $"Tree: {tree.templateName}\nActive: {tree.isActive}");
#endif
                }
            }
        }
        #endregion
    }

    #region Behavior Tree Classes
    public enum NodeResult
    {
        Success,
        Failure,
        Running
    }

    [System.Serializable]
    public class BehaviorTree
    {
        public MinipollBrain owner;
        public BehaviorNode rootNode;
        public bool isActive = true;
        public string templateName;
        public float lastExecutionTime;

        public BehaviorTree(MinipollBrain owner, BehaviorNode rootNode)
        {
            this.owner = owner;
            this.rootNode = rootNode;
            this.templateName = owner.GetCore()?.CurrentAgeStage.ToString() ?? "Unknown";
        }

        public NodeResult Execute()
        {
            if (!isActive || owner == null || !owner.IsAlive)
                return NodeResult.Failure;

            lastExecutionTime = Time.time;
            return rootNode.Execute(owner);
        }
    }

    [System.Serializable]
    public abstract class BehaviorNode
    {
        public string nodeName;

        public BehaviorNode(string name)
        {
            nodeName = name;
        }

        public abstract NodeResult Execute(MinipollBrain brain);
        public abstract BehaviorNode Clone();
    }

    public class ActionNode : BehaviorNode
    {
        private Func<MinipollBrain, NodeResult> action;

        public ActionNode(string name, Func<MinipollBrain, NodeResult> action) : base(name)
        {
            this.action = action;
        }

        public override NodeResult Execute(MinipollBrain brain)
        {
            return action?.Invoke(brain) ?? NodeResult.Failure;
        }

        public override BehaviorNode Clone()
        {
            return new ActionNode(nodeName, action);
        }
    }

    public class ConditionNode : BehaviorNode
    {
        private Func<MinipollBrain, bool> condition;

        public ConditionNode(string name, Func<MinipollBrain, bool> condition) : base(name)
        {
            this.condition = condition;
        }

        public override NodeResult Execute(MinipollBrain brain)
        {
            return condition?.Invoke(brain) == true ? NodeResult.Success : NodeResult.Failure;
        }

        public override BehaviorNode Clone()
        {
            return new ConditionNode(nodeName, condition);
        }
    }

    public class SelectorNode : BehaviorNode
    {
        private List<BehaviorNode> children;

        public SelectorNode(string name, List<BehaviorNode> children) : base(name)
        {
            this.children = children ?? new List<BehaviorNode>();
        }

        public override NodeResult Execute(MinipollBrain brain)
        {
            foreach (var child in children)
            {
                var result = child.Execute(brain);
                if (result == NodeResult.Success)
                    return NodeResult.Success;
                if (result == NodeResult.Running)
                    return NodeResult.Running;
            }
            return NodeResult.Failure;
        }

        public override BehaviorNode Clone()
        {
            var clonedChildren = children.Select(child => child.Clone()).ToList();
            return new SelectorNode(nodeName, clonedChildren);
        }
    }

    public class SequenceNode : BehaviorNode
    {
        private List<BehaviorNode> children;

        public SequenceNode(string name, List<BehaviorNode> children) : base(name)
        {
            this.children = children ?? new List<BehaviorNode>();
        }

        public override NodeResult Execute(MinipollBrain brain)
        {
            foreach (var child in children)
            {
                var result = child.Execute(brain);
                if (result == NodeResult.Failure)
                    return NodeResult.Failure;
                if (result == NodeResult.Running)
                    return NodeResult.Running;
            }
            return NodeResult.Success;
        }

        public override BehaviorNode Clone()
        {
            var clonedChildren = children.Select(child => child.Clone()).ToList();
            return new SequenceNode(nodeName, clonedChildren);
        }
    }

    [System.Serializable]
    public class BehaviorTreeTemplate
    {
        public string templateName;
        public BehaviorNode rootNode;

        public BehaviorTreeTemplate(string name)
        {
            templateName = name;
        }
    }
    #endregion
}