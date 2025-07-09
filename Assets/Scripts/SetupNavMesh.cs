using UnityEngine;
using UnityEngine.AI;

public class SetupNavMesh : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Setting up NavMesh environment...");
        
        // בדיקה שכל האובייקטים נמצאים במקום הנכון
        GameObject penguin = GameObject.Find("Penguin_Minipoll");
        if (penguin != null)
        {
            NavMeshAgent agent = penguin.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                Debug.Log($"NavMeshAgent found on penguin. IsOnNavMesh: {agent.isOnNavMesh}");
                agent.enabled = true;
            }
        }
        
        Debug.Log("NavMesh setup complete!");
    }
}