using System.Collections.Generic;
using System;
using Domains.Adventure;
using Domains.View.Widgets;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Coordinates Adventure board UI creation. It owns UI-local choice card ids.
    public sealed class AdventureBoardUIFlow
    {
        private readonly AdventureBoardWidgets _widgets;
        private readonly AdventureBoardLayout _layout;
        private readonly AdventureCardWidgetFactory _cardWidgetFactory;
        private readonly Dictionary<AdventureBoardSide, List<AdventureBoardCardWidgetBinding>> _bindingsBySide = new()
        {
            { AdventureBoardSide.Left, new List<AdventureBoardCardWidgetBinding>() },
            { AdventureBoardSide.Right, new List<AdventureBoardCardWidgetBinding>() },
        };

        public AdventureBoardUIFlow(
            AdventureBoardWidgets widgets,
            AdventureBoardLayout layout,
            AdventureCardWidgetFactory cardWidgetFactory)
        {
            _widgets = widgets;
            _layout = layout;
            _cardWidgetFactory = cardWidgetFactory;
        }

        public AdventureBoardPlacementResult PlaceCard(
            AdventureBoardCardViewModel card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            return PlaceCards(new[] { card });
        }

        public AdventureBoardPlacementResult PlaceCards(
            IReadOnlyList<AdventureBoardCardViewModel> cards)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            List<AdventureBoardCardViewModel> leftViewModels = new();
            List<AdventureBoardCardViewModel> rightViewModels = new();

            foreach (AdventureBoardCardViewModel card in cards)
            {
                switch (card.Side)
                {
                    case AdventureBoardSide.Left:
                        leftViewModels.Add(card);
                        break;

                    case AdventureBoardSide.Right:
                        rightViewModels.Add(card);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(card.Side), card.Side, null);
                }
            }

            IReadOnlyList<AdventureBoardCardPlacement> changedLeftPlacements =
                Array.Empty<AdventureBoardCardPlacement>();
            IReadOnlyList<AdventureBoardCardPlacement> changedRightPlacements =
                Array.Empty<AdventureBoardCardPlacement>();

            if (leftViewModels.Count > 0)
            {
                changedLeftPlacements = ReplaceSide(
                    AdventureBoardSide.Left,
                    leftViewModels);
            }

            if (rightViewModels.Count > 0)
            {
                changedRightPlacements = ReplaceSide(
                    AdventureBoardSide.Right,
                    rightViewModels);
            }

            return new AdventureBoardPlacementResult(
                changedLeftPlacements,
                changedRightPlacements);
        }

        public void ClearSide(AdventureBoardSide side)
        {
            DisposeSide(side);
            _layout.Clear(_widgets.GetArea(side));
        }

        public void ClearAll()
        {
            ClearSide(AdventureBoardSide.Left);
            ClearSide(AdventureBoardSide.Right);
        }

        public IReadOnlyList<AdventureBoardCardWidgetBinding> GetBindings(
            AdventureBoardSide side)
        {
            return _bindingsBySide[side];
        }

        private IReadOnlyList<AdventureBoardCardPlacement> ReplaceSide(
            AdventureBoardSide side,
            IReadOnlyList<AdventureBoardCardViewModel> viewModels)
        {
            DisposeSide(side);

            List<AdventureBoardCardWidgetBinding> bindings = CreateWidgets(viewModels);
            List<VisualElement> elements = new(bindings.Count);
            for (int i = 0; i < bindings.Count; i++)
            {
                elements.Add(bindings[i].Element);
            }

            IReadOnlyList<AdventureBoardCardPlacement> placements =
                _layout.ReplaceCards(_widgets.GetArea(side), elements);

            for (int i = 0; i < placements.Count; i++)
            {
                bindings[i].BindPlacement(placements[i]);
            }

            _bindingsBySide[side] = bindings;
            return placements;
        }

        private List<AdventureBoardCardWidgetBinding> CreateWidgets(
            IReadOnlyList<AdventureBoardCardViewModel> viewModels)
        {
            List<AdventureBoardCardWidgetBinding> widgets = new(viewModels.Count);
            uint nextBoardCardId = 0;

            foreach (AdventureBoardCardViewModel viewModel in viewModels)
            {
                AdventureBoardCardWidgetBinding widget =
                    _cardWidgetFactory.Create(nextBoardCardId++, viewModel);

                widgets.Add(widget);
            }

            return widgets;
        }

        private void DisposeSide(AdventureBoardSide side)
        {
            List<AdventureBoardCardWidgetBinding> bindings = _bindingsBySide[side];
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                bindings[i].Dispose();
            }

            bindings.Clear();
        }
    }
}
