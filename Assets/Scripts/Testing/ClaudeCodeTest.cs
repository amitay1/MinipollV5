using UnityEngine;

namespace MinipollGame.Testing
{
    /// <summary>
    /// Simple test script to verify Claude Code integration with Unity
    /// Creates an empty GameObject named "claude code" when the scene starts
    /// </summary>
    public class ClaudeCodeTest : MonoBehaviour
    {
        [Header("Claude Code Integration Test")]
        [SerializeField] private bool createOnStart = true;
        [SerializeField] private Vector3 spawnPosition = Vector3.zero;
        
        private void Start()
        {
            if (createOnStart)
            {
                CreateClaudeCodeGameObject();
            }
        }
        
        /// <summary>
        /// Creates an empty GameObject named "claude code" for testing purposes
        /// </summary>
        [ContextMenu("Create Claude Code GameObject")]
        public void CreateClaudeCodeGameObject()
        {
            // Check if object already exists
            GameObject existingObject = GameObject.Find("claude code");
            if (existingObject != null)
            {
                Debug.Log("Claude Code GameObject already exists!");
                return;
            }
            
            // Create new empty GameObject
            GameObject claudeCodeObject = new GameObject("claude code");
            claudeCodeObject.transform.position = spawnPosition;
            
            // Add a simple tag for identification
            claudeCodeObject.tag = "Untagged";
            
            Debug.Log("Claude Code GameObject created successfully at position: " + spawnPosition);
            
            // Optional: Add a simple component to make it visible in inspector
            claudeCodeObject.AddComponent<Transform>();
        }
        
        /// <summary>
        /// Removes the Claude Code GameObject if it exists
        /// </summary>
        [ContextMenu("Remove Claude Code GameObject")]
        public void RemoveClaudeCodeGameObject()
        {
            GameObject claudeCodeObject = GameObject.Find("claude code");
            if (claudeCodeObject != null)
            {
                DestroyImmediate(claudeCodeObject);
                Debug.Log("Claude Code GameObject removed successfully!");
            }
            else
            {
                Debug.Log("Claude Code GameObject not found!");
            }
        }
    }
}