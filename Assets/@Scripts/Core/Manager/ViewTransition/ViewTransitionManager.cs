using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Core.Managers.View
{
    public sealed class ViewTransitionManager : BaseManager<ViewTransitionManager>
    {
        private readonly string[] _transitionClasses = new string[(int)EViewTransitionType.Count];
        private readonly Dictionary<VisualElement, ActiveTransition> _activeTransitions = new();

        protected override void OnInit()
        {
            _transitionClasses[(int)EViewTransitionType.FadeIn] = "view-transition--fade-in";
            _transitionClasses[(int)EViewTransitionType.FadeOut] = "view-transition--fade-out";
        }

        protected override void OnDispose()
        {
            foreach (ActiveTransition transition in _activeTransitions.Values)
            {
                transition.Dispose();
            }

            _activeTransitions.Clear();
        }

        public async Awaitable Play(VisualElement visualElement, EViewTransitionType transitionType)
        {
            await Play(
                visualElement,
                _transitionClasses[(int)transitionType],
                true,
                _transitionClasses);
        }

        public async Awaitable Play(VisualElement visualElement, string transitionClass)
        {
            await Play(visualElement, transitionClass, true);
        }

        public async Awaitable Play(VisualElement visualElement, string transitionClass, bool enabled)
        {
            await Play(visualElement, transitionClass, enabled, null);
        }

        public async Awaitable Play(ViewTransitionTimeline timeline)
        {
            if (timeline == null ||
                timeline.Steps.Count == 0)
            {
                return;
            }

            ViewTransitionTimelinePlayback playback = new(timeline.Steps.Count);

            for (int i = 0; i < timeline.Steps.Count; i++)
            {
                _ = PlayTimelineStep(timeline, timeline.Steps[i], playback);
            }

            await playback.Awaitable;
        }

        private async Awaitable Play(
            VisualElement visualElement,
            string transitionClass,
            bool enabled,
            string[] transitionClasses)
        {
            if (_activeTransitions.TryGetValue(visualElement, out ActiveTransition activeTransition))
            {
                _activeTransitions.Remove(visualElement);
                activeTransition.Cancel();
                activeTransition.Dispose();
            }

            ActiveTransition newActiveTransition = new ActiveTransition(
                visualElement,
                transitionClasses,
                transitionClass,
                enabled);
            _activeTransitions[visualElement] = newActiveTransition;

            await newActiveTransition.Play();

            if (_activeTransitions.TryGetValue(visualElement, out ActiveTransition current) &&
                current == newActiveTransition)
            {
                _activeTransitions.Remove(visualElement);
            }

            newActiveTransition.Dispose();
        }

        private async Awaitable PlayTimelineStep(
            ViewTransitionTimeline timeline,
            ViewTransitionTimelineStep step,
            ViewTransitionTimelinePlayback playback)
        {
            try
            {
                if (step.StartMs > 0)
                {
                    float delaySeconds = step.StartMs * timeline.TimeScale / 1000f;
                    await Awaitable.WaitForSecondsAsync(delaySeconds);
                }

                switch (step.Type)
                {
                    case EViewTransitionTimelineStepType.Transition:
                        await Play(
                            step.VisualElement,
                            step.TransitionClass,
                            step.Enabled);
                        break;
                    case EViewTransitionTimelineStepType.Action:
                        if (step.Action != null)
                        {
                            await step.Action();
                        }
                        break;
                }
            }
            finally
            {
                playback.CompleteStep();
            }
        }
    }

    internal sealed class ViewTransitionTimelinePlayback
    {
        private readonly AwaitableCompletionSource _completionSource = new();
        private int _remainingStepCount;

        public ViewTransitionTimelinePlayback(int stepCount)
        {
            _remainingStepCount = stepCount;
        }

        public Awaitable Awaitable => _completionSource.Awaitable;

        public void CompleteStep()
        {
            _remainingStepCount--;

            if (_remainingStepCount <= 0)
            {
                _completionSource.SetResult();
            }
        }
    }
}
