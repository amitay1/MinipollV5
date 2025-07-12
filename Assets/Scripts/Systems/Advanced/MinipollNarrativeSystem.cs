/*************************************************************
 *  MinipollNarrativeSystem.cs
 *  
 *  תיאור כללי:
 *    מערכת נרטיב וסיפור דינמי:
 *      - יצירת סיפורים על בסיס אירועים
 *      - מעקב אחר דמויות ומערכות יחסים
 *      - זיכרון קולקטיבי והיסטוריה
 *      - יצירת אגדות ומיתוסים
 *      - תיעוד הישגים ואירועים חשובים
 *  
 *  דרישות קדם:
 *    - להניח על GameObject ריק בסצנה (NarrativeManager)
 *    - עבודה עם כל המערכות האחרות
 *************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using MinipollCore;
using MinipollGame.Systems.Core;
using MinipollGame.Core;
using MinipollGame.Social;
using MinipollGame.Systems.Advanced;
namespace MinipollGame.Systems.Core
{
    [System.Serializable]
    public enum StoryType
    {
        Personal,       // סיפור אישי
        Tribal,         // סיפור שבטי
        Heroic,         // סיפור גבורה
        Tragic,         // טרגדיה
        Romance,        // רומנטיקה
        Adventure,      // הרפתקה
        Discovery,      // גילוי
        Conflict,       // קונפליקט
        Legend,         // אגדה
        Myth,          // מיתוס
        Historical,     // היסטורי
        Prophetic      // נבואי
    }

    [System.Serializable]
    public enum StoryImportance
    {
        Trivial,        // טריוויאלי
        Minor,          // מינורי
        Significant,    // משמעותי
        Major,          // מרכזי
        Epic,           // אפי
        Legendary      // אגדי
    }

    [System.Serializable]
    public class StoryEvent
    {
        [Header("Event Details")]
        public string eventId;
        public string eventName;
        public string description;
        public float timestamp;
        public Vector3 location;
        public BiomeType biome;

        [Header("Characters Involved")]
        public List<MinipollBrain> protagonists = new List<MinipollBrain>();
        public List<MinipollBrain> antagonists = new List<MinipollBrain>();
        public List<MinipollBrain> witnesses = new List<MinipollBrain>();

        [Header("Story Classification")]
        public StoryType storyType;
        public StoryImportance importance;
        public List<string> tags = new List<string>();

        [Header("Narrative Elements")]
        public string conflict;         // הקונפליקט
        public string resolution;       // הפתרון
        public string moralLesson;      // המוסר השכל
        public List<string> themes = new List<string>();

        [Header("Consequences")]
        public List<string> outcomes = new List<string>();
        public float culturalImpact = 0f;
        public bool becameLegend = false;
        public int timesRetold = 0;

        public StoryEvent(string name, string desc, StoryType type)
        {
            eventId = Guid.NewGuid().ToString();
            eventName = name;
            description = desc;
            storyType = type;
            timestamp = Time.time;
            importance = StoryImportance.Minor;
        }

        public void AddProtagonist(MinipollBrain character)
        {
            if (!protagonists.Contains(character))
            {
                protagonists.Add(character);
            }
        }

        public void AddWitness(MinipollBrain witness)
        {
            if (!witnesses.Contains(witness) && !protagonists.Contains(witness) && !antagonists.Contains(witness))
            {
                witnesses.Add(witness);
            }
        }

        public float GetAge()
        {
            return Time.time - timestamp;
        }

        public bool IsRecent()
        {
            return GetAge() < 300f; // פחות מ-5 דקות
        }

        public bool IsAncient()
        {
            return GetAge() > 3600f; // יותר משעה
        }
    }

    [System.Serializable]
    public class Character
    {
        [Header("Character Info")]
        public MinipollBrain brain;
        public string characterName;
        public string title;                    // תואר (גיבור, חכם, וכו')
        public List<string> epithets = new List<string>(); // כינויים

        [Header("Reputation")]
        public Dictionary<string, float> traits = new Dictionary<string, float>();
        public List<string> accomplishments = new List<string>();
        public List<string> failures = new List<string>();
        public float fame = 0f;
        public float infamy = 0f;

        [Header("Relationships")]
        public List<string> allies = new List<string>();
        public List<string> enemies = new List<string>();
        public List<string> rivals = new List<string>();
        public string mentor;
        public List<string> students = new List<string>();

        [Header("Story Involvement")]
        public List<string> storiesInvolved = new List<string>();
        public int heroicDeeds = 0;
        public int tragicEvents = 0;
        public bool isLegendary = false;

        public Character(MinipollBrain minipoll)
        {
            brain = minipoll;
            characterName = minipoll.name;
            InitializeTraits();
        }

        private void InitializeTraits()
        {
            traits["Courage"] = 0f;
            traits["Wisdom"] = 0f;
            traits["Kindness"] = 0f;
            traits["Strength"] = 0f;
            traits["Leadership"] = 0f;
            traits["Cunning"] = 0f;
        }

        public void AddTrait(string trait, float value)
        {
            if (!traits.ContainsKey(trait))
                traits[trait] = 0f;
            traits[trait] += value;
            traits[trait] = Mathf.Clamp(traits[trait], -100f, 100f);
        }

        public string GetDominantTrait()
        {
            if (traits.Count == 0) return "Unknown";
            return traits.OrderByDescending(t => Mathf.Abs(t.Value)).First().Key;
        }

        public string GenerateEpithet()
        {
            string dominantTrait = GetDominantTrait();
            float traitValue = traits[dominantTrait];

            if (traitValue > 50f)
            {
                switch (dominantTrait)
                {
                    case "Courage": return "the Brave";
                    case "Wisdom": return "the Wise";
                    case "Kindness": return "the Kind";
                    case "Strength": return "the Strong";
                    case "Leadership": return "the Leader";
                    case "Cunning": return "the Clever";
                }
            }
            else if (traitValue < -50f)
            {
                switch (dominantTrait)
                {
                    case "Courage": return "the Coward";
                    case "Wisdom": return "the Foolish";
                    case "Kindness": return "the Cruel";
                    case "Strength": return "the Weak";
                    case "Leadership": return "the Follower";
                    case "Cunning": return "the Simple";
                }
            }

            return "";
        }
    }

    [System.Serializable]
    public class Saga
    {
        public string sagaName;
        public string description;
        public List<string> eventIds = new List<string>();
        public List<string> mainCharacters = new List<string>();
        public string theme;
        public float culturalSignificance = 0f;
        public bool isComplete = false;

        public Saga(string name, string desc)
        {
            sagaName = name;
            description = desc;
        }

        public void AddEvent(string eventId)
        {
            if (!eventIds.Contains(eventId))
            {
                eventIds.Add(eventId);
            }
        }
    }

    public class MinipollNarrativeSystem : MonoBehaviour
    {
        [Header("Narrative Settings")]
        public bool enableNarrative = true;
        public float storyDetectionSensitivity = 1f;
        public float legendThreshold = 100f;        // ערך פעם להפיכה לאגדה
        public int maxStoriesPerSession = 1000;

        [Header("Story Database")]
        public List<StoryEvent> allStories = new List<StoryEvent>();
        public List<Character> characters = new List<Character>();
        public List<Saga> sagas = new List<Saga>();

        [Header("Cultural Memory")]
        public Dictionary<string, float> collectiveMemory = new Dictionary<string, float>();
        public List<string> culturalValues = new List<string>();
        public Dictionary<string, int> storyThemes = new Dictionary<string, int>();

        [Header("Narrative Generation")]
        public bool autoGenerateStories = true;
        public float storyGenerationInterval = 30f;
        private float storyTimer = 0f;

        [Header("Story Sharing")]
        public float storytellingChance = 0.1f;     // סיכוי לספר סיפור
        public float storytellingRadius = 5f;       // רדיוס סיפור סיפורים
        public Dictionary<string, int> storyPopularity = new Dictionary<string, int>();

        [Header("Debug")]
        public bool debugMode = false;
        public bool showNarrativeUI = true;

        // Singleton
        public static MinipollNarrativeSystem Instance { get; private set; }

        // Events
        public event Action<StoryEvent> OnStoryCreated;
        public event Action<StoryEvent> OnStoryBecameLegend;
        public event Action<Character> OnCharacterBecameLegendary;
        public event Action<Saga> OnSagaCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeNarrativeSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (enableNarrative)
            {
                StartCoroutine(NarrativeCoroutine());
                SubscribeToEvents();
            }
        }

        private void InitializeNarrativeSystem()
        {
            // אתחול ערכים תרבותיים בסיסיים
            culturalValues.Add("Courage in adversity");
            culturalValues.Add("Helping others in need");
            culturalValues.Add("Seeking knowledge and wisdom");
            culturalValues.Add("Protecting the community");
            culturalValues.Add("Honoring the ancestors");

            if (debugMode)
                Debug.Log("Narrative System initialized");
        }

        private System.Collections.IEnumerator NarrativeCoroutine()
        {
            while (enableNarrative)
            {
                yield return new WaitForSeconds(1f);

                storyTimer += 1f;

                if (storyTimer >= storyGenerationInterval)
                {
                    storyTimer = 0f;

                    if (autoGenerateStories)
                    {
                        DetectAndCreateStories();
                    }

                    UpdateCharacters();
                    UpdateStoryPopularity();
                    ProcessStorytelling();
                    UpdateSagas();
                }
            }
        }       

        private void SubscribeToEvents()
        {        // מנוי לאירועים ממערכות אחרות

            // מערכת רפרודוקציה - static events
            // Note: MinipollReproductionSystem is commented out, so skip this
            /*
            if (MinipollReproductionSystem.Instance != null)
            {
                MinipollReproductionSystem.Instance.OnOffspringBorn += HandleBirthEvent;
                MinipollReproductionSystem.Instance.OnMatingStart += HandleMatingEvent;
            }
            */
            
            // מערכת קרב - subscribe to static events if they exist
            // Note: MinipollBattleSystem needs to have static events for this to work

            // מערכת דיפלומטיה - subscribe to static events
            // Note: MinipollDiplomacySystem may not have Instance property
            /*
            if (MinipollDiplomacySystem.Instance != null)
            {
                MinipollDiplomacySystem.Instance.OnWarDeclared += HandleWarEvent;
                MinipollDiplomacySystem.Instance.OnPeaceMade += HandlePeaceEvent;
            }
            */

            // מערכת חפצים עתיקים
            if (MinipollArtifactSystem.Instance != null)
            {
                MinipollArtifactSystem.Instance.OnArtifactDiscovered += HandleArtifactDiscovery;
            }
            // מערכת אירועי עולם
            if (MinipollWorldEventSystem.Instance != null)
            {
                // Use static event access
                MinipollWorldEventSystem.OnWorldEventStarted += HandleWorldEvent;
            }
        }

        #region Event Handlers
        private void HandleBirthEvent(MinipollBrain offspring)
        {
            var story = new StoryEvent(
                $"Birth of {offspring.name}",
                $"A new life has been born: {offspring.name}, bringing hope to the community",
                StoryType.Personal
            );
            story.AddProtagonist(offspring);
            story.importance = StoryImportance.Significant;
            story.themes.Add("New Life");
            story.themes.Add("Family");
            story.moralLesson = "Life continues through the generations";

            CreateStory(story);

            // עדכון תכונת דמות
            var character = GetOrCreateCharacter(offspring);
            character.AddTrait("Kindness", 10f);
            character.accomplishments.Add($"Parent of {offspring.name}");
        }

        private void HandleMatingEvent(MinipollBrain mate1, MinipollBrain mate2)
        {
            var story = new StoryEvent(
                $"Union of {mate1.name} and {mate2.name}",
                $"{mate1.name} and {mate2.name} formed a bond, beginning a new chapter together",
                StoryType.Romance
            );

            story.AddProtagonist(mate1);
            story.AddProtagonist(mate2);
            story.importance = StoryImportance.Minor;
            story.themes.Add("Love");
            story.themes.Add("Partnership");

            CreateStory(story);
        }

        private void HandleCombatEvent(MinipollBrain combatant1, MinipollBrain combatant2)
        {
            if (combatant1 == null || combatant2 == null) return;

            var battleSystem1 = combatant1.GetComponent<MinipollBattleSystem>();
            var battleSystem2 = combatant2.GetComponent<MinipollBattleSystem>();

            bool combatant1Won = combatant1.IsAlive && !combatant2.IsAlive;
            bool combatant2Won = combatant2.IsAlive && !combatant1.IsAlive;

            string storyName;
            string description;

            if (combatant1Won)
            {
                storyName = $"Victory of {combatant1.name}";
                description = $"{combatant1.name} emerged victorious in combat against {combatant2.name}";
            }
            else if (combatant2Won)
            {
                storyName = $"Victory of {combatant2.name}";
                description = $"{combatant2.name} emerged victorious in combat against {combatant1.name}";
            }
            else
            {
                storyName = $"Duel between {combatant1.name} and {combatant2.name}";
                description = $"{combatant1.name} and {combatant2.name} fought bravely but both survived";
            }

            var story = new StoryEvent(storyName, description, StoryType.Conflict);
            story.AddProtagonist(combatant1);
            story.AddProtagonist(combatant2);
            story.importance = StoryImportance.Significant;
            story.themes.Add("Combat");
            story.themes.Add("Bravery");

            CreateStory(story);

            // עדכון תכונות דמויות
            var char1 = GetOrCreateCharacter(combatant1);
            var char2 = GetOrCreateCharacter(combatant2);

            char1.AddTrait("Courage", 15f);
            char2.AddTrait("Courage", 15f);

            if (combatant1Won)
            {
                char1.AddTrait("Strength", 20f);
                char1.heroicDeeds++;
                char2.tragicEvents++;
            }
            else if (combatant2Won)
            {
                char2.AddTrait("Strength", 20f);
                char2.heroicDeeds++;
                char1.tragicEvents++;
            }
        }

        private void HandleVictoryEvent(MinipollBrain victor)
        {
            var character = GetOrCreateCharacter(victor);
            character.heroicDeeds++;

            if (character.heroicDeeds >= 5)
            {
                var epithet = character.GenerateEpithet();
                if (!string.IsNullOrEmpty(epithet) && !character.epithets.Contains(epithet))
                {
                    character.epithets.Add(epithet);

                    var story = new StoryEvent(
                        $"{victor.name} Earns Title",
                        $"Through many brave deeds, {victor.name} has earned the title '{epithet}'",
                        StoryType.Heroic
                    );

                    story.AddProtagonist(victor);
                    story.importance = StoryImportance.Major;
                    story.themes.Add("Recognition");
                    story.themes.Add("Honor");

                    CreateStory(story);
                }
            }
        }
        private void HandleWarEvent(Tribe tribeA, Tribe tribeB)
        {
            var story = new StoryEvent(
                $"War Declared",
                $"Tribe {tribeA.tribeName} declared war upon tribe {tribeB.tribeName}, beginning a time of conflict",
                StoryType.Conflict
            );
            if (tribeA.leader != null) story.AddProtagonist(tribeA.leader.GetComponent<MinipollBrain>());
            if (tribeB.leader != null) story.AddProtagonist(tribeB.leader.GetComponent<MinipollBrain>());
            story.importance = StoryImportance.Major;
            story.themes.Add("War");
            story.themes.Add("Conflict");
            story.moralLesson = "War brings suffering to all";

            CreateStory(story);
        }
        private void HandlePeaceEvent(Tribe tribeA, Tribe tribeB)
        {
            var story = new StoryEvent(
                $"Peace Restored",
                $"Tribe {tribeA.tribeName} and tribe {tribeB.tribeName} made peace, ending their conflict and choosing harmony",
                StoryType.Historical
            );

            if (tribeA.leader != null) story.AddProtagonist(tribeA.leader.GetComponent<MinipollBrain>());
            if (tribeB.leader != null) story.AddProtagonist(tribeB.leader.GetComponent<MinipollBrain>());
            story.importance = StoryImportance.Major;
            story.themes.Add("Peace");
            story.themes.Add("Reconciliation"); story.moralLesson = "Peace is more valuable than victory";

            CreateStory(story);

            // עדכון תכונות דמויות
            if (tribeA.leader != null)
            {
                var char1 = GetOrCreateCharacter(tribeA.leader.GetComponent<MinipollBrain>());
                char1.AddTrait("Wisdom", 15f);
            }
            if (tribeB.leader != null)
            {
                var char2 = GetOrCreateCharacter(tribeB.leader.GetComponent<MinipollBrain>());
                char2.AddTrait("Wisdom", 15f);
            }
        }

        private void HandleArtifactDiscovery(Artifact artifact, MinipollBrain discoverer)
        {
            var story = new StoryEvent(
                $"Discovery of {artifact.artifactName}",
                $"{discoverer.name} discovered the ancient {artifact.artifactName}, revealing secrets of the past",
                StoryType.Discovery
            );

            story.AddProtagonist(discoverer);
            story.importance = artifact.rarity >= ArtifactRarity.Epic ? StoryImportance.Epic : StoryImportance.Significant;
            story.themes.Add("Discovery");
            story.themes.Add("Ancient Wisdom");
            story.moralLesson = "Knowledge from the past guides the future";

            CreateStory(story);

            // עדכון תכונות דמות
            var character = GetOrCreateCharacter(discoverer);
            character.AddTrait("Wisdom", 10f + (int)artifact.rarity * 5f);
            character.accomplishments.Add($"Discovered {artifact.artifactName}");
        }

        private void HandleWorldEvent(WorldEvent worldEvent)
        {
            var story = new StoryEvent(
                $"The {worldEvent.eventName}",
                $"A great {worldEvent.eventName} occurred, affecting all who witnessed it",
                StoryType.Historical
            );

            story.importance = worldEvent.intensity > 1.5f ? StoryImportance.Epic : StoryImportance.Major;
            story.themes.Add("Nature");
            story.themes.Add("Survival");
            // הוספת עדים
            var allMinipoll = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None).Where(m => m.IsAlive);
            foreach (var minipoll in allMinipoll)
            {
                story.AddWitness(minipoll);
            }

            CreateStory(story);
        }

        #endregion

        #region Story Creation and Management

        public void CreateStory(StoryEvent story)
        {
            if (allStories.Count >= maxStoriesPerSession) return;

            // הגדרת מיקום וביום
            if (story.protagonists.Count > 0)
            {
                story.location = story.protagonists[0].transform.position;
                if (MinipollStreamingWorldSystem.Instance != null)
                {
                    var chunk = MinipollStreamingWorldSystem.Instance.GetChunkAt(story.location) as Chunk;
                    if (chunk != null)
                    {
                        story.biome = chunk.biome;
                    }
                }
            }

            // חישוב השפעה תרבותית
            story.culturalImpact = CalculateCulturalImpact(story);

            // הוספה למאגר
            allStories.Add(story);

            // עדכון נושאים תרבותיים
            foreach (var theme in story.themes)
            {
                if (!storyThemes.ContainsKey(theme))
                    storyThemes[theme] = 0;
                storyThemes[theme]++;
            }

            // רישום דמויות מעורבות
            foreach (var protagonist in story.protagonists)
            {
                var character = GetOrCreateCharacter(protagonist);
                character.storiesInvolved.Add(story.eventId);
            }

            OnStoryCreated?.Invoke(story);

            // בדיקת הפיכה לאגדה
            if (story.culturalImpact > legendThreshold)
            {
                MakeStoryLegendary(story);
            }

            if (debugMode)
                Debug.Log($"Created story: {story.eventName} (Impact: {story.culturalImpact:F1})");
        }

        private float CalculateCulturalImpact(StoryEvent story)
        {
            float impact = (int)story.importance * 10f;

            // בונוס לפי סוג סיפור
            switch (story.storyType)
            {
                case StoryType.Heroic:
                    impact *= 1.5f;
                    break;
                case StoryType.Tragic:
                    impact *= 1.3f;
                    break;
                case StoryType.Discovery:
                    impact *= 1.2f;
                    break;
                case StoryType.Legend:
                case StoryType.Myth:
                    impact *= 2f;
                    break;
            }

            // בונוס למספר דמויות מעורבות
            impact += (story.protagonists.Count + story.antagonists.Count) * 5f;

            // בונוס לעדים
            impact += story.witnesses.Count * 1f;

            return impact;
        }

        private void MakeStoryLegendary(StoryEvent story)
        {
            if (story.becameLegend) return;

            story.becameLegend = true;
            story.storyType = StoryType.Legend;
            story.culturalImpact *= 1.5f;

            OnStoryBecameLegend?.Invoke(story);

            // הפיכת דמויות לאגדיות
            foreach (var protagonist in story.protagonists)
            {
                var character = GetOrCreateCharacter(protagonist);
                if (!character.isLegendary)
                {
                    character.isLegendary = true;
                    character.fame += 50f;
                    OnCharacterBecameLegendary?.Invoke(character);
                }
            }

            if (debugMode)
                Debug.Log($"Story became legendary: {story.eventName}");
        }

        #endregion

        #region Character Management

        private Character GetOrCreateCharacter(MinipollBrain brain)
        {
            var character = characters.Find(c => c.brain == brain);
            if (character == null)
            {
                character = new Character(brain);
                characters.Add(character);

                // הוספת תכונות מגנטיקה
                var genetics = brain.GetComponent<MinipollGeneticsSystem>();
                if (genetics != null)
                {
                    character.AddTrait("Courage", genetics.GetGeneValue(GeneType.Strength) * 50f);
                    character.AddTrait("Wisdom", genetics.GetGeneValue(GeneType.Intelligence) * 50f);
                    character.AddTrait("Kindness", genetics.GetGeneValue(GeneType.Empathy) * 50f);
                    character.AddTrait("Leadership", genetics.GetGeneValue(GeneType.Charisma) * 50f);
                }
            }
            return character;
        }

        private void UpdateCharacters()
        {
            for (int i = characters.Count - 1; i >= 0; i--)
            {
                var character = characters[i];

                // הסרת דמויות שמתו
                if (character.brain == null || !character.brain.IsAlive)
                {
                    CreateDeathStory(character);
                    characters.RemoveAt(i);
                    continue;
                }

                // עדכון מוניטין
                UpdateCharacterReputation(character);
            }
        }

        private void CreateDeathStory(Character character)
        {
            var story = new StoryEvent(
                $"Death of {character.characterName}",
                $"{character.characterName} has passed away, leaving behind memories and legacy",
                character.isLegendary ? StoryType.Tragic : StoryType.Personal
            );

            story.importance = character.isLegendary ? StoryImportance.Epic : StoryImportance.Significant;
            story.themes.Add("Death");
            story.themes.Add("Legacy");
            story.moralLesson = "Even in death, one's deeds live on";

            // הוספת עדים (דמויות שהכירו)
            foreach (var otherChar in characters)
            {
                if (otherChar.brain != null && otherChar.brain.IsAlive)
                {
                    story.AddWitness(otherChar.brain);
                }
            }

            CreateStory(story);
        }

        private void UpdateCharacterReputation(Character character)
        {
            // עדכון מוניטין על בסיס פעולות אחרונות
            var skillSystem = character.brain.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null)
            {
                var topSkills = skillSystem.GetTopSkills(3);
                foreach (var skill in topSkills)
                {
                    switch (skill.skillType)
                    {
                        case SkillType.Combat:
                            character.AddTrait("Strength", 0.1f * skill.level);
                            break;
                        case SkillType.Medicine:
                            character.AddTrait("Wisdom", 0.1f * skill.level);
                            break;
                        case SkillType.Social:
                            character.AddTrait("Kindness", 0.1f * skill.level);
                            break;
                        case SkillType.Leadership:
                            character.AddTrait("Leadership", 0.1f * skill.level);
                            break;
                    }
                }
            }

            // עדכון כינויים
            var newEpithet = character.GenerateEpithet();
            if (!string.IsNullOrEmpty(newEpithet) && !character.epithets.Contains(newEpithet))
            {
                character.epithets.Add(newEpithet);
            }
        }

        #endregion

        #region Story Detection

        private void DetectAndCreateStories()
        {
            // זיהוי אירועים מעניינים שקרו לאחרונה
            DetectSkillMilestones();
            DetectSocialEvents();
            DetectSurvivalStories();
            DetectExplorationStories();
        }
        private void DetectSkillMilestones()
        {
            var allMinipoll = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None).Where(m => m.IsAlive);

            foreach (var brain in allMinipoll)
            {
                var skillSystem = brain.GetComponent<MinipollSkillSystem>();
                if (skillSystem == null) continue;

                var topSkills = skillSystem.GetTopSkills(1);
                if (topSkills.Count > 0)
                {
                    var topSkill = topSkills[0];

                    // יצירת סיפור על מיומנות גבוהה
                    if (topSkill.level >= 8 && UnityEngine.Random.value < 0.1f)
                    {
                        var story = new StoryEvent(
                            $"Mastery of {brain.name}",
                            $"{brain.name} has achieved great mastery in {topSkill.skillType}, becoming known for this talent",
                            StoryType.Personal
                        );

                        story.AddProtagonist(brain);
                        story.importance = StoryImportance.Significant;
                        story.themes.Add("Skill");
                        story.themes.Add("Mastery");

                        CreateStory(story);
                    }
                }
            }
        }
        private void DetectSocialEvents()
        {
            // זיהוי אירועים חברתיים מעניינים
            IEnumerable<MinipollBrain> allMinipoll = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None).Where(m => m.IsAlive);            foreach (MinipollBrain brain in allMinipoll)
            {
                object socialSystemObj = brain.GetSocialSystem();
                if (socialSystemObj == null) continue;

                // Cast the object to MinipollSocialRelations
                var socialSystem = socialSystemObj as MinipollGame.Social.MinipollSocialRelations;
                if (socialSystem == null) continue;                // חברויות חזקות
                foreach (var relationship in socialSystem.relationships)
                {
                    if (relationship.friendship > 90f && UnityEngine.Random.value < 0.05f)
                    {
                        var story = new StoryEvent(
                            $"Friendship of {brain.name} and {relationship.otherBrain.name}",
                            $"A deep friendship has formed between {brain.name} and {relationship.otherBrain.name}",
                            StoryType.Personal
                        );

                        story.AddProtagonist(brain);
                        story.AddProtagonist(relationship.otherBrain);
                        story.themes.Add("Friendship");
                        story.themes.Add("Loyalty");

                        CreateStory(story);
                    }
                }
            }
        }
        private void DetectSurvivalStories()
        {
            IEnumerable<MinipollBrain> allMinipoll = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None).Where(m => m.IsAlive);            foreach (var brain in allMinipoll)
            {                // הישרדות במצבים קשים
                if (brain.GetCore().Health.CurrentHealth < 20f && UnityEngine.Random.value < 0.2f)
                {
                    var story = new StoryEvent(
                        $"Survival of {brain.name}",
                        $"{brain.name} survived against all odds, showing remarkable resilience",
                        StoryType.Adventure
                    );

                    story.AddProtagonist(brain);
                    story.importance = StoryImportance.Significant;
                    story.themes.Add("Survival");
                    story.themes.Add("Resilience");

                    CreateStory(story);
                }
            }
        }
        private void DetectExplorationStories()
        {
            // סיפורי חקירה על בסיס תנועה
            var allMinipoll = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None).Where(m => m.IsAlive);

            foreach (var brain in allMinipoll)
            {
                var skillSystem = brain.GetComponent<MinipollSkillSystem>();
                if (skillSystem == null) continue;

                if (skillSystem.GetSkillLevel(SkillType.Navigation) >= 5 && UnityEngine.Random.value < 0.1f)
                {
                    var story = new StoryEvent(
                        $"Exploration by {brain.name}",
                        $"{brain.name} ventured into unknown territories, mapping new paths for others to follow",
                        StoryType.Adventure
                    );

                    story.AddProtagonist(brain);
                    story.themes.Add("Exploration");
                    story.themes.Add("Discovery");

                    CreateStory(story);
                }
            }
        }

        #endregion

        #region Storytelling and Oral Tradition
        private void ProcessStorytelling()
        {
            var allMinipoll = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None).Where(m => m.IsAlive).ToList();

            foreach (var storyteller in allMinipoll)
            {
                if (UnityEngine.Random.value < storytellingChance)
                {
                    TellStory(storyteller);
                }
            }
        }

        private void TellStory(MinipollBrain storyteller)
        {
            // מציאת מאזינים בסביבה
            var nearbyMinipoll = Physics.OverlapSphere(storyteller.transform.position, storytellingRadius)
                                       .Select(c => c.GetComponent<MinipollBrain>())
                                       .Where(m => m != null && m != storyteller && m.IsAlive)
                                       .ToList();

            if (nearbyMinipoll.Count == 0) return;

            // בחירת סיפור לספר
            var storyToTell = SelectStoryToTell(storyteller);
            if (storyToTell == null) return;

            // ספירת הסיפור
            storyToTell.timesRetold++;

            // עדכון פופולריות
            if (!storyPopularity.ContainsKey(storyToTell.eventId))
                storyPopularity[storyToTell.eventId] = 0;
            storyPopularity[storyToTell.eventId]++;            // השפעה על מאזינים
            foreach (var listener in nearbyMinipoll)
            {
                var needsSystemObj = listener.GetNeedsSystem();
                if (needsSystemObj != null)
                {
                    var needsSystem = needsSystemObj as MinipollGame.Core.MinipollNeedsSystem;
                    if (needsSystem != null)
                    {
                        needsSystem.social.Increase(10f);
                    }
                }
                  var emotionsSystemObj = listener.GetEmotionsSystem();
                if (emotionsSystemObj != null)
                {
                    var emotionsSystem = emotionsSystemObj as MinipollEmotionsSystem;
                    if (emotionsSystem != null)
                    {
                        emotionsSystem.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Happy, 5f);
                    }
                }
            }

            // נסיון לסופר
            var skillSystem = storyteller.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null)
            {
                skillSystem.GainExperience(SkillType.Social, 2f);
                skillSystem.GainExperience(SkillType.Art, 1f);
            }

            if (debugMode)
                Debug.Log($"{storyteller.name} told the story '{storyToTell.eventName}' to {nearbyMinipoll.Count} listeners");
        }

        private StoryEvent SelectStoryToTell(MinipollBrain storyteller)
        {
            // העדפה לסיפורים שהסופר מעורב בהם
            var character = characters.Find(c => c.brain == storyteller);
            if (character != null)
            {
                var involvedStories = allStories.Where(s => character.storiesInvolved.Contains(s.eventId)).ToList();
                if (involvedStories.Count > 0)
                {
                    return involvedStories[UnityEngine.Random.Range(0, involvedStories.Count)];
                }
            }

            // אחרת, סיפור פופולרי או חשוב
            var popularStories = allStories.Where(s => s.importance >= StoryImportance.Significant).ToList();
            if (popularStories.Count > 0)
            {
                return popularStories[UnityEngine.Random.Range(0, popularStories.Count)];
            }

            // ברירת מחדל - סיפור רנדומלי
            return allStories.Count > 0 ? allStories[UnityEngine.Random.Range(0, allStories.Count)] : null;
        }

        private void UpdateStoryPopularity()
        {
            // ירידה טבעית בפופולריות
            var keys = storyPopularity.Keys.ToList();
            foreach (var key in keys)
            {
                storyPopularity[key] = Mathf.Max(0, storyPopularity[key] - 1);
                if (storyPopularity[key] == 0)
                {
                    storyPopularity.Remove(key);
                }
            }
        }

        #endregion

        #region Saga Management

        private void UpdateSagas()
        {
            // יצירת סאגות מסיפורים קשורים
            foreach (var character in characters.Where(c => c.isLegendary && c.storiesInvolved.Count >= 3))
            {
                if (!sagas.Any(s => s.mainCharacters.Contains(character.characterName)))
                {
                    CreateCharacterSaga(character);
                }
            }

            // עדכון סאגות קיימות
            foreach (var saga in sagas)
            {
                if (!saga.isComplete && saga.eventIds.Count >= 5)
                {
                    CompleteSaga(saga);
                }
            }
        }

        private void CreateCharacterSaga(Character character)
        {
            var saga = new Saga(
                $"The Saga of {character.characterName}",
                $"The legendary tale of {character.characterName} and their great deeds"
            );

            saga.mainCharacters.Add(character.characterName);

            // הוספת כל הסיפורים של הדמות
            foreach (var storyId in character.storiesInvolved)
            {
                saga.AddEvent(storyId);
            }

            saga.theme = character.GetDominantTrait();
            saga.culturalSignificance = character.fame + character.heroicDeeds * 10f;

            sagas.Add(saga);

            if (debugMode)
                Debug.Log($"Created saga: {saga.sagaName}");
        }

        private void CompleteSaga(Saga saga)
        {
            saga.isComplete = true;
            saga.culturalSignificance *= 1.5f;

            OnSagaCompleted?.Invoke(saga);

            if (debugMode)
                Debug.Log($"Completed saga: {saga.sagaName}");
        }

        #endregion

        #region Public Interface

        public List<StoryEvent> GetAllStories()
        {
            return new List<StoryEvent>(allStories);
        }

        public List<StoryEvent> GetStoriesByType(StoryType type)
        {
            return allStories.Where(s => s.storyType == type).ToList();
        }

        public List<StoryEvent> GetStoriesByCharacter(MinipollBrain character)
        {
            return allStories.Where(s => s.protagonists.Contains(character) ||
                                       s.antagonists.Contains(character) ||
                                       s.witnesses.Contains(character)).ToList();
        }

        public List<StoryEvent> GetLegendaryStories()
        {
            return allStories.Where(s => s.becameLegend).ToList();
        }

        public List<Character> GetLegendaryCharacters()
        {
            return characters.Where(c => c.isLegendary).ToList();
        }

        public Character GetCharacterInfo(MinipollBrain brain)
        {
            return characters.Find(c => c.brain == brain);
        }

        public List<Saga> GetCompletedSagas()
        {
            return sagas.Where(s => s.isComplete).ToList();
        }

        public Dictionary<string, int> GetPopularThemes()
        {
            return new Dictionary<string, int>(storyThemes);
        }

        public string GenerateNarrativeReport()
        {
            var report = "=== NARRATIVE CHRONICLE ===\n\n";

            report += $"Total Stories Recorded: {allStories.Count}\n";
            report += $"Legendary Tales: {allStories.Count(s => s.becameLegend)}\n";
            report += $"Characters Documented: {characters.Count}\n";
            report += $"Legendary Figures: {characters.Count(c => c.isLegendary)}\n";
            report += $"Completed Sagas: {sagas.Count(s => s.isComplete)}\n\n";

            // נושאים פופולריים
            var topThemes = storyThemes.OrderByDescending(t => t.Value).Take(5);
            report += "Most Common Themes:\n";
            foreach (var theme in topThemes)
            {
                report += $"  {theme.Key}: {theme.Value} stories\n";
            }

            // דמויות אגדיות
            var legends = GetLegendaryCharacters();
            if (legends.Count > 0)
            {
                report += "\nLegendary Figures:\n";
                foreach (var legend in legends.Take(5))
                {
                    var epithets = string.Join(", ", legend.epithets);
                    report += $"  {legend.characterName}";
                    if (!string.IsNullOrEmpty(epithets)) report += $" ({epithets})";
                    report += $" - {legend.heroicDeeds} heroic deeds\n";
                }
            }

            // סיפורים מפורסמים
            var popularStories = allStories.Where(s => storyPopularity.ContainsKey(s.eventId))
                                          .OrderByDescending(s => storyPopularity[s.eventId])
                                          .Take(3);

            if (popularStories.Any())
            {
                report += "\nMost Retold Stories:\n";
                foreach (var story in popularStories)
                {
                    report += $"  '{story.eventName}' - told {story.timesRetold} times\n";
                }
            }

            return report;
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!showNarrativeUI || !debugMode) return;

            GUILayout.BeginArea(new Rect(Screen.width - 400, Screen.height - 200, 390, 190));
            GUILayout.Label("=== NARRATIVE SYSTEM ===");

            GUILayout.Label($"Stories: {allStories.Count}");
            GUILayout.Label($"Legends: {allStories.Count(s => s.becameLegend)}");
            GUILayout.Label($"Characters: {characters.Count}");
            GUILayout.Label($"Legendary Figures: {characters.Count(c => c.isLegendary)}");
            GUILayout.Label($"Active Sagas: {sagas.Count}");

            if (storyThemes.Count > 0)
            {
                var topTheme = storyThemes.OrderByDescending(t => t.Value).First();
                GUILayout.Label($"Top Theme: {topTheme.Key} ({topTheme.Value})");
            }

            if (GUILayout.Button("Generate Report"))
            {
                Debug.Log(GenerateNarrativeReport());
            }

            GUILayout.EndArea();
        }

        public void LogNarrativeState()
        {
            Debug.Log(GenerateNarrativeReport());
        }

        #endregion
    }
}