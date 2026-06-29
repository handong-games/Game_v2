using System;
using System.Collections.Generic;
using Domains.Adventure;
using Domains.View.Widgets;
using Game.Scenes.Adventure.Events.Widgets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure.Events
{
    // Role:
    // Binds pointer callbacks for currently placed dynamic board cards.
    // It owns callback registration details, not gameplay meaning.
    public sealed class AdventureCardWidgetEventBinder : IDisposable
    {
        private readonly AdventureCardWidgetEvents _events;
        private readonly List<CardRegistration> _registrations = new();
        private readonly Dictionary<VisualElement, uint> _cardIdsByElement = new();

        public AdventureCardWidgetEventBinder(AdventureCardWidgetEvents events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Bind(IReadOnlyList<AdventureBoardCardWidgetBinding> bindings)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));

            for (int i = 0; i < bindings.Count; i++)
            {
                Bind(bindings[i]);
            }
        }

        public void Unbind()
        {
            for (int i = _registrations.Count - 1; i >= 0; i--)
            {
                _registrations[i].Unbind();
            }

            _registrations.Clear();
            _cardIdsByElement.Clear();
        }

        public bool TryGetCardId(VisualElement element, out uint cardId)
        {
            return _cardIdsByElement.TryGetValue(element, out cardId);
        }

        private void Bind(AdventureBoardCardWidgetBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            if (!binding.IsInteractive)
                return;

            VisualElement eventTarget = binding.InteractionTarget;
            if (eventTarget == null)
                throw new InvalidOperationException(
                    $"Interactive board card is missing interaction target: {binding.ViewModel?.GetType().Name}");

            if (!TryGetClickCardId(binding.ViewModel, out uint cardId))
                throw new InvalidOperationException(
                    $"Interactive board card has no clickable id: {binding.ViewModel?.GetType().Name}");

            if (_cardIdsByElement.ContainsKey(eventTarget))
                throw new InvalidOperationException(
                    $"Board card event target is already bound: {binding.ViewModel?.GetType().Name}, {cardId}");

            EnsureWidgetEventsBound();

            CardRegistration registration = new(eventTarget, cardId, _events);
            registration.Bind();

            _registrations.Add(registration);
            _cardIdsByElement[eventTarget] = cardId;
        }

        private static bool TryGetClickCardId(
            AdventureBoardCardViewModel viewModel,
            out uint cardId)
        {
            // Skill targeting and choice selection must still be validated by gameplay side checks.
            switch (viewModel)
            {
                case AdventureChoiceCardViewModel choice:
                    cardId = choice.OfferCardId;
                    return true;

                case AdventureBoardCardViewModel boardCard:
                    cardId = boardCard.CardId;
                    return true;

                default:
                    cardId = default;
                    return false;
            }
        }

        private void EnsureWidgetEventsBound()
        {
            if (_events.PointerEntered == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureCardWidgetEvents.PointerEntered)} is not bound.");

            if (_events.PointerLeft == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureCardWidgetEvents.PointerLeft)} is not bound.");

            if (_events.Clicked == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureCardWidgetEvents.Clicked)} is not bound.");
        }

        public void Dispose()
        {
            Unbind();
        }

        private sealed class CardRegistration
        {
            private readonly VisualElement _target;
            private readonly uint _cardId;
            private readonly AdventureCardWidgetEvents _events;
            private readonly EventCallback<PointerEnterEvent> _pointerEntered;
            private readonly EventCallback<PointerLeaveEvent> _pointerLeft;
            private readonly EventCallback<PointerDownEvent> _pointerDown;

            public CardRegistration(
                VisualElement target,
                uint cardId,
                AdventureCardWidgetEvents events)
            {
                _target = target;
                _cardId = cardId;
                _events = events;
                _pointerEntered = OnPointerEntered;
                _pointerLeft = OnPointerLeft;
                _pointerDown = OnPointerDown;
            }

            public void Bind()
            {
                _target.RegisterCallback(_pointerEntered);
                _target.RegisterCallback(_pointerLeft);
                _target.RegisterCallback(_pointerDown);
            }

            public void Unbind()
            {
                _target.UnregisterCallback(_pointerEntered);
                _target.UnregisterCallback(_pointerLeft);
                _target.UnregisterCallback(_pointerDown);
            }

            private void OnPointerEntered(PointerEnterEvent evt)
            {
                _events.PointerEntered.Invoke(_target, _cardId);
            }

            private void OnPointerLeft(PointerLeaveEvent evt)
            {
                _events.PointerLeft.Invoke(_target, _cardId);
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                if (evt.button != (int)MouseButton.LeftMouse)
                    return;

                _events.Clicked.Invoke(_target, _cardId);
                evt.StopPropagation();
            }
        }
    }
}
