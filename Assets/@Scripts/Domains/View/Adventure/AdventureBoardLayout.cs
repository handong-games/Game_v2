using System;
using System.Collections.Generic;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Applies the board's slot/anchor layout rules inside a concrete VisualElement area.
    public sealed class AdventureBoardLayout
    {
        private const int MaxCardCount = 3;
        private const float CardSpacing = 264f;
        private const string CardEnterPendingClass = "card-board__card-anchor--enter-pending";
        private const string CardEnterClass = "card-board__card-anchor--enter";
        private const string CardExitClass = "card-board__card-anchor--exit";
        private const string CardDeathClass = "card-board__card-anchor--death";

        private readonly AdventureCardDealAnimator _dealAnimator;

        public AdventureBoardLayout(AdventureCardDealAnimator dealAnimator)
        {
            _dealAnimator = dealAnimator ?? throw new ArgumentNullException(nameof(dealAnimator));
        }

        public IReadOnlyList<AdventureBoardCardPlacement> ReplaceCards(
            VisualElement area,
            IReadOnlyList<VisualElement> cards,
            bool prepareEnter = false)
        {
            if (area == null)
                throw new ArgumentNullException(nameof(area));

            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            ValidateCardCount(cards.Count);

            area.Clear();

            List<AdventureBoardCardPlacement> placements = new(cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                VisualElement slot = CreateSlot(i, cards.Count);
                VisualElement anchor = CreateCardAnchor(prepareEnter);
                VisualElement card = cards[i];

                anchor.Add(card);
                slot.Add(anchor);
                area.Add(slot);

                placements.Add(new AdventureBoardCardPlacement(
                    card,
                    slot,
                    anchor,
                    i));
            }

            return placements;
        }

        public void ValidateCardCount(int cardCount)
        {
            if (cardCount > MaxCardCount)
            {
                throw new InvalidOperationException(
                    $"Board area supports up to {MaxCardCount} cards. Requested: {cardCount}");
            }
        }

        public void Clear(VisualElement area)
        {
            if (area == null)
                throw new ArgumentNullException(nameof(area));

            area.Clear();
        }

        public void RelayoutSlots(
            IReadOnlyList<AdventureBoardCardPlacement> placements)
        {
            if (placements == null)
                throw new ArgumentNullException(nameof(placements));

            ValidateCardCount(placements.Count);

            for (int i = 0; i < placements.Count; i++)
            {
                AdventureBoardCardPlacement placement = placements[i];
                VisualElement slot = placement?.Slot;
                if (slot == null)
                    throw new InvalidOperationException($"Board card placement is missing slot: {i}");

                ApplySlotLayout(slot, i, placements.Count);
                placement.SetIndex(i);
            }
        }

        public async Awaitable PlayEnter(
            IReadOnlyList<AdventureBoardCardPlacement> placements)
        {
            if (placements == null || placements.Count == 0)
            {
                await Awaitable.NextFrameAsync();
                return;
            }

            List<Awaitable> entries = new(placements.Count);
            for (int i = 0; i < placements.Count; i++)
            {
                entries.Add(PlayEnter(placements[i]));
            }

            for (int i = 0; i < entries.Count; i++)
            {
                await entries[i];
            }
        }

        public async Awaitable PlayEnterFromDeck(
            IReadOnlyList<AdventureBoardCardPlacement> placements,
            VisualElement cardDeck)
        {
            await _dealAnimator.Play(placements, cardDeck);
        }

        public Awaitable PlayExit(AdventureBoardCardPlacement placement)
        {
            if (placement == null || placement.Anchor == null)
                return Awaitable.NextFrameAsync();

            VisualElement anchor = placement.Anchor;
            if (anchor.ClassListContains(CardExitClass))
                return Awaitable.NextFrameAsync();

            if (anchor.ClassListContains(CardEnterPendingClass))
            {
                anchor.RemoveFromClassList(CardEnterPendingClass);
                return Awaitable.NextFrameAsync();
            }

            anchor.RemoveFromClassList(CardEnterClass);
            Awaitable transition = ViewTransitionAwaiter.WaitForEnd(anchor);
            anchor.AddToClassList(CardExitClass);
            return transition;
        }

        public Awaitable PlayDeath(AdventureBoardCardPlacement placement)
        {
            if (placement == null || placement.Anchor == null)
                return Awaitable.NextFrameAsync();

            VisualElement anchor = placement.Anchor;
            if (anchor.ClassListContains(CardEnterPendingClass))
                anchor.RemoveFromClassList(CardEnterPendingClass);

            anchor.RemoveFromClassList(CardEnterClass);
            anchor.RemoveFromClassList(CardExitClass);
            anchor.RemoveFromClassList(CardDeathClass);

            return PlayDeathAfterFrame(anchor);
        }

        private async Awaitable PlayEnter(AdventureBoardCardPlacement placement)
        {
            if (placement == null || placement.Anchor == null)
            {
                await Awaitable.NextFrameAsync();
                return;
            }

            VisualElement anchor = placement.Anchor;
            if (!anchor.ClassListContains(CardEnterPendingClass))
            {
                await Awaitable.NextFrameAsync();
                return;
            }

            Awaitable transition = ViewTransitionAwaiter.WaitForEnd(anchor);
            await Awaitable.NextFrameAsync();
            anchor.RemoveFromClassList(CardEnterPendingClass);
            anchor.AddToClassList(CardEnterClass);
            await transition;
            anchor.RemoveFromClassList(CardEnterClass);
        }

        private static async Awaitable PlayDeathAfterFrame(VisualElement anchor)
        {
            await Awaitable.NextFrameAsync();
            if (anchor.panel == null)
                return;

            anchor.style.opacity = StyleKeyword.Null;
            anchor.style.scale = StyleKeyword.Null;
            anchor.style.translate = StyleKeyword.Null;

            await Awaitable.NextFrameAsync();
            if (anchor.panel == null)
                return;

            Awaitable transition = ViewTransitionAwaiter.WaitForEnd(anchor);
            anchor.AddToClassList(CardDeathClass);
            await transition;
        }

        private static VisualElement CreateSlot(int index, int totalCount)
        {
            VisualElement slot = new()
            {
                name = $"card-board-slot-{index}",
            };

            slot.AddToClassList("card-board__slot");
            ApplySlotLayout(slot, index, totalCount);
            return slot;
        }

        private static void ApplySlotLayout(
            VisualElement slot,
            int index,
            int totalCount)
        {
            slot.name = $"card-board-slot-{index}";
            slot.style.left = Length.Percent(50);
            slot.style.top = Length.Percent(50);

            float offsetX = GetOffsetX(index, totalCount);
            slot.style.translate = new Translate(
                new Length(offsetX, LengthUnit.Pixel),
                new Length(0f, LengthUnit.Pixel));
        }

        private static VisualElement CreateCardAnchor(bool prepareEnter)
        {
            VisualElement anchor = new()
            {
                name = "card-board-card-anchor",
                pickingMode = PickingMode.Ignore,
            };

            anchor.AddToClassList("card-board__card-anchor");
            if (prepareEnter)
                anchor.AddToClassList(CardEnterPendingClass);

            return anchor;
        }

        private static float GetOffsetX(int index, int totalCount)
        {
            float startOffset = -((totalCount - 1) * CardSpacing) * 0.5f;
            return startOffset + index * CardSpacing;
        }
    }
}
