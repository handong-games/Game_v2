using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Core.Managers.View
{
    // Role:
    // Converts a UI Toolkit transition completion boundary into an Awaitable.
    public static class ViewTransitionAwaiter
    {
        public static Awaitable WaitForEnd(VisualElement element)
        {
            if (element == null || element.panel == null)
                return Awaitable.NextFrameAsync();

            AwaitableCompletionSource completionSource = new();
            bool completed = false;
            EventCallback<TransitionEndEvent> onTransitionEnd = null;
            EventCallback<TransitionCancelEvent> onTransitionCancel = null;
            EventCallback<DetachFromPanelEvent> onDetached = null;

            void Complete()
            {
                if (completed)
                    return;

                completed = true;
                element.UnregisterCallback(onTransitionEnd);
                element.UnregisterCallback(onTransitionCancel);
                element.UnregisterCallback(onDetached);
                completionSource.SetResult();
            }

            onTransitionEnd = evt =>
            {
                if (evt.target != element)
                    return;

                Complete();
            };

            onTransitionCancel = evt =>
            {
                if (evt.target != element)
                    return;

                Complete();
            };

            onDetached = _ => Complete();

            element.RegisterCallback(onTransitionEnd);
            element.RegisterCallback(onTransitionCancel);
            element.RegisterCallback(onDetached);
            return completionSource.Awaitable;
        }
    }
}
