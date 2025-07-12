using UnityEngine;
using UnityEngine.InputSystem;

namespace MinipollGame.Test
{
    public class FoodSource : MonoBehaviour
    {
        [Header("Food Properties")]
        public float nutritionValue = 30f;
        public float feedRadius = 3f;
        public KeyCode feedKey = KeyCode.F;
        
        private void Start()
        {
            // Make food source green
            GetComponent<MeshRenderer>().material.color = Color.green;
            gameObject.name = "FoodSource";
        }
        
        private void Update()
        {
            // Feed nearby creatures when F is pressed
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                FeedNearbyCreatures();
            }
        }
        
        private void FeedNearbyCreatures()
        {
            // Find all creatures in radius
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, feedRadius);
            
            int fedCount = 0;
            foreach (Collider col in nearbyObjects)
            {
                SimpleCreatureController creature = col.GetComponent<SimpleCreatureController>();
                if (creature != null)
                {
                    creature.Feed();
                    fedCount++;
                }
            }
            
            if (fedCount > 0)
            {
                Debug.Log($"[FoodSource] Fed {fedCount} creatures!");
                
                // Visual feedback - flash white
                StartCoroutine(FlashColor());
            }
        }
        
        private System.Collections.IEnumerator FlashColor()
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            Color originalColor = renderer.material.color;
            
            // Flash to white
            renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.2f);
            
            // Back to green
            renderer.material.color = originalColor;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw feed radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, feedRadius);
        }
        
        private void OnMouseDown()
        {
            // Alternative way to feed - click on food source
            FeedNearbyCreatures();
        }
    }
}
