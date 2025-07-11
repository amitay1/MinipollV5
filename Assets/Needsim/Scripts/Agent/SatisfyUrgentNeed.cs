﻿/**
 * This is part of the NEEDSIM Life simulation plug in for Unity, version 1.1
 * Copyright 2014 - 2017 Fantasieleben UG (haftungsbeschraenkt)
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
    /// Participating a slot. The respective behavior for value/urgency oriented behaviors.
    /// </summary>
    public class SatisfyUrgentNeed : Action
    {
        public SatisfyUrgentNeed(NEEDSIMNode agent)
            : base(agent)
        { }

        public override string Name
        {
            get
            {
                return "SatisfyUrgentNeed";
            }
        }

        /// <summary>
        /// Participating a slot. The respective behavior for value/urgency oriented behaviors.
        /// </summary>
        /// <returns>TODO</returns>
        public override Action.Result Run()
        {
            if (agent.Blackboard.currentState == Blackboard.AgentState.ExitNEEDSIMBehaviors)
            {
                //Actions should be interrupted until the agent state is dealt with.
                agent.DepartureFromSlot();
                return Result.Failure;
            }

            if (agent.Blackboard.activeSlot == null)
            {
                return Result.Failure;
            }

            if (agent.Blackboard.activeSlot.SlotState == Simulation.Slot.SlotStates.Blocked)
            {
                agent.DepartureFromSlot();
                return Result.Failure;
            }

            //Check whether the current interaction is still running. This is in a seperate block as it 
            // might be edited for some usage scenarios.
            bool interactionStillRunning = false;
            if (!agent.AffordanceCurrentlyUsed.InteractionStartedThisFrame
                && agent.AffordanceCurrentlyUsed.CurrentInteraction != null)
            {
                interactionStillRunning = true;
                agent.Blackboard.LastInteractionSatiesfiedUrgentNeed = agent.AffordanceTreeNode.CurrentInteractionSatisfiesMostUrgentNeed;
            }

            if (!interactionStillRunning)
            {
                //If the agent has a new most urgent need it is time to go satisfy it
                if (!agent.Blackboard.LastInteractionSatiesfiedUrgentNeed)
                {
                    agent.DepartureFromSlot();
                    agent.Blackboard.currentState = Blackboard.AgentState.PonderingNextAction;

                    return Result.Success;
                }
                //The following causes the agent to stay at the slot until another need becomes most urgent.
                else
                {
                    Simulation.Slot.Result result =
                        agent.AffordanceCurrentlyUsed.ProlongLastInteraction();
                    if (result != Simulation.Slot.Result.Success)
                    {
                        agent.AffordanceCurrentlyUsed.StartRandomInteraction();
                    }
                    return Result.Running;
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
                agent.AffordanceCurrentlyUsed.StartRandomInteraction();
            }

            return Result.Running;
        }
    }
}