/*************************************************************
 *  MinipollResearchSystem.cs
 *  
 *  תיאור כללי:
 *    מערכת מחקר וטכנולוגיה:
 *      - עץ טכנולוגיות מתקדמות
 *      - מחקר קבוצתי ושיתופי
 *      - גילויים מדעיים ופריצות דרך
 *      - יישום טכנולוגיות בחיי היומיום
 *      - מעבר נתונים בין דורות
 *  
 *  דרישות קדם:
 *    - להניח על GameObject ריק בסצנה (ResearchCenter)
 *    - עבודה עם מערכות אחרות
 *************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using MinipollGame.Core;
using MinipollGame.Systems.Core;

[System.Serializable]
public enum ResearchField
{
    Medicine,           // רפואה
    Engineering,        // הנדסה
    Agriculture,        // חקלאות
    Architecture,       // אדריכלות
    Astronomy,          // אסטרונומיה
    Biology,            // ביולוגיה
    Chemistry,          // כימיה
    Physics,            // פיזיקה
    Philosophy,         // פילוסופיה
    Mathematics,        // מתמטיקה
    Metallurgy,         // מטלורגיה
    Navigation,         // ניווט
    Communications,     // תקשורת
    Psychology         // פסיכולוגיה
}

[System.Serializable]
public enum TechnologyTier
{
    Stone,              // תקופת האבן
    Bronze,             // תקופת הברונזה
    Iron,               // תקופת הברזל
    Classical,          // תקופה קלאסית
    Medieval,           // ימי הביניים
    Renaissance,        // רנסנס
    Industrial,         // מהפכה תעשייתית
    Modern,             // מודרני
    Advanced,           // מתקדם
    Futuristic         // עתידני
}

[System.Serializable]
public class Technology
{
    [Header("Technology Info")]
    public string technologyName;
    public string description;
    public ResearchField field;
    public TechnologyTier tier;
    
    [Header("Research Requirements")]
    public float requiredResearchPoints = 100f;
    public float currentResearchPoints = 0f;
    public List<string> prerequisiteTechs = new List<string>();
    public int minimumResearchers = 1;
    public int minimumSkillLevel = 1;
    
    [Header("Resources Required")]
    public Dictionary<string, int> requiredResources = new Dictionary<string, int>();
    public float costMultiplier = 1f;
    
    [Header("Benefits")]
    public List<TechnologyBenefit> benefits = new List<TechnologyBenefit>();
    public float efficiencyBonus = 0f;      // בונוס יעילות כללי
    public float healthBonus = 0f;          // בונוס בריאות
    public float happinessBonus = 0f;       // בונוס אושר
    
    [Header("Status")]
    public bool isUnlocked = false;
    public bool isResearched = false;
    public float discoveryTime = 0f;
    public List<MinipollBrain> researchers = new List<MinipollBrain>();
    public MinipollBrain leadResearcher = null;
    
    [Header("Cultural Impact")]
    public float culturalValue = 50f;
    public List<string> culturalChanges = new List<string>();
    public bool causesParadigmShift = false;
    
    public Technology(string name, ResearchField researchField, TechnologyTier techTier)
    {
        technologyName = name;
        field = researchField;
        tier = techTier;
        requiredResearchPoints = (int)techTier * 50f + UnityEngine.Random.Range(20f, 80f);
    }
    
    public bool CanBeResearched()
    {
        if (isResearched) return false;
        
        // בדיקת דרישות קדם
        foreach (var prereq in prerequisiteTechs)
        {
            if (MinipollResearchSystem.Instance != null)
            {
                var prereqTech = MinipollResearchSystem.Instance.GetTechnology(prereq);
                if (prereqTech == null || !prereqTech.isResearched)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    public float GetProgress()
    {
        return requiredResearchPoints > 0 ? currentResearchPoints / requiredResearchPoints : 1f;
    }
    
    public void AddResearchProgress(float points, MinipollBrain researcher)
    {
        if (isResearched || !CanBeResearched()) return;
        
        currentResearchPoints += points;
        
        if (!researchers.Contains(researcher))
        {
            researchers.Add(researcher);
        }
        
        // קביעת החוקר הראשי
        if (leadResearcher == null || UnityEngine.Random.value < 0.1f)
        {
            leadResearcher = researcher;
        }
        
        // בדיקת השלמה
        if (currentResearchPoints >= requiredResearchPoints)
        {
            CompleteResearch();
        }
    }
    
    private void CompleteResearch()
    {
        isResearched = true;
        discoveryTime = Time.time;
        
        // יישום יתרונות
        ApplyBenefits();
        
        // הוספת ערך תרבותי
        if (MinipollResearchSystem.Instance != null)
        {
            MinipollResearchSystem.Instance.AddCulturalValue(culturalValue);
        }
    }
    
    private void ApplyBenefits()
    {
        foreach (var benefit in benefits)
        {
            benefit.Apply();
        }
        
        // השפעות גלובליות
        if (efficiencyBonus > 0f)
        {
            ApplyGlobalEfficiencyBonus();
        }
    }
      private void ApplyGlobalEfficiencyBonus()
    {
        var allMinipoll = UnityEngine.Object.FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None);
        foreach (var brain in allMinipoll)
        {
            if (!brain.IsAlive) continue;
            
            var skillSystem = brain.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null)
            {
                // בונוס לכישור הרלוונטי
                SkillType relevantSkill = GetRelevantSkill();
                skillSystem.AddSkillBuff(relevantSkill, 1f + efficiencyBonus, 3600f, $"Technology: {technologyName}");
            }
        }
    }
      private SkillType GetRelevantSkill()
    {
        switch (field)
        {
            case ResearchField.Medicine: return SkillType.Medicine;
            case ResearchField.Engineering: return SkillType.Building;
            case ResearchField.Agriculture: return SkillType.Gathering;
            case ResearchField.Architecture: return SkillType.Building;
            case ResearchField.Biology: return SkillType.Medicine;
            default: return SkillType.Crafting;
        }
    }
}

[System.Serializable]
public class TechnologyBenefit
{
    public enum BenefitType
    {
        SkillBonus,         // בונוס לכישור
        HealthBonus,        // בונוס בריאות
        EfficiencyBonus,    // בונוס יעילות
        NewAbility,         // יכולת חדשה
        ResourceBonus,      // בונוס משאבים
        SocialBonus,        // בונוס חברתי
        EnvironmentalBonus  // בונוס סביבתי
    }
    
    public BenefitType benefitType;
    public string targetName;          // שם המטרה (כישור, משאב וכו')
    public float bonusValue;           // ערך הבונוס
    public string description;
    
    public TechnologyBenefit(BenefitType type, string target, float value, string desc)
    {
        benefitType = type;
        targetName = target;
        bonusValue = value;
        description = desc;
    }
    
    public void Apply()
    {
        // יישום הבונוס בהתאם לסוג
        switch (benefitType)
        {
            case BenefitType.SkillBonus:
                ApplySkillBonus();
                break;
            case BenefitType.HealthBonus:
                ApplyHealthBonus();
                break;
            case BenefitType.EfficiencyBonus:
                ApplyEfficiencyBonus();
                break;
            case BenefitType.NewAbility:
                ApplyNewAbility();
                break;
        }
    }
      private void ApplySkillBonus()
    {
        // יישום בונוס כישור לכל המיניפולים
        var allMinipoll = UnityEngine.Object.FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None);
        foreach (var brain in allMinipoll)
        {
            if (!brain.IsAlive) continue;
            
            var skillSystem = brain.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null && Enum.TryParse<SkillType>(targetName, out SkillType skill))
            {
                skillSystem.AddSkillBuff(skill, bonusValue, 3600f, "Technology Bonus");
            }
        }
    }    private void ApplyHealthBonus()
    {
        var allMinipoll = UnityEngine.Object.FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None);
        foreach (var brain in allMinipoll)
        {
            if (brain.IsAlive)
            {
                var healthComponent = brain.GetComponent<MinipollHealth>();
                if (healthComponent != null)
                {
                    healthComponent.Heal(bonusValue);
                    // Note: MaxHealth capping is handled internally by the Heal method
                }
            }
        }
    }
    
    private void ApplyEfficiencyBonus()
    {
        // בונוס יעילות ליצור או מערכות
        // מיושם במערכות הרלוונטיות
    }
    
    private void ApplyNewAbility()
    {
        // הוספת יכולת חדשה
        // תלוי במימוש ספציפי
    }
}

[System.Serializable]
public class ResearchProject
{
    [Header("Project Info")]
    public string projectName;
    public string description;
    public Technology targetTechnology;
    
    [Header("Team")]
    public MinipollBrain projectLeader;
    public List<MinipollBrain> researchers = new List<MinipollBrain>();
    public int maxResearchers = 5;
    
    [Header("Progress")]
    public float projectProgress = 0f;
    public float dailyProgress = 0f;
    public float startTime;
    public float estimatedCompletion;
    
    [Header("Resources")]
    public Dictionary<string, int> resourcesUsed = new Dictionary<string, int>();
    public float fundingRequired = 0f;
    public float currentFunding = 0f;
    
    [Header("Collaboration")]
    public bool isCollaborative = false;
    public List<MinipollBrain> externalCollaborators = new List<MinipollBrain>();
    public float knowledgeSharing = 0f;

    public SkillType SkillType { get; private set; }

    public ResearchProject(Technology tech, MinipollBrain leader)
    {
        targetTechnology = tech;
        projectLeader = leader;
        projectName = $"Research: {tech.technologyName}";
        description = $"Research project to develop {tech.technologyName}";
        startTime = Time.time;
        
        if (!researchers.Contains(leader))
        {
            researchers.Add(leader);
        }
    }
    
    public bool AddResearcher(MinipollBrain researcher)
    {
        if (researchers.Count >= maxResearchers) return false;
        if (researchers.Contains(researcher)) return false;
        
        researchers.Add(researcher);
        return true;
    }
    
    public void RemoveResearcher(MinipollBrain researcher)
    {
        researchers.Remove(researcher);
        
        // אם המנהיג עזב, בחר חדש
        if (projectLeader == researcher && researchers.Count > 0)
        {
            projectLeader = researchers[0];
        }
    }
    
    public float CalculateProgressRate()
    {
        if (researchers.Count == 0) return 0f;
        
        float baseRate = 1f;
        
        // בונוס מכישורים של החוקרים
        float skillBonus = 0f;
        foreach (var researcher in researchers)
        {
            var skillSystem = researcher.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null)
            {
                skillBonus += skillSystem.GetSkillLevel(SkillType.Medicine) * 0.1f; // מדע
            }
        }
        
        // בונוס מחקר קבוצתי
        float teamBonus = 1f + (researchers.Count - 1) * 0.2f;
        
        // בונוס מאינטליגנציה ממוצעת
        float intelligenceBonus = 1f;
        foreach (var researcher in researchers)
        {
            var genetics = researcher.GetComponent<MinipollGeneticsSystem>();
            if (genetics != null)
            {
                intelligenceBonus += genetics.GetGeneValue(GeneType.Intelligence) * 0.5f;
            }
        }
        intelligenceBonus /= researchers.Count;
        
        return baseRate * (1f + skillBonus) * teamBonus * intelligenceBonus;
    }
    
    public bool IsStalled()
    {
        return dailyProgress < 0.1f && Time.time - startTime > 300f; // 5 דקות ללא התקדמות
    }
}

public class MinipollResearchSystem : MonoBehaviour
{
    [Header("Research Settings")]
    public bool enableResearch = true;
    public float baseResearchRate = 1f;
    public float collaborationBonus = 0.5f;
    public int maxActiveProjects = 10;
    
    [Header("Technology Database")]
    public List<Technology> allTechnologies = new List<Technology>();
    public Dictionary<ResearchField, List<Technology>> technologiesByField = new Dictionary<ResearchField, List<Technology>>();
    
    [Header("Active Research")]
    public List<ResearchProject> activeProjects = new List<ResearchProject>();
    public Dictionary<MinipollBrain, ResearchProject> researcherAssignments = new Dictionary<MinipollBrain, ResearchProject>();
    
    [Header("Research Centers")]
    public List<Vector3> researchCenters = new List<Vector3>();
    public float researchCenterRadius = 10f;
    public float researchCenterBonus = 2f;
    
    [Header("Knowledge Sharing")]
    public bool enableKnowledgeSharing = true;
    public float knowledgeSharingRange = 15f;
    public float knowledgeDecayRate = 0.01f;
    public Dictionary<ResearchField, float> globalKnowledge = new Dictionary<ResearchField, float>();
    
    [Header("Cultural Progress")]
    public float totalCulturalValue = 0f;
    public TechnologyTier currentTechLevel = TechnologyTier.Stone;
    public Dictionary<TechnologyTier, int> technologiesPerTier = new Dictionary<TechnologyTier, int>();
    
    [Header("Innovation")]
    public float innovationRate = 0.02f;      // סיכוי לחדשנות
    public List<string> recentDiscoveries = new List<string>();
    public float breakthroughThreshold = 500f; // סף לפריצת דרך
    
    [Header("Debug")]
    public bool debugMode = false;
    public bool showResearchUI = true;
    
    // Singleton
    public static MinipollResearchSystem Instance { get; private set; }
    
    // Events
    public event Action<Technology> OnTechnologyDiscovered;
    public event Action<ResearchProject> OnProjectStarted;
    public event Action<ResearchProject> OnProjectCompleted;
    public event Action<TechnologyTier> OnTechLevelAdvanced;
    public event Action<string> OnBreakthroughAchieved;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeResearchSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableResearch)
        {
            StartCoroutine(ResearchCoroutine());
        }
    }
    
    private void InitializeResearchSystem()
    {
        CreateTechnologyTree();
        InitializeGlobalKnowledge();
        InitializeTechTiers();
        
        if (debugMode)
            Debug.Log("Research System initialized with " + allTechnologies.Count + " technologies");
    }

    private void InitializeGlobalKnowledge()
    {
        throw new NotImplementedException();
    }

    private void InitializeTechTiers()
    {
        throw new NotImplementedException();
    }

    private System.Collections.IEnumerator ResearchCoroutine()
    {
        while (enableResearch)
        {
            yield return new WaitForSeconds(10f); // עדכון כל 10 שניות
            
            UpdateActiveProjects();
            ProcessKnowledgeSharing();
            CheckForInnovations();
            UpdateTechLevel();
            ManageAutomaticResearch();
        }
    }

    private void CheckForInnovations()
    {
        throw new NotImplementedException();
    }

    private void ManageAutomaticResearch()
    {
        throw new NotImplementedException();
    }

    private void UpdateTechLevel()
    {
        throw new NotImplementedException();
    }

    private void ProcessKnowledgeSharing()
    {
        throw new NotImplementedException();
    }

    #region Technology Tree Creation

    private void CreateTechnologyTree()
    {
        CreateStoneTechnologies();
        CreateBronzeTechnologies();
        CreateIronTechnologies();
        CreateClassicalTechnologies();
        CreateMedievalTechnologies();
        CreateRenaissanceTechnologies();
        
        OrganizeTechnologiesByField();
    }
    
    private void CreateStoneTechnologies()
    {
        // כלים בסיסיים
        var stoneTools = new Technology("Stone Tools", ResearchField.Engineering, TechnologyTier.Stone);
        stoneTools.description = "Basic tools made from stone for hunting and gathering";
        stoneTools.requiredResearchPoints = 25f;
        stoneTools.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Gathering", 0.2f, "Improved gathering efficiency"));
        stoneTools.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Hunting", 0.15f, "Better hunting tools"));
        allTechnologies.Add(stoneTools);
        
        // שליטה באש
        var fireControl = new Technology("Fire Control", ResearchField.Engineering, TechnologyTier.Stone);
        fireControl.description = "The ability to create and control fire";
        fireControl.requiredResearchPoints = 30f;
        fireControl.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Cooking", 0.3f, "Enables cooking"));
        fireControl.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.HealthBonus, "Health", 10f, "Cooked food prevents disease"));
        fireControl.culturalChanges.Add("Enables cooking and food preservation");
        fireControl.culturalChanges.Add("Provides warmth and protection");
        allTechnologies.Add(fireControl);
        
        // רפואה בסיסית
        var herbalMedicine = new Technology("Herbal Medicine", ResearchField.Medicine, TechnologyTier.Stone);
        herbalMedicine.description = "Using plants and herbs for healing";
        herbalMedicine.requiredResearchPoints = 40f;
        herbalMedicine.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Medicine", 0.25f, "Basic healing knowledge"));
        herbalMedicine.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.HealthBonus, "Health", 5f, "Natural remedies"));
        allTechnologies.Add(herbalMedicine);
        
        // חקלאות בסיסית
        var basicAgriculture = new Technology("Basic Agriculture", ResearchField.Agriculture, TechnologyTier.Stone);
        basicAgriculture.description = "Growing crops and domesticating plants";
        basicAgriculture.requiredResearchPoints = 50f;
        basicAgriculture.prerequisiteTechs.Add("Stone Tools");
        basicAgriculture.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Farming", 0.4f, "Crop cultivation"));
        basicAgriculture.causesParadigmShift = true;
        basicAgriculture.culturalValue = 100f;
        allTechnologies.Add(basicAgriculture);
    }
    
    private void CreateBronzeTechnologies()
    {
        // עבודת מתכות
        var bronzeWorking = new Technology("Bronze Working", ResearchField.Metallurgy, TechnologyTier.Bronze);
        bronzeWorking.description = "The art of working with bronze metals";
        bronzeWorking.requiredResearchPoints = 80f;
        bronzeWorking.prerequisiteTechs.Add("Fire Control");
        bronzeWorking.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Crafting", 0.3f, "Metal crafting"));
        bronzeWorking.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.NewAbility, "Bronze Tools", 1f, "Can create bronze tools"));
        allTechnologies.Add(bronzeWorking);
        
        // כתיבה
        var writing = new Technology("Writing System", ResearchField.Communications, TechnologyTier.Bronze);
        writing.description = "A system for recording information using symbols";
        writing.requiredResearchPoints = 100f;
        writing.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Teaching", 0.5f, "Knowledge preservation"));
        writing.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SocialBonus, "Culture", 0.3f, "Cultural development"));
        writing.causesParadigmShift = true;
        writing.culturalValue = 150f;
        allTechnologies.Add(writing);
        
        // אדריכלות בסיסית
        var stoneBuilding = new Technology("Stone Architecture", ResearchField.Architecture, TechnologyTier.Bronze);
        stoneBuilding.description = "Building permanent structures from stone";
        stoneBuilding.requiredResearchPoints = 90f;
        stoneBuilding.prerequisiteTechs.Add("Stone Tools");
        stoneBuilding.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Building", 0.4f, "Advanced construction"));
        stoneBuilding.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SocialBonus, "Safety", 0.2f, "Permanent shelter"));
        allTechnologies.Add(stoneBuilding);
    }
    
    private void CreateIronTechnologies()
    {
        // עבודת ברזל
        var ironWorking = new Technology("Iron Working", ResearchField.Metallurgy, TechnologyTier.Iron);
        ironWorking.description = "The technology to work with iron, stronger than bronze";
        ironWorking.requiredResearchPoints = 120f;
        ironWorking.prerequisiteTechs.Add("Bronze Working");
        ironWorking.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Combat", 0.3f, "Superior weapons"));
        ironWorking.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Building", 0.25f, "Better tools"));
        allTechnologies.Add(ironWorking);
        
        // הנדסה
        var engineering = new Technology("Basic Engineering", ResearchField.Engineering, TechnologyTier.Iron);
        engineering.description = "Understanding of mechanical principles and construction";
        engineering.requiredResearchPoints = 100f;
        engineering.prerequisiteTechs.Add("Stone Architecture");
        engineering.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Building", 0.5f, "Engineering principles"));
        engineering.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.NewAbility, "Bridges", 1f, "Can build bridges"));
        allTechnologies.Add(engineering);
        
        // רפואה מתקדמת
        var advancedMedicine = new Technology("Advanced Medicine", ResearchField.Medicine, TechnologyTier.Iron);
        advancedMedicine.description = "Systematic approach to healing and anatomy";
        advancedMedicine.requiredResearchPoints = 110f;
        advancedMedicine.prerequisiteTechs.Add("Herbal Medicine");
        advancedMedicine.prerequisiteTechs.Add("Writing System");
        advancedMedicine.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Medicine", 0.6f, "Medical knowledge"));
        advancedMedicine.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.HealthBonus, "Health", 15f, "Better healing"));
        allTechnologies.Add(advancedMedicine);
    }
    
    private void CreateClassicalTechnologies()
    {
        // מתמטיקה
        var mathematics = new Technology("Mathematics", ResearchField.Mathematics, TechnologyTier.Classical);
        mathematics.description = "Advanced mathematical concepts and calculations";
        mathematics.requiredResearchPoints = 150f;
        mathematics.prerequisiteTechs.Add("Writing System");
        mathematics.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Teaching", 0.4f, "Logical thinking"));
        mathematics.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Building", 0.3f, "Precise construction"));
        mathematics.causesParadigmShift = true;
        allTechnologies.Add(mathematics);
        
        // פילוסופיה
        var philosophy = new Technology("Philosophy", ResearchField.Philosophy, TechnologyTier.Classical);
        philosophy.description = "Systematic thinking about existence and ethics";
        philosophy.requiredResearchPoints = 140f;
        philosophy.prerequisiteTechs.Add("Writing System");
        philosophy.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SocialBonus, "Wisdom", 0.5f, "Deeper understanding"));
        philosophy.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SocialBonus, "Leadership", 0.3f, "Better governance"));
        philosophy.culturalValue = 200f;
        allTechnologies.Add(philosophy);
        
        // אסטרונומיה
        var astronomy = new Technology("Astronomy", ResearchField.Astronomy, TechnologyTier.Classical);
        astronomy.description = "Study of celestial bodies and their movements";
        astronomy.requiredResearchPoints = 130f;
        astronomy.prerequisiteTechs.Add("Mathematics");
        astronomy.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Navigation", 0.6f, "Stellar navigation"));
        astronomy.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.NewAbility, "Calendar", 1f, "Accurate timekeeping"));
        allTechnologies.Add(astronomy);
    }
    
    private void CreateMedievalTechnologies()
    {
        // הדפסה
        var printing = new Technology("Printing Press", ResearchField.Communications, TechnologyTier.Medieval);
        printing.description = "Mechanical device for mass production of text";
        printing.requiredResearchPoints = 200f;
        printing.prerequisiteTechs.Add("Basic Engineering");
        printing.prerequisiteTechs.Add("Writing System");
        printing.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Teaching", 1f, "Mass education"));
        printing.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SocialBonus, "Knowledge", 0.8f, "Information spread"));
        printing.causesParadigmShift = true;
        printing.culturalValue = 250f;
        allTechnologies.Add(printing);
        
        // כימיה
        var alchemy = new Technology("Alchemy", ResearchField.Chemistry, TechnologyTier.Medieval);
        alchemy.description = "Early chemical knowledge and experimentation";
        alchemy.requiredResearchPoints = 180f;
        alchemy.prerequisiteTechs.Add("Advanced Medicine");
        alchemy.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Medicine", 0.4f, "Chemical compounds"));
        alchemy.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.NewAbility, "Potions", 1f, "Medicinal compounds"));
        allTechnologies.Add(alchemy);
    }
    
    private void CreateRenaissanceTechnologies()
    {
        // מדע ניסויי
        var scientificMethod = new Technology("Scientific Method", ResearchField.Philosophy, TechnologyTier.Renaissance);
        scientificMethod.description = "Systematic approach to understanding the natural world";
        scientificMethod.requiredResearchPoints = 300f;
        scientificMethod.prerequisiteTechs.Add("Mathematics");
        scientificMethod.prerequisiteTechs.Add("Philosophy");
        scientificMethod.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.EfficiencyBonus, "Research", 1f, "Faster research"));
        scientificMethod.causesParadigmShift = true;
        scientificMethod.culturalValue = 400f;
        allTechnologies.Add(scientificMethod);
        
        // אופטיקה
        var optics = new Technology("Optics", ResearchField.Physics, TechnologyTier.Renaissance);
        optics.description = "Understanding of light and vision";
        optics.requiredResearchPoints = 250f;
        optics.prerequisiteTechs.Add("Mathematics");
        optics.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.NewAbility, "Telescope", 1f, "See distant objects"));
        optics.benefits.Add(new TechnologyBenefit(TechnologyBenefit.BenefitType.SkillBonus, "Astronomy", 0.5f, "Better observation"));
        allTechnologies.Add(optics);
    }
    
    private void OrganizeTechnologiesByField()
    {
        technologiesByField.Clear();
        
        foreach (var tech in allTechnologies)
        {
            if (!technologiesByField.ContainsKey(tech.field))
            {
                technologiesByField[tech.field] = new List<Technology>();
            }
            technologiesByField[tech.field].Add(tech);
        }
    }
    
    #endregion
    
    #region Research Management
    
    public bool StartResearchProject(Technology technology, MinipollBrain leader)
    {
        if (!technology.CanBeResearched()) return false;
        if (activeProjects.Count >= maxActiveProjects) return false;
        if (researcherAssignments.ContainsKey(leader)) return false;
        
        var project = new ResearchProject(technology, leader);
        activeProjects.Add(project);
        researcherAssignments[leader] = project;
        
        OnProjectStarted?.Invoke(project);
        
        if (debugMode)
            Debug.Log($"{leader.name} started research project: {technology.technologyName}");
            
        return true;
    }
    
    public bool JoinResearchProject(ResearchProject project, MinipollBrain researcher)
    {
        if (researcherAssignments.ContainsKey(researcher)) return false;
        if (!project.AddResearcher(researcher)) return false;
        
        researcherAssignments[researcher] = project;
        
        if (debugMode)
            Debug.Log($"{researcher.name} joined research project: {project.projectName}");
            
        return true;
    }
    
    public void LeaveResearchProject(MinipollBrain researcher)
    {
        if (!researcherAssignments.ContainsKey(researcher)) return;
        
        var project = researcherAssignments[researcher];
        project.RemoveResearcher(researcher);
        researcherAssignments.Remove(researcher);
        
        // אם הפרויקט נשאר ללא חוקרים, סגור אותו
        if (project.researchers.Count == 0)
        {
            activeProjects.Remove(project);
            
            if (debugMode)
                Debug.Log($"Research project abandoned: {project.projectName}");
        }
    }
    
    private void UpdateActiveProjects()
    {        for (int i = activeProjects.Count - 1; i >= 0; i--)
        {
            var project = activeProjects[i];
            
            // הסרת חוקרים שמתו
            for (int j = project.researchers.Count - 1; j >= 0; j--)        {
                // Add cleanup logic here if needed
            }
        }
    }
      #endregion
    
    #region Public Interface
    
    public Technology GetTechnology(string technologyName)
    {
        return allTechnologies.FirstOrDefault(tech => tech.technologyName == technologyName);
    }
    
    public void AddCulturalValue(float value)
    {
        totalCulturalValue += value;
        
        if (debugMode)
            Debug.Log($"Cultural value increased by {value}. Total: {totalCulturalValue}");
    }
    
    #endregion
}
