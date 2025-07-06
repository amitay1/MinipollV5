/***************************************************************
 *  MinipollCinematicSystem.cs
 *
 *  תיאור כללי:
 *    מערכת קטעי מעבר (Cutscenes/Cinematics) עבור אירועים חשובים במשחק:
 *      - נניח כשהמיניפול נהיה מנהיג שבט, כשהוא בונה מבנה ענק, או קרב אפי
 *      - מקפיאים שליטה, מזיזים מצלמה בצורה דרמטית, מנגנים אנימציות/סצנה
 *    מאפשר יצירת "CinematicSequence" המגדיר שלבים במצלמה, דיאלוג, אפקטים.
 *
 *  דרישות קדם:
 *    - מצלמה נפרדת או Camera.main שאפשר להזיז
 *    - מנגנון UI פשוט לתצוגת כתוביות
 ***************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MinipollCore;
[System.Serializable]
public class CinematicStep
{
    public float duration;                   // כמה זמן שלב זה נמשך
    public Vector3 cameraPositionOffset;     // לאן להזיז את המצלמה
    public Vector3 cameraLookAtOffset;       // על מה נסתכל
    public string subtitleText;              // טקסט להציג
}

[System.Serializable]
public class CinematicSequence
{
    public string sequenceName;
    public List<CinematicStep> steps = new List<CinematicStep>();
}

public class MinipollCinematicSystem : MonoBehaviour
{
    public Camera mainCam;
    public bool isPlaying = false;
    private CinematicSequence currentSeq;
    private int stepIndex = 0;
    private float stepTimer = 0f;
    private Vector3 camStartPos;
    private Vector3 lookStartPos;

    [Header("Subtitle UI (Optional)")]
    public UnityEngine.UI.Text subtitleUI; 
    public bool freezeGameDuringCinematic = true;

    private void Awake()
    {
        if (!mainCam)
        {
            mainCam = Camera.main;
        }
    }

    private void Update()
    {
        if (isPlaying && currentSeq != null)
        {
            UpdateCinematic(Time.deltaTime);
        }
    }

    public void PlayCinematic(CinematicSequence seq)
    {
        if (isPlaying)
        {
            Debug.LogWarning("Already playing a cinematic!");
            return;
        }
        currentSeq = seq;
        stepIndex = 0;
        stepTimer = 0f;
        isPlaying = true;
        camStartPos = mainCam.transform.position;
        lookStartPos = camStartPos + mainCam.transform.forward * 10f; // הערכה
        if (freezeGameDuringCinematic) Time.timeScale = 0f; // עוצרים משחק (מלבד Update UI)
        Debug.Log($"Started cinematic: {seq.sequenceName}");
    }

    private void UpdateCinematic(float dt)
    {
        if (stepIndex >= currentSeq.steps.Count)
        {
            // סיימנו
            StopCinematic();
            return;
        }
        CinematicStep step = currentSeq.steps[stepIndex];
        stepTimer += dt;

        // לזוז ממצב התחלתי למצב offset?
        float progress = stepTimer / step.duration;
        if (progress > 1f) progress = 1f;

        Vector3 targetPos = camStartPos + step.cameraPositionOffset;
        mainCam.transform.position = Vector3.Lerp(camStartPos, targetPos, progress);

        Vector3 targetLook = lookStartPos + step.cameraLookAtOffset;
        Vector3 lookPos = Vector3.Lerp(lookStartPos, targetLook, progress);
        mainCam.transform.LookAt(lookPos);

        // כתוביות
        if (subtitleUI)
            subtitleUI.text = step.subtitleText;

        if (stepTimer >= step.duration)
        {
            // שלב הסתיים
            stepIndex++;
            stepTimer = 0f;
            // נעדכן נקודת התחלה חדשה
            camStartPos = mainCam.transform.position;
            lookStartPos = lookPos;
        }
    }

    private void StopCinematic()
    {
        isPlaying = false;
        // משחררים
        if (freezeGameDuringCinematic) Time.timeScale = 1f;
        Debug.Log("Cinematic ended.");
        if (subtitleUI) subtitleUI.text = "";
        currentSeq = null;
    }
}
