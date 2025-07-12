using UnityEngine;

public class TestUISystem : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🔍 Looking for CompleteArtUISystem...");
        
        CompleteArtUISystem uiSystem = FindFirstObjectByType<CompleteArtUISystem>();
        
        if (uiSystem != null)
        {
            Debug.Log("✅ Found CompleteArtUISystem! Checking status...");
            uiSystem.CheckUISystemStatus();
            
            Debug.Log("🚀 Attempting to initialize UI...");
            uiSystem.ForceInitializeUI();
        }
        else
        {
            Debug.LogError("❌ CompleteArtUISystem not found!");
        }
    }
}
