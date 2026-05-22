using System;
using System.Collections.Generic;
using Domains.Card;
using Domains.View.Widgets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed class CardDealer
    {
        private const int MaxCardCount = 3;
        private const float CardSpacing = 264f;
        private const float StartScale = 0.28f;
        private const float DealFallbackSeconds = 0.8f;
        private const string CardDealEnterClass = "card-deal--enter";

        private readonly List<VisualElement> _cards = new();
        private readonly List<VisualElement> _leftCards = new();
        private readonly List<VisualElement> _rightCards = new();
        private readonly Dictionary<VisualElement, uint> _cardIdsByElement = new();
        private readonly Dictionary<uint, VisualElement> _cardElementsById = new();
        private readonly Dictionary<uint, CombatCardWidget> _cardWidgetsById = new();

        private VisualElement _cardDeck;
        private VisualElement _cardBoard;
        private VisualElement _leftArea;
        private VisualElement _rightArea;

        public IReadOnlyList<VisualElement> Cards => _cards;

        public void Bind(VisualElement cardDeck, VisualElement cardBoard)
        {
            _cardDeck = cardDeck;
            _cardBoard = cardBoard;
            _leftArea = _cardBoard?.Q<VisualElement>("card-board-left-area");
            _rightArea = _cardBoard?.Q<VisualElement>("card-board-right-area");
        }

        private async Awaitable DealCardAsync(
            BoardCardViewModel boardCardViewModel,
            ECardZone zone,
            int totalCount)
        {
            VisualElement area = GetArea(zone);
            List<VisualElement> cards = GetCards(zone);

            if (area == null || _cardDeck == null)
                return;

            int clampedTotalCount = Mathf.Clamp(totalCount, 1, MaxCardCount);
            int index = cards.Count;

            if (index >= clampedTotalCount)
                return;

            VisualElement slot = CreateSlot(index, clampedTotalCount);
            VisualElement cardAnchor = CreateCardAnchor();
            CombatCardWidget card = CreateCard(boardCardViewModel);
            cardAnchor.Add(card);

            PrepareCardBeforeLayout(cardAnchor);

            slot.Add(cardAnchor);
            area.Add(slot);
            _cards.Add(cardAnchor);
            cards.Add(cardAnchor);
            RegisterCard(cardAnchor, card, boardCardViewModel.CardId);

            await Awaitable.NextFrameAsync();

            PrepareCardFromDeck(cardAnchor);

            await PlayDealEnterAsync(cardAnchor);

            cardAnchor.pickingMode = PickingMode.Position;
        }

        public async Awaitable DealAsync(CardBoardViewModel board)
        {
            IReadOnlyList<BoardCardViewModel> leftCards = board.LeftCards;
            IReadOnlyList<BoardCardViewModel> rightCards = board.RightCards;

            if (_leftCards.Count == 0)
            {
                for (int i = 0; i < leftCards.Count; i++)
                {
                    await DealCardAsync(leftCards[i], ECardZone.Left, leftCards.Count);
                }
            }
            else
            {
                RefreshZone(ECardZone.Left, leftCards);
            }

            ClearZone(ECardZone.Right);

            for (int i = 0; i < rightCards.Count; i++)
            {
                await DealCardAsync(rightCards[i], ECardZone.Right, rightCards.Count);
            }
        }

        public async Awaitable ShowHealthWidgetsAsync()
        {
            if (_cardWidgetsById.Count == 0)
                return;

            AwaitableCompletionSource completionSource = new();
            int remainingCount = _cardWidgetsById.Count;

            foreach (CombatCardWidget cardWidget in _cardWidgetsById.Values)
            {
                _ = ShowHealthWidgetAsync(cardWidget, () =>
                {
                    remainingCount--;
                    if (remainingCount == 0)
                    {
                        completionSource.SetResult();
                    }
                });
            }

            await completionSource.Awaitable;
        }

        public bool TryGetCardId(VisualElement card, out uint cardId)
        {
            return _cardIdsByElement.TryGetValue(card, out cardId);
        }

        public void Refresh(CardBoardViewModel board)
        {
            RefreshZone(ECardZone.Left, board.LeftCards);
            RefreshZone(ECardZone.Right, board.RightCards);
        }

        public void Clear()
        {
            UnbindCards();
            _leftArea?.Clear();
            _rightArea?.Clear();
            _cards.Clear();
            _leftCards.Clear();
            _rightCards.Clear();
            _cardIdsByElement.Clear();
            _cardElementsById.Clear();
            _cardWidgetsById.Clear();
        }

        private VisualElement GetArea(ECardZone zone)
        {
            return zone == ECardZone.Left
                ? _leftArea
                : _rightArea;
        }

        private List<VisualElement> GetCards(ECardZone zone)
        {
            return zone == ECardZone.Left
                ? _leftCards
                : _rightCards;
        }

        private static VisualElement CreateSlot(int index, int totalCount)
        {
            VisualElement slot = new()
            {
                name = $"card-board-slot-{index}",
            };

            slot.AddToClassList("card-board__slot");
            slot.style.left = Length.Percent(50);
            slot.style.top = Length.Percent(50);

            float offsetX = GetOffsetX(index, totalCount);
            slot.style.translate = new Translate(
                new Length(offsetX, LengthUnit.Pixel),
                new Length(0f, LengthUnit.Pixel));

            return slot;
        }

        private static VisualElement CreateCardAnchor()
        {
            VisualElement cardAnchor = new()
            {
                name = "card-board-card-anchor",
                pickingMode = PickingMode.Ignore,
            };

            cardAnchor.AddToClassList("card-board__card-anchor");
            return cardAnchor;
        }

        private static CombatCardWidget CreateCard(BoardCardViewModel cardViewModel)
        {
            CombatCardWidget widget = CombatCardWidget.Create();
            widget.Bind(cardViewModel.Card, cardViewModel.Health);
            return widget;
        }

        private static void PrepareCardBeforeLayout(VisualElement card)
        {
            card.style.opacity = 0f;
            card.style.scale = new Scale(new Vector2(StartScale, StartScale));
        }

        private static async Awaitable ShowHealthWidgetAsync(CombatCardWidget cardWidget, System.Action onCompleted)
        {
            await cardWidget.ShowHealthAsync();
            onCompleted?.Invoke();
        }

        private void RegisterCard(VisualElement card, CombatCardWidget cardWidget, uint cardId)
        {
            _cardIdsByElement.Add(card, cardId);
            _cardElementsById.Add(cardId, card);
            _cardWidgetsById.Add(cardId, cardWidget);
        }

        private void UnregisterCard(VisualElement card)
        {
            if (!_cardIdsByElement.Remove(card, out uint cardId))
                return;

            _cardElementsById.Remove(cardId);
            if (_cardWidgetsById.TryGetValue(cardId, out CombatCardWidget cardWidget))
            {
                cardWidget.Unbind();
                _cardWidgetsById.Remove(cardId);
            }
        }

        private void UnbindCards()
        {
            foreach (CombatCardWidget cardWidget in _cardWidgetsById.Values)
            {
                cardWidget.Unbind();
            }
        }

        private void PrepareCardFromDeck(VisualElement card)
        {
            Vector2 deckCenter = _cardDeck.worldBound.center;
            Vector2 cardCenter = card.worldBound.center;
            Vector2 offset = deckCenter - cardCenter;

            card.RemoveFromClassList(CardDealEnterClass);
            card.style.opacity = 0f;
            card.style.scale = new Scale(new Vector2(StartScale, StartScale));
            card.style.translate = new Translate(
                new Length(offset.x, LengthUnit.Pixel),
                new Length(offset.y, LengthUnit.Pixel));
        }

        private async Awaitable PlayDealEnterAsync(VisualElement card)
        {
            AwaitableCompletionSource completionSource = new();
            bool completed = false;

            EventCallback<TransitionEndEvent> onTransitionEnd = evt =>
            {
                if (evt.target != card || completed)
                    return;

                completed = true;
                completionSource.SetResult();
            };

            EventCallback<TransitionCancelEvent> onTransitionCancel = evt =>
            {
                if (evt.target != card || completed)
                    return;

                completed = true;
                completionSource.SetResult();
            };

            card.RegisterCallback(onTransitionEnd);
            card.RegisterCallback(onTransitionCancel);
            _ = CompleteDealAfterFallback(() =>
            {
                if (completed)
                    return;

                completed = true;
                completionSource.SetResult();
            });

            await Awaitable.NextFrameAsync();

            card.AddToClassList(CardDealEnterClass);

            await Awaitable.NextFrameAsync();

            ClearDealStartStyle(card);

            await completionSource.Awaitable;

            card.UnregisterCallback(onTransitionEnd);
            card.UnregisterCallback(onTransitionCancel);
        }

        private static async Awaitable CompleteDealAfterFallback(Action complete)
        {
            await Awaitable.WaitForSecondsAsync(DealFallbackSeconds);
            complete();
        }

        private static void ClearDealStartStyle(VisualElement card)
        {
            card.style.opacity = StyleKeyword.Null;
            card.style.scale = StyleKeyword.Null;
            card.style.translate = StyleKeyword.Null;
        }

        private static float GetOffsetX(int index, int totalCount)
        {
            float startOffset = -((totalCount - 1) * CardSpacing) * 0.5f;
            return startOffset + index * CardSpacing;
        }

        private void RefreshZone(ECardZone zone, IReadOnlyList<BoardCardViewModel> cards)
        {
            ClearZone(zone);

            VisualElement area = GetArea(zone);
            List<VisualElement> cardElements = GetCards(zone);
            if (area == null)
                return;

            for (int i = 0; i < cards.Count; i++)
            {
                BoardCardViewModel boardCard = cards[i];
                VisualElement slot = CreateSlot(i, Mathf.Clamp(cards.Count, 1, MaxCardCount));
                VisualElement cardAnchor = CreateCardAnchor();
                CombatCardWidget cardWidget = CreateCard(boardCard);

                cardAnchor.Add(cardWidget);
                slot.Add(cardAnchor);
                area.Add(slot);
                _cards.Add(cardAnchor);
                cardElements.Add(cardAnchor);
                RegisterCard(cardAnchor, cardWidget, boardCard.CardId);
                cardAnchor.pickingMode = PickingMode.Position;
            }
        }

        private void ClearZone(ECardZone zone)
        {
            List<VisualElement> cards = GetCards(zone);
            for (int i = 0; i < cards.Count; i++)
            {
                VisualElement card = cards[i];
                _cards.Remove(card);
                UnregisterCard(card);
                card.RemoveFromHierarchy();
            }

            cards.Clear();
            GetArea(zone)?.Clear();
        }
    }
}
