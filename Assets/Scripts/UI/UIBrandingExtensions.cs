using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MinipollGame.UI
{
    /// <summary>
    /// Additional branding styles and enums for the comprehensive UI system
    /// Extends the existing MinipollBrandManager with modern UI patterns
    /// </summary>
    public static class UIBrandingExtensions
    {
        /// <summary>
        /// Extended button styles for modern UI
        /// </summary>
        public enum ButtonStyle
        {
            Primary,    // Main action buttons (blue)
            Secondary,  // Secondary actions (gray)
            Success,    // Positive actions (green)
            Warning,    // Caution actions (orange/yellow)
            Danger      // Destructive actions (red/pink)
        }
        
        /// <summary>
        /// Panel styling options
        /// </summary>
        public enum PanelStyle
        {
            Light,      // Light background panels
            Dark,       // Dark background panels
            Transparent, // Semi-transparent panels
            Glass       // Glass-like effect panels
        }
        
        /// <summary>
        /// Typography styles for different UI contexts
        /// </summary>
        public enum TypographyStyle
        {
            H1,         // Large headers
            H2,         // Medium headers  
            H3,         // Small headers
            H4,         // Sub-headers
            Body,       // Regular body text
            Button,     // Button text
            Caption,    // Small descriptive text
            Label       // Form labels
        }
        
        /// <summary>
        /// Modern color palette extending MinipollBrandColors
        /// </summary>
        public static class ModernColors
        {
            // Primary colors from existing branding with null safety
            public static Color Primary => MinipollBrandManager.Colors?.MinipollBlue ?? new Color(0.3f, 0.6f, 1f);
            public static Color Secondary => MinipollBrandManager.Colors?.DeepNavy ?? new Color(0.1f, 0.1f, 0.3f);
            public static Color Success => MinipollBrandManager.Colors?.GrowthGreen ?? new Color(0.2f, 0.7f, 0.4f);
            public static Color Warning => MinipollBrandManager.Colors?.WarmOrange ?? new Color(1f, 0.6f, 0.2f);
            public static Color Danger => MinipollBrandManager.Colors?.HeartPink ?? new Color(1f, 0.3f, 0.5f);
            
            // UI specific colors
            public static Color Background => new Color(0.95f, 0.95f, 0.95f, 1f);
            public static Color Surface => Color.white;
            public static Color OnSurface => MinipollBrandManager.Colors?.DeepNavy ?? new Color(0.1f, 0.1f, 0.3f);
            public static Color OnPrimary => MinipollBrandManager.Colors?.CloudWhite ?? Color.white;
            
            // Interactive states
            public static Color Hover => new Color(1f, 1f, 1f, 0.1f);
            public static Color Press => new Color(0f, 0f, 0f, 0.1f);
            public static Color Disabled => MinipollBrandManager.Colors?.LightGray ?? new Color(0.7f, 0.7f, 0.7f);
        }
        
        /// <summary>
        /// Apply modern button styling
        /// </summary>
        public static void StyleModernButton(Button button, ButtonStyle style)
        {
            if (button == null) 
            {
                Debug.LogWarning("StyleModernButton: Button parameter is null");
                return;
            }
            
            var image = button.GetComponent<Image>();
            if (image == null) image = button.gameObject.AddComponent<Image>();
            
            // Apply color based on style
            switch (style)
            {
                case ButtonStyle.Primary:
                    image.color = ModernColors.Primary;
                    break;
                case ButtonStyle.Secondary:
                    image.color = MinipollBrandManager.Colors?.LightGray ?? new Color(0.7f, 0.7f, 0.7f);
                    break;
                case ButtonStyle.Success:
                    image.color = ModernColors.Success;
                    break;
                case ButtonStyle.Warning:
                    image.color = ModernColors.Warning;
                    break;
                case ButtonStyle.Danger:
                    image.color = ModernColors.Danger;
                    break;
            }
            
            // Add subtle corner rounding if possible
            // Note: This would require a custom shader or image sprite
        }
        
        /// <summary>
        /// Apply modern panel styling
        /// </summary>
        public static void StyleModernPanel(Image panel, PanelStyle style)
        {
            if (panel == null) return;
            
            switch (style)
            {
                case PanelStyle.Light:
                    panel.color = ModernColors.Surface;
                    break;
                case PanelStyle.Dark:
                    panel.color = MinipollBrandManager.Colors?.DeepNavy ?? new Color(0.1f, 0.1f, 0.3f);
                    break;
                case PanelStyle.Transparent:
                    panel.color = new Color(0f, 0f, 0f, 0.5f);
                    break;
                case PanelStyle.Glass:
                    panel.color = new Color(1f, 1f, 1f, 0.1f);
                    break;
            }
        }
        
        /// <summary>
        /// Apply modern typography styling
        /// </summary>
        public static void StyleModernText(TextMeshProUGUI text, TypographyStyle style)
        {
            if (text == null) return;
            
            // Apply modern spacing and sizing
            switch (style)
            {
                case TypographyStyle.H1:
                    text.fontSize = 48f;
                    text.fontStyle = FontStyles.Bold;
                    text.color = ModernColors.OnSurface;
                    break;
                case TypographyStyle.H2:
                    text.fontSize = 36f;
                    text.fontStyle = FontStyles.Bold;
                    text.color = ModernColors.OnSurface;
                    break;
                case TypographyStyle.H3:
                    text.fontSize = 24f;
                    text.fontStyle = FontStyles.Bold;
                    text.color = ModernColors.OnSurface;
                    break;
                case TypographyStyle.H4:
                    text.fontSize = 20f;
                    text.fontStyle = FontStyles.Bold;
                    text.color = ModernColors.OnSurface;
                    break;
                case TypographyStyle.Body:
                    text.fontSize = 16f;
                    text.fontStyle = FontStyles.Normal;
                    text.color = ModernColors.OnSurface;
                    break;
                case TypographyStyle.Button:
                    text.fontSize = 18f;
                    text.fontStyle = FontStyles.Bold;
                    text.color = ModernColors.OnPrimary;
                    break;
                case TypographyStyle.Caption:
                    text.fontSize = 12f;
                    text.fontStyle = FontStyles.Normal;
                    text.color = ModernColors.OnSurface;
                    break;
                case TypographyStyle.Label:
                    text.fontSize = 14f;
                    text.fontStyle = FontStyles.Normal;
                    text.color = ModernColors.OnSurface;
                    break;
            }
        }
        
        /// <summary>
        /// Create a gradient background
        /// </summary>
        public static void ApplyGradientBackground(Image image, Color topColor, Color bottomColor)
        {
            if (image == null) return;
            
            // Note: This would require a custom shader or gradient texture
            // For now, use a blend of the colors
            image.color = Color.Lerp(topColor, bottomColor, 0.5f);
        }
        
        /// <summary>
        /// Add shadow effect to UI element
        /// </summary>
        public static void AddShadow(GameObject uiElement, Color shadowColor, Vector2 offset)
        {
            if (uiElement == null) return;
            
            var shadow = uiElement.AddComponent<Shadow>();
            shadow.effectColor = shadowColor;
            shadow.effectDistance = offset;
        }
        
        /// <summary>
        /// Add outline effect to text
        /// </summary>
        public static void AddOutline(TextMeshProUGUI text, Color outlineColor, float thickness = 1f)
        {
            if (text == null) return;
            
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = Vector2.one * thickness;
        }
    }
}
