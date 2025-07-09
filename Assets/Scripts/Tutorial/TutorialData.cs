using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that holds tutorial step data
/// Allows designers to easily edit tutorial content without code changes
/// </summary>
[CreateAssetMenu(fileName = "TutorialData", menuName = "Minipoll/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [Header("🎓 Tutorial Configuration")]
    public string tutorialName = "First Time Tutorial";
    public bool enableTypewriter = true;
    public float typewriterSpeed = 0.05f;
    public bool allowSkip = true;
    
    [Header("📚 Tutorial Steps")]
    public List<TutorialStepData> steps = new List<TutorialStepData>();
    
    [System.Serializable]
    public class TutorialStepData
    {
        [Header("📝 Content")]
        public string title = "Tutorial Step";
        
        [TextArea(4, 8)]
        public string message = "Tutorial message goes here...";
        
        [Header("🎯 Interaction")]
        public HighlightType highlightType = HighlightType.None;
        public string targetObjectName = "";
        public Vector2 uiPosition = Vector2.zero;
        
        [Header("⏱️ Timing")]
        public float waitTimeBeforeNext = 2f;
        public bool requiresUserAction = false;
        public string actionDescription = "";
        
        [Header("🎨 Visual")]
        public Sprite tutorialImage;
        public Color backgroundColor = Color.white;
        
        [Header("🔊 Audio")]
        public AudioClip voiceClip;
        public AudioClip soundEffect;
    }
    
    public enum HighlightType
    {
        None,
        GameObject,
        UIPosition,
        Screen
    }
}

/// <summary>
/// Tutorial step for comfortable player experience
/// Each step focuses on building emotional connection with Minipoll
/// </summary>
[System.Serializable]
public class ComfortableTutorialStep
{
    [Header("💕 Emotional Context")]
    public string emotionalGoal = "Build trust and understanding";
    
    [Header("📖 Story Content")]
    public string title;
    
    [TextArea(3, 6)]
    public string message;
    
    [Header("🎭 Minipoll Reaction")]
    public string minipollEmotion = "curious";
    public string minipollAnimation = "idle";
    
    [Header("🎯 Learning Objective")]
    public string whatPlayerLearns = "How to care for Minipoll";
    public bool isActionRequired = false;
    public string actionInstructions = "";
    
    [Header("⏱️ Pacing")]
    public float minimumTime = 3f;
    public bool waitForPlayerReady = true;
    
    [Header("🌟 Encouragement")]
    public string positiveReinforcement = "Great job! Minipoll feels loved!";
    public bool showEncouragement = true;
}

/// <summary>
/// Hebrew tutorial steps for Minipoll creature care
/// Designed to create emotional bonding through gentle guidance
/// </summary>
public static class HebrewTutorialSteps
{
    public static List<ComfortableTutorialStep> GetDefaultSteps()
    {
        return new List<ComfortableTutorialStep>
        {
            new ComfortableTutorialStep
            {
                emotionalGoal = "Welcome and excitement",
                title = "ברוכים הבאים! 🎉",
                message = "שלום ומוקדים! הנה החבר החדש שלכם - Minipoll! 💕\n\nהוא קצת מבוהל כי הוא חדש כאן, אבל הוא כבר מחכה להכיר אתכם!",
                minipollEmotion = "shy",
                minipollAnimation = "look_around",
                whatPlayerLearns = "First introduction to Minipoll",
                minimumTime = 4f,
                positiveReinforcement = "Minipoll שמח לראות אתכם! 😊"
            },
            
            new ComfortableTutorialStep
            {
                emotionalGoal = "Understanding creature needs",
                title = "הבנת הצרכים 🍎💤🎾🛁",
                message = "כמו חיות מחמד אמיתיות, ל-Minipoll יש צרכים שאתם יכולים לדאוג להם:\n\n🍎 רעב - נאכיל אותו כשהוא רעב\n💤 עייפות - נדאג שינוח כשהוא עייף\n🎾 משחק - נשחק איתו כשהוא משתעמם\n🛁 ניקיון - נשמור עליו נקי ויפה",
                minipollEmotion = "attentive",
                minipollAnimation = "needs_demo",
                whatPlayerLearns = "The four basic needs system",
                minimumTime = 6f,
                positiveReinforcement = "עכשיו אתם מבינים מה Minipoll צריך! 🌟"
            },
            
            new ComfortableTutorialStep
            {
                emotionalGoal = "First caring interaction",
                title = "האכלה ראשונה 🍎",
                message = "הו לא! Minipoll נראה רעב... תראו איך הוא מסתכל עליכם בעיניים גדולות וחמודות! 🥺\n\nבואו נראה לו שאנחנו דואגים לו ונתן לו משהו טעים לאכול!",
                minipollEmotion = "hungry",
                minipollAnimation = "beg_for_food",
                whatPlayerLearns = "How to feed and care",
                isActionRequired = true,
                actionInstructions = "לחצו על כפתור האוכל 🍎 כדי להאכיל את Minipoll",
                minimumTime = 3f,
                positiveReinforcement = "וואו! ראיתם איך Minipoll התרגש? הוא מרגיש שאכפת לכם ממנו! 💕"
            },
            
            new ComfortableTutorialStep
            {
                emotionalGoal = "Recognizing emotional responses",
                title = "הבנת רגשות 😊💝",
                message = "הבחנתם איך Minipoll השתנה אחרי שקיבל אוכל? הפנים שלו, התנועות, הקולות - הכל מספר לכם מה הוא מרגיש!\n\nזה הקסם של הקשר - כשאתם דואגים לו, הוא מרגיש את האהבה שלכם.",
                minipollEmotion = "happy",
                minipollAnimation = "content_after_eating",
                whatPlayerLearns = "Emotional feedback and connection",
                minimumTime = 5f,
                positiveReinforcement = "אתם כבר לומדים לקרוא את הרגשות של Minipoll! 🎯"
            },
            
            new ComfortableTutorialStep
            {
                emotionalGoal = "Long-term relationship building",
                title = "בניית קשר מיוחד 🌟",
                message = "ככל שתבלו זמן עם Minipoll, הוא ילמד להכיר אתכם יותר טוב. הוא יזכור מה אתם אוהבים לעשות איתו, איך אתם מטפלים בו, ואפילו יפתח אישיות מיוחדת!\n\nכל Minipoll הוא ייחודי - בדיוק כמו חברות אמיתיות.",
                minipollEmotion = "bonding",
                minipollAnimation = "affectionate",
                whatPlayerLearns = "Personality development and memory",
                minimumTime = 6f,
                positiveReinforcement = "הקשר ביניכם כבר מתחיל להיבנות! 💞"
            },
            
            new ComfortableTutorialStep
            {
                emotionalGoal = "Encouragement and confidence",
                title = "אתם מוכנים! 🎊",
                message = "מדהים! אתם כבר יודעים את הבסיס של טיפול ב-Minipoll.\n\nזכרו - זה לא סתם משחק, זה חברות אמיתית. תהיו סבלניים, תתנו אהבה, ותראו איך Minipoll הופך להיות החבר הכי נאמן שלכם! 💕\n\nעכשיו... בואו נתחיל את ההרפתקה ביחד!",
                minipollEmotion = "excited",
                minipollAnimation = "ready_to_play",
                whatPlayerLearns = "Confidence to care independently",
                minimumTime = 5f,
                positiveReinforcement = "Minipoll מתרגש להתחיל את החיים החדשים איתכם! 🚀"
            }
        };
    }
}
