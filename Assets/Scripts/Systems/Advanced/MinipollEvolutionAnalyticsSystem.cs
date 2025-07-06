/*************************************************************
 *  MinipollEvolutionAnalyticsSystem.cs
 *  
 *  תיאור כללי:
 *    מערכת אנליטיקה והתפתחות אבולוציונית:
 *      - מעקב אחר שינויים גנטיים לאורך זמן
 *      - ניתוח מגמות אבולוציוניות
 *      - זיהוי הסתגלויות מוצלחות
 *      - דוחות מדעיים על האוכלוסייה
 *      - ניבוי כיוונים עתידיים
 *  
 *  דרישות קדם:
 *    - להניח על GameObject ריק בסצנה (EvolutionAnalytics)
 *    - עבודה עם מערכות גנטיקה ואוכלוסייה
 *************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using MinipollCore;
using MinipollGame.Core;
using MinipollGame.Systems.Core; // Added for extension methods
// Note: MinipollExtensions are now available through MinipollGame.Systems.Advanced namespace
// Assuming GeneType, MinipollBrain, MinipollGeneticsSystem, MinipollReproductionSystem, 
// MinipollSkillSystem, BiomeType, MinipollStreamingWorldSystem, MinipollWorldEventSystem, 
// WorldEventType, MinipollDiseaseSystem, MinipollBattleSystem, MinipollNeedsSystem 
// and other related classes/enums (like Chunk, WorldEvent) are defined elsewhere in the project.
namespace MinipollGame.Systems.Advanced
{
    [System.Serializable]
    public enum EvolutionTrend
    {
        Increasing,     // עולה
        Decreasing,     // יורד
        Stable,         // יציב
        Oscillating,    // מתנדנד
        Branching      // מתפצל
    }

    [System.Serializable]
    public class GeneticDataPoint
    {
        public float timestamp;
        public GeneType geneType;
        public float averageValue;
        public float minValue;
        public float maxValue;
        public float standardDeviation;
        public int sampleSize;
        public int generation;

        public GeneticDataPoint(float time, GeneType gene, List<float> values, int gen)
        {
            timestamp = time;
            geneType = gene;
            generation = gen;
            sampleSize = values.Count;

            if (values.Count > 0)
            {
                averageValue = values.Average();
                minValue = values.Min();
                maxValue = values.Max();

                // חישוב סטיית תקן
                float variance = values.Select(x => (x - averageValue) * (x - averageValue)).Average();
                standardDeviation = Mathf.Sqrt(variance);
            }
        }
    }

    [System.Serializable]
    public class PopulationSnapshot
    {
        public float timestamp;
        public int totalPopulation;
        public int alivePopulation;
        public float averageAge;
        public float averageFitness;
        public float averageHealth;
        public int currentGeneration;
        public Dictionary<GeneType, float> geneAverages = new Dictionary<GeneType, float>();
        public Dictionary<string, int> skillLevelDistribution = new Dictionary<string, int>();
        public Dictionary<BiomeType, int> biomeDistribution = new Dictionary<BiomeType, int>();

        public PopulationSnapshot()
        {
            timestamp = Time.time;
        }
    }

    [System.Serializable]
    public class EvolutionaryPressure
    {
        public string pressureName;
        public float intensity;             // עוצמת לחץ
        public float duration;              // משך הלחץ
        public GeneType affectedGene;       // גן מושפע
        public float selectionCoefficient;  // מקדם סלקציה
        public string description;
        public bool isActive;

        public EvolutionaryPressure(string name, GeneType gene, float intense, string desc)
        {
            pressureName = name;
            affectedGene = gene;
            intensity = intense;
            description = desc;
            isActive = true;
            duration = 0f;
        }
    }

    [System.Serializable]
    public class AdaptationEvent
    {
        public float timestamp;
        public string adaptationName;
        public GeneType primaryGene;
        public float changeAmount;          // כמות השינוי
        public int affectedPopulation;      // כמה פרטים הושפעו
        public string trigger;              // מה גרם להסתגלות
        public float fitnessImprovement;    // שיפור כשירות

        public AdaptationEvent(string name, GeneType gene, float change, int population, string trig)
        {
            timestamp = Time.time;
            adaptationName = name;
            primaryGene = gene;
            changeAmount = change;
            affectedPopulation = population;
            trigger = trig;
        }
    }

    public class MinipollEvolutionAnalyticsSystem : MonoBehaviour
    {
        [Header("Analytics Settings")]
        public bool enableAnalytics = true;
        public float dataCollectionInterval = 60f;     // כל דקה
        public float snapshotInterval = 300f;          // תמונת מצב כל 5 דקות
        public int maxDataPoints = 1000;              // מקסימום נקודות מידע
        public int maxSnapshots = 200;                // מקסימום תמונות מצב

        [Header("Data Storage")]
        public List<GeneticDataPoint> geneticHistory = new List<GeneticDataPoint>();
        public List<PopulationSnapshot> populationHistory = new List<PopulationSnapshot>();
        public List<AdaptationEvent> adaptationEvents = new List<AdaptationEvent>();
        public List<EvolutionaryPressure> activePressures = new List<EvolutionaryPressure>();

        [Header("Current Analysis")]
        public Dictionary<GeneType, EvolutionTrend> currentTrends = new Dictionary<GeneType, EvolutionTrend>();
        public float overallDiversityIndex = 0f;      // מדד גיוון
        public float populationStability = 0f;        // יציבות אוכלוסייה
        public int dominantGeneration = 0;             // דור דומיננטי

        [Header("Predictions")]
        public Dictionary<GeneType, float> predictedChanges = new Dictionary<GeneType, float>();
        public float extinctionRisk = 0f;             // סיכון להכחדה
        public List<string> recommendedInterventions = new List<string>();

        [Header("Research Goals")]
        public List<string> activeResearchQuestions = new List<string>();
        public Dictionary<string, float> researchProgress = new Dictionary<string, float>();

        [Header("Debug")]
        public bool debugMode = false;
        public bool showAnalyticsUI = true;

        // Private timers
        private float dataTimer = 0f;
        private float snapshotTimer = 0f;
        private float analysisTimer = 0f;
        private object m;

        // Singleton
        public static MinipollEvolutionAnalyticsSystem Instance { get; private set; }

        // Events
        public event Action<AdaptationEvent> OnAdaptationDetected;
        public event Action<EvolutionaryPressure> OnPressureDetected;
        public event Action<PopulationSnapshot> OnPopulationSnapshot;
        public event Action<string> OnResearchCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeResearchQuestions();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (enableAnalytics)
            {
                StartCoroutine(AnalyticsCoroutine());

                if (debugMode)
                    Debug.Log("Evolution Analytics System started");
            }
        }

        private System.Collections.IEnumerator AnalyticsCoroutine()
        {
            while (enableAnalytics)
            {
                yield return new WaitForSeconds(1f);

                dataTimer += 1f;
                snapshotTimer += 1f;
                analysisTimer += 1f;

                // איסוף נתונים גנטיים
                if (dataTimer >= dataCollectionInterval)
                {
                    dataTimer = 0f;
                    CollectGeneticData();
                }

                // תמונת מצב אוכלוסייה
                if (snapshotTimer >= snapshotInterval)
                {
                    snapshotTimer = 0f;
                    TakePopulationSnapshot();
                }

                // ניתוח מגמות
                if (analysisTimer >= dataCollectionInterval * 2f)
                {
                    analysisTimer = 0f;
                    AnalyzeTrends();
                    DetectAdaptations();
                    UpdatePressures();
                    GeneratePredictions();
                }
            }
        }

        #region Data Collection
        private void CollectGeneticData()
        {
            var allMinipoll = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None);
            var geneticSystems = new List<MinipollGeneticsSystem>();

            foreach (var brain in allMinipoll)
            {
                if (!brain.IsAlive) continue;
                var genetics = brain.GetComponent<MinipollGeneticsSystem>();
                if (genetics != null)
                {
                    geneticSystems.Add(genetics);
                }
            }

            if (geneticSystems.Count == 0) return;

            // איסוף נתונים לכל סוג גן
            foreach (GeneType geneType in Enum.GetValues(typeof(GeneType)))
            {
                var values = new List<float>();
                int totalGeneration = 0;

                foreach (var genetics in geneticSystems)
                {
                    values.Add(genetics.GetGeneValue(geneType));
                    totalGeneration += genetics.GetGeneration();
                }

                int avgGeneration = totalGeneration / geneticSystems.Count;
                var dataPoint = new GeneticDataPoint(Time.time, geneType, values, avgGeneration);

                geneticHistory.Add(dataPoint);
            }

            // ניקוי נתונים ישנים
            if (geneticHistory.Count > maxDataPoints)
            {
                int toRemove = geneticHistory.Count - maxDataPoints;
                geneticHistory.RemoveRange(0, toRemove);
            }

            if (debugMode)
                Debug.Log($"Collected genetic data from {geneticSystems.Count} specimens");
        }

        private void TakePopulationSnapshot()
        {
            var snapshot = new PopulationSnapshot();
            var allMinipoll = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None);
            var aliveMinipoll = allMinipoll.Where(m => m.IsAlive).ToList();

            snapshot.totalPopulation = allMinipoll.Length;
            snapshot.alivePopulation = aliveMinipoll.Count;

            if (aliveMinipoll.Count == 0)
            {
                populationHistory.Add(snapshot);
                return;            }

            // חישוב נתונים סטטיסטיים
            // Note: MinipollReproductionSystem is currently disabled, using basic calculations
            
            if (aliveMinipoll.Count > 0)
            {
                // Calculate average age based on time alive (simplified approach)
                snapshot.averageAge = aliveMinipoll.Average(m => Time.time); // Simplified age calculation
                
                snapshot.currentGeneration = aliveMinipoll.Max(m =>
                {
                    var genetics = m.GetComponent<MinipollGeneticsSystem>();
                    return genetics != null ? genetics.GetGeneration() : 0;
                });
            }

            List<MinipollGeneticsSystem> geneticSystems = aliveMinipoll.Select(m => m.GetComponent<MinipollGeneticsSystem>())
                                             .Where(g => g != null).ToList();

            if (geneticSystems.Count > 0)
            {
                snapshot.averageFitness = geneticSystems.Average(g => g.GetOverallFitness());

                // ממוצעי גנים
                foreach (GeneType geneType in Enum.GetValues(typeof(GeneType)))
                {
                    float avgValue = geneticSystems.Average(g => g.GetGeneValue(geneType));
                    snapshot.geneAverages[geneType] = avgValue;
                }
            }

            // בריאות ממוצעת
            // snapshot.averageHealth = aliveMinipoll.Average(m => m.Health);

            // התפלגות כישורים
            CollectSkillDistribution(aliveMinipoll, snapshot);

            // התפלגות ביומים
            CollectBiomeDistribution(aliveMinipoll, snapshot);

            populationHistory.Add(snapshot);
            OnPopulationSnapshot?.Invoke(snapshot);

            // ניקוי היסטוריה ישנה
            if (populationHistory.Count > maxSnapshots)
            {
                populationHistory.RemoveAt(0);
            }

            if (debugMode)
                Debug.Log($"Population snapshot: {snapshot.alivePopulation} alive, avg fitness: {snapshot.averageFitness:F2}");
        }

        private void CollectSkillDistribution(List<MinipollBrain> minipoll, PopulationSnapshot snapshot)
        {
            var skillCounts = new Dictionary<string, int>();

            foreach (var brain in minipoll)
            {
                var skillSystem = brain.GetComponent<MinipollSkillSystem>();
                if (skillSystem == null) continue;

                var topSkills = skillSystem.GetTopSkills(3);
                foreach (var skill in topSkills)
                {
                    string skillKey = $"{skill.skillType}_L{skill.level}";
                    if (!skillCounts.ContainsKey(skillKey))
                        skillCounts[skillKey] = 0;
                    skillCounts[skillKey]++;
                }
            }

            snapshot.skillLevelDistribution = skillCounts;
        }

        private void CollectBiomeDistribution(List<MinipollBrain> minipoll, PopulationSnapshot snapshot)
        {
            var biomeCounts = new Dictionary<BiomeType, int>();
            foreach (var brain in minipoll)
            {
                if (MinipollStreamingWorldSystem.Instance != null)
                {
                    var chunk = MinipollStreamingWorldSystem.Instance.GetChunkAt(brain.transform.position) as Chunk;
                    if (chunk != null)
                    {
                        if (!biomeCounts.ContainsKey(chunk.biome))
                            biomeCounts[chunk.biome] = 0;
                        biomeCounts[chunk.biome]++;
                    }
                }
                else
                {
                    if (debugMode) Debug.LogWarning("MinipollStreamingWorldSystem.Instance is null. Cannot collect biome distribution.");
                }
            }

            snapshot.biomeDistribution = biomeCounts;
        }

        #endregion

        #region Trend Analysis

        private void AnalyzeTrends()
        {
            if (geneticHistory.Count < 10) return; // צריך מספיק נתונים

            foreach (GeneType geneType in Enum.GetValues(typeof(GeneType)))
            {
                var recentData = geneticHistory.Where(d => d.geneType == geneType)
                                              .OrderBy(d => d.timestamp)
                                              .TakeLast(20)
                                              .ToList();

                if (recentData.Count < 5) continue;

                EvolutionTrend trend = CalculateTrend(recentData);
                currentTrends[geneType] = trend;
            }

            // חישוב מדד גיוון
            CalculateDiversityIndex();

            // חישוב יציבות אוכלוסייה
            CalculatePopulationStability();
        }

        private EvolutionTrend CalculateTrend(List<GeneticDataPoint> data)
        {
            if (data.Count < 3) return EvolutionTrend.Stable;

            var values = data.Select(d => d.averageValue).ToArray();
            float slope = CalculateLinearSlope(values);
            float variance = CalculateVariance(values);

            // בדיקת מגמה
            if (Mathf.Abs(slope) > 0.05f)
            {
                return slope > 0 ? EvolutionTrend.Increasing : EvolutionTrend.Decreasing;
            }
            else if (variance > 0.1f)
            {
                return EvolutionTrend.Oscillating;
            }
            else
            {
                return EvolutionTrend.Stable;
            }
        }

        private float CalculateLinearSlope(float[] values)
        {
            int n = values.Length;
            float sumX = 0f, sumY = 0f, sumXY = 0f, sumX2 = 0f;

            for (int i = 0; i < n; i++)
            {
                sumX += i;
                sumY += values[i];
                sumXY += i * values[i];
                sumX2 += i * i;
            }

            return (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        }

        private float CalculateVariance(float[] values)
        {
            float mean = values.Average();
            return values.Select(v => (v - mean) * (v - mean)).Average();
        }

        private void CalculateDiversityIndex()
        {
            if (populationHistory.Count == 0) return;

            var latestSnapshot = populationHistory.Last();
            float totalVariance = 0f;
            int geneCount = 0;

            foreach (var geneAvg in latestSnapshot.geneAverages)
            {
                var recentData = geneticHistory.Where(d => d.geneType == geneAvg.Key)
                                              .TakeLast(5)
                                              .ToList();

                if (recentData.Count > 0)
                {
                    float avgStdDev = recentData.Average(d => d.standardDeviation);
                    totalVariance += avgStdDev;
                    geneCount++;
                }
            }

            overallDiversityIndex = geneCount > 0 ? totalVariance / geneCount : 0f;
        }

        private void CalculatePopulationStability()
        {
            if (populationHistory.Count < 5) return;

            var recentSnapshots = populationHistory.TakeLast(5).ToList();
            var populationSizes = recentSnapshots.Select(s => (float)s.alivePopulation).ToArray();

            float variance = CalculateVariance(populationSizes);
            float mean = populationSizes.Average();

            // יציבות = 1 - (סטיית תקן יחסית)
            populationStability = mean > 0 ? 1f - (Mathf.Sqrt(variance) / mean) : 0f;
            populationStability = Mathf.Clamp01(populationStability);
        }

        #endregion

        #region Adaptation Detection

        private void DetectAdaptations()
        {
            foreach (GeneType geneType in Enum.GetValues(typeof(GeneType)))
            {
                DetectGeneAdaptation(geneType);
            }
        }

        private void DetectGeneAdaptation(GeneType geneType)
        {
            var recentData = geneticHistory.Where(d => d.geneType == geneType)
                                          .OrderBy(d => d.timestamp)
                                          .TakeLast(10)
                                          .ToList();

            if (recentData.Count < 5) return;

            float startValue = recentData.First().averageValue;
            float endValue = recentData.Last().averageValue;
            float change = endValue - startValue;

            // זיהוי שינוי משמעותי
            if (Mathf.Abs(change) > 0.15f) // שינוי של 15% או יותר
            {
                string adaptationName = $"{geneType} {(change > 0 ? "Enhancement" : "Reduction")}";

                // בדיקה אם זה אירוע חדש
                bool isNewAdaptation = !adaptationEvents.Any(a =>
                    a.primaryGene == geneType &&
                    Time.time - a.timestamp < 300f); // לא היה אירוע דומה ב-5 דקות האחרונות

                if (isNewAdaptation)
                {
                    var adaptation = new AdaptationEvent(
                        adaptationName,
                        geneType,
                        change,
                        recentData.Last().sampleSize,
                        DetermineAdaptationTrigger(geneType, change)
                    );

                    adaptation.fitnessImprovement = EstimateFitnessImpact(geneType, change);

                    adaptationEvents.Add(adaptation);
                    OnAdaptationDetected?.Invoke(adaptation);

                    if (debugMode)
                        Debug.Log($"Adaptation detected: {adaptationName} (Change: {change:F3})");
                }
            }
        }

        private string DetermineAdaptationTrigger(GeneType geneType, float change)
        {
            // זיהוי מה עלול לגרום להסתגלות
            if (MinipollWorldEventSystem.Instance != null)
            {
                var activeEvents = MinipollWorldEventSystem.Instance.GetActiveEvents();
                if (activeEvents.Count > 0)
                {
                    // Ensure activeEvents.First() is safe if GetActiveEvents() can return empty or null elements
                    var firstEvent = activeEvents.FirstOrDefault(e => e != null);
                    if (firstEvent != null)
                    {
                        return $"Response to {firstEvent.eventName}";
                    }
                }
            }
            else
            {
                if (debugMode) Debug.LogWarning("MinipollWorldEventSystem.Instance is null. Cannot determine adaptation trigger from world events.");
            }

            // נחש על בסיס סוג הגן
            switch (geneType)
            {
                case GeneType.Health:
                    return "Disease pressure or environmental challenge";
                case GeneType.Strength:
                    return "Increased competition or combat pressure";
                case GeneType.Intelligence:
                    return "Complex environmental challenges";
                case GeneType.Speed:
                    return "Predation pressure or resource competition";
                default:
                    return "Unknown selective pressure";
            }
        }

        private float EstimateFitnessImpact(GeneType geneType, float change)
        {
            // הערכת השפעה על הכשירות
            float impact = Mathf.Abs(change) * 0.1f;

            // גנים מסוימים חשובים יותר
            switch (geneType)
            {
                case GeneType.Health:
                case GeneType.Fertility:
                    impact *= 1.5f;
                    break;
                case GeneType.Intelligence:
                case GeneType.Strength:
                    impact *= 1.2f;
                    break;
            }

            return change > 0 ? impact : -impact;
        }

        #endregion

        #region Evolutionary Pressures

        private void UpdatePressures()
        {
            // עדכון לחצים קיימים
            for (int i = activePressures.Count - 1; i >= 0; i--)
            {
                var pressure = activePressures[i];
                pressure.duration += dataCollectionInterval;

                // הסרת לחצים שפג תוקפם
                if (pressure.duration > 600f) // 10 דקות
                {
                    pressure.isActive = false;
                    activePressures.RemoveAt(i);
                }
            }

            // זיהוי לחצים חדשים
            DetectNewPressures();
        }

        private void DetectNewPressures()
        {
            // לחץ ממחלות
            DetectDiseasePressure();

            // לחץ מאירועי עולם
            DetectEnvironmentalPressure();

            // לחץ מתחרות על משאבים
            DetectResourcePressure();

            // לחץ ממלחמות
            DetectCombatPressure();
        }

        private void DetectResourcePressure()
        {
            throw new NotImplementedException();
        }

        private void DetectDiseasePressure()
        {
            var allDiseaseSystems = FindObjectsByType<MinipollDiseaseSystem>(FindObjectsSortMode.None);
            int sickIndividuals = allDiseaseSystems.Count(d => d.GetActiveDiseases().Count > 0);
            float diseaseRate = allDiseaseSystems.Length > 0 ? (float)sickIndividuals / allDiseaseSystems.Length : 0f;

            if (diseaseRate > 0.3f) // יותר מ-30% חולים
            {
                var pressure = new EvolutionaryPressure(
                    "Disease Outbreak",
                    GeneType.Health,
                    diseaseRate,
                    "High disease prevalence creating selection pressure for health resistance"
                );

                AddPressure(pressure);
            }
        }

        private void DetectEnvironmentalPressure()
        {
            if (MinipollWorldEventSystem.Instance == null)
            {
                if (debugMode) Debug.LogWarning("MinipollWorldEventSystem.Instance is null. Cannot detect environmental pressure.");
                return;
            }

            var activeEvents = MinipollWorldEventSystem.Instance.GetActiveEvents();
            foreach (var worldEvent in activeEvents)
            {
                if (worldEvent.eventType == WorldEventType.Natural && worldEvent.intensity > 1.2f)
                {
                    var pressure = new EvolutionaryPressure(
                        $"Environmental Challenge: {worldEvent.eventName}",
                        GeneType.Endurance,
                        worldEvent.intensity,
                        $"Harsh environmental conditions from {worldEvent.eventName}"
                    );

                    AddPressure(pressure);
                }
            }
        }
        // private void DetectResourcePressure()
        // {
        //     // IEnumerable<MinipollBrain> allMinipoll = FindObjectsByType<MinipollBrain>(FindObjectsSortMode.None).Where(m => m.IsAlive);
        //     // var hungryMinipoll = allMinipoll.Where(static m =>
        //     {
        //         // var needs = m.GetComponent<MinipollNeedsSystem>();
        //         // return needs != null && needs.hunger.currentValue < 40f;
        //     });

        //     // float hungryRatio = allMinipoll.Count() > 0 ? (float)hungryMinipoll.Count() / allMinipoll.Count() : 0f;

        //     if (hungryRatio > 0.4f) // יותר מ-40% רעבים
        //     {
        //         var pressure = new EvolutionaryPressure(
        //             "Resource Scarcity",
        //             GeneType.Intelligence,
        //             hungryRatio,
        //             "Limited food resources creating pressure for better foraging abilities"
        //         );

        //         AddPressure(pressure);
        //     }
        // }
        private void DetectCombatPressure()
        {
            var allBattleSystems = FindObjectsByType<MinipollBattleSystem>(FindObjectsSortMode.None);
            int fightingIndividuals = allBattleSystems.Count(b => b.IsInCombat());
            float combatRate = allBattleSystems.Length > 0 ? (float)fightingIndividuals / allBattleSystems.Length : 0f;

            if (combatRate > 0.2f) // יותר מ-20% בקרב
            {
                var pressure = new EvolutionaryPressure(
                    "High Combat Activity",
                    GeneType.Strength,
                    combatRate,
                    "Frequent combat creating selection pressure for combat abilities"
                );

                AddPressure(pressure);
            }
        }

        private void AddPressure(EvolutionaryPressure pressure)
        {
            // בדיקה שאין כבר לחץ דומה
            bool exists = activePressures.Any(p => p.pressureName == pressure.pressureName && p.isActive);

            if (!exists)
            {
                activePressures.Add(pressure);
                OnPressureDetected?.Invoke(pressure);

                if (debugMode)
                    Debug.Log($"New evolutionary pressure detected: {pressure.pressureName}");
            }
        }

        #endregion

        #region Predictions and Research

        private void GeneratePredictions()
        {
            predictedChanges.Clear();

            // ניבוי שינויים גנטיים על בסיס מגמות נוכחיות
            foreach (var trend in currentTrends)
            {
                float predictedChange = PredictGeneticChange(trend.Key, trend.Value);
                predictedChanges[trend.Key] = predictedChange;
            }

            // חישוב סיכון הכחדה
            CalculateExtinctionRisk();

            // יצירת המלצות
            GenerateRecommendations();

            // עדכון מחקרים
            UpdateResearchProgress();
        }

        private float PredictGeneticChange(GeneType geneType, EvolutionTrend trend)
        {
            var recentData = geneticHistory.Where(d => d.geneType == geneType)
                                          .OrderBy(d => d.timestamp)
                                          .TakeLast(10)
                                          .ToList();

            if (recentData.Count < 3) return 0f;

            var values = recentData.Select(d => d.averageValue).ToArray();
            float slope = CalculateLinearSlope(values);

            // ניבוי לעוד 5 מדידות קדימה
            float prediction = slope * 5f;

            // התאמה לפי לחצים פעילים
            var relevantPressures = activePressures.Where(p => p.affectedGene == geneType && p.isActive);
            foreach (var pressure in relevantPressures)
            {
                prediction += pressure.intensity * 0.1f;
            }

            return Mathf.Clamp(prediction, -0.5f, 0.5f);
        }

        private void CalculateExtinctionRisk()
        {
            extinctionRisk = 0f;

            if (populationHistory.Count < 3) return;

            var recentSnapshots = populationHistory.TakeLast(5).ToList();

            // בדיקת ירידה באוכלוסייה
            float populationTrend = (recentSnapshots.Last().alivePopulation - recentSnapshots.First().alivePopulation) /
                                   (float)recentSnapshots.First().alivePopulation;

            if (populationTrend < -0.2f) extinctionRisk += 0.3f; // ירידה של 20%+

            // בדיקת גיוון גנטי נמוך
            if (overallDiversityIndex < 0.1f) extinctionRisk += 0.4f;

            // בדיקת כשירות נמוכה
            float avgFitness = recentSnapshots.Average(s => s.averageFitness);
            if (avgFitness < 0.3f) extinctionRisk += 0.3f;

            extinctionRisk = Mathf.Clamp01(extinctionRisk);
        }

        private void GenerateRecommendations()
        {
            recommendedInterventions.Clear();

            if (extinctionRisk > 0.6f)
            {
                recommendedInterventions.Add("URGENT: Population at high extinction risk - consider intervention");
            }

            if (overallDiversityIndex < 0.15f)
            {
                recommendedInterventions.Add("Low genetic diversity - introduce new breeding programs");
            }

            if (populationStability < 0.3f)
            {
                recommendedInterventions.Add("Population unstable - address environmental pressures");
            }

            foreach (var pressure in activePressures.Where(p => p.intensity > 0.8f))
            {
                recommendedInterventions.Add($"High pressure from {pressure.pressureName} - targeted relief needed");
            }
        }

        private void UpdateResearchProgress()
        {
            foreach (var question in activeResearchQuestions)
            {
                if (!researchProgress.ContainsKey(question))
                    researchProgress[question] = 0f;

                // התקדמות מחקר על בסיס איסוף נתונים
                researchProgress[question] += 1f / 100f; // 1% בכל עדכון

                if (researchProgress[question] >= 1f)
                {
                    CompleteResearch(question);
                }
            }
        }

        private void CompleteResearch(string question)
        {
            researchProgress[question] = 1f;
            OnResearchCompleted?.Invoke(question);

            if (debugMode)
                Debug.Log($"Research completed: {question}");
        }

        private void InitializeResearchQuestions()
        {
            activeResearchQuestions.Add("How does environmental stress affect genetic diversity?");
            activeResearchQuestions.Add("What factors contribute to successful adaptation?");
            activeResearchQuestions.Add("How do social structures influence evolution?");
            activeResearchQuestions.Add("What is the optimal population size for stability?");
            activeResearchQuestions.Add("How quickly do beneficial mutations spread?");
        }

        #endregion

        #region Public Interface

        public PopulationSnapshot GetLatestSnapshot()
        {
            return populationHistory.Count > 0 ? populationHistory.Last() : null;
        }

        public List<GeneticDataPoint> GetGeneHistory(GeneType geneType)
        {
            return geneticHistory.Where(d => d.geneType == geneType).OrderBy(d => d.timestamp).ToList();
        }

        public EvolutionTrend GetGeneTrend(GeneType geneType)
        {
            return currentTrends.ContainsKey(geneType) ? currentTrends[geneType] : EvolutionTrend.Stable;
        }

        public float GetDiversityIndex()
        {
            return overallDiversityIndex;
        }

        public float GetExtinctionRisk()
        {
            return extinctionRisk;
        }

        public List<EvolutionaryPressure> GetActivePressures()
        {
            return activePressures.Where(p => p.isActive).ToList();
        }

        public List<AdaptationEvent> GetRecentAdaptations(int count = 10)
        {
            return adaptationEvents.OrderByDescending(a => a.timestamp).Take(count).ToList();
        }

        public string GenerateEvolutionReport()
        {
            var report = "=== EVOLUTION ANALYTICS REPORT ===\n\n";

            var latest = GetLatestSnapshot();
            if (latest != null)
            {
                report += $"Population Status:\n";
                report += $"  Alive: {latest.alivePopulation}\n";
                report += $"  Average Age: {latest.averageAge:F1}\n";
                report += $"  Average Fitness: {latest.averageFitness:F2}\n";
                report += $"  Current Generation: {latest.currentGeneration}\n\n";
            }

            report += $"Genetic Diversity: {overallDiversityIndex:F3}\n";
            report += $"Population Stability: {populationStability:F3}\n";
            report += $"Extinction Risk: {extinctionRisk:F3}\n\n";

            if (adaptationEvents.Count > 0)
            {
                report += "Recent Adaptations:\n";
                var recentAdaptations = GetRecentAdaptations(5);
                foreach (var adaptation in recentAdaptations)
                {
                    report += $"  {adaptation.adaptationName}: {adaptation.changeAmount:F3} ({adaptation.trigger})\n";
                }
                report += "\n";
            }

            if (activePressures.Count > 0)
            {
                report += "Active Evolutionary Pressures:\n";
                foreach (var pressure in activePressures.Where(p => p.isActive))
                {
                    report += $"  {pressure.pressureName}: {pressure.intensity:F2} intensity\n";
                }
                report += "\n";
            }

            if (recommendedInterventions.Count > 0)
            {
                report += "Recommendations:\n";
                foreach (var recommendation in recommendedInterventions)
                {
                    report += $"  • {recommendation}\n";
                }
            }

            return report;
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!showAnalyticsUI || !debugMode) return;

            GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 340, 300));
            GUILayout.Label("=== EVOLUTION ANALYTICS ===");

            var latest = GetLatestSnapshot();
            if (latest != null)
            {
                GUILayout.Label($"Population: {latest.alivePopulation}");
                GUILayout.Label($"Generation: {latest.currentGeneration}");
                GUILayout.Label($"Avg Fitness: {latest.averageFitness:F2}");
            }

            GUILayout.Label($"Diversity: {overallDiversityIndex:F3}");
            GUILayout.Label($"Stability: {populationStability:F3}");
            GUILayout.Label($"Extinction Risk: {extinctionRisk:F3}");

            GUILayout.Label($"Active Pressures: {activePressures.Count(p => p.isActive)}");
            GUILayout.Label($"Recent Adaptations: {adaptationEvents.Count}");

            if (GUILayout.Button("Generate Report"))
            {
                Debug.Log(GenerateEvolutionReport());
            }

            GUILayout.EndArea();
        }

        public void LogAnalyticsState()
        {
            Debug.Log(GenerateEvolutionReport());
        }

        #endregion
    }
}