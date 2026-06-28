using System.Collections.Generic;
using Domains.Player;
using Domains.View.Widgets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private const string TargetHoverClass = "card--target-hover";

        private readonly List<VisualElement> _registeredCards = new();
        private readonly Dictionary<VisualElement, uint> _cardIdsByElement = new();

        private VisualElement _cardBoard;
        private VisualElement _cardDeck;
        private VisualElement _hoveredCard;

        private void ClearCards()
        {
            UnregisterCardEvents();
            _boardUIFlow.ClearAll();
        }

        private void RegisterCardEvents()
        {
            RegisterCardEvents(AdventureBoardSide.Left);
            RegisterCardEvents(AdventureBoardSide.Right);
        }

        private void RegisterCardEvents(AdventureBoardSide side)
        {
            IReadOnlyList<AdventureBoardCardWidgetBinding> bindings =
                _boardUIFlow.GetBindings(side);

            for (int i = 0; i < bindings.Count; i++)
            {
                AdventureBoardCardWidgetBinding binding = bindings[i];
                VisualElement eventTarget = binding.Placement?.Anchor;
                if (eventTarget == null)
                    continue;

                if (!TryGetBoardCardId(binding.ViewModel, out uint cardId))
                    continue;

                eventTarget.RegisterCallback<PointerEnterEvent>(OnCardPointerEnter);
                eventTarget.RegisterCallback<PointerLeaveEvent>(OnCardPointerLeave);
                eventTarget.RegisterCallback<PointerDownEvent>(OnCardPointerDown);
                _registeredCards.Add(eventTarget);
                _cardIdsByElement[eventTarget] = cardId;
            }
        }

        private void UnregisterCardEvents()
        {
            for (int i = 0; i < _registeredCards.Count; i++)
            {
                VisualElement card = _registeredCards[i];
                card.UnregisterCallback<PointerEnterEvent>(OnCardPointerEnter);
                card.UnregisterCallback<PointerLeaveEvent>(OnCardPointerLeave);
                card.UnregisterCallback<PointerDownEvent>(OnCardPointerDown);
                card.RemoveFromClassList(TargetHoverClass);
            }

            _registeredCards.Clear();
            _cardIdsByElement.Clear();
            _hoveredCard = null;
        }

        private void OnCardPointerEnter(PointerEnterEvent evt)
        {
            if (!IsSkillTargetingActive)
                return;

            SetHoveredCard(evt.currentTarget as VisualElement);
        }

        private void OnCardPointerLeave(PointerLeaveEvent evt)
        {
            VisualElement card = evt.currentTarget as VisualElement;
            if (_hoveredCard != card)
                return;

            SetHoveredCard(null);
        }

        private void OnCardPointerDown(PointerDownEvent evt)
        {
            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (!IsSkillTargetingActive)
            {
                SelectCard(evt.currentTarget as VisualElement);
                evt.StopPropagation();
                return;
            }

            SetHoveredCard(evt.currentTarget as VisualElement);
            ConfirmSkillTarget();
            evt.StopPropagation();
        }

        private void SelectCard(VisualElement card)
        {
            if (card == null)
                return;

            if (!_cardIdsByElement.TryGetValue(card, out uint cardId))
                return;

            _controller.OnCardClicked(cardId);
        }

        internal void OnBoardChanged(IReadOnlyList<AdventureCardViewModel> cards)
        {
            UnbindGameplayCueReceivers();
            UnregisterCardEvents();
            _boardUIFlow.PlaceCards(_controller.GetBoardCards());
            RegisterCardEvents();
            BindGameplayCueReceivers();
        }

        private void SetHoveredCard(VisualElement card)
        {
            if (_hoveredCard == card)
                return;

            _hoveredCard?.RemoveFromClassList(TargetHoverClass);
            _hoveredCard = card;
            _hoveredCard?.AddToClassList(TargetHoverClass);
        }

        private static bool TryGetBoardCardId(
            AdventureBoardCardViewModel viewModel,
            out uint cardId)
        {
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
    }
}
