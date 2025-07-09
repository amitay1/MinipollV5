/**
 * This is part of the NEEDSIM Life simulation plug in for Unity, version 1.1
 * Copyright 2014 - 2017 Fantasieleben UG (haftungsbeschraenkt)
 *
 * http;//www.fantasieleben.com for further details.
 *
 * For questions please get in touch with Tilman Geishauser: tilman@fantasieleben.com
 */

using UnityEngine;
using System.Collections;

namespace NEEDSIM
{
    public class DecideClosestValue : Action
    {
        public DecideClosestValue(NEEDSIMNode agent)
            : base(agent)
        { }

        public override string Name
        {
            get
            {
                return "DecideClosestValue";
            }
        }

        /// <summary>
        /// Try to get a slot based on utility of all available slots (relative to the current satisfaction level of the needs of the agent).
        /// </summary>
        /// <returns>Success if a slot has been distributed to the agent.</returns>
        public override Action.Result Run()
        {
            if (agent.Blackboard.currentState == Blackboard.AgentState.ExitNEEDSIMBehaviors)
            {
                //Actions should be interrupted until the agent state is dealt with.
                return Result.Failure;
            }

            //If previously a slot was allocated, try to consume it.
            if (agent.Blackboard.currentState == Blackboard.AgentState.WaitingForSlot)
            {
                if (agent.AcceptSlot())
                {
                    return Result.Success;
                }
                else
                {
                    agent.Blackboard.currentState = Blackboard.AgentState.PonderingNextAction;
                }
            }

            //Try to allocate a slot to the agent that has the highest value regardless of state.
            //Simulation.Bidding.Result biddingResult = Simulation.Bidding.ValueOrientedBid(agent.AffordanceTreeNode);
            // Use the example callback
            Simulation.Bidding.Result biddingResult = Simulation.Bidding.ValueOrientedBid(agent.AffordanceTreeNode, preferShortestDistanceByAir);

            //Simulation.Bidding.BidParentGrandparentRoot(agent.AffordanceTreeNode);
            if (biddingResult == Simulation.Bidding.Result.Success)
            {
                agent.Blackboard.currentState = Blackboard.AgentState.WaitingForSlot;
                return Result.Running;
            }
            if (biddingResult == Simulation.Bidding.Result.OffersAreZero)
            {
                Debug.Log("No offers of value found.");
            }
            if(biddingResult == Simulation.Bidding.Result.OffersIsNull)
            {
                Debug.Log("No offers found.");
            }

            agent.Blackboard.currentState = Blackboard.AgentState.None;
            return Result.Failure;
        }

        public float preferShortestDistanceByAir(Simulation.AffordanceTreeNode caller, Simulation.Affordance good, string needName)
        {
            float distance = Vector3.Distance(caller.gameObject.transform.position, good.gameObject.transform.position);

            const float maxDistance = 3;

            return (maxDistance - distance);
        }
    }
}
