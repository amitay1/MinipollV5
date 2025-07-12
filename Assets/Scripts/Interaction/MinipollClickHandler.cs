using UnityEngine;
using MinipollGame.UI;
using MinipollGame.Core;

namespace MinipollGame.Interaction
{
    /// <summary>
    /// Minipoll Click Handler - Handles mouse clicks on Minipoll creatures for selection
    /// Integrates with GameSceneUI to enable Feed/Play/Clean actions
    /// </summary>
    public class MinipollClickHandler : MonoBehaviour
    {
        [Header("⚙️ Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private LayerMask minipollLayerMask = -1; // All layers by default
        
        [Header("🎯 Selection Visual")]
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private bool createIndicatorIfMissing = true;
        [SerializeField] private Color selectionColor = Color.yellow;
        [SerializeField] private float indicatorHeight = 2.0f;
        
        // References
        private GameSceneUI gameSceneUI;
        private MinipollGame.Core.MinipollCore currentSelectedMinipoll;
        private GameObject currentSelectionIndicator;
        private Camera playerCamera;
        
        void Start()
        {
            InitializeClickHandler();
        }
        
        void InitializeClickHandler()
        {
            // Find GameSceneUI
            gameSceneUI = FindFirstObjectByType<GameSceneUI>();
            if (gameSceneUI == null)
            {
                Debug.LogWarning("⚠️ MinipollClickHandler: GameSceneUI not found - creating one");
                GameObject uiController = new GameObject("GameSceneUIController");
                gameSceneUI = uiController.AddComponent<GameSceneUI>();
            }
            
            // Find main camera
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindFirstObjectByType<Camera>();
            }
            
            if (enableDebugLogs)
            {
                Debug.Log("🎯 MinipollClickHandler: Initialized - ready to handle Minipoll selection");
            }
        }
        
        void Update()
        {
            HandleMouseInput();
        }
        
        void HandleMouseInput()
        {
            // Check for mouse click
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, minipollLayerMask))
                {
                    // Check if we hit a Minipoll
                    MinipollGame.Core.MinipollCore minipoll = hit.collider.GetComponent<MinipollGame.Core.MinipollCore>();
                    if (minipoll == null)
                    {
                        // Try to find MinipollCore in parent objects
                        minipoll = hit.collider.GetComponentInParent<MinipollGame.Core.MinipollCore>();
                    }
                    
                    if (minipoll != null)
                    {
                        SelectMinipoll(minipoll);
                    }
                    else
                    {
                        // Clicked on something that's not a Minipoll - deselect
                        DeselectMinipoll();
                    }
                }
                else
                {
                    // Clicked on empty space - deselect
                    DeselectMinipoll();
                }
            }
        }
        
        public void SelectMinipoll(MinipollGame.Core.MinipollCore minipoll)
        {
            if (minipoll == currentSelectedMinipoll)
            {
                if (enableDebugLogs)
                    Debug.Log($"🎯 {minipoll.name} is already selected");
                return;
            }
            
            // Deselect previous if any
            if (currentSelectedMinipoll != null)
            {
                RemoveSelectionIndicator();
            }
            
            // Select new Minipoll
            currentSelectedMinipoll = minipoll;
            
            // Update UI
            if (gameSceneUI != null)
            {
                gameSceneUI.SelectMinipoll(minipoll);
            }
            
            // Show selection indicator
            ShowSelectionIndicator(minipoll);
            
            if (enableDebugLogs)
                Debug.Log($"🎯 Selected Minipoll: {minipoll.name}");
        }
        
        public void DeselectMinipoll()
        {
            if (currentSelectedMinipoll == null) return;
            
            if (enableDebugLogs)
                Debug.Log($"🎯 Deselected Minipoll: {currentSelectedMinipoll.name}");
            
            currentSelectedMinipoll = null;
            
            // Update UI
            if (gameSceneUI != null)
            {
                gameSceneUI.DeselectMinipoll();
            }
            
            // Hide selection indicator
            RemoveSelectionIndicator();
        }
        
        void ShowSelectionIndicator(MinipollGame.Core.MinipollCore minipoll)
        {
            if (selectionIndicator != null)
            {
                currentSelectionIndicator = Instantiate(selectionIndicator);
            }
            else if (createIndicatorIfMissing)
            {
                currentSelectionIndicator = CreateSelectionIndicator();
            }
            
            if (currentSelectionIndicator != null)
            {
                // Position indicator above the Minipoll
                Vector3 indicatorPosition = minipoll.transform.position + Vector3.up * indicatorHeight;
                currentSelectionIndicator.transform.position = indicatorPosition;
                
                // Make it a child of the Minipoll so it follows the creature
                currentSelectionIndicator.transform.SetParent(minipoll.transform);
            }
        }
        
        void RemoveSelectionIndicator()
        {
            if (currentSelectionIndicator != null)
            {
                Destroy(currentSelectionIndicator);
                currentSelectionIndicator = null;
            }
        }
        
        GameObject CreateSelectionIndicator()
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "SelectionIndicator";
            
            // Remove collider so it doesn't interfere with selection
            Collider indicatorCollider = indicator.GetComponent<Collider>();
            if (indicatorCollider != null)
            {
                Destroy(indicatorCollider);
            }
            
            // Scale and color the indicator
            indicator.transform.localScale = new Vector3(2f, 0.1f, 2f);
            
            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = selectionColor;
                
                // Make it slightly transparent and glowing
                if (renderer.material.HasProperty("_Mode"))
                {
                    renderer.material.SetFloat("_Mode", 3); // Transparent mode
                }
                if (renderer.material.HasProperty("_Color"))
                {
                    Color glowColor = selectionColor;
                    glowColor.a = 0.7f;
                    renderer.material.color = glowColor;
                }
            }
            
            // Add a simple rotation animation
            IndicatorRotator rotator = indicator.AddComponent<IndicatorRotator>();
            
            return indicator;
        }
        
        /// <summary>
        /// Gets the currently selected Minipoll
        /// </summary>
        public MinipollGame.Core.MinipollCore GetSelectedMinipoll()
        {
            return currentSelectedMinipoll;
        }
        
        /// <summary>
        /// Checks if any Minipoll is currently selected
        /// </summary>
        public bool HasSelection()
        {
            return currentSelectedMinipoll != null;
        }
    }
    
    /// <summary>
    /// Simple component to rotate the selection indicator
    /// </summary>
    public class IndicatorRotator : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 30f;
        
        void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
