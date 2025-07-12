using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using MinipollGame.Managers;

namespace MinipollGame.UI.Menus
{
    /// <summary>
    /// Settings menu UI controller
    /// </summary>
    public class SettingsMenuUI : MonoBehaviour
    {
        [Header("Audio Settings")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider ambientVolumeSlider;
        public TextMeshProUGUI masterVolumeText;
        public TextMeshProUGUI musicVolumeText;
        public TextMeshProUGUI sfxVolumeText;
        public TextMeshProUGUI ambientVolumeText;
        
        [Header("Graphics Settings")]
        public TMP_Dropdown qualityDropdown;
        public TMP_Dropdown resolutionDropdown;
        public Toggle fullscreenToggle;
        public Toggle vsyncToggle;
        public Slider fpsLimitSlider;
        public TextMeshProUGUI fpsLimitText;
        
        [Header("Gameplay Settings")]
        public Slider gameSpeedSlider;
        public TextMeshProUGUI gameSpeedText;
        public Toggle autosaveToggle;
        public Slider autosaveIntervalSlider;
        public TextMeshProUGUI autosaveIntervalText;
        public Toggle tutorialToggle;
        
        [Header("Controls")]
        public Slider mouseSensitivitySlider;
        public TextMeshProUGUI mouseSensitivityText;
        public Toggle invertYAxisToggle;
        
        [Header("UI References")]
        public Button applyButton;
        public Button resetButton;
        public Button backButton;
        
        [Header("Audio Mixer")]
        public AudioMixer audioMixer;
        
        private GameManager gameManager;
        private UIManager uiManager;
        private AudioManager audioManager;
        private Resolution[] availableResolutions;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            uiManager = FindObjectOfType<UIManager>();
            audioManager = FindObjectOfType<AudioManager>();
            
            SetupUI();
        }

        private void Start()
        {
            LoadSettings();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Setup UI components and listeners
        /// </summary>
        private void SetupUI()
        {
            SetupAudioSliders();
            SetupGraphicsSettings();
            SetupGameplaySettings();
            SetupControlSettings();
            SetupButtons();
        }

        /// <summary>
        /// Setup audio sliders
        /// </summary>
        private void SetupAudioSliders()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
                
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
                
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                
            if (ambientVolumeSlider != null)
                ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
        }

        /// <summary>
        /// Setup graphics settings
        /// </summary>
        private void SetupGraphicsSettings()
        {
            // Setup quality dropdown
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
            
            // Setup resolution dropdown
            if (resolutionDropdown != null)
            {
                SetupResolutionDropdown();
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }
            
            // Setup toggles
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
                
            if (vsyncToggle != null)
                vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
                
            // Setup FPS limit
            if (fpsLimitSlider != null)
                fpsLimitSlider.onValueChanged.AddListener(OnFPSLimitChanged);
        }

        /// <summary>
        /// Setup gameplay settings
        /// </summary>
        private void SetupGameplaySettings()
        {
            if (gameSpeedSlider != null)
                gameSpeedSlider.onValueChanged.AddListener(OnGameSpeedChanged);
                
            if (autosaveToggle != null)
                autosaveToggle.onValueChanged.AddListener(OnAutosaveChanged);
                
            if (autosaveIntervalSlider != null)
                autosaveIntervalSlider.onValueChanged.AddListener(OnAutosaveIntervalChanged);
                
            if (tutorialToggle != null)
                tutorialToggle.onValueChanged.AddListener(OnTutorialChanged);
        }

        /// <summary>
        /// Setup control settings
        /// </summary>
        private void SetupControlSettings()
        {
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
                
            if (invertYAxisToggle != null)
                invertYAxisToggle.onValueChanged.AddListener(OnInvertYAxisChanged);
        }

        /// <summary>
        /// Setup buttons
        /// </summary>
        private void SetupButtons()
        {
            if (applyButton != null)
                applyButton.onClick.AddListener(ApplySettings);
                
            if (resetButton != null)
                resetButton.onClick.AddListener(ResetToDefaults);
                
            if (backButton != null)
                backButton.onClick.AddListener(CloseSettings);
        }

        /// <summary>
        /// Setup resolution dropdown with available resolutions
        /// </summary>
        private void SetupResolutionDropdown()
        {
            availableResolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            
            var resolutionOptions = new System.Collections.Generic.List<string>();
            int currentResolutionIndex = 0;
            
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                string option = availableResolutions[i].width + " x " + availableResolutions[i].height;
                resolutionOptions.Add(option);
                
                if (availableResolutions[i].width == Screen.currentResolution.width &&
                    availableResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(resolutionOptions);
            resolutionDropdown.value = currentResolutionIndex;
        }

        /// <summary>
        /// Load settings from PlayerPrefs
        /// </summary>
        private void LoadSettings()
        {
            // Audio settings
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            if (ambientVolumeSlider != null)
                ambientVolumeSlider.value = PlayerPrefs.GetFloat("AmbientVolume", 1f);
            
            // Graphics settings
            if (qualityDropdown != null)
                qualityDropdown.value = QualitySettings.GetQualityLevel();
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = Screen.fullScreen;
            if (vsyncToggle != null)
                vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
            if (fpsLimitSlider != null)
                fpsLimitSlider.value = Application.targetFrameRate == -1 ? 60 : Application.targetFrameRate;
            
            // Gameplay settings
            if (gameSpeedSlider != null)
                gameSpeedSlider.value = PlayerPrefs.GetFloat("GameSpeed", 1f);
            if (autosaveToggle != null)
                autosaveToggle.isOn = PlayerPrefs.GetInt("AutosaveEnabled", 1) == 1;
            if (autosaveIntervalSlider != null)
                autosaveIntervalSlider.value = PlayerPrefs.GetFloat("AutosaveInterval", 300f);
            if (tutorialToggle != null)
                tutorialToggle.isOn = PlayerPrefs.GetInt("TutorialEnabled", 1) == 1;
            
            // Control settings
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
            if (invertYAxisToggle != null)
                invertYAxisToggle.isOn = PlayerPrefs.GetInt("InvertYAxis", 0) == 1;
        }

        /// <summary>
        /// Audio volume change handlers
        /// </summary>
        private void OnMasterVolumeChanged(float value)
        {
            if (audioMixer != null)
                audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
            if (masterVolumeText != null)
                masterVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (audioMixer != null)
                audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
            if (musicVolumeText != null)
                musicVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (audioMixer != null)
                audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
            if (sfxVolumeText != null)
                sfxVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }

        private void OnAmbientVolumeChanged(float value)
        {
            if (audioMixer != null)
                audioMixer.SetFloat("AmbientVolume", Mathf.Log10(value) * 20);
            if (ambientVolumeText != null)
                ambientVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }

        /// <summary>
        /// Graphics settings change handlers
        /// </summary>
        private void OnQualityChanged(int index)
        {
            QualitySettings.SetQualityLevel(index);
        }

        private void OnResolutionChanged(int index)
        {
            Resolution resolution = availableResolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        private void OnFullscreenChanged(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
        }

        private void OnVSyncChanged(bool vsync)
        {
            QualitySettings.vSyncCount = vsync ? 1 : 0;
        }

        private void OnFPSLimitChanged(float value)
        {
            int fps = Mathf.RoundToInt(value);
            Application.targetFrameRate = fps;
            if (fpsLimitText != null)
                fpsLimitText.text = fps.ToString() + " FPS";
        }

        /// <summary>
        /// Gameplay settings change handlers
        /// </summary>
        private void OnGameSpeedChanged(float value)
        {
            if (gameManager != null)
                // TODO: Implement SetGameSpeed in GameManager
                // gameManager.SetGameSpeed(value);
            if (gameSpeedText != null)
                gameSpeedText.text = value.ToString("F1") + "x";
        }

        private void OnAutosaveChanged(bool enabled)
        {
            if (gameManager != null)
            {
                // TODO: Implement SetAutosaveEnabled in GameManager
                // gameManager.SetAutosaveEnabled(enabled);
            }
        }

        private void OnAutosaveIntervalChanged(float value)
        {
            if (gameManager != null)
            if (gameManager != null)
            {
                // TODO: Implement SetAutosaveInterval in GameManager
                // gameManager.SetAutosaveInterval(value);
            }
            if (autosaveIntervalText != null)
                autosaveIntervalText.text = Mathf.RoundToInt(value) + "s";
        }

        private void OnTutorialChanged(bool enabled)
        {
            // Handle tutorial enable/disable
        }

        /// <summary>
        /// Control settings change handlers
        /// </summary>
        private void OnMouseSensitivityChanged(float value)
        {
            if (mouseSensitivityText != null)
                mouseSensitivityText.text = value.ToString("F1");
        }

        private void OnInvertYAxisChanged(bool invert)
        {
            // Handle Y-axis inversion
        }

        /// <summary>
        /// Apply all settings and save to PlayerPrefs
        /// </summary>
        public void ApplySettings()
        {
            SaveSettings();
            CloseSettings();
        }

        /// <summary>
        /// Save settings to PlayerPrefs
        /// </summary>
        private void SaveSettings()
        {
            // Audio settings
            if (masterVolumeSlider != null)
                PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            if (musicVolumeSlider != null)
                PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            if (sfxVolumeSlider != null)
                PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            if (ambientVolumeSlider != null)
                PlayerPrefs.SetFloat("AmbientVolume", ambientVolumeSlider.value);
            
            // Gameplay settings
            if (gameSpeedSlider != null)
                PlayerPrefs.SetFloat("GameSpeed", gameSpeedSlider.value);
            if (autosaveToggle != null)
                PlayerPrefs.SetInt("AutosaveEnabled", autosaveToggle.isOn ? 1 : 0);
            if (autosaveIntervalSlider != null)
                PlayerPrefs.SetFloat("AutosaveInterval", autosaveIntervalSlider.value);
            if (tutorialToggle != null)
                PlayerPrefs.SetInt("TutorialEnabled", tutorialToggle.isOn ? 1 : 0);
            
            // Control settings
            if (mouseSensitivitySlider != null)
                PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);
            if (invertYAxisToggle != null)
                PlayerPrefs.SetInt("InvertYAxis", invertYAxisToggle.isOn ? 1 : 0);
            
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            PlayerPrefs.DeleteAll();
            LoadSettings();
        }

        /// <summary>
        /// Close the settings menu
        /// </summary>
        public void CloseSettings()
        {
            if (uiManager != null)
            {
                // TODO: Implement HideSettingsMenu in UIManager
                // uiManager.HideSettingsMenu();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Show the settings menu
        /// </summary>
        public void ShowSettings()
        {
            gameObject.SetActive(true);
            LoadSettings();
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (applyButton != null) applyButton.onClick.RemoveAllListeners();
            if (resetButton != null) resetButton.onClick.RemoveAllListeners();
            if (backButton != null) backButton.onClick.RemoveAllListeners();
        }
    }
}
