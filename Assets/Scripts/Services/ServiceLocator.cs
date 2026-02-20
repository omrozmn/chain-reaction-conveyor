using System;
using System.Collections.Generic;

namespace ChainReactionConveyor.Services
{
    /// <summary>
    /// Simple service locator for dependency injection
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        public static ServiceLocator Instance => _instance ??= new ServiceLocator();

        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<Type, Func<object>> _factories = new();

        private ServiceLocator() { }

        public void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
            UnityEngine.Debug.Log($"[ServiceLocator] Registered: {typeof(T).Name}");
        }

        public void RegisterFactory<T>(Func<T> factory) where T : class
        {
            _factories[typeof(T)] = () => factory();
        }

        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            if (_factories.TryGetValue(typeof(T), out var factory))
            {
                var instance = (T)factory();
                _services[typeof(T)] = instance;
                return instance;
            }

            throw new InvalidOperationException($"Service {typeof(T).Name} not registered!");
        }

        public T GetOrNull<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            return null;
        }

        public bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T)) || _factories.ContainsKey(typeof(T));
        }

        public void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
            _factories.Remove(typeof(T));
        }

        public void Clear()
        {
            _services.Clear();
            _factories.Clear();
        }
    }
}
