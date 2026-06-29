using Game.Core.Managers.View;
using Game.Scenes.Adventure.Events.Widgets;
using System;
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
        private AdventureTurnWidgetEvents _events;
        private bool _isShown;

        public EndTurnWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public void Bind(AdventureTurnWidgetEvents events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Unbind()
        {
            _events = null;
        }

        public async Awaitable Show(ViewTransitionManager transitionManager)
        {
            if (_isShown)
                return;

            _isShown = true;
            SetHidden();
            SetClickable(true);

            if (transitionManager != null)
                await transitionManager.Play(this, EnterClass);
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
            _events?.EndTurnClicked?.Invoke();
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
