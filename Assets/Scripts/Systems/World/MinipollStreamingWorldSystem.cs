/***************************************************************
 *  MinipollStreamingWorldSystem.cs
 *
 *  תיאור כללי:
 *    מנגנון טעינה דינמית של אזורי מפה (Chunks/Scenes):
 *      - כששחקן (או המצלמה) מתרחק מאזור, הסצנה/אובייקטים נפרקים מהזיכרון
 *      - כשמתקרבים לאזור חדש, הוא נטען “On Demand”
 *    מאפשר ליצור עולם עצום מבלי לטעון הכול בבת אחת.
 *
 *  דרישות קדם:
 *    - Scenes נפרדות או Assets המחולקים לפי אזורים
 *    - ב-Unity, נשתמש ב-SceneManager.LoadSceneAsync(..., LoadSceneMode.Additive)
 *      ו-UnloadSceneAsync
 ***************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class StreamingChunk
{
    public string chunkSceneName;   // שם הסצנה (או asset bundle)
    public Vector3 centerPosition;  // מיקום מרכז הגאוגרפי של ה-Chunk
    public float loadRadius;        // מרחק מהמצלמה/שחקן כדי לטעון
    public bool isLoaded;
}

public class MinipollStreamingWorldSystem : MonoBehaviour
{
    public static MinipollStreamingWorldSystem Instance;

    [Header("Chunks to Stream")]
    public List<StreamingChunk> streamingChunks = new List<StreamingChunk>();

    [Header("Reference")]
    public Transform playerOrCamera; // מה בודקים מרחק ממנו

    [Header("Check Interval")]
    public float checkInterval = 2f;
    private float timer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        if (!playerOrCamera) 
            playerOrCamera = Camera.main?.transform;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            UpdateStreamingChunks();
        }
    }

    private void UpdateStreamingChunks()
    {
        if (!playerOrCamera) return;

        Vector3 pos = playerOrCamera.position;
        foreach (var chunk in streamingChunks)
        {
            float dist = Vector3.Distance(pos, chunk.centerPosition);
            if (dist < chunk.loadRadius)
            {
                // אמור להיות טעון
                if (!chunk.isLoaded)
                {
                    StartCoroutine(LoadChunkScene(chunk));
                }
            }
            else
            {
                // אמור להיות לא טעון
                if (chunk.isLoaded)
                {
                    StartCoroutine(UnloadChunkScene(chunk));
                }
            }
        }
    }

    private IEnumerator LoadChunkScene(StreamingChunk chunk)
    {
        chunk.isLoaded = true;
        Debug.Log($"Loading chunk scene: {chunk.chunkSceneName}");
        AsyncOperation op = SceneManager.LoadSceneAsync(chunk.chunkSceneName, LoadSceneMode.Additive);
        yield return op;
        if (op.isDone)
        {
            Debug.Log($"Chunk scene {chunk.chunkSceneName} loaded successfully.");
        }
    }

    private IEnumerator UnloadChunkScene(StreamingChunk chunk)
    {
        chunk.isLoaded = false;
        Debug.Log($"Unloading chunk scene: {chunk.chunkSceneName}");
        AsyncOperation op = SceneManager.UnloadSceneAsync(chunk.chunkSceneName);
        yield return op;
        if (op.isDone)
        {
            Debug.Log($"Chunk scene {chunk.chunkSceneName} unloaded successfully.");
        }
    }

    internal object GetChunkAt(Vector3 position)
    {
        throw new NotImplementedException();
    }
}
