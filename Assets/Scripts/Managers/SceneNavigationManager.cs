using UnityEngine;
using UnityEngine.SceneManagement;

namespace MinipollGame.Managers
{
    /// <summary>
    /// Scene Navigation Manager - מנהל מעבר בין סצנות הפיתוח השונות
    /// מאפשר מעבר קל בין הסצנות השונות של המערכות
    /// </summary>
    public class SceneNavigationManager : MonoBehaviour
    {
        [Header("=== Scene Navigation ===")]
        [SerializeField] private bool showNavigationUI = true;
        [SerializeField] private KeyCode navigationKey = KeyCode.Tab;
        
        // Scene Names
        private readonly string[] sceneNames = {
            "01_CoreCreatureScene",
            "02_SocialSystemsScene", 
            "03_WorldEconomyScene",
            "04_AdvancedFeaturesScene",
            "05_IntegrationTestScene"
        };
        
        private readonly string[] sceneDescriptions = {
            "Core Creature - Basic creature systems",
            "Social Systems - Multi-creature interactions", 
            "World Economy - Building and economy systems",
            "Advanced Features - Genetics and special systems",
            "Integration Test - All systems working together"
        };
        
        private int currentSceneIndex = 0;
        private bool showUI = false;
        
        private void Awake()
        {
            // Make this persistent across scenes
            DontDestroyOnLoad(gameObject);
            
            // Find current scene index
            string currentScene = SceneManager.GetActiveScene().name;
            for (int i = 0; i < sceneNames.Length; i++)
            {
                if (sceneNames[i] == currentScene)
                {
                    currentSceneIndex = i;
                    break;
                }
            }
        }
        
        private void Update()
        {
            if (!showNavigationUI) return;
            
            // Toggle navigation UI
            if (Input.GetKeyDown(navigationKey))
            {
                showUI = !showUI;
            }
            
            // Quick navigation with number keys
            for (int i = 1; i <= sceneNames.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    LoadScene(i - 1);
                    return;
                }
            }
            
            // Next/Previous with arrow keys
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                LoadNextScene();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                LoadPreviousScene();
            }
        }
        
        private void OnGUI()
        {
            if (!showNavigationUI || !showUI) return;
            
            // Background box
            GUI.Box(new Rect(10, 10, 400, 200), "Scene Navigation");
            
            // Current scene info
            GUI.Label(new Rect(20, 40, 360, 20), $"Current: {sceneNames[currentSceneIndex]}");
            GUI.Label(new Rect(20, 60, 360, 20), sceneDescriptions[currentSceneIndex]);
            
            // Scene buttons
            for (int i = 0; i < sceneNames.Length; i++)
            {
                Color originalColor = GUI.backgroundColor;
                if (i == currentSceneIndex)
                    GUI.backgroundColor = Color.green;
                
                if (GUI.Button(new Rect(20, 90 + (i * 25), 360, 20), 
                              $"{i + 1}. {sceneNames[i]}"))
                {
                    LoadScene(i);
                }
                
                GUI.backgroundColor = originalColor;
            }
            
            // Instructions
            GUI.Label(new Rect(20, 220, 360, 20), "Press Tab to toggle, 1-5 to load scenes, Arrow keys to navigate");
        }
        
        public void LoadScene(int index)
        {
            if (index >= 0 && index < sceneNames.Length)
            {
                Debug.Log($"[SceneNavigation] Loading scene: {sceneNames[index]}");
                SceneManager.LoadScene(sceneNames[index]);
                currentSceneIndex = index;
                showUI = false;
            }
        }
        
        public void LoadNextScene()
        {
            int nextIndex = (currentSceneIndex + 1) % sceneNames.Length;
            LoadScene(nextIndex);
        }
        
        public void LoadPreviousScene()
        {
            int prevIndex = (currentSceneIndex - 1 + sceneNames.Length) % sceneNames.Length;
            LoadScene(prevIndex);
        }
        
        public void LoadCoreCreatureScene() => LoadScene(0);
        public void LoadSocialSystemsScene() => LoadScene(1);
        public void LoadWorldEconomyScene() => LoadScene(2);
        public void LoadAdvancedFeaturesScene() => LoadScene(3);
        public void LoadIntegrationTestScene() => LoadScene(4);
    }
}