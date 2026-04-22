using Game.Core.Managers.Dependency;
using Game.Core.Managers.View;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Views.TitleView
{
    public partial class TitleView : BaseView
    {
        [Inject]
        private TitleViewController _controller;

        private Label _titleEchoText;
        private Label _titleText;
        private VisualElement _titleLogicalRoot;
        private VisualElement _titleMenu;
        private Label _versionText;
        private Button _newGameButton;
        private Button _settingsButton;
        private Button _quitButton;
        private readonly List<Button> _menuButtons = new();

        protected override void OnBind(VisualElement root)
        {
            if (Root.childCount == 0)
                return;

            BindRequiredElements();

            _newGameButton.clicked += _controller.OnNewGame;
            _settingsButton.clicked += _controller.OnSettings;
            _quitButton.clicked += _controller.OnQuit;
        }

        protected override void OnAttachedToPanel(AttachToPanelEvent evt)
        {
        }

        protected override void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            CancelIntroIfNeeded();
        }

        public override void Dispose()
        {
            CancelIntroIfNeeded();
            _newGameButton.clicked -= _controller.OnNewGame;
            _settingsButton.clicked -= _controller.OnSettings;
            _quitButton.clicked -= _controller.OnQuit;
            UnregisterMenuFocusHandlers(_newGameButton);
            UnregisterMenuFocusHandlers(_settingsButton);
            UnregisterMenuFocusHandlers(_quitButton);

            _titleEchoText = null;
            _titleText = null;
            _titleLogicalRoot = null;
            _titleMenu = null;
            _versionText = null;
            _newGameButton = null;
            _settingsButton = null;
            _quitButton = null;
            _menuButtons.Clear();
            base.Dispose();
        }

        private void BindRequiredElements()
        {
            _titleEchoText = RequireElement<Label>("title-text-echo");
            _titleText = RequireElement<Label>("title-text");
            _titleLogicalRoot = RequireElement<VisualElement>("title-logical-root");
            _titleMenu = RequireElement<VisualElement>("title-menu");
            _versionText = RequireElement<Label>("version-text");

            _newGameButton = RequireElement<Button>("btn-new-game");
            _settingsButton = RequireElement<Button>("btn-settings");
            _quitButton = RequireElement<Button>("btn-quit");

            _menuButtons.Clear();
            _menuButtons.Add(_newGameButton);
            _menuButtons.Add(_settingsButton);
            _menuButtons.Add(_quitButton);

            RegisterMenuFocusHandlers(_newGameButton);
            RegisterMenuFocusHandlers(_settingsButton);
            RegisterMenuFocusHandlers(_quitButton);
            SetMenuButtonFocused(_newGameButton);
        }

        private T RequireElement<T>(string name) where T : VisualElement
        {
            T element = Root.Q<T>(name);
            if (element == null)
                throw new InvalidOperationException($"Required element not found: {name}");

            return element;
        }

        public override void ApplyResponsiveLayout(float frameScale, float frameWidth, float frameHeight, Vector2Int targetSize)
        {
            if (_titleLogicalRoot == null || frameWidth <= 0f || frameHeight <= 0f)
                return;

            const float logicalBaseHeight = 1080f;
            const float logicalMinWidth = 1680f;
            const float logicalMaxWidth = 2580f;
            const float logicalMinAspect = logicalMinWidth / logicalBaseHeight;

            float aspectRatio = frameWidth / frameHeight;
            float logicalWidth;
            float logicalHeight;

            if (aspectRatio < logicalMinAspect)
            {
                logicalWidth = logicalMinWidth;
                logicalHeight = logicalWidth / aspectRatio;
            }
            else
            {
                logicalWidth = Mathf.Clamp(aspectRatio * logicalBaseHeight, logicalMinWidth, logicalMaxWidth);
                logicalHeight = logicalBaseHeight;
            }

            float uniformScale = frameWidth / logicalWidth;

            _titleLogicalRoot.style.left = 0f;
            _titleLogicalRoot.style.top = 0f;
            _titleLogicalRoot.style.width = logicalWidth;
            _titleLogicalRoot.style.height = logicalHeight;
            _titleLogicalRoot.style.scale = new Scale(new Vector2(uniformScale, uniformScale));
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
