using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MinipollGame.Core;
using MinipollGame.Managers;
using MinipollGame.Systems.Core;

namespace MinipollGame.UI
{
    /// <summary>
    /// UI Controller that connects game data to the UI display
    /// This will update all the UI elements with real game information
    /// </summary>
    public class MinipollUIController : MonoBehaviour
    {
        [Header("UI Text References")]
        public Text gameTitleText;
        public Text populationInfoText;
        public Text selectedMinipollInfoText;
        public Text systemStatusText;
        
        [Header("Game References")]
        public MinipollGame.Core.MinipollCore selectedMinipoll;
        public MinipollGame.Managers.MinipollManager minipollManager;
        public GameManager gameManager;
        
        [Header("Update Settings")]
        public float updateInterval = 1f;
        private float lastUpdateTime;
        
        void Start()
        {
            FindUIElements();
            FindGameReferences();
        }
        
        void Update()
        {
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateUI();
                lastUpdateTime = Time.time;
            }
        }
        
        private void FindUIElements()
        {
            if (gameTitleText == null)
                gameTitleText = GameObject.Find("GameTitle")?.GetComponent<Text>();
            
            if (populationInfoText == null)
                populationInfoText = GameObject.Find("PopulationInfo")?.GetComponent<Text>();
            
            if (selectedMinipollInfoText == null)
                selectedMinipollInfoText = GameObject.Find("SelectedMinipollInfo")?.GetComponent<Text>();
            
            if (systemStatusText == null)
                systemStatusText = GameObject.Find("SystemStatus")?.GetComponent<Text>();
                
            Debug.Log("🔗 UI Elements found and connected");
        }
        
        private void FindGameReferences()
        {
            if (minipollManager == null)
                minipollManager = FindFirstObjectByType<MinipollManager>();
            
            if (gameManager == null)
                gameManager = FindFirstObjectByType<GameManager>();
                
            // Find the main minipoll
            if (selectedMinipoll == null)
            {
                MinipollGame.Core.MinipollCore[] minipolls = FindObjectsByType<MinipollGame.Core.MinipollCore>(FindObjectsSortMode.None);
                if (minipolls.Length > 0)
                    selectedMinipoll = minipolls[0];
            }
            
            Debug.Log("🎮 Game references found and connected");
        }
        
        private void UpdateUI()
        {
            UpdateGameTitle();
            UpdatePopulationInfo();
            UpdateSelectedMinipollInfo();
            UpdateSystemStatus();
        }
        
        private void UpdateGameTitle()
        {
            if (gameTitleText != null)
            {
                int day = Mathf.FloorToInt(Time.time / 86400f) + 1; // Simple time calculation
                string timeOfDay = GetTimeOfDay();
                gameTitleText.text = $"🐧 MINIPOLL SIMULATION V5 - Day {day} {timeOfDay}";
            }
        }
        
        private void UpdatePopulationInfo()
        {
            if (populationInfoText != null)
            {
                // Simple count - just find all MinipollCore objects
                int totalMinipoll = FindObjectsByType<MinipollGame.Core.MinipollCore>(FindObjectsSortMode.None).Length;
                
                string info = $"📊 POPULATION STATUS\\n" +
                             $"Total Minipolls: {totalMinipoll}\\n" +
                             $"Active: {totalMinipoll}\\n" +
                             $"\\n🏪 RESOURCES\\n" +
                             $"🍎 Food: Available\\n" +
                             $"💧 Water: Available\\n" +
                             $"🏠 Shelter: Available";
                
                populationInfoText.text = info;
            }
        }
        
        private void UpdateSelectedMinipollInfo()
        {
            if (selectedMinipollInfoText != null && selectedMinipoll != null)
            {
                string name = selectedMinipoll.Name;
                string gender = selectedMinipoll.Gender == Gender.Male ? "♂ Male" : "♀ Female";
                int age = Mathf.FloorToInt(selectedMinipoll.Age);
                
                // Get health and needs info
                float health = selectedMinipoll.Health.CurrentHealth / selectedMinipoll.Health.MaxHealth;
                float energy = selectedMinipoll.Needs.energy.currentValue / 100f; // Needs system energy
                
                string info = $"🎯 SELECTED MINIPOLL\\n" +
                             $"Name: {name}\\n" +
                             $"Gender: {gender}\\n" +
                             $"Age: {age} days\\n" +
                             $"\\n❤️ HEALTH & STATUS\\n" +
                             $"Health: {health:P0}\\n" +
                             $"Energy: {energy:P0}\\n";
                
                // Add needs info if available
                if (selectedMinipoll.Needs != null)
                {
                    info += $"\\n🎯 NEEDS SYSTEM\\n" +
                           $"🍎 Hunger: {selectedMinipoll.Needs.hunger.currentValue:F0}%\\n" +
                           $"💧 Thirst: {selectedMinipoll.Needs.hygiene.currentValue:F0}%\\n" +
                           $"😴 Energy: {selectedMinipoll.Needs.energy.currentValue:F0}%\\n" +
                           $"👥 Social: {selectedMinipoll.Needs.social.currentValue:F0}%";
                }
                
                // Add brain status - simplified
                if (selectedMinipoll.Brain != null)
                {
                    info += $"\\n🧠 AI BRAIN STATUS\\n" +
                           $"State: Active\\n" +
                           $"Decision: Running";
                }
                
                selectedMinipollInfoText.text = info;
            }
        }
        
        private void UpdateSystemStatus()
        {
            if (systemStatusText != null)
            {
                List<string> systemStatuses = new List<string>();
                
                // Check movement system
                bool movementOK = selectedMinipoll != null && selectedMinipoll.Movement != null;
                systemStatuses.Add($"🚶 Movement: {(movementOK ? "✅" : "❌")}");
                
                // Check animation system
                bool animationOK = selectedMinipoll != null && selectedMinipoll.GetComponent<Animator>() != null;
                systemStatuses.Add($"🎭 Animation: {(animationOK ? "✅" : "❌")}");
                
                // Check NEEDSIM system
                bool needsimOK = selectedMinipoll != null && selectedMinipoll.Needs != null;
                systemStatuses.Add($"🎯 NEEDSIM: {(needsimOK ? "✅" : "❌")}");
                
                // Check brain system
                bool brainOK = selectedMinipoll != null && selectedMinipoll.Brain != null;
                systemStatuses.Add($"🧠 AI Brain: {(brainOK ? "✅" : "❌")}");
                
                // Check memory system
                bool memoryOK = selectedMinipoll != null && selectedMinipoll.Memory != null;
                systemStatuses.Add($"💭 Memory: {(memoryOK ? "✅" : "❌")}");
                
                // Check social system
                bool socialOK = selectedMinipoll != null && selectedMinipoll.SocialRelations != null;
                systemStatuses.Add($"👥 Social: {(socialOK ? "✅" : "❌")}");
                
                string statusText = "⚙️ SYSTEM STATUS\\n" + string.Join("\\n", systemStatuses);
                systemStatusText.text = statusText;
            }
        }
        
        private string GetTimeOfDay()
        {
            // Simple time calculation based on actual time
            float timeOfDay = (Time.time % 86400f) / 86400f;
            
            if (timeOfDay < 0.25f) return "🌙 Night";
            else if (timeOfDay < 0.5f) return "🌅 Morning";
            else if (timeOfDay < 0.75f) return "☀️ Day";
            else return "🌇 Evening";
        }
        
        public void SelectMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            selectedMinipoll = minipoll;
            Debug.Log($"🎯 Selected Minipoll: {minipoll.Name}");
        }
        
        [ContextMenu("Update UI Now")]
        public void ForceUpdateUI()
        {
            UpdateUI();
        }
        
        [ContextMenu("Find References")]
        public void FindAllReferences()
        {
            FindUIElements();
            FindGameReferences();
        }
    }
}
