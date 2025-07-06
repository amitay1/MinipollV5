using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MinipollGame.Systems.Core;

/// <summary>
/// מערכת הסטטיסטיקות של Minipoll
/// מנהלת את כל הנתונים המספריים והתכונות
/// </summary>
namespace MinipollGame.Core
{
    public class MinipollStats : MonoBehaviour
    {
        #region Stat Definitions
        [System.Serializable]
        public class Stat
        {
            public StatType type;
            public float baseValue;
            public float currentValue;
            public float minValue = 0f;
            public float maxValue = 100f;

            private List<StatModifier> modifiers = new List<StatModifier>();

            public float GetValue()
            {
                float finalValue = baseValue;

                // Apply modifiers
                foreach (var modifier in modifiers)
                {
                    switch (modifier.type)
                    {
                        case ModifierType.Flat:
                            finalValue += modifier.value;
                            break;
                        case ModifierType.Percentage:
                            finalValue *= (1 + modifier.value / 100f);
                            break;
                    }
                }

                // Clamp to range
                currentValue = Mathf.Clamp(finalValue, minValue, maxValue);
                return currentValue;
            }

            public void AddModifier(StatModifier modifier)
            {
                modifiers.Add(modifier);
            }

            public void RemoveModifier(StatModifier modifier)
            {
                modifiers.Remove(modifier);
            }

            public void ClearModifiers()
            {
                modifiers.Clear();
            }
        }

        [System.Serializable]
        public class StatModifier
        {
            public string source;
            public ModifierType type;
            public float value;
            public float duration; // 0 = permanent

            public StatModifier(string source, ModifierType type, float value, float duration = 0)
            {
                this.source = source;
                this.type = type;
                this.value = value;
                this.duration = duration;
            }
        }
        #endregion

        #region Configuration
        [Header("=== Base Stats ===")]
        [SerializeField] private List<Stat> stats = new List<Stat>();

        [Header("=== Age Stage Multipliers ===")]
        [SerializeField] private AgeStageMultipliers babyMultipliers;
        [SerializeField] private AgeStageMultipliers childMultipliers;
        [SerializeField] private AgeStageMultipliers adultMultipliers;
        [SerializeField] private AgeStageMultipliers elderMultipliers;

        [System.Serializable]
        public class AgeStageMultipliers
        {
            [Range(0.1f, 2f)] public float healthMultiplier = 1f;
            [Range(0.1f, 2f)] public float speedMultiplier = 1f;
            [Range(0.1f, 2f)] public float strengthMultiplier = 1f;
            [Range(0.1f, 2f)] public float intelligenceMultiplier = 1f;
            [Range(0.1f, 2f)] public float socialMultiplier = 1f;
        }

        [Header("=== Genetics Influence ===")]
        [SerializeField] private bool useGeneticVariation = true;
        [SerializeField][Range(0f, 0.3f)] private float geneticVariationRange = 0.15f;
        #endregion

        #region References
        private MinipollCore core;
        private Dictionary<StatType, Stat> statDictionary;
        private Dictionary<string, List<StatModifier>> temporaryModifiers;
        #endregion

        #region Events
        public event Action<StatType, float, float> OnStatChanged; // type, oldValue, newValue
        public event Action<StatType, StatModifier> OnModifierAdded;
        public event Action<StatType, StatModifier> OnModifierRemoved;
        #endregion

        #region Initialization
        private void Awake()
        {
            core = GetComponent<MinipollCore>();
            InitializeStats();
            BuildStatDictionary();
            temporaryModifiers = new Dictionary<string, List<StatModifier>>();
        }

        private void InitializeStats()
        {
            // Ensure all stat types exist
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                if (!stats.Exists(s => s.type == statType))
                {
                    stats.Add(CreateDefaultStat(statType));
                }
            }

            // Apply genetic variation if enabled
            if (useGeneticVariation)
            {
                ApplyGeneticVariation();
            }
        }

        private Stat CreateDefaultStat(StatType type)
        {
            Stat stat = new Stat { type = type };

            // Set default values based on stat type
            switch (type)
            {
                case StatType.MaxHealth:
                    stat.baseValue = 100f;
                    stat.maxValue = 200f;
                    break;
                case StatType.Speed:
                    stat.baseValue = 5f;
                    stat.maxValue = 10f;
                    break;
                case StatType.Strength:
                    stat.baseValue = 10f;
                    stat.maxValue = 50f;
                    break;
                case StatType.Defense:
                    stat.baseValue = 5f;
                    stat.maxValue = 50f;
                    break;
                case StatType.Intelligence:
                    stat.baseValue = 10f;
                    stat.maxValue = 100f;
                    break;
                case StatType.Charisma:
                    stat.baseValue = 10f;
                    stat.maxValue = 100f;
                    break;
                case StatType.Luck:
                    stat.baseValue = 10f;
                    stat.maxValue = 100f;
                    break;
                case StatType.Fertility:
                    stat.baseValue = 50f;
                    stat.maxValue = 100f;
                    break;
                case StatType.Longevity:
                    stat.baseValue = 100f;
                    stat.maxValue = 200f;
                    break;
            }

            stat.currentValue = stat.baseValue;
            return stat;
        }

        private void BuildStatDictionary()
        {
            statDictionary = new Dictionary<StatType, Stat>();
            foreach (var stat in stats)
            {
                statDictionary[stat.type] = stat;
            }
        }

        private void ApplyGeneticVariation()
        {
            foreach (var stat in stats)
            {
                float variation = UnityEngine.Random.Range(-geneticVariationRange, geneticVariationRange);
                stat.baseValue *= (1 + variation);
                stat.currentValue = stat.baseValue;
            }
        }
        #endregion

        #region Stat Access
        public float GetStat(StatType type)
        {
            if (statDictionary == null)
            {
                Debug.LogWarning($"[MinipollStats] StatDictionary is null on {gameObject.name}. Rebuilding...");
                BuildStatDictionary();
            }
            
            if (statDictionary != null && statDictionary.TryGetValue(type, out Stat stat))
            {
                return stat.GetValue();
            }

            Debug.LogWarning($"[Stats] Stat {type} not found on {gameObject.name}");
            return 0f;
        }

        public float GetBaseStat(StatType type)
        {
            if (statDictionary == null)
            {
                Debug.LogWarning($"[MinipollStats] StatDictionary is null on {gameObject.name}. Rebuilding...");
                BuildStatDictionary();
            }
            
            if (statDictionary != null && statDictionary.TryGetValue(type, out Stat stat))
            {
                return stat.baseValue;
            }
            return 0f;
        }

        public void SetBaseStat(StatType type, float value)
        {
            if (statDictionary == null)
            {
                Debug.LogWarning($"[MinipollStats] StatDictionary is null on {gameObject.name}. Rebuilding...");
                BuildStatDictionary();
            }
            
            if (statDictionary != null && statDictionary.TryGetValue(type, out Stat stat))
            {
                float oldValue = stat.GetValue();
                stat.baseValue = Mathf.Clamp(value, stat.minValue, stat.maxValue);
                float newValue = stat.GetValue();

                if (Math.Abs(oldValue - newValue) > 0.01f)
                {
                    OnStatChanged?.Invoke(type, oldValue, newValue);
                }
            }
        }

        public void ModifyStat(StatType type, float amount)
        {
            SetBaseStat(type, GetBaseStat(type) + amount);
        }

        // Quick access properties
        public float MaxHealth => GetStat(StatType.MaxHealth);
        public float Speed => GetStat(StatType.Speed);
        public float Strength => GetStat(StatType.Strength);
        public float Defense => GetStat(StatType.Defense);
        public float Intelligence => GetStat(StatType.Intelligence);
        public float Charisma => GetStat(StatType.Charisma);
        public float Luck => GetStat(StatType.Luck);
        public float Fertility => GetStat(StatType.Fertility);
        public float Longevity => GetStat(StatType.Longevity);

        public static int Idle { get; internal set; }
        public static int Sleeping { get; internal set; }
        #endregion

        #region Modifiers
        public void AddModifier(StatType type, StatModifier modifier)
        {
            if (statDictionary == null)
            {
                Debug.LogWarning($"[MinipollStats] StatDictionary is null on {gameObject.name}. Rebuilding...");
                BuildStatDictionary();
            }
            
            if (statDictionary != null && statDictionary.TryGetValue(type, out Stat stat))
            {
                float oldValue = stat.GetValue();
                stat.AddModifier(modifier);
                float newValue = stat.GetValue();

                // Track temporary modifiers
                if (modifier.duration > 0)
                {
                    if (!temporaryModifiers.ContainsKey(modifier.source))
                        temporaryModifiers[modifier.source] = new List<StatModifier>();

                    temporaryModifiers[modifier.source].Add(modifier);
                    StartCoroutine(RemoveModifierAfterDuration(type, modifier));
                }

                OnModifierAdded?.Invoke(type, modifier);

                if (Math.Abs(oldValue - newValue) > 0.01f)
                {
                    OnStatChanged?.Invoke(type, oldValue, newValue);
                }

                Debug.Log($"[Stats] Added {modifier.type} modifier ({modifier.value}) to {type} from {modifier.source}");
            }
        }

        public void RemoveModifier(StatType type, StatModifier modifier)
        {
            if (statDictionary == null)
            {
                Debug.LogWarning($"[MinipollStats] StatDictionary is null on {gameObject.name}. Rebuilding...");
                BuildStatDictionary();
            }
            
            if (statDictionary != null && statDictionary.TryGetValue(type, out Stat stat))
            {
                float oldValue = stat.GetValue();
                stat.RemoveModifier(modifier);
                float newValue = stat.GetValue();

                // Remove from temporary tracking
                if (temporaryModifiers.ContainsKey(modifier.source))
                {
                    temporaryModifiers[modifier.source].Remove(modifier);
                    if (temporaryModifiers[modifier.source].Count == 0)
                        temporaryModifiers.Remove(modifier.source);
                }

                OnModifierRemoved?.Invoke(type, modifier);

                if (Math.Abs(oldValue - newValue) > 0.01f)
                {
                    OnStatChanged?.Invoke(type, oldValue, newValue);
                }
            }
        }

        public void RemoveAllModifiersFromSource(string source)
        {
            if (temporaryModifiers.TryGetValue(source, out List<StatModifier> modifiers))
            {
                foreach (var modifier in modifiers.ToArray())
                {
                    foreach (var stat in stats)
                    {
                        stat.RemoveModifier(modifier);
                    }
                }
                temporaryModifiers.Remove(source);
            }
        }

        private System.Collections.IEnumerator RemoveModifierAfterDuration(StatType type, StatModifier modifier)
        {
            yield return new WaitForSeconds(modifier.duration);
            RemoveModifier(type, modifier);
        }
        #endregion

        #region Age Stage Management
        public void InitializeForAgeStage(Systems.Core.AgeStage stage)
        {
            ApplyAgeStageMultipliers(stage);
        }

        private void ApplyAgeStageMultipliers(Systems.Core.AgeStage stage)
        {
            AgeStageMultipliers multipliers = GetMultipliersForStage(stage);

            // Apply multipliers as modifiers
            AddModifier(StatType.MaxHealth, new StatModifier("AgeStage", ModifierType.Percentage,
                (multipliers.healthMultiplier - 1) * 100, 0));

            AddModifier(StatType.Speed, new StatModifier("AgeStage", ModifierType.Percentage,
                (multipliers.speedMultiplier - 1) * 100, 0));

            AddModifier(StatType.Strength, new StatModifier("AgeStage", ModifierType.Percentage,
                (multipliers.strengthMultiplier - 1) * 100, 0));

            AddModifier(StatType.Intelligence, new StatModifier("AgeStage", ModifierType.Percentage,
                (multipliers.intelligenceMultiplier - 1) * 100, 0));

            AddModifier(StatType.Charisma, new StatModifier("AgeStage", ModifierType.Percentage,
                (multipliers.socialMultiplier - 1) * 100, 0));
        }

        public void TransitionToAgeStage(Systems.Core.AgeStage newStage)
        {
            RemoveAllModifiersFromSource("AgeStage");
            ApplyAgeStageMultipliers(newStage);
        }

        private AgeStageMultipliers GetMultipliersForStage(Systems.Core.AgeStage stage)
        {
            switch (stage)
            {
                case Systems.Core.AgeStage.Baby:
                    return babyMultipliers;
                case Systems.Core.AgeStage.Child:
                    return childMultipliers;
                case Systems.Core.AgeStage.Adult:
                    return adultMultipliers;
                case Systems.Core.AgeStage.Elder:
                    return elderMultipliers;
                default:
                    return adultMultipliers;
            }
        }
        #endregion

        #region Buffs and Debuffs
        public void ApplyBuff(BuffType buffType, float duration, float strength = 1f)
        {
            string buffName = $"Buff_{buffType}";

            switch (buffType)
            {
                case BuffType.Strength:
                    AddModifier(StatType.Strength, new StatModifier(buffName, ModifierType.Percentage, 25f * strength, duration));
                    break;
                case BuffType.Speed:
                    AddModifier(StatType.Speed, new StatModifier(buffName, ModifierType.Percentage, 30f * strength, duration));
                    break;
                case BuffType.Intelligence:
                    AddModifier(StatType.Intelligence, new StatModifier(buffName, ModifierType.Percentage, 20f * strength, duration));
                    break;
                case BuffType.AllStats:
                    AddModifier(StatType.Strength, new StatModifier(buffName, ModifierType.Percentage, 15f * strength, duration));
                    AddModifier(StatType.Speed, new StatModifier(buffName, ModifierType.Percentage, 15f * strength, duration));
                    AddModifier(StatType.Intelligence, new StatModifier(buffName, ModifierType.Percentage, 15f * strength, duration));
                    AddModifier(StatType.Defense, new StatModifier(buffName, ModifierType.Percentage, 15f * strength, duration));
                    break;
            }
        }

        public void ApplyDebuff(DebuffType debuffType, float duration, float strength = 1f)
        {
            string debuffName = $"Debuff_{debuffType}";

            switch (debuffType)
            {
                case DebuffType.Weakness:
                    AddModifier(StatType.Strength, new StatModifier(debuffName, ModifierType.Percentage, -25f * strength, duration));
                    break;
                case DebuffType.Slowness:
                    AddModifier(StatType.Speed, new StatModifier(debuffName, ModifierType.Percentage, -30f * strength, duration));
                    break;
                case DebuffType.Confusion:
                    AddModifier(StatType.Intelligence, new StatModifier(debuffName, ModifierType.Percentage, -40f * strength, duration));
                    break;
                case DebuffType.Vulnerability:
                    AddModifier(StatType.Defense, new StatModifier(debuffName, ModifierType.Percentage, -50f * strength, duration));
                    break;
            }
        }
        #endregion

        #region Combat Calculations
        public float CalculateDamage(float baseDamage, DamageType damageType = DamageType.Physical)
        {
            float strength = GetStat(StatType.Strength);
            float luck = GetStat(StatType.Luck);

            float finalDamage = baseDamage * (1 + strength / 100f);
            finalDamage *= UnityEngine.Random.Range(0.8f, 1.2f); // Random variance
            finalDamage *= (1 + luck / 500f); // Luck bonus

            return finalDamage;
        }

        public float CalculateDefense(float incomingDamage, DamageType damageType = DamageType.Physical)
        {
            float defense = GetStat(StatType.Defense);
            float reductionPercent = defense / (defense + 100f);
            return incomingDamage * (1 - reductionPercent);
        }

        public bool PerformSkillCheck(StatType stat, float difficulty)
        {
            float statValue = GetStat(stat);
            float luck = GetStat(StatType.Luck);
            float roll = UnityEngine.Random.Range(0f, 100f);
            
            float total = statValue + (luck * 0.1f) + roll;
            return total >= difficulty;
        }
        #endregion

        #region Save/Load
        public StatsData GetStatsData()
        {
            StatsData data = new StatsData();
            foreach (var stat in stats)
            {
                data.statValues[stat.type] = stat.baseValue;
            }
            return data;
        }

        public void LoadStatsData(StatsData data)
        {
            foreach (var kvp in data.statValues)
            {
                SetBaseStat(kvp.Key, kvp.Value);
            }
        }
        #endregion

        #region Debug
        [ContextMenu("Debug - Print All Stats")]
        private void DebugPrintStats()
        {
            foreach (var stat in stats)
            {
                Debug.Log($"{stat.type}: {stat.GetValue():F1} (Base: {stat.baseValue:F1})");
            }
        }

        [ContextMenu("Debug - Apply Random Buff")]
        private void DebugRandomBuff()
        {
            BuffType[] buffs = (BuffType[])Enum.GetValues(typeof(BuffType));
            BuffType randomBuff = buffs[UnityEngine.Random.Range(0, buffs.Length)];
            ApplyBuff(randomBuff, 10f);        }
        #endregion
    }

    #region Data Classes

    [System.Serializable]
    public class StatsData
    {
        public Dictionary<StatType, float> statValues = new Dictionary<StatType, float>();
    }
    #endregion
}