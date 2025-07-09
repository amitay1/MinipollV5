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
    /// <summary>
    /// This is an alternative to the Action DecideGoal. It shows how to replace the default evaluation with a custom method by using a callback.
    /// 
    /// For goal oriented behaviors: Get a goal from the simulation, and try to get a slot where the goal can be satisfied.
    /// </summary>
    public class DecideClosestGoal : Action
    {
        public DecideClosestGoal(NEEDSIMNode agent)
            : base(agent)
        { }

        public override string Name
        {
            get
            {
                return "DecideClosestGoal";
            }
        }

        /// <summary>
        /// Get a goal from the simulation, and try to get a slot where the goal can be satisfied.
        /// </summary>
        /// <returns>Success if a slot has been distributed to the agent.</returns>
        public override Action.Result Run()
        {
            if (agent.Blackboard.currentState == Blackboard.AgentState.ExitNEEDSIMBehaviors)
            {
                //Actions should be interrupted until the agent state is dealt with.
                return Result.Failure;
            }

            //Get the goal to satisfy the need with the lowest satisfaction. You can replace the goals for your specific game.
            agent.AffordanceTreeNode.Goal = agent.Satisfaction.GoalToSatisfyLowestNeed();

            //If previously a slot was allocated to this agent, try to consume/use it.
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

            //Try to allocate a slot to the agent that will satisfy the goal.
            // Simple solution
            // Simulation.Bidding.Result biddingResult = Simulation.Bidding.GoalOrientedBid(agent.AffordanceTreeNode);
            // You can instead use callbacks to adjust evaluataion of the slots to your game.
            Simulation.Bidding.Result biddingResult = Simulation.Bidding.GoalOrientedBid(agent.AffordanceTreeNode, preferShortestDistanceByAir);

            if (biddingResult == Simulation.Bidding.Result.Success)
            {
                agent.Blackboard.currentState = Blackboard.AgentState.WaitingForSlot;
                return Result.Running;
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
