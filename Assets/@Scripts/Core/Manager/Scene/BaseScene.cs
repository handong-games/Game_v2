using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Game.Core.Managers.Scene
{
    public abstract class BaseScene
    {
        private AsyncOperationHandle<IList<UnityEngine.Object>> _preloadHandle;

        protected BaseScene()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
        }
        
        protected virtual bool RequiresPreloadAssets => true;
        public async Awaitable BeforeUnload()
        {
            await OnBeforeUnload();
        }

        protected abstract Awaitable OnBeforeUnload();
        protected abstract void OnLoaded();
        protected abstract void OnUnloaded();

        private void SceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            _preloadHandle = Addressables.LoadAssetsAsync<UnityEngine.Object>(GetType().Name, null);
            _preloadHandle.Completed += OnLoadCompleted;
        }
        
        private async void OnLoadCompleted(AsyncOperationHandle<IList<UnityEngine.Object>> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                string message = $"Failed to load scene assets: {GetType().Name}";
                if (RequiresPreloadAssets)
                {
                    Debug.LogError(message);
                    return;
                }

                Debug.LogWarning(message);
            }

            await Awaitable.NextFrameAsync();
            OnLoaded();
        }
        
        private void SceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneUnloaded -= SceneUnloaded;

            if (_preloadHandle.IsValid())
            {
                _preloadHandle.Completed -= OnLoadCompleted;
                Addressables.Release(_preloadHandle);
            }

            OnUnloaded();
        }
    }
}
