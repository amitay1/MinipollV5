/***************************************************************
 *  MinipollGeneticsSystem.cs
 *
 *  תיאור כללי:
 *    מודול גנטיקה בסיסית – כל מיניפול יכול להכיל "גנים" שקובעים
 *    תכונות פיזיות/התנהגותיות (מהירות, סיבולת, צבע, אופי וכו’).
 *    - בעת יצירה (Spawn) יכול להתקבל סט תכונות רנדומלי או קבוע
 *    - בעת רבייה (InheritFrom), משלב 50-50 תכונות מהורים + מוטציה
 *    - מגדיר/משנה פרמטרים במודולים אחרים (למשל moveSpeed ב-MovementController)
 *
 *  דרישות קדם:
 *    - לשים על אותה GameObject עם MinipollBrain, 
 *      ורצוי להתחבר ל-MovementController, VisualController וכו’.
 ***************************************************************/

using UnityEngine;
using MinipollGame.Core;
using MinipollGame.Systems.Core;
using MinipollGame.Controllers;
[System.Serializable]
public class MinipollGenes
{
    [Header("Physical / Stats")]
    [Range(1f, 5f)]
    public float baseMoveSpeed = 2f;     // משפיע על MovementController
    [Range(50f, 150f)]
    public float baseHealth = 100f;      // משפיע על Brain.health max
    [Range(0f, 1f)]
    public float colorHue = 0.5f;        // משפיע על Visual (למשל, גוון הצבע)

    [Header("Behavior / Personality")]
    [Range(0f, 1f)]
    public float aggression = 0.5f;
    [Range(0f, 1f)]
    public float curiosity = 0.5f;
    [Range(0f, 1f)]
    public float sociability = 0.5f;
}

public class MinipollGeneticsSystem : MonoBehaviour
{
    public MinipollGenes genes; // הגנים של היצור הנוכחי

    [Header("Mutation")]
    [Tooltip("הסיכוי למוטציה בכל אלל (תכונה) בעת הירושה")]
    [Range(0f, 1f)]
    public float mutationChance = 0.1f; 
    [Tooltip("עד כמה המוטציה מקסימלית יכולה לשנות (+/- X אחוז)")]
    [Range(0f, 0.5f)]
    public float mutationAmount = 0.1f;

    private MinipollBrain brain;
    internal int? generation;

    private void Awake()
    {
        // אם אין גנים, נייצר סט דיפולטי
        if (genes == null)
        {
            genes = new MinipollGenes();
            RandomizeGenes();
        }
        brain = GetComponent<MinipollBrain>();
    }

    private void Start()
    {
        // נעדכן באופן ראשוני מודולים אחרים
        ApplyGenesToModules();
    }

    /// <summary>
    /// הפעלה בעת "התורשה" – משלב גנים מהורה א’ והורה ב’, 
    /// 50% רנדומליים לכל אלל, ואז מוטציה
    /// </summary>
    public void InheritFrom(MinipollGeneticsSystem parentA, MinipollGeneticsSystem parentB)
    {
        if (parentA == null || parentB == null) return;

        genes.baseMoveSpeed = Random.value < 0.5f ? parentA.genes.baseMoveSpeed : parentB.genes.baseMoveSpeed;
        genes.baseHealth    = Random.value < 0.5f ? parentA.genes.baseHealth    : parentB.genes.baseHealth;
        genes.colorHue      = Random.value < 0.5f ? parentA.genes.colorHue      : parentB.genes.colorHue;
        genes.aggression    = Random.value < 0.5f ? parentA.genes.aggression    : parentB.genes.aggression;
        genes.curiosity     = Random.value < 0.5f ? parentA.genes.curiosity     : parentB.genes.curiosity;
        genes.sociability   = Random.value < 0.5f ? parentA.genes.sociability   : parentB.genes.sociability;

        // מוטציה לכל תכונה
        Mutate(ref genes.baseMoveSpeed, 1f, 5f);
        Mutate(ref genes.baseHealth, 50f, 150f);
        Mutate(ref genes.colorHue, 0f, 1f);
        Mutate(ref genes.aggression, 0f, 1f);
        Mutate(ref genes.curiosity, 0f, 1f);
        Mutate(ref genes.sociability, 0f, 1f);

        // אחרי שסיימנו הירושה => ניישם מייד
        ApplyGenesToModules();
    }

    /// <summary>
    /// שינוי אקראי קטן בתכונה, אם במקרה מתמזל
    /// </summary>
    private void Mutate(ref float value, float minVal, float maxVal)
    {
        if (Random.value < mutationChance)
        {
            // שינוי באחוזים
            float range = (maxVal - minVal) * mutationAmount;
            float delta = Random.Range(-range, range);
            value += delta;
            if (value < minVal) value = minVal;
            if (value > maxVal) value = maxVal;
        }
    }

    /// <summary>
    /// אם אין גנים מלכתחילה, אפשר לבצע רנדומיזציה מלאה 
    /// (משתמש פעם אחת בלבד, במקרה שהיצור לא ירש)
    /// </summary>
    public void RandomizeGenes()
    {
        genes.baseMoveSpeed = Random.Range(1f, 5f);
        genes.baseHealth    = Random.Range(50f, 150f);
        genes.colorHue      = Random.Range(0f, 1f);
        genes.aggression    = Random.Range(0f, 1f);
        genes.curiosity     = Random.Range(0f, 1f);
        genes.sociability   = Random.Range(0f, 1f);
    }    /// <summary>
    /// מפעיל את הגנים על המודולים – למשל MovementController, Brain.health, VisualController
    /// </summary>
    public void ApplyGenesToModules()
    {
        // Movement
        var moveCtrl = brain?.GetMovementController() as MinipollMovementController;
        if (moveCtrl != null)
        {
            moveCtrl.moveSpeed = genes.baseMoveSpeed;
        }

        // בריאות מקסימלית => נערוך brain.health אם הוא מתחת/מעל
        if (brain)
        {
            float oldHealth = brain.health;
            float maxH = genes.baseHealth;
            // אם נרצה “Max Health” בנפרד, אפשר לשמור brain.maxHealth
            // כרגע פשוט נקבע health לצורך הדגמה:
            if (oldHealth > maxH) brain.health = maxH;
        }

        // צבע – נשלח ל MinipollVisualController
        var visCtrl = GetComponentInChildren<MinipollVisualController>();
        if (visCtrl)
        {
            // נצבע בגוון HSL (או HSV) => נשתמש בשיטה פשוטה: Color.HSVToRGB
            Color c = Color.HSVToRGB(genes.colorHue, 0.8f, 1f);
            visCtrl.ForceColor(c);
        }

        // אפשר גם להשפיע על מערכות אחרות (למשל Emotions / social)
        // לדוגמה, אם aggression גבוה => מוריד trust מהר, וכו’ (Implement as needed)
    }    internal float GetGeneValue(object intelligence)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Set gender and update related genetic traits
    /// </summary>
    internal void SetGender(Gender gender)
    {
        // TODO: Implement gender-specific genetic modifications
        // For now, just log the change
        Debug.Log($"[MinipollGeneticsSystem] Setting gender to: {gender}");
        
        // Could implement:
        // - Gender-specific gene expression
        // - Different fitness calculations
        // - Gender-linked traits
    }
}
