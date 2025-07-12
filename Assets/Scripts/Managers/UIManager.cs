/***************************************************************
 *  UIManager.cs
 *  
 *  תיאור כללי:
 *    מנהל יחיד במערכת ה-UI של המשחק:
 *      - מחובר ל-Canvas בסצנה, מכיל רפרנסים ל-TMP/Text/Sliders/Panels
 *      - מציג מידע על זמן, מזג אוויר, כמות מיניפולים
 *      - מאפשר שליטה במשחק (כגון מהירות משחק, Pause Menu)
 *      - תצוגת פרטי Minipoll נבחר (אם יש בחירה)
 *
 *  דרישות קדם:
 *    - למקם על אובייקט "UIManager" (אפשר גם על Canvas)
 *    - לצרף רכיבי UI (Text, Slider, Panels) בכיוון שהגדרנו בהיררכיה
 *    - לנהל אירועי OnMinipollSpawned/Removed ולהתעדכן בשינויים
 *
 ***************************************************************/

using UnityEngine;
using UnityEngine.UI;   // עבור Slider, Button, Image...
using TMPro;           // אם משתמשים ב-TextMeshPro
using UnityEngine.InputSystem;
using System;
using MinipollCore;
using MinipollGame.Systems;


namespace MinipollGame.Managers
{
    // Define EmotionStatus class to match the emotion system structure
    [System.Serializable]
    public class EmotionStatus
    {
        // public MinipollCore.MinipollEmotionsSystem.EmotionType emotion;
        public float intensity;

        // public EmotionStatus(MinipollCore.MinipollEmotionsSystem.EmotionType emotion, float intensity)
        // {
        //     this.emotion = emotion;
        //     this.intensity = intensity;
        // }
    }

    public class UIManager : MonoBehaviour
    {
        #region Singleton (אופציונלי)
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIManager>();
                    if (_instance == null)
                    {
                        GameObject uiObj = new GameObject("UIManager_AutoCreated");
                        _instance = uiObj.AddComponent<UIManager>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                if (_instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
        }
        #endregion

        [Header("References")]
        public GameManager gameManager;            // קישור ל-GameManager
        public WorldManager worldManager;          // קישור ל-WorldManager
        public MinipollManager minipollManager;    // קישור ל-MinipollManager

        [Header("HUD Elements")]
        public TextMeshProUGUI dayText;
        public TextMeshProUGUI weatherText;
        public TextMeshProUGUI minipollCountText;
        public Slider gameSpeedSlider;
        public TextMeshProUGUI gameSpeedLabel;

        [Header("Time Display Elements")]
        public TextMeshProUGUI timeOfDayText;

        [Header("Pause Menu")]
        public GameObject pauseMenuPanel;
        public GameObject pauseMenu; // For PauseMenuUI component reference

        [Header("Selection Panel")]  
        public GameObject selectionPanel; // For MinipollSelectionPanel component reference

        [Header("Minipoll Info Panel")]
        public GameObject minipollInfoPanel;
        public TextMeshProUGUI selectedMinipollName;
        public Slider emotionIntensitySlider;
        public TextMeshProUGUI emotionText;
        public Transform relationshipsContainer;  // למשל, נציג חברויות
        public Transform experiencesContainer;    // למשל, רשימת זיכרונות

        // משתנה לזכור מיניפול "נבחר" אם יש
        private MinipollGame.Core.MinipollCore selectedMinipoll = null;

        private void Start()
        {
            // אם לא הוגדר, מחפשים אוטומטית
            if (gameManager == null) gameManager = GameManager.Instance;
            if (worldManager == null) worldManager = WorldManager.Instance;
            if (minipollManager == null) minipollManager = MinipollManager.Instance;

            // נרשמים לאירועים רלוונטיים
            if (minipollManager != null)
            {
                minipollManager.OnMinipollSpawned += OnMinipollSpawned;
                minipollManager.OnMinipollRemoved += OnMinipollRemoved;
            }

            if (worldManager != null)
            {
                worldManager.timeSystem.OnNewDay += OnNewDay;
                worldManager.timeSystem.OnSeasonChanged += OnSeasonChanged;
                worldManager.weatherSystem.OnWeatherChanged += OnWeatherChanged;
            }

            // עדכון ראשוני
            UpdateMinipollCountUI();
            UpdateGameSpeedUI();
            if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
            if (minipollInfoPanel) minipollInfoPanel.SetActive(false);
        }        private void OnMinipollRemoved(MinipollClass @class)
        {
            UpdateMinipollCountUI();
        }

        private void OnMinipollSpawned(MinipollClass @class)
        {
            UpdateMinipollCountUI();
        }

        private void OnMinipollSpawned(MinipollGame.Core.MinipollCore core)
        {
            UpdateMinipollCountUI();
        }

        private void OnMinipollRemoved(MinipollGame.Core.MinipollCore core)
        {
            // אם המחוק היה הנבחר, נבטל את הבחירה
            if (selectedMinipoll == core)
            {
                selectedMinipoll = null;
                if (minipollInfoPanel) minipollInfoPanel.SetActive(false);
            }
            UpdateMinipollCountUI();
        }

        private void Update()
        {
            // אם משחק ב-Paused, מציגים תפריט עצור
            if (gameManager != null && gameManager.CurrentState == GameManager.GameState.Paused)
            {
                if (pauseMenuPanel && !pauseMenuPanel.activeInHierarchy)
                    pauseMenuPanel.SetActive(true);
            }
            else
            {
                if (pauseMenuPanel && pauseMenuPanel.activeInHierarchy)
                    pauseMenuPanel.SetActive(false);
            }

            // עדכון שוטף של תצוגת זמן ומזג
            UpdateTimeOfDayUI();
            // אם תרצה, אפשר להציג גם Season וכו’

            // בדיקת לחיצת עכבר כדי לבחור Minipoll? - עם Input System החדש
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    MinipollGame.Core.MinipollCore maybeMinipoll = hit.collider.GetComponent<MinipollGame.Core.MinipollCore>();
                    if (maybeMinipoll != null)
                    {
                        SelectMinipoll(maybeMinipoll);
                    }
                }
            }

            // אם יש Minipoll נבחר, נעדכן את הפאנל
            if (selectedMinipoll)
            {
                UpdateSelectedMinipollPanel();
            }
        }
        private void SelectMinipoll(MinipollGame.Core.MinipollCore maybeMinipoll)
        {
            selectedMinipoll = maybeMinipoll;
            if (minipollInfoPanel)
            {
                minipollInfoPanel.SetActive(true);
                UpdateSelectedMinipollPanel();
            }
        }

        private void UpdateSelectedMinipollPanel()
        {
            if (!selectedMinipoll || !minipollInfoPanel) return;

            // Update name
            if (selectedMinipollName)
                selectedMinipollName.text = selectedMinipoll.Name;

            // Update emotion display
            if (selectedMinipoll.Emotions && emotionText && emotionIntensitySlider)
            {
                var currentEmotion = selectedMinipoll.Emotions.GetDominantEmotion();
                emotionText.text = currentEmotion.emotion.ToString();
                emotionIntensitySlider.value = currentEmotion.intensity;
            }
        }

        /// <summary>
        /// Show the minipoll panel when a minipoll is selected
        /// </summary>
        public void ShowMinipollPanel(MinipollGame.Core.MinipollCore minipoll)
        {
            SelectMinipoll(minipoll);
        }

        /// <summary>
        /// Hide the minipoll panel when no minipoll is selected
        /// </summary>
        public void HideMinipollPanel()
        {
            selectedMinipoll = null;
            if (minipollInfoPanel)
                minipollInfoPanel.SetActive(false);
        }

        /// <summary>
        /// Update the health display for the selected minipoll
        /// </summary>
        public void UpdateSelectedMinipollHealth(float healthRatio)
        {
            // This could update a health bar if we had one
            // For now, we'll just store the information
            if (selectedMinipoll)
            {
                // Could update UI health bar here
                Debug.Log($"Health updated for {selectedMinipoll.Name}: {healthRatio:P0}");
            }
        }

        #region Minipoll Events

        private void UpdateMinipollCountUI()
        {
            if (minipollCountText != null && minipollManager != null)
            {
                int count = minipollManager.GetAllMinipolls().Count;
                minipollCountText.text = $"Minipolls: {count}";
            }
        }

        #endregion

        #region Time / Weather UI Updates

        private void OnNewDay(int dayCount)
        {
            if (dayText != null)
            {
                dayText.text = $"Day: {dayCount}";
            }
        }

        private void OnSeasonChanged(WorldManager.TimeSystem.Season newSeason)
        {
            // אפשר להוסיף ל-UI טקסט עונה, או לשלב בweatherText
        }

        private void OnWeatherChanged(WorldManager.WeatherSystem.WeatherType newWeather)
        {
            if (weatherText != null)
            {
                weatherText.text = $"Weather: {newWeather}";
            }
        }

        private void UpdateTimeOfDayUI()
        {
            if (worldManager == null || worldManager.timeSystem == null) return;
            if (timeOfDayText == null) return;

            // נניח שהtimeSystem.currentTime01=0 => שחר (06:00),
            // 0.5 => 18:00, 1 => שוב 06:00
            // רק לצורך הדגמה - נוסחה פשוטה

            float day01 = worldManager.timeSystem.currentTime01;
            // ממירים day01 ל-24 שעות
            float hourF = 6f + day01 * 12f;  // 0=6:00, 0.5=12 שעות אחרי=18:00, 1=6:00 למחרת
            if (hourF >= 24f) hourF -= 24f;

            int hour = Mathf.FloorToInt(hourF);
            int minute = Mathf.FloorToInt((hourF - hour) * 60f);

            timeOfDayText.text = $"{hour:D2}:{minute:D2}";
        }

        #endregion

        #region Game Speed / Pause Menu

        public void OnGameSpeedSliderChanged(float newValue)
        {
            if (gameManager == null) return;
            gameManager.gameSpeed = newValue;
            UpdateGameSpeedUI();
        }

        private void UpdateGameSpeedUI()
        {
            if (gameManager == null) return;
            
            if (gameSpeedSlider != null)
            {
                gameSpeedSlider.value = gameManager.gameSpeed;
            }
            if (gameSpeedLabel != null)
            {
                gameSpeedLabel.text = $"Speed: {gameManager.gameSpeed:F1}x";
            }
        }

        public void OnPauseButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.TogglePauseGame();
            }
        }

        // internal void ShowMinipollPanel(MinipollGame.Core.MinipollCore minipollCore)
        // {
        //     throw new NotImplementedException();
        // }

        // internal void ShowMinipollPanel(MinipollGame.Core.MinipollCore minipollCore)
        // {
        //     throw new NotImplementedException();
        // }

        #endregion


    }
}