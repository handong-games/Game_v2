using System.Collections.Generic;
using Domains.Event;
using Domains.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private const string TargetHoverClass = "card--target-hover";

        private readonly List<VisualElement> _registeredCards = new();

        private VisualElement _cardBoard;
        private VisualElement _cardDeck;
        private CardDealer _cardDealer;
        private VisualElement _hoveredCard;

        private void BindCards()
        {
            _cardBoard = Root.Q<VisualElement>("card-board");
            _cardDeck = Root.Q<VisualElement>("card-deck");

            _cardDealer = new CardDealer();
            _cardDealer.Bind(_cardDeck, _cardBoard);
        }

        private void ClearCards()
        {
            UnregisterCardEvents();
            _cardDealer?.Clear();
        }

        private async void OnCardsDrawn(CardBoardViewModel board)
        {
            if (_cardDealer == null)
                return;

            UnregisterCardEvents();

            await _cardDealer.DealAsync(board);
            RegisterCardEvents();

            AdventureEvents.CardDealCompleted?.Invoke();
        }

        private void RegisterCardEvents()
        {
            IReadOnlyList<VisualElement> cards = _cardDealer.Cards;
            for (int i = 0; i < cards.Count; i++)
            {
                VisualElement card = cards[i];
                card.RegisterCallback<PointerEnterEvent>(OnCardPointerEnter);
                card.RegisterCallback<PointerLeaveEvent>(OnCardPointerLeave);
                card.RegisterCallback<PointerDownEvent>(OnCardPointerDown);
                _registeredCards.Add(card);
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

            if (!_cardDealer.TryGetCardId(card, out uint cardId))
                return;

            _controller.OnCardClicked(cardId);
        }

        private void OnBoardChanged(CardBoardViewModel board)
        {
            if (_cardDealer == null)
                return;

            UnregisterCardEvents();
            _cardDealer.Refresh(board);
            RegisterCardEvents();
        }

        private void SetHoveredCard(VisualElement card)
        {
            if (_hoveredCard == card)
                return;

            _hoveredCard?.RemoveFromClassList(TargetHoverClass);
            _hoveredCard = card;
            _hoveredCard?.AddToClassList(TargetHoverClass);
        }
    }
}
