using UnityEngine;
using Minipoll.Core.Architecture;

namespace Minipoll.Data
{
    /// <summary>
    /// Default Prefab Registry for the Minipoll game
    /// This can be created as a ScriptableObject in the Unity Editor
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultPrefabRegistry", menuName = "Minipoll/Default Prefab Registry")]
    public class DefaultPrefabRegistry : PrefabRegistryData
    {
        [Header("Auto-Setup")]
        [SerializeField] private bool autoRegisterFoundPrefabs = true;
        
        private void OnEnable()
        {
            if (autoRegisterFoundPrefabs)
            {
                // This will be called when the ScriptableObject is loaded
                // Can be used to auto-register prefabs found in the project
            }
        }
        
        // This would typically be populated in the Unity Editor
        // with references to all the game's prefabs
    }
}
