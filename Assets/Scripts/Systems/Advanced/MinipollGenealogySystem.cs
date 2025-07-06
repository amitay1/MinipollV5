/***************************************************************
 *  MinipollGenealogySystem.cs
 *
 *  תיאור כללי:
 *    מודול לניהול שושלות (Genealogy) ומשפחות של המיניפולים:
 *      - כשהורה מוליד צאצא (MinipollReproductionSystem), נרשום אותם כ-PARENT->CHILD
 *      - מאפשר מעקב אחר דורות, סבא/נכד, ומידע על LINEAGE
 *      - אפשר להוסיף אפקטים חברתיים (אחוות אחים, ירושה)
 *
 *  דרישות קדם:
 *    - MinipollReproductionSystem מזמן OnBirth => נרשום בקובץ זה PARENT->CHILD
 *    - סקריפט זה יכול להיות גלובלי (GenealogyManager) או על כל מיניפול
 *      + כאן נדגים גישה גלובלית עם Singleton.
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using MinipollGame.Core;
using MinipollGame.Systems.Core;
using MinipollGame.Controllers;

[System.Serializable]
public class FamilyLink
{
    public string parentId;
    public string childId;
}

public class MinipollGenealogySystem : MonoBehaviour
{
    public static MinipollGenealogySystem Instance;

    [Header("Genealogy Data")]
    [Tooltip("אוסף קשרים הורה-ילד. נשמור IDs (למשל uniqueName)")]
    public List<FamilyLink> familyLinks = new List<FamilyLink>();

    private Dictionary<string, MinipollBrain> minipollIndex = new Dictionary<string, MinipollBrain>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// רישום מיפוי id->MinipollBrain (נעשה ב-Start של המיניפולים)
    /// </summary>
    public void RegisterMinipoll(MinipollBrain brain)
    {
        if (!minipollIndex.ContainsKey(brain.name))
        {
            minipollIndex[brain.name] = brain;
        }
    }

    /// <summary>
    /// קריאה כשהורה מוליד צאצא
    /// </summary>
    public void RecordBirth(MinipollBrain parent, MinipollBrain child)
    {
        FamilyLink link = new FamilyLink();
        link.parentId = parent.name;
        link.childId = child.name;
        familyLinks.Add(link);

        if (!minipollIndex.ContainsKey(child.name))
        {
            minipollIndex[child.name] = child;
        }

        Debug.Log($"Genealogy: {parent.name} => Child {child.name} recorded.");
    }

    /// <summary>
    /// מציאת כלל הילדים של מיניפול
    /// </summary>
    public List<MinipollBrain> GetChildrenOf(string parentName)
    {
        List<MinipollBrain> result = new List<MinipollBrain>();
        foreach (var link in familyLinks)
        {
            if (link.parentId == parentName)
            {
                if (minipollIndex.ContainsKey(link.childId))
                {
                    result.Add(minipollIndex[link.childId]);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// מוצא כלל האבות (rekursive)
    /// </summary>
    public List<MinipollBrain> GetAncestorsOf(string childName)
    {
        List<MinipollBrain> ancestors = new List<MinipollBrain>();
        // מחפשים מי הורה של child
        var links = familyLinks.FindAll(l => l.childId == childName);
        foreach (var l in links)
        {
            // הורה
            if (minipollIndex.ContainsKey(l.parentId))
            {
                var parentBrain = minipollIndex[l.parentId];
                ancestors.Add(parentBrain);
                // גם האבות של parent
                ancestors.AddRange(GetAncestorsOf(l.parentId));
            }
        }
        return ancestors;
    }

    /// <summary>
    /// מוצא כלל הצאצאים (rekursive)
    /// </summary>
    public List<MinipollBrain> GetDescendantsOf(string ancestorName)
    {
        List<MinipollBrain> desc = new List<MinipollBrain>();
        var kids = GetChildrenOf(ancestorName);
        desc.AddRange(kids);
        foreach (var kid in kids)
        {
            desc.AddRange(GetDescendantsOf(kid.name));
        }
        return desc;
    }
}
