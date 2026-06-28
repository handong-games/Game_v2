using System;
using System.Collections.Generic;
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

        public IReadOnlyList<AdventureBoardCardPlacement> ReplaceCards(
            VisualElement area,
            IReadOnlyList<VisualElement> cards)
        {
            if (area == null)
                throw new ArgumentNullException(nameof(area));

            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            if (cards.Count > MaxCardCount)
            {
                throw new InvalidOperationException(
                    $"Board area supports up to {MaxCardCount} cards. Requested: {cards.Count}");
            }

            area.Clear();

            List<AdventureBoardCardPlacement> placements = new(cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                VisualElement slot = CreateSlot(i, cards.Count);
                VisualElement anchor = CreateCardAnchor();
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

        public void Clear(VisualElement area)
        {
            if (area == null)
                throw new ArgumentNullException(nameof(area));

            area.Clear();
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
            VisualElement anchor = new()
            {
                name = "card-board-card-anchor",
                pickingMode = PickingMode.Position,
            };

            anchor.AddToClassList("card-board__card-anchor");
            return anchor;
        }

        private static float GetOffsetX(int index, int totalCount)
        {
            float startOffset = -((totalCount - 1) * CardSpacing) * 0.5f;
            return startOffset + index * CardSpacing;
        }
    }
}
