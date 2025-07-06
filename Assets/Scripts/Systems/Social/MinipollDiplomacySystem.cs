/***************************************************************
 *  MinipollDiplomacySystem.cs
 *
 *  תיאור כללי:
 *    סקריפט לטיפול בדיפלומטיה בין שבטים/קבוצות מיניפולים.
 *      - מנגנון "סכמי שלום" (פקטורים של שלום/איבה),
 *      - אפשרות לכרות ברית בין שני שבטים או להכריז מלחמה,
 *      - קביעת השפעה על היחסים החברתיים בין חברי שבטים אלה.
 *
 *  דרישות קדם:
 *    - קיים TribeManager ו-Tribe (מיניפול טרייב סיסטם).
 *    - מנהל גלובלי אחד: DiplomacyManager (דומה ל-TribeManager).
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for Find
using System;
using MinipollGame.Core;
using MinipollGame.Social; // Required for Action

[System.Serializable]
public enum DiplomaticStatus
{
    Neutral,
    Alliance,
    War
}

[System.Serializable]
public class TribeDiplomacy
{
    public Tribe tribeA;
    public Tribe tribeB;
    public DiplomaticStatus status = DiplomaticStatus.Neutral;
    public float goodwill = 50f;  // 0..100
    public float tension = 0f;    // 0..100 => אם גבוה מאוד => מלחמה

    public TribeDiplomacy(Tribe a, Tribe b)
    {
        tribeA = a;
        tribeB = b;
    }
}

public class DiplomacyManager : MonoBehaviour
{
    public static DiplomacyManager Instance;
    public List<TribeDiplomacy> allDiplomacies = new List<TribeDiplomacy>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// מוצא או יוצר רשומת דיפלומטיה בין שני שבטים
    /// </summary>
    public TribeDiplomacy GetOrCreateDiplomacy(Tribe a, Tribe b)
    {
        if (a == b) return null; // לא הגיוני שבט עם עצמו

        // סדר אחיד כדי למנוע כפילויות כמו (a,b) ו-(b,a)
        Tribe firstTribe = a;
        Tribe secondTribe = b;
        if (string.Compare(a.tribeName, b.tribeName) > 0)
        {
            firstTribe = b;
            secondTribe = a;
        }

        var d = allDiplomacies.Find(x => (x.tribeA == firstTribe && x.tribeB == secondTribe));
        if (d == null)
        {
            d = new TribeDiplomacy(firstTribe, secondTribe);
            allDiplomacies.Add(d); // הוספת הדיפלומטיה החדשה לרשימה
        }
        return d; // החזרת הדיפלומטיה שנמצאה או נוצרה
    }

    /// <summary>
    /// בכל פריים/זמן, נעדכן tension, goodwill בהתאם לאירועים וכו’
    /// </summary>
    public void UpdateDiplomacies(float deltaTime)
    {
        foreach (var d in allDiplomacies)
        {
            // אפשר להחיל שינויים איטיים: tension ירד עם הזמן אם ב-Neutral
            if (d.status == DiplomaticStatus.Neutral)
            {
                d.tension = Mathf.Max(0f, d.tension - 0.05f * deltaTime);
            }
            else if (d.status == DiplomaticStatus.Alliance)
            {
                // נשמור goodwill גבוה
                d.goodwill = Mathf.Min(100f, d.goodwill + 0.01f * deltaTime);
                d.tension = Mathf.Max(0f, d.tension - 0.1f * deltaTime);
            }
            else if (d.status == DiplomaticStatus.War)
            {
                // במלחמה tension עולה או נשאר גבוה
                d.tension = Mathf.Min(100f, d.tension + 0.1f * deltaTime);
                // goodwill כנראה יורד
                d.goodwill = Mathf.Max(0f, d.goodwill - 0.1f * deltaTime);
            }

            // אם tension>80 => מלחמה
            if (d.tension > 80f && d.status != DiplomaticStatus.War)
            {
                SetDiplomaticStatus(d, DiplomaticStatus.War);
            }
            // אם goodwill>80 => ברית
            if (d.goodwill > 80f && d.status != DiplomaticStatus.Alliance)
            {
                SetDiplomaticStatus(d, DiplomaticStatus.Alliance);
            }
        }
    }

    /// <summary>
    /// החלפת סטטוס דיפלומטי (מלחמה/ברית/נייטרלי).
    /// משפיע על היחסים בין כל חברי השבטים.
    /// </summary>
    public void SetDiplomaticStatus(TribeDiplomacy d, DiplomaticStatus newStatus)
    {
        d.status = newStatus;
        switch (newStatus)
        {
            case DiplomaticStatus.Neutral:
                Debug.Log($"Tribes {d.tribeA.tribeName} and {d.tribeB.tribeName} are now Neutral.");
                break;
            case DiplomaticStatus.Alliance:
                Debug.Log($"Tribes {d.tribeA.tribeName} and {d.tribeB.tribeName} formed an ALLIANCE!");
                BoostFriendshipBetweenTribes(d.tribeA, d.tribeB, +20f);
                break;
            case DiplomaticStatus.War:
                Debug.Log($"Tribes {d.tribeA.tribeName} and {d.tribeB.tribeName} declared WAR!");
                BoostFriendshipBetweenTribes(d.tribeA, d.tribeB, -50f);
                break;
        }
    }

    /// <summary>
    /// במקרה של שינוי משמעותי (ברית/מלחמה), משפיע על רמת ה-friendship בין כל חברי השבטים
    /// </summary>
    private void BoostFriendshipBetweenTribes(Tribe a, Tribe b, float delta)
    {
        foreach (var memA in a.members)
        {
            foreach (var memB in b.members)
            {
                var soA = memA.GetComponent<MinipollSocialRelations>();
                if (soA != null)
                {
                    soA.DecreaseTrust(memB.GetComponent<MinipollBrain>(), -delta); 
                    // -delta => אם delta חיובי => מגדיל trust, אם delta שלילי => מוריד
                }
                var soB = memB.GetComponent<MinipollSocialRelations>();
                if (soB != null)
                {
                    soB.DecreaseTrust(memA.GetComponent<MinipollBrain>(), -delta);
                }
            }        }
    }
}

/// <summary>
/// Component-based diplomacy system for individual Minipolls
/// </summary>
public class MinipollDiplomacySystem : MonoBehaviour
{
    private MinipollBrain brain;
    private MinipollTribeSystem tribeSystem;

    // Events
    public static event Action<Tribe, Tribe> OnWarDeclared;
    public static event Action<Tribe, Tribe> OnPeaceMade;

    private void Awake()
    {
        brain = GetComponent<MinipollBrain>();
        tribeSystem = GetComponent<MinipollTribeSystem>();
    }

    /// <summary>
    /// Check if this Minipoll's tribe is at war with the target Minipoll's tribe
    /// </summary>
    public bool IsAtWarWith(MinipollBrain target)
    {
        if (target == null || tribeSystem == null) return false;
        
        var targetTribeSystem = target.GetComponent<MinipollTribeSystem>();
        if (targetTribeSystem == null) return false;

        var myTribe = tribeSystem.GetTribe();
        var targetTribe = targetTribeSystem.GetTribe();
        
        if (myTribe == null || targetTribe == null || myTribe == targetTribe) return false;

        var diplomacyManager = DiplomacyManager.Instance;
        if (diplomacyManager == null) return false;

        var diplomacy = diplomacyManager.GetOrCreateDiplomacy(myTribe, targetTribe);
        return diplomacy?.status == DiplomaticStatus.War;
    }

    /// <summary>
    /// Trigger war event
    /// </summary>
    public static void TriggerWar(Tribe tribeA, Tribe tribeB)
    {
        OnWarDeclared?.Invoke(tribeA, tribeB);
    }

    /// <summary>
    /// Trigger peace event
    /// </summary>
    public static void TriggerPeace(Tribe tribeA, Tribe tribeB)
    {
        OnPeaceMade?.Invoke(tribeA, tribeB);
    }

}
