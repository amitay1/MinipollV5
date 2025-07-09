using UnityEngine;

namespace MinipollGame.Test
{
    public class SimpleCreatureController : MonoBehaviour
    {
        [Header("Basic Properties")]
        public string creatureName = "SimpleCreature";
        public float health = 100f;
        public float hunger = 75f;
        public float thirst = 75f;
        
        [Header("Movement")]
        public float moveSpeed = 3f;
        
        private Rigidbody rb;
        private Material mat;
        
        private void Start()
        {
            // Get components
            rb = GetComponent<Rigidbody>();
            mat = GetComponent<MeshRenderer>().material;
            
            // Set name and color
            gameObject.name = creatureName;
            mat.color = Color.cyan;
            
            Debug.Log($"[SimpleCreatureController] {creatureName} is alive!");
        }
        
        private void Update()
        {
            // Decrease needs over time
            hunger -= Time.deltaTime * 2f;
            thirst -= Time.deltaTime * 3f;
            
            // Clamp values
            hunger = Mathf.Max(0, hunger);
            thirst = Mathf.Max(0, thirst);
            
            // Update color based on needs
            UpdateVisuals();
            
            // Simple random movement
            if (Random.Range(0f, 1f) < 0.005f) // 0.5% chance per frame
            {
                MoveRandomly();
            }
        }
        
        private void UpdateVisuals()
        {
            if (hunger < 25f)
            {
                mat.color = Color.red; // Hungry = red
            }
            else if (thirst < 25f)
            {
                mat.color = Color.blue; // Thirsty = blue
            }
            else
            {
                mat.color = Color.cyan; // Happy = cyan
            }
        }
        
        private void MoveRandomly()
        {
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
            
            rb.AddForce(randomDirection * moveSpeed, ForceMode.Impulse);
        }
        
        public void Feed()
        {
            hunger = Mathf.Min(100f, hunger + 30f);
            Debug.Log($"{creatureName} was fed! Hunger: {hunger:F1}");
        }
        
        public void GiveWater()
        {
            thirst = Mathf.Min(100f, thirst + 40f);
            Debug.Log($"{creatureName} drank water! Thirst: {thirst:F1}");
        }
    }
}