/***************************************************************
 *  InputManager.cs
 *
 *  תיאור כללי:
 *    מנהל Singleton שמטפל בקלט המשתמש:
 *      1. תנועת מצלמה (Pan/Zoom/Rotate) עם WASD/עכבר/גלגלת.
 *      2. קליק שמאלי על העולם - הפעלת אירוע (למשל בחירת Minipoll).
 *      3. קליק ימני (בונוס) - תפריט הקשר או פעולות אחרות.
 *      4. קיצורי מקשים (Escape לפאוז, F1/F2 לשינויי DEBUG, וכו’).
 *
 *    השיטה היא Legacy Input:
 *      - Input.GetAxis("Horizontal"/"Vertical"/"Mouse X"/"Mouse Y") וכו’.
 *    התמיכה ב-New Input System תצריך סקריפטים אחרים.
 *
 *  דרישות קדם:
 *    - לשים את הסקריפט על אובייקט בסצנה (למשל "InputManager").
 *    - לבטל "CameraController" נפרד אם יש, או למזג לוגיקה כאן.
 *    - להשתמש במצלמה ראשית עם תג "MainCamera".
 *
 ***************************************************************/

using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    #region Singleton
    private static InputManager _instance;
    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("InputManager_AutoCreated");
                _instance = go.AddComponent<InputManager>();
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
    }
    #endregion

    #region Camera Control Fields

    [Header("Camera References")]
    public Camera mainCamera; // המצלמה הראשית
    [Tooltip("מהירות תנועה של המצלמה ב-WASD")]
    public float panSpeed = 10f;
    [Tooltip("מהירות סיבוב המצלמה ב-Q/E או החזקת מיקרה עכבר")]
    public float rotationSpeed = 50f;
    [Tooltip("טווח זום על ידי גלגלת העכבר")]
    public float zoomSpeed = 10f;
    [Tooltip("מינ’ ומקס’ מרחק למצלמה (אם בפרספקטיבה) או size (אם באורתוגרפית)")]
    public float minZoom = 5f;
    public float maxZoom = 30f;

    // שמירה של זווית סיבוב אופקית אם נרצה מצלמה isometric-ish
    private float currentYaw = 0f;

    #endregion

    #region Mouse Click Events

    // אירוע למי שרוצה להאזין לקליק שמאלי בעולם
    public event Action<RaycastHit> OnLeftClickWorld;
    // אירוע לקליק ימני בעולם
    public event Action<RaycastHit> OnRightClickWorld;

    [Header("Click Settings")]
    [Tooltip("אורך Raycast לחיפוש אובייקטים")]
    public float raycastDistance = 100f;
    #endregion

    #region Other Keys / Shortcuts

    // אפשר להגדיר אירועים נוספים, כמו OnEscapePressed
    public event Action OnEscapePressed;

    #endregion

    private void Start()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
            if (!mainCamera)
            {
                Debug.LogError("No Main Camera found or assigned to InputManager!");
            }
        }
    }

    private void Update()
    {
        if (!mainCamera) return;

        HandleCameraMovement();
        HandleMouseClicks();
        HandleShortcuts();
    }

    #region Camera Movement / Zoom

    private void HandleCameraMovement()
    {
        // 1) תנועה אופקית עם מקשי W/A/S/D
        float h = Input.GetAxis("Horizontal");   // A= -1, D= +1
        float v = Input.GetAxis("Vertical");     // S= -1, W= +1
        Vector3 move = new Vector3(h, 0f, v);

        // לוקחים בחשבון שהמצלמה כבר מסובבת?
        // אפשר לנוע מקומית (transform.right/forward) או גלובלית
        // כאן נפשט: נעה בכיוון local של המצלמה (בלי tilt)
        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0f; // מבטלים רכיב אנכי
        camForward.Normalize();

        Vector3 camRight = mainCamera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 desiredMove = (camForward * v + camRight * h) * panSpeed * Time.deltaTime;
        mainCamera.transform.position += desiredMove;

        // 2) סיבוב המצלמה עם Q/E
        if (Input.GetKey(KeyCode.Q))
        {
            currentYaw -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            currentYaw += rotationSpeed * Time.deltaTime;
        }

        // בנוסף, אפשר לאפשר סיבוב עם מחזיקת כפתור עכבר אמצעי
        if (Input.GetMouseButton(2)) // Middle mouse
        {
            float mx = Input.GetAxis("Mouse X");
            currentYaw += mx * rotationSpeed * 2f * Time.deltaTime;
        }

        mainCamera.transform.RotateAround(mainCamera.transform.position, Vector3.up, currentYaw - mainCamera.transform.eulerAngles.y);

        // 3) זום עם גלגלת העכבר
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // חיובי = גלגלת מעלה, שלילי = גלגלת מטה
        if (scroll != 0f)
        {
            // אם המצלמה פרספקטיבה, נשנה את fieldOfView
            // אם אורתו, נשנה את camera.orthographicSize
            // כאן נניח פרספקטיבה
            float fov = mainCamera.fieldOfView;
            fov -= scroll * zoomSpeed;
            fov = Mathf.Clamp(fov, minZoom, maxZoom);
            mainCamera.fieldOfView = fov;
        }
    }

    #endregion

    #region Mouse Click Handling

    private void HandleMouseClicks()
    {
        // Left Click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                OnLeftClickWorld?.Invoke(hit);
            }
        }

        // Right Click
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                OnRightClickWorld?.Invoke(hit);
            }
        }
    }

    #endregion

    #region Keyboard Shortcuts

    private void HandleShortcuts()
    {
        // Escape => Pause/Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapePressed?.Invoke();
        }

        // אפשר להוסיף נוספים, למשל:
        // if (Input.GetKeyDown(KeyCode.F1)) { Debug.Log("F1 pressed"); }
    }

    #endregion
}
