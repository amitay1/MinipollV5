/***************************************************************
 *  MinipollWorldEventSystem.cs
 *
 *  תיאור כללי:
 *    מערכת של אירועים עולמיים אקראיים/מתוכננים שיכולים להשפיע 
 *    על כל המיניפולים או אזורים בעולם:
 *      - אירועי מזג אוויר קיצוניים (טורנדו, שרב)
 *      - פלישת יצורים (bandits, מפלצות)
 *      - אירועי ברכה (גשם מוזהב, בונוס מזון)
 *    כל אירוע מגדיר משך, השפעה, אזור/רדיוס. 
 *    האירועים מנוהלים ע"י מנגנון רישום, 
 *    וב-Update כל אירוע מתקדם עד לסיום.
 *
 *  דרישות קדם:
 *    - ממקמים WorldEventSystem אחד בסצנה (Singleton).
 *    - MinipollManager, WorldManager וכו’ יכולים לתקשר איתו.
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using MinipollGame.Systems.Core;
using MinipollGame.Core;

[System.Serializable]
public class WorldEvent
{
    public string eventName;
    public WorldEventType eventType;  // Missing property
    public float intensity = 1f;      // Missing property
    public float duration;            // כמה זמן נמשך
    public float elapsed;             // כמה עבר
    public Vector3 centerPosition;    // איפה מתקיים
    public float radius;              // רדיוס השפעה
    public Action<WorldEvent> onStart;
    public Action<WorldEvent, float> onUpdate; // event, deltaTime
    public Action<WorldEvent> onEnd;
}

public class MinipollWorldEventSystem : MonoBehaviour
{
    public static MinipollWorldEventSystem Instance;

    // Events
    public static event Action<WorldEvent> OnWorldEventStarted;

    [Header("Active Events")]
    public List<WorldEvent> activeEvents = new List<WorldEvent>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = activeEvents.Count - 1; i >= 0; i--)
        {
            var ev = activeEvents[i];
            ev.elapsed += dt;
            ev.onUpdate?.Invoke(ev, dt);

            if (ev.elapsed >= ev.duration)
            {
                // סיום
                ev.onEnd?.Invoke(ev);
                activeEvents.RemoveAt(i);
            }
        }
    }    /// <summary>
    /// הוספת אירוע חדש למערכת
    /// </summary>
    public void AddEvent(WorldEvent ev)
    {
        ev.elapsed = 0f;
        activeEvents.Add(ev);
        ev.onStart?.Invoke(ev);
        OnWorldEventStarted?.Invoke(ev);
        Debug.Log($"WorldEvent {ev.eventName} started at {ev.centerPosition}, radius={ev.radius}, duration={ev.duration}.");
    }

    /// <summary>
    /// קבלת רשימת האירועים הפעילים
    /// </summary>
    public List<WorldEvent> GetActiveEvents()
    {
        return activeEvents;
    }

    /// <summary>
    /// דוגמה: יצירת אירוע "רעידת אדמה" שיוריד בריאות למיניפולים בטווח
    /// </summary>
    public void TriggerEarthquake(Vector3 pos, float radius, float duration, float damagePerSecond)
    {
        WorldEvent quake = new WorldEvent()
        {
            eventName = "Earthquake",
            duration = duration,
            centerPosition = pos,
            radius = radius,
            onStart = (e) => { Debug.Log("Earthquake begins!"); },
            onUpdate = (e, deltaTime) =>
            {
                // בכל פריים: לפגוע בבריאות מיניפולים בטווח
                Collider[] hits = Physics.OverlapSphere(e.centerPosition, e.radius);
                foreach (var c in hits)
                {
                    var b = c.GetComponent<MinipollBrain>();
                    if (b != null && b.IsAlive)
                    {
                        b.TakeDamage(damagePerSecond * deltaTime);
                    }
                }
            },
            onEnd = (e) => { Debug.Log("Earthquake ended!"); }
        };
        AddEvent(quake);
    }
}
