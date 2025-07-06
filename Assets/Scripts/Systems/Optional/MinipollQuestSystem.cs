/***************************************************************
 *  MinipollQuestSystem.cs
 *
 *  תיאור כללי:
 *    מערכת משימות מתקדמת (קווסטים):
 *      - Quest הוא אובייקט שמכיל רשימת Objectives (איסוף, הריגה, הגעה לנקודה)
 *      - אפשר להוסיף משימות ל"מיניפול" או לפרויקט
 *      - מעקב אחר סטטוס המשימה (NotStarted / InProgress / Completed / Failed)
 *      - מאפשר לייצר לוח משימות (QuestBoard) גלובלי שמיניפולים יכולים לקחת משימות ממנו
 *
 *  דרישות קדם:
 *    - MinipollTaskSystem קיים? (אפשר למזג איתו או להשתמש בנפרד)
 *    - מנגנוני איסוף, לחימה, וכו’ כדי לעדכן התקדמות המשימה
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using MinipollCore;
[System.Serializable]
public enum QuestStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}

[System.Serializable]
public class QuestObjective
{
    public string description;
    public int currentAmount;
    public int requiredAmount;
    public bool isComplete()
    {
        return (currentAmount >= requiredAmount);
    }
}

[System.Serializable]
public class Quest
{
    public string questName;
    public QuestStatus status;
    public List<QuestObjective> objectives;
    public int rewardCoins;
    public string rewardItemName;
    public int rewardItemCount;

    public Quest(string name, int coins = 0)
    {
        questName = name;
        status = QuestStatus.NotStarted;
        objectives = new List<QuestObjective>();
        rewardCoins = coins;
    }
}

public class MinipollQuestSystem : MonoBehaviour
{
    public List<Quest> quests = new List<Quest>();
    public bool selfUpdate = false;
    private MinipollEconomySystem economy;

    private void Awake()
    {
        economy = GetComponent<MinipollEconomySystem>();
    }

    private void Update()
    {
        if (selfUpdate)
        {
            UpdateQuests(Time.deltaTime);
        }
    }

    public void UpdateQuests(float dt)
    {
        foreach (var q in quests)
        {
            if (q.status == QuestStatus.InProgress)
            {
                bool allComplete = true;
                foreach (var obj in q.objectives)
                {
                    if (!obj.isComplete())
                    {
                        allComplete = false;
                        break;
                    }
                }
                if (allComplete)
                {
                    q.status = QuestStatus.Completed;
                    RewardQuest(q);
                }
            }
        }
    }

    public void StartQuest(Quest quest)
    {
        if (quest.status == QuestStatus.NotStarted)
        {
            quest.status = QuestStatus.InProgress;
            Debug.Log($"Quest {quest.questName} started!");
        }
    }

    public void AddQuest(Quest quest)
    {
        quests.Add(quest);
        Debug.Log($"Added quest: {quest.questName}");
    }

    public void UpdateObjective(string questName, int objectiveIndex, int amount)
    {
        Quest q = quests.Find(x => x.questName == questName);
        if (q != null && q.status == QuestStatus.InProgress)
        {
            if (objectiveIndex >= 0 && objectiveIndex < q.objectives.Count)
            {
                q.objectives[objectiveIndex].currentAmount += amount;
                Debug.Log($"Quest {questName} objective {objectiveIndex} updated: +{amount}");
            }
        }
    }

    private void RewardQuest(Quest quest)
    {
        Debug.Log($"Quest {quest.questName} completed! Rewarding {quest.rewardCoins} coins + {quest.rewardItemCount} {quest.rewardItemName}");

        if (economy != null)
        {
            economy.coins += quest.rewardCoins;
            if (!string.IsNullOrEmpty(quest.rewardItemName))
            {
                economy.AddItem(quest.rewardItemName, quest.rewardItemCount);
            }
        }
    }
}

/***************************************************************
 *  QuestBoard.cs
 *  לוח משימות גלובלי: quests שהמיניפולים יכולים לקחת
 ***************************************************************/
public class QuestBoard : MonoBehaviour
{
    public static QuestBoard Instance;
    public List<Quest> availableQuests = new List<Quest>();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// הוספת קווסט ללוח
    /// </summary>
    public void PostQuest(Quest quest)
    {
        availableQuests.Add(quest);
        Debug.Log($"QuestBoard posted quest: {quest.questName}");
    }

    /// <summary>
    /// משיכת קווסט ע"י מיניפול
    /// </summary>
    public Quest TakeQuest(string questName)
    {
        var q = availableQuests.Find(x => x.questName == questName && x.status == QuestStatus.NotStarted);
        if (q != null)
        {
            availableQuests.Remove(q);
            Debug.Log($"Quest {questName} taken from board");
            return q;
        }
        return null;
    }
}
