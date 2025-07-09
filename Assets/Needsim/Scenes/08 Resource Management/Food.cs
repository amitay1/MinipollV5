using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEEDSIMSampleSceneScripts
{
    /// <summary>
    /// An example for creating a depletable resource
    /// </summary>
    public class Food : MonoBehaviour
    {
        //In the Editor set a value above 0 here
        [SerializeField]
        float foodLeft = 10;

        //How much one character eats per second
        [SerializeField]
        float foodDepletionRate = 0.25f;

        //These will disappear as less food is on the table
        [SerializeField]
        private GameObject FoodAtFullAmount;
        [SerializeField]
        private GameObject FoodAtThreeQuarterAmount;
        [SerializeField]
        private GameObject FoodAtHalfAmount;
        [SerializeField]
        private GameObject FoodAtOneQuarterAmount;

        float foodAtStart;

        NEEDSIM.NEEDSIMNode node;

        bool hadInteraction = false;

        // Use this for initialization
        void Start()
        {
            node = GetComponent<NEEDSIM.NEEDSIMNode>();
            foodAtStart = foodLeft;
        }

        // Update is called once per frame
        void Update()
        {
            //Check
            foreach (Simulation.Slot slot in node.AffordanceTreeNode.Slots)
            {
                if (slot.SlotState == Simulation.Slot.SlotStates.ReadyCharacter)
                {
                    //If a new interaction was started
                    if (!hadInteraction && node.AffordanceTreeNode.Affordance.CurrentInteraction != null)
                    {
                        foodLeft = foodLeft - foodDepletionRate * Time.deltaTime;
                        
                        if (foodLeft < 0)
                        {
                            slot.InterruptInteraction();
                            slot.AgentDeparture();
                            node.enabled = false;
                        }
                    }
                }
            }

            if (node.AffordanceTreeNode.Affordance.CurrentInteraction == null)
            {
                hadInteraction = false;
            }

            //Let food disappear 
            if (0 > foodLeft)
            {
                FoodAtOneQuarterAmount.SetActive(false);
            }
            else if ((foodAtStart * 0.25) > foodLeft)
            {
                FoodAtHalfAmount.SetActive(false);
            }
            else if ((foodAtStart * 0.5) > foodLeft)
            {
                FoodAtThreeQuarterAmount.SetActive(false);
            }
            else if ((foodAtStart * 0.75) > foodLeft)
            {
                FoodAtFullAmount.SetActive(false);
            }
        }
    }
}
