using System;
using UnityEngine;
using VContainer.Unity;

namespace Domains.Scene.Title
{
    public sealed class TitleSceneEntryPoint : IStartable, IDisposable
    {
        private readonly TitleSceneLocalizationOwner _localizationOwner;
        private readonly TitleSceneBgmOwner _bgmOwner;
        private readonly TitleSceneNavigator _navigator;
        private bool _disposed;

        public TitleSceneEntryPoint(
            TitleSceneLocalizationOwner localizationOwner,
            TitleSceneBgmOwner bgmOwner,
            TitleSceneNavigator navigator)
        {
            _localizationOwner = localizationOwner;
            _bgmOwner = bgmOwner;
            _navigator = navigator;
        }

        public async void Start()
        {
            try
            {
                await _localizationOwner.Preload();
                if (_disposed)
                    return;

                await _bgmOwner.LoadAndPlay();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            if (_disposed)
                return;

            _navigator.ShowTitle();
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
