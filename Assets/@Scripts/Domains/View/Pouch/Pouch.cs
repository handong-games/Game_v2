using System;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class Pouch : VisualElement
    {
        public event Action Clicked;

        private const string ImageName = "pouch-image";
        private const string HiddenClass = "pouch__image--hidden";
        private const string EnterClass = "pouch__image--enter";
        private const string ExitClass = "pouch__image--exit";
        private const string FloatUpClass = "pouch--float-up";
        private const string FloatDownClass = "pouch--float-down";

        private VisualElement _image;
        private bool? _isFloatingUp;

        public Pouch()
        {
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<TransitionEndEvent>(OnFloatTransitionEnd);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public async Awaitable Show()
        {
            PrepareHidden();
            if (_image == null)
                return;

            SetClickable(true);
            StartIdleAnimation();
            await ViewTransitionManager.Instance.Play(_image, EnterClass);
        }

        public async Awaitable Hide()
        {
            EnsureInitialized();
            if (_image == null)
                return;

            SetClickable(false);
            StopIdleAnimation();
            await ViewTransitionManager.Instance.Play(_image, ExitClass);
        }

        private void PrepareHidden()
        {
            EnsureInitialized();
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
            PrepareHidden();
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            StopIdleAnimation();
        }

        private async void OnPointerDown(PointerDownEvent evt)
        {
            if (pickingMode != PickingMode.Position)
                return;

            evt.StopImmediatePropagation();
            await Hide();
            Clicked?.Invoke();
        }

        private void EnsureInitialized()
        {
            _image ??= this.Q<VisualElement>(ImageName);
        }

        private void StartIdleAnimation()
        {
            EnsureInitialized();
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
