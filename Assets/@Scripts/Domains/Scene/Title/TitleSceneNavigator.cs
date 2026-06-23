using Domains.CharacterSelect;
using Domains.Settings.View;
using Game.Core.Managers.View;
using Game.Core.Ports;
using VContainer;
using Views.TitleView;

namespace Domains.Scene.Title
{
    public sealed class TitleSceneNavigator
    {
        private readonly ISceneViewNavigator _viewNavigator;
        private readonly IObjectResolver _resolver;

        public TitleSceneNavigator(
            ISceneViewNavigator viewNavigator,
            IObjectResolver resolver)
        {
            _viewNavigator = viewNavigator;
            _resolver = resolver;
        }
        
        public void ShowTitle()
        {
            TitleView titleView = _resolver.Resolve<TitleView>();
            _viewNavigator.Show(titleView);
        }

        public void ShowCharacterSelect()
        {
            CharacterSelectView characterSelectView = _resolver.Resolve<CharacterSelectView>();
            _viewNavigator.Show(characterSelectView);
        }

        public void ShowSettings()
        {
            SettingsView settingsView = _resolver.Resolve<SettingsView>();
            _viewNavigator.Show(settingsView);
        }

        public void HideCurrent()
        {
            _viewNavigator.HideCurrent();
        }
    }
}
