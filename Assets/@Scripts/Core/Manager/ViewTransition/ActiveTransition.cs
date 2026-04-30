using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Core.Managers.View
{
    internal sealed class ActiveTransition
    {
        private readonly VisualElement _visualElement;
        private readonly string[] _transitionClasses;
        private readonly string _transitionClass;
        private readonly bool _enabled;
        private readonly AwaitableCompletionSource _completionSource = new();
        private readonly EventCallback<TransitionEndEvent> _onTransitionEnd;
        private readonly EventCallback<TransitionCancelEvent> _onTransitionCancel;
        private bool _completed;
        private bool _disposed;

        public ActiveTransition(
            VisualElement visualElement,
            string[] transitionClasses,
            string transitionClass,
            bool enabled)
        {
            _visualElement = visualElement;
            _transitionClasses = transitionClasses;
            _transitionClass = transitionClass;
            _enabled = enabled;

            _onTransitionEnd = evt =>
            {
                if (evt.target == _visualElement)
                {
                    Complete();
                }
            };

            _onTransitionCancel = evt =>
            {
                if (evt.target == _visualElement)
                {
                    Complete();
                }
            };

            _visualElement.RegisterCallback(_onTransitionEnd);
            _visualElement.RegisterCallback(_onTransitionCancel);
        }

        public async Awaitable Play()
        {
            await Awaitable.NextFrameAsync();

            if (_transitionClasses != null)
            {
                for (int i = 0; i < _transitionClasses.Length; i++)
                {
                    _visualElement.RemoveFromClassList(_transitionClasses[i]);
                }
            }

            if (_enabled)
            {
                _visualElement.AddToClassList(_transitionClass);
            }
            else
            {
                _visualElement.RemoveFromClassList(_transitionClass);
            }

            await _completionSource.Awaitable;
        }

        public void Cancel()
        {
            Complete();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _visualElement.UnregisterCallback(_onTransitionEnd);
            _visualElement.UnregisterCallback(_onTransitionCancel);
        }

        private void Complete()
        {
            if (_completed)
                return;

            _completed = true;
            _completionSource.SetResult();
        }
    }
}
