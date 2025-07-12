using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MinipollGame.Core;
using MinipollGame.Managers;

namespace MinipollGame.UI.HUD
{
    /// <summary>
    /// Displays relationship information for the selected Minipoll
    /// </summary>
    public class RelationsPanel : MonoBehaviour
    {
        [Header("UI References")]
        public Transform relationshipsContainer;
        public GameObject relationshipItemPrefab;
        public TextMeshProUGUI titleText;
        public ScrollRect scrollRect;
        
        [Header("Display Settings")]
        public int maxRelationshipsToShow = 10;
        public bool showOnlyPositiveRelations = false;
        public bool showOnlyNegativeRelations = false;
        public float relationshipThreshold = 0.1f;
        
        [Header("Visual Settings")]
        public Color positiveRelationColor = Color.green;
        public Color negativeRelationColor = Color.red;
        public Color neutralRelationColor = Color.gray;
        
        private MinipollGame.Core.MinipollCore currentMinipoll;
        private List<RelationshipItem> relationshipItems = new List<RelationshipItem>();
        private UIManager uiManager;

        [System.Serializable]
        public class RelationshipItem
        {
            public GameObject gameObject;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI relationTypeText;
            public Slider relationshipSlider;
            public Image backgroundImage;
            public Button selectButton;
        }

        private void Awake()
        {
            uiManager = FindObjectOfType<UIManager>();
            
            if (titleText != null)
                titleText.text = "Relationships";
        }

        private void Start()
        {
            // Initially hide the relations panel
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Set the Minipoll to display relationships for
        /// </summary>
        public void SetMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            currentMinipoll = minipoll;
            
            if (currentMinipoll == null)
            {
                HideRelationsPanel();
                return;
            }
            
            ShowRelationsPanel();
            UpdateRelationshipDisplay();
        }

        /// <summary>
        /// Show the relations panel
        /// </summary>
        public void ShowRelationsPanel()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the relations panel
        /// </summary>
        public void HideRelationsPanel()
        {
            gameObject.SetActive(false);
            ClearRelationshipItems();
        }

        /// <summary>
        /// Update the relationship display with current data
        /// </summary>
        public void UpdateRelationshipDisplay()
        {
            if (currentMinipoll == null || currentMinipoll.SocialRelations == null)
                return;

            ClearRelationshipItems();
            CreateRelationshipItems();
        }

        /// <summary>
        /// Create relationship item UI elements
        /// </summary>
        private void CreateRelationshipItems()
        {
            if (relationshipItemPrefab == null || relationshipsContainer == null)
                return;

            var relationships = GetFilteredRelationships();
            
            // Sort relationships by combined strength (friendship - fear)
            relationships.Sort((a, b) => {
                float aValue = a.friendship - a.fear;
                float bValue = b.friendship - b.fear;
                return Mathf.Abs(bValue).CompareTo(Mathf.Abs(aValue));
            });

            int itemsCreated = 0;
            foreach (var relationship in relationships)
            {
                if (itemsCreated >= maxRelationshipsToShow)
                    break;

                // Get the other minipoll's core component
                var otherMinipoll = relationship.otherBrain.GetComponent<MinipollGame.Core.MinipollCore>();
                if (otherMinipoll != null)
                {
                    float relationshipValue = (relationship.friendship - relationship.fear) / 100f; // Normalize to -1 to 1
                    CreateRelationshipItem(otherMinipoll, relationshipValue);
                    itemsCreated++;
                }
            }

            // Update scroll rect if available
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>
        /// Get relationships filtered by current settings
        /// </summary>
        private List<MinipollGame.Social.SocialRelationship> GetFilteredRelationships()
        {
            var filteredRelationships = new List<MinipollGame.Social.SocialRelationship>();
            
            if (currentMinipoll.SocialRelations?.relationships == null)
                return filteredRelationships;

            foreach (var relationship in currentMinipoll.SocialRelations.relationships)
            {
                // Calculate combined relationship value (friendship - fear)
                float relationshipValue = relationship.friendship - relationship.fear;
                
                // Apply threshold filter
                if (Mathf.Abs(relationshipValue) < relationshipThreshold * 100f) // Scale threshold to 0-100 range
                    continue;
                
                // Apply positive/negative filters
                if (showOnlyPositiveRelations && relationshipValue <= 0)
                    continue;
                    
                if (showOnlyNegativeRelations && relationshipValue >= 0)
                    continue;
                
                filteredRelationships.Add(relationship);
            }

            return filteredRelationships;
        }

        /// <summary>
        /// Create a single relationship item
        /// </summary>
        private void CreateRelationshipItem(MinipollGame.Core.MinipollCore otherMinipoll, float relationshipValue)
        {
            GameObject itemObject = Instantiate(relationshipItemPrefab, relationshipsContainer);
            RelationshipItem item = SetupRelationshipItem(itemObject);
            
            UpdateRelationshipItem(item, otherMinipoll, relationshipValue);
            
            relationshipItems.Add(item);
        }

        /// <summary>
        /// Setup a relationship item with component references
        /// </summary>
        private RelationshipItem SetupRelationshipItem(GameObject itemObject)
        {
            RelationshipItem item = new RelationshipItem();
            item.gameObject = itemObject;
            
            // Find components in the prefab
            var texts = itemObject.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) item.nameText = texts[0];
            if (texts.Length > 1) item.relationTypeText = texts[1];
            
            item.relationshipSlider = itemObject.GetComponentInChildren<Slider>();
            item.backgroundImage = itemObject.GetComponent<Image>();
            item.selectButton = itemObject.GetComponentInChildren<Button>();
            
            return item;
        }

        /// <summary>
        /// Update a relationship item with data
        /// </summary>
        private void UpdateRelationshipItem(RelationshipItem item, MinipollGame.Core.MinipollCore otherMinipoll, float relationshipValue)
        {
            // Update name
            if (item.nameText != null)
                item.nameText.text = otherMinipoll.Name ?? "Unknown";
            
            // Update relationship type and slider
            if (item.relationTypeText != null && item.relationshipSlider != null)
            {
                string relationshipType = GetRelationshipType(relationshipValue);
                item.relationTypeText.text = relationshipType;
                
                // Normalize relationship value to 0-1 range for slider
                item.relationshipSlider.value = (relationshipValue + 1f) / 2f;
            }
            
            // Update colors based on relationship value
            Color relationshipColor = GetRelationshipColor(relationshipValue);
            
            if (item.backgroundImage != null)
                item.backgroundImage.color = relationshipColor;
            
            // Setup button to select the other Minipoll
            if (item.selectButton != null)
            {
                item.selectButton.onClick.RemoveAllListeners();
                item.selectButton.onClick.AddListener(() => SelectMinipoll(otherMinipoll));
            }
        }

        /// <summary>
        /// Get relationship type string based on value
        /// </summary>
        private string GetRelationshipType(float value)
        {
            if (value > 0.7f) return "Best Friend";
            if (value > 0.4f) return "Friend";
            if (value > 0.1f) return "Acquaintance";
            if (value > -0.1f) return "Neutral";
            if (value > -0.4f) return "Dislike";
            if (value > -0.7f) return "Enemy";
            return "Nemesis";
        }

        /// <summary>
        /// Get the appropriate color for a relationship value
        /// </summary>
        private Color GetRelationshipColor(float value)
        {
            if (value > 0.1f)
                return Color.Lerp(neutralRelationColor, positiveRelationColor, value);
            else if (value < -0.1f)
                return Color.Lerp(neutralRelationColor, negativeRelationColor, Mathf.Abs(value));
            else
                return neutralRelationColor;
        }

        /// <summary>
        /// Select a different Minipoll
        /// </summary>
        private void SelectMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            if (uiManager != null)
            {
                // For now, just log since SelectMinipoll method access needs investigation
                Debug.Log($"Attempting to select Minipoll: {minipoll.Name}");
            }
        }

        /// <summary>
        /// Clear all relationship items
        /// </summary>
        private void ClearRelationshipItems()
        {
            foreach (var item in relationshipItems)
            {
                if (item.gameObject != null)
                    Destroy(item.gameObject);
            }
            relationshipItems.Clear();
        }

        /// <summary>
        /// Toggle showing only positive relationships
        /// </summary>
        public void TogglePositiveOnly()
        {
            showOnlyPositiveRelations = !showOnlyPositiveRelations;
            if (showOnlyPositiveRelations)
                showOnlyNegativeRelations = false;
            
            UpdateRelationshipDisplay();
        }

        /// <summary>
        /// Toggle showing only negative relationships
        /// </summary>
        public void ToggleNegativeOnly()
        {
            showOnlyNegativeRelations = !showOnlyNegativeRelations;
            if (showOnlyNegativeRelations)
                showOnlyPositiveRelations = false;
            
            UpdateRelationshipDisplay();
        }

        /// <summary>
        /// Set the relationship threshold for display
        /// </summary>
        public void SetRelationshipThreshold(float threshold)
        {
            relationshipThreshold = Mathf.Clamp01(threshold);
            UpdateRelationshipDisplay();
        }

        /// <summary>
        /// Called when a Minipoll is selected
        /// </summary>
        public void OnMinipollSelected(MinipollGame.Core.MinipollCore minipoll)
        {
            SetMinipoll(minipoll);
        }

        /// <summary>
        /// Called when a Minipoll is deselected
        /// </summary>
        public void OnMinipollDeselected()
        {
            SetMinipoll(null);
        }

        private void OnDestroy()
        {
            ClearRelationshipItems();
        }
    }
}
