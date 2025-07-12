using UnityEngine;

namespace MinipollGame.Utils
{
    /// <summary>
    /// Global debug logging controller to manage console spam
    /// Use this instead of Debug.Log directly for gameplay messages
    /// </summary>
    public static class DebugLogger
    {
        [Header("Debug Settings")]
        public static bool EnableUILogs = false;
        public static bool EnableMovementLogs = false;
        public static bool EnableMinipollCoreLogs = false;
        public static bool EnableEmotionLogs = false;
        public static bool EnableAILogs = false;
        
        // Keep errors and warnings always enabled for critical issues
        public static bool EnableErrors = true;
        public static bool EnableWarnings = true;
        
        /// <summary>
        /// Log UI related messages
        /// </summary>
        public static void LogUI(string message)
        {
            if (EnableUILogs)
                Debug.Log($"[UI] {message}");
        }
        
        /// <summary>
        /// Log movement related messages
        /// </summary>
        public static void LogMovement(string message)
        {
            if (EnableMovementLogs)
                Debug.Log($"[Movement] {message}");
        }
        
        /// <summary>
        /// Log MinipollCore related messages
        /// </summary>
        public static void LogCore(string message)
        {
            if (EnableMinipollCoreLogs)
                Debug.Log($"[Core] {message}");
        }
        
        /// <summary>
        /// Log emotion system messages
        /// </summary>
        public static void LogEmotion(string message)
        {
            if (EnableEmotionLogs)
                Debug.Log($"[Emotion] {message}");
        }
        
        /// <summary>
        /// Log AI system messages
        /// </summary>
        public static void LogAI(string message)
        {
            if (EnableAILogs)
                Debug.Log($"[AI] {message}");
        }
        
        /// <summary>
        /// Always log errors (can be disabled if needed)
        /// </summary>
        public static void LogError(string message)
        {
            if (EnableErrors)
                Debug.LogError(message);
        }
        
        /// <summary>
        /// Always log warnings (can be disabled if needed)
        /// </summary>
        public static void LogWarning(string message)
        {
            if (EnableWarnings)
                Debug.LogWarning(message);
        }
        
        /// <summary>
        /// Enable all debug categories for troubleshooting
        /// </summary>
        public static void EnableAllLogs()
        {
            EnableUILogs = true;
            EnableMovementLogs = true;
            EnableMinipollCoreLogs = true;
            EnableEmotionLogs = true;
            EnableAILogs = true;
        }
        
        /// <summary>
        /// Disable all debug categories for clean console
        /// </summary>
        public static void DisableAllLogs()
        {
            EnableUILogs = false;
            EnableMovementLogs = false;
            EnableMinipollCoreLogs = false;
            EnableEmotionLogs = false;
            EnableAILogs = false;
        }
    }
}
