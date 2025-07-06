/***************************************************************
 *  MinipollMemorySystem.cs
 *
 *  תיאור כללי:
 *    מודול זיכרון ולמידה בסיסית:
 *      - שומר אירועים (תאריך, תוכן, עצם קשור, האם חיובי/שלילי, עוצמה)
 *      - מסיר אירועים ישנים (מעל X זמן)
 *      - אפשר להרחיב: ללמוד מעשים קודמים של מיניפולים אחרים, להשפיע על רגש/יחסים
 *
 *  דרישות קדם:
 *    - למקם על אובייקט של המיניפול. MinipollBrain יקרא InitSystem(this) ו-UpdateMemory(deltaTime).
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using MinipollGame.Systems.Core;
using Unity.VisualScripting;

namespace MinipollGame.Core
{
    [System.Serializable]
    public class MemoryEvent
    {
        public string description;
        public GameObject relatedObject; // יכול להיות Minipoll אחר או item בעולם
        public float timestamp;          // Time.time
        public bool positive;            // אירוע חיובי או שלילי
        public float emotionalImpact;    // כמה זה משפיע רגשית (0..1 או יותר)
        public MemoryEvent(string desc, GameObject relObj, bool pos, float impact)
        {
            description = desc;
            relatedObject = relObj;
            timestamp = Time.time;
            positive = pos;
            emotionalImpact = impact;
        }
    }

    public class MinipollMemorySystem : MonoBehaviour
    {
        private MinipollBrain brain;

        [Header("Memory Storage")]
        public List<MemoryEvent> memories = new List<MemoryEvent>();

        [Header("Memory Limits")]
        [Tooltip("כמה אירועים מקסימלי נשמור בזיכרון? (שאר הישנים נמחקים)")]
        public int maxMemories = 50;
        [Tooltip("זמן (בשניות) אחרי כמה מוחקים אירוע מהזיכרון")]
        public float forgetAfterSeconds = 120f; // 2 דקות לדוגמה

        [Header("Learning Impact")]
        public bool enableEmotionIntegration = true;
        [Tooltip("אם אירוע חיובי, נוסיף רגש שמחה (Happy) בעוצמה=emotionalImpact * positiveEmotionMultiplier")]
        public float positiveEmotionMultiplier = 0.5f;
        [Tooltip("אם אירוע שלילי, נוסיף רגש עצב או פחד (תלוי במקרה)")]
        public float negativeEmotionMultiplier = 0.5f;
        private object emoSys;

        // אפשר להרחיב גם ל-relationship learning

        public void InitSystem(MinipollBrain ownerBrain)
        {
            brain = ownerBrain;
        }

        public void UpdateMemory(float deltaTime)
        {
            ForgetOldMemories();
            LimitMemoryCapacity();

            // אפשר להוסיף לוגיקת עיבוד אם צריך (למשל, ליצור רגשות מורכבים מאירועים)
        }

        /// <summary>
        /// הוספת אירוע לזיכרון
        /// </summary>
        public void AddMemory(string desc, GameObject relatedObj, bool positive, float impact)
        {
            var me = new MemoryEvent(desc, relatedObj, positive, impact);
            memories.Add(me);        // השפעה מיידית על רגשות (אם מוגדר)
            if (enableEmotionIntegration && brain != null)
            {
                emoSys = brain.GetEmotionsSystem();
                if (emoSys != null)
                {
                    if (positive)
                    {
                        // נוסיף רגש שמח - TODO: implement when EmotionsSystem is ready
                        // emoSys.AddEmotion(EmotionType.Happy, impact * positiveEmotionMultiplier, 0.01f, 0f);
                    }
                    else
                    {
                        // שלילי: נוסיף רגש עצב או פחד - TODO: implement when EmotionsSystem is ready
                        // emoSys.AddEmotion(EmotionType.Sad, impact * negativeEmotionMultiplier, 0.02f, 0f);
                    }
                }
            }
        }

        /// <summary>
        /// מוחק אירועים שמעל זמן forgetAfterSeconds
        /// </summary>
        private void ForgetOldMemories()
        {
            float now = Time.time;
            for (int i = memories.Count - 1; i >= 0; i--)
            {
                float age = now - memories[i].timestamp;
                if (age > forgetAfterSeconds)
                {
                    memories.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// מגביל את כמות הזיכרונות
        /// </summary>
        private void LimitMemoryCapacity()
        {
            while (memories.Count > maxMemories)
            {
                memories.RemoveAt(0); // מוחק את הכי ישן (בתחילת הרשימה)
            }
        }

        #region Public Helpers

        public List<MemoryEvent> GetAllMemories()
        {
            return memories;
        }        /// <summary>
        /// רושם אינטראקציה בזיכרון עם מיניפול אחר
        /// </summary>
        public void RememberInteraction(MinipollCore otherMinipoll, InteractionType interactionType, float emotionalImpact)
        {
            if (otherMinipoll == null) return;

            string description = $"Interaction: {interactionType} with {otherMinipoll.name}";
            bool isPositive = emotionalImpact > 0f;

            AddMemory(description, otherMinipoll.gameObject, isPositive, Mathf.Abs(emotionalImpact));
        }

        /// <summary>
        /// רושם אינטראקציה בזיכרון (מתודה חלופית)
        /// </summary>
        public void RememberInteraction(AgeStage ageStage, InteractionType interactionType, float emotionalImpact)
        {
            string description = $"Interaction: {interactionType} involving {ageStage} stage";
            bool isPositive = emotionalImpact > 0f;

            AddMemory(description, null, isPositive, Mathf.Abs(emotionalImpact));
        }

        // אפשר פונקציות לחיפוש: FindRecentEventsAboutObject(GameObject obj)
        #endregion
    }
}