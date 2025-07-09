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
using UnityEngine.UI;

namespace NEEDSIMSampleSceneScripts
{
    public class OwnershipExample : MonoBehaviour
    {
        public NEEDSIM.NEEDSIMNode BedJohn;
        public NEEDSIM.NEEDSIMNode BedAdam;
        public NEEDSIM.NEEDSIMNode BedMarie;
        public NEEDSIM.NEEDSIMNode BedElena;
        public NEEDSIM.NEEDSIMNode TableToGather;
        public NEEDSIM.NEEDSIMNode TableToAvoid;

        public NEEDSIM.NEEDSIMNode John;
        public NEEDSIM.NEEDSIMNode Adam;
        public NEEDSIM.NEEDSIMNode Marie;
        public NEEDSIM.NEEDSIMNode Elena;

        private Simulation.AffordanceTreeNode ghostOwner;

        private bool toggleBedOwnerships = false;
        private bool toggleTableAvoidance = false;

        // Use this for initialization
        void Start()
        {
            // The ghostOwner is used to block objects
            ghostOwner = new Simulation.AffordanceTreeNode(null, "ghostOwner", "", Vector3.zero);
            ToggleBedOwnership();
            ToggleTableAvoidance();
        }

        public void ToggleBedOwnership()
        {
            if (!toggleBedOwnerships)
            {
                //Each agent claims his bed
                Simulation.Ownership.Instance.ClaimOwnership(BedJohn.AffordanceTreeNode.Affordance, John.AffordanceTreeNode);
                Simulation.Ownership.Instance.ClaimOwnership(BedAdam.AffordanceTreeNode.Affordance, Adam.AffordanceTreeNode);
                Simulation.Ownership.Instance.ClaimOwnership(BedMarie.AffordanceTreeNode.Affordance, Marie.AffordanceTreeNode);
                Simulation.Ownership.Instance.ClaimOwnership(BedElena.AffordanceTreeNode.Affordance, Elena.AffordanceTreeNode);

                toggleBedOwnerships = true;
            }
            else
            {
                // The specific ownerships are removed - if there were more owners only the specific owner would be removed
                Simulation.Ownership.Instance.RemoveOwnership(BedJohn.AffordanceTreeNode.Affordance, John.AffordanceTreeNode);
                Simulation.Ownership.Instance.RemoveOwnership(BedAdam.AffordanceTreeNode.Affordance, Adam.AffordanceTreeNode);
                Simulation.Ownership.Instance.RemoveOwnership(BedMarie.AffordanceTreeNode.Affordance, Marie.AffordanceTreeNode);
                Simulation.Ownership.Instance.RemoveOwnership(BedElena.AffordanceTreeNode.Affordance, Elena.AffordanceTreeNode);

                toggleBedOwnerships = false;
            }
        }

        public void ToggleTableAvoidance()
        {
            if (!toggleTableAvoidance)
            {
                //Many agents can own the same object. Here ownership is claimed by all agents for the table to gather at.
                Simulation.Ownership.Instance.ClaimOwnership(TableToGather.AffordanceTreeNode.Affordance, John.AffordanceTreeNode);
                Simulation.Ownership.Instance.ClaimOwnership(TableToGather.AffordanceTreeNode.Affordance, Adam.AffordanceTreeNode);
                Simulation.Ownership.Instance.ClaimOwnership(TableToGather.AffordanceTreeNode.Affordance, Marie.AffordanceTreeNode);
                Simulation.Ownership.Instance.ClaimOwnership(TableToGather.AffordanceTreeNode.Affordance, Elena.AffordanceTreeNode);

                //A ghost owner claims ownership for the table to avoid, thereby making it unavailable to other agents.
                Simulation.Ownership.Instance.ClaimOwnership(TableToAvoid.AffordanceTreeNode.Affordance, ghostOwner);

                toggleTableAvoidance = true;
            }
            else
            {
                // All ownership for both tables is released
                Simulation.Ownership.Instance.RemoveAllOwnerships(TableToGather.AffordanceTreeNode.Affordance);
                Simulation.Ownership.Instance.RemoveAllOwnerships(TableToAvoid.AffordanceTreeNode.Affordance);

                toggleTableAvoidance = false;
            }
        }
    }
}