using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// MenuButtonsAnimationController - Clean button animations
/// </summary>
public class MenuButtonsAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    public float clickAnimationScale = 0.9f;
    public float clickAnimationDuration = 0.15f;
    
    [Header("Debug")]
    public bool showDebugMessages = true;
    
    private Button[] menuButtons;
    
    void Start()
    {
        menuButtons = GetComponentsInChildren<Button>();
        
        if (showDebugMessages)
        {
            Debug.Log($" Found {menuButtons.Length} buttons");
        }
        
        SetupButtonClickAnimations();
        
        if (showDebugMessages)
        {
            Debug.Log(" MenuButtonsAnimationController ready!");
        }
    }
    
    void SetupButtonClickAnimations()
    {
        foreach (Button button in menuButtons)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => OnButtonClick(button));
            }
        }
    }
    
    void OnButtonClick(Button button)
    {
        if (showDebugMessages)
        {
            Debug.Log($" Button clicked: {button.name}");
        }
        
        StartCoroutine(ButtonClickAnimation(button));
    }
    
    IEnumerator ButtonClickAnimation(Button button)
    {
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        Vector3 originalScale = buttonRect.localScale;
        
        float time = 0f;
        while (time < clickAnimationDuration)
        {
            time += Time.deltaTime;
            float progress = time / clickAnimationDuration;
            
            float scale = Mathf.Lerp(1f, clickAnimationScale, progress);
            buttonRect.localScale = originalScale * scale;
            
            yield return null;
        }
        
        time = 0f;
        while (time < clickAnimationDuration)
        {
            time += Time.deltaTime;
            float progress = time / clickAnimationDuration;
            
            float scale = Mathf.Lerp(clickAnimationScale, 1f, progress);
            buttonRect.localScale = originalScale * scale;
            
            yield return null;
        }
        
        buttonRect.localScale = originalScale;
    }
}
