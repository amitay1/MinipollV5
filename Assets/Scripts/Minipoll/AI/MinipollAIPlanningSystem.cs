/***************************************************************
 *  MinipollAIPlanningSystem.cs
 *
 *  תיאור כללי:
 *    מערכת תכנון יעד-פעולה (GOAP - Goal Oriented Action Planning):
 *      - יש למיניפול "מטרות" (Goals) עם עדיפויות, והמערכת מחפשת רצף "פעולות" (Actions)
 *        שיביאו להשגת מטרותיו, תוך התאמה למצב הנוכחי.
 *      - בכל פרק זמן בודקים את המצב (צרכים, מרחקים וכו’), בונים תוכנית פעולה (plan)
 *        ומבצעים שלב־שלב.
 *
 *  דרישות קדם:
 *    - מניחים שכל Minipoll שמפעיל GOAP מחזיק את הסקריפט הזה (או Manager חיצוני).
 *    - דרושה רשימת Actions מוגדרת (לאו דווקא בקובץ זה; אפשר להגדיר מחלקות ירושה).
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;



namespace MinipollGame.AI
{

    [System.Serializable]
    public class GoapGoal
    {
        public string goalName;
        public float priority;
        // ייתכן שנחזיק תנאי הצלחה (worldState) או פונקציית בדיקה
        public Func<bool> isGoalSatisfied;  // אם goal כבר מושג
    }

    [System.Serializable]
    public class GoapAction
    {
        public string actionName;
        // קדם-תנאים, מה צריך בעולם כדי לבצע
        public Func<bool> preconditions;
        // תוצאות - שינוי בעולם אחרי ביצוע
        public Action applyEffect;
        // עלות פעולה, לצורכי חיפוש
        public float cost;
        // משך הביצוע
        public float duration;

        // האם אפשר כרגע לבצע?
        public bool CanPerform()
        {
            if (preconditions == null) return true;
            return preconditions.Invoke();
        }

        // ביצוע הפעולה עצמה
        public void Perform()
        {
            applyEffect?.Invoke();
        }
    }

    public enum GoapState
    {
        Idle,
        Planning,
        ExecutingPlan
    }

    public class MinipollAIPlanningSystem : MonoBehaviour
    {
        [Header("Goals")]
        public List<GoapGoal> goals = new List<GoapGoal>();

        [Header("Actions Bank")]
        public List<GoapAction> availableActions = new List<GoapAction>();

        [Header("Debug")]
        public GoapState currentState = GoapState.Idle;
        public List<GoapAction> currentPlan = new List<GoapAction>();
        private int planIndex = 0;
        private float actionTimer = 0f;
        public bool verboseLogging = false;

        private void Update()
        {
            switch (currentState)
            {
                case GoapState.Idle:
                    // בודקים אם יש מטרה לא מושגת
                    GoapGoal topGoal = GetTopGoal();
                    if (topGoal != null && (topGoal.isGoalSatisfied == null || !topGoal.isGoalSatisfied()))
                    {
                        currentState = GoapState.Planning;
                    }
                    break;

                case GoapState.Planning:
                    if (verboseLogging) Debug.Log($"{name}: Planning for top goal...");
                    // כאן נבצע חיפוש תוכנית (תכנון).
                    // לצורך הדגמה: ניקח את סדר הפעולות שאפשר לבצע, בלי A* מורכב.
                    currentPlan = CreateSimplePlan();
                    planIndex = 0;
                    if (currentPlan.Count > 0)
                    {
                        currentState = GoapState.ExecutingPlan;
                        if (verboseLogging) Debug.Log($"{name}: Found plan with {currentPlan.Count} actions");
                    }
                    else
                    {
                        // אין תוכנית => Idle
                        currentState = GoapState.Idle;
                    }
                    break;

                case GoapState.ExecutingPlan:
                    if (planIndex >= currentPlan.Count)
                    {
                        // סיימנו התוכנית
                        currentPlan.Clear();
                        currentState = GoapState.Idle;
                        break;
                    }
                    ExecuteCurrentAction();
                    break;
            }
        }

        /// <summary>
        /// מוצא מטרה עם עדיפות גבוהה ביותר שעדיין לא מושגת
        /// </summary>
        private GoapGoal GetTopGoal()
        {
            GoapGoal best = null;
            float bestPriority = float.MinValue;
            foreach (var g in goals)
            {
                bool satisfied = (g.isGoalSatisfied != null && g.isGoalSatisfied());
                if (!satisfied && g.priority > bestPriority)
                {
                    bestPriority = g.priority;
                    best = g;
                }
            }
            return best;
        }

        /// <summary>
        /// יצירת תוכנית פשוטה (דוגמה מפושטת): 
        /// מחזירה רשימה של Actions שאפשר לבצע (באותו סדר שהוגדר ב-availableActions)
        /// </summary>
        private List<GoapAction> CreateSimplePlan()
        {
            List<GoapAction> plan = new List<GoapAction>();
            // רעיון: נחפש את כל הפעולות CanPerform= true, נוסיף לסדר 
            // כמובן שב-GOAP אמיתי מבוצע חיפוש/גרף
            for (int i = 0; i < availableActions.Count; i++)
            {
                if (availableActions[i].CanPerform())
                {
                    plan.Add(availableActions[i]);
                }
            }
            return plan;
        }

        private void ExecuteCurrentAction()
        {
            var act = currentPlan[planIndex];
            if (act == null)
            {
                planIndex++;
                return;
            }
            if (actionTimer == 0f && verboseLogging)
            {
                Debug.Log($"{name}: Starting action '{act.actionName}'");
            }

            actionTimer += Time.deltaTime;
            if (actionTimer >= act.duration)
            {
                // מסיימים פעולה
                act.Perform();
                if (verboseLogging)
                {
                    Debug.Log($"{name}: Finished action '{act.actionName}'");
                }
                planIndex++;
                actionTimer = 0f;
            }
        }
    }
}    