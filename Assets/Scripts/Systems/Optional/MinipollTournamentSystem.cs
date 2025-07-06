/***************************************************************
 *  MinipollTournamentSystem.cs
 *
 *  תיאור כללי:
 *    מערכת "טורנירים" או תחרויות בין מיניפולים (או שבטים). 
 *      - מגדירים רשימת משתתפים
 *      - טורניר יכול להיות סגנון של קרב נוקאאוט, או מירוץ, או איסוף משאבים
 *      - המנצח זוכה בפרס, מציגים אותו
 *
 *  דרישות קדם:
 *    - MinipollManager זמין, למפות שמות למיניפולים
 *    - מנגנון קרב (BattleSystem) או אחר בהתאם לסוג טורניר
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using MinipollCore;
using MinipollGame.Managers;
[System.Serializable]
public enum TournamentType
{
    BattleRoyale,
    ResourceRace,
    SocialContest
}

[System.Serializable]
public class TournamentParticipant
{
    public string minipollName;
    public bool eliminated;
    public TournamentParticipant(string name)
    {
        minipollName = name;
        eliminated = false;
    }
}

[System.Serializable]
public class Tournament
{
    public string tournamentName;
    public TournamentType type;
    public List<TournamentParticipant> participants = new List<TournamentParticipant>();
    public bool inProgress;
    public bool finished;
    public string winner;
}

public class MinipollTournamentSystem : MonoBehaviour
{
    public static MinipollTournamentSystem Instance;

    [Header("Active Tournaments")]
    public List<Tournament> tournaments = new List<Tournament>();

    public MinipollManager minipollManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (!minipollManager) minipollManager = FindObjectOfType<MinipollManager>();
    }

    private void Update()
    {
        // נעדכן טורנירים בתהליך
        for (int i = 0; i < tournaments.Count; i++)
        {
            var t = tournaments[i];
            if (t.inProgress && !t.finished)
            {
                UpdateTournament(t);
            }
        }
    }

    /// <summary>
    /// יצירת טורניר חדש
    /// </summary>
    public Tournament CreateTournament(string name, TournamentType type, List<string> participantNames)
    {
        var t = new Tournament
        {
            tournamentName = name,
            type = type,
            inProgress = false,
            finished = false,
            winner = ""
        };
        foreach (var p in participantNames)
        {
            t.participants.Add(new TournamentParticipant(p));
        }
        tournaments.Add(t);
        return t;
    }

    public void StartTournament(Tournament t)
    {
        if (t.participants.Count < 2)
        {
            Debug.LogWarning("Need at least 2 participants to start a tournament!");
            return;
        }
        t.inProgress = true;
        Debug.Log($"Tournament {t.tournamentName} started with {t.participants.Count} participants!");
    }

    /// <summary>
    /// עדכון לוגיקת הטורניר - תלוי סוג
    /// </summary>
    private void UpdateTournament(Tournament t)
    {
        switch (t.type)
        {
            case TournamentType.BattleRoyale:
                UpdateBattleRoyale(t);
                break;
            case TournamentType.ResourceRace:
                UpdateResourceRace(t);
                break;
            case TournamentType.SocialContest:
                UpdateSocialContest(t);
                break;
        }
        // בדיקת סיום
        int remaining = 0;
        string candidate = "";
        foreach (var p in t.participants)
        {
            if (!p.eliminated)
            {
                remaining++;
                candidate = p.minipollName;
            }
        }
        if (remaining <= 1)
        {
            t.finished = true;
            t.winner = (remaining == 1) ? candidate : "No winner";
            Debug.Log($"Tournament {t.tournamentName} finished. Winner: {t.winner}");
        }
    }

    private void UpdateBattleRoyale(Tournament t)
    {
        // רעיון: אם שניים נפגשים בטווח קרוב => StartBattle...
        // או פשוט אקראית "מדמה" קרבות. כאן הדגמה פשטנית.
        foreach (var p in t.participants)
        {
            if (p.eliminated) continue;
            if (Random.value < 0.001f) 
            {
                p.eliminated = true;
                Debug.Log($"Participant {p.minipollName} was eliminated randomly in battle royale!");
            }
        }
    }

    private void UpdateResourceRace(Tournament t)
    {
        // משימה: איסוף X משאבים => מי שהגיע ראשון => השאר מודחים
        // כאן פשטות: אקראי מישהו שסיים
        if (Random.value < 0.002f)
        {
            // first found not eliminated => winner
            for (int i=0; i<t.participants.Count; i++)
            {
                if (!t.participants[i].eliminated)
                {
                    // מניחים שהוא הגיע ליעד
                    // שאר eliminated
                    for (int j=0; j<t.participants.Count; j++)
                    {
                        if (j != i) t.participants[j].eliminated = true;
                    }
                    Debug.Log($"ResourceRace: {t.participants[i].minipollName} finished first!");
                    break;
                }
            }
        }
    }

    private void UpdateSocialContest(Tournament t)
    {
        // תחרות "מי מתחבב על כולם"?
        // כאן רק דגימה: אקראית
        if (Random.value < 0.003f)
        {
            // מפל רנדום
            for (int i=0; i<t.participants.Count; i++)
            {
                if (!t.participants[i].eliminated)
                {
                    if (Random.value < 0.5f)
                    {
                        t.participants[i].eliminated = true;
                        Debug.Log($"SocialContest: {t.participants[i].minipollName} lost popularity!");
                    }
                }
            }
        }
    }
}
