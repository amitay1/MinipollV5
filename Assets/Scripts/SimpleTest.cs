using UnityEngine;

public class SimpleTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("SIMPLE TEST START");
        print("SIMPLE TEST PRINT");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T KEY PRESSED");
        }
    }
}
