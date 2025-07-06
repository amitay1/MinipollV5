using UnityEngine;

/// <summary>
/// ScriptableObject for managing Minipoll brand colors
/// Based on TASK001 branding specifications
/// </summary>
[CreateAssetMenu(fileName = "MinipollBrandColors", menuName = "Minipoll/Branding/Brand Colors", order = 1)]
public class MinipollBrandColors : ScriptableObject
{
    [Header("Primary Colors")]
    [SerializeField] private Color minipollBlue = new Color(0.29f, 0.56f, 0.89f, 1f); // #4A90E2
    [SerializeField] private Color heartPink = new Color(1f, 0.42f, 0.62f, 1f); // #FF6B9D
    [SerializeField] private Color growthGreen = new Color(0.49f, 0.83f, 0.13f, 1f); // #7ED321

    [Header("Secondary Colors")]
    [SerializeField] private Color softPurple = new Color(0.74f, 0.47f, 1f, 1f); // #BD78FF
    [SerializeField] private Color warmOrange = new Color(0.96f, 0.65f, 0.14f, 1f); // #F5A623
    [SerializeField] private Color cloudWhite = new Color(0.97f, 0.98f, 0.98f, 1f); // #F8F9FA

    [Header("Supporting Colors")]
    [SerializeField] private Color deepNavy = new Color(0.17f, 0.24f, 0.31f, 1f); // #2C3E50
    [SerializeField] private Color lightGray = new Color(0.93f, 0.94f, 0.95f, 1f); // #ECF0F1
    [SerializeField] private Color successGreen = new Color(0.15f, 0.68f, 0.38f, 1f); // #27AE60
    [SerializeField] private Color warningAmber = new Color(0.95f, 0.61f, 0.07f, 1f); // #F39C12

    // Public getters for easy access
    public Color MinipollBlue => minipollBlue;
    public Color HeartPink => heartPink;
    public Color GrowthGreen => growthGreen;
    public Color SoftPurple => softPurple;
    public Color WarmOrange => warmOrange;
    public Color CloudWhite => cloudWhite;
    public Color DeepNavy => deepNavy;
    public Color LightGray => lightGray;
    public Color SuccessGreen => successGreen;
    public Color WarningAmber => warningAmber;

    /// <summary>
    /// Get color by emotion type for Minipoll character
    /// </summary>
    public Color GetEmotionColor(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => heartPink,
            EmotionType.Calm => minipollBlue,
            EmotionType.Excited => warmOrange,
            EmotionType.Growing => growthGreen,
            EmotionType.Magical => softPurple,
            EmotionType.Neutral => cloudWhite,
            _ => minipollBlue
        };
    }

    /// <summary>
    /// Get color for creature life stage
    /// </summary>
    public Color GetLifeStageColor(LifeStage stage)
    {
        return stage switch
        {
            LifeStage.Baby => Color.Lerp(heartPink, cloudWhite, 0.3f), // Soft pink
            LifeStage.Young => Color.Lerp(minipollBlue, cloudWhite, 0.2f), // Light blue
            LifeStage.Adult => minipollBlue, // Full blue
            LifeStage.Special => Color.Lerp(softPurple, warmOrange, 0.5f), // Golden purple
            _ => minipollBlue
        };
    }
}

public enum EmotionType
{
    Happy,
    Calm,
    Excited,
    Growing,
    Magical,
    Neutral
}

public enum LifeStage
{
    Baby,
    Young,
    Adult,
    Special
}
