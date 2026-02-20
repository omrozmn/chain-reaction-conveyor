using UnityEngine;
using System;

namespace ChainReactionConveyor.Mechanics
{
    /// <summary>
    /// Interface for game mechanics (Conveyor, Board, etc.)
    /// </summary>
    public interface IMechanic
    {
        void Initialize();
        void Shutdown();
        void OnLevelStart();
        void OnLevelEnd();
        void OnUpdate(float deltaTime);
    }

    /// <summary>
    /// Base class for mechanics with common functionality
    /// </summary>
    public abstract class BaseMechanic : MonoBehaviour, IMechanic
    {
        protected bool _isInitialized = false;
        protected bool _isPlaying = false;

        public virtual void Initialize()
        {
            _isInitialized = true;
            Debug.Log($"[{GetType().Name}] Initialized");
        }

        public virtual void Shutdown()
        {
            _isInitialized = false;
            _isPlaying = false;
            Debug.Log($"[{GetType().Name}] Shutdown");
        }

        public virtual void OnLevelStart()
        {
            _isPlaying = true;
            Debug.Log($"[{GetType().Name}] Level Started");
        }

        public virtual void OnLevelEnd()
        {
            _isPlaying = false;
            Debug.Log($"[{GetType().Name}] Level Ended");
        }

        public virtual void OnUpdate(float deltaTime) { }

        protected void Publish<T>(T eventData) where T : struct
        {
            Services.EventBus.Instance.Publish(eventData);
        }
    }
}
