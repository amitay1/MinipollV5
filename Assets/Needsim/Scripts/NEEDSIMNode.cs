/**
 * This is part of the NEEDSIM Life simulation plug in for Unity, version 1.2.2
 * Copyright 2014 - 2021 Fantasieleben UG (haftungsbeschraenkt)
 *
 * http;//www.fantasieleben.com for further details.
 *
 * For questions please get in touch with Tilman Geishauser: tilman@fantasieleben.com
 */

using UnityEngine;
using Simulation;
using System.Collections;
using System.Collections.Generic;

namespace NEEDSIM
{
    /// <summary>
    /// Every object and agent in NEEDSIM Life simulation has a NEEDSIMNode: This is the essential component for using NEEDSIM Life simulation.
    /// </summary>
    [System.Serializable]
    public class NEEDSIMNode : MonoBehaviour
    {
        #region Fields set in the inspector view

        public string[] InteractionData;
        public bool isAgent;
        public float o_Space;
        public bool drawGizmosInGame;
        public bool ModifyLookAt;
        public Vector3[] slotPositionsWorldSpace;
        public Vector3[] slotPositionsLocalSpace;
        public Vector3[] slotLocalLookAtTarget;
        public bool[] auctionableBool;
        public bool showDebugInGame;
        public bool showDebugInInspector;
        public string speciesName;
        /// <summary>
        /// The selected plan is used by the example decision making method and is not needed in custom adaptions
        /// </summary>
        public ExamplePlans selectedPlan;
        public float[] StartingSatisfactionLevels_FloatValues;
        public string[] StartingSatisfactionLevels_NeedKeys;
        public bool SpecificSatisfactionLevelsAtStart;

        #endregion

        #region Agent

        public Blackboard Blackboard { get; set; }
        private IDecisionMaking decisionMaking; //This is an example controller for agents and can be replaced
        public bool runningInteractions = false;

        #endregion

        public delegate void MovementStartedByAgent();
        public event MovementStartedByAgent onMovementStartedByAgent;
        public delegate void InteractionStartedByAgent(string interactionName);
        /// <summary>
        /// This event can for example be used to trigger transitions in the animator with the same name, e.g. that for the interaction "Eat" a trigger named "Eat" is
        /// in the animation and is used to transition into a state or sub-state-machine that plays animation(s) for eating.
        /// </summary>
        public event InteractionStartedByAgent onInteractionStartedByAgent;

        /// <summary>
        /// The simulation object used in the NEEDSIM Simulation
        /// </summary>
        public AffordanceTreeNode AffordanceTreeNode { get; set; }

        //whether setup has already finished.
        private bool isSetUp = false;

        void Start()
        {
            if (!isSetUp)
            {
                Setup();
            }
        }

        /// <summary>
        /// Recursively build an Affordance Tree from the scene hierarchy. This method will not work for intermediate objects in the hierarchy - there is only deeper search if a direct ancestor is a NEEDSIMNode.
        /// </summary>
        public void BuildTreeBasedOnSceneHierarchy()
        {
            NEEDSIM.NEEDSIMNode[] children = GetComponentsInChildren<NEEDSIM.NEEDSIMNode>();
            Debug.Log(children.Length.ToString());

            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].transform.parent == this.transform)
                {
                    NEEDSIMRoot.Instance.AddNEEDSIMNode(children[i], this);
                    children[i].BuildTreeBasedOnSceneHierarchy();
                }
            }
        }

        #region setting up general NEEDSIM Node

        /// <summary>
        /// Setting up the AffordanceTreeNode
        /// </summary>
        /// <returns>false if the instance was already set up</returns>
        public bool Setup()
        {
            if (!isSetUp)
            {
                if (NEEDSIMRoot.Instance.IsSimulationInitialized)
                {
                    if (AffordanceTreeNode == null)
                    {
                        // Please use NEEDSIM.NEEDSIMRoot.Instance.AddNEEDSIMNode("your game object".GetComponent<NEEDSIM.NEEDSIMNode>()); where you created the object with a NÉEDSIM node
                        Debug.LogError(" AffordanceTreeNode not set up. (" + this.name + ")");
                    }
                    else
                    {
                        SetUpInteractions();
                        SetUpSlots();
                        ValidateSpecies();

                        if (isAgent)
                        {
                            SetUpAgent();
                        }
                    }
                }

                isSetUp = true;
            }

            return isSetUp;
        }


        void Update()
        {
            if (NEEDSIMRoot.Instance.IsSimulationInitialized && AffordanceTreeNode != null && !NEEDSIMRoot.Instance.IsPaused)
            {
                if (AffordanceTreeNode.Slots != null)
                {
                    //Positions are updated in case the object was moved at runtime.
                    foreach (Slot slot in AffordanceTreeNode.Slots)
                    {
                        slot.Position = transform.TransformPoint(slot.LocalPosition);
                        slot.LookAt = transform.TransformPoint(slot.LocalLookAt);
                    }


                    //Whether an interaction is currently running
                    runningInteractions = AffordanceTreeNode.Affordance.CurrentInteraction != null;

                    if (isAgent)
                    {
                        // This applies need change rates, for example a decay for the hunger need makes the agent more hungry over time.
                        AffordanceTreeNode.SatisfactionLevels.ApplyChangePerSecond();

                        if (decisionMaking != null)
                        {
                            //Now that the need satisfactions are updated let the decision making look at the new world state
                            decisionMaking.Update();
                        }
                    }

                }
            }
            else if (AffordanceTreeNode == null)
            {
                Debug.LogWarning("AffordanceTreeNode not yet set up.");
            }
        }

        /// <summary>
        /// If the simulation distributed a slot to this agent try to accept it.
        /// </summary>
        /// <returns>Whether the slot was accepted</returns>
        public bool AcceptSlot()
        {
            Slot slot = AffordanceTreeNode.AvailableSlot(true);

            if (slot == null || !isAgent)
            {
                return false;
            }

            bool slotAccepted = Blackboard.AcceptSlot(slot);
            if (slotAccepted)
            {
                //IF someone subscribed to the event let them know that the agent started to move
                onMovementStartedByAgent?.Invoke();
            }

            return slotAccepted;
        }

        /// <summary>
        /// This method tries to call the AgentArrivalEasy() method at the slot it is passed to.
        /// </summary>
        /// <param name="slot">The slot this agent arrives to</param>
        /// <returns>Whether the arrival at a slot by this agent was successful</returns>
        public bool ArrivalAtSlot(Slot slot)
        {
            // Only when the smart object is an agent it can arrive at slots that belong to other smart objects.
            if (slot == null || !isAgent)
            {
                return false;
            }

            Slot.Result result = slot.AgentArrivalEasy(this.AffordanceTreeNode);
            if (!(result == Slot.Result.Success))
            {
                if (result == Slot.Result.InteractionAlreadyRunning)
                {
                    //In some situations this might be useful debug information
                    //Debug.Log("No new interaction started - participating in currently running interaction.");
                }
                else
                {
                    Debug.LogWarning("agent arrival failure: " + slot.Position.ToString());
                    return false;
                }
            }

            //When arriving at a slot an interaction should be started, unless one was already running.
            if (slot.CurrentInteraction == null)
            {
                Debug.LogWarning("no interaction: " + slot.Position.ToString());
                return false;
            }

            Blackboard.currentState = Blackboard.AgentState.ParticipatingSlot;

            // IF someone subscribed to the event inform them that an interaction was started
            onInteractionStartedByAgent?.Invoke(slot.CurrentInteraction.Name);

            return true;
        }

        public bool DepartureFromSlot()
        {
            if (Blackboard == null || Blackboard.activeSlot == null)
            {
                return false;
            }

            Blackboard.activeSlot.AgentDeparture();
            Blackboard.activeSlot = null;
            return true;
        }
        /// <summary>
        /// Interrupt what this agent was doing and interrupt interactions with it
        /// </summary>
        public void InteruptInteractions()
        {
            if (NEEDSIMRoot.Instance != null && NEEDSIMRoot.Instance.IsSimulationInitialized)
            {
                //Only agents have blackboards. If they have active slots, they should be freed up for other agents.
                if (Blackboard != null && Blackboard.activeSlot != null)
                {
                    Blackboard.ExitNEEDSIMBehaviors();
                }

                if (AffordanceTreeNode != null)
                {
                    foreach (Slot slot in AffordanceTreeNode.Slots)
                    {
                        slot.InterruptInteraction();
                    }
                }
            }
        }

        /// <summary>
        /// Instead of destroying the component just deactivate it and remove it from its node from the simulation
        /// </summary>
        public void KillObject()
        {
            this.enabled = false;
            RemoveNEEDSIMNode();
        }

        void OnDestroy()
        {
            RemoveNEEDSIMNode();
        }

        void OnDisable()
        {
            AreSlotsAvailable(false);
        }
        void OnEnable()
        {
            AreSlotsAvailable(true);
        }

        private void SetUpInteractions()
        {
            foreach (string name in InteractionData)
            {
                if (name != "" && name != Simulation.Strings.None)
                {
                    if (!Manager.Instance.Data.InteractionByNameDictionary.ContainsKey(name))
                    {
                        Debug.LogError("At " + gameObject.name + ": Interaction " + name + " not found. Please check whether the correct database is selected in the SimulationManager.");
                    }
                    else
                    {
                        Interaction interaction = Manager.Instance.Data.InteractionByNameDictionary[name];

                        bool hasUnassignedInteraction = false;
                        foreach (string affectedNeed in interaction.SatisfactionRates.Keys)
                        {
                            if (affectedNeed == Simulation.Strings.AssignNeedLabel)
                            {
                                Debug.LogError("There is an unassigned need in the interaction " + interaction + ". The interaction" +
                                    "was not added to objects in the scene.");
                                hasUnassignedInteraction = true;
                            }
                        }
                        if (!hasUnassignedInteraction)
                        {
                            // Add the interaction to the simulation
                            AffordanceTreeNode.Affordance.AddInteraction(interaction);
                        }
                    }
                }
                else if (!isAgent)
                {
                    Debug.LogWarning("Unnamed Interaction at object that is not an agent. Please assign an interaction.");
                }
            }
        }

        private void SetUpSlots()
        {
            for (int i = 0; i < slotPositionsWorldSpace.Length; i++)
            {
                // Try to add the slot to the simulation
                if (!AffordanceTreeNode.AddSlot(slotPositionsWorldSpace[i], slotPositionsLocalSpace[i], slotLocalLookAtTarget[i], auctionableBool[i]))
                {
                    Debug.LogWarning("Slot not added");
                }
            }
        }

        private void ValidateSpecies()
        {
            if (isAgent && !(GameDataManager.SpeciesLoaded(speciesName)))
            {
                isAgent = false;
                Debug.LogWarning("Species not found.");
            }
        }

        #endregion

        #region wrapper for agent nodes

        public Simulation.Affordance AffordanceCurrentlyUsed
        {
            get
            {
                if (!isAgent)
                {
                    Debug.LogError("Only agents use affordances.");
                    return null;
                }
                //When starting to use an affodance the agents becomes a child of the node that the affordance belongs to
                return AffordanceTreeNode.Parent.Affordance;
            }
        }

        public Simulation.Needs Satisfaction
        {
            get
            {
                if (!isAgent)
                {
                    Debug.LogError("Only agents have their needs satisfied.");
                    return null;
                }
                return AffordanceTreeNode.SatisfactionLevels;
            }
        }

        #endregion

        private bool SetUpAgent()
        {
            if (!SpecificSatisfactionLevelsAtStart)
            {
                // Start with the agent's need at random levels
                AffordanceTreeNode.SatisfactionLevels.RandomizeValues();
            }
            else
            {
                if (StartingSatisfactionLevels_FloatValues.Length == 0 ||
   StartingSatisfactionLevels_NeedKeys.Length == 0)
                {
                    Debug.LogError("No start values set.");
                    return false;
                }

                Dictionary<string, float> result = new Dictionary<string, float>();

                for (int i = 0; i < StartingSatisfactionLevels_FloatValues.Length; i++)
                {
                    result.Add(StartingSatisfactionLevels_NeedKeys[i], StartingSatisfactionLevels_FloatValues[i]);
                }

                AffordanceTreeNode.SatisfactionLevels.SetSpecficSatisfactionLevels(result);
            }

            //Blackboard and planDemo are examples on how to control agents.
            //You might want use parts of the code for integration into your solution.
            Blackboard = new Blackboard(gameObject);

            decisionMaking = new PlanDemo(this);

            return true;
        }

        private void RemoveNEEDSIMNode()
        {
            if (NEEDSIMRoot.Instance != null && NEEDSIMRoot.Instance.IsSimulationInitialized)
            {
                //Only agents have blackboards. If they have active slots, they should be freed up for other agents.
                if (Blackboard != null && Blackboard.activeSlot != null)
                {
                    DepartureFromSlot();
                    NEEDSIMRoot.Instance.Blackboards.Remove(Blackboard);
                    Blackboard = null;
                }

                if (AffordanceTreeNode != null)
                {
                    foreach (Slot slot in AffordanceTreeNode.Slots)
                    {
                        slot.SetSlotBlocked();
                        slot.InterruptInteraction();
                    }

                    if (AffordanceTreeNode.Parent != null)
                    {
                        AffordanceTreeNode.Remove();
                    }
                }
            }
        }

        /// <summary>
        /// Makes slot available to others or unavailable
        /// </summary>
        /// <param name="value"></param>
        private void AreSlotsAvailable(bool value)
        {
            if (AffordanceTreeNode != null)
            {
                if (AffordanceTreeNode.Slots != null)
                {
                    foreach (Simulation.Slot slot in AffordanceTreeNode.Slots)
                    {
                        slot.IsAuctionable = value;
                    }
                }
            }
        }

        void OnGUI()
        {
            if (showDebugInGame && NEEDSIMRoot.Instance.IsSimulationInitialized && AffordanceTreeNode != null)
            {
                Vector3 GUIposition = Camera.main.WorldToScreenPoint(this.transform.position);
                int textAreaHeight = 50;
                string textAreaContent = speciesName + "\n\n";

                if (AffordanceTreeNode.Species == null)
                {
                    textAreaHeight += 15;
                    textAreaContent += "No species found";
                }
                else
                {
                    foreach (string need in AffordanceTreeNode.Species.needs)
                    {
                        try
                        {
                            textAreaContent += need + ": " + AffordanceTreeNode.SatisfactionLevels.GetValue(need).ToString("F2") + "\n";
                        }
                        catch (KeyNotFoundException)
                        {
                            Debug.LogError("Species data not in sync with Affordance Tree node.");
                        }
                        textAreaHeight += 20;
                    }

                    textAreaHeight += 25;
                    if (AffordanceTreeNode.Goal != null)
                    {
                        textAreaContent += "\nGoal: Get " + AffordanceTreeNode.Goal.NeedToSatisfy;
                        textAreaContent += "\n to state: " + AffordanceTreeNode.Goal.SatisfactionState.ToString();
                    }
                    else
                    {
                        textAreaContent += "\nNo goal specified.";
                    }

                    //planDemo is an optional tool for agent control
                    if (decisionMaking != null)
                    {
                        textAreaHeight += 15;
                        textAreaContent += "\nAction: " + decisionMaking.printCurrentAction();
                    }
                }

                textAreaContent = GUI.TextArea(new Rect(GUIposition.x, Screen.height - GUIposition.y - 120, 140, textAreaHeight), textAreaContent);
            }
        }

#if UNITY_EDITOR

        void OnDrawGizmos()
        {
            if (Application.isPlaying
                && drawGizmosInGame
                && NEEDSIMRoot.Instance.IsSimulationInitialized)
            {
                DrawSlots();
            }
        }

        private void DrawSlots()
        {
            foreach (Slot slot in AffordanceTreeNode.Slots)
            {
                switch (slot.SlotState)
                {
                    case Slot.SlotStates.ReadyForAuction:
                        Gizmos.color = GeneralSettings.AuctionableSlotColor;
                        break;
                    case Slot.SlotStates.Blocked:
                        Gizmos.color = GeneralSettings.BlockedSlotColor;
                        break;
                    case Slot.SlotStates.ReadyCharacter:
                        Gizmos.color = GeneralSettings.ReadyCharacterSlotColor;
                        break;
                    case Slot.SlotStates.Reserved:
                        Gizmos.color = GeneralSettings.ReservedSlotColor;
                        break;
                    default:
                        Gizmos.color = GeneralSettings.AuctionableSlotColor;
                        break;
                }

                Gizmos.DrawWireSphere(slot.Position, GeneralSettings.SlotRepresentationRadius);
            }
        }
#endif
    }
}