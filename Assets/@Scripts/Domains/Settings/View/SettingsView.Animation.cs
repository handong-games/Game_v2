using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Settings.View
{
    public sealed partial class SettingsView
    {
        private async Awaitable PlayIntroAnimation()
        {
            await Awaitable.NextFrameAsync();

            if (_screenRoot == null)
                return;

            _screenRoot.AddToClassList(IntroShownClass);
        }

        private void ResetIntroState()
        {
            _screenRoot?.RemoveFromClassList(IntroShownClass);
        }

        private void ResetCloseState()
        {
            _isClosing = false;
            _screenRoot?.RemoveFromClassList(ClosingClass);
            _closeButton?.SetEnabled(true);
        }

        private void PlayCloseAnimation()
        {
            if (_screenRoot == null)
                return;

            _isClosing = true;
            _closeButton?.SetEnabled(false);
            _screenRoot.AddToClassList(ClosingClass);
        }

        private void OnCloseTransitionEnd(TransitionEndEvent evt)
        {
            if (!_isClosing || evt.target != _screenRoot)
                return;

            OnCloseAnimationCompleted();
        }
    }
}
