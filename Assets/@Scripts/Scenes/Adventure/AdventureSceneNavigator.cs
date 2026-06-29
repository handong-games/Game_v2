using Domains.Adventure;
using Game.Core.Ports;
using System;
using VContainer;

namespace Game.Scenes.Adventure
{
    // Role:
    // Resolves and shows the Adventure screen through the scene view navigator.
    // It does not prepare gameplay data or drive screen events.
    public sealed class AdventureSceneNavigator
    {
        private readonly ISceneViewNavigator _viewNavigator;
        private readonly IObjectResolver _resolver;

        public AdventureSceneNavigator(
            ISceneViewNavigator viewNavigator,
            IObjectResolver resolver)
        {
            _viewNavigator = viewNavigator ?? throw new ArgumentNullException(nameof(viewNavigator));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public void ShowAdventure()
        {
            AdventureView adventureView = _resolver.Resolve<AdventureView>();
            _viewNavigator.Show(adventureView);
        }

        public void HideCurrent()
        {
            _viewNavigator.HideCurrent();
        }
    }
}
