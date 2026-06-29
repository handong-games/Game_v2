using Game.Core.Managers.View;
using Game.Scenes.Adventure.Events.Widgets;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class Pouch : VisualElement
    {
        private const string ImageName = "pouch-image";
        private const string HiddenClass = "pouch__image--hidden";
        private const string EnterClass = "pouch__image--enter";
        private const string ExitClass = "pouch__image--exit";
        private const string FloatUpClass = "pouch--float-up";
        private const string FloatDownClass = "pouch--float-down";

        private VisualElement _image;
        private AdventurePouchWidgetEvents _events;
        private bool? _isFloatingUp;

        public Pouch()
        {
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<TransitionEndEvent>(OnFloatTransitionEnd);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public void Bind(AdventurePouchWidgetEvents events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Unbind()
        {
            _events = null;
        }

        public async Awaitable Show(ViewTransitionManager transitionManager)
        {
            PrepareHidden();
            if (_image == null || transitionManager == null)
                return;

            SetClickable(true);
            StartIdleAnimation();
            await transitionManager.Play(_image, EnterClass);
        }

        public async Awaitable Hide(ViewTransitionManager transitionManager)
        {
            if (_image == null || transitionManager == null)
                return;

            SetClickable(false);
            StopIdleAnimation();
            await transitionManager.Play(_image, ExitClass);
        }

        private void PrepareHidden()
        {
            if (_image == null)
                return;

            StopIdleAnimation();
            SetClickable(false);
            _image.RemoveFromClassList(EnterClass);
            _image.RemoveFromClassList(ExitClass);
            ClearFloatClasses();
            _image.AddToClassList(HiddenClass);
        }

        private void SetClickable(bool enabled)
        {
            pickingMode = enabled ? PickingMode.Position : PickingMode.Ignore;
        }

        private void StopIdleAnimation()
        {
            _isFloatingUp = null;
            ClearFloatClasses();
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _image = this.Q<VisualElement>(ImageName);
            PrepareHidden();
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            StopIdleAnimation();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (pickingMode != PickingMode.Position)
                return;

            evt.StopImmediatePropagation();
            _events?.Clicked?.Invoke();
        }

        private void StartIdleAnimation()
        {
            if (_image == null)
                return;

            ClearFloatClasses();
            _isFloatingUp = true;
            ApplyFloatClass();
        }

        private void OnFloatTransitionEnd(TransitionEndEvent evt)
        {
            if (evt.target != this ||
                !_isFloatingUp.HasValue)
                return;

            _isFloatingUp = !_isFloatingUp.Value;
            ApplyFloatClass();
        }

        private void ApplyFloatClass()
        {
            if (_isFloatingUp == true)
            {
                RemoveFromClassList(FloatDownClass);
                AddToClassList(FloatUpClass);
                return;
            }

            RemoveFromClassList(FloatUpClass);
            AddToClassList(FloatDownClass);
        }

        private void ClearFloatClasses()
        {
            RemoveFromClassList(FloatUpClass);
            RemoveFromClassList(FloatDownClass);
        }
    }
}
