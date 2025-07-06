/***************************************************************
 *  MinipollMagicSystem.cs
 *
 *  תיאור כללי:
 *    מנגנון קסמים/כשפים למיניפולים:
 *      - לכל מיניפול יש "Mana" ו"SpellBook" (רשימת לחשים).
 *      - לחשים דורשים Mana, ובעלי השפעה (התקפה, ריפוי, זימון).
 *      - מנגנון "למידת לחשים" – שימוש ב-Rewards/Mentors/Skill XP.
 *      - בקריאה ל-CastSpell(spellName), בודקים אם יש מספיק Mana, ומשפיעים על העולם או ישויות אחרות.
 *
 *  דרישות קדם:
 *    - אפשר להשתלב עם EmotionsSystem (קסם שמחה?), BattleSystem (התקפה קסומה), ArtifactSystem וכו’.
 *    - הסקריפט ממוקם על Minipoll, עם MinipollBrain זמין.
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using MinipollCore;
using MinipollGame.Core;
public enum SpellType
{
    Heal,
    Damage,
    Buff,
    Debuff,
    Utility
}

[System.Serializable]
public class Spell
{
    public string spellName;
    public SpellType spellType;
    public float manaCost;
    public float power;
    public float castTime;
    public float cooldown;
    public string description;
}

public class MinipollMagicSystem : MonoBehaviour
{
    private MinipollGame.Core.MinipollBrain brain;

    [Header("Mana & Spell Book")]
    public float maxMana = 100f;
    public float currentMana = 100f;
    public float manaRegenRate = 2f;  // Mana לשנייה
    public List<Spell> spellBook = new List<Spell>();

    [Header("Skill-based magic? (optional)")]
    public bool requireSkillLevel = false;
    public string requiredSkillName = "Magic";
    public int requiredSkillLevel = 1;

    [Header("Debug")]
    public bool selfUpdate = true;

    private void Awake()
    {
        brain = GetComponent<MinipollGame.Core.MinipollBrain>();
    }

    private void Update()
    {
        if (selfUpdate)
        {
            UpdateMagic(Time.deltaTime);
        }
    }

    public void UpdateMagic(float deltaTime)
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * deltaTime;
            if (currentMana > maxMana) currentMana = maxMana;
        }
    }

    public bool CastSpell(string spellName, MinipollGame.Core.MinipollBrain target = null)
    {
        var spell = spellBook.Find(s => s.spellName == spellName);
        if (spell == null) return false;
        
        if (currentMana < spell.manaCost) return false;
        
        if (requireSkillLevel)
        {
            var skillSys = brain.GetComponent<MinipollSkillSystem>();
            if (skillSys != null)
            {
                var skill = skillSys.GetSkill(requiredSkillName);
                if (skill == null || skill.level < requiredSkillLevel)
                    return false;
            }
        }
        
        currentMana -= spell.manaCost;
        ExecuteSpell(spell, target);
        return true;
    }

    private void ExecuteSpell(Spell spell, MinipollBrain target)
    {
        switch (spell.spellType)
        {
            // case SpellType.Heal:
            //     if (target != null)
            //         target.Health += spell.power;
            //     else
            //         brain.Health += spell.power;
            //     break;
            // case SpellType.Damage:
            //     if (target != null)
            //         target.TakeDamage(spell.power);
            //     break;
        }
    }

    /// <summary>
    /// פונקציה להוספת כישוף חדש
    /// </summary>
    public void LearnSpell(Spell newSpell)
    {
        var existing = spellBook.Find(x => x.spellName == newSpell.spellName);
        if (existing == null)
        {
            spellBook.Add(newSpell);
            Debug.Log($"{brain.name} learned new spell: {newSpell.spellName}");
        }
        else
        {
            Debug.LogWarning($"{brain.name} already knows spell {newSpell.spellName}");
        }
    }
}
