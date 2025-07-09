using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEEDSIMSampleSceneScripts
{
    [RequireComponent(typeof(NEEDSIM.NEEDSIMNode))]
    public class AgentDeath : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        [Tooltip("The names of the needs have to match the needs in the database. If any that is listed here goes to 0 the agent dies.")]
        [SerializeField]
        private string[] DeadlyNeeds;

        public bool IsDeath
        {
            get; private set;
        }

        private NEEDSIM.NEEDSIMNode NEEDSIMNode;

        void Start()
        {
            NEEDSIMNode = GetComponent<NEEDSIM.NEEDSIMNode>();
            IsDeath = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsDeath)
            {
                for (int i = 0; i < DeadlyNeeds.Length; i++)
                {
                    if (NEEDSIMNode.AffordanceTreeNode.SatisfactionLevels.GetValue(DeadlyNeeds[i]) <= 0)
                    {
                        Die();
                    }
                }
            }
        }

        public void Die()
        {
            // You only die once
            if(!IsDeath)
            {
                GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;

                //Instead you could also call Destroy(NEEDSIMNode); - but then there would be no dead corpse
                NEEDSIMNode.KillObject();

                animator.SetTrigger("Die");
                IsDeath = true;
            }
        }
    }
}