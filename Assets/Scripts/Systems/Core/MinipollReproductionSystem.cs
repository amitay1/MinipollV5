// /***************************************************************
//  *  MinipollReproductionSystem.cs
//  *
//  *  תיאור כללי:
//  *    מודול המטפל ברבייה של המיניפול:
//  *      - בדיקת תנאים (בריאות נאותה, Hunger גבוה מספיק וכו’)
//  *      - חיפוש פרט מתאים להזדווגות (אם הוא "מוכן" גם)
//  *      - הפקת צאצא במיקומם
//  *      - ממשק מול MinipollGeneticsSystem כדי להעביר/לשלב תכונות
//  *
//  *  דרישות קדם:
//  *    - למקם על המיניפול (יחד עם MinipollBrain, וכו’).
//  *    - MinipollBrain יקרא UpdateReproduction(deltaTime), או נעשה selfUpdate=true אם נרצה.
//  *    - דרוש MinipollManager כדי ליצור את הצאצא (SpawnMinipoll).
//  *
//  ***************************************************************/

// using UnityEngine;
// using System;
// using MinipollCore;

// public class MinipollReproductionSystem : MonoBehaviour
// {
//     private static MinipollReproductionSystem _instance;
//     public static MinipollReproductionSystem Instance => _instance;

//     private MinipollBrain brain;

//     // Events
//     public static event Action<MinipollBrain> OnOffspringBorn;
//     public static event Action<MinipollBrain, MinipollBrain> OnMatingStart;

//     [Header("Basic Settings")]
//     [Tooltip("אם false, היצור לא יתרבה")]
//     public bool canReproduce = true;

//     [Tooltip("זמן המתנה בין לידות (שניות)")]
//     public float cooldownTime = 60f; // דקה בין "לידות"
//     private float cooldownTimer = 0f;

//     [Tooltip("אנרגיה/רעב מינימליים כדי להרשות רבייה")]
//     public float minEnergyToReproduce = 70f;
//     public float minHungerToReproduce = 50f;

//     [Tooltip("מחסור במשאבים לאחר לידה (למשל -30 Hunger, -40 Energy)")]
//     public float postBirthHungerCost = 30f;
//     public float postBirthEnergyCost = 40f;

//     [Tooltip("רדיוס לחיפוש בני זוג")]
//     public float mateSearchRadius = 5f;

//     [Tooltip("רדיוס קרבה סופי להזדווגות")]
//     public float mateProximityThreshold = 1.5f;

//     [Header("Self Update")]
//     public bool selfUpdate = false;
//     internal float birthTime;    private void Awake()
//     {
//         // Singleton initialization
//         if (_instance == null)
//         {
//             _instance = this;
//         }
//         else if (_instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
        
//         brain = GetComponent<MinipollBrain>();
//     }

//     private void Update()
//     {
//         if (selfUpdate)
//         {
//             UpdateReproduction(Time.deltaTime);
//         }
//     }

//     public void UpdateReproduction(float deltaTime)
//     {
//         if (!canReproduce) return;

//         // עדכון cooldown
//         if (cooldownTimer > 0f)
//         {
//             cooldownTimer -= deltaTime;
//             return;
//         }

//         // בדיקת תנאי בסיס: האם יש מספיק אנרגיה/רעב?
//         var needs = brain.GetNeedsSystem();
//         if (needs == null) return;

//         if (needs.energy.currentValue < minEnergyToReproduce) return;
//         if (needs.hunger.currentValue < minHungerToReproduce) return;

//         // חיפוש פרט אחר שמוכן גם (באותו רדיוס)
//         MinipollBrain partner = FindMate();
//         if (partner != null)
//         {
//             // בודקים מרחק
//             float dist = Vector3.Distance(transform.position, partner.transform.position);
//             if (dist > mateProximityThreshold)
//             {
//                 // להתקרב אליו (למשל)
//                 brain.GetMovementController()?.StartSeekingResource(partner.transform.position);
//             }
//             else
//             {
//                 // מספיק קרובים => בצעו "הזדווגות" => יצירת צאצא
//                 PerformMating(partner);
//             }
//         }
//         else
//         {
//             // אין בן זוג => אפשר לשוטט או לחפש
//             // או לא לעשות כלום
//         }
//     }

//     private MinipollBrain FindMate()
//     {
//         // נגיד שנחפש Collider ברדיוס
//         Collider[] hits = Physics.OverlapSphere(transform.position, mateSearchRadius);
//         foreach (var c in hits)
//         {
//             if (c.gameObject == this.gameObject) continue;
//             var otherBrain = c.GetComponent<MinipollBrain>();
//             if (otherBrain != null && otherBrain != brain)
//             {
//                 // נבדוק אם גם אצלו ניתן להתרבות, אנרגיה מספיקה, cooldown אפס וכו’
//                 var repSys = otherBrain.GetComponent<MinipollReproductionSystem>();
//                 var needs = otherBrain.GetNeedsSystem();
//                 if (repSys != null && needs != null)
//                 {
//                     if (repSys.canReproduce && repSys.cooldownTimer <= 0f &&
//                         needs.energy.currentValue >= repSys.minEnergyToReproduce &&
//                         needs.hunger.currentValue >= repSys.minHungerToReproduce)
//                     {
//                         // מצאנו בן זוג פוטנציאלי
//                         return otherBrain;
//                     }
//                 }
//             }
//         }
//         return null;
//     }    private void PerformMating(MinipollBrain partner)
//     {
//         // Trigger mating start event
//         OnMatingStart?.Invoke(brain, partner);

//         // ניצור צאצא דרך MinipollManager
//         var manager = MinipollManager.Instance;
//         if (manager == null) return;        // קובעים מיקום הלידה בין 2 ההורים
//         Vector3 spawnPos = (transform.position + partner.transform.position) * 0.5f;
//         var babyCore = manager.SpawnMinipoll(spawnPos);
//         var baby = babyCore != null ? babyCore.GetComponent<MinipollBrain>() : null;
//         if (baby != null)
//             OnOffspringBorn?.Invoke(baby);

//         // כעת נעביר תכונות תורשתיות (דרך MinipollGeneticsSystem, אם קיים)
//         var genSysSelf = GetComponent<MinipollGeneticsSystem>();
//         var genSysMate = partner.GetComponent<MinipollGeneticsSystem>();
//         var genSysBaby = baby.GetComponent<MinipollGeneticsSystem>();

//         if (genSysSelf != null && genSysMate != null && genSysBaby != null)
//         {
//             genSysBaby.InheritFrom(genSysSelf, genSysMate);
//         }

//         // הורדת משאבים להורים
//         var selfNeeds = brain.GetNeedsSystem();
//         selfNeeds.hunger.currentValue = Mathf.Max(0f, selfNeeds.hunger.currentValue - postBirthHungerCost);
//         selfNeeds.energy.currentValue = Mathf.Max(0f, selfNeeds.energy.currentValue - postBirthEnergyCost);

//         var partnerNeeds = partner.GetNeedsSystem();
//         partnerNeeds.hunger.currentValue = Mathf.Max(0f, partnerNeeds.hunger.currentValue - postBirthHungerCost);
//         partnerNeeds.energy.currentValue = Mathf.Max(0f, partnerNeeds.energy.currentValue - postBirthEnergyCost);

//         // הפעל cooldown
//         cooldownTimer = cooldownTime;
//         var repSys = partner.GetComponent<MinipollReproductionSystem>();
//         if (repSys != null)
//         {
//             repSys.cooldownTimer = repSys.cooldownTime;
//         }
//     }

//     /// <summary>
//     /// Attempts breeding between two minipolls - called from MinipollCore
//     /// </summary>
//     public void AttemptBreeding(MinipollGame.Core.MinipollCore minipoll1, MinipollGame.Core.MinipollCore minipoll2)
//     {
//         if (minipoll1 == null || minipoll2 == null) return;
        
//         var brain1 = minipoll1.GetComponent<MinipollBrain>();
//         var brain2 = minipoll2.GetComponent<MinipollBrain>();
        
//         if (brain1 != null && brain2 != null)
//         {
//             PerformMating(brain2); // Use brain2 as partner for brain1
//         }
//     }
// }
