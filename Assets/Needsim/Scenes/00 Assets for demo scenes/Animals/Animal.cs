/**
 * This is part of the NEEDSIM Life simulation plug in for Unity, version 1.2.2
 * Copyright 2014 - 2021 Fantasieleben UG (haftungsbeschraenkt)
 *
 * http;//www.fantasieleben.com for further details.
 *
 * For questions please get in touch with Tilman Geishauser: tilman@fantasieleben.com
 */

using UnityEngine;
using System.Collections;

namespace NEEDSIMSampleSceneScripts
{
    /// <summary>
    /// This example suggests an idea for playing animations based on the states of NEEDSIMNodes.
    /// </summary>
    [RequireComponent(typeof(NEEDSIM.NEEDSIMNode))]
    public class Animal : MonoBehaviour
    {
        protected Animator animator;
        protected NEEDSIM.NEEDSIMNode needsimNode;

        internal virtual void Start()
        {
            animator = GetComponentInChildren<Animator>();
            needsimNode = GetComponent<NEEDSIM.NEEDSIMNode>();
            needsimNode.onInteractionStartedByAgent += InteractionStarted;
            needsimNode.onMovementStartedByAgent += MovementStarted;
        }

        private void InteractionStarted(string interactionName)
        {
            //Rotate agent towards LookAt 
            gameObject.transform.LookAt(needsimNode.Blackboard.activeSlot.LookAt);

            animator.SetTrigger(interactionName);
        }

        private void MovementStarted()
        {
            //Rotate agent into movement direction
            gameObject.transform.LookAt(needsimNode.GetComponent<UnityEngine.AI.NavMeshAgent>().steeringTarget);
            gameObject.transform.Rotate(90.0f, gameObject.transform.rotation.y, gameObject.transform.rotation.z);

            animator.SetTrigger("Movement");
        }

        internal virtual void Update()
        {
            //Override by the animals fox, bunny and dear
        }

        private void OnDestroy()
        {
            needsimNode.onInteractionStartedByAgent -= InteractionStarted;
            needsimNode.onMovementStartedByAgent -= MovementStarted;
        }
    }
}
