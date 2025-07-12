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

    // Public accessors with fallback
    public static MinipollBrandColors Colors => Instance.brandColors ?? CreateFallbackColors();
    public static MinipollTypography Typography => Instance.typography;
    public static Sprite MinipollLogo => Instance.minipollLogo;
    public static Sprite HeartCodeStudiosLogo => Instance.heartCodeStudiosLogo;
    public static Sprite MinipollIcon => Instance.minipollIcon;

    // Fallback colors when brand colors are not assigned
    private static MinipollBrandColors CreateFallbackColors()
    {
        var fallback = ScriptableObject.CreateInstance<MinipollBrandColors>();
        fallback.name = "Fallback Brand Colors";
        
        // Initialize with reflection to set default values
        var type = typeof(MinipollBrandColors);
        var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(Color))
            {
                // Set fallback colors based on field names
                var color = field.Name switch
                {
                    "minipollBlue" => new Color(0.29f, 0.56f, 0.89f, 1f),
                    "heartPink" => new Color(1f, 0.42f, 0.62f, 1f),
                    "growthGreen" => new Color(0.49f, 0.83f, 0.13f, 1f),
                    "softPurple" => new Color(0.74f, 0.47f, 1f, 1f),
                    "warmOrange" => new Color(0.96f, 0.65f, 0.14f, 1f),
                    "cloudWhite" => new Color(0.97f, 0.98f, 0.98f, 1f),
                    "deepNavy" => new Color(0.17f, 0.24f, 0.31f, 1f),
                    "lightGray" => new Color(0.93f, 0.94f, 0.95f, 1f),
                    "successGreen" => new Color(0.15f, 0.68f, 0.38f, 1f),
                    "warningAmber" => new Color(0.95f, 0.61f, 0.07f, 1f),
                    _ => Color.white
                };
                field.SetValue(fallback, color);
            }
        }
        
        return fallback;
    }

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
