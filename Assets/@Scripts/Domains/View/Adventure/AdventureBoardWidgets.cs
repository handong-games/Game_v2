using System;
using Domains.Adventure;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Holds the Adventure board VisualElement references used by board UI flows.
    public sealed class AdventureBoardWidgets
    {
        private const string LeftAreaName = "card-board-left-area";
        private const string RightAreaName = "card-board-right-area";

        public VisualElement LeftArea { get; private set; }
        public VisualElement RightArea { get; private set; }

        public void Initialize(VisualElement cardBoard)
        {
            if (cardBoard == null)
                throw new ArgumentNullException(nameof(cardBoard));

            LeftArea = cardBoard.Q<VisualElement>(LeftAreaName);
            if (LeftArea == null)
                throw new InvalidOperationException($"Required element missing: {LeftAreaName}");

            RightArea = cardBoard.Q<VisualElement>(RightAreaName);
            if (RightArea == null)
                throw new InvalidOperationException($"Required element missing: {RightAreaName}");
        }

        public VisualElement GetArea(AdventureBoardSide side)
        {
            return side switch
            {
                AdventureBoardSide.Left => LeftArea,
                AdventureBoardSide.Right => RightArea,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null),
            };
        }
    }
}
