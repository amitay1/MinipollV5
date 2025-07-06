/***************************************************************
 *  MinipollManager.cs
 *  
 *  תיאור כללי:
 *    Singleton שמנהל את כל המיניפולים במשחק:
 *      - ריכוז רשימה של Minipoll פעילים
 *      - ספאון ראשוני וספאון דינמי (אם צריך)
 *      - מחיקת מיניפולים (אם הם מתים או נעלמים)
 *      - עדכון מידע גלובלי (למשל, מספרם) לתצוגה ב-UI
 *      - אירועים (OnMinipollSpawned, OnMinipollRemoved)
 *
 *  דרישות קדם:
 *    - להניח את הסקריפט הזה על אובייקט בשם "MinipollManager"
 *    - GameObject עם Collider/Renderer שמתפקד כ"MinipollContainer" 
 *      (או שימוש ב-transform אחר כ-Parent), לפי הצורך.
 *    - Prefab של Minipoll (עם כל הסקריפטים: MinipollBrain.cs, וכו’)
 *
 *  שימוש:
 *    1) הגדרת Minipoll Prefab ב-Inspector
 *    2) הגדרת Minipoll Container (Transform ריק בסצנה) כיעד לקיבוץ
 *    3) הגדרת כמות התחלתית וכו’ * ***************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

 namespace MinipollGame.Managers
{
public class MinipollManager : MonoBehaviour
{
    #region Singleton
    private static MinipollManager _instance;
    public static MinipollManager Instance
    {        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<MinipollManager>();
                if (_instance == null)
                {
                    // אין בסצנה => ניצור אחד
                    GameObject mm = new GameObject("MinipollManager_AutoCreated");
                    _instance = mm.AddComponent<MinipollManager>();
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
    public GameManager gameManager;          // לא חובה, מחברים אם רוצים
    public Transform minipollContainer;      // המיכל בו נניח את כל המיניפולים
    public GameObject minipollPrefab;        // הפריפאב של מיניפול

    [Header("Initial Spawn Settings")]
    [Tooltip("כמה מיניפולים ליצור עם תחילת המשחק")]
    public int initialSpawnCount = 5;
    [Tooltip("אזור רנדומלי שבו נמקם אותם בהתחלה")]
    public Vector3 spawnCenter = Vector3.zero;
    public float spawnRange = 10f;

    [Header("Dynamic Spawn Settings")]
    [Tooltip("האם לאפשר ספאון דינמי במהלך המשחק?")]
    public bool allowAutoSpawn = false;
    [Tooltip("כל כמה שניות לבצע ניסיון ספאון נוסף?")]
    public float spawnInterval = 60f;
    private float spawnTimer = 0f;

    [Tooltip("כמות מקסימלית של מיניפולים בעולם (0=ללא הגבלה)")]
    public int maxMinipolls = 20;    // רשימה גלובלית עם המיניפולים הפעילים
    private List<MinipollClass> activeMinipolls = new List<MinipollClass>();

    // אירועים למי שרוצה להתעדכן
    public event Action<MinipollClass> OnMinipollSpawned;
    public event Action<MinipollClass> OnMinipollRemoved;

    private void Start()
    {
        // אם לא הוגדר GameManager, נחפש לבד
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        // אם לא הוגדר מיכל למיניפולים, ניצור אחד
        if (minipollContainer == null)
        {
            GameObject containerGO = new GameObject("MinipollContainer_AutoCreated");
            minipollContainer = containerGO.transform;
        }

        // ספאון ראשוני
        for (int i = 0; i < initialSpawnCount; i++)
        {
            Vector3 pos = GetRandomSpawnPosition();
            SpawnMinipoll(pos);
        }
    }

    private void Update()
    {
        // אם המשחק לא במצב Playing, לא עושים כלום
        if (gameManager != null && gameManager.CurrentState != GameManager.GameState.Playing)
            return;

        // ספאון דינמי
        if (allowAutoSpawn && (maxMinipolls == 0 || activeMinipolls.Count < maxMinipolls))
        {
            spawnTimer += Time.deltaTime * gameManager.gameSpeed; 
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                Vector3 pos = GetRandomSpawnPosition();
                SpawnMinipoll(pos);
            }
        }
    }    /// <summary>
    /// פונקציית ספאון: יוצרת MinipollClass חדש, ממקמת ומאתחלת
    /// </summary>
    public MinipollClass SpawnMinipoll(Vector3 position)
    {
        // בדיקה אם עברנו את המקסימום
        if (maxMinipolls > 0 && activeMinipolls.Count >= maxMinipolls)
        {
            Debug.LogWarning("Cannot spawn more Minipolls: reached max limit");
            return null;
        }

        if (!minipollPrefab)
        {
            Debug.LogError("No Minipoll prefab assigned to MinipollManager!");
            return null;
        }
        
        // יוצרים GameObject חדש מסודר
        GameObject newObj = Instantiate(minipollPrefab, position, Quaternion.identity, minipollContainer);
        newObj.name = "Minipoll_" + (activeMinipolls.Count + 1);
        
        // Register with NEEDSIM system if NEEDSIMNode component exists
        // var needsimNode = newObj.GetComponent<NEEDSIM.NEEDSIMNode>();
        // if (needsimNode != null && NEEDSIM.NEEDSIMRoot.Instance != null)
        // {
        //     NEEDSIM.NEEDSIMRoot.Instance.AddNEEDSIMNode(needsimNode);
        //     needsimNode.Setup();
        //     Debug.Log($"[MinipollManager] Registered minipoll with NEEDSIM system: {newObj.name}");
        // }
        
        // Create MinipollClass wrapper from actual components
        MinipollClass mp = new MinipollClass();
        mp.gameObject = newObj;
        mp.transform = newObj.transform;
        mp.name = newObj.name;
        
        // Try to get health from MinipollCore or Health component
        var minipollCore = newObj.GetComponent<MinipollGame.Core.MinipollCore>();
        if (minipollCore != null)
        {
            mp.health = minipollCore.Health?.CurrentHealth ?? 100f;
        }
        else
        {
            var healthComponent = newObj.GetComponent<MinipollGame.Core.MinipollHealth>();
            mp.health = healthComponent?.CurrentHealth ?? 100f;
        }

        // Add to our list and notify
        activeMinipolls.Add(mp);

        // מאזינים לאירועים... (נניח שיש בפנים OnDeath)
        // mp.OnDeath += () => { RemoveMinipoll(mp); };

        // שולחים אירוע
        OnMinipollSpawned?.Invoke(mp);

        return mp;
    }    /// <summary>
    /// כאשר מיניפול מת או נמחק, נקרא לפונקציה זו כדי להסירו מהרשימה ולהרוס האובייקט
    /// </summary>
    public void RemoveMinipoll(MinipollClass mp)
    {
        if (mp == null) return;
        if (activeMinipolls.Contains(mp))
        {
            activeMinipolls.Remove(mp);
        }
        // שולחים אירוע למאזינים
        OnMinipollRemoved?.Invoke(mp);
        // הורסים מהסצנה
        Destroy(mp.gameObject);
    }    /// <summary>
    /// Register a minipoll with the manager
    /// </summary>
    public void RegisterMinipoll(MinipollGame.Core.MinipollCore minipoll)
    {
        if (minipoll == null) return;
        
        // Add to our tracking (convert to MinipollClass for compatibility)
        var mp = new MinipollClass { gameObject = minipoll.gameObject, name = minipoll.Name };
        if (!activeMinipolls.Contains(mp))
        {
            activeMinipolls.Add(mp);
            OnMinipollSpawned?.Invoke(mp);
        }
        
        Debug.Log($"[MinipollManager] Registered minipoll: {minipoll.Name}");
    }

    /// <summary>
    /// Unregister a minipoll from the manager
    /// </summary>
    public void UnregisterMinipoll(MinipollGame.Core.MinipollCore minipoll)
    {
        if (minipoll == null) return;
          // Find and remove from our tracking
        var mp = activeMinipolls.Find(m => ReferenceEquals(m.gameObject, minipoll.gameObject));
        if (mp != null)
        {
            activeMinipolls.Remove(mp);
            OnMinipollRemoved?.Invoke(mp);
        }
        
        Debug.Log($"[MinipollManager] Unregistered minipoll: {minipoll.Name}");
    }

    /// <summary>
    /// Whether to use corpse pooling for dead minipolls
    /// </summary>
    public bool UseCorpsePool { get; set; } = false;

    /// <summary>
    /// Move a dead minipoll to the corpse pool instead of destroying it
    /// </summary>
    public void MoveToCorpsePool(MinipollGame.Core.MinipollCore minipoll)
    {
        if (minipoll == null) return;
        
        // For now, just disable the minipoll and move it to a "dead" area
        // UnregisterMinipoll(minipoll);
        
        // TODO: Implement actual corpse pooling
        minipoll.gameObject.SetActive(false);
        
        Debug.Log($"[MinipollManager] Moved {minipoll.Name} to corpse pool");
    }

    private void Destroy(object gameObject)
    {
        UnityEngine.Object.Destroy((UnityEngine.Object)gameObject);
    }

        /// <summary>
        /// מחזיר רשימת רפרנסים למיניפולים הפעילים כרגע
        /// </summary>
        public List<MinipollClass> GetAllMinipolls()
    {
        return activeMinipolls;
    }    /// <summary>
    /// מקבל MinipollClass ספציפי לפי שם (או מזהה), מחזיר null אם לא נמצא.
    /// </summary>
    public MinipollClass FindMinipollByName(string minipollName)
    {
        return activeMinipolls.Find(m => m.name == minipollName);
    }

    /// <summary>
    /// מחשב נקודה רנדומלית סביב spawnCenter במרחק spawnRange
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRange;
        Vector3 pos = spawnCenter + new Vector3(randomCircle.x, 0f, randomCircle.y);
        return pos;
    }

    internal GameObject GetMinipollById(int minipollId)
    {
        throw new NotImplementedException();
    }

        internal void RecordDeath(Core.MinipollCore core, GameObject killer)
        {
            throw new NotImplementedException();
        }

        // internal void UnregisterMinipoll(Core.MinipollCore minipollCore)
        // {
        //     throw new NotImplementedException();
        // }

        // internal void RegisterMinipoll(Core.MinipollCore minipollCore)
        // {
        //     throw new NotImplementedException();
        // }

        // internal void MoveToCorpsePool(MinipollCore minipollCore)
        // {
        //     throw new NotImplementedException();
        // }

        // internal void RecordDeath(MinipollCore core, GameObject killer)
        // {
        //     throw new NotImplementedException();
        // }

        // internal void RecordDeath(MinipollGame.Core.MinipollCore core, GameObject killer)
        // {
        //     throw new NotImplementedException();
        // }

        // internal void RegisterMinipoll(MinipollGame.Core.MinipollCore minipollCore)
        // {
        //     throw new NotImplementedException();
        // }

        // internal void UnregisterMinipoll(MinipollGame.Core.MinipollCore minipollCore)
        // {
        //     throw new NotImplementedException();
        // }

        // internal void MoveToCorpsePool(MinipollGame.Core.MinipollCore minipollCore)
        // {
        //     throw new NotImplementedException();
        // }
    }

    public class MinipollClass
    {
        internal string name;
        internal object gameObject;
        internal object transform;
        internal float health;

        internal T GetComponent<T>()
        {
            throw new NotImplementedException();
        }
    }
}