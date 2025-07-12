using UnityEngine;
using UnityEngine.InputSystem;

namespace MinipollGame.Test
{
    public class WaterSource : MonoBehaviour
    {
        [Header("Water Properties")]
        public float hydrationValue = 25f;
        public float drinkRadius = 3f;
        public KeyCode drinkKey = KeyCode.W;
        
        private void Start()
        {
            // Make water source blue
            GetComponent<MeshRenderer>().material.color = Color.blue;
            gameObject.name = "WaterSource";
        }
        
        private void Update()
        {
            // Check if Minipoll is nearby and player presses drink key
            if (Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame)
            {
                CheckForMinipoll();
            }
        }
        
        private void CheckForMinipoll()
        {
            // Find all Minipoll creatures within drink radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, drinkRadius);
            
            foreach (Collider collider in colliders)
            {
                var minipoll = collider.GetComponent<MinipollGame.Core.MinipollCore>();
                if (minipoll != null)
                {
                    // Give water to the minipoll
                    minipoll.GiveWater(hydrationValue);
                    Debug.Log($"[WaterSource] Gave {hydrationValue} hydration to {minipoll.name}");
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw the drink radius in the scene view
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, drinkRadius);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Auto-trigger when minipoll gets close enough
            var minipoll = other.GetComponent<MinipollGame.Core.MinipollCore>();
            if (minipoll != null)
            {
                Debug.Log($"[WaterSource] {minipoll.name} is near water source");
            }
        }
    }
}
