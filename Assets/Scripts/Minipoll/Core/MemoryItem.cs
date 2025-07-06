using UnityEngine;
using System;
using MinipollCore;
using MinipollGame.Managers;


namespace MinipollGame.Core
{
    

[System.Serializable]
public class MemoryItem
{
    [Header("Basic Info")]
    public string memoryId;
    public string description;
    public MemoryType memoryType;
    public float importance;
    public float timestamp;
    
    [Header("Context")]
    public Vector3 location;
    public MinipollGame.Core.MinipollBrain relatedMinipoll;
    public string emotionalContext;
    
    [Header("Memory Strength")]
    public float strength = 100f;
    public float decayRate = 0.1f;
    public int accessCount = 0;
    public float lastAccessed;
    
    [Header("Associations")]
    public string[] tags;
        private string[] relatedMemoryIds;
        // private MemoryManager.MemoryType danger;
        private float severity;
        private MemoryManager.MemoryType danger;

        public string[] RelatedMemoryIds { get => relatedMemoryIds; set => relatedMemoryIds = value; }

        public MemoryItem(string desc, MemoryType type, float imp = 0.5f)
    {
        memoryId = Guid.NewGuid().ToString();
        description = desc;
        memoryType = type;
        importance = imp;
        timestamp = Time.time;
        lastAccessed = Time.time;
        strength = 100f;
    }

        public MemoryItem(string description, MemoryManager.MemoryType danger, float severity)
        {
            this.description = description;
            this.danger = danger;
            this.severity = severity;
        }

        // public MemoryItem(string description, MemoryManager.MemoryType danger, float severity)
        // {
        //     this.description = description;
        //     this.danger = danger;
        //     this.severity = severity;
        // }

        public void Access()
    {
        accessCount++;
        lastAccessed = Time.time;
        strength = Mathf.Min(100f, strength + 5f); // חיזוק הזיכרון בגישה
    }
    
    public void Decay(float deltaTime)
    {
        strength -= decayRate * deltaTime;
        strength = Mathf.Max(0f, strength);
    }
    
    public bool IsForgotten()
    {
        return strength <= 0f;
    }
    
    public float GetAge()
    {
        return Time.time - timestamp;
    }
    
    [System.Serializable]
    public enum MemoryType
    {
        Event,          // אירוע כללי
        Social,         // אינטראקציה חברתית
        Danger,         // סכנה
        Food,           // מזון
        Location,       // מיקום
        Skill,          // למידת מיומנות
        Emotional,      // חוויה רגשית
        Knowledge,      // ידע
        Relationship,   // יחסים
        Achievement     // הישג
    }
}
}