using System;
using Domains.Adventure;
using Game.Scenes.Adventure.Events;
using UnityEngine;
using VContainer.Unity;

namespace Game.Scenes.Adventure
{
    // Role:
    // Starts AdventureScene after its scope has been built.
    // It does not load startup models; AdventureSceneLoader and AdventureSceneScope handle that earlier.
    public sealed class AdventureSceneEntryPoint : IStartable, IDisposable
    {
        private readonly AdventureSceneLocalization _localization;
        private readonly AdventureStartFlow _startFlow;
        private readonly AdventureSceneNavigator _navigator;
        private readonly AdventureScreenController _controller;
        private readonly AdventureGameToScreenEventBinder _gameToScreenEventBinder;
        private readonly AdventureWidgetToScreenEventBinder _widgetToScreenEventBinder;
        private bool _disposed;

        public AdventureSceneEntryPoint(
            AdventureSceneLocalization localization,
            AdventureStartFlow startFlow,
            AdventureSceneNavigator navigator,
            AdventureScreenController controller,
            AdventureGameToScreenEventBinder gameToScreenEventBinder,
            AdventureWidgetToScreenEventBinder widgetToScreenEventBinder)
        {
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
            _startFlow = startFlow ?? throw new ArgumentNullException(nameof(startFlow));
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _gameToScreenEventBinder = gameToScreenEventBinder ?? throw new ArgumentNullException(nameof(gameToScreenEventBinder));
            _widgetToScreenEventBinder = widgetToScreenEventBinder ?? throw new ArgumentNullException(nameof(widgetToScreenEventBinder));
        }

        public async void Start()
        {
            try
            {
                if (_disposed)
                    return;

                // Bind screen event routes before gameplay startup emits the first presentation payload.
                _gameToScreenEventBinder.Bind();
                _widgetToScreenEventBinder.Bind();
                _startFlow.InitializeRuntime();
                _controller.InitializeAdventure();
                await _localization.Preload();
                _navigator.ShowAdventure();
                await _controller.StartAdventure();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _navigator.HideCurrent();
            _widgetToScreenEventBinder.Dispose();
            _gameToScreenEventBinder.Dispose();
        }
    }
}
