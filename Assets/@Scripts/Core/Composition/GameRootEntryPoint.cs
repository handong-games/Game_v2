using System;
using Game.Core.Managers.Audio;
using Game.Core.Managers.DB;
using Game.Core.Managers.Garphic;
using Game.Core.Managers.Locale;
using Game.Core.Managers.Save;
using Game.Core.Managers.View;
using Game.Messages;
using VContainer.Unity;

namespace Game.Core.Composition
{
    public sealed class GameRootEntryPoint : IInitializable, IDisposable
    {
        private readonly DBManager _dbManager;
        private readonly SaveManager _saveManager;
        private readonly GraphicManager _graphicManager;
        private readonly AudioManager _audioManager;
        private readonly LocaleManager _localeManager;
        private readonly GameplayMessageManager _gameplayMessageManager;
        private readonly ViewTransitionManager _viewTransitionManager;
        private readonly ViewManager _viewManager;

        public GameRootEntryPoint(
            DBManager dbManager,
            SaveManager saveManager,
            GraphicManager graphicManager,
            AudioManager audioManager,
            LocaleManager localeManager,
            GameplayMessageManager gameplayMessageManager,
            ViewTransitionManager viewTransitionManager,
            ViewManager viewManager)
        {
            _dbManager = dbManager;
            _saveManager = saveManager;
            _graphicManager = graphicManager;
            _audioManager = audioManager;
            _localeManager = localeManager;
            _gameplayMessageManager = gameplayMessageManager;
            _viewTransitionManager = viewTransitionManager;
            _viewManager = viewManager;
        }

        public void Initialize()
        {
            _dbManager.Initialize();
            _saveManager.Initialize();
            _graphicManager.Initialize();
            _audioManager.Initialize();
            _localeManager.Initialize();
            _gameplayMessageManager.Initialize();
            _viewTransitionManager.Initialize();
            _viewManager.Initialize();
        }

        public void Dispose()
        {
            _viewManager.Dispose();
            _viewTransitionManager.Dispose();
            _gameplayMessageManager.Dispose();
            _localeManager.Dispose();
            _audioManager.Dispose();
            _graphicManager.Dispose();
            _saveManager.Dispose();
            _dbManager.Dispose();
        }
    }
}
