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
    /// For goal oriented behaviors: Get a goal from the simulation, and try to get a slot where the goal can be satisfied.
    /// </summary>
    public class DecideGoal : Action
    {
        public DecideGoal(NEEDSIMNode agent)
            : base(agent)
        { }

        public override string Name
        {
            get
            {
                return "DecideGoal";
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

            // Example for setting a specific goal.
            //agent.AffordanceTreeNode.Goal = new Simulation.Goal("ExampleNeedName", Simulation.Needs.NeedSatisfactions.Maximized);

            //Example for getting a sorted list of all goals for an agents
            //System.Collections.Generic.List<Simulation.Goal> list = new System.Collections.Generic.List<Simulation.Goal>();
            //list = agent.AffordanceTreeNode.SatisfactionLevels.GoalForEachNeed();
            //Debug.Log(list.Count);
            //foreach (Simulation.Goal goal in list)
            //{
            //    Debug.Log(goal.NeedToSatisfy + " " + goal.SatisfactionState);
            //}

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
            Simulation.Bidding.Result biddingResult = Simulation.Bidding.GoalOrientedBid(agent.AffordanceTreeNode);
            // You can instead use callbacks to adjust evaluataion of the slots to your game.
            // This is demonstrated in the class DecideClosestGoal
            // Simulation.Bidding.Result biddingResult = Simulation.Bidding.GoalOrientedBid(agent.AffordanceTreeNode, preferShortestDistanceByAir);

            if (biddingResult == Simulation.Bidding.Result.Success)
            {
                agent.Blackboard.currentState = Blackboard.AgentState.WaitingForSlot;
                return Result.Running;
            }

            agent.Blackboard.currentState = Blackboard.AgentState.None;
            return Result.Failure;
        }
    }
}