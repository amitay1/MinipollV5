/***************************************************************
 * GameManager (Advanced)
 *
 * תיאור כללי:
 *   - מנהל את מצב המשחק (GameState) באופן מורחב (Initializing, Playing, Paused, GameOver, וכו’)
 *   - מרכז שליטה בקצב המשחק (Time Scale / gameSpeed)
 *   - שמירה וטעינה של PlayerData בסיסית
 *   - מאפשר קריאות לאתחול/סיום משחק, חיבור למערכות אחרות (לדוגמה WorldManager, UIManager)
 *   - כולל אירועים כמו OnGameStateChanged, OnPlayerDataChanged
 *
 ***************************************************************/

using UnityEngine;
using System;
using System.IO;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance
    
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    var gmObj = new GameObject("GameManager_AutoCreated");
                    _instance = gmObj.AddComponent<GameManager>();
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
        else if (_instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
    }
    #endregion

    public enum GameState
    {
        Initializing,
        Playing,
        Paused,
        GameOver
    }

    [Header("Game State")]
    public GameState CurrentState = GameState.Initializing;
    public event Action<GameState> OnGameStateChanged;

    [Header("Game Speed / Time Scale")]
    [Range(0f, 10f)] public float gameSpeed = 1f;

    [Header("Player Data")]
    public PlayerData playerData = new PlayerData();
    public event Action<PlayerData> OnPlayerDataChanged;

    private string saveFilePath;

    private void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");

        // ברגע שהמשחק מתחיל, נטען נתונים (אופציונלי)
        LoadPlayerData();

        // נניח שמיד לאחר הסצנה הראשית, נתחיל לשחק
        SetGameState(GameState.Playing);
    }

    private void Update()
    {
        // עדכון Time.timeScale
        Time.timeScale = (CurrentState == GameState.Playing) ? gameSpeed : 0f;
    }

    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            SetGameState(GameState.Paused);
        else if (CurrentState == GameState.Paused)
            SetGameState(GameState.Playing);
    }

    public void TogglePauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            PauseGame();
        }
        else if (CurrentState == GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = gameSpeed;
    }

    public void EndGame()
    {
        SetGameState(GameState.GameOver);
        // אפשר להציג מסך סיום, תפריט ניקוד וכו’.
    }

    #region Save/Load Player Data
    [Serializable]
    public class PlayerData
    {
        public string playerName = "Player";
        public int playerScore = 0;
        public int playerLevel = 1;
        public int currentDay = 1;
        // אפשר להתרחב למטבעות, חפצים וכו’.
    }
    
    // Property to access current day
    public int currentDay 
    { 
        get { return playerData.currentDay; } 
        set { playerData.currentDay = value; OnPlayerDataChanged?.Invoke(playerData); } 
    }

    public void UpdatePlayerScore(int deltaScore)
    {
        playerData.playerScore += deltaScore;
        OnPlayerDataChanged?.Invoke(playerData);
    }

    public void SavePlayerData()
    {
        try
        {
            string json = JsonUtility.ToJson(playerData, prettyPrint: true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"GameManager: Player data saved to {saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed saving player data: " + e.Message);
        }
    }

    public void LoadPlayerData()
    {
        if (!File.Exists(saveFilePath)) return;
        try
        {
            string json = File.ReadAllText(saveFilePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
            OnPlayerDataChanged?.Invoke(playerData);
            Debug.Log($"GameManager: Player data loaded successfully. Welcome back, {playerData.playerName}!");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed loading player data: " + e.Message);
        }
    }
    #endregion
}
