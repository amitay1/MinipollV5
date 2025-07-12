using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MinipollGame.Core;
using MinipollGame.Managers;

namespace MinipollGame.UI.HUD
{
    /// <summary>
    /// Handles the selection panel UI for displaying selected Minipoll information
    /// </summary>
    public class MinipollSelectionPanel : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panelContainer;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI healthText;
        public Slider healthSlider;
        
        [Header("Needs Display")]
        public Transform needsContainer;
        public GameObject needBarPrefab; // Prefab for displaying need bars
        
        [Header("Emotion Display")]
        public TextMeshProUGUI emotionText;
        public Slider emotionIntensitySlider;
        
        [Header("Relationship Display")]
        public Transform relationshipsContainer;
        public GameObject relationshipItemPrefab; // Prefab for relationship items
        
        private MinipollGame.Core.MinipollCore selectedMinipoll;
        private UIManager uiManager;

        private void Awake()
        {
            uiManager = FindObjectOfType<UIManager>();
            
            // Hide panel initially
            if (panelContainer)
                panelContainer.SetActive(false);
        }

        /// <summary>
        /// Select a Minipoll and display its information
        /// </summary>
        public void SelectMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            selectedMinipoll = minipoll;
            
            if (selectedMinipoll == null)
            {
                HidePanel();
                return;
            }
            
            ShowPanel();
            UpdateDisplay();
        }

        /// <summary>
        /// Show the selection panel
        /// </summary>
        public void ShowPanel()
        {
            if (panelContainer)
                panelContainer.SetActive(true);
        }

        /// <summary>
        /// Hide the selection panel
        /// </summary>
        public void HidePanel()
        {
            if (panelContainer)
                panelContainer.SetActive(false);
            
            selectedMinipoll = null;
        }

        /// <summary>
        /// Update all displayed information
        /// </summary>
        private void UpdateDisplay()
        {
            if (selectedMinipoll == null) return;

            UpdateBasicInfo();
            UpdateNeeds();
            UpdateEmotions();
            UpdateRelationships();
        }

        /// <summary>
        /// Update basic information (name, health)
        /// </summary>
        private void UpdateBasicInfo()
        {
            if (nameText)
                nameText.text = selectedMinipoll.Name;

            if (selectedMinipoll.Health != null)
            {
                float healthRatio = selectedMinipoll.Health.CurrentHealth / selectedMinipoll.Health.MaxHealth;
                
                if (healthText)
                    healthText.text = $"Health: {selectedMinipoll.Health.CurrentHealth:F1}/{selectedMinipoll.Health.MaxHealth:F1}";
                
                if (healthSlider)
                    healthSlider.value = healthRatio;
            }
        }

        /// <summary>
        /// Update needs display
        /// </summary>
        private void UpdateNeeds()
        {
            if (needsContainer == null || selectedMinipoll.NeedsSystem == null) return;

            // Clear existing need displays
            foreach (Transform child in needsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create need bars for each need
            var needsSystem = selectedMinipoll.NeedsSystem;
            
            CreateNeedBar("Energy", needsSystem.energy);
            CreateNeedBar("Hunger", needsSystem.hunger);
            CreateNeedBar("Social", needsSystem.social);
            CreateNeedBar("Fun", needsSystem.fun);
            CreateNeedBar("Hygiene", needsSystem.hygiene);
        }

        /// <summary>
        /// Create a need bar for a specific need
        /// </summary>
        private void CreateNeedBar(string needName, MinipollGame.Core.Need need)
        {
            if (needBarPrefab == null || needsContainer == null) return;

            GameObject needBar = Instantiate(needBarPrefab, needsContainer);
            
            // Assuming the prefab has Text for name and Slider for value
            TextMeshProUGUI needNameText = needBar.GetComponentInChildren<TextMeshProUGUI>();
            Slider needSlider = needBar.GetComponentInChildren<Slider>();
            
            if (needNameText)
                needNameText.text = needName;
            
            if (needSlider)
                needSlider.value = need.currentValue / 100f; // Assuming needs are 0-100
        }

        /// <summary>
        /// Update emotion display
        /// </summary>
        private void UpdateEmotions()
        {
            if (selectedMinipoll.Emotions == null) return;

            var dominantEmotion = selectedMinipoll.Emotions.GetDominantEmotion();
            
            if (emotionText)
                emotionText.text = dominantEmotion.emotion.ToString();
            
            if (emotionIntensitySlider)
                emotionIntensitySlider.value = dominantEmotion.intensity;
        }

        /// <summary>
        /// Update relationships display
        /// </summary>
        private void UpdateRelationships()
        {
            if (relationshipsContainer == null || selectedMinipoll.SocialRelations == null) return;

            // Clear existing relationship displays
            foreach (Transform child in relationshipsContainer)
            {
                Destroy(child.gameObject);
            }

            // Add relationship items
            // TODO: Implement GetAllRelationships in MinipollSocialRelations
            // var relationships = selectedMinipoll.SocialRelations.GetAllRelationships();
            /*
            foreach (var relationship in relationships)
            {
                CreateRelationshipItem(relationship);
            }
            */
        }

        /// <summary>
        /// Create a relationship item display
        /// </summary>
        private void CreateRelationshipItem(object relationship)
        {
            if (relationshipItemPrefab == null || relationshipsContainer == null) return;

            GameObject relationshipItem = Instantiate(relationshipItemPrefab, relationshipsContainer);
            
            // Customize based on your relationship data structure
            // This is a placeholder implementation
            TextMeshProUGUI relationshipText = relationshipItem.GetComponentInChildren<TextMeshProUGUI>();
            if (relationshipText)
                relationshipText.text = relationship.ToString();
        }

        /// <summary>
        /// Update display continuously
        /// </summary>
        private void Update()
        {
            if (selectedMinipoll != null && panelContainer.activeSelf)
            {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Called when close button is pressed
        /// </summary>
        public void OnCloseButtonPressed()
        {
            HidePanel();
            
            // Notify UI manager about deselection
            if (uiManager != null)
            {
                // TODO: Implement DeselectMinipoll in UIManager
                // uiManager.DeselectMinipoll();
            }
        }
    }
}
