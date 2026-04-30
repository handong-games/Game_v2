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
    }
}
