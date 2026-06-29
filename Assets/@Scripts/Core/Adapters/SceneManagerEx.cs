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
            _transitionPlayer = transitionPlayer ?? throw new ArgumentNullException(nameof(transitionPlayer));
            _preloadService = preloadService ?? throw new ArgumentNullException(nameof(preloadService));
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public async void Load(GameSceneId sceneId)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            AsyncOperation loadOperation = null;

            try
            {
                Awaitable fadeOut = _transitionPlayer.FadeOut();
                Awaitable preload = _preloadService.Preload(sceneId);
                loadOperation = SceneManager.LoadSceneAsync(GameSceneNames.ToSceneName(sceneId));
                if (loadOperation == null)
                    throw new InvalidOperationException($"Failed to start scene load: {sceneId}");

                loadOperation.allowSceneActivation = false;

                await fadeOut;
                await preload;

                while (loadOperation.progress < 0.9f)
                {
                    await Awaitable.NextFrameAsync();
                }

                loadOperation.allowSceneActivation = true;
            }
            catch (Exception exception)
            {
                _isLoading = false;
                ReleasePendingSceneActivation(loadOperation);
                Debug.LogException(exception);
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

        private static void ReleasePendingSceneActivation(AsyncOperation loadOperation)
        {
            if (loadOperation == null || loadOperation.isDone)
                return;

            // Unity scene load operations cannot be cancelled. If a parallel preload fails while
            // activation is blocked, release the operation so Unity's async queue is not stalled.
            loadOperation.allowSceneActivation = true;
        }
    }
}
