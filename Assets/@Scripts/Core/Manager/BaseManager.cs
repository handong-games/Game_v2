using System;
using UnityEngine;

namespace Game.Core.Managers
{
    public abstract class BaseManager<T> where T : BaseManager<T>
    {
        private static T _instance;
        private static bool _isInitialized;
        private static bool _isPostInitialized;
        public static T Instance => _instance;

        internal static void Create()
        {
            if (_instance != null)
                return;

            _instance = (T)Activator.CreateInstance(typeof(T), true);
        }

        internal static void Init()
        {
            if (_isInitialized)
                return;

            if (_instance == null)
            {
                Create();
            }

            _instance.OnInit();
            _isInitialized = true;
        }

        internal static void PostInit()
        {
            if (_isPostInitialized)
                return;

            _instance.OnPostInit();
            _isPostInitialized = true;
        }
        
        internal static void Dispose()
        {
            if (_instance != null)
            {
                _instance.OnDispose();
                _instance = null;
                _isInitialized = false;
                _isPostInitialized = false;
            }
        }
        
        protected abstract void OnInit();
        protected virtual void OnPostInit()
        {
        }

        protected abstract void OnDispose();
    }
}
