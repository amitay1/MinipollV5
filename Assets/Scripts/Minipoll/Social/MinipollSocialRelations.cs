/***************************************************************
 *  MinipollSocialRelations.cs
 *
 *  תיאור כללי:
 *    מודול ניהול יחסים חברתיים של המיניפול:
 *      - מעקב אחרי רשימת "מערכות יחסים" עם Minipoll אחרים
 *      - מדדים כמו trust/friendship/fear
 *      - עדכון ערכים לפי אינטראקציות (חיוביות/שליליות)
 *      - זיכרון אינטראקציות חוזרות (לא חובה, אפשר דרך MemorySystem)
 *      - רף/אירועים: אם fear גבוה מאוד => בריחה, וכו’
 *
 *  דרישות קדם:
 *    - ממוקם על אובייקט של מיניפול (יחד עם MinipollBrain).
 *    - MinipollBrain קורא InitSystem(this) ו-UpdateSocial(deltaTime).
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using MinipollGame.Core;

namespace MinipollGame.Social
{


    [System.Serializable]
    public partial class SocialRelationship
    {
        public MinipollBrain otherBrain;
        [Range(0f, 100f)] public float trust = 50f;
        [Range(0f, 100f)] public float friendship = 0f;
        [Range(0f, 100f)] public float fear = 0f;

        // אפשר להרחיב: אוהד/שונא, היסטוריית אינטראקציות וכו’.
        public SocialRelationship(MinipollBrain other)
        {
            otherBrain = other;
        }
    }

    public class MinipollSocialRelations : MonoBehaviour
    {
        private MinipollBrain brain;

        [Header("Relationships")]
        public List<SocialRelationship> relationships = new List<SocialRelationship>();

        [Header("Settings")]
        [Tooltip("רדיוס לזיהוי יצורים בסביבה וקביעת קשר נגדם")]
        public float detectionRadius = 10f;
        [Tooltip("טווח מרחק מקסימלי לשימור יחסים (אחרת דועך)")]
        public float maxRelationshipDistance = 30f;
        [Tooltip("כמה friendship עולה/יורד בהגעה לטווח קרוב / התרחקות")]
        public float friendshipGainPerSecond = 0.1f;
        public float friendshipLossPerSecond = 0.05f;
        [Tooltip("פחד גדל אם trust נמוך ויריב קרוב")]
        public float fearIncreaseRate = 0.2f;
        public float fearDecreaseRate = 0.05f;

        [Tooltip("אם fear>threshold, נעיר התנהגות בריחה")]
        public float fearThreshold = 60f;

        [Tooltip("האם להדפיס לוג על שינויים?")]
        public bool verbose = false;

        public void InitSystem(MinipollBrain ownerBrain)
        {
            brain = ownerBrain;
        }

        public void UpdateSocial(float deltaTime)
        {
            // 1) נאתר יצורים בסביבה שנוכל לעדכן יחס אליהם
            DetectNearbyMinipolls(deltaTime);

            // 2) עדכון דועך על יחסים רחוקים
            UpdateRelationshipsDistanceEffects(deltaTime);
        }

        /// <summary>
        /// איתור יצורים בקרבת המיניפול, הוספה לרשימת relationships אם לא קיים
        /// </summary>
        private void DetectNearbyMinipolls(float deltaTime)
        {
            Collider[] hits = Physics.OverlapSphere(brain.transform.position, detectionRadius);
            foreach (var c in hits)
            {
                if (c.gameObject == this.gameObject) continue;
                var otherBrain = c.GetComponent<MinipollBrain>();
                if (otherBrain != null)
                {
                    // בדוק אם כבר יש Relationship
                    SocialRelationship rel = relationships.Find(r => r.otherBrain == otherBrain);
                    if (rel == null)
                    {
                        // יצור קשר חדש
                        rel = new SocialRelationship(otherBrain);
                        relationships.Add(rel);

                        if (verbose)
                            Debug.Log($"{brain.name} started new relationship with {otherBrain.name}");
                    }

                    // עדכן נתוני חברות (הנחה: אם קרובים => מתיידדים?)
                    rel.friendship += friendshipGainPerSecond * deltaTime;
                    if (rel.friendship > 100f) rel.friendship = 100f;

                    // עדכן פחד (הנחה: אם trust נמוך => גדל פחד כשקרובים)
                    float trustFactor = rel.trust / 100f; // 0..1
                    float fearIncrement = (1f - trustFactor) * fearIncreaseRate * deltaTime;
                    rel.fear += fearIncrement;
                    if (rel.fear > 100f) rel.fear = 100f;
                }
            }
        }

        /// <summary>
        /// עדכון היחסים לפי מרחק מעבר לdetectionRadius.
        /// לדוגמה, אם יצור התרחק מ- 30f => ידידות דועכת.
        /// </summary>
        private void UpdateRelationshipsDistanceEffects(float deltaTime)
        {
            for (int i = relationships.Count - 1; i >= 0; i--)
            {
                var rel = relationships[i];
                if (rel.otherBrain == null)
                {
                    // יצור הפסיק להתקיים
                    relationships.RemoveAt(i);
                    continue;
                }

                float dist = Vector3.Distance(brain.transform.position, rel.otherBrain.transform.position);
                if (dist > maxRelationshipDistance)
                {
                    // התרחקו מאוד => דועכת יותר
                    rel.friendship -= friendshipLossPerSecond * 2f * deltaTime;
                }
                else
                {
                    // עדיין לא קרובים, אבל מעל detectionRadius => דועך רגיל
                    if (dist > detectionRadius)
                    {
                        rel.friendship -= friendshipLossPerSecond * deltaTime;
                    }
                }

                if (rel.friendship < 0f) rel.friendship = 0f;

                // פחד דועך אם לא קרוב
                if (dist > detectionRadius)
                {
                    rel.fear -= fearDecreaseRate * deltaTime;
                    if (rel.fear < 0f) rel.fear = 0f;
                }

                // אם יצור מת
                if (!rel.otherBrain.IsAlive)
                {
                    // אולי נוריד פחד כי האיום מת? או נסיר את הרלציה
                    relationships.RemoveAt(i);
                    if (verbose)
                        Debug.Log($"{brain.name} relationship removed - {rel.otherBrain.name} died.");
                    continue;
                }
            }
        }

        /// <summary>
        /// פונקציה (לדוגמה) לעדכן אמון/חברות חיובית
        /// </summary>
        public void IncreaseTrust(MinipollBrain other, float amount)
        {
            var rel = GetRelationship(other);
            if (rel == null)
            {
                rel = new SocialRelationship(other);
                relationships.Add(rel);
            }
            rel.trust += amount;
            if (rel.trust > 100f) rel.trust = 100f;

            if (verbose)
                Debug.Log($"{brain.name} trust++ with {other.name} => {rel.trust:F1}");
        }

        /// <summary>
        /// פונקציה (לדוגמה) לעדכן אמון/חברות שלילית
        /// </summary>
        public void DecreaseTrust(MinipollBrain other, float amount)
        {
            var rel = GetRelationship(other);
            if (rel == null)
            {
                rel = new SocialRelationship(other);
                relationships.Add(rel);
            }
            rel.trust -= amount;
            if (rel.trust < 0f) rel.trust = 0f;

            if (verbose)
                Debug.Log($"{brain.name} trust-- with {other.name} => {rel.trust:F1}");
        }

        /// <summary>
        /// מוצא relationship לאובייקט מסוים
        /// </summary>
        public SocialRelationship GetRelationship(MinipollBrain other)
        {
            return relationships.Find(r => r.otherBrain == other);
        }

        /// <summary>
        /// בודק אם הפחד שלנו ממישהו עובר סף => נוכל להחליט על בריחה
        /// </summary>
        public bool ShouldEscapeFromAny(out MinipollBrain threat)
        {
            threat = null;
            float maxFear = 0f;
            foreach (var rel in relationships)
            {
                if (rel.fear > fearThreshold && rel.fear > maxFear)
                {
                    threat = rel.otherBrain;
                    maxFear = rel.fear;
                }
            }
            return (threat != null);
        }    /// <summary>
             /// מאפשר או מבטל יכולת הזדווגות של המיניפול
             /// </summary>
        public void EnableMating(bool enabled)
        {
            // יישום פשוט - נוכל להוסיף פרופרטי matingEnabled במקום אחר במערכת
            if (verbose)
                Debug.Log($"{brain.name} mating {(enabled ? "enabled" : "disabled")}");
            // כאן אפשר להוסיף לוגיקה נוספת כמו שינוי פרמטרים או הפעלת מערכת רבייה
        }

        /// <summary>
        /// משנה את הערך של יחס עם מיניפול אחר (חיובי או שלילי)
        /// </summary>
        public void ModifyRelationship(MinipollBrain otherMinipoll, float amount)
        {
            if (otherMinipoll == null) return;

            var otherBrain = otherMinipoll.GetComponent<MinipollBrain>();
            if (otherBrain == null) return;

            var rel = GetRelationship(otherBrain);
            if (rel == null)
            {
                rel = new SocialRelationship(otherBrain);
                relationships.Add(rel);
            }

            // משנה את הידידות על בסיס הכמות
            rel.friendship += amount * 20f; // כפלנו ב-20 כדי שהשינוי יהיה יותר משמעותי
            rel.friendship = Mathf.Clamp(rel.friendship, 0f, 100f);

            // משנה את האמון בהתאם
            rel.trust += amount * 10f;
            rel.trust = Mathf.Clamp(rel.trust, 0f, 100f);

            // אם אינטראקציה שלילית, מגדיל פחד
            if (amount < 0f)
            {
                rel.fear += Mathf.Abs(amount) * 15f;
                rel.fear = Mathf.Clamp(rel.fear, 0f, 100f);
            }

            if (verbose)
                Debug.Log($"{brain.name} modified relationship with {otherMinipoll.name}: friendship={rel.friendship:F1}, trust={rel.trust:F1}, fear={rel.fear:F1}");
        }

        /// <summary>
        /// מעבד אינטראקציה חברתית עם מיניפול אחר
        /// </summary>
        public void ProcessSocialInteraction(MinipollBrain otherMinipoll)
        {
            if (otherMinipoll == null) return;

            var otherBrain = otherMinipoll.GetComponent<MinipollBrain>();
            if (otherBrain == null) return;

            var rel = GetRelationship(otherBrain);
            if (rel == null)
            {
                rel = new SocialRelationship(otherBrain);
                relationships.Add(rel);
            }

            // אינטראקציה חברתית בסיסית - מגדילה ידידות וקצת אמון
            rel.friendship += 2f;
            rel.trust += 1f;
            rel.friendship = Mathf.Clamp(rel.friendship, 0f, 100f);
            rel.trust = Mathf.Clamp(rel.trust, 0f, 100f);

            // מקטינה פחד אם היה
            rel.fear -= 1f;
            rel.fear = Mathf.Clamp(rel.fear, 0f, 100f);

            if (verbose)
                Debug.Log($"{brain.name} processed social interaction with {otherMinipoll.name}");
        }

        /// <summary>
        /// מחזיר רשימה של החברים (מיניפולים עם ידידות גבוהה)
        /// </summary>
        public IEnumerable<MinipollBrain> GetFriends()
        {
            foreach (var rel in relationships)
            {
                if (rel.otherBrain != null && rel.friendship >= 60f) // סף ידידות של 60
                {
                    yield return rel.otherBrain;
                }
            }
        }

        internal float GetRelationshipValue(int targetId)
        {
            throw new NotImplementedException();
        }

        internal void CallForHelp()
        {
            throw new NotImplementedException();
        }

        internal void ModifyRelationship(MinipollGame.Managers.MemoryManager attacker, float v)
        {
            throw new NotImplementedException();
        }

        internal void ProcessSocialInteraction(MinipollGame.Managers.MemoryManager other)
        {
            throw new NotImplementedException();
        }

        internal void ModifyRelationship(Core.MinipollCore healer, float v)
        {
            throw new NotImplementedException();
        }

        internal void ProcessSocialInteraction(Core.MinipollCore other)
        {
            throw new NotImplementedException();
        }
    }
}