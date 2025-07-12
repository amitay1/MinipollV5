using UnityEngine;

public class CleanupDuplicates : MonoBehaviour
{
    [Header("Click this button to clean duplicates")]
    public bool cleanupNow = false;
    
    void Update()
    {
        if (cleanupNow)
        {
            cleanupNow = false;
            CleanupAllDuplicates();
        }
    }
    
    public void CleanupAllDuplicates()
    {
        Debug.Log("🧹 Starting cleanup of duplicate UI objects...");
        
        // Find and destroy duplicate Canvas objects
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"Found {canvases.Length} Canvas objects");
        
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            
            // Keep only specific canvases we need
            if (canvas.name.Contains("MinipollGameUI"))
            {
                Debug.Log($"Keeping: {canvas.name}");
                continue;
            }
            
            // Destroy duplicate UI canvases
            if (canvas.name.Contains("CompleteArtUI") || 
                canvas.name.Contains("ComprehensiveUI") || 
                canvas.name.Contains("TestCanvas") ||
                canvas.name.Contains("BasicUI"))
            {
                Debug.Log($"🗑️ Destroying duplicate: {canvas.name}");
                DestroyImmediate(canvas.gameObject);
            }
        }
        
        // Clean up any GameObjects with duplicate names
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        // Track GameObjects by name to find duplicates
        System.Collections.Generic.Dictionary<string, int> nameCount = 
            new System.Collections.Generic.Dictionary<string, int>();
            
        foreach (GameObject obj in allObjects)
        {
            string name = obj.name;
            if (nameCount.ContainsKey(name))
            {
                nameCount[name]++;
                
                // If it's the second occurrence of certain UI objects, destroy it
                if (name.Contains("GameUIManager") || 
                    name.Contains("WorldManager") || 
                    name.Contains("MinipollManager"))
                {
                    Debug.Log($"🗑️ Destroying duplicate: {name} (#{nameCount[name]})");
                    DestroyImmediate(obj);
                }
            }
            else
            {
                nameCount[name] = 1;
            }
        }
        
        Debug.Log("✅ Cleanup completed!");
    }
}
