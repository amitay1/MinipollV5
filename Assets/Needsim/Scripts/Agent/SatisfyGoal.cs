/**
 * This is part of the NEEDSIM Life simulation plug in for Unity, version 1.2.1
 * Copyright 2014 - 2017 Fantasieleben Inh. Tilman Geishauser
 *
 * http;//www.fantasieleben.com for further details.
 *
 * For questions please get in touch with Tilman Geishauser: tilman@fantasieleben.com
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NEEDSIM
{
    /// <summary>
    /// Participate in a slot to satisfy a goal.
    /// </summary>
    public class SatisfyGoal : Action
    {
        public SatisfyGoal(NEEDSIMNode agent)
            : base(agent)
        { }

        public override string Name
        {
            get
            {
                return "SatisfyGoal";
            }
        }

        /// <summary>
        /// Satisfying a goal at an AffordanceTree node.
        /// </summary>
        /// <returns>Success if need satsifaction goal was achieved, running whilst it is being satisfied. </returns>
        public override Action.Result Run()
        {
            if (agent.Blackboard.currentState == Blackboard.AgentState.ExitNEEDSIMBehaviors)
            {
                //Actions should be interrupted until the agent state is dealt with.
                return Result.Failure;
            }

            if(agent.Blackboard.activeSlot == null)
            {
                return Result.Failure;
            }

            if (agent.Blackboard.activeSlot.SlotState == Simulation.Slot.SlotStates.Blocked)
            {
                agent.DepartureFromSlot();
                return Result.Failure;
            }

            //Check whether the current interaction is still running.
            bool interactionStillRunning = false;
            if (!agent.AffordanceCurrentlyUsed.InteractionStartedThisFrame
                && agent.AffordanceCurrentlyUsed.CurrentInteraction != null)
            {
                interactionStillRunning = true;
            }

            //If goal is achieved get ready for next action
            if (!interactionStillRunning
                && agent.AffordanceTreeNode.Goal.HasBeenAchieved)
            {
                //If you do not want agents to stay at the same slot until their type of goal is finished you can just go to the else case
                Simulation.Goal newGoal = agent.Satisfaction.GoalToSatisfyLowestNeed();
                if (newGoal.NeedToSatisfy == agent.AffordanceTreeNode.Goal.NeedToSatisfy)
                {
                    agent.AffordanceTreeNode.Goal = newGoal;
                }
                else
                {
                    agent.DepartureFromSlot();
                    agent.Blackboard.currentState = Blackboard.AgentState.PonderingNextAction;
                    return Result.Success;
                }
            }

            //Participate in current interaction or start a new one
            if (agent.AffordanceCurrentlyUsed.CurrentInteraction != null)
            {
                agent.AffordanceTreeNode.ApplyParentInteraction();
                agent.Blackboard.currentState = Blackboard.AgentState.ParticipatingSlot;
            }
            else
            {
                Simulation.Slot.Result result
                    = agent.AffordanceCurrentlyUsed.StartRandomInteraction();
                if (result != Simulation.Slot.Result.Success)
                {
                    Debug.LogError(result);
                }
            }

            return Result.Running;
        }
    }
}