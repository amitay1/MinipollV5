using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central brand manager for Minipoll - Singleton pattern
/// Provides easy access to branding assets and helper methods
/// </summary>
public class MinipollBrandManager : MonoBehaviour
{
    [Header("Brand Assets")]
    [SerializeField] private MinipollBrandColors brandColors;
    [SerializeField] private MinipollTypography typography;

    [Header("Logo Assets")]
    [SerializeField] private Sprite minipollLogo;
    [SerializeField] private Sprite heartCodeStudiosLogo;
    [SerializeField] private Sprite minipollIcon;

    // Singleton instance
    private static MinipollBrandManager _instance;
    public static MinipollBrandManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MinipollBrandManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("MinipollBrandManager");
                    _instance = go.AddComponent<MinipollBrandManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // Public accessors
    public static MinipollBrandColors Colors => Instance.brandColors;
    public static MinipollTypography Typography => Instance.typography;
    public static Sprite MinipollLogo => Instance.minipollLogo;
    public static Sprite HeartCodeStudiosLogo => Instance.heartCodeStudiosLogo;
    public static Sprite MinipollIcon => Instance.minipollIcon;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    #region Helper Methods

    /// <summary>
    /// Style a button with Minipoll branding
    /// </summary>
    public static void StyleButton(Button button, ButtonStyle style = ButtonStyle.Primary)
    {
        if (button == null || Colors == null) return;

        var colors = button.colors;
        switch (style)
        {
            case ButtonStyle.Primary:
                colors.normalColor = Colors.MinipollBlue;
                colors.highlightedColor = Color.Lerp(Colors.MinipollBlue, Color.white, 0.1f);
                colors.pressedColor = Color.Lerp(Colors.MinipollBlue, Color.black, 0.1f);
                break;
            case ButtonStyle.Secondary:
                colors.normalColor = Colors.HeartPink;
                colors.highlightedColor = Color.Lerp(Colors.HeartPink, Color.white, 0.1f);
                colors.pressedColor = Color.Lerp(Colors.HeartPink, Color.black, 0.1f);
                break;
            case ButtonStyle.Success:
                colors.normalColor = Colors.SuccessGreen;
                colors.highlightedColor = Color.Lerp(Colors.SuccessGreen, Color.white, 0.1f);
                colors.pressedColor = Color.Lerp(Colors.SuccessGreen, Color.black, 0.1f);
                break;
            case ButtonStyle.Warning:
                colors.normalColor = Colors.WarningAmber;
                colors.highlightedColor = Color.Lerp(Colors.WarningAmber, Color.white, 0.1f);
                colors.pressedColor = Color.Lerp(Colors.WarningAmber, Color.black, 0.1f);
                break;
        }
        button.colors = colors;

        // Style button text
        var textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null && Typography != null)
        {
            Typography.ApplyTypography(textComponent, TypographyStyle.Button);
            textComponent.color = Colors.CloudWhite;
        }
    }

    /// <summary>
    /// Style a panel with Minipoll branding
    /// </summary>
    public static void StylePanel(Image panel, PanelStyle style = PanelStyle.Light)
    {
        if (panel == null || Colors == null) return;

        switch (style)
        {
            case PanelStyle.Light:
                panel.color = Colors.CloudWhite;
                break;
            case PanelStyle.Primary:
                panel.color = Color.Lerp(Colors.MinipollBlue, Color.white, 0.8f);
                break;
            case PanelStyle.Secondary:
                panel.color = Color.Lerp(Colors.HeartPink, Color.white, 0.8f);
                break;
            case PanelStyle.Dark:
                panel.color = Colors.LightGray;
                break;
        }
    }

    /// <summary>
    /// Apply text styling with automatic responsive sizing
    /// </summary>
    public static void StyleText(TextMeshProUGUI textComponent, TypographyStyle style, Color? color = null)
    {
        if (textComponent == null || Typography == null) return;

        Typography.ApplyTypography(textComponent, style);
        
        if (color.HasValue)
            textComponent.color = color.Value;
        else
            textComponent.color = Colors?.DeepNavy ?? Color.black;

        // Apply responsive sizing
        float screenWidth = Screen.width;
        textComponent.fontSize = Typography.GetResponsiveFontSize(style, screenWidth);
    }

    /// <summary>
    /// Create smooth color animation
    /// </summary>
    public static void AnimateColorChange(Image target, Color targetColor, float duration = 0.3f)
    {
        if (target == null) return;
        
        // This would require a coroutine system - simplified for now
        target.color = Color.Lerp(target.color, targetColor, Time.deltaTime / duration);
    }

    #endregion

    #region Validation
    private void OnValidate()
    {
        if (brandColors == null)
            Debug.LogWarning("MinipollBrandManager: Brand Colors asset not assigned!");
        
        if (typography == null)
            Debug.LogWarning("MinipollBrandManager: Typography asset not assigned!");
    }
    #endregion
}

public enum ButtonStyle
{
    Primary,   // Minipoll Blue
    Secondary, // Heart Pink
    Success,   // Success Green
    Warning    // Warning Amber
}

public enum PanelStyle
{
    Light,     // Cloud White
    Primary,   // Light Blue
    Secondary, // Light Pink
    Dark       // Light Gray
}
