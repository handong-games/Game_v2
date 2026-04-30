using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Core.Managers.Scene
{
    public abstract class BaseScene
    {
        private AsyncOperationHandle<IList<UnityEngine.Object>> _preloadHandle;

        protected abstract Awaitable OnBeforeUnload();
        protected abstract void OnLoaded();
        protected abstract void OnUnloaded();

        public void Loaded()
        {
            _preloadHandle = Addressables.LoadAssetsAsync<UnityEngine.Object>(GetType().Name, null);
            _preloadHandle.Completed += OnLoadCompleted;
        }
        
        private async void OnLoadCompleted(AsyncOperationHandle<IList<UnityEngine.Object>> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                string message = $"Failed to load scene assets: {GetType().Name}";
                Debug.LogWarning(message);
            }

            await Awaitable.NextFrameAsync();
            OnLoaded();
        }
        
        public async Awaitable BeforeUnload()
        {
            await OnBeforeUnload();
        }
        
        public void Unloaded()
        {
            if (_preloadHandle.IsValid())
            {
                _preloadHandle.Completed -= OnLoadCompleted;
                Addressables.Release(_preloadHandle);
            }

            OnUnloaded();
        }
    }
}
