/***************************************************************
 *  MinipollWorldInteraction.cs
 *
 *  תיאור כללי:
 *    סקריפט שנמצא על המיניפול עצמו (יחד עם MinipollBrain וכו’),
 *    ותפקידו לנהל אינטראקציות עם חפצים בסביבה (InteractableObject).
 *      - איתור חפץ (OverlapSphere/Raycast)
 *      - גישה לפונקציית OnInteract(himself)
 *      - עדכון צרכים/מצב בהתאם
 *    ניתן לשלב לוגיקה שהמיניפול "חכם": 
 *      בוחר לחפש אוכל כש-Hunger נמוך, 
 *      לגשת למיטה כש-Energy נמוכה, וכו’.
 *
 *  דרישות קדם:
 *    - להניח על Prefab Minipoll (אולי על אותו אובייקט של Brain).
 *    - יש לשלב UpdateWorldInteraction(...) איפשהו, לדוגמה ב-MinipollBrain.Update() או ב-MovementController.
 ***************************************************************/

using UnityEngine;
using MinipollGame.Core;
using MinipollGame.Systems.Core;
using MinipollGame.Controllers;
using System;
public class MinipollWorldInteraction : MonoBehaviour
{
    private MinipollBrain brain;

    [Header("Interaction Settings")]
    public float interactionRadius = 1.5f; // טווח שבו אפשר להשתמש בחפץ
    public float detectionRadius = 5f;     // רדיוס לחיפוש חפצים רלוונטיים
    public LayerMask interactableLayer;    // שכבת "Interactable" לסינון

    [Tooltip("האם מודול זה יקבל עדכונים מעצמו או ממקום אחר?")]
    public bool selfUpdate = true;

    private void Awake()
    {
        brain = GetComponent<MinipollBrain>();
        if (!brain)
        {
            brain = GetComponentInParent<MinipollBrain>();
        }
    }

    private void Update()
    {
        if (!selfUpdate) return;

        // UpdateWorldInteraction(Time.deltaTime);
    }

    /// <summary>
    /// בפועל קוראים לפונקציה זו בכל פריים (אם selfUpdate=true),
    /// או ידנית מתוך MinipollBrain/MotionController.
    /// </summary>
    // public void UpdateWorldInteraction(float deltaTime)
    // {
    //     // כאן אפשר לבדוק אם יש צורך מסוים נמוך => לחפש חפץ שימלא אותו
    //     // כדוגמה: אם Hunger < 30 => לחפש Food
    //     if (brain.GetNeedsSystem() is NeedsSystem needsSystem)
    //     {
    //         // if (needsSystem.hunger.currentValue < 30f)
    //         {
    //             // ננסה לאתר Food קרוב
    //             // InteractableObject food = FindClosestObject(InteractableType.Food);
    //             if (food != null)
    //             {
    //                 // נווט לשם (MovementController)
    //                 if (Vector3.Distance(transform.position, food.transform.position) > interactionRadius)
    //                 {
    //                     // רחוק => StartSeekingResource
    //                     // brain.GetMovementController()?.StartSeekingResource(food.transform.position);
    //                 }
    //                 else
    //                 {
    //                     // מספיק קרוב => OnInteract
    //                     food.OnInteract(gameObject);
    //                 }
    //             }
    //         }
    //         // else if (needsSystem.energy.currentValue < 20f)
    //         {
    //             // לחפש מיטה?
    //             InteractableObject bed = FindClosestObject(InteractableType.Bed);
    //             if (bed != null)
    //             {
    //                 // אם קרובים => OnInteract
    //                 float dist = Vector3.Distance(transform.position, bed.transform.position);
    //                 if (dist <= interactionRadius)
    //                 {
    //                     bed.OnInteract(gameObject);
    //                 }
    //                 else
    //                 {
    //                     // אחרת => Seek
    //                     brain.GetMovementController()?.StartSeekingResource(bed.transform.position);
    //                 }
    //             }
    //         }
    //         // אפשר להוסיף עוד לוגיקה: כיף נמוך => לחפש Toy, וכו’.
    //     }
    // }

    /// <summary>
    /// מאתר חפץ קרוב מסוג מסוים ברדיוס detectionRadius
    /// </summary>
    public InteractableObject FindClosestObject(InteractableType type)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);
        float minDist = float.MaxValue;
        InteractableObject closest = null;
        foreach (var c in hits)
        {
            var io = c.GetComponent<InteractableObject>();
            if (io != null && io.objectType == type)
            {
                float d = Vector3.Distance(transform.position, io.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    closest = io;
                }
            }
        }
        return closest;
    }

    /// <summary>
    /// מאתר את האוכל הקרוב ביותר
    /// </summary>
    public GameObject FindNearestFood()
    {
        var food = FindClosestObject(InteractableType.Food);
        return food?.gameObject;
    }

    /// <summary>
    /// מאתר את המים הקרובים ביותר
    /// </summary>
    public GameObject FindNearestWater()
    {
        var water = FindClosestObject(InteractableType.Water);
        return water?.gameObject;
    }
}

internal class NeedsSystem
{
    internal object hunger;
    internal object energy;

    internal void FillNeed(string v1, float v2)
    {
        throw new NotImplementedException();
    }
}