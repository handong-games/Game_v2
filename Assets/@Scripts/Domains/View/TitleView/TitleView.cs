using Game.Core.Managers.Dependency;
using Game.Core.Managers.View;
using UnityEngine.UIElements;

namespace Views.TitleView
{
    public class TitleView : BaseView
    {
        [Inject]
        private TitleViewController _controller;

        private Button _newGameButton;
        private Button _settingsButton;
        private Button _quitButton;
        
        protected override void OnBind(VisualElement root)
        {
            /* Set VisualElements */
            if (Root.childCount == 0)
                return;

            Root.Q<Label>("title-text-echo")?.AddToClassList("title-screen__branding-echo--visible");
            Root.Q<Label>("title-text")?.AddToClassList("title-screen__branding-text--visible");
            Root.Q<VisualElement>("title-menu")?.AddToClassList("title-screen__menu--visible");
            Root.Q<Label>("version-text")?.AddToClassList("title-screen__footer-version--visible");

            /* Set Events */
            _newGameButton = Root.Q<Button>("btn-new-game");
            _settingsButton = Root.Q<Button>("btn-settings");
            _quitButton = Root.Q<Button>("btn-quit");

            _newGameButton.clicked += _controller.OnNewGame;
            _settingsButton.clicked += _controller.OnSettings;
            _quitButton.clicked += _controller.OnQuit;
        }

        public override void Dispose()
        {
            _newGameButton.clicked -= _controller.OnNewGame;
            _settingsButton.clicked -= _controller.OnSettings;
            _quitButton.clicked -= _controller.OnQuit;

            _newGameButton = null;
            _settingsButton = null;
            _quitButton = null;
        }
    }
}
