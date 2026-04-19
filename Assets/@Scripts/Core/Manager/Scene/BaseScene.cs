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
                Debug.LogError($"Failed to load scene assets: {GetType().Name}");
                return;
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
