/***************************************************************
 *  MinipollTribeSystem.cs
 *
 *  תיאור כללי:
 *    מודול ליצירת וניהול "שבט" או קבוצה של Minipolls. 
 *      - כל מיניפול יכול להיות חבר בשבט (אם joinedTribe != null)
 *      - שבט מתאפיין בשם, חברים, מנהיג (leader)
 *      - אפשר להצטרף/לעזוב, לבחור מנהיג באופן דינמי (למשל לפי הכי ותיק)
 *      - מונע חשיבה קבוצתית ברמה בסיסית: עזרה הדדית, שיתוף משאבים.
 *
 *  דרישות קדם:
 *    - ממוקם על המיניפול (כמו שאר הסקריפטים).
 *    - MinipollBrain מזהה ומפעיל UpdateTribe(deltaTime), או שתשמש selfUpdate.
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using MinipollGame.Core;
using MinipollGame.Systems;
namespace MinipollGame.Social
{

    public class MinipollTribeSystem : MonoBehaviour
    {
        private MinipollBrain brain;

        [Header("Tribe Membership")]
        public Tribe currentTribe;        // אם null => לא משויך
        public bool canJoinTribes = true; // אם false, יישאר בודד
        public int tribeId = -1;          // ID of the current tribe, -1 if no tribe

        [Header("Self Update")]
        public bool selfUpdate = false;

        private void Awake()
        {
            brain = GetComponent<MinipollBrain>();
        }

        private void Update()
        {
            if (selfUpdate)
            {
                UpdateTribe(Time.deltaTime);
            }
        }

        public void UpdateTribe(float deltaTime)
        {
            if (!canJoinTribes) return;

            // אם אנחנו לא בשבט, נשקול להצטרף לאחד
            if (currentTribe == null)
            {
                // נחפש שבט בסביבה? או ניצור חדש?
                // כאן דוגמה: אם יש שבט בקרבת radius => נצטרף.
                Tribe nearest = TribeManager.Instance?.FindNearestTribe(transform.position, 10f);
                if (nearest != null)
                {
                    JoinTribe(nearest);
                }
                else
                {
                    // אפשר להקים שבט חדש?
                    if (UnityEngine.Random.value < 0.001f * deltaTime)
                    {
                        CreateNewTribe("Tribe_" + brain.name);
                    }
                }
            }
            else
            {
                // כבר שייך לשבט. אפשר לעדכן מערכות ביחד, או לבדוק עזיבה
                // למשל אם fear גבוה מדי מחברי השבט, נעזוב
                var social = brain.GetSocialSystem();
                if (social != null)
                {
                    float averageFear = currentTribe.CalculateAvgFearToward(brain);
                    if (averageFear > 70f)
                    {
                        LeaveTribe();
                    }
                }
            }
        }

        public void JoinTribe(Tribe tribe)
        {
            if (currentTribe != null)
            {
                LeaveTribe();
            }
            tribe.AddMember(this);
            currentTribe = tribe;
            Debug.Log($"{brain.name} joined tribe {tribe.tribeName}");
        }

        public void LeaveTribe()
        {
            if (currentTribe == null) return;
            currentTribe.RemoveMember(this);
            Debug.Log($"{brain.name} left tribe {currentTribe.tribeName}");
            currentTribe = null;
        }
        public void CreateNewTribe(string tribeName)
        {
            if (TribeManager.Instance == null) return;
            Tribe newTribe = TribeManager.Instance.CreateTribe(tribeName, this);
            JoinTribe(newTribe);
        }

        public Tribe GetTribe()
        {
            return currentTribe;
        }
    }

    /***************************************************************
     *  Tribe.cs
     *  מבנה נתונים מייצג שבט
     ***************************************************************/
    [System.Serializable]
    public class Tribe
    {
        public string tribeName;
        public MinipollTribeSystem leader;
        public List<MinipollTribeSystem> members = new List<MinipollTribeSystem>();

        public Tribe(string name, MinipollTribeSystem founder)
        {
            tribeName = name;
            leader = founder;
            members.Add(founder);
        }

        public void AddMember(MinipollTribeSystem m)
        {
            if (!members.Contains(m))
            {
                members.Add(m);
                // אולי מעדכן leader וכו’
            }
        }

        public void RemoveMember(MinipollTribeSystem m)
        {
            if (members.Contains(m))
            {
                members.Remove(m);
                if (m == leader && members.Count > 0)
                {
                    leader = members[0]; // בחירת מנהיג חדש כפשרה פשוטה
                }
            }
        }

        public float CalculateAvgFearToward(MinipollBrain target)
        {
            float total = 0f;
            int count = 0;
            foreach (MinipollTribeSystem mem in members)
            {
                MinipollBrain brain = mem.GetComponent<MinipollBrain>();
                var social = brain?.GetSocialSystem();
                if (social != null)
                {
                    // var rel = social.GetRelationship(target);
                    // if (rel != null)
                    // {
                    //     total += rel.fear;
                    //     count++;
                    // }
                }
            }
            if (count == 0) return 0f;
            return total / count;
        }
    }

    /***************************************************************
     *  TribeManager.cs
     *  מנהל יחיד שמטפל בכל השבטים
     ***************************************************************/
    public class TribeManager : MonoBehaviour
    {
        public static TribeManager Instance;
        public List<Tribe> allTribes = new List<Tribe>();

        private void Awake()
        {
            Instance = this;
        }

        public Tribe CreateTribe(string tribeName, MinipollTribeSystem founder)
        {
            Tribe t = new Tribe(tribeName, founder);
            allTribes.Add(t);
            return t;
        }

        public Tribe FindNearestTribe(Vector3 pos, float radius)
        {
            // דוגמה מפושטת: מחפשים שבט שיש בו חבר אחד לפחות, שמרחקו <= radius
            Tribe nearest = null;
            float minDist = float.MaxValue;
            foreach (var t in allTribes)
            {
                foreach (var mem in t.members)
                {
                    float dist = Vector3.Distance(pos, mem.transform.position);
                    if (dist < radius && dist < minDist)
                    {
                        minDist = dist;
                        nearest = t;
                    }
                }
            }
            return nearest;
        }
    }
}