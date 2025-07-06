using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using MinipollGame.Core;
using MinipollGame.Systems.Core;

namespace MinipollGame.Systems.Advanced
{


public static class MinipollExtensions
{
    // Extensions for MinipollGeneticsSystem
    public static int GetGeneration(this MinipollGeneticsSystem genetics)
    {
        // אם אין שדה generation, החזר ערך ברירת מחדל
        // או הוסף את השדה למחלקה
        return genetics.generation ?? 0;
    }    public static float GetOverallFitness(this MinipollGeneticsSystem genetics)
    {
        // חישוב כשירות כללית על בסיס הגנים
        float fitness = 0f;
        foreach (MinipollGame.Systems.Core.GeneType gene in Enum.GetValues(typeof(MinipollGame.Systems.Core.GeneType)))
        {
            fitness += genetics.GetGeneValue(gene);
        }
        return fitness / Enum.GetValues(typeof(MinipollGame.Systems.Core.GeneType)).Length;
    }// Extensions for MinipollSkillSystem
    public static void GainExperience(this MinipollSkillSystem skillSystem, MinipollGame.Systems.Core.SkillType skill, float amount)
    {
        // הוסף נסיון לכישור
        var skillData = skillSystem.skills.FirstOrDefault(s => s.skillType == skill);
        if (skillData != null)
        {
            skillData.experience += amount;
            // Note: CheckLevelUp method may need to be updated to accept the correct SkillType
            // skillSystem.CheckLevelUp(skill);
        }
    }
    public static List<MinipollSkill> GetTopSkills(this MinipollSkillSystem skillSystem, int count)
    {
        return skillSystem.skills
            .OrderByDescending(s => s.level)
            .Take(count)
            .ToList();
    }
}

// Event definitions חסרים
[System.Serializable]
public class MinipollReproductionSystemEvents
{
    public static object Instance { get; internal set; }

        public event Action<MinipollBrain, MinipollBrain> OnOffspringBorn;
    public event Action<MinipollBrain, MinipollBrain> OnMatingStart;
    
    public void InvokeOffspringBorn(MinipollBrain parent, MinipollBrain offspring)
    {
        OnOffspringBorn?.Invoke(parent, offspring);
    }
    
    public void InvokeMatingStart(MinipollBrain mate1, MinipollBrain mate2)
    {
        OnMatingStart?.Invoke(mate1, mate2);
    }

    // Note: Removed broken GetComponent<T>() method - this class should not simulate Unity component behavior
}

// WorldEvent structure

public class WorldEvent1
{
    public string eventName;
    public MinipollGame.Systems.Core.WorldEventType eventType;
    public float intensity = 1f;
    public float duration;
    public Vector3 epicenter;
    public float radius;
    public bool isActive;
    
    public WorldEvent1(string name, MinipollGame.Systems.Core.WorldEventType type, float intense = 1f)
    {
        eventName = name;
        eventType = type;
        intensity = intense;
        isActive = true;
    }
}

// Disease structure
[Serializable]
public class Disease1
{
    public string diseaseName;
    public float severity;
    public float contagiousness;
    public float duration;
    public List<string> symptoms = new List<string>();
}

// WorldChunk structure
[System.Serializable]
public class WorldChunk
{
    public int x;
    public int z;
    public MinipollGame.Systems.Core.BiomeType biome;
    public GameObject chunkObject;
    public List<GameObject> resources = new List<GameObject>();
}

// SkillData structure
[System.Serializable]
public class SkillData
{
    public MinipollGame.Systems.Core.SkillType skillType;
    public int level;
    public float experience;
    public float experienceToNextLevel;
}

// SocialRelationship fix
public partial class SocialRelationship
{
    public MinipollBrain otherMinipoll;
    // שאר השדות...
}

}