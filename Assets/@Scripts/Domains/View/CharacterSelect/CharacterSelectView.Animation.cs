using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.CharacterSelect
{
    public sealed partial class CharacterSelectView
    {
        private async Awaitable PlayIntroAnimation()
        {
            await Awaitable.NextFrameAsync();

            if (_screenRoot == null)
                return;

            _screenRoot.AddToClassList(ShownClass);
        }

        private void PlayCloseAnimation()
        {
            if (_screenRoot == null)
                return;

            _isClosing = true;
            _navigation?.SetEnabled(false);
            _cardList?.SetEnabled(false);
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
