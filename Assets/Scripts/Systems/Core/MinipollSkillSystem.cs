/***************************************************************
 *  MinipollSkillSystem.cs
 *
 *  תיאור כללי:
 *    מערכת כישורים (Skills) למיניפול:
 *      - כל כישור מחזיק רמת ניסיון (XP) ורמה (Level)
 *      - שימוש (למשל Gathering) יעלה XP בכישור המתאים
 *      - ברמה מסוימת, מוכרז LevelUp (ואולי הבונוס רלוונטי למודולים)
 *
 *  דרישות קדם:
 *    - לשים על המיניפול
 *    - לקרוא skillSystem.GainSkillXP("Gathering", 5f) כאשר מיניפול אוסף משאב וכו’.
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using MinipollCore;
using MinipollGame.Systems.Core;

[System.Serializable]
public class MinipollSkill
{
    public string skillName;
    public int level;
    public float currentXP;
    public float xpToNextLevel;
    internal SkillType skillType;
    internal float experience;

    public MinipollSkill(string name)
    {
        skillName = name;
        level = 1;
        currentXP = 0f;
        xpToNextLevel = 100f; // דוגמה
    }

    public void AddXP(float amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            level++;
            // אפשר להגדיל xpToNextLevel בהמשך
            xpToNextLevel *= 1.2f; // כל רמה הבאה קצת יותר קשה
        }
    }
}

public class MinipollSkillSystem : MonoBehaviour
{
    [Header("Skill List")]
    public List<MinipollSkill> skills = new List<MinipollSkill>();

    [Header("Default Skills")]
    public string[] defaultSkillNames = { "Gathering", "Fighting", "Social", "Exploration" };

    [Tooltip("האם להוסיף אוטומטית כישורי ברירת מחדל ב-Awake?")]
    public bool addDefaultSkillsOnAwake = true;

    private void Awake()
    {
        if (addDefaultSkillsOnAwake)
        {
            foreach (var sName in defaultSkillNames)
            {
                if (GetSkill(sName) == null)
                {
                    skills.Add(new MinipollSkill(sName));
                }
            }
        }
    }

    /// <summary>
    /// הוספת XP לכישור (מייצר כישור חדש אם לא קיים)
    /// </summary>
    public void GainSkillXP(string skillName, float amount)
    {
        var sk = GetSkill(skillName);
        if (sk == null)
        {
            sk = new MinipollSkill(skillName);
            skills.Add(sk);
        }
        sk.AddXP(amount);
        // אפשר ליידע מערכות אחרות: אם sk.level עולה, maybe speed++...
    }

    public MinipollSkill GetSkill(string skillName)
    {
        return skills.Find(x => x.skillName == skillName);
    }

    internal float GetSkillLevel(object medicine)
    {
        throw new NotImplementedException();
    }

    internal void AddSkillBuff(object teaching, float v1, float v2, string v3)
    {
        throw new NotImplementedException();
    }

    internal void CheckLevelUp(SkillType skill)
    {
        throw new NotImplementedException();
    }

    internal float GetSkillEfficiency(SkillType teaching)
    {
        throw new NotImplementedException();
    }

    internal void GainExperience(SkillType navigation, float v)
    {
        throw new NotImplementedException();
    }

    
}
