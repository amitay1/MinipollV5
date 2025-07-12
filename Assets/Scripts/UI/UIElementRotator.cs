using UnityEngine;

namespace MinipollGame.UI
{
    /// <summary>
    /// Simple component to rotate UI elements continuously
    /// </summary>
    public class UIElementRotator : MonoBehaviour
    {
        public float rotationSpeed = 30f;

        void Update()
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
}
