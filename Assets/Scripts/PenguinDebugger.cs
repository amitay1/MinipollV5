using UnityEngine;
using NEEDSIM;

public class PenguinDebugger : MonoBehaviour
{
    public NEEDSIMNode needsimNode;
    
    void Start()
    {
        needsimNode = GetComponent<NEEDSIMNode>();
        if (needsimNode == null)
        {
            Debug.LogError("NEEDSIMNode not found on " + gameObject.name);
        }
    }
    
    void Update()
    {
        if (needsimNode != null)
        {
            Debug.Log($"Penguin Position: {transform.position}");
            Debug.Log($"NavMeshAgent enabled: {GetComponent<UnityEngine.AI.NavMeshAgent>()?.enabled}");
            Debug.Log($"NavMeshAgent on NavMesh: {GetComponent<UnityEngine.AI.NavMeshAgent>()?.isOnNavMesh}");
        }
    }
}