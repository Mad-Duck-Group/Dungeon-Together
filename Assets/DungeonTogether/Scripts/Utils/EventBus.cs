using System;
using System.Collections.Generic;

namespace DungeonTogether.Scripts.Utils
{
    /// <summary>
    /// Inherit from this interface to handle events from the EventBus.
    /// </summary>
    /// <typeparam name="T">Type of the event data.</typeparam>
    public interface IEventBusHandler<in T> where T : struct
    {
        /// <summary>
        /// Handles the event data.
        /// </summary>
        /// <param name="eventData">Event data to handle.</param>
        void OnHandleEvent(T eventData);
    }
    /// <summary>
    /// Event bus design patten that allows for decoupled communication between objects.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<object>> EventListeners = new();
        
        /// <summary>
        /// Subscribes to the event type.
        /// </summary>
        /// <param name="listener">Listener to subscribe.</param>
        /// <typeparam name="T">Type of the event data.</typeparam>
        public static void Subscribe<T>(this IEventBusHandler<T> listener) where T : struct
        {
            var type = typeof(T);
            if (!EventListeners.ContainsKey(type))
            {
                EventListeners[type] = new List<object> { listener };
            }
            else
            {
                EventListeners[type].Add(listener);
            }
        }
        
        /// <summary>
        /// Unsubscribes from the event type.
        /// </summary>
        /// <param name="listener">Listener to unsubscribe.</param>
        /// <typeparam name="T">Type of the event data.</typeparam>
        public static void Unsubscribe<T>(this IEventBusHandler<T> listener) where T : struct
        {
            var type = typeof(T);
            if (!EventListeners.ContainsKey(type))
            {
                return;
            }
            EventListeners[type].Remove(listener);
            if (EventListeners[type].Count == 0)
            {
                EventListeners.Remove(type);
            }
        }
        
        /// <summary>
        /// Invokes the event with the specified event data. Note: It is recommended to invoke from the static method of the event data.
        /// </summary>
        /// <param name="eventData">Event data to invoke.</param>
        /// <typeparam name="T">Type of the event data.</typeparam>
        public static void Invoke<T>(this T eventData) where T : struct
        {
            var type = typeof(T);
            if (!EventListeners.ContainsKey(type))
            {
                return;
            }
            foreach (var listener in EventListeners[type])
            {
                ((IEventBusHandler<T>) listener).OnHandleEvent(eventData);
            }
        }
    }
}
