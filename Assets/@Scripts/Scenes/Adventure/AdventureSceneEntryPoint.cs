using System;
using Domains.Adventure;
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

        public AdventureSceneEntryPoint(
            AdventureSceneLocalization localization,
            AdventureStartFlow startFlow,
            AdventureSceneNavigator navigator)
        {
            _localization = localization;
            _startFlow = startFlow;
            _navigator = navigator;
        }

        public void Start()
        {
            _startFlow.StartAdventure();
            _localization.Preload();
            _navigator.ShowAdventure();
        }

        public void Dispose()
        {
            _navigator.HideCurrent();
        }
    }
}
