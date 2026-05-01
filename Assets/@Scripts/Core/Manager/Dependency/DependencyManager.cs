using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Core.Managers.Dependency.Generated;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core.Managers.Dependency
{
    public sealed class DependencyManager : BaseManager<DependencyManager>
    {
        private readonly Dictionary<Type, object> _globalInstances = new();
        private readonly Dictionary<string, Dictionary<Type, object>> _sceneInstances = new();
        private readonly Dictionary<Type, DependencyDescriptor> _descriptors = new();
        private readonly Dictionary<string, List<Type>> _sceneTypes = new();

        protected override void OnInit()
        {
            CacheDependencyMetadata();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnDispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            DisposeAll();
        }

        public void Inject(object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Type currentType = target.GetType();
            while (currentType != null && currentType != typeof(object))
            {
                FieldInfo[] fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    if (field.GetCustomAttribute<InjectAttribute>() == null)
                        continue;

                    object dependency = Resolve(field.FieldType);
                    field.SetValue(target, dependency);
                }

                currentType = currentType.BaseType;
            }
        }

        public T Instantiate<T>() where T : class, new()
        {
            T instance = new T();
            Inject(instance);
            return instance;
        }

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            if (!_descriptors.TryGetValue(type, out DependencyDescriptor descriptor))
                throw new InvalidOperationException($"Dependency is not registered: {type.FullName}");

            if (descriptor.IsGlobal)
                return ResolveGlobal(type);

            if (string.IsNullOrWhiteSpace(descriptor.SceneName))
                throw new InvalidOperationException($"Dependency scene is not defined: {type.FullName}");

            return ResolveScene(descriptor.SceneName, type);
        }

        public void DisposeScene(string sceneName)
        {
            if (!_sceneInstances.TryGetValue(sceneName, out Dictionary<Type, object> instances))
                return;

            foreach (object instance in instances.Values)
            {
                if (instance is IDisposable disposable)
                    disposable.Dispose();
            }

            instances.Clear();
            _sceneInstances.Remove(sceneName);
        }

        public void DisposeAll()
        {
            foreach (Dictionary<Type, object> instances in _sceneInstances.Values)
            {
                foreach (object instance in instances.Values)
                {
                    if (instance is IDisposable disposable)
                        disposable.Dispose();
                }
            }

            _sceneInstances.Clear();

            foreach (object instance in _globalInstances.Values)
            {
                if (instance is IDisposable disposable)
                    disposable.Dispose();
            }

            _globalInstances.Clear();
        }

        private void CacheDependencyMetadata()
        {
            _descriptors.Clear();
            _sceneTypes.Clear();

            for (int i = 0; i < DependencyRegistry.All.Length; i++)
            {
                DependencyDescriptor descriptor = DependencyRegistry.All[i];
                _descriptors[descriptor.Type] = descriptor;

                if (descriptor.IsGlobal)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(descriptor.SceneName))
                    continue;

                if (!_sceneTypes.TryGetValue(descriptor.SceneName, out List<Type> types))
                {
                    types = new List<Type>();
                    _sceneTypes.Add(descriptor.SceneName, types);
                }

                types.Add(descriptor.Type);
            }
        }

        private async Awaitable PrewarmSceneAsync(string sceneName)
        {
            if (!_sceneTypes.TryGetValue(sceneName, out List<Type> types))
                return;

            for (int i = 0; i < types.Count; i++)
            {
                Resolve(types[i]);
                await Awaitable.NextFrameAsync();
            }
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene loadedScene, LoadSceneMode mode)
        {
            _ = PrewarmSceneAsync(loadedScene.name);
        }

        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene unloadedScene)
        {
            DisposeScene(unloadedScene.name);
        }

        private object ResolveGlobal(Type type)
        {
            if (_globalInstances.TryGetValue(type, out object instance))
                return instance;

            instance = CreateInstance(type);
            _globalInstances.Add(type, instance);
            return instance;
        }

        private object ResolveScene(string sceneName, Type type)
        {
            if (!_sceneInstances.TryGetValue(sceneName, out Dictionary<Type, object> instances))
            {
                instances = new Dictionary<Type, object>();
                _sceneInstances.Add(sceneName, instances);
            }

            if (instances.TryGetValue(type, out object instance))
                return instance;

            instance = CreateInstance(type);
            instances.Add(type, instance);
            return instance;
        }

        private object CreateInstance(Type type)
        {
            object instance = Activator.CreateInstance(type, nonPublic: true);
            Inject(instance);
            return instance;
        }
    }
}
