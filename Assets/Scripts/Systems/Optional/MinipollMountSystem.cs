/***************************************************************
 *  MinipollMountSystem.cs
 *
 *  תיאור כללי:
 *    מודול "רכיבה" על יצורים. 
 *      - המיניפול יכול לאלף ולהפוך חיה ל-Mount
 *      - ברגע מורכב, השחקן/מיניפול משתלט על תנועת ה-Mount (או מואץ)
 *      - ירידה מהרכיבה/חזרה למצב רגיל
 *
 *  דרישות קדם:
 *    - יש "MountableCreature" בסצנה (יכול להיות עוד GameObject עם סקריפט שמאפשר הרכבה).
 *    - המיניפול יכול להחליט לעלות, אם היחסים/אמון מספיקים, וכו’.
 ***************************************************************/

using UnityEngine;
using MinipollCore;
using MinipollGame.Core;
using MinipollGame.Social;
public class MinipollMountSystem : MonoBehaviour
{
    private MinipollBrain brain;

    [Header("Current Mount")]
    public MountableCreature currentMount;
    public bool isMounted = false;

    [Header("Taming Requirements")]
    [Tooltip("רמת חברות נדרשת עם היצור כדי לאלף אותו")]
    public float requiredFriendship = 50f;
    [Tooltip("פריטים נדרשים (למשל \"Carrot\"=2)")]
    public string requiredItemName = "Carrot";
    public int requiredItemCount = 2;

    [Tooltip("מהירות ריצה מוגדלת בתנאי רכיבה")]
    public float mountSpeedMultiplier = 2f;

    private MinipollEconomySystem economy;
    private MinipollSocialRelations social;

    private float originalSpeed = 0f;

    private void Awake()
    {
        brain = GetComponent<MinipollBrain>();
        economy = GetComponent<MinipollEconomySystem>();
        social = GetComponent<MinipollSocialRelations>();
    }

    /// <summary>
    /// קריאה כאשר רוצים לנסות לאלף (Tame) יצור רכיב
    /// </summary>
    public bool TryTame(MountableCreature creature)
    {
        if (creature.isTamed) 
        {
            Debug.Log($"Creature {creature.name} already tamed!");
            return false;
        }

        // בודקים חברות (אם זה MinipollBrain => social relationship)
        float friendship = 0f;
        if (social != null && creature.ownerBrain != null)
        {
            var rel = social.GetRelationship(creature.ownerBrain);
            if (rel != null) 
                friendship = rel.friendship;
        }
        // בודקים חומרים
        int itemCount = economy.items.ContainsKey(requiredItemName) ? economy.items[requiredItemName] : 0;
        if (friendship < requiredFriendship || itemCount < requiredItemCount)
        {
            Debug.LogWarning($"Not enough friendship ({friendship}/{requiredFriendship}) or no {requiredItemName} for taming!");
            return false;
        }

        // צורכים פריטים
        economy.RemoveItem(requiredItemName, requiredItemCount);
        // הופך היצור ל-Tamed
        creature.isTamed = true;
        creature.ownerBrain = brain;
        Debug.Log($"{brain.name} tamed {creature.name} successfully!");
        return true;
    }

    /// <summary>
    /// קריאה כדי לעלות על יצור רכיב שכבר בוּיית
    /// </summary>
    public bool MountCreature(MountableCreature creature)
    {
        if (creature.isTamed && creature.ownerBrain == brain && !isMounted)
        {
            currentMount = creature;
            isMounted = true;
            // הגדלת מהירות
            int moveCtrl = (int)brain.GetMovementController();
            if (moveCtrl != null)
            {
                originalSpeed.Equals(moveCtrl);
                moveCtrl.Equals(moveCtrl * originalSpeed);
            }
            // אולי נשבית collider וכו’
            Debug.Log($"{brain.name} mounted {creature.name}!");
            return true;
        }
        return false;
    }

    /// <summary>
    /// ירידה מהרכיבה
    /// </summary>
    public void Dismount()
    {
        if (!isMounted) return;
        isMounted = false;

        // מחזירים מהירות
        object moveCtrl = brain.GetMovementController();
        if (moveCtrl != null && originalSpeed>0f)
        {
            moveCtrl.Equals(originalSpeed);
            originalSpeed = 0f;
        }
        Debug.Log($"{brain.name} dismounted from {currentMount.name}.");
        currentMount = null;
    }
}

/***************************************************************
 *  MountableCreature.cs
 *  סקריפט על היצור הרכיב עצמו
 ***************************************************************/
public class MountableCreature : MonoBehaviour
{
    public MinipollBrain ownerBrain;
    public bool isTamed = false;
}
