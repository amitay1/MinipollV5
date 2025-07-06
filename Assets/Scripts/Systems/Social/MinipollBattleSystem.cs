/*************************************************************
 *  MinipollBattleSystem.cs
 *  
 *  תיאור כללי:
 *    מערכת קרב ולחימה:
 *      - קרבות יחיד נגד יחיד
 *      - קרבות קבוצתיים
 *      - כלי נשק וציוד
 *      - טקטיקות וכישורי לחימה
 *      - פציעות והחלמה
 *      - מורל וגבורה
 *  
 *  דרישות קדם:
 *    - להניח על אובייקט עם MinipollBrain
 *    - עבודה עם מערכות אחרות
 *************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using MinipollCore;
using Unity.VisualScripting;
using MinipollGame.Social;
using MinipollGame.Systems.Core;
using MinipollGame.Core;


[System.Serializable]
public enum CombatStance
{
    Peaceful,       // שלום - לא נלחם
    Defensive,      // הגנתי - רק הגנה עצמית
    Aggressive,     // תוקפני - מחפש קרבות
    Berserker,      // פראי - לוחם ללא מחשבה
    Tactical       // טקטי - מתוכנן
}

[System.Serializable]
public enum WeaponType
{
    None,           // אין נשק
    Fists,          // אגרופים
    Club,           // אלה
    Spear,          // חנית
    Sword,          // חרב
    Bow,            // קשת
    Shield,         // מגן
    Throwing       // זריקה
}

[System.Serializable]
public enum AttackType
{
    Melee,          // קרב צמוד
    Ranged,         // רחוק
    Magic,          // קסם
    Defensive,      // הגנתי
    Grapple,        // היאבקות
    Charge,         // מתקפת הסתערות
    Ambush         // מארב
}

[System.Serializable]
public class CombatStats
{
    [Header("Combat Abilities")]
    public float strength = 50f;           // כוח
    public float agility = 50f;            // זריזות
    public float endurance = 50f;          // סיבולת
    public float accuracy = 50f;           // דיוק
    public float defense = 50f;            // הגנה
    
    [Header("Combat Experience")]
    public int combatLevel = 1;            // רמת לחימה
    public float combatExperience = 0f;    // נסיון קרב
    public int victories = 0;              // ניצחונות
    public int defeats = 0;                // תבוסות
    public int kills = 0;                  // הרג
    
    [Header("Current State")]
    public float stamina = 100f;           // סיבולת נוכחית
    public float maxStamina = 100f;        // סיבולת מקסימלית
    public float morale = 50f;             // מורל
    public float fear = 0f;                // פחד
    public bool isExhausted = false;       // מותש
    
    [Header("Injuries")]
    public List<Injury> activeInjuries = new List<Injury>();
    public float totalInjurySeverity = 0f;
    
    public void UpdateCombatLevel()
    {
        int newLevel = Mathf.FloorToInt(combatExperience / 100f) + 1;
        if (newLevel > combatLevel)
        {
            combatLevel = newLevel;
            // שיפור יכולות עם עלייה ברמה
            strength += 2f;
            agility += 1f;
            defense += 1f;
        }
    }
    
    public float GetWinRate()
    {
        int totalFights = victories + defeats;
        return totalFights > 0 ? (float)victories / totalFights : 0f;
    }
}

[System.Serializable]
public class Weapon
{
    [Header("Weapon Info")]
    public string weaponName;
    public WeaponType weaponType;
    public float damage = 10f;             // נזק בסיסי
    public float range = 1f;               // טווח
    public float speed = 1f;               // מהירות תקיפה
    public float accuracy = 0.8f;          // דיוק
    
    [Header("Special Properties")]
    public float armorPenetration = 0f;    // חדירת שריון
    public float criticalChance = 0.1f;    // סיכוי להיט קריטי
    public float knockbackForce = 0f;      // כוח דחיפה
    public bool isTwoHanded = false;       // שתי ידיים
    
    [Header("Durability")]
    public float durability = 100f;        // עמידות
    public float maxDurability = 100f;
    
    [Header("Requirements")]
    public float requiredStrength = 0f;    // כוח נדרש
    public float requiredSkill = 0f;       // כישור נדרש
    
    public Weapon(string name, WeaponType type, float dmg)
    {
        weaponName = name;
        weaponType = type;
        damage = dmg;
    }
    
    public float GetEffectiveDamage()
    {
        float durabilityMultiplier = durability / maxDurability;
        return damage * durabilityMultiplier;
    }
    
    public void TakeDamage(float amount)
    {
        durability = Mathf.Max(0, durability - amount);
    }
    
    public bool IsBroken()
    {
        return durability <= 0f;
    }
}

[System.Serializable]
public class Injury
{
    public enum InjuryType
    {
        Bruise,         // חבורה
        Cut,            // חתך
        Burn,           // כוויה
        Fracture,       // שבר
        Concussion,     // זעזוע מוח
        Exhaustion,     // תשישות
        Poison,         // הרעלה
        Infection      // זיהום
    }
    
    [Header("Injury Details")]
    public InjuryType injuryType;
    public string bodyPart;             // איבר פגוע
    public float severity = 1f;         // חומרה (0-3)
    public float healingTime = 60f;     // זמן החלמה
    public float painLevel = 1f;        // רמת כאב
    
    [Header("Effects")]
    public float movementPenalty = 0f;  // עיכוב תנועה
    public float combatPenalty = 0f;    // עיכוב קרב
    public float healingRate = 1f;      // קצב החלמה
    
    [Header("Status")]
    public float timeRemaining;         // זמן נותר להחלמה
    public bool isTreated = false;      // האם טופל
    public MinipollBrain treatedBy;     // מי טיפל
    
    public Injury(InjuryType type, string part, float sev)
    {
        injuryType = type;
        bodyPart = part;
        severity = sev;
        timeRemaining = healingTime * severity;
        
        // הגדרת השפעות לפי סוג פציעה
        SetInjuryEffects();
    }
    
    private void SetInjuryEffects()
    {
        switch (injuryType)
        {
            case InjuryType.Fracture:
                movementPenalty = severity * 0.3f;
                combatPenalty = severity * 0.4f;
                painLevel = severity * 2f;
                healingTime = 120f;
                break;
                
            case InjuryType.Concussion:
                combatPenalty = severity * 0.5f;
                painLevel = severity * 1.5f;
                healingTime = 90f;
                break;
                
            case InjuryType.Cut:
                combatPenalty = severity * 0.2f;
                painLevel = severity * 1f;
                healingTime = 45f;
                break;
                
            case InjuryType.Burn:
                painLevel = severity * 2.5f;
                combatPenalty = severity * 0.3f;
                healingTime = 75f;
                break;
                
            case InjuryType.Exhaustion:
                movementPenalty = severity * 0.4f;
                combatPenalty = severity * 0.6f;
                healingTime = 30f;
                break;
        }
    }
    
    public void Heal(float amount)
    {
        timeRemaining = Mathf.Max(0, timeRemaining - amount);
        if (isTreated)
        {
            timeRemaining -= amount * 0.5f; // טיפול מאיץ החלמה
        }
    }
    
    public bool IsHealed()
    {
        return timeRemaining <= 0f;
    }
}

[System.Serializable]
public class CombatAction
{
    public AttackType attackType;
    public Vector3 targetPosition;
    public MinipollBrain target;
    public float damage;
    public bool wasSuccessful;
    public float timestamp;
    
    public CombatAction(AttackType type, MinipollBrain tgt, float dmg)
    {
        attackType = type;
        target = tgt;
        damage = dmg;
        timestamp = Time.time;
    }
}

    public class MinipollBattleSystem : MonoBehaviour
    {
        private MinipollBrain brain;

        [Header("Combat Status")]
        public CombatStats combatStats = new CombatStats();
        public CombatStance currentStance = CombatStance.Defensive;
        public bool isInCombat = false;
        public MinipollBrain currentTarget = null;

        [Header("Equipment")]
        public Weapon primaryWeapon;
        public Weapon secondaryWeapon;      // מגן או נשק שני
        public float armorValue = 0f;       // ערך שריון

        [Header("Combat Behavior")]
        [Range(0f, 1f)]
        public float courage = 0.5f;        // אומץ
        [Range(0f, 1f)]
        public float aggression = 0.5f;     // תוקפנות
        [Range(0f, 1f)]
        public float tactical = 0.5f;       // חשיבה טקטית
        [Range(0f, 1f)]
        public float loyalty = 0.5f;        // נאמנות לחברים

        [Header("Combat Range")]
        public float detectionRange = 8f;   // טווח זיהוי אויבים
        public float engagementRange = 3f;  // טווח תקיפה
        public float fleeRange = 12f;       // טווח בריחה

        [Header("Combat Timing")]
        public float attackCooldown = 2f;   // זמן המתנה בין תקיפות
        private float lastAttackTime = 0f;
        public float combatDuration = 0f;   // משך הקרב הנוכחי
        private float combatStartTime = 0f;

        [Header("Group Combat")]
        public List<MinipollBrain> allies = new List<MinipollBrain>();
        public List<MinipollBrain> enemies = new List<MinipollBrain>();
        public bool fightingInGroup = false;
        public MinipollBrain leader = null; // מנהיג הקבוצה

        [Header("Debug")]
        public bool debugMode = false;
        public bool showCombatInfo = true;

        // Events
        public event Action<MinipollBrain, MinipollBrain> OnCombatStarted;
        public event Action<MinipollBrain, MinipollBrain> OnCombatEnded;
        public event Action<CombatAction> OnAttackExecuted;
        public event Action<Injury> OnInjuryReceived;
        public event Action<MinipollBrain> OnEnemyDefeated;

        private void Awake()
        {
            brain = GetComponent<MinipollBrain>();
            InitializeCombatSystem();
        }

        private void Start()
        {
            // הגדרת תכונות קרב על בסיס גנטיקה
            var genetics = brain.GetComponent<MinipollGeneticsSystem>();
            if (genetics != null)
            {
                courage = genetics.GetGeneValue(GeneType.Strength) * 0.6f + genetics.GetGeneValue(GeneType.Charisma) * 0.4f;
                aggression = genetics.GetGeneValue(GeneType.Aggression);
                tactical = genetics.GetGeneValue(GeneType.Intelligence);

                // עדכון יכולות קרב
                combatStats.strength = genetics.GetGeneValue(GeneType.Strength) * 100f;
                combatStats.agility = genetics.GetGeneValue(GeneType.Speed) * 100f;
                combatStats.endurance = genetics.GetGeneValue(GeneType.Endurance) * 100f;
            }

            // הגדרת נשק בסיסי
            if (primaryWeapon == null)
            {
                primaryWeapon = new Weapon("Fists", WeaponType.Fists, 5f);
            }

            // עדכון יכולות על בסיס כישורים
            UpdateCombatStatsFromSkills();
        }

        private void Update()
        {
            if (!brain.IsAlive) return;

            float deltaTime = Time.deltaTime;

            // עדכון פציעות
            UpdateInjuries(deltaTime);

            // עדכון סיבולת
            UpdateStamina(deltaTime);

            // עדכון מצב קרב
            if (isInCombat)
            {
                UpdateCombat(deltaTime);
            }
            else
            {
                // חיפוש אויבים או איומים
                SearchForThreats();
            }

            // עדכון מורל
            UpdateMorale(deltaTime);
        }

        #region Combat System Initialization

        private void InitializeCombatSystem()
        {
            // אתחול נתוני קרב
            combatStats.stamina = combatStats.maxStamina;
            combatStats.morale = 50f;

            // יצירת נשק בסיסי
            CreateBasicWeapons();
        }

        private void CreateBasicWeapons()
        {
            // כאן ניתן להוסיף יצירת נשקים שונים על בסיס מיקום או תנאים
        }

        private void UpdateCombatStatsFromSkills()
        { var skillSystem = brain.GetComponent<MinipollSkillSystem>();
            if (skillSystem == null) return;

            float combatSkill = skillSystem.GetSkillLevel(SkillType.Combat);
            float huntingSkill = skillSystem.GetSkillLevel(SkillType.Hunting);

            // שיפור יכולות על בסיס כישורים
            combatStats.accuracy += combatSkill * 5f;
            combatStats.defense += combatSkill * 3f;
            combatStats.strength += huntingSkill * 2f;

            // שיפור יכולות נשק
            if (primaryWeapon != null)
            {
                primaryWeapon.accuracy += combatSkill * 0.05f;
                primaryWeapon.damage += combatSkill * 1f;
            }
        }

        #endregion

        #region Combat Detection and Targeting

        private void SearchForThreats()
        {
            if (currentStance == CombatStance.Peaceful) return;

            var nearbyColliders = Physics.OverlapSphere(transform.position, detectionRange);

            foreach (var collider in nearbyColliders)
            {
                var otherBrain = collider.GetComponent<MinipollBrain>();
                if (otherBrain == null || otherBrain == brain || !otherBrain.IsAlive) continue;

                // בדיקה אם זה אויב
                if (IsEnemy(otherBrain))
                {
                    // החלטה על תגובה
                    DecideEngagement(otherBrain);
                    break; // מטפל באויב אחד בכל פעם
                }
            }
        }

        private bool IsEnemy(MinipollBrain otherBrain)
        {
            // בדיקה אם זה אויב על בסיס מערכת דיפלומטיה
            var diplomacySystem = brain.GetComponent<MinipollDiplomacySystem>();
            if (diplomacySystem != null)
            {
                try
                {
                    // return diplomacySystem.IsAtWarWith(otherBrain);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Diplomacy system error for {brain.name}: {ex.Message}");
                    // Fallback to tribe-based hostility check
                }
            }

            // בדיקה אם זה שבט אחר ואנחנו תוקפניים
            var tribeSystem = brain.GetComponent<MinipollTribeSystem>();
            var otherTribe = otherBrain.GetComponent<MinipollTribeSystem>();

            if (tribeSystem != null && otherTribe != null)
            {
                return tribeSystem.tribeId != otherTribe.tribeId && currentStance == CombatStance.Aggressive;
            }

            return false;
        }

        private void DecideEngagement(MinipollBrain enemy)
        {
            // החלטה האם לתקוף על בסיס תכונות אישיות ומצב

            float engagementScore = 0f;

            // תוקפנות ואומץ
            engagementScore += aggression * 0.4f;
            engagementScore += courage * 0.3f;

            // השוואת כוחות
            var enemyBattleSystem = enemy.GetComponent<MinipollBattleSystem>();
            if (enemyBattleSystem != null)
            {
                float powerRatio = GetCombatPower() / enemyBattleSystem.GetCombatPower();
                engagementScore += (powerRatio - 1f) * 0.2f;
            }

            // מצב בריאות
            // engagementScore += brain.Health / 100f * 0.1f;

            // מורל
            engagementScore += (combatStats.morale / 100f) * 0.1f;

            // בדיקת החלטה
            if (engagementScore > 0.6f || currentStance == CombatStance.Berserker)
            {
                StartCombat(enemy);
            }
            else if (engagementScore < 0.2f)
            {
                // בריחה
                AttemptFlee(enemy);
            }
        }

        private void StartCombat(MinipollBrain enemy)
        {
            if (isInCombat) return;

            isInCombat = true;
            currentTarget = enemy;
            combatStartTime = Time.time;
            combatDuration = 0f;

            // הודעה למערכת התנועה לעצור
            var movementController = brain.GetMovementController();
            if (movementController != null)
            {
                // movementController.StopMovement();
            }

            // עדכון רגשות - עליית אדרנלין
            var emotionsSystem = brain.GetEmotionsSystem();
            if (emotionsSystem != null)
            {
                // emotionsSystem.ChangeEmotion(Emotion.Excitement, 30f);
                // emotionsSystem.ChangeEmotion(Emotion.Fear, combatStats.fear);
            }

            OnCombatStarted?.Invoke(brain, enemy);

            if (debugMode)
                Debug.Log($"{brain.name} started combat with {enemy.name}");
        }

        #endregion

        #region Combat Execution

        private void UpdateCombat(float deltaTime)
        {
            if (currentTarget == null || !currentTarget.IsAlive)
            {
                EndCombat(false);
                return;
            }

            combatDuration += deltaTime;

            // בדיקת מרחק מהיעד
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distanceToTarget > fleeRange)
            {
                // היעד רחוק מדי - סיום קרב
                EndCombat(false);
                return;
            }

            // בדיקת אפשרות תקיפה
            if (CanAttack())
            {
                if (distanceToTarget <= GetWeaponRange())
                {
                    ExecuteAttack();
                }
                else
                {
                    // ניסיון התקרבות
                    MoveTowardsTarget();
                }
            }

            // בדיקת בריחה
            if (ShouldFlee())
            {
                AttemptFlee(currentTarget);
            }
        }

        private bool CanAttack()
        {
            return Time.time - lastAttackTime >= attackCooldown &&
                   combatStats.stamina > 20f &&
                   !combatStats.isExhausted;
        }

        private void ExecuteAttack()
        {
            if (currentTarget == null) return;

            // בחירת סוג התקפה
            AttackType attackType = ChooseAttackType();

            // חישוב נזק
            float damage = CalculateAttackDamage(attackType);

            // בדיקת הצלחה
            bool hitSuccess = RollAttackSuccess(attackType);

            if (hitSuccess)
            {
                // תקיפה מוצלחת
                ApplyDamage(currentTarget, damage, attackType);

                // צריכת סיבולת
                ConsumeSamina(10f + damage * 0.5f);

                // נסיון בקרב
                GainCombatExperience(2f);

                // עדכון נשק
                if (primaryWeapon != null)
                {
                    primaryWeapon.TakeDamage(1f);
                }
            }
            else
            {
                // החטאה
                ConsumeSamina(5f);
                GainCombatExperience(0.5f);

                if (debugMode)
                    Debug.Log($"{brain.name} missed attack on {currentTarget.name}");
            }

            lastAttackTime = Time.time;

            // רישום פעולת קרב
            var combatAction = new CombatAction(attackType, currentTarget, hitSuccess ? damage : 0f);
            combatAction.wasSuccessful = hitSuccess;
            OnAttackExecuted?.Invoke(combatAction);
        }

        private AttackType ChooseAttackType()
        {
            // בחירת סוג התקפה על בסיס נשק וטקטיקה

            if (primaryWeapon != null)
            {
                switch (primaryWeapon.weaponType)
                {
                    case WeaponType.Bow:
                    case WeaponType.Throwing:
                        return AttackType.Ranged;

                    case WeaponType.Shield:
                        return AttackType.Defensive;

                    default:
                        break;
                }
            }

            // בחירה על בסיס אישיות וטקטיקה
            if (tactical > 0.7f && UnityEngine.Random.value < 0.3f)
            {
                return AttackType.Ambush;
            }
            else if (currentStance == CombatStance.Berserker)
            {
                return AttackType.Charge;
            }
            else if (combatStats.stamina > 70f && aggression > 0.6f)
            {
                return AttackType.Charge;
            }

            return AttackType.Melee;
        }

        private float CalculateAttackDamage(AttackType attackType)
        {
            float baseDamage = combatStats.strength * 0.2f;

            // נזק נשק
            if (primaryWeapon != null)
            {
                baseDamage += primaryWeapon.GetEffectiveDamage();
            }

            // מודיפקטורים לפי סוג התקפה
            switch (attackType)
            {
                case AttackType.Charge:
                    baseDamage *= 1.5f;
                    break;
                case AttackType.Ambush:
                    baseDamage *= 2f;
                    break;
                case AttackType.Ranged:
                    baseDamage *= 0.8f;
                    break;
                case AttackType.Defensive:
                    baseDamage *= 0.6f;
                    break;
            }

            // רנדומיזציה
            baseDamage *= UnityEngine.Random.Range(0.8f, 1.2f);

            // היט קריטי
            if (primaryWeapon != null && UnityEngine.Random.value < primaryWeapon.criticalChance)
            {
                baseDamage *= 2f;

                if (debugMode)
                    Debug.Log($"{brain.name} scored a critical hit!");
            }

            return baseDamage;
        }

        private bool RollAttackSuccess(AttackType attackType)
        {
            float baseAccuracy = combatStats.accuracy / 100f;

            // דיוק נשק
            if (primaryWeapon != null)
            {
                baseAccuracy *= primaryWeapon.accuracy;
            }

            // מודיפקטורים
            switch (attackType)
            {
                case AttackType.Ambush:
                    baseAccuracy *= 1.3f;
                    break;
                case AttackType.Charge:
                    baseAccuracy *= 0.8f;
                    break;
                case AttackType.Ranged:
                    float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
                    baseAccuracy *= Mathf.Lerp(1f, 0.3f, distance / 10f);
                    break;
            }

            // השפעת פציעות
            baseAccuracy *= (1f - combatStats.totalInjurySeverity * 0.1f);

            // עייפות
            if (combatStats.isExhausted)
                baseAccuracy *= 0.5f;

            return UnityEngine.Random.value < baseAccuracy;
        }

        private void ApplyDamage(MinipollBrain target, float damage, AttackType attackType)
        {
            var targetBattleSystem = target.GetComponent<MinipollBattleSystem>();

            // הפחתת נזק על ידי שריון
            if (targetBattleSystem != null)
            {
                damage = targetBattleSystem.ReduceDamageByArmor(damage);
            }

            // גרימת נזק
            target.TakeDamage(damage);

            // יצירת פציעה
            if (targetBattleSystem != null)
            {
                targetBattleSystem.CreateInjury(attackType, damage);
            }

            if (debugMode)
                Debug.Log($"{brain.name} dealt {damage:F1} damage to {target.name}");

            // בדיקת מוות
            if (!target.IsAlive)
            {
                OnEnemyDefeated?.Invoke(target);
                combatStats.kills++;
                combatStats.victories++;
                GainCombatExperience(10f);
                EndCombat(true);
            }
        }

        #endregion

        #region Damage and Injury System

        public float ReduceDamageByArmor(float incomingDamage)
        {
            // הפחתת נזק על ידי שריון
            float armorReduction = armorValue / (armorValue + 100f);
            float reducedDamage = incomingDamage * (1f - armorReduction);

            return Mathf.Max(reducedDamage, incomingDamage * 0.1f); // לפחות 10% מהנזק עובר
        }

        public void CreateInjury(AttackType attackType, float damage)
        {
            if (damage < 5f) return; // נזק קטן לא יוצר פציעות

            // קביעת סוג פציעה
            Injury.InjuryType injuryType = DetermineInjuryType(attackType, damage);
            string bodyPart = ChooseBodyPart();
            float severity = Mathf.Clamp(damage / 20f, 0.5f, 3f);

            var injury = new Injury(injuryType, bodyPart, severity);
            combatStats.activeInjuries.Add(injury);
            combatStats.totalInjurySeverity += severity;

            OnInjuryReceived?.Invoke(injury);

            // השפעה על מורל
            combatStats.morale -= severity * 10f;
            combatStats.fear += severity * 5f;

            if (debugMode)
                Debug.Log($"{brain.name} received {injuryType} injury to {bodyPart} (severity: {severity:F1})");
        }

        private Injury.InjuryType DetermineInjuryType(AttackType attackType, float damage)
        {
            switch (attackType)
            {
                case AttackType.Charge:
                    return damage > 15f ? Injury.InjuryType.Fracture : Injury.InjuryType.Bruise;

                case AttackType.Melee:
                    if (primaryWeapon?.weaponType == WeaponType.Sword)
                        return Injury.InjuryType.Cut;
                    else if (primaryWeapon?.weaponType == WeaponType.Club)
                        return damage > 12f ? Injury.InjuryType.Fracture : Injury.InjuryType.Bruise;
                    break;

                case AttackType.Ranged:
                    return Injury.InjuryType.Cut;
            }

            return Injury.InjuryType.Bruise;
        }

        private string ChooseBodyPart()
        {
            string[] bodyParts = { "Head", "Torso", "Left Arm", "Right Arm", "Left Leg", "Right Leg" };
            float[] probabilities = { 0.1f, 0.4f, 0.15f, 0.15f, 0.1f, 0.1f };

            float random = UnityEngine.Random.value;
            float cumulative = 0f;

            for (int i = 0; i < bodyParts.Length; i++)
            {
                cumulative += probabilities[i];
                if (random <= cumulative)
                {
                    return bodyParts[i];
                }
            }

            return "Torso";
        }

        private void UpdateInjuries(float deltaTime)
        {
            for (int i = combatStats.activeInjuries.Count - 1; i >= 0; i--)
            {
                var injury = combatStats.activeInjuries[i];
                injury.Heal(deltaTime * injury.healingRate);

                if (injury.IsHealed())
                {
                    combatStats.totalInjurySeverity -= injury.severity;
                    combatStats.activeInjuries.RemoveAt(i);

                    if (debugMode)
                        Debug.Log($"{brain.name} healed from {injury.injuryType} injury");
                }
            }
        }

        public void TreatInjury(Injury injury, MinipollBrain medic)
        {
            if (injury.isTreated) return;

            injury.isTreated = true;
            injury.treatedBy = medic;
            injury.healingRate *= 2f; // טיפול מאיץ החלמה
            injury.painLevel *= 0.5f; // הקטנת כאב

            // נסיון למטפל
            var medicSkills = medic.GetComponent<MinipollSkillSystem>();
            if (medicSkills != null)
            {
                medicSkills.GainExperience(SkillType.Medicine, 3f);
            }

            if (debugMode)
                Debug.Log($"{medic.name} treated {brain.name}'s {injury.injuryType} injury");
        }

        #endregion

        #region Combat Support Functions

        private void UpdateStamina(float deltaTime)
        {
            if (isInCombat)
            {
                // ירידה איטית בסיבולת בקרב
                combatStats.stamina -= deltaTime * 2f;
            }
            else
            {
                // התחדשות סיבולת מחוץ לקרב
                float recoveryRate = combatStats.endurance * 0.1f;
                combatStats.stamina = Mathf.Min(combatStats.maxStamina, combatStats.stamina + recoveryRate * deltaTime);
            }

            // בדיקת תשישות
            combatStats.isExhausted = combatStats.stamina < 10f;

            if (combatStats.isExhausted && isInCombat)
            {
                // כפיית בריחה בתשישות
                AttemptFlee(currentTarget);
            }
        }

        private void ConsumeSamina(float amount)
        {
            combatStats.stamina = Mathf.Max(0f, combatStats.stamina - amount);
        }

        private void UpdateMorale(float deltaTime)
        {
            // החזרה איטית של מורל
            if (!isInCombat)
            {
                combatStats.morale = Mathf.Min(100f, combatStats.morale + deltaTime * 2f);
                combatStats.fear = Mathf.Max(0f, combatStats.fear - deltaTime * 3f);
            }

            // השפעת פציעות על מורל
            if (combatStats.totalInjurySeverity > 2f)
            {
                combatStats.morale -= deltaTime * 5f;
            }
        }

        private void GainCombatExperience(float amount)
        {
            combatStats.combatExperience += amount;
            combatStats.UpdateCombatLevel();

            // נסיון בכישור קרב
            var skillSystem = brain.GetComponent<MinipollSkillSystem>();
            if (skillSystem != null)
            {
                skillSystem.GainExperience(SkillType.Combat, amount * 0.5f);
            }
        }

        private float GetWeaponRange()
        {
            if (primaryWeapon != null)
            {
                return primaryWeapon.range;
            }
            return 1.5f; // טווח אגרופים
        }

        private void MoveTowardsTarget()
        {
            if (currentTarget == null) return;

            var movementController = brain.GetMovementController();
            if (movementController != null)
            {
                Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
                Vector3 targetPosition = currentTarget.transform.position - direction * (GetWeaponRange() * 0.8f);
                // movementController.MoveTo(targetPosition);
            }
        }

        private bool ShouldFlee()
        {
            // בדיקות לבריחה
            if (combatStats.morale < 20f) return true;
            if (combatStats.isExhausted) return true;
            // if (brain.Health <= ) return true;
            if (combatStats.totalInjurySeverity > 3f) return true;

            // בריחה על בסיס היחס כוחות
            if (currentTarget != null)
            {
                var enemyBattleSystem = currentTarget.GetComponent<MinipollBattleSystem>();
                if (enemyBattleSystem != null)
                {
                    float powerRatio = GetCombatPower() / enemyBattleSystem.GetCombatPower();
                    if (powerRatio < 0.4f && courage < 0.7f) return true;
                }
            }

            return false;
        }

        private void AttemptFlee(MinipollBrain enemy)
        {
            // ניסיון בריחה
            Vector3 fleeDirection = (transform.position - enemy.transform.position).normalized;
            Vector3 fleePosition = transform.position + fleeDirection * fleeRange;

            var movementController = brain.GetMovementController();
            if (movementController != null)
            {
                // movementController.MoveTo(fleePosition);
            }

            // פגיעה במורל
            combatStats.morale -= 10f;
            combatStats.fear += 5f;

            if (isInCombat)
            {
                EndCombat(false);
            }

            if (debugMode)
                Debug.Log($"{brain.name} is fleeing from {enemy.name}");
        }

        private void EndCombat(bool victorious)
        {
            if (!isInCombat) return;

            isInCombat = false;
            var enemy = currentTarget;
            currentTarget = null;

            // עדכון סטטיסטיקות
            if (victorious)
            {
                combatStats.victories++;
                combatStats.morale += 20f;
                GainCombatExperience(5f);
            }
            else
            {
                combatStats.defeats++;
                combatStats.morale -= 15f;
                combatStats.fear += 10f;
            }

            OnCombatEnded?.Invoke(brain, enemy);

            if (debugMode)
                Debug.Log($"{brain.name} ended combat with {enemy?.name} - {(victorious ? "Victory" : "Defeat")}");
        }

        #endregion

        #region Public Interface

        public float GetCombatPower()
        {
            float power = combatStats.strength + combatStats.agility + combatStats.defense;
            power *= (combatStats.stamina / combatStats.maxStamina); // עייפות
            power *= (1f - combatStats.totalInjurySeverity * 0.1f); // פציעות
            power += armorValue;

            if (primaryWeapon != null)
            {
                power += primaryWeapon.GetEffectiveDamage() * 2f;
            }

            return power;
        }

        public bool IsInCombat()
        {
            return isInCombat;
        }

        public MinipollBrain GetCurrentTarget()
        {
            return currentTarget;
        }

        public void EquipWeapon(Weapon weapon, bool isPrimary = true)
        {
            if (isPrimary)
            {
                primaryWeapon = weapon;
            }
            else
            {
                secondaryWeapon = weapon;
            }

            if (debugMode)
                Debug.Log($"{brain.name} equipped {weapon.weaponName}");
        }

        public void SetCombatStance(CombatStance stance)
        {
            currentStance = stance;

            if (debugMode)
                Debug.Log($"{brain.name} changed combat stance to {stance}");
        }

        public void ForceAttack(MinipollBrain target)
        {
            if (target != null && target.IsAlive)
            {
                StartCombat(target);
            }
        }

        public List<Injury> GetActiveInjuries()
        {
            return new List<Injury>(combatStats.activeInjuries);
        }

        public string GetCombatSummary()
        {
            return $"Combat: L{combatStats.combatLevel} | " +
                   $"W/L: {combatStats.victories}/{combatStats.defeats} | " +
                   $"Power: {GetCombatPower():F0} | " +
                   $"Morale: {combatStats.morale:F0} | " +
                   $"Injuries: {combatStats.activeInjuries.Count}";
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (debugMode)
            {
                // טווח זיהוי
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, detectionRange);

                // טווח תקיפה
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, engagementRange);

                // קו ליעד נוכחי
                if (currentTarget != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, currentTarget.transform.position);
                }

#if UNITY_EDITOR
                if (showCombatInfo)
                {
                    string info = $"Combat L{combatStats.combatLevel}\n";
                    info += $"Power: {GetCombatPower():F0}\n";
                    info += $"Stance: {currentStance}\n";
                    info += $"Stamina: {combatStats.stamina:F0}/{combatStats.maxStamina:F0}\n";
                    info += $"Morale: {combatStats.morale:F0}";

                    if (isInCombat)
                    {
                        info += $"\nFIGHTING: {currentTarget?.name}";
                    }

                    if (combatStats.activeInjuries.Count > 0)
                    {
                        info += $"\nInjuries: {combatStats.activeInjuries.Count}";
                    }

                    UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, info);
                }
#endif
            }
        }

        public void LogCombatStatus()
        {
            string log = $"=== Combat Status for {brain.name} ===\n";
            log += $"Combat Level: {combatStats.combatLevel}\n";
            log += $"Combat Experience: {combatStats.combatExperience:F1}\n";
            log += $"Combat Power: {GetCombatPower():F1}\n";
            log += $"Current Stance: {currentStance}\n\n";

            log += "Combat Stats:\n";
            log += $"  Strength: {combatStats.strength:F1}\n";
            log += $"  Agility: {combatStats.agility:F1}\n";
            log += $"  Endurance: {combatStats.endurance:F1}\n";
            log += $"  Accuracy: {combatStats.accuracy:F1}\n";
            log += $"  Defense: {combatStats.defense:F1}\n\n";

            log += "Combat Record:\n";
            log += $"  Victories: {combatStats.victories}\n";
            log += $"  Defeats: {combatStats.defeats}\n";
            log += $"  Kills: {combatStats.kills}\n";
            log += $"  Win Rate: {combatStats.GetWinRate() * 100:F1}%\n\n";

            log += "Current State:\n";
            log += $"  Stamina: {combatStats.stamina:F1}/{combatStats.maxStamina:F1}\n";
            log += $"  Morale: {combatStats.morale:F1}\n";
            log += $"  Fear: {combatStats.fear:F1}\n";
            log += $"  Is Exhausted: {combatStats.isExhausted}\n";
            log += $"  In Combat: {isInCombat}\n";

            if (currentTarget != null)
            {
                log += $"  Fighting: {currentTarget.name}\n";
            }

            log += "\nEquipment:\n";
            if (primaryWeapon != null)
            {
                log += $"  Primary: {primaryWeapon.weaponName} (Dmg: {primaryWeapon.GetEffectiveDamage():F1})\n";
            }
            if (secondaryWeapon != null)
            {
                log += $"  Secondary: {secondaryWeapon.weaponName}\n";
            }
            log += $"  Armor: {armorValue:F1}\n";

            if (combatStats.activeInjuries.Count > 0)
            {
                log += $"\nActive Injuries ({combatStats.activeInjuries.Count}):\n";
                foreach (var injury in combatStats.activeInjuries)
                {
                    log += $"  {injury.injuryType} on {injury.bodyPart} (Severity: {injury.severity:F1})\n";
                    log += $"    Time to heal: {injury.timeRemaining:F0}s\n";
                    log += $"    Treated: {injury.isTreated}\n";
                }
            }

            Debug.Log(log);
        }

        #endregion
    }
