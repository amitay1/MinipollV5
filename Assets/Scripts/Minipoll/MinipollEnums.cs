/// <summary>
/// כל ה-Enums המשמשים במערכת Minipoll
/// קובץ מרכזי לכל סוגי ה-Enums בפרויקט
/// </summary>
using UnityEngine;

namespace MinipollGame.Systems.Core
{


// Basic Properties
public enum Gender
{
    Male,
    Female,
    Random
}

public enum AgeStage
{
    Baby,
    Child,
    Adult,
    Elder
}

// Needs System
public enum NeedType
{
    Hunger,
    Thirst,
    Sleep,
    Social,
    Fun,
    Hygiene,
    Comfort,
    Safety
}

// Emotions System
public enum EmotionType
{
    Neutral,
    Happy,
    Sad,
    Angry,
    Fear,
    Love,
    Tired,
    Surprised,
    Disgusted,
    Confused,
    Excited,
    Bored,
    Stressed,
    Relaxed,
    Proud,
    Ashamed,
    Jealous,
    Grateful
}

public enum MoodType
{
    Depressed,
    Sad,
    Neutral,
    Content,
    Happy,
    Ecstatic
}

// Social System
public enum InteractionType
{
    Friendly,
    Hostile,
    Romantic,
    Playful,
    Helpful,
    Trading,
    Teaching,
    Learning,
    Greeting,
    Farewell,
    Apologizing,
    Thanking,
    Complaining,
    Arguing,
    Fighting,
    Hugging,
    Kissing,
    Dancing,
    Playing,
    Working,
    Eating,
    Sleeping,
    Attacked,
    Helped,
    Fed,
    Healed,
    Taught,
    Entertained
}

public enum RelationshipType
{
    Stranger,
    Acquaintance,
    Friend,
    BestFriend,
    Rival,
    Enemy,
    Romantic,
    Partner,
    Family,
    Parent,
    Child,
    Sibling
}

public enum RelationshipLevel
{
    Hatred,     // -100 to -75
    Dislike,    // -75 to -25
    Neutral,    // -25 to 25
    Like,       // 25 to 75
    Love        // 75 to 100
}

// Tribe System
public enum TribeRole
{
    None,
    Leader,
    Elder,
    Warrior,
    Gatherer,
    Crafter,
    Healer,
    Scout,
    Teacher,
    Entertainer,
    Builder,
    Farmer,
    Merchant,
    Guard,
    Child
}

public enum TribeStatus
{
    Peaceful,
    Alert,
    Defensive,
    Aggressive,
    Celebrating,
    Mourning,
    Working,
    Resting
}

// Stats System
public enum StatType
{
    MaxHealth,
    Speed,
    Strength,
    Defense,
    Intelligence,
    Charisma,
    Luck,
    Fertility,
    Longevity,
    Creativity,
    Wisdom,
    Agility,
    Endurance,
    Perception
}

public enum ModifierType
{
    Flat,
    Percentage,
    Multiplier
}

public enum BuffType
{
    Strength,
    Speed,
    Intelligence,
    AllStats,
    Regeneration,
    Shield,
    Invisibility,
    Luck,
    Experience,
    Social
}

public enum DebuffType
{
    Weakness,
    Slowness,
    Confusion,
    Vulnerability,
    Poison,
    Burn,
    Freeze,
    Stun,
    Silence,
    Blind
}

// Health System
public enum DamageType
{
    Physical,
    Poison,
    Fire,
    Ice,
    Electric,
    Psychic,
    Bleed,
    Fall,
    Hunger,
    Disease,
    Magic,
    Environmental
}

public enum HealthState
{
    Healthy,
    Injured,
    Critical,
    Dying,
    Dead
}

// AI System
public enum AIState
{
    Idle,
    Wandering,
    Seeking,
    Fleeing,
    Fighting,
    Working,
    Socializing,
    Sleeping,
    Eating,
    Playing,
    Learning,
    Teaching,
    Building,
    Gathering,
    Hunting,
    Mating,
    Caring,
    Exploring,
    Resting,
    Celebrating,
    Mourning
}

public enum GoalPriority
{
    Critical,   // Must be done immediately
    High,       // Very important
    Medium,     // Normal priority
    Low,        // Can wait
    Optional    // Nice to have
}

public enum DecisionFactor
{
    Needs,
    Emotions,
    Social,
    Safety,
    Curiosity,
    Duty,
    Pleasure,
    Growth
}

// Memory System
public enum MemoryType
{
    Event,
    Location,
    Entity,
    Knowledge,
    Skill,
    Emotion,
    Relationship
}

public enum MemoryImportance
{
    Trivial,
    Minor,
    Normal,
    Important,
    Critical,
    Unforgettable
}

// Movement System
public enum MovementState
{
    Stationary,
    Walking,
    Running,
    Jumping,
    Falling,
    Swimming,
    Climbing,
    Crawling,
    Sliding,
    Flying
}

public enum MovementSpeed
{
    Immobile,
    VerySlow,
    Slow,
    Normal,
    Fast,
    VeryFast,
    Sprint
}

// World System
public enum WeatherType
{
    Clear,
    Cloudy,
    Rainy,
    Stormy,
    Snowy,
    Foggy,
    Windy,
    Hot,
    Cold
}

public enum TimeOfDay
{
    Dawn,
    Morning,
    Noon,
    Afternoon,
    Evening,
    Night,
    Midnight
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public enum BiomeType
{
    Forest,
    Desert,
    Mountain,
    Ocean,
    Plains,
    Swamp,
    Tundra,
    Jungle,
    Cave,
    Volcanic,
        Mountains,
        Coast
    }

// Resource System
public enum ResourceType
{
    Food,
    Water,
    Wood,
    Stone,
    Metal,
    Fiber,
    Medicine,
    Luxury,
    Tool,
    Weapon
}

public enum ItemQuality
{
    Broken,
    Poor,
    Common,
    Good,
    Excellent,
    Masterwork,
    Legendary
}

// Building System
public enum BuildingType
{
    House,
    Storage,
    Workshop,
    Farm,
    Well,
    Tower,
    Wall,
    Gate,
    Bridge,
    Monument
}

public enum BuildingState
{
    Blueprint,
    UnderConstruction,
    Complete,
    Damaged,
    Destroyed
}

// Economy System
public enum CurrencyType
{
    None,
    Barter,
    Coins,
    Gems,
    Favor,
    Knowledge
}

public enum TradeResult
{
    Success,
    Refused,
    CounterOffer,
    NoStock,
    CantAfford,
    NotInterested
}

// Disease System
public enum DiseaseType
{
    None,
    Cold,
    Flu,
    Plague,
    Parasite,
    Infection,
    Curse,
    Madness
}

public enum DiseaseStage
{
    Incubation,
    Onset,
    Active,
    Recovery,
    Cured,
    Chronic,
    Terminal
}

// Reproduction System
public enum PregnancyStage
{
    None,
    Early,
    Middle,
    Late,
    Labor
}

public enum GeneticTrait
{
    // Physical
    Size,
    Color,
    Pattern,
    EyeColor,
    // Mental
    Intelligence,
    Creativity,
    Memory,
    // Social
    Charisma,
    Empathy,
    Leadership,
    // Special
    Luck,
    Longevity,
    Fertility
}

// Skill System
public enum SkillType
{
    // Basic
    Movement,
    Communication,
    Observation,
    // Survival
    Foraging,
    Hunting,
    Shelter,
    // Social
    Leadership,
    Trading,
    Teaching,
    // Combat
    Fighting,
    Defense,
    Strategy,
    // Crafting
    Building,
    Toolmaking,
    Art,
    // Special
    Magic,
    Healing,
    Prophecy,
        Navigation,
        Gathering,
        Combat,
        Crafting,
        Medicine,
        Social
    }

public enum SkillLevel
{
    Untrained,
    Novice,
    Apprentice,
    Skilled,
    Expert,
    Master,
    Grandmaster
}

// Event System
public enum EventType
{
    Birth,
    Death,
    Marriage,
    Divorce,
    Fight,
    Trade,
    Discovery,
    Disaster,
    Celebration,
    War,
    Peace,
    Migration,
    Invention,
    Miracle,
    Curse
}

public enum EventScope
{
    Personal,
    Family,
    Tribe,
    Regional,
    Global
}

// Animation States
public enum AnimationState
{
    Idle,
    Walk,
    Run,
    Jump,
    Fall,
    Land,
    Attack,
    Defend,
    Eat,
    Drink,
    Sleep,
    WakeUp,
    Sit,
    Stand,
    Dance,
    Celebrate,
    Cry,
    Laugh,
    Talk,
    Listen,
    Work,
    Craft,
    Gather,
    Build,
    Die,
    Birth,
    Transform
}

// UI System
public enum UIPanel
{
    None,
    MainMenu,
    PauseMenu,
    Settings,
    Inventory,
    Stats,
    Relationships,
    Tribe,
    World,
    Help,
    Credits
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Achievement,
    Quest,
    Social,
    Combat,
    Resource,
    Building
}

// Save System
public enum SaveSlot
{
    Auto,
    Quick,
    Slot1,
    Slot2,
    Slot3,
    Slot4,
    Slot5,
    Cloud
}

public enum SaveState
{
    Empty,
    Saving,
    Loading,
    Corrupted,
    Valid
}

// Debug System
public enum DebugMode
{
    None,
    Basic,
    Detailed,
    Performance,
    AI,
    Physics,
    Network,
    All
}

public enum LogLevel
{
    Verbose,
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}
public enum GeneType
{
    // Physical Traits
    Size,
    Color,
    Pattern,
    BodyShape,
    EyeColor,
    EyeSize,
    
    // Stats
    HealthBonus,
    SpeedBonus,
    StrengthBonus,
    IntelligenceBonus,
    
    // Behavioral
    Aggression,
    Sociability,
    Curiosity,
    Fearfulness,
    
    // Special
    Longevity,
    Fertility,
    MutationRate,
    LuckBonus,
        Intelligence,
        Health,
        Strength,
        Speed,
        Endurance,
        Charisma,
        Empathy
    }

    // World Events
    public enum WorldEventType
    {
        // Environmental
        WeatherChange,
        SeasonChange,
        DayNightCycle,
        NaturalDisaster,

        // Population
        PopulationBoom,
        PopulationCrash,
        Migration,
        NewSpeciesDiscovered,

        // Resources
        ResourceAbundance,
        ResourceScarcity,
        NewResourceDiscovered,
        ResourceDepleted,

        // Social
        WarDeclared,
        PeaceAchieved,
        AllianceFormed,
        TribeFormed,
        TribeDisbanded,

        // Individual
        MinipollBorn,
        MinipollDied,
        MinipollEvolved,
        MinipollAchievement,

        // Special
        MiracleEvent,
        CurseEvent,
        Discovery,
        Catastrophe,
        Natural
    }
}