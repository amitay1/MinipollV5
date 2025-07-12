using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minipoll.Core.Architecture
{
    /// <summary>
    /// ScriptableObject that defines the prefab registry for the Minipoll Prefab Architecture
    /// Contains all prefabs that should be registered at startup
    /// </summary>
    [CreateAssetMenu(fileName = "PrefabRegistry", menuName = "Minipoll/Core/Prefab Registry", order = 1)]
    public class PrefabRegistryData : ScriptableObject
    {
        [Header("Prefab Entries")]
        [SerializeField] private List<PrefabEntry> prefabEntries = new List<PrefabEntry>();
        
        public List<PrefabEntry> PrefabEntries => prefabEntries;
        
        #region Editor Utilities
        
        #if UNITY_EDITOR
        [ContextMenu("Validate Registry")]
        private void ValidateRegistry()
        {
            int validEntries = 0;
            int invalidEntries = 0;
            
            foreach (var entry in prefabEntries)
            {
                if (entry.IsValid())
                    validEntries++;
                else
                    invalidEntries++;
            }
            
            Debug.Log($"[PrefabRegistry] Validation complete: {validEntries} valid entries, {invalidEntries} invalid entries");
        }
        
        [ContextMenu("Auto-Generate IDs")]
        private void AutoGenerateIds()
        {
            foreach (var entry in prefabEntries)
            {
                if (string.IsNullOrEmpty(entry.PrefabId) && entry.Prefab != null)
                {
                    entry.PrefabId = entry.Prefab.name.ToLower().Replace(" ", "_");
                }
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("[PrefabRegistry] Auto-generated missing prefab IDs");
        }
        
        [ContextMenu("Sort Entries")]
        private void SortEntries()
        {
            prefabEntries.Sort((a, b) => 
            {
                int categoryComparison = string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase);
                if (categoryComparison != 0)
                    return categoryComparison;
                    
                return string.Compare(a.PrefabId, b.PrefabId, StringComparison.OrdinalIgnoreCase);
            });
            
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("[PrefabRegistry] Sorted entries by category and prefab ID");
        }
        #endif
        
        #endregion
        
        #region Runtime Utilities
        
        /// <summary>
        /// Find a prefab entry by ID
        /// </summary>
        public PrefabEntry FindEntry(string prefabId)
        {
            return prefabEntries.Find(entry => entry.PrefabId == prefabId);
        }
        
        /// <summary>
        /// Get all entries in a specific category
        /// </summary>
        public List<PrefabEntry> GetEntriesByCategory(string category)
        {
            return prefabEntries.FindAll(entry => entry.Category == category);
        }
        
        /// <summary>
        /// Get all entries with a specific tag
        /// </summary>
        public List<PrefabEntry> GetEntriesByTag(string tag)
        {
            return prefabEntries.FindAll(entry => entry.Tags.Contains(tag));
        }
        
        #endregion
    }
    
    /// <summary>
    /// Individual prefab entry in the registry
    /// </summary>
    [Serializable]
    public class PrefabEntry
    {
        [Header("Identification")]
        [SerializeField] private string prefabId;
        [SerializeField] private GameObject prefab;
        
        [Header("Organization")]
        [SerializeField] private string category = "General";
        [SerializeField] private List<string> tags = new List<string>();
        
        [Header("Configuration")]
        [SerializeField] private bool autoRegister = true;
        [SerializeField] private int maxInstances = -1; // -1 = unlimited
        [SerializeField] private bool poolInstances = false;
        
        [Header("Metadata")]
        [TextArea(2, 4)]
        [SerializeField] private string description;
        
        public string PrefabId 
        { 
            get => prefabId; 
            set => prefabId = value; 
        }
        
        public GameObject Prefab => prefab;
        public string Category => category;
        public List<string> Tags => tags;
        public bool AutoRegister => autoRegister;
        public int MaxInstances => maxInstances;
        public bool PoolInstances => poolInstances;
        public string Description => description;
        
        /// <summary>
        /// Check if this prefab entry is valid
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(prefabId) && prefab != null;
        }
        
        /// <summary>
        /// Check if this entry has a specific tag
        /// </summary>
        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }
    }
    
    /// <summary>
    /// Common prefab categories used in the Minipoll game
    /// </summary>
    public static class PrefabCategories
    {
        public const string CREATURES = "Creatures";
        public const string ENVIRONMENT = "Environment";
        public const string FOOD = "Food";
        public const string INTERACTIVE = "Interactive";
        public const string UI = "UI";
        public const string EFFECTS = "Effects";
        public const string AUDIO = "Audio";
        public const string WEATHER = "Weather";
        public const string SYSTEMS = "Systems";
        public const string DEBUG = "Debug";
    }
    
    /// <summary>
    /// Common prefab tags used in the Minipoll game
    /// </summary>
    public static class PrefabTags
    {
        // Creature tags
        public const string MINIPOLL = "minipoll";
        public const string BABY_MINIPOLL = "baby_minipoll";
        public const string ADULT_MINIPOLL = "adult_minipoll";
        
        // Interaction tags
        public const string FEEDABLE = "feedable";
        public const string CLICKABLE = "clickable";
        public const string MOVEABLE = "moveable";
        public const string COLLECTIBLE = "collectible";
        
        // Environment tags
        public const string WATER_SOURCE = "water_source";
        public const string FOOD_SOURCE = "food_source";
        public const string SHELTER = "shelter";
        public const string DECORATION = "decoration";
        
        // System tags
        public const string MANAGER = "manager";
        public const string CONTROLLER = "controller";
        public const string SINGLETON = "singleton";
        
        // Special tags
        public const string PERSISTENT = "persistent";
        public const string SCENE_SPECIFIC = "scene_specific";
        public const string PLAYER_ONLY = "player_only";
    }
}
