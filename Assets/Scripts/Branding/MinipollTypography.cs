using UnityEngine;
using TMPro;

/// <summary>
/// ScriptableObject for managing Minipoll typography system
/// Based on TASK001 branding specifications
/// </summary>
[CreateAssetMenu(fileName = "MinipollTypography", menuName = "Minipoll/Branding/Typography", order = 2)]
public class MinipollTypography : ScriptableObject
{
    [Header("Font Assets")]
    [SerializeField] private TMP_FontAsset comfortaaFont; // Primary font for headers and logo
    [SerializeField] private TMP_FontAsset openSansFont; // Secondary font for body text and UI
    [SerializeField] private TMP_FontAsset fredokaOneFont; // Accent font for numbers and special items

    [Header("Font Sizes")]
    [SerializeField] private float h1Size = 48f; // Main titles
    [SerializeField] private float h2Size = 36f; // Section headers
    [SerializeField] private float h3Size = 28f; // Subsection headers
    [SerializeField] private float h4Size = 24f; // Small headers
    [SerializeField] private float bodySize = 16f; // Regular body text
    [SerializeField] private float buttonSize = 18f; // Button text
    [SerializeField] private float captionSize = 14f; // Small text/captions
    [SerializeField] private float numberSize = 32f; // Numbers and stats
    [SerializeField] private float logoSize = 42f; // Logo text

    // Font Asset Properties
    public TMP_FontAsset ComfortaaFont => comfortaaFont;
    public TMP_FontAsset OpenSansFont => openSansFont;
    public TMP_FontAsset FredokaOneFont => fredokaOneFont;

    // Font Size Properties
    public float H1Size => h1Size;
    public float H2Size => h2Size;
    public float H3Size => h3Size;
    public float H4Size => h4Size;
    public float BodySize => bodySize;
    public float ButtonSize => buttonSize;
    public float CaptionSize => captionSize;
    public float NumberSize => numberSize;
    public float LogoSize => logoSize;

    /// <summary>
    /// Configure a TextMeshPro component with typography settings
    /// </summary>
    public void ApplyTypography(TextMeshProUGUI textComponent, TypographyStyle style)
    {
        if (textComponent == null) return;

        switch (style)
        {
            case TypographyStyle.H1:
                textComponent.font = comfortaaFont;
                textComponent.fontSize = h1Size;
                textComponent.fontWeight = FontWeight.Bold;
                break;

            case TypographyStyle.H2:
                textComponent.font = comfortaaFont;
                textComponent.fontSize = h2Size;
                textComponent.fontWeight = FontWeight.Bold;
                break;

            case TypographyStyle.H3:
                textComponent.font = comfortaaFont;
                textComponent.fontSize = h3Size;
                textComponent.fontWeight = FontWeight.Regular;
                break;

            case TypographyStyle.H4:
                textComponent.font = comfortaaFont;
                textComponent.fontSize = h4Size;
                textComponent.fontWeight = FontWeight.Regular;
                break;

            case TypographyStyle.Body:
                textComponent.font = openSansFont;
                textComponent.fontSize = bodySize;
                textComponent.fontWeight = FontWeight.Regular;
                break;

            case TypographyStyle.Button:
                textComponent.font = openSansFont;
                textComponent.fontSize = buttonSize;
                textComponent.fontWeight = FontWeight.SemiBold;
                break;

            case TypographyStyle.Caption:
                textComponent.font = openSansFont;
                textComponent.fontSize = captionSize;
                textComponent.fontWeight = FontWeight.Regular;
                break;

            case TypographyStyle.Number:
                textComponent.font = fredokaOneFont;
                textComponent.fontSize = numberSize;
                textComponent.fontWeight = FontWeight.Regular;
                break;

            case TypographyStyle.Logo:
                textComponent.font = comfortaaFont;
                textComponent.fontSize = logoSize;
                textComponent.fontWeight = FontWeight.Bold;
                break;
        }
    }

    /// <summary>
    /// Get responsive font size based on screen width
    /// </summary>
    public float GetResponsiveFontSize(TypographyStyle style, float screenWidth)
    {
        float baseSize = style switch
        {
            TypographyStyle.H1 => h1Size,
            TypographyStyle.H2 => h2Size,
            TypographyStyle.H3 => h3Size,
            TypographyStyle.H4 => h4Size,
            TypographyStyle.Body => bodySize,
            TypographyStyle.Button => buttonSize,
            TypographyStyle.Caption => captionSize,
            TypographyStyle.Number => numberSize,
            TypographyStyle.Logo => logoSize,
            _ => bodySize
        };

        // Scale down for smaller screens
        if (screenWidth < 375f) // Small mobile
            return baseSize * 0.8f;
        else if (screenWidth < 768f) // Mobile
            return baseSize * 0.9f;
        else // Tablet and desktop
            return baseSize;
    }
}

public enum TypographyStyle
{
    H1,      // Main titles (Comfortaa Bold)
    H2,      // Section headers (Comfortaa Bold)
    H3,      // Subsection headers (Comfortaa Regular)
    H4,      // Small headers (Comfortaa Regular)
    Body,    // Regular text (Open Sans Regular)
    Button,  // Button text (Open Sans SemiBold)
    Caption, // Small text (Open Sans Regular)
    Number,  // Numbers and stats (Fredoka One)
    Logo     // Logo text (Comfortaa Bold)
}
