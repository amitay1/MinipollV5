using UnityEngine;

public class UIAutoStarter : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🚀 UIAutoStarter: Looking for CompleteArtUISystem...");
        
        // מצא את מערכת ה-UI
        CompleteArtUISystem uiSystem = FindFirstObjectByType<CompleteArtUISystem>();
        
        if (uiSystem != null)
        {
            Debug.Log("✅ Found CompleteArtUISystem! Auto-starting...");
            
            // הפעל את המערכת מיד
            uiSystem.ForceInitializeUI();
        }
        else
        {
            Debug.LogError("❌ CompleteArtUISystem not found!");
        }
        
        // השמד את עצמך אחרי שסיימת
        Destroy(gameObject);
    }
}
