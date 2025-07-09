using UnityEngine;
using UnityEngine.AI;

public class TestMovement : MonoBehaviour
{
    public Transform target;
    NavMeshAgent agent;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("No NavMeshAgent found!");
            return;
        }
        
        // מחפש מטרה לזוז אליה
        if (target == null)
        {
            GameObject food = GameObject.Find("Food_Source");
            if (food != null) target = food.transform;
        }
    }
    
    void Update()
    {
        if (agent != null && target != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
            Debug.Log("Moving to: " + target.name);
        }
        else
        {
            Debug.Log("Cannot move - NavMesh not available");
        }
    }
}