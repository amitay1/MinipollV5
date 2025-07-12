using UnityEngine;

public class UITester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🧪 UITester Start() called");
        
        var uiSystem = FindFirstObjectByType<CompleteArtUISystem>();
        if (uiSystem != null)
        {
            Debug.Log("✅ Found CompleteArtUISystem, calling InitializeCompleteUI()");
            uiSystem.InitializeCompleteUI();
        }
        else
        {
            Debug.LogError("❌ CompleteArtUISystem not found!");
        }
    }
}
