using VContainer.Unity;

namespace Domains.Scene.Title
{
    public sealed class TitleSceneEntryPoint : IStartable
    {
        private readonly TitleSceneLocalizationOwner _localizationOwner;
        private readonly TitleSceneBgmOwner _bgmOwner;
        private readonly TitleSceneNavigator _navigator;

        public TitleSceneEntryPoint(
            TitleSceneLocalizationOwner localizationOwner,
            TitleSceneBgmOwner bgmOwner,
            TitleSceneNavigator navigator)
        {
            _localizationOwner = localizationOwner;
            _bgmOwner = bgmOwner;
            _navigator = navigator;
        }

        public void Start()
        {
            _localizationOwner.Preload();
            _bgmOwner.LoadAndPlay();
            _navigator.ShowTitle();
        }
    }
}
