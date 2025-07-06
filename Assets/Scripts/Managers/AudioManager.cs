/***************************************************************
 *  AudioManager.cs
 *  
 *  תיאור כללי:
 *    Singleton המרכז את השמע במשחק (Music, SFX, Voice ועוד).
 *    1) יכולת לנגן רקע (BGM) בלולאה.
 *    2) נגן אפקטים קוליים שונים (Jump, Click, Explosion וכו’).
 *    3) שליטה בגלובל ווליום/עוצמת מוזיקה/אפקטים.
 *    4) אפשרות לעשות Fade In/Out למוזיקה.
 *
 *  דרישות קדם:
 *    - לשים על GameObject אחד בסצנה.
 *    - שני AudioSources (או יותר) – אחד למוזיקה, אחד ל-SFX.
 *    - ניתן להרחיב לקבוצות אודיו (voice, ambience) אם תרצה בעתיד.
 *
 ***************************************************************/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                if (_instance == null)
                {
                    GameObject am = new GameObject("AudioManager_AutoCreated");
                    _instance = am.AddComponent<AudioManager>();
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

        // נכין AudioSources בסיסיים אם אין
        PrepareAudioSources();
    }
    #endregion

    [Header("Audio Source References")]
    public AudioSource musicSource;   // למוזיקת רקע
    public AudioSource sfxSource;     // לאפקטים
    [Range(0f,1f)]
    public float masterVolume = 1f;
    [Range(0f,1f)]
    public float musicVolume = 1f;
    [Range(0f,1f)]
    public float sfxVolume = 1f;

    [Header("Current BGM Info")]
    public AudioClip currentMusic;
    public bool loopMusic = true;

    // רשימת אפקטים להדגמה (אופציונלי)
    [Serializable]
    public class SFXClip
    {
        public string sfxName;
        public AudioClip clip;
    }
    [Header("SFX Library")]
    public List<SFXClip> sfxClips = new List<SFXClip>();

    #region Unity Methods

    private void Start()
    {
        // נגן מוזיקה התחלתית אם הוגדרה
        if (currentMusic)
        {
            PlayMusic(currentMusic, loopMusic);
        }
        ApplyVolumeSettings();
    }

    private void Update()
    {
        ApplyVolumeSettings();
    }

    #endregion

    #region Volume & AudioSources Setup

    private void PrepareAudioSources()
    {
        if (!musicSource)
        {
            GameObject musGO = new GameObject("MusicSource");
            musGO.transform.SetParent(this.transform);
            musicSource = musGO.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (!sfxSource)
        {
            GameObject sfxGO = new GameObject("SFXSource");
            sfxGO.transform.SetParent(this.transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }
    }

    private void ApplyVolumeSettings()
    {
        // מאחדים עוצמות: Master, Music, SFX
        if (musicSource)
        {
            musicSource.volume = masterVolume * musicVolume;
        }
        if (sfxSource)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }
    }

    #endregion

    #region Music Functions

    /// <summary>
    /// מנגן מוזיקה חדשה באופן מיידי
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!musicSource) return;
        if (clip == null) return;
        currentMusic = clip;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    /// <summary>
    /// מבצע Fade Out למוזיקה הנוכחית ואז מנגן מוזיקה חדשה (או משתיק)
    /// </summary>
    public void SwitchMusicWithFade(AudioClip newClip, float fadeTime = 2f, bool loop = true)
    {
        StartCoroutine(SwitchMusicRoutine(newClip, fadeTime, loop));
    }

    private IEnumerator SwitchMusicRoutine(AudioClip newClip, float fadeTime, bool loop)
    {
        if (!musicSource) yield break;

        float startVol = musicSource.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float factor = 1f - (t / fadeTime);
            musicSource.volume = startVol * factor;
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = startVol;

        // מנגן את הקליפ החדש (אם קיים) או משאיר מושתק
        if (newClip)
        {
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    #endregion

    #region SFX Functions

    /// <summary>
    /// מנגן אפקט בודד (OneShot) בעוצמת Volume הנוכחית
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (!sfxSource || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    /// <summary>
    /// מחפש ב-SFXClips לפי שם, ומשמיע (OneShot).
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        var sfx = sfxClips.Find(s => s.sfxName == sfxName);
        if (sfx != null && sfx.clip != null)
        {
            PlaySFX(sfx.clip);
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxName}' not found in AudioManager!");
        }
    }

    /// <summary>
    /// מנגן אפקט Looping נפרד (עם AudioSource חדש)
    /// (למשל רעש רקע, אפקט מנוע, וכו')
    /// </summary>
    public AudioSource PlayLoopingSFX(AudioClip clip)
    {
        if (!clip) return null;
        // ניצור אובייקט זמני עם AudioSource
        GameObject loopObj = new GameObject($"LoopingSFX_{clip.name}");
        loopObj.transform.SetParent(this.transform);

        AudioSource src = loopObj.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.volume = masterVolume * sfxVolume;
        src.Play();

        return src;
    }

    /// <summary>
    /// מפסיק AudioSource לופינג שהוחזר מ-PlayLoopingSFX
    /// </summary>
    public void StopLoopingSFX(AudioSource loopingSource)
    {
        if (!loopingSource) return;
        loopingSource.Stop();
        Destroy(loopingSource.gameObject);
    }

    #endregion
}
