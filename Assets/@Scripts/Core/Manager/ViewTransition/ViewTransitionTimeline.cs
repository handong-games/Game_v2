using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Core.Managers.View
{
    public enum EViewTransitionTimelineStepType
    {
        Transition,
        Action,
    }

    public sealed class ViewTransitionTimeline
    {
        private readonly List<ViewTransitionTimelineStep> _steps = new();

        public ViewTransitionTimeline(float timeScale = 1f)
        {
            TimeScale = timeScale;
        }

        public float TimeScale { get; }

        public IReadOnlyList<ViewTransitionTimelineStep> Steps => _steps;

        public ViewTransitionTimeline Play(
            int startMs,
            VisualElement visualElement,
            string transitionClass)
        {
            return Play(startMs, visualElement, transitionClass, true);
        }

        public ViewTransitionTimeline Play(
            int startMs,
            VisualElement visualElement,
            string transitionClass,
            bool enabled)
        {
            if (visualElement == null)
            {
                return this;
            }

            _steps.Add(ViewTransitionTimelineStep.CreateTransition(
                startMs,
                visualElement,
                transitionClass,
                enabled));

            return this;
        }

        public ViewTransitionTimeline Run(int startMs, Func<Awaitable> action)
        {
            _steps.Add(ViewTransitionTimelineStep.CreateAction(startMs, action));
            return this;
        }
    }

    public readonly struct ViewTransitionTimelineStep
    {
        private ViewTransitionTimelineStep(
            int startMs,
            EViewTransitionTimelineStepType type,
            VisualElement visualElement,
            string transitionClass,
            bool enabled,
            Func<Awaitable> action)
        {
            StartMs = startMs;
            Type = type;
            VisualElement = visualElement;
            TransitionClass = transitionClass;
            Enabled = enabled;
            Action = action;
        }

        public int StartMs { get; }
        public EViewTransitionTimelineStepType Type { get; }
        public VisualElement VisualElement { get; }
        public string TransitionClass { get; }
        public bool Enabled { get; }
        public Func<Awaitable> Action { get; }

        public static ViewTransitionTimelineStep CreateTransition(
            int startMs,
            VisualElement visualElement,
            string transitionClass,
            bool enabled)
        {
            return new ViewTransitionTimelineStep(
                startMs,
                EViewTransitionTimelineStepType.Transition,
                visualElement,
                transitionClass,
                enabled,
                null);
        }

        public static ViewTransitionTimelineStep CreateAction(
            int startMs,
            Func<Awaitable> action)
        {
            return new ViewTransitionTimelineStep(
                startMs,
                EViewTransitionTimelineStepType.Action,
                null,
                null,
                true,
                action);
        }
    }
}
