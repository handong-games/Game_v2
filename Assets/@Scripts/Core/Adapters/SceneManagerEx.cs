using System;
using Game.Core.Ports;
using Game.Core.SceneLoading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core.Adapters
{
    public sealed class SceneManagerEx : IDisposable
    {
        private readonly ISceneTransitionPlayer _transitionPlayer;
        private readonly ScenePreloadService _preloadService;
        private bool _isLoading;

        public SceneManagerEx(
            ISceneTransitionPlayer transitionPlayer,
            ScenePreloadService preloadService)
        {
            _transitionPlayer = transitionPlayer;
            _preloadService = preloadService;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public async void Load(GameSceneId sceneId)
        {
            if (_isLoading)
                return;

            _isLoading = true;

            try
            {
                Awaitable fadeOut = _transitionPlayer.FadeOut();
                Awaitable preload = _preloadService.Preload(sceneId);
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(GameSceneNames.ToSceneName(sceneId));
                loadOperation.allowSceneActivation = false;

                await fadeOut;
                await preload;

                while (loadOperation.progress < 0.9f)
                {
                    await Awaitable.NextFrameAsync();
                }

                loadOperation.allowSceneActivation = true;
            }
            catch
            {
                _isLoading = false;
                throw;
            }
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _isLoading = false;
        }
    }
}
