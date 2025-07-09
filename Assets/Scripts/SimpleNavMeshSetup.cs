using UnityEngine;
using UnityEngine.AI;

public class SimpleNavMeshSetup : MonoBehaviour
{
    void Start()
    {
        Debug.Log("מגדיר NavMesh פשוט לחוויה ראשונה...");
        
        // הגדר את האובייקטים הסטטיים
        SetupStaticObjects();
        
        Debug.Log("NavMesh מוכן לחוויה ראשונה!");
    }
    
    void SetupStaticObjects()
    {
        // מצא ואגדר כל משטחי הקרקע כסטטיים
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Ground") || 
                obj.name.Contains("Static") ||
                obj.name.Contains("Floor"))
            {
                obj.isStatic = true;
                Debug.Log($"הוגדר {obj.name} כסטטי");
            }
        }
    }
}
