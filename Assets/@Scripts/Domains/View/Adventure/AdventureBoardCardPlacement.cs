using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Describes where one visual card was placed in the board hierarchy.
    public sealed class AdventureBoardCardPlacement
    {
        public AdventureBoardCardPlacement(
            VisualElement card,
            VisualElement slot,
            VisualElement anchor,
            int index)
        {
            Card = card;
            Slot = slot;
            Anchor = anchor;
            Index = index;
        }

        public VisualElement Card { get; }
        public VisualElement Slot { get; }
        public VisualElement Anchor { get; }
        public int Index { get; }
    }
}
