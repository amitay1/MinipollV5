using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Simple UI highlight system for tutorial
/// Creates gentle pulsing circle to draw attention to important elements
/// </summary>
public class TutorialHighlight : MonoBehaviour
{
    [Header("🎯 Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 0.8f;
    public Vector3 scaleRange = new Vector3(0.1f, 0.1f, 0.1f);
    
    private Image highlightImage;
    private bool isActive = false;
    
    void Start()
    {
        highlightImage = GetComponent<Image>();
        if (highlightImage == null)
        {
            highlightImage = gameObject.AddComponent<Image>();
        }
        
        // Setup circle sprite and color
        highlightImage.color = highlightColor;
        highlightImage.type = Image.Type.Simple;
        
        // Start invisible
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (isActive)
        {
            AnimateHighlight();
        }
    }
    
    void AnimateHighlight()
    {
        // Pulsing alpha animation
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        Color color = highlightColor;
        color.a = alpha;
        highlightImage.color = color;
        
        // Gentle scale animation
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed * 0.5f) * 0.1f;
        transform.localScale = Vector3.one * scale;
    }
    
    public void ShowHighlight(Vector3 worldPosition)
    {
        gameObject.SetActive(true);
        isActive = true;
        transform.position = worldPosition;
    }
    
    public void ShowHighlight(RectTransform uiElement)
    {
        gameObject.SetActive(true);
        isActive = true;
        
        // Position over UI element
        transform.position = uiElement.position;
        
        // Match size roughly
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = uiElement.sizeDelta * 1.2f;
        }
    }
    
    public void HideHighlight()
    {
        isActive = false;
        gameObject.SetActive(false);
    }
}
