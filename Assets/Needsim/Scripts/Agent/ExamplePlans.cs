/**
 * This is part of the NEEDSIM Life simulation plug in for Unity, version 1.1
 * Copyright 2014 - 2017 Fantasieleben UG (haftungsbeschraenkt)
 *
 * http;//www.fantasieleben.com for further details.
 *
 * For questions please get in touch with Tilman Geishauser: tilman@fantasieleben.com
 */

namespace NEEDSIM
{
    /// <summary>
    /// Goal oriented behavior is more efficient, and allows to search the data structure specifically for interactions that help satisfy the respective need. 
    /// Value oriented behavior puts more emphasis on the utility of any need satisfaction. 
    /// The chase and interruption behaviors are examples to clarify how you could use sequences of actions in your game.
    /// The ClosestGoalByAir is demonstrating how to replace evaluation with your own method, in this example closer objects are favoured.
    /// </summary>
    public enum ExamplePlans
    {
        GoalOriented,
        ValueOriented,
        GoalOrientedChase,
        InterruptionFuchsalarm,
        ClosestGoalByAir,
        ClosestValueByAir
    }
}