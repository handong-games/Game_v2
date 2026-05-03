using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public enum ECardBoardSide
    {
        Player,
        Encounter,
    }

    public sealed class CardDealer
    {
        private const int MaxCardCount = 3;
        private const float CardSpacing = 224f;
        private const float StartScale = 0.28f;
        private const string CardDealEnterClass = "card-deal--enter";

        private readonly List<VisualElement> _playerCards = new();
        private readonly List<VisualElement> _encounterCards = new();

        private VisualElement _cardDeck;
        private VisualElement _cardBoard;
        private VisualElement _playerArea;
        private VisualElement _encounterArea;

        public void Bind(VisualElement cardDeck, VisualElement cardBoard)
        {
            _cardDeck = cardDeck;
            _cardBoard = cardBoard;
            _playerArea = _cardBoard?.Q<VisualElement>("card-board-player-area");
            _encounterArea = _cardBoard?.Q<VisualElement>("card-board-encounter-area");
        }

        public async Awaitable DealPlaceholderAsync(ECardBoardSide side, int totalCount)
        {
            VisualElement area = GetArea(side);
            List<VisualElement> cards = GetCards(side);

            if (area == null || _cardDeck == null)
                return;

            int clampedTotalCount = Mathf.Clamp(totalCount, 1, MaxCardCount);
            int index = cards.Count;

            if (index >= clampedTotalCount)
                return;

            VisualElement slot = CreateSlot(index, clampedTotalCount);
            VisualElement card = CreatePlaceholderCard(side, index);

            slot.Add(card);
            area.Add(slot);
            cards.Add(card);

            await Awaitable.NextFrameAsync();

            PrepareCardFromDeck(card);

            await PlayDealEnterAsync(card);

            card.pickingMode = PickingMode.Position;
        }

        public void Clear()
        {
            _playerArea?.Clear();
            _encounterArea?.Clear();
            _playerCards.Clear();
            _encounterCards.Clear();
        }

        private VisualElement GetArea(ECardBoardSide side)
        {
            return side == ECardBoardSide.Player
                ? _playerArea
                : _encounterArea;
        }

        private List<VisualElement> GetCards(ECardBoardSide side)
        {
            return side == ECardBoardSide.Player
                ? _playerCards
                : _encounterCards;
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

        private static VisualElement CreatePlaceholderCard(ECardBoardSide side, int index)
        {
            string sideName = side == ECardBoardSide.Player
                ? "player"
                : "encounter";

            VisualElement card = new()
            {
                name = $"card-board-{sideName}-placeholder-card-{index}",
                pickingMode = PickingMode.Ignore,
            };

            card.AddToClassList("card-board__placeholder-card");

            return card;
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

            await Awaitable.NextFrameAsync();

            card.AddToClassList(CardDealEnterClass);

            await Awaitable.NextFrameAsync();

            ClearDealStartStyle(card);

            await completionSource.Awaitable;

            card.UnregisterCallback(onTransitionEnd);
            card.UnregisterCallback(onTransitionCancel);
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
    }
}
