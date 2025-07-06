// Assets/Scripts/Core/GameEventBus.cs
using System;
using System.Collections.Generic;


namespace MinipollGame.Core
{
    /// <summary> Bus מאוחד – Zero‑GC, Generic‑only. </summary>
    public static class GameEventBus
    {
        // =========  Private  =========
        private static readonly Dictionary<Type, List<Delegate>> _subs = new();

        public static object Instance { get; internal set; }

        // =========  Public  =========
        /// <summary> Subscribe once; חוסם כפילויות. </summary>
        public static void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!_subs.TryGetValue(type, out var list))
            {
                list = new List<Delegate>(4);
                _subs[type] = list;
            }
            if (!list.Contains(listener)) list.Add(listener);
        }

        /// <summary> הסרה בטוחה; OK אם לא נרשמת. </summary>
        public static void Unsubscribe<T>(Action<T> listener)
        {
            if (_subs.TryGetValue(typeof(T), out var list))
                list.Remove(listener);
        }

        /// <summary> פרסום אירוע. 0 GC: אין Boxing כי T מחייב struct/class. </summary>
        public static void Publish<T>(T evt)
        {
            if (!_subs.TryGetValue(typeof(T), out var list)) return;

            // יצירת עותק מקומי כדי לאפשר הסרה במהלך האיתור
            var tmp = list.ToArray();
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i] is Action<T> cb) cb.Invoke(evt);
            }
        }

        /// <summary> ניקוי מוחלט – קריאה נדירה (טעינת Scene). </summary>
        public static void ClearAll() => _subs.Clear();
    }
}
