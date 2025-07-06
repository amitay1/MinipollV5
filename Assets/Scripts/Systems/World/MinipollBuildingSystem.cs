/***************************************************************
 *  MinipollBuildingSystem.cs
 *
 *  תיאור כללי:
 *    מודול המאפשר למיניפול “ליצור מבנים”:
 *      - איסוף משאבים (עץ, אבן וכד’)
 *      - הגדרת “תוכנית בנייה” (BuildingBlueprint)
 *      - הקמת המבנה בשלבים, לכל שלב נדרשים חומרים
 *      - בסיום, נוצר מבנה פיזי (GameObject) בסצנה
 *
 *  דרישות קדם:
 *    - למקם סקריפט זה על המיניפול (או בכפוף ל-MinipollBrain).
 *    - תלות במערכות Economy/Inventory (כדי להחזיק חומרים), MinipollTaskSystem וכו’.
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using MinipollGame.Core;

[System.Serializable]
public class BuildingBlueprint
{
    public string buildingName;
    public List<BuildStage> stages = new List<BuildStage>();

    [System.Serializable]
    public class BuildStage
    {
        public string description;            // “לבנות שלד עץ”
        public Dictionary<string,int> cost;   // חומרים נדרשים: (“Wood”:10,”Stone”:5 וכו’)
        public float timeRequired;            // זמן (שניות)
    }
}

public class MinipollBuildingSystem : MonoBehaviour
{
    private MinipollBrain brain;
    [Header("Current Project")]
    public BuildingBlueprint currentBlueprint;
    public int currentStageIndex = -1;
    public float stageTimer = 0f;

    [Tooltip("האובייקט שמחזיק משאבים, למשל MinipollEconomySystem או אחר")]
    public MinipollEconomySystem economySystem;
    public bool isBuilding = false;

    [Header("Settings")]
    public bool selfUpdate = false;

    private void Awake()
    {
        brain = GetComponent<MinipollBrain>();
        if (!economySystem)
        {
            economySystem = GetComponent<MinipollEconomySystem>();
        }
    }

    private void Update()
    {
        if (selfUpdate)
        {
            UpdateBuilding(Time.deltaTime);
        }
    }

    public void UpdateBuilding(float deltaTime)
    {
        if (!isBuilding || currentBlueprint == null || currentStageIndex < 0) return;

        var stage = currentBlueprint.stages[currentStageIndex];
        stageTimer += deltaTime;

        // בודק אם סיימנו את הזמן לבנייה לשלב זה
        if (stageTimer >= stage.timeRequired)
        {
            // סיימנו שלב
            currentStageIndex++;
            stageTimer = 0f;

            if (currentStageIndex >= currentBlueprint.stages.Count)
            {
                // סיימנו את כל השלבים => בנייה מושלמת
                Debug.Log($"{brain.name} completed building {currentBlueprint.buildingName}!");
                PlaceFinishedBuilding();
                isBuilding = false;
                currentBlueprint = null;
                currentStageIndex = -1;
            }
            else
            {
                // עבר לשלב הבא
                Debug.Log($"{brain.name} advanced to stage {currentStageIndex} of {currentBlueprint.buildingName}");
                if (!CheckAndConsumeResources(currentBlueprint.stages[currentStageIndex]))
                {
                    // אין מספיק חומרים => בנייה נעצרת
                    Debug.LogWarning($"{brain.name} lacks resources for stage {currentStageIndex}, building halted.");
                    isBuilding = false;
                }
            }
        }
    }

    /// <summary>
    /// מתחיל פרויקט בנייה לפי bluePrint
    /// </summary>
    public bool StartBuildingProject(BuildingBlueprint blueprint)
    {
        if (isBuilding)
        {
            Debug.LogWarning($"{brain.name} is already building something!");
            return false;
        }
        currentBlueprint = blueprint;
        currentStageIndex = 0;
        stageTimer = 0f;

        // קודם נבדוק אם יש חומרים לשלבים
        if (!CheckAndConsumeResources(blueprint.stages[0]))
        {
            Debug.LogWarning($"{brain.name} lacks resources for stage 0 of {blueprint.buildingName}");
            currentBlueprint = null;
            currentStageIndex = -1;
            return false;
        }

        isBuilding = true;
        Debug.Log($"{brain.name} started building {blueprint.buildingName}");
        return true;
    }

    /// <summary>
    /// בודק אם יש מספיק חומרים בשלב, צורך אותם
    /// </summary>
    private bool CheckAndConsumeResources(BuildingBlueprint.BuildStage stage)
    {
        if (economySystem == null)
        {
            Debug.LogWarning("No economy system - can't check resources");
            return false;
        }
        if (stage.cost == null) return true;

        // בודק ולוקח
        foreach (var kvp in stage.cost)
        {
            string itemName = kvp.Key;
            int required = kvp.Value;
            if (!economySystem.items.ContainsKey(itemName) || economySystem.items[itemName] < required)
            {
                // חסר חומר
                return false;
            }
        }
        // אם יש הכל, נסיר
        foreach (var kvp in stage.cost)
        {
            economySystem.RemoveItem(kvp.Key, kvp.Value);
        }
        return true;
    }

    /// <summary>
    /// מניח מבנה מוכן בסצנה
    /// </summary>
    private void PlaceFinishedBuilding()
    {
        // לצורך הדגמה, ניצור קובייה
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.transform.position = transform.position + transform.forward * 2f;
        building.transform.localScale = new Vector3(4,2,4);
        building.name = $"{currentBlueprint.buildingName}_Built";
        building.GetComponent<Renderer>().material.color = Color.grey;
    }
}
