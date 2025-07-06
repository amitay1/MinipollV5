/***************************************************************
 *  MinipollEmotionsSystem.cs
 *
 *  תיאור כללי:
 *    מודול המטפל ברגשות המיניפול:
 *      - רשימת רגשות פעילים (emotion type + intensity + decayRate + spreadRadius)
 *      - בכל פריים מעדכן דעיכה, מפיץ לרדיוס מסוים אם צריך
 *      - API להוספת רגש חדש (אם כבר קיים, הגברת העוצמה)
 *      - אפשרות לתגובות (למשל, כאשר הרגש מגיע לעוצמה גבוהה)
 *
 *  דרישות קדם:
 *    - למקם על אותו אובייקט עם MinipollBrain
 *    - MinipollBrain יקרא InitSystem(this) ו-UpdateEmotions(deltaTime)
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace MinipollCore.core
{
    

public class MinipollEmotionsSystem : MonoBehaviour
{
    [System.Serializable]
    public enum EmotionType
    {
        Happy,
        Sad,
        Afraid,
        Curious,
        Angry,
        Disgusted,
        Surprised,
        Trusting,
        Loving,
        Jealous,
        Embarrassed,
        Scared,
            Excited
            // ניתן להרחיב
        }

    [System.Serializable]
    public class EmotionStatus
    {
        public EmotionType emotion;
        [Range(0f,1f)]
        public float intensity;       // 0..1
        public float decayRate;       // (לשנייה)
        public float spreadRadius;    // רדיוס להפצת הרגש לאחרים

        public EmotionStatus(EmotionType e, float intensity, float decay, float spread)
        {
            this.emotion = e;
            this.intensity = intensity;
            this.decayRate = decay;
            this.spreadRadius = spread;
        }
    }

    private MinipollGame.Core.MinipollBrain brain;   // רפרנס ל"ראש"
    
    // Events for emotion and mood changes
    public System.Action<EmotionType, float> OnEmotionChanged;
    public System.Action<EmotionType> OnMoodChanged;
    
    [Header("Active Emotions")]
    public List<EmotionStatus> activeEmotions = new List<EmotionStatus>();

    [Header("Settings")]
    [Range(0f, 1f)]
    public float emotionSpreadChance = 0.2f; // הסתברות בפריים (או לשנייה) להפיץ רגש ברדיוס 
    public float globalDecayMultiplier = 1f; // להכפיל/להקטין כל decay
    public bool allowSpread = true;          // האם להתיר הדבקה בכלל
    public bool verbose = false;            // האם להדפיס לוג

    public void InitSystem(MinipollGame.Core.MinipollBrain ownerBrain)
    {
        brain = ownerBrain;
    }

    /// <summary>
    /// מעודכן בכל פריים ע"י MinipollBrain
    /// </summary>
    public void UpdateEmotions(float deltaTime)
    {
        for (int i = activeEmotions.Count - 1; i >= 0; i--)
        {
            var em = activeEmotions[i];
            // 1) דעיכה
            em.intensity -= em.decayRate * globalDecayMultiplier * deltaTime;
            if (em.intensity <= 0f)
            {
                // סיום הרגש
                if (verbose)
                    Debug.Log($"{brain.name} emotion {em.emotion} ended.");
                activeEmotions.RemoveAt(i);
            }
            else
            {
                // 2) הפצה (אם active)
                if (allowSpread && em.spreadRadius > 0f)
                {
                    // נניח סיכוי emotionSpreadChance * deltaTime
                    float chance = emotionSpreadChance * deltaTime;
                    if (Random.value < chance)
                    {
                        SpreadEmotion(em);
                    }
                }
            }
        }
    }

    /// <summary>
    /// הוספת רגש חדש או הגברת רגש קיים
    /// </summary>
    public void AddEmotion(EmotionType type, float intensity, float decayRate, float spreadRadius)
    {
        // בודקים אם הרגש כבר קיים
        var existing = activeEmotions.Find(e => e.emotion == type);
        if (existing == null)
        {
            // יוצרים חדש
            var newEmotion = new EmotionStatus(type, intensity, decayRate, spreadRadius);
            activeEmotions.Add(newEmotion);
            if (verbose)
                Debug.Log($"{brain.name} emotion ADDED {type} intensity={intensity:F2}");
        }
        else
        {
            // נגביר עוצמה
            float old = existing.intensity;
            existing.intensity = Mathf.Clamp01(existing.intensity + intensity);
            // נעדכן decay אם זה משמעותי (לא חובה)
            if (decayRate > existing.decayRate)
                existing.decayRate = decayRate;

            // נעדכן spreadRadius אם חדש יותר גדול
            if (spreadRadius > existing.spreadRadius)
                existing.spreadRadius = spreadRadius;

            if (verbose)
                Debug.Log($"{brain.name} emotion {type} intens UPDATED {old:F2} => {existing.intensity:F2}");
        }
        
        // Trigger emotion changed event
        OnEmotionChanged?.Invoke(type, intensity);
    }

    /// <summary>
    /// Add emotional event with default settings
    /// </summary>
    public void AddEmotionalEvent(EmotionType type, float intensity, GameObject source = null)
    {
        float decayRate = 0.02f; // Default decay rate
        float spreadRadius = 0f; // No spreading by default
        AddEmotion(type, intensity, decayRate, spreadRadius);
    }

    /// <summary>
    /// מפיץ חלקית את הרגש לאחרים ברדיוס
    /// </summary>
    private void SpreadEmotion(EmotionStatus em)
    {
        Collider[] hits = Physics.OverlapSphere(brain.transform.position, em.spreadRadius);
        foreach (var c in hits)
        {
            if (c.gameObject == this.gameObject) continue; // לא נדביק את עצמנו
            var otherBrain = c.GetComponent<MinipollGame.Core.MinipollBrain>();
            if (otherBrain != null)
            {
                // ניגש למערכת הרגש שלו
                MinipollEmotionsSystem otherEmotions = otherBrain.GetEmotionsSystem() as MinipollEmotionsSystem;
                if (otherEmotions != null)
                {
                    // מפיצים בעוצמה חלקית (נגיד חצי)
                    float newIntens = em.intensity * 0.5f;
                    otherEmotions.AddEmotion(em.emotion, newIntens, em.decayRate, em.spreadRadius);
                }
            }
        }
    }

    /// <summary>
    /// מחזיר את עוצמת רגש ספציפי (0 אם לא קיים)
    /// </summary>
    public float GetEmotionLevel(EmotionType type)
    {
        float level = 0f;
        foreach (var em in activeEmotions)
        {
            if (em.emotion == type && em.intensity > level)
            {
                level = em.intensity;
            }
        }
        return level;
    }

    /// <summary>
    /// מחזיר הרגש הדומיננטי הנוכחי (או null אם אין)
    /// </summary>
    public EmotionStatus GetDominantEmotion()
    {
        EmotionStatus top = null;
        float maxInt = 0f;
        foreach (var em in activeEmotions)
        {
            if (em.intensity > maxInt)
            {
                maxInt = em.intensity;
                top = em;
            }
        }
        return top;
    }

    internal float GetEmotionIntensity(string dominantEmotion)
    {
        // Parse the string to EmotionType and return its intensity
        if (System.Enum.TryParse<EmotionType>(dominantEmotion, true, out EmotionType emotionType))
        {
            return GetEmotionLevel(emotionType);
        }
        return 0f;
    }

    internal void AddEmotion(EmotionType emotionType, float influence)
    {
        // Use default settings for decay rate and spread radius
        float defaultDecayRate = 0.02f;
        float defaultSpreadRadius = 0f;
        AddEmotion(emotionType, influence, defaultDecayRate, defaultSpreadRadius);
    }

    /// <summary>
    /// Maps external MinipollCore.Emotion to local EmotionType
    /// </summary>
    private EmotionType MapExternalToLocalEmotion(MinipollCore.Emotion externalEmotion)
    {
        switch (externalEmotion)
        {
            case MinipollCore.Emotion.Happiness: return EmotionType.Happy;
            case MinipollCore.Emotion.Sadness: return EmotionType.Sad;
            case MinipollCore.Emotion.Anger: return EmotionType.Angry;
            case MinipollCore.Emotion.Fear: return EmotionType.Afraid;
            case MinipollCore.Emotion.Surprise: return EmotionType.Surprised;
            case MinipollCore.Emotion.Disgust: return EmotionType.Disgusted;
            case MinipollCore.Emotion.Trust: return EmotionType.Trusting;
            case MinipollCore.Emotion.Love: return EmotionType.Loving;
            case MinipollCore.Emotion.Excitement: return EmotionType.Excited;
            case MinipollCore.Emotion.Wonder: return EmotionType.Curious;
            case MinipollCore.Emotion.Anxiety: return EmotionType.Scared;
            case MinipollCore.Emotion.Shame: return EmotionType.Embarrassed;
            default: return EmotionType.Happy;
        }
    }

    /// <summary>
    /// Maps MinipollGame.Systems.Core.EmotionType to local EmotionType
    /// </summary>
    private EmotionType MapCoreToLocalEmotion(MinipollGame.Systems.Core.EmotionType coreEmotion)
    {
        switch (coreEmotion)
        {
            case MinipollGame.Systems.Core.EmotionType.Happy: return EmotionType.Happy;
            case MinipollGame.Systems.Core.EmotionType.Sad: return EmotionType.Sad;
            case MinipollGame.Systems.Core.EmotionType.Angry: return EmotionType.Angry;
            case MinipollGame.Systems.Core.EmotionType.Fear: return EmotionType.Afraid;
            case MinipollGame.Systems.Core.EmotionType.Surprised: return EmotionType.Surprised;
            case MinipollGame.Systems.Core.EmotionType.Disgusted: return EmotionType.Disgusted;
            case MinipollGame.Systems.Core.EmotionType.Love: return EmotionType.Loving;
            case MinipollGame.Systems.Core.EmotionType.Excited: return EmotionType.Excited;
            case MinipollGame.Systems.Core.EmotionType.Jealous: return EmotionType.Jealous;
            case MinipollGame.Systems.Core.EmotionType.Ashamed: return EmotionType.Embarrassed;
            case MinipollGame.Systems.Core.EmotionType.Confused: return EmotionType.Curious;
            default: return EmotionType.Happy;
        }
    }

    internal void ChangeEmotion(object emotionObj, float intensity)
    {
        // Handle different emotion object types
        if (emotionObj is EmotionType localEmotion)
        {
            AddEmotion(localEmotion, intensity);
        }
        else if (emotionObj is MinipollCore.Emotion externalEmotion)
        {
            EmotionType mappedEmotion = MapExternalToLocalEmotion(externalEmotion);
            AddEmotion(mappedEmotion, intensity);
        }
        else if (emotionObj is MinipollGame.Systems.Core.EmotionType coreEmotion)
        {
            EmotionType mappedEmotion = MapCoreToLocalEmotion(coreEmotion);
            AddEmotion(mappedEmotion, intensity);
        }
        else if (emotionObj is string emotionString)
        {
            if (System.Enum.TryParse<EmotionType>(emotionString, true, out EmotionType parsedEmotion))
            {
                AddEmotion(parsedEmotion, intensity);
            }
        }
    }

    internal void ChangeEmotionEnum(MinipollCore.Emotion externalEmotion, float intensity)
    {
        EmotionType mappedEmotion = MapExternalToLocalEmotion(externalEmotion);
        AddEmotion(mappedEmotion, intensity);
    }

        internal void AddEmotionalEvent(MinipollEmotionsSystem sourceSystem, float intensity)
        {
            // This method appears to be for emotion contagion from another emotions system
            // We can add a default emotion based on the source system's dominant emotion
            if (sourceSystem != null)
            {
                var dominantEmotion = sourceSystem.GetDominantEmotion();
                if (dominantEmotion != null)
                {
                    AddEmotionalEvent(dominantEmotion.emotion, intensity);
                }
                else
                {
                    // Default to curiosity if no dominant emotion
                    AddEmotionalEvent(EmotionType.Curious, intensity);
                }
            }
        }

        internal void AddEmotionalEvent(EmotionType emotionType, float intensity)
        {
            // Use default settings for emotional events
            float defaultDecayRate = 0.05f; // Slightly faster decay for events
            float defaultSpreadRadius = 2f; // Events can spread to nearby characters
            AddEmotion(emotionType, intensity, defaultDecayRate, defaultSpreadRadius);
        }

        internal void AddEmotionalEvent(MinipollGame.Systems.Core.EmotionType coreEmotion, float intensity)
        {
            EmotionType mappedEmotion = MapCoreToLocalEmotion(coreEmotion);
            AddEmotionalEvent(mappedEmotion, intensity);
        }
    }
}