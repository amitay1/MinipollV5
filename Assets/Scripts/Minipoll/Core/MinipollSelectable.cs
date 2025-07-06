using UnityEngine;
using UnityEngine.EventSystems;
using System;
using MinipollGame.Controllers;

/// <summary>
/// מערכת הבחירה של Minipoll
/// מאפשרת לשחקן לבחור ולקיים אינטראקציה עם היצורים
/// </summary>
[RequireComponent(typeof(MinipollGame.Core.MinipollCore))]
[RequireComponent(typeof(Collider))]
public class MinipollSelectable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region Selection Properties
    [Header("=== Selection Settings ===")]
    [SerializeField] private bool isSelectable = true;
    [SerializeField] private bool allowMultiSelect = false;
    [SerializeField] private float selectionRadius = 1.5f;
    [SerializeField] private LayerMask selectionLayer = -1;
    
    [Header("=== Visual Feedback ===")]
    [SerializeField] private bool showSelectionIndicator = true;
    [SerializeField] private GameObject selectionIndicatorPrefab;
    [SerializeField] private Color selectionColor = Color.green;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private float indicatorHeight = 0.1f;
    
    [Header("=== Outline Settings ===")]
    [SerializeField] private bool useOutline = true;
    [SerializeField] private float outlineWidth = 0.03f;
    [SerializeField] private Material outlineMaterial;
    
    // State
    public bool IsSelected { get; private set; }
    public bool IsHovered { get; private set; }
    public bool IsSelectable => isSelectable && core.IsAlive;
      // Selection group
    public static MinipollSelectable CurrentSelection { get; private set; }

    public static event Action<MinipollSelectable> OnAnySelected;
    public static event Action<MinipollSelectable> OnAnyDeselected;
    #endregion

    #region Events
    public event Action OnSelected;
    public event Action OnDeselected;
    public event Action OnHoverEnter;
    public event Action OnHoverExit;
    public event Action OnDoubleClick;
    public event Action<MinipollSelectable> OnRightClick;
    #endregion

    #region References
    private MinipollGame.Core.MinipollCore core;
    private MinipollVisualController visualController;
    private Collider selectionCollider;
    private GameObject selectionIndicator;
    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] outlinedMaterials;
    
    private Camera mainCamera;
    private float lastClickTime;
    private const float DOUBLE_CLICK_TIME = 0.3f;
    #endregion

    #region Initialization
    private void Awake()
    {
        core = GetComponent<MinipollGame.Core.MinipollCore>();
        visualController = GetComponentInChildren<MinipollVisualController>();
        selectionCollider = GetComponent<Collider>();
        
        // Ensure collider is set up correctly
        SetupCollider();
        
        // Get renderers for outline effect
        if (useOutline)
        {
            SetupOutlineEffect();
        }
        
        // Cache main camera
        mainCamera = Camera.main;
    }

    private void Start()
    {
        // Create selection indicator
        if (showSelectionIndicator)
        {
            CreateSelectionIndicator();
        }
          // Register with selection manager if exists
        // TODO: Implement SelectionManager
        // if (SelectionManager.Instance != null)
        // {
        //     SelectionManager.Instance.RegisterSelectable(this);
        // }
    }

    private void SetupCollider()
    {
        if (!selectionCollider)
        {
            selectionCollider = gameObject.AddComponent<SphereCollider>();
        }
        
        // Set collider as trigger for selection
        selectionCollider.isTrigger = true;
        
        // Set appropriate size
        if (selectionCollider is SphereCollider sphere)
        {
            sphere.radius = selectionRadius;
        }
        else if (selectionCollider is BoxCollider box)
        {
            box.size = Vector3.one * selectionRadius * 2;
        }
          // Ensure on correct layer
        if (selectionLayer != 0)
        {
            // Find the first set bit in the layer mask (which represents the layer index)
            int layerIndex = 0;
            int mask = selectionLayer.value;
            while (mask > 1)
            {
                mask >>= 1;
                layerIndex++;
            }
            
            // Ensure layer is within valid range [0-31]
            if (layerIndex >= 0 && layerIndex <= 31)
            {
                gameObject.layer = layerIndex;
            }
        }
    }

    private void SetupOutlineEffect()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        outlinedMaterials = new Material[renderers.Length];
          for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
            
            // Create outlined version
            if (outlineMaterial != null)
            {
                outlinedMaterials[i] = new Material(outlineMaterial);
                outlinedMaterials[i].SetFloat("_OutlineWidth", outlineWidth);
            }
        }
    }

    private void CreateSelectionIndicator()
    {
        if (selectionIndicatorPrefab != null)
        {
            selectionIndicator = Instantiate(selectionIndicatorPrefab, transform);
            selectionIndicator.transform.localPosition = Vector3.up * indicatorHeight;
        }
        else
        {
            // Create default selection ring
            selectionIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            selectionIndicator.name = "SelectionIndicator";
            selectionIndicator.transform.SetParent(transform);
            selectionIndicator.transform.localPosition = Vector3.up * indicatorHeight;
            selectionIndicator.transform.localScale = new Vector3(selectionRadius * 2, 0.05f, selectionRadius * 2);
            
            // Remove collider
            Destroy(selectionIndicator.GetComponent<Collider>());
            
            // Set material
            Renderer indicatorRenderer = selectionIndicator.GetComponent<Renderer>();
            indicatorRenderer.material = new Material(Shader.Find("Standard"));
            indicatorRenderer.material.color = selectionColor;
        }
        
        selectionIndicator.SetActive(false);
    }
    #endregion

    #region Selection Methods
    public void Select(bool notify = true)
    {
        if (!IsSelectable || IsSelected) return;
        
        // Deselect current selection if not multi-select
        if (!allowMultiSelect && CurrentSelection != null && CurrentSelection != this)
        {
            CurrentSelection.Deselect();
        }
        
        IsSelected = true;
        CurrentSelection = this;
        
        // Visual feedback
        UpdateVisualState();
          // Audio feedback
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySFX("UI_Select");
        }
        
        // Events
        if (notify)
        {
            OnSelected?.Invoke();
            OnAnySelected?.Invoke(this);
        }
        
        Debug.Log($"[Selectable] {core.Name} selected");
    }

    public void Deselect(bool notify = true)
    {
        if (!IsSelected) return;
        
        IsSelected = false;
        
        if (CurrentSelection == this)
            CurrentSelection = null;
            
        // Visual feedback
        UpdateVisualState();
        
        // Events
        if (notify)
        {
            OnDeselected?.Invoke();
            OnAnyDeselected?.Invoke(this);
        }
        
        Debug.Log($"[Selectable] {core.Name} deselected");
    }

    public void ToggleSelection()
    {
        if (IsSelected)
            Deselect();
        else
            Select();
    }
    #endregion

    #region Mouse/Touch Input
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsSelectable) return;
        
        // Check for double click
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= DOUBLE_CLICK_TIME)
        {
            HandleDoubleClick();
            return;
        }
        
        lastClickTime = Time.time;
          // Handle different mouse buttons
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick(eventData);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(eventData);
        }
    }

    private void HandleLeftClick(PointerEventData eventData)
    {
        // Check modifiers
        bool isShiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isCtrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        
        if (isShiftHeld && allowMultiSelect)
        {
            // Add to selection
            ToggleSelection();
        }
        else if (isCtrlHeld && allowMultiSelect)
        {
            // Toggle selection
            ToggleSelection();
        }
        else
        {
            // Normal selection
            Select();
        }
    }

    private void HandleRightClick(PointerEventData eventData)
    {
        OnRightClick?.Invoke(this);
          // Show context menu if available
        // TODO: Implement ContextMenuManager
        // if (ContextMenuManager.Instance)
        // {
        //     ContextMenuManager.Instance.ShowMinipollMenu(this);
        // }
    }

    private void HandleDoubleClick()
    {
        OnDoubleClick?.Invoke();
          // Default behavior - focus camera on minipoll
        // TODO: Implement CameraController
        // if (CameraController.Instance)
        // {
        //     CameraController.Instance.FocusOn(transform);
        // }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsSelectable) return;
        
        IsHovered = true;
        UpdateVisualState();
          // Show tooltip
        // TODO: Implement ShowMinipollTooltip method in UIManager
        // if (UIManager.Instance)
        // {
        //     UIManager.Instance.ShowMinipollTooltip(core);
        // }
        
        OnHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
        UpdateVisualState();
          // Hide tooltip
        // TODO: Implement HideTooltip method in UIManager
        // if (UIManager.Instance)
        // {
        //     UIManager.Instance.HideTooltip();
        // }
        
        OnHoverExit?.Invoke();
    }
    #endregion

    #region Raycast Selection
    private void Update()
    {
        // Handle keyboard shortcuts when selected
        if (IsSelected)
        {
            HandleSelectedInput();
        }
        
        // Update selection indicator rotation
        if (selectionIndicator != null && selectionIndicator.activeSelf)
        {
            selectionIndicator.transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }
    }

    private void HandleSelectedInput()
    {
        // Delete key - kill minipoll (debug)
        if (Input.GetKeyDown(KeyCode.Delete) && Application.isEditor)
        {
            if (core.Health)
                core.Health.Kill();
        }
          // Space - make minipoll jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (core.Movement)
            {
                // TODO: Implement Jump method in MinipollMovementController
                // core.Movement.Jump();
            }
        }
        
        // Tab - cycle to next minipoll
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SelectNext();
        }
    }

    public static void SelectByRaycast()
    {
        if (Camera.main == null) return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            MinipollSelectable selectable = hit.collider.GetComponent<MinipollSelectable>();
            if (selectable != null && selectable.IsSelectable)
            {
                selectable.Select();
            }
            else if (!Input.GetKey(KeyCode.LeftShift)) // Don't deselect when shift is held
            {
                // Clicked on non-selectable - deselect current
                if (CurrentSelection != null)
                    CurrentSelection.Deselect();
            }
        }
    }
    #endregion

    #region Visual Feedback
    private void UpdateVisualState()
    {
        // Selection indicator
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(IsSelected);
            
            if (IsSelected)
            {
                // Update color
                Renderer indicatorRenderer = selectionIndicator.GetComponent<Renderer>();
                if (indicatorRenderer)
                {
                    indicatorRenderer.material.color = IsHovered ? hoverColor : selectionColor;
                }
            }
        }
        
        // Outline effect
        if (useOutline && renderers != null)
        {
            bool showOutline = IsSelected || IsHovered;
            
            for (int i = 0; i < renderers.Length; i++)
            {
                if (showOutline && outlinedMaterials[i] != null)
                {
                    // Apply outline
                    Material[] mats = renderers[i].materials;
                    if (mats.Length > 1)
                    {
                        mats[1] = outlinedMaterials[i];
                    }
                    else
                    {
                        mats = new Material[] { mats[0], outlinedMaterials[i] };
                    }
                    renderers[i].materials = mats;
                    
                    // Set outline color
                    outlinedMaterials[i].SetColor("_OutlineColor", IsSelected ? selectionColor : hoverColor);
                }
                else
                {
                    // Remove outline
                    Material[] mats = renderers[i].materials;
                    if (mats.Length > 1)
                    {
                        mats = new Material[] { mats[0] };
                        renderers[i].materials = mats;
                    }
                }
            }
        }
          // Additional visual feedback
        if (visualController != null)
        {
            // TODO: Implement OnSelected, OnHovered, OnDeselected methods in MinipollVisualController
            // if (IsSelected)
            //     visualController.OnSelected();            // else if (IsHovered)
            //     visualController.OnHovered();
            // else
            //     visualController.OnDeselected();
        }
    }
    #endregion

    #region Selection Helpers
    public void SelectNext()
    {
        MinipollSelectable[] allSelectables = FindObjectsByType<MinipollSelectable>(FindObjectsSortMode.None);
        
        if (allSelectables.Length <= 1) return;
        
        // Sort by distance from camera
        Array.Sort(allSelectables, (a, b) => 
        {
            if (mainCamera == null) return 0;
            float distA = Vector3.Distance(mainCamera.transform.position, a.transform.position);
            float distB = Vector3.Distance(mainCamera.transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });
        
        // Find current index
        int currentIndex = Array.IndexOf(allSelectables, this);
        int nextIndex = (currentIndex + 1) % allSelectables.Length;
        
        // Select next
        allSelectables[nextIndex].Select();
    }    public void SelectPrevious()
    {
        MinipollSelectable[] allSelectables = FindObjectsByType<MinipollSelectable>(FindObjectsSortMode.None);
        
        if (allSelectables.Length <= 1) return;
        
        // Sort by distance from camera
        Array.Sort(allSelectables, (a, b) => 
        {
            if (mainCamera == null) return 0;
            float distA = Vector3.Distance(mainCamera.transform.position, a.transform.position);
            float distB = Vector3.Distance(mainCamera.transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });
        
        // Find current index
        int currentIndex = Array.IndexOf(allSelectables, this);
        int previousIndex = currentIndex - 1;
        if (previousIndex < 0) previousIndex = allSelectables.Length - 1;
        
        // Select previous
        allSelectables[previousIndex].Select();
    }    public static void DeselectAll()
    {
        MinipollSelectable[] allSelectables = FindObjectsByType<MinipollSelectable>(FindObjectsSortMode.None);
        foreach (var selectable in allSelectables)
        {
            selectable.Deselect();
        }
    }    public static MinipollSelectable[] GetAllSelected()
    {
        MinipollSelectable[] allSelectables = FindObjectsByType<MinipollSelectable>(FindObjectsSortMode.None);
        return Array.FindAll(allSelectables, s => s.IsSelected);
    }
    #endregion

    #region State Management
    public void SetSelectable(bool selectable)
    {
        isSelectable = selectable;
        
        if (!selectable && IsSelected)
        {
            Deselect();
        }
        
        // Update collider
        if (selectionCollider)
            selectionCollider.enabled = selectable;
    }

    public void SetSelectionColor(Color color)
    {
        selectionColor = color;
        UpdateVisualState();
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // Deselect if selected
        if (IsSelected)
        {
            Deselect();
        }
          // Unregister from selection manager
        // TODO: Implement SelectionManager
        // if (SelectionManager.Instance != null)
        // {
        //     SelectionManager.Instance.UnregisterSelectable(this);
        // }
        
        // Clean up materials
        if (outlinedMaterials != null)
        {
            foreach (var mat in outlinedMaterials)
            {
                if (mat != null)
                    Destroy(mat);
            }
        }
    }

    private void OnDisable()
    {
        // Hide visual feedback
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
            
        // Remove hover state
        if (IsHovered)
        {
            IsHovered = false;
            UpdateVisualState();
        }
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        // Draw selection radius
        Gizmos.color = IsSelected ? selectionColor : Color.gray;
        Gizmos.DrawWireSphere(transform.position, selectionRadius);
        
        // Draw indicator position
        if (showSelectionIndicator)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * indicatorHeight, Vector3.one * 0.1f);
        }    }
    #endregion
}