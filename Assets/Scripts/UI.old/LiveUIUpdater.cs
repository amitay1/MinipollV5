using UnityEngine;
using UnityEngine.UI;
using MinipollGame.Core;
using MinipollGame.Managers;
using MinipollGame.Systems.Core;

namespace MinipollGame.UI
{
    /// <summary>
    /// Live UI data updater - connects real game data to UI text elements
    /// This script will be activated automatically and update all UI panels with real game information
    /// </summary>
    public class LiveUIUpdater : MonoBehaviour
    {
        [Header("Auto-Update Settings")]
        public float updateInterval = 1f;
        private float lastUpdateTime;
        
        // UI Text components (will be found automatically)
        private Text gameTitleText;
        private Text populationInfoText;
        private Text selectedMinipollInfoText;
        private Text systemStatusText;
        
        // Game references (will be found automatically)
        private MinipollGame.Core.MinipollCore selectedMinipoll;
        private MinipollGame.Managers.MinipollManager minipollManager;
        private GameManager gameManager;
        
        void Start()
        {
            StartCoroutine(InitializeAfterDelay());
        }
        
        System.Collections.IEnumerator InitializeAfterDelay()
        {
            yield return new WaitForSeconds(2f); // Wait for UI elements to be created
            
            FindUIElements();
            FindGameReferences();
            
            Debug.Log("🎨 Live UI Updater initialized and ready!");
        }
        
        void Update()
        {
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateAllUI();
                lastUpdateTime = Time.time;
            }
        }
        
        private void FindUIElements()
        {
            gameTitleText = FindUIText("GameTitle");
            populationInfoText = FindUIText("PopulationInfo");
            selectedMinipollInfoText = FindUIText("SelectedMinipollInfo");
            systemStatusText = FindUIText("SystemStatus");
            
            Debug.Log($"🔗 Found UI elements: Title={gameTitleText != null}, Pop={populationInfoText != null}, Selected={selectedMinipollInfoText != null}, Status={systemStatusText != null}");
        }
        
        private Text FindUIText(string name)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Text text = obj.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 12;
                    text.color = Color.white;
                    return text;
                }
            }
            return null;
        }
        
        private void FindGameReferences()
        {
            minipollManager = FindFirstObjectByType<MinipollGame.Managers.MinipollManager>();
            gameManager = FindFirstObjectByType<GameManager>();
            
            MinipollGame.Core.MinipollCore[] minipolls = FindObjectsByType<MinipollGame.Core.MinipollCore>(FindObjectsSortMode.None);
            if (minipolls.Length > 0)
                selectedMinipoll = minipolls[0];
            
            Debug.Log($"🎮 Found game references: Manager={minipollManager != null}, GameMgr={gameManager != null}, Minipoll={selectedMinipoll != null}");
        }
        
        private void UpdateAllUI()
        {
            try
            {
                UpdateGameTitle();
                UpdatePopulationInfo();
                UpdateSelectedMinipollInfo();
                UpdateSystemStatus();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating UI: {e.Message}");
            }
        }
        
        private void UpdateGameTitle()
        {
            if (gameTitleText != null)
            {
                int day = 1;
                if (gameManager != null)
                    day = Mathf.FloorToInt(Time.time / 86400f) + 1; // Simple time calculation
                
                string timeOfDay = GetTimeOfDay();
                gameTitleText.text = $"🐧 MINIPOLL SIMULATION V5\\nDay {day} - {timeOfDay}";
            }
        }
        
        private void UpdatePopulationInfo()
        {
            if (populationInfoText != null)
            {
                int totalCount = FindObjectsByType<MinipollGame.Core.MinipollCore>(FindObjectsSortMode.None).Length;
                
                string info = $"📊 POPULATION STATUS\\n" +
                             $"Active Minipolls: {totalCount}\\n" +
                             $"Alive: {totalCount}\\n" +
                             $"\\n🏪 RESOURCES\\n" +
                             $"🍎 Food: Available\\n" +
                             $"💧 Water: Available\\n" +
                             $"🏠 Sleep Area: Available";
                
                populationInfoText.text = info;
            }
        }
        
        private void UpdateSelectedMinipollInfo()
        {
            if (selectedMinipollInfoText != null && selectedMinipoll != null)
            {
                string name = selectedMinipoll.Name;
                string gender = selectedMinipoll.Gender == Gender.Male ? "♂ Male" : "♀ Female";
                float age = selectedMinipoll.Age;
                
                float health = selectedMinipoll.Health.CurrentHealth / selectedMinipoll.Health.MaxHealth;
                float energy = selectedMinipoll.Needs != null ? selectedMinipoll.Needs.energy.currentValue / 100f : 0.5f;
                
                string currentGoal = "Thinking";
                if (selectedMinipoll.Brain != null)
                    currentGoal = "Active";
                
                string info = $"🎯 SELECTED MINIPOLL\\n" +
                             $"Name: {name}\\n" +
                             $"Gender: {gender}\\n" +
                             $"Age: {age:F1} days\\n" +
                             $"\\n❤️ HEALTH & STATUS\\n" +
                             $"Health: {health:P0}\\n" +
                             $"Energy: {energy:P0}\\n" +
                             $"\\n🧠 AI BRAIN STATUS\\n" +
                             $"Current Goal: {currentGoal}\\n" +
                             $"State: Active";
                
                selectedMinipollInfoText.text = info;
            }
        }
        
        private void UpdateSystemStatus()
        {
            if (systemStatusText != null)
            {
                var statusLines = new System.Collections.Generic.List<string>();
                
                // Check each system
                bool movementOK = selectedMinipoll != null && selectedMinipoll.Movement != null;
                statusLines.Add($"🚶 Movement: {(movementOK ? "✅" : "❌")}");
                
                bool animationOK = selectedMinipoll != null && selectedMinipoll.GetComponent<Animator>() != null;
                statusLines.Add($"🎭 Animation: {(animationOK ? "✅" : "❌")}");
                
                bool needsimOK = selectedMinipoll != null && selectedMinipoll.Needs != null;
                statusLines.Add($"🎯 NEEDSIM: {(needsimOK ? "✅" : "❌")}");
                
                bool brainOK = selectedMinipoll != null && selectedMinipoll.Brain != null;
                statusLines.Add($"🧠 AI Brain: {(brainOK ? "✅" : "❌")}");
                
                bool memoryOK = selectedMinipoll != null && selectedMinipoll.Memory != null;
                statusLines.Add($"💭 Memory: {(memoryOK ? "✅" : "❌")}");
                
                bool socialOK = selectedMinipoll != null && selectedMinipoll.SocialRelations != null;
                statusLines.Add($"👥 Social: {(socialOK ? "✅" : "❌")}");
                
                string statusText = "⚙️ SYSTEM STATUS\\n" + string.Join("\\n", statusLines.ToArray());
                systemStatusText.text = statusText;
            }
        }
        
        private string GetTimeOfDay()
        {
            // Simple time calculation
            float timeOfDay = (Time.time % 86400f) / 86400f;
            
            if (timeOfDay < 0.25f) return "🌙 Night";
            else if (timeOfDay < 0.5f) return "🌅 Morning";  
            else if (timeOfDay < 0.75f) return "☀️ Day";
            else return "🌇 Evening";
        }
        
        public void SelectMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            selectedMinipoll = minipoll;
            Debug.Log($"🎯 UI Selected Minipoll: {minipoll.Name}");
        }
        
        [ContextMenu("Force Update")]
        public void ForceUpdate()
        {
            UpdateAllUI();
        }
        
        [ContextMenu("Reconnect References")]
        public void ReconnectReferences()
        {
            FindUIElements();
            FindGameReferences();
        }
    }
}
