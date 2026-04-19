using System;
using UnityEngine;

namespace Game.Core.Managers
{
    public abstract class BaseManager<T> where T : BaseManager<T>
    {
        private static T _instance;
        public static T Instance => _instance;

        public static void Init()
        {
            if (_instance == null)
            {
                _instance = (T)Activator.CreateInstance(typeof(T), true);
                _instance.OnInit();
            }
        }
        
        public static void Dispose()
        {
            if (_instance != null)
            {
                _instance.OnDispose();
                _instance = null;
            }
        }
        
        protected abstract void OnInit();
        protected abstract void OnDispose();
    }
}