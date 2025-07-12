using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minipoll.Core.Events
{
    /// <summary>
    /// Central Event System - Level 1 Foundation System
    /// Provides centralized communication hub for all game systems
    /// Prevents tight coupling between systems through event-driven architecture
    /// </summary>
    public static class EventSystem
    {
        private static Dictionary<Type, List<object>> eventSubscribers = new Dictionary<Type, List<object>>();
        private static Dictionary<string, List<Action<object>>> namedEventSubscribers = new Dictionary<string, List<Action<object>>>();
        
        #region Generic Event System
        
        /// <summary>
        /// Subscribe to a typed event
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);
            
            if (!eventSubscribers.ContainsKey(eventType))
                eventSubscribers[eventType] = new List<object>();
                
            eventSubscribers[eventType].Add(handler);
            
            Debug.Log($"[EventSystem] Subscribed to {eventType.Name}. Total subscribers: {eventSubscribers[eventType].Count}");
        }
        
        /// <summary>
        /// Unsubscribe from a typed event
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);
            
            if (eventSubscribers.ContainsKey(eventType))
            {
                eventSubscribers[eventType].Remove(handler);
                Debug.Log($"[EventSystem] Unsubscribed from {eventType.Name}. Remaining subscribers: {eventSubscribers[eventType].Count}");
            }
        }
        
        /// <summary>
        /// Publish a typed event to all subscribers
        /// </summary>
        public static void Publish<T>(T eventData) where T : class
        {
            Type eventType = typeof(T);
            
            if (eventSubscribers.ContainsKey(eventType))
            {
                foreach (var subscriber in eventSubscribers[eventType])
                {
                    try
                    {
                        ((Action<T>)subscriber).Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventSystem] Error in event handler for {eventType.Name}: {ex.Message}");
                    }
                }
                
                Debug.Log($"[EventSystem] Published {eventType.Name} to {eventSubscribers[eventType].Count} subscribers");
            }
        }
        
        #endregion
        
        #region Named Event System (for simple events)
        
        /// <summary>
        /// Subscribe to a named event with data
        /// </summary>
        public static void Subscribe(string eventName, Action<object> handler)
        {
            if (!namedEventSubscribers.ContainsKey(eventName))
                namedEventSubscribers[eventName] = new List<Action<object>>();
                
            namedEventSubscribers[eventName].Add(handler);
            Debug.Log($"[EventSystem] Subscribed to named event '{eventName}'. Total subscribers: {namedEventSubscribers[eventName].Count}");
        }
        
        /// <summary>
        /// Subscribe to a named event without data
        /// </summary>
        public static void Subscribe(string eventName, Action handler)
        {
            Subscribe(eventName, _ => handler());
        }
        
        /// <summary>
        /// Unsubscribe from a named event
        /// </summary>
        public static void Unsubscribe(string eventName, Action<object> handler)
        {
            if (namedEventSubscribers.ContainsKey(eventName))
            {
                namedEventSubscribers[eventName].Remove(handler);
                Debug.Log($"[EventSystem] Unsubscribed from named event '{eventName}'. Remaining subscribers: {namedEventSubscribers[eventName].Count}");
            }
        }
        
        /// <summary>
        /// Publish a named event with data
        /// </summary>
        public static void Publish(string eventName, object data = null)
        {
            if (namedEventSubscribers.ContainsKey(eventName))
            {
                foreach (var subscriber in namedEventSubscribers[eventName])
                {
                    try
                    {
                        subscriber.Invoke(data);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventSystem] Error in named event handler for '{eventName}': {ex.Message}");
                    }
                }
                
                Debug.Log($"[EventSystem] Published named event '{eventName}' to {namedEventSubscribers[eventName].Count} subscribers");
            }
        }
        
        #endregion
        
        #region System Management
        
        /// <summary>
        /// Clear all event subscriptions (useful for scene transitions)
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            int totalSubscribers = 0;
            foreach (var kvp in eventSubscribers)
                totalSubscribers += kvp.Value.Count;
            foreach (var kvp in namedEventSubscribers)
                totalSubscribers += kvp.Value.Count;
                
            eventSubscribers.Clear();
            namedEventSubscribers.Clear();
            
            Debug.Log($"[EventSystem] Cleared all subscriptions. Total removed: {totalSubscribers}");
        }
        
        /// <summary>
        /// Get debug information about current subscriptions
        /// </summary>
        public static string GetDebugInfo()
        {
            var info = "[EventSystem Debug Info]\n";
            
            info += "Typed Events:\n";
            foreach (var kvp in eventSubscribers)
            {
                info += $"  {kvp.Key.Name}: {kvp.Value.Count} subscribers\n";
            }
            
            info += "Named Events:\n";
            foreach (var kvp in namedEventSubscribers)
            {
                info += $"  '{kvp.Key}': {kvp.Value.Count} subscribers\n";
            }
            
            return info;
        }
        
        #endregion
    }
}

namespace Minipoll.Core.Events.Types
{
    /// <summary>
    /// Base class for all game events
    /// </summary>
    public abstract class GameEvent
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public virtual string EventName => GetType().Name;
    }
    
    // Core Foundation Events
    public class SystemInitializedEvent : GameEvent
    {
        public string SystemName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    
    public class SystemShutdownEvent : GameEvent
    {
        public string SystemName { get; set; }
    }
    
    // Scene Management Events
    public class SceneLoadStartEvent : GameEvent
    {
        public string SceneName { get; set; }
    }
    
    public class SceneLoadCompleteEvent : GameEvent
    {
        public string SceneName { get; set; }
        public bool Success { get; set; }
    }
    
    // Save/Load Events
    public class GameSaveEvent : GameEvent
    {
        public string SaveSlot { get; set; }
        public bool AutoSave { get; set; }
    }
    
    public class GameLoadEvent : GameEvent
    {
        public string SaveSlot { get; set; }
        public bool Success { get; set; }
    }
    
    // Creature Events
    public class MinipollSpawnedEvent : GameEvent
    {
        public GameObject MinipollObject { get; set; }
        public string MinipollId { get; set; }
    }
    
    public class MinipollNeedChangedEvent : GameEvent
    {
        public string MinipollId { get; set; }
        public string NeedType { get; set; }
        public float OldValue { get; set; }
        public float NewValue { get; set; }
    }
    
    // Interaction Events
    public class PlayerInteractionEvent : GameEvent
    {
        public GameObject TargetObject { get; set; }
        public string InteractionType { get; set; }
        public Vector3 Position { get; set; }
    }
    
    // Audio Events
    public class AudioRequestEvent : GameEvent
    {
        public string AudioClipName { get; set; }
        public Vector3 Position { get; set; }
        public float Volume { get; set; } = 1f;
        public bool Loop { get; set; } = false;
    }
    
    // Weather Events
    public class WeatherChangeEvent : GameEvent
    {
        public string WeatherType { get; set; }
        public float Intensity { get; set; }
        public float Duration { get; set; }
    }
}
