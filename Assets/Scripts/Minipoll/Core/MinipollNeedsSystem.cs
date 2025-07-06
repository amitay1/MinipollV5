/***************************************************************
 *  MinipollNeedsSystem.cs
 *
 *  תיאור כללי:
 *    מטפל בצרכים (Needs) של המיניפול: אנרגיה, רעב, חברה, כיף, היגיינה.
 *    - שומר ערכים נוכחיים (0..100)
 *    - מחיל שחיקה (decreaseRate) או עלייה (increase) בתנאים שונים
 *    - ממשק עדכון צרכים (לדוגמה: EatFood => מעלה Hunger)
 *    - אם צורך קריטי לרמה נמוכה מדי, גורם נזק לבריאות
 *
 *  דרישות קדם:
 *    - להניח סקריפט זה על האובייקט של המיניפול (יחד עם MinipollBrain)
 *    - MinipollBrain קורא InitSystem(this) ו-UpdateNeeds() בכל פריים.
 *
 ***************************************************************/

using System;
using MinipollGame.Systems.Core;
using UnityEngine;

namespace MinipollGame.Core
{
    [System.Serializable]
    public class Need
    {
        public string needName;           // "Energy", "Hunger", "Social", ...
        [Range(0f, 100f)]
        public float currentValue = 100f; // התחלה מלאה
        public float decreaseRate = 1f;   // ירידה לשנייה
        public float criticalThreshold = 10f; // מתחת לערך זה נחשב קריטי

        [Tooltip("כמה נזק נגרם לבריאות בכל שנייה כשהצורך מתחת לסף הקריטי")]
        public float healthDamagePerSecondWhenCritical = 1f;

        // פונקציה פנימית להורדת ערך
        public void Decrease(float deltaTime)
        {
            currentValue -= decreaseRate * deltaTime;
            if (currentValue < 0f) currentValue = 0f;
        }

        // הוספה ידנית (לא בהכרח עקבית בשיעור לזמן)
        public void Increase(float amount)
        {
            currentValue += amount;
            if (currentValue > 100f) currentValue = 100f;
        }

        // בדיקת קריטי
        public bool IsCritical()
        {
            return (currentValue <= criticalThreshold);
        }        public void FillValue(float v)
        {
            currentValue = Mathf.Clamp(v, 0f, 100f);
        }
    }

    public class MinipollNeedsSystem : MonoBehaviour
    {
        private MinipollBrain brain; // Owner reference

        [Header("Needs List")]
        public Need energy = new Need { needName = "Energy", currentValue = 80f, decreaseRate = 0.2f, criticalThreshold = 10f, healthDamagePerSecondWhenCritical = 2f };
        public Need hunger = new Need { needName = "Hunger", currentValue = 60f, decreaseRate = 0.3f, criticalThreshold = 10f, healthDamagePerSecondWhenCritical = 4f };
        public Need social = new Need { needName = "Social", currentValue = 70f, decreaseRate = 0.1f, criticalThreshold = 5f, healthDamagePerSecondWhenCritical = 1f };
        public Need fun    = new Need { needName = "Fun",    currentValue = 50f, decreaseRate = 0.1f, criticalThreshold = 5f, healthDamagePerSecondWhenCritical = 1f };
        public Need hygiene= new Need { needName = "Hygiene",currentValue = 60f, decreaseRate = 0.05f,criticalThreshold = 10f,healthDamagePerSecondWhenCritical = 1.5f };

        [Header("Modifiers")]
        [Tooltip("מכפיל כללי על הירידה בצרכים בלילה (לדוגמה - אם רוצים שיהיה שונה מהיום)")]
        public float nightNeedsMultiplier = 1.2f;

        [Tooltip("האם צרכים קריטיים לאורך זמן יגרמו נזק למיניפול?")]
        public bool enableCriticalDamage = true;

        [Tooltip("אם true, נדפיס לוג בהגיע צורך לסף נמוך מ-10 למשל")]
        public bool verbose = false;

        public Action<NeedType> OnNeedCritical { get; internal set; }
        public Action<NeedType> OnNeedSatisfied { get; internal set; }

        // InitSystem נקרא מ-MinipollBrain
        public void InitSystem(MinipollBrain ownerBrain)
        {
            brain = ownerBrain;
        }

        public void UpdateNeeds(float deltaTime)
        {
            // 1) בירור אם לילה כדי ליישם מכפיל
            float multiplier = 1f;
            if (WorldManager.Instance != null)
            {
                float sunlight = WorldManager.Instance.timeSystem.GetSunlightFactor();
                // נניח ש-0..0.3 זה לילה
                if (sunlight < 0.3f)
                {
                    multiplier = nightNeedsMultiplier;
                }
            }

            // 2) מחילים ירידה על כל הצרכים
            DecreaseNeed(energy, deltaTime, multiplier);
            DecreaseNeed(hunger, deltaTime, multiplier);
            DecreaseNeed(social, deltaTime, multiplier);
            DecreaseNeed(fun, deltaTime, multiplier);
            DecreaseNeed(hygiene, deltaTime, multiplier);

            // 3) בדיקה אם צרכים קריטיים => פוגעים בבריאות
            if (enableCriticalDamage && brain != null)
            {
                ApplyCriticalDamage(energy, deltaTime);
                ApplyCriticalDamage(hunger, deltaTime);
                ApplyCriticalDamage(social, deltaTime);
                ApplyCriticalDamage(fun, deltaTime);
                ApplyCriticalDamage(hygiene, deltaTime);
            }
        }

        private void DecreaseNeed(Need need, float deltaTime, float multiplier)
        {
            float oldVal = need.currentValue;
            need.currentValue -= (need.decreaseRate * multiplier * deltaTime);
            if (need.currentValue < 0f) need.currentValue = 0f;

            if (verbose && oldVal > 10f && need.currentValue <= 10f)
            {
                Debug.Log($"{brain.name}'s need {need.needName} is critical! Value={need.currentValue:F1}");
            }
        }

        private void ApplyCriticalDamage(Need need, float deltaTime)
        {
            if (need.IsCritical())
            {
                brain.TakeDamage(need.healthDamagePerSecondWhenCritical * deltaTime);
            }
        }

        #region Public Methods for Fulfilling Needs

        /// <summary>
        /// פונקציה למילוי חלקי של צורך מסוים:
        /// למשל, אכילה => 
        ///   FillNeed("Hunger", 20f);
        /// </summary>
        public void FillNeed(string needName, float amount)
        {
            Need target = GetNeedByName(needName);
            if (target != null)
            {
                target.Increase(amount);
                if (verbose)
                    Debug.Log($"{brain.name}'s {needName} increased by {amount}, new val={target.currentValue}");
            }
        }        public Need GetNeedByName(string needName)
        {
            switch (needName.ToLower())
            {
                case "energy": return energy;
                case "hunger": return hunger;
                case "thirst": return hygiene; // For now, using hygiene as thirst substitute
                case "social": return social;
                case "fun":    return fun;
                case "hygiene":return hygiene;
            }
            return null;
        }

        /// <summary>
        /// Gets the normalized value (0-1) of a specific need
        /// </summary>
        /// <param name="needName">Name of the need (Hunger, Energy, Social, Fun, Hygiene)</param>
        /// <returns>Normalized value between 0 and 1</returns>
        public float GetNormalizedNeed(string needName)
        {
            Need need = GetNeedByName(needName);
            if (need != null)
            {
                return need.currentValue / 100f;
            }
            Debug.LogWarning($"Unknown need: {needName}");
            return 0f;
        }        public void Drink(float amount)
        {
            FillNeed("Thirst", amount);
        }

        public void Sleep(float quality)
        {
            FillNeed("Energy", quality * 30f); // Sleep restores energy
        }

        public void Socialize(float v)
        {
            FillNeed("Social", v);
        }        public float GetNeedValue(NeedType needType)
        {
            switch (needType)
            {
                case NeedType.Hunger: return hunger.currentValue;
                case NeedType.Thirst: 
                    // For now, use hygiene as thirst substitute
                    return hygiene.currentValue;
                case NeedType.Sleep:
                    return energy.currentValue; // Sleep affects energy
                case NeedType.Social: return social.currentValue;
                case NeedType.Fun: return fun.currentValue;
                case NeedType.Hygiene: return hygiene.currentValue;
                default: return 50f;
            }
        }

        #endregion
    }
}
