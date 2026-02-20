using System;
using System.Collections.Generic;

namespace ChainReactionConveyor.Services
{
    /// <summary>
    /// Global event bus for decoupled communication
    /// </summary>
    public class EventBus
    {
        private static EventBus _instance;
        public static EventBus Instance => _instance ??= new EventBus();

        private readonly Dictionary<Type, List<Delegate>> _listeners = new();
        private readonly Dictionary<Type, List<Delegate>> _onceListeners = new();

        private EventBus() { }

        public void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!_listeners.ContainsKey(type))
            {
                _listeners[type] = new List<Delegate>();
            }
            _listeners[type].Add(listener);
        }

        public void SubscribeOnce<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!_onceListeners.ContainsKey(type))
            {
                _onceListeners[type] = new List<Delegate>();
            }
            _onceListeners[type].Add(listener);
        }

        public void Unsubscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (_listeners.TryGetValue(type, out var list))
            {
                list.Remove(listener);
            }
        }

        public void Publish<T>(T eventData)
        {
            var type = typeof(T);

            // Handle one-time listeners first
            if (_onceListeners.TryGetValue(type, out var onceList))
            {
                foreach (var listener in onceList)
                {
                    ((Action<T>)listener)(eventData);
                }
                _onceListeners[type].Clear();
            }

            // Handle permanent listeners
            if (_listeners.TryGetValue(type, out var list))
            {
                foreach (var listener in list)
                {
                    ((Action<T>)listener)(eventData);
                }
            }
        }

        public void Clear()
        {
            _listeners.Clear();
            _onceListeners.Clear();
        }

        public void Clear<T>()
        {
            _listeners.Remove(typeof(T));
            _onceListeners.Remove(typeof(T));
        }
    }

    #region Game Events

    public struct LevelStartEvent
    {
        public int LevelId;
        public int Seed;
    }

    public struct LevelCompleteEvent
    {
        public int LevelId;
        public int Score;
    }

    public struct LevelFailEvent
    {
        public int LevelId;
        public string Reason;
    }

    public struct ItemPlacedEvent
    {
        public int X;
        public int Y;
        public int ItemId;
    }

    public struct ChainResolvedEvent
    {
        public int ClusterSize;
        public int ChainDepth;
    }

    public struct ComboActivatedEvent
    {
        public int ComboCount;
        public string BonusType;
    }

    public struct BoosterUsedEvent
    {
        public string BoosterType;
    }

    public struct ContinueUsedEvent
    {
        public int LevelId;
    }

    #endregion
}
