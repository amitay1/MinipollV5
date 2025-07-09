/**
 * This is part of the NEEDSIM Life simulation plug in for Unity, version 1.2.1
 * Copyright 2014 - 2017 Fantasieleben UG (haftungsbeschraenkt)
 *
 * http;//www.fantasieleben.com for further details.
 *
 * For questions please get in touch with Tilman Geishauser: tilman@fantasieleben.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEEDSIM
{
    /// <summary>
    /// The method for decision making can be changed, examples are GOAP, FSM or Behavior Trees.
    /// </summary>
    public interface IDecisionMaking
    {
        /// <summary>
        /// Update runs the plan.
        /// 
        /// If the currently running action returns, upon evaluation, Result.Running, we keep on running that action.
        /// If Result.Failure is returend we start a new sequence.
        /// If Result.Success is returned we go to the next step in the current sequence, or, if at the last step,
        /// start a new sequence.
        /// </summary>
        public void Update();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The name of the current action</returns>
        public string printCurrentAction();
    }
}