using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 8, -10);
    public float smoothTime = 0.3f;
    
    private Vector3 velocity = Vector3.zero;
    
    void Start()
    {
        // חפש את הדמות Minipoll אוטומטית
        if (target == null)
        {
            GameObject minipoll = GameObject.Find("Minipoll");
            if (minipoll != null)
            {
                target = minipoll.transform;
                Debug.Log("SimpleCameraFollow: מחובר לדמות Minipoll");
            }
        }
        
        // הגדר זווית של 45 מעלות
        transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // חשב מיקום רצוי
        Vector3 targetPosition = target.position + offset;
        
        // זוז בצורה חלקה למיקום החדש
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        
        // שמור על זווית קבועה של 45 מעלות
        transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }
}
