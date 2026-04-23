using Game.Core.Managers.Dependency;
using Game.Core.Managers.View;
using System;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Views.TitleView
{
    public partial class TitleView : BaseView
    {
        [Inject]
        private TitleViewController _controller;

        private VisualElement _titleLogo;
        private VisualElement _titleMenu;
        private VisualElement _titleVersion;
        private Button _newGameButton;
        private Button _settingsButton;
        private Button _quitButton;
        private readonly List<Button> _menuButtons = new();

        protected override void OnBind(VisualElement root)
        {
            if (Root.childCount == 0)
                return;

            _titleLogo = Root.Q<VisualElement>("title-logo");
            _titleMenu = Root.Q<VisualElement>("title-menu");
            _titleVersion = Root.Q<VisualElement>("title-version");
            
            _newGameButton = Root.Q<Button>("btn-new-game");
            _settingsButton =Root.Q<Button>("btn-settings");
            _quitButton = Root.Q<Button>("btn-quit");

            _menuButtons.Clear();
            _menuButtons.Add(_newGameButton);
            _menuButtons.Add(_settingsButton);
            _menuButtons.Add(_quitButton);
            
            RegisterMenuFocusHandlers(_newGameButton);
            RegisterMenuFocusHandlers(_settingsButton);
            RegisterMenuFocusHandlers(_quitButton);

            _newGameButton.clicked += _controller.OnNewGame;
            _settingsButton.clicked += _controller.OnSettings;
            _quitButton.clicked += _controller.OnQuit;
        }

        protected override void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _ = PlayIntroAnimation();
        }

        protected override void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {

        }

        public override void Dispose()
        {
            _newGameButton.clicked -= _controller.OnNewGame;
            _settingsButton.clicked -= _controller.OnSettings;
            _quitButton.clicked -= _controller.OnQuit;
            
            UnregisterMenuFocusHandlers(_newGameButton);
            UnregisterMenuFocusHandlers(_settingsButton);
            UnregisterMenuFocusHandlers(_quitButton);

            _titleLogo = null;
            _titleMenu = null;
            _newGameButton = null;
            _settingsButton = null;
            _quitButton = null;
            _menuButtons.Clear();
            
            base.Dispose();
        }

        private void RegisterMenuFocusHandlers(Button button)
        {
            button?.RegisterCallback<FocusInEvent>(OnMenuButtonFocusIn);
            button?.RegisterCallback<FocusOutEvent>(OnMenuButtonFocusOut);
        }

        private void UnregisterMenuFocusHandlers(Button button)
        {
            button?.UnregisterCallback<FocusInEvent>(OnMenuButtonFocusIn);
            button?.UnregisterCallback<FocusOutEvent>(OnMenuButtonFocusOut);
        }

        private void OnMenuButtonFocusIn(FocusInEvent evt)
        {
            if (evt.currentTarget is Button button)
            {
                SetMenuButtonFocused(button);
            }
        }

        private void OnMenuButtonFocusOut(FocusOutEvent evt)
        {
            if (evt.currentTarget is Button button)
            {
                button.RemoveFromClassList("title-screen__menu-button--focused");
            }
        }

        private void SetMenuButtonFocused(Button focusedButton)
        {
            foreach (Button button in _menuButtons)
            {
                if (button == null)
                    continue;

                button.EnableInClassList("title-screen__menu-button--focused", button == focusedButton);
            }
        }
    }
}
