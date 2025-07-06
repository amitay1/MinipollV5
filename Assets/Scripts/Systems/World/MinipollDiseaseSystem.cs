/***************************************************************
 *  MinipollDiseaseSystem.cs
 *
 *  תיאור כללי:
 *    מודול ניהול מחלות/זיהומים עבור המיניפול:
 *      - רשימת מחלות אפשריות, כל מחלה עם משך, סימפטומים, מדבקות וכו’.
 *      - בכל פריים בודק אם היצור חולה, מוריד בריאות בהתאם, מעביר לאחרים בקרבה.
 *      - כולל התחסנות/החלמה (duration), אחוז הדבקה, מרחק הדבקה.
 *
 *  דרישות קדם:
 *    - למקם על המיניפול (יחד עם MinipollBrain וכו’).
 *    - קריאה: diseaseSystem.UpdateDisease(deltaTime) בכל פריים (או selfUpdate).
 *
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using MinipollGame.Core;

[System.Serializable]
public class Disease
{
    public string diseaseName;
    public float duration;          // כמה זמן המחלה נמשכת (שניות)
    public float timeInfected;      // כמה כבר עבר
    public float healthDamageRate;  // נזק לשנייה
    public float infectRadius;      // רדיוס הדבקה
    public float infectChancePerSec;// הסתברות להדביק אחרים בשנייה (אם הם בטווח)
    public bool isContagious;       // האם בכלל המחלה מדבקת

    public Disease(string name, float dur, float dmg, float radius, float chance, bool contagious)
    {
        diseaseName = name;
        duration = dur;
        healthDamageRate = dmg;
        infectRadius = radius;
        infectChancePerSec = chance;
        isContagious = contagious;
        timeInfected = 0f;
    }
}

public class MinipollDiseaseSystem : MonoBehaviour
{
    private MinipollBrain brain;

    [Header("Diseases Active")]
    public List<Disease> activeDiseases = new List<Disease>();

    [Header("Infection Settings")]
    public bool enableInfectionSpread = true;
    public bool selfUpdate = false;

    private void Awake()
    {
        brain = GetComponent<MinipollBrain>();
    }

    private void Update()
    {
        if (selfUpdate)
        {
            UpdateDisease(Time.deltaTime);
        }
    }

    public void UpdateDisease(float deltaTime)
    {
        for (int i = activeDiseases.Count - 1; i >= 0; i--)
        {
            var dis = activeDiseases[i];
            dis.timeInfected += deltaTime;

            // 1) הורדת בריאות
            if (dis.healthDamageRate > 0f && brain.IsAlive)
            {
                brain.TakeDamage(dis.healthDamageRate * deltaTime);
            }

            // 2) הפצה
            if (enableInfectionSpread && dis.isContagious && brain.IsAlive)
            {
                SpreadInfection(dis, deltaTime);
            }

            // 3) סיום מחלה?
            if (dis.timeInfected >= dis.duration)
            {
                // החלמה
                activeDiseases.RemoveAt(i);
                // אפשר להודיע ל-UI? 
            }
        }
    }

    /// <summary>
    /// הפצת מחלה לאחרים ברדיוס
    /// </summary>
    private void SpreadInfection(Disease dis, float deltaTime)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, dis.infectRadius);
        float chanceFrame = dis.infectChancePerSec * deltaTime; 
        foreach (var c in hits)
        {
            if (c.gameObject == this.gameObject) continue;
            var otherDisease = c.GetComponent<MinipollDiseaseSystem>();
            if (otherDisease != null && otherDisease.brain.IsAlive)
            {
                // בהסתברות chanceFrame מדביקים
                if (Random.value < chanceFrame)
                {
                    // ידביק אם האחר לא Already infected in that disease
                    if (!otherDisease.HasDisease(dis.diseaseName))
                    {
                        otherDisease.Infect(
                            new Disease(dis.diseaseName, dis.duration, dis.healthDamageRate,
                                        dis.infectRadius, dis.infectChancePerSec, dis.isContagious)
                        );
                    }
                }
            }
        }
    }

    /// <summary>
    /// הדבקה ממוקדת
    /// </summary>
    public void Infect(Disease newDisease)
    {
        // אם כבר חולה במחלה הזו, לא נכפיל
        if (HasDisease(newDisease.diseaseName)) return;
        activeDiseases.Add(newDisease);
        // אפשר להודיע "נדבקתי!"
    }

    public bool HasDisease(string diseaseName)
    {
        return activeDiseases.Exists(d => d.diseaseName == diseaseName);
    }

    /// <summary>
    /// קבלת רשימת המחלות הפעילות
    /// </summary>
    public List<Disease> GetActiveDiseases()
    {
        return activeDiseases;
    }
}
