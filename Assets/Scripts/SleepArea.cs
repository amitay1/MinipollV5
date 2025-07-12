using UnityEngine;
using UnityEngine.InputSystem;

namespace MinipollGame.Test
{
    public class SleepArea : MonoBehaviour
    {
        [Header("Sleep Properties")]
        public float restValue = 20f;
        public float sleepRadius = 3f;
        public KeyCode sleepKey = KeyCode.S;
        public float sleepQuality = 1.0f; // Quality of sleep (0-1)
        
        private void Start()
        {
            // Make sleep area purple
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.magenta;
            }
            gameObject.name = "SleepArea";
        }
        
        private void Update()
        {
            // Check if Minipoll is nearby and player presses sleep key
            if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
            {
                CheckForMinipoll();
            }
        }
        
        private void CheckForMinipoll()
        {
            // Find all Minipoll creatures within sleep radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, sleepRadius);
            
            foreach (Collider collider in colliders)
            {
                var minipoll = collider.GetComponent<MinipollGame.Core.MinipollCore>();
                if (minipoll != null)
                {
                    // Let the minipoll sleep
                    minipoll.Sleep(sleepQuality * restValue);
                    Debug.Log($"[SleepArea] {minipoll.name} is sleeping! Quality: {sleepQuality}");
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw the sleep radius in the scene view
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, sleepRadius);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Auto-trigger when minipoll gets close enough
            var minipoll = other.GetComponent<MinipollGame.Core.MinipollCore>();
            if (minipoll != null)
            {
                Debug.Log($"[SleepArea] {minipoll.name} entered sleep area");
                // Could auto-start sleeping if tired enough
                CheckAutoSleep(minipoll);
            }
        }
        
        private void CheckAutoSleep(MinipollGame.Core.MinipollCore minipoll)
        {
            // Auto-sleep if energy is very low
            var needs = minipoll.GetComponent<MinipollGame.Core.MinipollNeedsSystem>();
            if (needs != null)
            {
                float energyLevel = needs.GetNormalizedNeed("Sleep");
                if (energyLevel < 0.3f) // If less than 30% energy
                {
                    minipoll.Sleep(sleepQuality * restValue);
                    Debug.Log($"[SleepArea] Auto-sleep activated for {minipoll.name}");
                }
            }
        }
    }
}
