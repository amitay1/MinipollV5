/***************************************************************
 *  InteractableObject.cs
 *
 *  תיאור כללי:
 *    סקריפט גנרי לכל אובייקט בעולם שהמיניפול יכול "להשתמש" בו:
 *      - יכול להיות מזון (Food), מים (Water), צעצוע (Toy), מיטה (Bed) וכו’.
 *      - מכיל שדות לזיהוי הסוג, כמות, וכדומה.
 *      - כשהמיניפול "אוסף" את זה, הוא מפעיל OnInteract(...).
 *
 *  דרישות קדם:
 *    - למקם על אובייקט בעולם שרוצים שיהיה אינטראקטיבי, עם Collider (isTrigger=FALSE או TRUE לפי הצורך).
 *    - ייתכן שנרצה Tag או Layer מיוחדים ל-Food, Water, וכו’.
 *    - MinipollWorldInteraction.cs מזהה זאת ומפעיל את הפונקציות.
 *
 ***************************************************************/

using System;
using MinipollCore;
using MinipollGame.Core;
using UnityEngine;

public enum InteractableType
{
    Food,
    Water,
    Toy,
    Bed,
    Other
}

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    [Header("Interactable Settings")]
    public InteractableType objectType = InteractableType.Other;
    public int quantity = 10;        // כמות "שימוש" נגיד אוכל=10 מנות
    public bool consumedOnUse = true;// האם החפץ נעלם/מתמעט כשהשתמשנו בו

    [Header("Visual")]
    public bool highlightOnHover = true;
    public Color highlightColor = Color.yellow;
    private Color originalColor;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend)
        {
            originalColor = rend.material.color;
        }
    }

    /// <summary>
    /// קריאה כאשר מיניפול או שחקן רוצה להשתמש באובייקט
    /// </summary>
    /// <param name="caller">המיניפול שקרא לאינטראקציה</param>
    public void OnInteract(GameObject caller)
    {
        if (quantity <= 0)
        {
            // כבר אין מה לאסוף
            return;
        }

        // בהתאם לסוג, נניח משפיעים על צרכי המיניפול
        MinipollBrain brain = caller.GetComponent<MinipollBrain>();
        if (brain)
        {
            switch (objectType)
            {
                case InteractableType.Food:
                    // מעלים hunger
                    (brain.GetNeedsSystem() as NeedsSystem)?.FillNeed("Hunger", 30f); 
                    break;
                case InteractableType.Water:
                    // אולי מייצג צמא (אם היה לנו Need כזה)
                    // לדוגמה, אם אין Need, אפשר למלא Energy קצת...
                    (brain.GetNeedsSystem() as NeedsSystem)?.FillNeed("Energy", 10f);
                    break;
                case InteractableType.Toy:
                    // מעלה fun 
                    (brain.GetNeedsSystem() as NeedsSystem)?.FillNeed("Fun", 20f);
                    break;
                case InteractableType.Bed:
                    // גורם ל-Resting? 
                    (brain.GetMovementController() as MovementController)?.StartResting();
                    break;
                case InteractableType.Other:
                    // אפקט אחר או כלום
                    break;
            }
        }

        // הורדת כמות
        quantity--;
        if (quantity <= 0 && consumedOnUse)
        {
            // הורסים את האובייקט לגמרי
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// הדגשה ויזואלית כשהעכבר מעל האובייקט (לא חובה, רק דוגמה)
    /// </summary>
    private void OnMouseEnter()
    {
        if (highlightOnHover && rend)
        {
            rend.material.color = highlightColor;
        }
    }
    private void OnMouseExit()
    {
        if (highlightOnHover && rend)
        {
            rend.material.color = originalColor;
        }
    }
}

internal class MovementController
{
    internal void StartResting()
    {
        throw new NotImplementedException();
    }
}