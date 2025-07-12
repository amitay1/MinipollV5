using UnityEngine;
using System.Collections.Generic;
using MinipollGame.Managers;

namespace MinipollGame.Audio
{
    /// <summary>
    /// Audio pool manager for SFX management and pooling
    /// </summary>
    public class AudioPoolManager : MonoBehaviour
    {
        [Header("Pool Settings")]
        public int poolSize = 20;
        public AudioSource audioSourcePrefab;
        
        [Header("SFX Library")]
        public List<AudioClipData> audioClips = new List<AudioClipData>();
        
        [Header("Settings")]
        public float defaultVolume = 1f;
        public float defaultPitch = 1f;
        public bool autoReturnToPool = true;
        public float maxAudioSourceLifetime = 10f;
        
        private Queue<AudioSource> availableAudioSources = new Queue<AudioSource>();
        private List<AudioSource> activeAudioSources = new List<AudioSource>();
        private Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>();
        private AudioManager audioManager;

        [System.Serializable]
        public class AudioClipData
        {
            public string name;
            public AudioClip clip;
            public float volume = 1f;
            public float pitch = 1f;
            public bool randomizePitch = false;
            public Vector2 pitchRange = new Vector2(0.8f, 1.2f);
            public bool loop = false;
            public UnityEngine.Audio.AudioMixerGroup mixerGroup;
        }

        private void Awake()
        {
            audioManager = FindObjectOfType<AudioManager>();
            InitializePool();
            BuildClipDictionary();
        }

        /// <summary>
        /// Initialize the audio source pool
        /// </summary>
        private void InitializePool()
        {
            // Create pool parent if it doesn't exist
            Transform poolParent = transform;
            
            // Create audio sources for the pool
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource audioSource;
                
                if (audioSourcePrefab != null)
                {
                    audioSource = Instantiate(audioSourcePrefab, poolParent);
                }
                else
                {
                    // Create a basic audio source if no prefab is provided
                    GameObject audioSourceObject = new GameObject($"SFX_{i}");
                    audioSourceObject.transform.SetParent(poolParent);
                    audioSource = audioSourceObject.AddComponent<AudioSource>();
                    
                    // Setup default audio source settings
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f; // 2D by default
                }
                
                audioSource.gameObject.SetActive(false);
                availableAudioSources.Enqueue(audioSource);
            }
        }

        /// <summary>
        /// Build dictionary for quick clip lookup
        /// </summary>
        private void BuildClipDictionary()
        {
            clipDictionary.Clear();
            
            foreach (var clipData in audioClips)
            {
                if (clipData.clip != null && !string.IsNullOrEmpty(clipData.name))
                {
                    if (!clipDictionary.ContainsKey(clipData.name))
                    {
                        clipDictionary.Add(clipData.name, clipData.clip);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate audio clip name: {clipData.name}");
                    }
                }
            }
        }

        /// <summary>
        /// Play an audio clip by name
        /// </summary>
        public AudioSource PlayClip(string clipName, Vector3 position = default)
        {
            if (!clipDictionary.ContainsKey(clipName))
            {
                Debug.LogWarning($"Audio clip '{clipName}' not found in library!");
                return null;
            }

            return PlayClip(clipDictionary[clipName], position);
        }

        /// <summary>
        /// Play an audio clip directly
        /// </summary>
        public AudioSource PlayClip(AudioClip clip, Vector3 position = default)
        {
            if (clip == null)
            {
                Debug.LogWarning("Attempted to play null audio clip!");
                return null;
            }

            AudioSource audioSource = GetAudioSource();
            if (audioSource == null)
            {
                Debug.LogWarning("No available audio sources in pool!");
                return null;
            }

            // Find clip data for settings
            AudioClipData clipData = GetClipData(clip);
            
            SetupAudioSource(audioSource, clip, clipData, position);
            audioSource.Play();

            // Handle auto-return to pool
            if (autoReturnToPool && !clipData.loop)
            {
                StartCoroutine(ReturnToPoolAfterPlaying(audioSource, clip.length));
            }

            return audioSource;
        }

        /// <summary>
        /// Play audio clip with custom settings
        /// </summary>
        public AudioSource PlayClip(string clipName, Vector3 position, float volume, float pitch = 1f)
        {
            AudioSource audioSource = PlayClip(clipName, position);
            if (audioSource != null)
            {
                audioSource.volume = volume;
                audioSource.pitch = pitch;
            }
            return audioSource;
        }

        /// <summary>
        /// Play audio clip in 2D (no spatial audio)
        /// </summary>
        public AudioSource PlayClip2D(string clipName, float volume = 1f, float pitch = 1f)
        {
            AudioSource audioSource = PlayClip(clipName);
            if (audioSource != null)
            {
                audioSource.spatialBlend = 0f; // 2D
                audioSource.volume = volume;
                audioSource.pitch = pitch;
            }
            return audioSource;
        }

        /// <summary>
        /// Play audio clip in 3D (spatial audio)
        /// </summary>
        public AudioSource PlayClip3D(string clipName, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            AudioSource audioSource = PlayClip(clipName, position);
            if (audioSource != null)
            {
                audioSource.spatialBlend = 1f; // 3D
                audioSource.volume = volume;
                audioSource.pitch = pitch;
                audioSource.transform.position = position;
            }
            return audioSource;
        }

        /// <summary>
        /// Stop an audio source and return it to the pool
        /// </summary>
        public void StopAndReturnToPool(AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                ReturnToPool(audioSource);
            }
        }

        /// <summary>
        /// Stop all active audio sources
        /// </summary>
        public void StopAllSounds()
        {
            foreach (var audioSource in activeAudioSources.ToArray())
            {
                StopAndReturnToPool(audioSource);
            }
        }

        /// <summary>
        /// Get an available audio source from the pool
        /// </summary>
        private AudioSource GetAudioSource()
        {
            AudioSource audioSource;

            if (availableAudioSources.Count > 0)
            {
                audioSource = availableAudioSources.Dequeue();
            }
            else
            {
                // Pool exhausted, try to reclaim an inactive source
                audioSource = FindInactiveAudioSource();
                
                if (audioSource == null)
                {
                    // Create a new one if absolutely necessary
                    return CreateNewAudioSource();
                }
            }

            audioSource.gameObject.SetActive(true);
            activeAudioSources.Add(audioSource);
            return audioSource;
        }

        /// <summary>
        /// Return an audio source to the pool
        /// </summary>
        private void ReturnToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;

            audioSource.Stop();
            audioSource.clip = null;
            audioSource.gameObject.SetActive(false);
            
            if (activeAudioSources.Contains(audioSource))
            {
                activeAudioSources.Remove(audioSource);
                availableAudioSources.Enqueue(audioSource);
            }
        }

        /// <summary>
        /// Setup an audio source with clip and settings
        /// </summary>
        private void SetupAudioSource(AudioSource audioSource, AudioClip clip, AudioClipData clipData, Vector3 position)
        {
            audioSource.clip = clip;
            audioSource.volume = clipData != null ? clipData.volume * defaultVolume : defaultVolume;
            
            // Handle pitch
            if (clipData != null && clipData.randomizePitch)
            {
                audioSource.pitch = Random.Range(clipData.pitchRange.x, clipData.pitchRange.y);
            }
            else
            {
                audioSource.pitch = clipData != null ? clipData.pitch * defaultPitch : defaultPitch;
            }
            
            audioSource.loop = clipData != null ? clipData.loop : false;
            
            // Set mixer group if available
            if (clipData?.mixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = clipData.mixerGroup;
            }
            
            // Set position
            audioSource.transform.position = position;
        }

        /// <summary>
        /// Get clip data by audio clip
        /// </summary>
        private AudioClipData GetClipData(AudioClip clip)
        {
            return audioClips.Find(data => data.clip == clip);
        }

        /// <summary>
        /// Find an inactive audio source for reuse
        /// </summary>
        private AudioSource FindInactiveAudioSource()
        {
            for (int i = activeAudioSources.Count - 1; i >= 0; i--)
            {
                var audioSource = activeAudioSources[i];
                if (!audioSource.isPlaying)
                {
                    activeAudioSources.RemoveAt(i);
                    return audioSource;
                }
            }
            return null;
        }

        /// <summary>
        /// Create a new audio source when pool is exhausted
        /// </summary>
        private AudioSource CreateNewAudioSource()
        {
            GameObject audioSourceObject = new GameObject($"SFX_Extra_{Time.time}");
            audioSourceObject.transform.SetParent(transform);
            AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();
            
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            
            return audioSource;
        }

        /// <summary>
        /// Coroutine to return audio source to pool after playing
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolAfterPlaying(AudioSource audioSource, float clipLength)
        {
            float timeElapsed = 0f;
            
            while (timeElapsed < maxAudioSourceLifetime && audioSource.isPlaying)
            {
                yield return null;
                timeElapsed += Time.unscaledDeltaTime;
            }
            
            ReturnToPool(audioSource);
        }

        /// <summary>
        /// Add a new audio clip to the library at runtime
        /// </summary>
        public void AddClipToLibrary(string name, AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            AudioClipData newClipData = new AudioClipData
            {
                name = name,
                clip = clip,
                volume = volume,
                pitch = pitch
            };
            
            audioClips.Add(newClipData);
            
            if (!clipDictionary.ContainsKey(name))
            {
                clipDictionary.Add(name, clip);
            }
        }

        /// <summary>
        /// Get all active audio source count
        /// </summary>
        public int GetActiveAudioSourceCount()
        {
            return activeAudioSources.Count;
        }

        /// <summary>
        /// Get available audio source count
        /// </summary>
        public int GetAvailableAudioSourceCount()
        {
            return availableAudioSources.Count;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                BuildClipDictionary();
            }
        }
    }
}
