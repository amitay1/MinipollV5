/*************************************************************
 *  MinipollArtifactSystem.cs
 *  
 *  תיאור כללי:
 *    מערכת חפצים עתיקים ותרבותיים:
 *      - גילוי חפצים נדירים ועתיקים
 *      - השפעות מיסטיות וייחודיות
 *      - מערכת אוסף ומוזאון
 *      - חקר ומחקר חפצים
 *      - סחר בחפצים נדירים
 *      - יצירת מורשת תרבותית
 *  
 *  דרישות קדם:
 *    - להניח על GameObject ריק בסצנה (ArtifactManager)
 *    - עבודה עם מערכות אחרות
 *************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using MinipollGame.Core;
using MinipollGame.Systems.Advanced;

// Assuming MinipollBrain, MinipollSkillSystem, SkillType, MinipollGeneticsSystem, GeneType,
// MinipollStreamingWorldSystem, BiomeType, MinipollEconomySystem, EconomicItem,
// MinipollDiplomacySystem, MinipollMemorySystem, MinipollNeedsSystem, MinipollEmotionsSystem, Emotion
// and other related classes/enums (like Chunk) are defined elsewhere in the project.
namespace MinipollGame.Systems.Core
{
    public enum ArtifactRarity
    {
        Common,         // נפוץ
        Uncommon,       // לא נפוץ
        Rare,           // נדיר
        Epic,           // אפי
        Legendary,      // אגדי
        Mythical,       // מיתי
        Divine         // אלוהי
    }

    [System.Serializable]
    public enum ArtifactType
    {
        Tool,           // כלי עבודה עתיק
        Weapon,         // נשק עתיק
        Jewelry,        // תכשיטים
        Art,            // יצירת אמנות
        Religious,      // חפץ דתי
        Scientific,     // מכשיר מדעי
        Historical,     // חפץ היסטורי
        Mystical,       // חפץ מיסטי
        Cultural,       // חפץ תרבותי
        Unknown        // לא מזוהה
    }

    [System.Serializable]
    public enum ArtifactOrigin
    {
        AncientCivilization,    // ציוויליזציה עתיקה
        LostTribe,              // שבט אבוד
        AlienVisitors,          // מבקרים מהחלל
        NaturalFormation,       // היווצרות טבעית
        TimeAnomaly,            // אנומליית זמן
        MysticalSource,         // מקור מיסטי
        UnknownOrigin          // מקור לא ידוע
    }

    [System.Serializable]
    public class ArtifactPower
    {
        public string powerName;
        public string description;
        public float intensity = 1f;           // עוצמת השפעה
        public float range = 1f;               // טווח השפעה
        public float duration = -1f;           // משך (‎-1 = קבוע)
        public bool requiresActivation = false; // דרוש הפעלה
        public float activationCost = 0f;       // עלות הפעלה
        public List<string> effects = new List<string>();

        public ArtifactPower(string name, string desc, float intense = 1f)
        {
            powerName = name;
            description = desc;
            intensity = intense;
        }
    }

    [System.Serializable]
    public class Artifact
    {
        [Header("Basic Info")]
        public string artifactName;
        public string discoveredName;          // השם שהתגלה בו
        public string trueName;                // השם האמיתי (אם ידוע)
        public ArtifactType artifactType;
        public ArtifactRarity rarity;
        public ArtifactOrigin origin;

        [Header("Physical Properties")]
        public string material;                // חומר
        public float age = 1000f;              // גיל בשנים
        public float weight = 1f;              // משקל
        public Vector3 dimensions;             // מימדים
        public Color primaryColor = Color.gray;
        public string physicalDescription;

        [Header("Discovery")]
        public Vector3 discoveryLocation;      // מקום גילוי
        public float discoveryTime;            // זמן גילוי
        public MinipollBrain discoveredBy;     // מי גילה
        public BiomeType discoveryBiome;       // ביום הגילוי
        public string discoveryCircumstances;  // נסיבות הגילוי

        [Header("Powers and Effects")]
        public List<ArtifactPower> powers = new List<ArtifactPower>();
        public bool isActive = false;          // האם פעיל
        public float powerLevel = 1f;          // רמת כוח נוכחית
        public float maxPowerLevel = 1f;       // רמת כוח מקסימלית

        [Header("Research")]
        public float researchProgress = 0f;    // התקדמות מחקר
        public float requiredResearch = 100f;  // מחקר נדרש לפענוח
        public bool isDeciphered = false;      // האם פוענח
        public List<string> researchNotes = new List<string>();
        public List<MinipollBrain> researchers = new List<MinipollBrain>();

        [Header("Cultural Impact")]
        public float culturalValue = 100f;     // ערך תרבותי
        public float scientificValue = 50f;    // ערך מדעי
        public float economicValue = 200f;     // ערך כלכלי
        public int timesDisplayed = 0;         // כמה פעמים הוצג
        public List<string> culturalInfluences = new List<string>();

        [Header("Ownership")]
        public MinipollBrain currentOwner;     // בעלים נוכחי
        public List<MinipollBrain> previousOwners = new List<MinipollBrain>();
        public bool isInMuseum = false;        // האם במוזאון
        public bool isForTrade = false;        // האם למכירה

        [Header("Status")]
        public bool isLost = false;            // האם אבד
        public bool isDamaged = false;         // האם פגום
        public float condition = 100f;         // מצב (0-100)
        public float lastActivation = 0f;      // הפעלה אחרונה

        public Artifact(string name, ArtifactType type, ArtifactRarity rare)
        {
            artifactName = name;
            discoveredName = name;
            trueName = "";
            artifactType = type;
            rarity = rare;
            discoveryTime = Time.time;

            GenerateRandomProperties();
        }

        private void GenerateRandomProperties()
        {
            // יצירת תכונות רנדומליות על בסיס נדירות
            float rarityMultiplier = ((int)rarity + 1) / 7f;

            age = UnityEngine.Random.Range(100f, 5000f) * rarityMultiplier;
            weight = UnityEngine.Random.Range(0.1f, 10f);
            dimensions = new Vector3(
                UnityEngine.Random.Range(0.1f, 2f),
                UnityEngine.Random.Range(0.1f, 2f),
                UnityEngine.Random.Range(0.1f, 0.5f)
            );

            culturalValue = UnityEngine.Random.Range(50f, 500f) * rarityMultiplier;
            scientificValue = UnityEngine.Random.Range(20f, 200f) * rarityMultiplier;
            economicValue = UnityEngine.Random.Range(100f, 1000f) * rarityMultiplier;

            requiredResearch = UnityEngine.Random.Range(50f, 300f) * rarityMultiplier;

            // יצירת כוחות על בסיס נדירות
            int powerCount = UnityEngine.Random.Range(0, (int)rarity + 1);
            for (int i = 0; i < powerCount; i++)
            {
                powers.Add(GenerateRandomPower());
            }
        }

        private ArtifactPower GenerateRandomPower()
        {
            string[] powerNames = {
            "Healing Aura", "Strength Enhancement", "Wisdom Boost", "Luck Charm",
            "Speed Blessing", "Protection Ward", "Fertility Boost", "Fear Ward",
            "Communication Aid", "Memory Enhancement", "Skill Amplifier", "Energy Boost"
        };

            string powerName = powerNames[UnityEngine.Random.Range(0, powerNames.Length)];
            string description = $"Mystical power that affects {powerName.ToLower()}";
            float intensity = UnityEngine.Random.Range(0.1f, 2f);

            var power = new ArtifactPower(powerName, description, intensity);
            power.range = UnityEngine.Random.Range(1f, 10f);
            power.requiresActivation = UnityEngine.Random.value < 0.3f;

            return power;
        }

        public void Activate(MinipollBrain activator)
        {
            if (!isDeciphered || !isActive) return;

            lastActivation = Time.time;

            foreach (var power in powers)
            {
                if (power.requiresActivation)
                {
                    ApplyPowerEffect(power, activator);
                }
            }
        }

        private void ApplyPowerEffect(ArtifactPower power, MinipollBrain target)
        {
            // יישום השפעות החפץ
            if (target == null) return;

            switch (power.powerName)
            {
                case "Healing Aura":
                    // Assuming MinipollBrain has a Heal method or similar functionality
                    // For example, if it's on MinipollCore component:
                    MinipollHealthSystem healthSystem = target.GetComponent<MinipollHealthSystem>();
                    if (healthSystem != null) healthSystem.Heal(power.intensity * 20f);
                    break;

                case "Strength Enhancement":
                    var genetics = target.GetComponent<MinipollGeneticsSystem>();
                    if (genetics != null)
                    {
                        // זמני - ניתן להוסיף מערכת באפים זמניים
                        // e.g., genetics.ApplyTemporaryBuff(GeneType.Strength, power.intensity, 300f);
                    }
                    break;

                case "Wisdom Boost":
                    var skillSystem = target.GetComponent<MinipollSkillSystem>();
                    if (skillSystem != null)
                    {
                        // Assuming SkillType.Teaching exists
                        skillSystem.AddSkillBuff(SkillType.Teaching, 1f + power.intensity, 300f, "Artifact Power");
                    }
                    break;

                case "Energy Boost":
                    var needsSystem = target.GetNeedsSystem() as MinipollNeedsSystem;
                    if (needsSystem != null && needsSystem.energy != null) // Added null check for needsSystem.energy
                    {
                        // needsSystem.energy.FillValue(power.intensity * 30f);
                    }
                    break;
            }
        }

        public void Research(MinipollBrain researcher, float researchAmount)
        {
            if (isDeciphered) return;

            researchProgress += researchAmount;

            if (!researchers.Contains(researcher))
            {
                researchers.Add(researcher);
            }

            // הוספת הערת מחקר
            if (UnityEngine.Random.value < 0.3f)
            {
                string note = GenerateResearchNote(researcher);
                researchNotes.Add($"[{researcher.name}]: {note}");
            }

            // בדיקת השלמת מחקר
            if (researchProgress >= requiredResearch)
            {
                CompleteResearch();
            }
        }

        private string GenerateResearchNote(MinipollBrain researcher)
        {
            string[] notes = {
            "Found unusual markings on the surface",
            "Material appears to be of unknown composition",
            "Detected faint energy emanations",
            "Symbol patterns suggest ancient origin",
            "Weight distribution is abnormal",
            "Shows signs of advanced craftsmanship",
            "May have been used in rituals",
            "Contains traces of rare minerals"
        };

            return notes[UnityEngine.Random.Range(0, notes.Length)];
        }

        private void CompleteResearch()
        {
            isDeciphered = true;

            // גילוי השם האמיתי
            if (string.IsNullOrEmpty(trueName))
            {
                trueName = GenerateTrueName();
            }

            // הוספת ערך תרבותי
            culturalValue += 100f;
            scientificValue += 50f;
        }

        private string GenerateTrueName()
        {
            string[] prefixes = { "Ancient", "Sacred", "Lost", "Forgotten", "Divine", "Mystic" };
            string[] suffixes = { "Relic", "Artifact", "Totem", "Charm", "Talisman", "Icon" };

            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];

            return $"{prefix} {suffix}";
        }

        public float GetTotalValue()
        {
            return culturalValue + scientificValue + economicValue;
        }
    }

    internal class MinipollHealthSystem
    {
        internal void Heal(float v)
        {
            throw new NotImplementedException();
        }
    }

    public class MinipollArtifactSystem : MonoBehaviour
    {
        [Header("Discovery Settings")]
        public bool enableArtifactDiscovery = true;
        public float discoveryChance = 0.001f;     // סיכוי גילוי בשנייה
        public float baseDiscoveryRadius = 2f;     // רדיוס גילוי בסיסי

        [Header("Artifact Database")]
        public List<Artifact> discoveredArtifacts = new List<Artifact>();
        public Dictionary<BiomeType, float> biomeDiscoveryModifiers = new Dictionary<BiomeType, float>();

        [Header("Museum System")]
        public bool hasMuseum = false;
        public Vector3 museumLocation;
        public List<Artifact> museumCollection = new List<Artifact>();
        public int museumCapacity = 50;
        public float museumCulturalImpact = 0f;

        [Header("Research Settings")]
        public float baseResearchRate = 1f;
        public float groupResearchBonus = 0.5f;    // בונוס למחקר קבוצתי
        public int maxResearchersPerArtifact = 3;

        [Header("Trading")]
        public List<Artifact> artifactsForTrade = new List<Artifact>();
        public Dictionary<ArtifactRarity, float> rarityPriceMultipliers = new Dictionary<ArtifactRarity, float>();

        [Header("Global Effects")]
        public float totalCulturalInfluence = 0f;
        public Dictionary<string, float> civilizationBonuses = new Dictionary<string, float>();

        [Header("Debug")]
        public bool debugMode = false;
        public bool showArtifactLocations = true;

        // Singleton
        public static MinipollArtifactSystem Instance { get; private set; }

        // Events
        public event Action<Artifact, MinipollBrain> OnArtifactDiscovered;
        public event Action<Artifact> OnArtifactResearched;
        public event Action<Artifact, MinipollBrain> OnArtifactActivated;
        public event Action<Artifact> OnArtifactAddedToMuseum;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeArtifactSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [Obsolete]
        private void Start()
        {
            StartCoroutine(ArtifactDiscoveryCoroutine());
        }

        private void InitializeArtifactSystem()
        {
            // הגדרת מודיפקטורי גילוי לפי ביום
            biomeDiscoveryModifiers[BiomeType.Desert] = 2f;      // מדבר - הרבה עתיקות
            biomeDiscoveryModifiers[BiomeType.Mountains] = 1.5f; // הרים - מערות עתיקות
            biomeDiscoveryModifiers[BiomeType.Forest] = 0.8f;    // יער - פחות סיכוי
            biomeDiscoveryModifiers[BiomeType.Plains] = 1f;      // מישורים - רגיל
            biomeDiscoveryModifiers[BiomeType.Swamp] = 1.2f;     // ביצה - שימור טוב
            biomeDiscoveryModifiers[BiomeType.Coast] = 1.3f;     // חוף - ימי
            biomeDiscoveryModifiers[BiomeType.Cave] = 3f;        // מערה - הכי טוב

            // הגדרת מחירי נדירות
            rarityPriceMultipliers[ArtifactRarity.Common] = 1f;
            rarityPriceMultipliers[ArtifactRarity.Uncommon] = 2f;
            rarityPriceMultipliers[ArtifactRarity.Rare] = 5f;
            rarityPriceMultipliers[ArtifactRarity.Epic] = 10f;
            rarityPriceMultipliers[ArtifactRarity.Legendary] = 25f;
            rarityPriceMultipliers[ArtifactRarity.Mythical] = 50f;
            rarityPriceMultipliers[ArtifactRarity.Divine] = 100f;
        }

        [Obsolete]
        private System.Collections.IEnumerator ArtifactDiscoveryCoroutine()
        {
            while (enableArtifactDiscovery)
            {
                yield return new WaitForSeconds(1f);

                if (UnityEngine.Random.value < discoveryChance)
                {
                    AttemptArtifactDiscovery();
                }
            }
        }

        #region Artifact Discovery

        [Obsolete]
        private void AttemptArtifactDiscovery()
        {
            var allMinipoll = FindObjectsOfType<MinipollBrain>();
            var aliveMinipoll = allMinipoll.Where(m => m.IsAlive).ToList();

            if (aliveMinipoll.Count == 0) return;

            // בחירת מיניפול רנדומלי לניסיון גילוי
            var discoverer = aliveMinipoll[UnityEngine.Random.Range(0, aliveMinipoll.Count)];

            // בדיקת תנאי גילוי
            if (CanDiscoverArtifact(discoverer))
            {
                var artifact = GenerateArtifact(discoverer);
                DiscoverArtifact(artifact, discoverer);
            }
        }

        private bool CanDiscoverArtifact(MinipollBrain discoverer)
        {
            if (discoverer == null) return false;

            float currentDiscoveryChance = this.discoveryChance; // Use a local variable for calculations

            // בדיקת כישורים רלוונטיים
            var skillSystem = discoverer.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null)
            {
                int explorationSkill = (int)skillSystem.GetSkillLevel(SkillType.Navigation); // Assuming SkillType.Navigation
                int gatheringSkill = (int)skillSystem.GetSkillLevel(SkillType.Gathering);   // Assuming SkillType.Gathering

                // כישורים גבוהים מגדילים סיכוי
                float skillBonus = (explorationSkill + gatheringSkill) * 0.01f; // Adjusted bonus factor
                currentDiscoveryChance += skillBonus;
            }

            // בדיקת ביום
            if (MinipollStreamingWorldSystem.Instance != null)
            {
                if (MinipollStreamingWorldSystem.Instance.GetChunkAt(discoverer.transform.position) is Chunk chunk && biomeDiscoveryModifiers.ContainsKey(chunk.biome))
                {
                    currentDiscoveryChance *= biomeDiscoveryModifiers[chunk.biome];
                }
            }
            else
            {
                if (debugMode) Debug.LogWarning("MinipollStreamingWorldSystem.Instance is null. Biome modifier not applied.");
            }

            // סקרנות גנטית
            var genetics = discoverer.GetComponent<MinipollGeneticsSystem>();
            if (genetics != null)
            {
                float curiosity = genetics.GetGeneValue(GeneType.Curiosity); // Assuming GeneType.Curiosity
                currentDiscoveryChance *= (0.5f + curiosity);
            }

            return UnityEngine.Random.value < currentDiscoveryChance;
        }

        private Artifact GenerateArtifact(MinipollBrain discoverer)
        {
            // קביעת נדירות על בסיס הסתברות
            ArtifactRarity rarity = DetermineRarity();

            // קביעת סוג על בסיס ביום ותכונות
            ArtifactType type = DetermineType(discoverer);

            // יצירת החפץ
            string artifactName = GenerateArtifactName(type, rarity);
            var artifact = new Artifact(artifactName, type, rarity);

            // הגדרת מיקום ונסיבות גילוי
            artifact.discoveryLocation = discoverer.transform.position;
            artifact.discoveredBy = discoverer;
            artifact.discoveryCircumstances = GenerateDiscoveryStory(discoverer);

            if (MinipollStreamingWorldSystem.Instance != null)
            {
                var chunk = MinipollStreamingWorldSystem.Instance.GetChunkAt(discoverer.transform.position) as Chunk;
                if (chunk != null)
                {
                    artifact.discoveryBiome = chunk.biome;
                }
            }

            // קביעת מקור
            artifact.origin = DetermineOrigin(type, rarity);

            return artifact;
        }

        private ArtifactRarity DetermineRarity()
        {
            float rand = UnityEngine.Random.value;

            if (rand < 0.45f) return ArtifactRarity.Common;
            if (rand < 0.70f) return ArtifactRarity.Uncommon;
            if (rand < 0.85f) return ArtifactRarity.Rare;
            if (rand < 0.94f) return ArtifactRarity.Epic;
            if (rand < 0.98f) return ArtifactRarity.Legendary;
            if (rand < 0.999f) return ArtifactRarity.Mythical;
            return ArtifactRarity.Divine;
        }

        private ArtifactType DetermineType(MinipollBrain discoverer)
        {
            // הטיה לפי כישורים של המגלה
            var skillSystem = discoverer.GetComponent<MinipollSkillSystem>();

            if (skillSystem != null)
            {
                var topSkills = skillSystem.GetTopSkills(3);

                foreach (var skill in topSkills)
                {
                    switch (skill.skillType)
                    {
                        case SkillType.Combat:
                            if (UnityEngine.Random.value < 0.4f) return ArtifactType.Weapon;
                            break;
                        case SkillType.Crafting:
                            if (UnityEngine.Random.value < 0.4f) return ArtifactType.Tool;
                            break;
                        case SkillType.Art:
                            if (UnityEngine.Random.value < 0.4f) return ArtifactType.Art;
                            break;
                        case SkillType.Medicine:
                            if (UnityEngine.Random.value < 0.3f) return ArtifactType.Scientific;
                            break;
                    }
                }
            }

            // ברירת מחדל רנדומלית
            var types = Enum.GetValues(typeof(ArtifactType));
            return (ArtifactType)types.GetValue(UnityEngine.Random.Range(0, types.Length));
        }

        private ArtifactOrigin DetermineOrigin(ArtifactType type, ArtifactRarity rarity)
        {
            // נדירות גבוהה = מקור יותר אקזוטי
            if (rarity >= ArtifactRarity.Mythical)
            {
                return UnityEngine.Random.value < 0.5f ? ArtifactOrigin.AlienVisitors : ArtifactOrigin.TimeAnomaly;
            }
            else if (rarity >= ArtifactRarity.Epic)
            {
                return UnityEngine.Random.value < 0.7f ? ArtifactOrigin.LostTribe : ArtifactOrigin.MysticalSource;
            }
            else
            {
                return ArtifactOrigin.AncientCivilization;
            }
        }

        private string GenerateArtifactName(ArtifactType type, ArtifactRarity rarity)
        {
            string[] rarityAdjectives = {
            "Old", "Aged", "Unusual", "Rare", "Legendary", "Mythical", "Divine"
        };

            string[] typeNouns = {
            "Tool", "Weapon", "Ornament", "Sculpture", "Relic",
            "Instrument", "Tablet", "Charm", "Symbol", "Fragment"
        };

            string adjective = rarityAdjectives[(int)rarity];
            string noun = typeNouns[UnityEngine.Random.Range(0, typeNouns.Length)];

            return $"{adjective} {noun}";
        }

        private string GenerateDiscoveryStory(MinipollBrain discoverer)
        {
            string[] circumstances = {
            "Found while gathering resources in the area",
            "Discovered buried under ancient debris",
            "Unearthed during exploration of a cave",
            "Located following strange markings on rocks",
            "Found after a vivid dream led here",
            "Discovered while seeking shelter from weather",
            "Noticed glinting in sunlight among stones",
            "Found while investigating unusual sounds"
        };

            return circumstances[UnityEngine.Random.Range(0, circumstances.Length)];
        }

        private void DiscoverArtifact(Artifact artifact, MinipollBrain discoverer)
        {
            if (artifact == null || discoverer == null) return;

            discoveredArtifacts.Add(artifact);
            artifact.currentOwner = discoverer;

            // הוספה למלאי של המגלה אם יש מערכת כלכלה
            var economySystem = discoverer.GetComponent<MinipollEconomySystem>();
            if (economySystem != null)
            {
                // Assuming EconomicItem class is defined
                var economicItem = new EconomicItem(artifact.artifactName, artifact.economicValue);
                economicItem.category = "Artifact";
                economicItem.quality = 1f + ((int)artifact.rarity * 0.2f);
                // economySystem.AddItem(economicItem);
            }

            // נסיון בכישורים
            var skillSystem = discoverer.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null)
            {
                skillSystem.GainExperience(SkillType.Navigation, 5f * (int)artifact.rarity);
                skillSystem.GainExperience(SkillType.Gathering, 3f * (int)artifact.rarity);
            }

            // שיפור מוניטין
            // הגישה הקודמת הניחה קיום רכיב דיפלומטיה על המגלה עם שדה מוניטין גלובלי,
            // וזה פחות סביר בהינתן DiplomacyManager כ-Singleton.
            // המוניטין הוא כנראה תכונה של MinipollBrain עצמו או של השבט שלו.

            // דוגמה להעלאת מוניטין (דורש מימוש ב-MinipollBrain):
            // ודא של-MinipollBrain יש שדה public float reputation; או מתודה public void GainReputation(float amount);
            // למשל:
            // if (discoverer != null)
            // {
            //     // Option 1: Direct field access (add 'public float reputation;' to MinipollBrain class)
            //     // discoverer.reputation += (int)artifact.rarity * 2f;
            //
            //     // Option 2: Method call (add 'public void GainReputation(float amount);' to MinipollBrain class)
            //     // discoverer.GainReputation((int)artifact.rarity * 2f);
            //
            //     // Option 3: If reputation is managed by the Tribe associated with the MinipollBrain
            //     // Tribe discovererTribe = discoverer.GetTribe(); // Assuming MinipollBrain has GetTribe()
            //     // if (discovererTribe != null)
            //     // {
            //     //     discovererTribe.GainReputation((int)artifact.rarity * 2f); // Assuming Tribe has GainReputation()
            //     // }
            //
            //     if (debugMode)
            //         Debug.Log($"{discoverer.name} gained reputation for discovering {artifact.artifactName}. (Actual implementation depends on MinipollBrain/Tribe structure)");
            // }
            // מכיוון שהמימוש המדויק תלוי בקוד חיצוני, אשאיר זאת כהערה מנחה.
            // יש לוודא שהלוגיקה לשינוי מוניטין ממומשת במקום המתאים (MinipollBrain או Tribe).

            OnArtifactDiscovered?.Invoke(artifact, discoverer);

            if (debugMode)
                Debug.Log($"{discoverer.name} discovered {artifact.artifactName} ({artifact.rarity})");

            // הוספת זיכרון חיובי
            var memorySystem = discoverer.GetMemorySystem();
            if (memorySystem != null)
            {
                // memorySystem.AddMemory($"Discovered ancient artifact: {artifact.artifactName}",
                                    //   null, true, 0.8f + (int)artifact.rarity * 0.1f);
            }
        }

        #endregion

        #region Research System

        public void ResearchArtifact(Artifact artifact, MinipollBrain researcher)
        {
            if (artifact == null || researcher == null) return;
            if (artifact.isDeciphered) return;
            if (artifact.researchers.Count >= maxResearchersPerArtifact && !artifact.researchers.Contains(researcher)) return;

            // חישוב כמות מחקר
            float researchAmount = baseResearchRate;

            // בונוס מכישורים
            var skillSystem = researcher.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null)
            {
                // researchAmount *= skillSystem.GetSkillEfficiency(SkillType.Medicine); // מדע // Assuming SkillType.Medicine
                // researchAmount *= skillSystem.GetSkillEfficiency(SkillType.Teaching); // מחקר // Assuming SkillType.Teaching
            }

            // בונוס מאינטליגנציה
            var genetics = researcher.GetComponent<MinipollGeneticsSystem>();
            if (genetics != null)
            {
                researchAmount *= (0.5f + genetics.GetGeneValue(GeneType.Intelligence)); // Assuming GeneType.Intelligence
            }

            // בונוס מחקר קבוצתי
            if (artifact.researchers.Count > 1)
            {
                researchAmount *= (1f + artifact.researchers.Count * groupResearchBonus);
            }

            // ביצוע המחקר
            artifact.Research(researcher, researchAmount);

            // נסיון בכישורי מחקר
            if (skillSystem != null)
            {
                skillSystem.GainExperience(SkillType.Medicine, 1f);
                skillSystem.GainExperience(SkillType.Teaching, 0.5f);
            }

            if (artifact.isDeciphered)
            {
                OnArtifactResearched?.Invoke(artifact);

                // פרס למחקרים
                foreach (var researcherBrain in artifact.researchers)
                {
                    var researcherSkills = researcherBrain.GetComponent<MinipollSkillSystem>();
                    if (researcherSkills != null)
                    {
                        researcherSkills.GainExperience(SkillType.Medicine, 10f);
                    }
                }

                if (debugMode)
                    Debug.Log($"Artifact {artifact.artifactName} has been fully researched!");
            }
        }

        #endregion

        #region Museum System

        public void CreateMuseum(Vector3 location)
        {
            if (hasMuseum) return;

            hasMuseum = true;
            museumLocation = location;
            museumCollection.Clear();

            if (debugMode)
                Debug.Log("Museum created at " + location);
        }

        public bool AddToMuseum(Artifact artifact)
        {
            if (!hasMuseum || museumCollection.Count >= museumCapacity) return false;
            if (museumCollection.Contains(artifact)) return false;

            museumCollection.Add(artifact);
            artifact.isInMuseum = true;
            artifact.timesDisplayed++;

            // עדכון השפעה תרבותית
            museumCulturalImpact += artifact.culturalValue * 0.1f;
            totalCulturalInfluence += artifact.culturalValue * 0.05f;

            OnArtifactAddedToMuseum?.Invoke(artifact);

            if (debugMode)
                Debug.Log($"Added {artifact.artifactName} to museum");

            return true;
        }

        public void VisitMuseum(MinipollBrain visitor)
        {
            if (!hasMuseum || museumCollection.Count == 0 || visitor == null) return;

            // השפעה חיובית מביקור במוזאון
            var needsSystem = visitor.GetNeedsSystem() as MinipollNeedsSystem;
            if (needsSystem != null)
            {
                // if (needsSystem.social != null) needsSystem.social.FillValue(20f); // Added null check
                                                                                   // if (needsSystem.culture != null) needsSystem.culture.FillValue(30f); // אם יש צורך תרבותי
            }

            // נסיון בכישורי אמנות ותרבות
            var skillSystem = visitor.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null)
            {
                skillSystem.GainExperience(SkillType.Art, 2f); // Assuming SkillType.Art
            }

            // שיפור מצב רוח
            var emotionsSystem = visitor.GetEmotionsSystem();
            if (emotionsSystem != null)
            {
                // emotionsSystem.ChangeEmotion(Emotion.Happiness, 15f); // Assuming Emotion enum and ChangeEmotion method
            }

            if (debugMode)
                Debug.Log($"{visitor.name} visited the museum");
        }

        #endregion

        #region Artifact Trading

        public void SetArtifactForTrade(Artifact artifact, bool forTrade)
        {
            artifact.isForTrade = forTrade;

            if (forTrade && !artifactsForTrade.Contains(artifact))
            {
                artifactsForTrade.Add(artifact);
            }
            else if (!forTrade)
            {
                artifactsForTrade.Remove(artifact);
            }
        }

        public float CalculateTradeValue(Artifact artifact)
        {
            float baseValue = artifact.economicValue;

            // מכפיל נדירות
            if (rarityPriceMultipliers.ContainsKey(artifact.rarity))
            {
                baseValue *= rarityPriceMultipliers[artifact.rarity];
            }

            // בונוס אם מחקר הושלם
            if (artifact.isDeciphered)
            {
                baseValue *= 1.5f;
            }

            // פגיעה במחיר אם פגום
            if (artifact.isDamaged)
            {
                baseValue *= artifact.condition / 100f;
            }

            return baseValue;
        }

        public bool TradeArtifact(Artifact artifact, MinipollBrain from, MinipollBrain to, float price)
        {
            if (artifact == null || from == null || to == null) return false;
            if (!artifact.isForTrade || artifact.currentOwner != from) return false;

            // העברת בעלות
            artifact.previousOwners.Add(from);
            artifact.currentOwner = to;
            artifact.isForTrade = false;
            artifactsForTrade.Remove(artifact);

            // עדכון מערכות כלכליות
            var fromEconomy = from.GetComponent<MinipollEconomySystem>();
            var toEconomy = to.GetComponent<MinipollEconomySystem>();

            if (fromEconomy != null && toEconomy != null)
            {
                fromEconomy.EarnCurrency(price, "Artifact sale");
                toEconomy.SpendCurrency(price, "Artifact purchase");
            }

            if (debugMode)
                Debug.Log($"{from.name} sold {artifact.artifactName} to {to.name} for {price}");

            return true;
        }

        #endregion

        #region Global Effects

        private void UpdateGlobalEffects()
        {
            // חישוב השפעות גלובליות של חפצים
            totalCulturalInfluence = 0f;
            civilizationBonuses.Clear();

            foreach (var artifact in discoveredArtifacts)
            {
                if (artifact.isDeciphered && !artifact.isLost)
                {
                    totalCulturalInfluence += artifact.culturalValue * 0.01f;

                    // בונוסים ספציפיים לפי סוג חפץ
                    switch (artifact.artifactType)
                    {
                        case ArtifactType.Scientific:
                            AddCivilizationBonus("Research Speed", artifact.scientificValue * 0.05f);
                            break;
                        case ArtifactType.Cultural:
                            AddCivilizationBonus("Social Cohesion", artifact.culturalValue * 0.03f);
                            break;
                        case ArtifactType.Religious:
                            AddCivilizationBonus("Spiritual Strength", artifact.culturalValue * 0.04f);
                            break;
                    }
                }
            }
        }

        private void AddCivilizationBonus(string bonusName, float value)
        {
            if (!civilizationBonuses.ContainsKey(bonusName))
            {
                civilizationBonuses[bonusName] = 0f;
            }
            civilizationBonuses[bonusName] += value;
        }

        #endregion

        #region Public Interface

        public List<Artifact> GetDiscoveredArtifacts()
        {
            return new List<Artifact>(discoveredArtifacts);
        }

        public List<Artifact> GetArtifactsByOwner(MinipollBrain owner)
        {
            return discoveredArtifacts.Where(a => a.currentOwner == owner).ToList();
        }

        public List<Artifact> GetArtifactsByRarity(ArtifactRarity rarity)
        {
            return discoveredArtifacts.Where(a => a.rarity == rarity).ToList();
        }

        public List<Artifact> GetArtifactsForTrade()
        {
            return new List<Artifact>(artifactsForTrade);
        }

        public Artifact GetMostValuableArtifact()
        {
            return discoveredArtifacts.OrderByDescending(a => a.GetTotalValue()).FirstOrDefault();
        }

        public float GetTotalCulturalInfluence()
        {
            return totalCulturalInfluence;
        }

        public Dictionary<string, float> GetCivilizationBonuses()
        {
            return new Dictionary<string, float>(civilizationBonuses);
        }

        public string GenerateArtifactReport()
        {
            var report = "=== ARTIFACT COLLECTION REPORT ===\n\n";

            report += $"Total Artifacts Discovered: {discoveredArtifacts.Count}\n";
            report += $"Research Progress: {discoveredArtifacts.Count(a => a.isDeciphered)}/{discoveredArtifacts.Count} deciphered\n";
            report += $"Total Cultural Influence: {totalCulturalInfluence:F1}\n\n";

            // התפלגות לפי נדירות
            report += "Rarity Distribution:\n";
            foreach (ArtifactRarity rarity in Enum.GetValues(typeof(ArtifactRarity)))
            {
                int count = discoveredArtifacts.Count(a => a.rarity == rarity);
                if (count > 0)
                {
                    report += $"  {rarity}: {count}\n";
                }
            }

            // מוזאון
            if (hasMuseum)
            {
                report += $"\nMuseum Collection: {museumCollection.Count}/{museumCapacity}\n";
                report += $"Museum Cultural Impact: {museumCulturalImpact:F1}\n";
            }

            // חפצים יקרי ערך
            var valuable = discoveredArtifacts.OrderByDescending(a => a.GetTotalValue()).Take(5);
            report += "\nMost Valuable Artifacts:\n";
            foreach (var artifact in valuable)
            {
                report += $"  {artifact.artifactName} - {artifact.GetTotalValue():F0} value\n";
            }

            return report;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showArtifactLocations || !debugMode) return;

            foreach (var artifact in discoveredArtifacts)
            {
                if (artifact.isLost) continue;

                Color gizmoColor = Color.yellow;
                switch (artifact.rarity)
                {
                    case ArtifactRarity.Rare: gizmoColor = Color.blue; break;
                    case ArtifactRarity.Epic: gizmoColor = Color.magenta; break;
                    case ArtifactRarity.Legendary: gizmoColor = Color.red; break;
                    case ArtifactRarity.Mythical: gizmoColor = Color.cyan; break;
                    case ArtifactRarity.Divine: gizmoColor = Color.white; break;
                }

                Gizmos.color = gizmoColor;

                if (artifact.currentOwner != null)
                {
                    Gizmos.DrawWireSphere(artifact.currentOwner.transform.position + Vector3.up * 2f, 0.5f);
                }
                else
                {
                    Gizmos.DrawWireSphere(artifact.discoveryLocation, 0.3f);
                }
            }

            // מוזאון
            if (hasMuseum)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(museumLocation, Vector3.one * 3f);
            }
        }

        public void LogArtifactStatus()
        {
            Debug.Log(GenerateArtifactReport());
        }

        #endregion
    }

    internal class Chunk
    {
        internal BiomeType biome;
    }

    internal class EconomicItem
    {
        internal string category;
        internal float quality;
        private string artifactName;
        private float economicValue;

        public EconomicItem(string artifactName, float economicValue)
        {
            this.artifactName = artifactName;
            this.economicValue = economicValue;
        }
    }
}