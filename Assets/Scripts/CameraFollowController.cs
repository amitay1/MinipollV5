using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // הדמות שהמצלמה תעקוב אחריה
    
    [Header("Camera Settings")]
    public float followDistance = 10f; // מרחק המצלמה מהדמות
    public float heightOffset = 5f; // גובה המצלמה מעל הדמות
    public float followSpeed = 2f; // מהירות מעקב
    public float rotationSpeed = 2f; // מהירות סיבוב
    
    [Header("Angle Settings")]
    public float cameraAngle = 45f; // זווית המצלמה (45 מעלות)
    public bool lockAngle = true; // נעל את הזווית
    
    [Header("Smoothing")]
    public bool smoothFollow = true;
    public bool smoothRotation = true;
    
    private Vector3 desiredPosition;
    private Vector3 currentVelocity;
    
    void Start()
    {
        // אם לא הוגדרה מטרה, נחפש את הדמות Minipoll
        if (target == null)
        {
            GameObject minipoll = GameObject.Find("Minipoll");
            if (minipoll != null)
            {
                target = minipoll.transform;
                Debug.Log("CameraFollowController: נמצאה דמות Minipoll אוטומטית");
            }
            else
            {
                Debug.LogWarning("CameraFollowController: לא נמצאה דמות לעקוב אחריה!");
            }
        }
        
        // הגדר את המיקום והזווית הראשונית
        if (target != null)
        {
            SetInitialPosition();
        }
    }
    
    void SetInitialPosition()
    {
        // חשב מיקום ראשוני בזווית 45 מעלות
        Vector3 direction = new Vector3(0, 0, -followDistance);
        direction = Quaternion.Euler(cameraAngle, 0, 0) * direction;
        
        transform.position = target.position + direction + Vector3.up * heightOffset;
        transform.LookAt(target.position + Vector3.up * (heightOffset * 0.5f));
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        FollowTarget();
        LookAtTarget();
    }
    
    void FollowTarget()
    {
        // חשב מיקום רצוי מאחורי המטרה בזווית 45 מעלות
        Vector3 targetDirection = target.forward;
        
        // צור וקטור מיקום ברצוי - מאחורי הדמות ולמעלה
        Vector3 offset = -targetDirection * followDistance + Vector3.up * heightOffset;
        
        // הוסף זווית של 45 מעלות
        offset = Quaternion.Euler(cameraAngle, 0, 0) * offset;
        
        desiredPosition = target.position + offset;
        
        // זוז למיקום הרצוי
        if (smoothFollow)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                desiredPosition, 
                ref currentVelocity, 
                1f / followSpeed
            );
        }
        else
        {
            transform.position = desiredPosition;
        }
    }
    
    void LookAtTarget()
    {
        if (lockAngle)
        {
            // נעל את הזווית ותמיד הסתכל על הדמות
            Vector3 lookDirection = target.position - transform.position;
            lookDirection.Normalize();
            
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);
            
            if (smoothRotation)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    desiredRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                transform.rotation = desiredRotation;
            }
        }
        else
        {
            // מצב חופשי - המצלמה יכולה לסוב חופשית
            Vector3 targetPoint = target.position + Vector3.up * (heightOffset * 0.5f);
            transform.LookAt(targetPoint);
        }
    }
    
    // פונקציות עזר לשינוי הגדרות בזמן ריצה
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            SetInitialPosition();
        }
    }
    
    public void SetCameraAngle(float newAngle)
    {
        cameraAngle = newAngle;
    }
    
    public void SetFollowDistance(float newDistance)
    {
        followDistance = newDistance;
    }
    
    public void SetHeightOffset(float newHeight)
    {
        heightOffset = newHeight;
    }
    
    // הצג קווי עזר בעורך
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // הצג קו חיבור בין המצלמה למטרה
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position);
        
        // הצג את המיקום הרצוי
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(desiredPosition, 0.5f);
        
        // הצג את כיוון המבט
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}
