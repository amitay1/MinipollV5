/***************************************************************
 *  MinipollBlinkController.cs
 *
 *  תיאור כללי:
 *    סקריפט ספציפי לניהול העיניים (מצמוץ, סגירה/פתיחה).
 *    יכול להתלבש על אובייקט "Eyes" או על עין ספציפית.
 *    כאשר מגיע זמן מצמוץ, מנמיך שקיפות עפעף / משנה ספרייט
 *      למשך פריימים ספורים, ואז פותח שוב.
 *
 *  דרישות קדם:
 *    - למקם על GameObject "Eyes" או child של "Minipoll".
 *    - שיהיה בו SpriteRenderer נוסף או תמונה של עפעפיים (או מנגנון אחר).
 *    - אפשר גם לעבוד עם Animator (states: open, blink).
 *
 ***************************************************************/

using UnityEngine;
namespace MinipollGame.Controllers
{

    [RequireComponent(typeof(SpriteRenderer))]
    public class MinipollBlinkController : MonoBehaviour
    {
        private SpriteRenderer sr;
        [Header("Blink Interval")]
        [Tooltip("ממוצע זמן בין מצמוצים")]
        public float averageBlinkInterval = 4f;
        [Tooltip("רנדומליות בערך הזמן")]
        public float blinkIntervalRandomness = 1f;
        [Tooltip("משך המצמוץ בשניות")]
        public float blinkDuration = 0.1f;

        [Header("Blink Sprites")]
        public Sprite openEyesSprite;
        public Sprite closedEyesSprite;

        private float nextBlinkTime = 0f;
        private float blinkTimer = 0f;
        private bool isBlinking = false;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            // הגדר ספרייט עיניים פתוחות בהתחלה
            if (openEyesSprite)
                sr.sprite = openEyesSprite;
            ScheduleNextBlink();
        }

        private void Update()
        {
            if (!isBlinking)
            {
                // מחכים עד שמגיע זמן המצמוץ הבא
                if (Time.time >= nextBlinkTime)
                {
                    // מתחילים מצמוץ
                    isBlinking = true;
                    blinkTimer = 0f;
                    if (closedEyesSprite)
                        sr.sprite = closedEyesSprite;
                }
            }
            else
            {
                // אנחנו במצמוץ
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkDuration)
                {
                    // סיימנו מצמוץ
                    isBlinking = false;
                    // מחזירים עיניים פתוחות
                    if (openEyesSprite)
                        sr.sprite = openEyesSprite;
                    // בוחרים זמן חדש
                    ScheduleNextBlink();
                }
            }
        }

        private void ScheduleNextBlink()
        {
            // קובעים זמן אקראי הבא
            float interval = averageBlinkInterval + Random.Range(-blinkIntervalRandomness, blinkIntervalRandomness);
            if (interval < 1f) interval = 1f; // מניעה שלא יהיה קצר מדי
            nextBlinkTime = Time.time + interval;
        }

        /// <summary>
        /// מאפשר להכריח מצמוץ מיידי
        /// </summary>
        public void ForceBlink()
        {
            nextBlinkTime = Time.time;
        }
    }
}