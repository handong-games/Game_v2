using System;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class EndTurnWidget : VisualElement
    {
        private const string ButtonName = "end-turn-button";
        private const string HiddenClass = "ui-transition--hidden";
        private const string FromBottomClass = "ui-transition--from-bottom";
        private const string EnterClass = "ui-transition--enter";

        private Button _button;
        private bool _isShown;

        public event Action Clicked;

        public EndTurnWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public async Awaitable Show()
        {
            if (_isShown)
                return;

            _isShown = true;
            SetHidden();
            SetClickable(true);

            await ViewTransitionManager.Instance.Play(this, EnterClass);
        }

        public void Hide()
        {
            _isShown = false;
            SetClickable(false);
            SetHidden();
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _button = this.Q<Button>(ButtonName);
            if (_button != null)
                _button.clicked += OnClicked;

            SetClickable(_isShown);
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            if (_button != null)
                _button.clicked -= OnClicked;
        }

        private void OnClicked()
        {
            Clicked?.Invoke();
        }

        private void SetClickable(bool clickable)
        {
            pickingMode = clickable ? PickingMode.Position : PickingMode.Ignore;

            if (_button != null)
                _button.SetEnabled(clickable);
        }

        private void SetHidden()
        {
            RemoveFromClassList(EnterClass);
            AddToClassList(HiddenClass);
            AddToClassList(FromBottomClass);
        }
    }
}
