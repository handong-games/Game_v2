using Domains.Adventure;
using Game.Core.Ports;
using VContainer;

namespace Domains.Scene.Adventure
{
    public sealed class AdventureSceneNavigator
    {
        private readonly ISceneViewNavigator _viewNavigator;
        private readonly IObjectResolver _resolver;

        public AdventureSceneNavigator(
            ISceneViewNavigator viewNavigator,
            IObjectResolver resolver)
        {
            _viewNavigator = viewNavigator;
            _resolver = resolver;
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
