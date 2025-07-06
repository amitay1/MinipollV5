/***************************************************************
 *  MinipollTaskSystem.cs
 *
 *  תיאור כללי:
 *    מערכת משימות (Tasks) שמאפשרת לרשום "טאסק" למיניפול: 
 *      - אסוף X יחידות מזון, 
 *      - שמור על אזור מסוים, 
 *      - תגיע למיקום Y
 *    המודול עוקב אחרי רשימת משימות, ומבצע מעקב מצב (InProgress, Completed).
 *
 *  דרישות קדם:
 *    - לשים על המיניפול
 *    - להוסיף/להסיר משימות מבחוץ, 
 *      - במשחק אמיתי, ייתכן שגוף חיצוני יקצה משימות (כמו Tribe leader).
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using MinipollGame.Core;

[System.Serializable]
public class MinipollTask
{
    public string taskName;
    public TaskStatus status;
    public float targetValue;
    public float currentValue;
    public Vector3 targetPosition;
    public enum TaskStatus { NotStarted, InProgress, Completed, Failed }

    public MinipollTask(string name, float tValue = 1f, Vector3 pos = default)
    {
        taskName = name;
        targetValue = tValue;
        targetPosition = pos;
        status = TaskStatus.NotStarted;
        currentValue = 0f;
    }
}

public class MinipollTaskSystem : MonoBehaviour
{
    public List<MinipollTask> tasks = new List<MinipollTask>();

    [Header("Settings")]
    public bool selfUpdate = false; // אם נרצה לעדכן עצמאי
    private MinipollBrain brain;

    public event Action<MinipollTask> OnTaskStarted;
    public event Action<MinipollTask> OnTaskCompleted;
    public event Action<MinipollTask> OnTaskFailed;

    private void Awake()
    {
        brain = GetComponent<MinipollBrain>();
    }

    private void Update()
    {
        if (selfUpdate)
        {
            UpdateTasks(Time.deltaTime);
        }
    }

    public void UpdateTasks(float deltaTime)
    {
        // עובר על כל המשימות, בודק סטטוס
        for (int i = 0; i < tasks.Count; i++)
        {
            var t = tasks[i];
            switch (t.status)
            {
                case MinipollTask.TaskStatus.NotStarted:
                    // מעבירים ל-InProgress
                    t.status = MinipollTask.TaskStatus.InProgress;
                    OnTaskStarted?.Invoke(t);
                    break;
                case MinipollTask.TaskStatus.InProgress:
                    // כאן הגיון משימה: למשל אם taskName="CollectFood10", 
                    // נבדוק אם minipoll אסף 10 יחידות.
                    CheckTaskCompletion(t);
                    break;
                case MinipollTask.TaskStatus.Completed:
                case MinipollTask.TaskStatus.Failed:
                    // לא עושים כלום, או נסיר
                    break;
            }
        }
    }

    private void CheckTaskCompletion(MinipollTask task)
    {
        // דוגמה פשוטה: אם "CollectFood10" => taskName="CollectFood", targetValue=10
        // בודקים האם HungerNeed עלה בכמות? 
        // בפועל, נצטרך מנגנון ספירת מזון. כאן נדגים עקרון.

        if (task.taskName.Contains("CollectFood"))
        {
            // נניח currentValue=כמות שכבר אסף
            // אפשר לשלב InteractableObject וכו’
            // אם currentValue>=targetValue => success
            if (task.currentValue >= task.targetValue)
            {
                task.status = MinipollTask.TaskStatus.Completed;
                OnTaskCompleted?.Invoke(task);
            }
        }
        else if (task.taskName == "GoToPosition")
        {
            float dist = Vector3.Distance(transform.position, task.targetPosition);
            if (dist < 1f)
            {
                task.status = MinipollTask.TaskStatus.Completed;
                OnTaskCompleted?.Invoke(task);
            }
        }
        // אפשר עוד... 
    }

    public void AddTask(MinipollTask newTask)
    {
        tasks.Add(newTask);
    }

    public void RemoveTask(MinipollTask task)
    {
        tasks.Remove(task);
    }
}
