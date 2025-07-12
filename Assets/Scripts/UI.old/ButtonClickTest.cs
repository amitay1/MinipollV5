using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple button click test to verify input system is working
/// This script will be added directly to the scene to test button clicks
/// </summary>
public class ButtonClickTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🧪 ButtonClickTest: Starting input system test...");
        
        // Find all buttons in the scene
        Button[] allButtons = FindObjectsOfType<Button>();
        Debug.Log($"🧪 Found {allButtons.Length} buttons in scene");
        
        foreach (Button button in allButtons)
        {
            if (button != null)
            {
                Debug.Log($"🧪 Setting up test click for: {button.name}");
                
                // Remove existing listeners to avoid conflicts
                button.onClick.RemoveAllListeners();
                
                // Add our test click handler
                string buttonName = button.name;
                button.onClick.AddListener(() => OnButtonClicked(buttonName, button));
            }
        }
        
        Debug.Log("🧪 Button click test setup complete!");
    }
    
    void OnButtonClicked(string buttonName, Button button)
    {
        Debug.Log($"🎉 BUTTON CLICKED! Button: {buttonName}");
        Debug.Log($"🎉 Input system is working! Button position: {button.transform.position}");
        
        // Test animation if available
        Animation anim = button.GetComponent<Animation>();
        if (anim != null && anim.clip != null)
        {
            Debug.Log($"🎬 Playing animation: {anim.clip.name}");
            anim.Play();
            
            // Load game scene after animation (for play button)
            if (buttonName.ToLower().Contains("play"))
            {
                Invoke("LoadGameScene", anim.clip.length);
            }
        }
        else
        {
            Debug.Log("⚠️ No animation found on button");
            
            // Load game scene immediately (for play button)
            if (buttonName.ToLower().Contains("play"))
            {
                LoadGameScene();
            }
        }
    }
    
    void LoadGameScene()
    {
        Debug.Log("🎮 Loading game scene: 02_GameScene");
        SceneManager.LoadScene("02_GameScene");
    }
}
