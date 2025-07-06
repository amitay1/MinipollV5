using System;
using System.Collections.Generic;
using UnityEngine;

namespace MinipollGame.Core
{
    /// <summary>
    /// Custom Event System for the Minipoll game
    /// Handles all game events and communication between systems
    /// </summary>
    public class EventSystem : MonoBehaviour
    {
        private static EventSystem _instance;
        
        public static EventSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<EventSystem>();
                    if (_instance == null)
                    {
                        GameObject eventSystemGO = new GameObject("Custom EventSystem");
                        _instance = eventSystemGO.AddComponent<EventSystem>();
                        DontDestroyOnLoad(eventSystemGO);
                    }
                }
                return _instance;
            }
        }

        // Event handlers storage
        private Dictionary<Type, List<Delegate>> eventHandlers = new Dictionary<Type, List<Delegate>>();
        private Dictionary<string, List<Action<object>>> namedEventHandlers = new Dictionary<string, List<Action<object>>>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #region Generic Type-Safe Events

        /// <summary>
        /// Subscribe to an event with type safety
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);
            if (!eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] = new List<Delegate>();
            }
            eventHandlers[eventType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);
            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType].Remove(handler);
                if (eventHandlers[eventType].Count == 0)
                {
                    eventHandlers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Publish an event
        /// </summary>
        public void Publish<T>(T eventData) where T : class
        {
            Type eventType = typeof(T);
            if (eventHandlers.ContainsKey(eventType))
            {
                var handlers = eventHandlers[eventType].ToArray(); // Copy to avoid modification during iteration
                foreach (var handler in handlers)
                {
                    try
                    {
                        (handler as Action<T>)?.Invoke(eventData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in event handler for {eventType}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Publish an event with additional data
        /// </summary>
        public void Publish<T>(T eventData, Dictionary<string, object> additionalData) where T : class
        {
            // For now, just call the regular publish method
            // Can be extended later to handle additional data
            Publish(eventData);
        }

        #endregion

        #region String-Based Events (Legacy Support)

        /// <summary>
        /// Subscribe to an event by name (less recommended)
        /// </summary>
        public void Subscribe<T>(string eventName, Action<object> handler)
        {
            if (!namedEventHandlers.ContainsKey(eventName))
            {
                namedEventHandlers[eventName] = new List<Action<object>>();
            }
            namedEventHandlers[eventName].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from a named event
        /// </summary>
        public void Unsubscribe<T>(string eventName, Action<object> handler)
        {
            if (namedEventHandlers.ContainsKey(eventName))
            {
                namedEventHandlers[eventName].Remove(handler);
                if (namedEventHandlers[eventName].Count == 0)
                {
                    namedEventHandlers.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// Publish a named event
        /// </summary>
        public void Publish(string eventName, object eventData)
        {
            if (namedEventHandlers.ContainsKey(eventName))
            {
                var handlers = namedEventHandlers[eventName].ToArray();
                foreach (var handler in handlers)
                {
                    try
                    {
                        handler?.Invoke(eventData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in named event handler for {eventName}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Publish event with dictionary data - used by some systems
        /// </summary>
        public void Publish(Dictionary<string, object> eventData)
        {
            if (eventData.ContainsKey("location") && eventData.ContainsKey("amount"))
            {
                // This appears to be a nutrient decomposition event
                // Handle it appropriately or just log for now
                Debug.Log($"Decomposition event: location={eventData["location"]}, amount={eventData["amount"]}");
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clear all subscriptions
        /// </summary>
        public void ClearAllSubscriptions()
        {
            eventHandlers.Clear();
            namedEventHandlers.Clear();
        }

        /// <summary>
        /// Clear subscriptions for a specific event type
        /// </summary>
        public void ClearSubscriptions<T>() where T : class
        {
            Type eventType = typeof(T);
            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers.Remove(eventType);
            }
        }

        private void OnDestroy()
        {
            ClearAllSubscriptions();
        }

        #endregion

        #region Debug

        public void LogEventStats()
        {
            Debug.Log("=== Event System Statistics ===");
            Debug.Log($"Generic Events: {eventHandlers.Count} types");
            foreach (var kvp in eventHandlers)
            {
                Debug.Log($"  {kvp.Key.Name}: {kvp.Value.Count} handlers");
            }

            Debug.Log($"Named Events: {namedEventHandlers.Count} types");
            foreach (var kvp in namedEventHandlers)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value.Count} handlers");
            }
        }

        #endregion
    }
}
