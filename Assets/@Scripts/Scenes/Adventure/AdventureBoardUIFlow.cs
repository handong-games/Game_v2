using System.Collections.Generic;
using System;
using Domains.Adventure;
using Domains.View.Widgets;
using Game.Core.Managers.View;
using Game.Scenes.Adventure.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Coordinates Adventure board UI creation and placement.
    // Card ids come from ViewModels; this flow does not create UI-local ids.
    public sealed class AdventureBoardUIFlow : IDisposable
    {
        private readonly AdventureBoardWidgets _widgets;
        private readonly AdventureBoardLayout _layout;
        private readonly AdventureCardWidgetFactory _cardWidgetFactory;
        private readonly AdventureCardWidgetEventBinder _cardEventBinder;
        private readonly AdventureBoardCardRegistry _cardRegistry;

        public AdventureBoardUIFlow(
            AdventureBoardWidgets widgets,
            AdventureBoardLayout layout,
            AdventureCardWidgetFactory cardWidgetFactory,
            AdventureCardWidgetEventBinder cardEventBinder,
            AdventureBoardCardRegistry cardRegistry)
        {
            _widgets = widgets ?? throw new ArgumentNullException(nameof(widgets));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _cardWidgetFactory = cardWidgetFactory ?? throw new ArgumentNullException(nameof(cardWidgetFactory));
            _cardEventBinder = cardEventBinder ?? throw new ArgumentNullException(nameof(cardEventBinder));
            _cardRegistry = cardRegistry ?? throw new ArgumentNullException(nameof(cardRegistry));
        }

        public async Awaitable<AdventureBoardPlacementResult> ReplaceBoardWithEnter(
            IReadOnlyList<AdventureBoardCardViewModel> cards,
            VisualElement cardDeck,
            Func<bool> canContinue)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            if (cardDeck == null)
                throw new ArgumentNullException(nameof(cardDeck));

            SplitCards(
                cards,
                out List<AdventureBoardCardViewModel> leftViewModels,
                out List<AdventureBoardCardViewModel> rightViewModels);

            bool hasReplacementRequest = leftViewModels.Count > 0 || rightViewModels.Count > 0;
            if (hasReplacementRequest)
                _cardEventBinder.Unbind();

            IReadOnlyList<AdventureBoardCardPlacement> changedLeftPlacements =
                leftViewModels.Count > 0
                    ? await ReplaceSideWithEnter(AdventureBoardSide.Left, leftViewModels, cardDeck, canContinue)
                    : Array.Empty<AdventureBoardCardPlacement>();

            if (!CanContinue(canContinue))
            {
                return new AdventureBoardPlacementResult(
                    Array.Empty<AdventureBoardCardPlacement>(),
                    Array.Empty<AdventureBoardCardPlacement>());
            }

            IReadOnlyList<AdventureBoardCardPlacement> changedRightPlacements =
                rightViewModels.Count > 0
                    ? await ReplaceSideWithEnter(AdventureBoardSide.Right, rightViewModels, cardDeck, canContinue)
                    : Array.Empty<AdventureBoardCardPlacement>();

            if (!CanContinue(canContinue))
            {
                return new AdventureBoardPlacementResult(
                    Array.Empty<AdventureBoardCardPlacement>(),
                    Array.Empty<AdventureBoardCardPlacement>());
            }

            if (hasReplacementRequest)
                RefreshCardEventBindings();

            return new AdventureBoardPlacementResult(
                changedLeftPlacements,
                changedRightPlacements);
        }

        public async Awaitable<AdventureBoardPlacementResult> ReplaceBoardAfterExit(
            IReadOnlyList<AdventureBoardCardViewModel> cards)
        {
            return await ReplaceBoardAfterExit(cards, canContinue: null);
        }

        public async Awaitable<AdventureBoardPlacementResult> ReplaceBoardAfterExit(
            IReadOnlyList<AdventureBoardCardViewModel> cards,
            Func<bool> canContinue)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            SplitCards(
                cards,
                out List<AdventureBoardCardViewModel> leftViewModels,
                out List<AdventureBoardCardViewModel> rightViewModels);

            return await ReplaceBothSidesAfterExit(
                leftViewModels,
                rightViewModels,
                canContinue);
        }

        public async Awaitable<AdventureBoardPlacementResult> RefreshRightSideFromDeckAfterExit(
            IReadOnlyList<AdventureBoardCardViewModel> cards,
            VisualElement cardDeck,
            Func<bool> canContinue)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            if (cardDeck == null)
                throw new ArgumentNullException(nameof(cardDeck));

            SplitCards(
                cards,
                out _,
                out List<AdventureBoardCardViewModel> rightViewModels);

            SideReplacement right = null;
            try
            {
                right = CreateSideReplacement(
                    AdventureBoardSide.Right,
                    rightViewModels);
            }
            catch
            {
                right?.DisposeNewBindings();
                throw;
            }

            if (!right.ShouldReplace)
            {
                right.DisposeNewBindings();
                return new AdventureBoardPlacementResult(
                    Array.Empty<AdventureBoardCardPlacement>(),
                    Array.Empty<AdventureBoardCardPlacement>());
            }

            _cardEventBinder.Unbind();
            await PlaySideExit(AdventureBoardSide.Right);

            if (!CanContinue(canContinue))
            {
                right.DisposeNewBindings();
                ClearExitedSide(AdventureBoardSide.Right);
                return new AdventureBoardPlacementResult(
                    Array.Empty<AdventureBoardCardPlacement>(),
                    Array.Empty<AdventureBoardCardPlacement>());
            }

            bool applied = false;
            try
            {
                IReadOnlyList<AdventureBoardCardPlacement> rightPlacements =
                    ApplySideReplacement(right);
                applied = true;

                await _layout.PlayEnterFromDeck(rightPlacements, cardDeck);

                if (!CanContinue(canContinue))
                {
                    ClearExitedSide(AdventureBoardSide.Right);
                    right.ReleaseNewBindingsOwnership();
                    return new AdventureBoardPlacementResult(
                        Array.Empty<AdventureBoardCardPlacement>(),
                        Array.Empty<AdventureBoardCardPlacement>());
                }

                RefreshCardEventBindings();
                right.ReleaseNewBindingsOwnership();

                return new AdventureBoardPlacementResult(
                    Array.Empty<AdventureBoardCardPlacement>(),
                    rightPlacements);
            }
            catch
            {
                if (applied)
                {
                    ClearExitedSide(AdventureBoardSide.Right);
                    right.ReleaseNewBindingsOwnership();
                }
                else
                {
                    right.DisposeNewBindings();
                }

                RefreshCardEventBindings();
                throw;
            }
        }

        private static void SplitCards(
            IReadOnlyList<AdventureBoardCardViewModel> cards,
            out List<AdventureBoardCardViewModel> leftViewModels,
            out List<AdventureBoardCardViewModel> rightViewModels)
        {
            leftViewModels = new List<AdventureBoardCardViewModel>();
            rightViewModels = new List<AdventureBoardCardViewModel>();

            foreach (AdventureBoardCardViewModel card in cards)
            {
                if (card == null)
                    throw new ArgumentException("Board card view model list contains null.", nameof(cards));

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
        }

        private void ClearSide(AdventureBoardSide side)
        {
            ClearSideWithoutRefreshing(side);
            RefreshCardEventBindings();
        }

        private void ClearSideWithoutRefreshing(AdventureBoardSide side)
        {
            _cardEventBinder.Unbind();
            DisposeSide(side);

            if (!_widgets.IsInitialized)
                return;

            _layout.Clear(_widgets.GetArea(side));
        }

        public void ClearAll()
        {
            ClearSideWithoutRefreshing(AdventureBoardSide.Left);
            ClearSideWithoutRefreshing(AdventureBoardSide.Right);
        }

        public async Awaitable<bool> RemoveCardAfterExit(
            uint cardId,
            Func<bool> canContinue)
        {
            if (!CanContinue(canContinue))
                return false;

            if (TryFindBindingIndex(
                    AdventureBoardSide.Left,
                    cardId,
                    out int leftIndex))
            {
                return await RemoveBindingAfterExit(
                    AdventureBoardSide.Left,
                    leftIndex,
                    canContinue);
            }

            if (TryFindBindingIndex(
                    AdventureBoardSide.Right,
                    cardId,
                    out int rightIndex))
            {
                return await RemoveBindingAfterExit(
                    AdventureBoardSide.Right,
                    rightIndex,
                    canContinue);
            }

            throw new InvalidOperationException(
                $"Card id {cardId} was requested to exit, but it is not placed on the board UI.");
        }

        public async Awaitable<bool> PlayCardDeathAndRemove(
            uint cardId,
            Func<bool> canContinue)
        {
            if (!CanContinue(canContinue))
                return false;

            if (!TryFindBinding(
                    cardId,
                    out AdventureBoardSide side,
                    out int index))
            {
                return false;
            }

            List<AdventureBoardCardWidgetBinding> bindings = _cardRegistry.GetMutableBindings(side);
            AdventureBoardCardWidgetBinding binding = bindings[index];
            _cardEventBinder.Unbind();
            await _layout.PlayDeath(binding.Placement);

            int currentIndex = bindings.IndexOf(binding);
            if (currentIndex < 0)
            {
                if (CanContinue(canContinue))
                    RefreshCardEventBindings();

                return false;
            }

            RemoveBindingAt(side, currentIndex);

            if (!CanContinue(canContinue))
                return false;

            RelayoutSide(side);
            RefreshCardEventBindings();
            return true;
        }

        public async Awaitable<bool> ClearSideAfterExit(
            AdventureBoardSide side,
            Func<bool> canContinue)
        {
            _cardEventBinder.Unbind();
            await PlaySideExit(side);

            if (!CanContinue(canContinue))
            {
                ClearSideWithoutRefreshing(side);
                return false;
            }

            ClearSide(side);
            return true;
        }

        private bool TryGetBinding(
            uint cardId,
            out AdventureBoardCardWidgetBinding binding)
        {
            return _cardRegistry.TryGetBinding(cardId, out binding);
        }

        public bool TryGetCardId(
            VisualElement element,
            out uint cardId)
        {
            return _cardEventBinder.TryGetCardId(element, out cardId);
        }

        private bool TryGetBinding(
            AdventureBoardSide side,
            uint cardId,
            out AdventureBoardCardWidgetBinding binding)
        {
            return _cardRegistry.TryGetBinding(side, cardId, out binding);
        }

        private bool TryFindBindingIndex(
            AdventureBoardSide side,
            uint cardId,
            out int index)
        {
            return _cardRegistry.TryFindBindingIndex(side, cardId, out index);
        }

        private bool TryFindBinding(
            uint cardId,
            out AdventureBoardSide side,
            out int index)
        {
            if (TryFindBindingIndex(AdventureBoardSide.Left, cardId, out index))
            {
                side = AdventureBoardSide.Left;
                return true;
            }

            if (TryFindBindingIndex(AdventureBoardSide.Right, cardId, out index))
            {
                side = AdventureBoardSide.Right;
                return true;
            }

            side = default;
            index = -1;
            return false;
        }

        public bool TryGetCardWidget<T>(
            AdventureBoardSide side,
            uint cardId,
            out T widget)
            where T : class
        {
            return _cardRegistry.TryGetCardWidget(side, cardId, out widget);
        }

        public bool TrySetCardHealth(
            uint cardId,
            int currentHealth,
            int maxHealth)
        {
            if (!TryGetBinding(cardId, out AdventureBoardCardWidgetBinding binding))
                return false;

            if (binding.Element is not IAdventureHealthCardWidget healthCard)
                return false;

            healthCard.SetHealth(currentHealth, maxHealth);
            return true;
        }

        public async Awaitable ShowHealthBars(ViewTransitionManager transitionManager)
        {
            List<Awaitable> transitions = new();
            AddHealthBarTransitions(AdventureBoardSide.Left, transitionManager, transitions);
            AddHealthBarTransitions(AdventureBoardSide.Right, transitionManager, transitions);

            if (transitions.Count == 0)
            {
                await Awaitable.NextFrameAsync();
                return;
            }

            for (int i = 0; i < transitions.Count; i++)
            {
                await transitions[i];
            }
        }

        private void AddHealthBarTransitions(
            AdventureBoardSide side,
            ViewTransitionManager transitionManager,
            List<Awaitable> transitions)
        {
            IReadOnlyList<AdventureBoardCardWidgetBinding> bindings = _cardRegistry.GetBindings(side);
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].Element is not IAdventureHealthCardWidget healthCard)
                    continue;

                transitions.Add(healthCard.ShowHealthAsync(transitionManager));
            }
        }

        private IReadOnlyList<AdventureBoardCardPlacement> ReplaceSide(
            AdventureBoardSide side,
            IReadOnlyList<AdventureBoardCardViewModel> viewModels,
            bool prepareEnter)
        {
            if (IsSideUnchanged(side, viewModels))
                return Array.Empty<AdventureBoardCardPlacement>();

            VisualElement area = _widgets.GetArea(side);
            _layout.ValidateCardCount(viewModels.Count);
            List<AdventureBoardCardWidgetBinding> bindings = CreateWidgets(viewModels);
            List<VisualElement> elements = new(bindings.Count);
            for (int i = 0; i < bindings.Count; i++)
            {
                elements.Add(bindings[i].Element);
            }

            DisposeSide(side);

            try
            {
                IReadOnlyList<AdventureBoardCardPlacement> placements =
                    _layout.ReplaceCards(area, elements, prepareEnter);

                for (int i = 0; i < placements.Count; i++)
                {
                    bindings[i].BindPlacement(placements[i]);
                }

                _cardRegistry.SetBindings(side, bindings);
                return placements;
            }
            catch
            {
                ClearFailedSideReplacement(side, area, bindings);
                throw;
            }
        }

        private async Awaitable<IReadOnlyList<AdventureBoardCardPlacement>> ReplaceSideWithEnter(
            AdventureBoardSide side,
            IReadOnlyList<AdventureBoardCardViewModel> viewModels,
            VisualElement cardDeck,
            Func<bool> canContinue)
        {
            IReadOnlyList<AdventureBoardCardPlacement> placements =
                ReplaceSide(side, viewModels, prepareEnter: true);

            if (placements.Count == 0)
                return placements;

            if (!CanContinue(canContinue))
            {
                ClearSideWithoutRefreshing(side);
                return Array.Empty<AdventureBoardCardPlacement>();
            }

            try
            {
                await _layout.PlayEnterFromDeck(placements, cardDeck);
            }
            catch
            {
                ClearSide(side);
                throw;
            }

            if (!CanContinue(canContinue))
            {
                ClearSideWithoutRefreshing(side);
                return Array.Empty<AdventureBoardCardPlacement>();
            }

            return placements;
        }

        private void ClearFailedSideReplacement(
            AdventureBoardSide side,
            VisualElement area,
            List<AdventureBoardCardWidgetBinding> bindings)
        {
            _layout.Clear(area);
            DisposeBindings(bindings);

            RefreshCardEventBindings();
        }

        private async Awaitable<AdventureBoardPlacementResult> ReplaceBothSidesAfterExit(
            IReadOnlyList<AdventureBoardCardViewModel> leftViewModels,
            IReadOnlyList<AdventureBoardCardViewModel> rightViewModels,
            Func<bool> canContinue)
        {
            SideReplacement left = null;
            SideReplacement right = null;

            try
            {
                left = CreateSideReplacement(
                    AdventureBoardSide.Left,
                    leftViewModels);
                right = CreateSideReplacement(
                    AdventureBoardSide.Right,
                    rightViewModels);
            }
            catch
            {
                left?.DisposeNewBindings();
                right?.DisposeNewBindings();
                throw;
            }

            if (!left.ShouldReplace && !right.ShouldReplace)
            {
                left.DisposeNewBindings();
                right.DisposeNewBindings();
                return new AdventureBoardPlacementResult(
                    Array.Empty<AdventureBoardCardPlacement>(),
                    Array.Empty<AdventureBoardCardPlacement>());
            }

            _cardEventBinder.Unbind();

            List<Awaitable> exits = new(2);
            if (left.ShouldReplace)
                exits.Add(PlaySideExit(AdventureBoardSide.Left));
            if (right.ShouldReplace)
                exits.Add(PlaySideExit(AdventureBoardSide.Right));

            for (int i = 0; i < exits.Count; i++)
            {
                await exits[i];
            }

            if (!CanContinue(canContinue))
            {
                left.DisposeNewBindings();
                right.DisposeNewBindings();
                ClearExitedSides(left, right);
                return new AdventureBoardPlacementResult(
                    Array.Empty<AdventureBoardCardPlacement>(),
                    Array.Empty<AdventureBoardCardPlacement>());
            }

            try
            {
                IReadOnlyList<AdventureBoardCardPlacement> leftPlacements =
                    ApplySideReplacement(left);
                IReadOnlyList<AdventureBoardCardPlacement> rightPlacements =
                    ApplySideReplacement(right);

                List<Awaitable> entries = new(2);
                if (leftPlacements.Count > 0)
                    entries.Add(_layout.PlayEnter(leftPlacements));
                if (rightPlacements.Count > 0)
                    entries.Add(_layout.PlayEnter(rightPlacements));

                for (int i = 0; i < entries.Count; i++)
                {
                    await entries[i];
                }

                if (!CanContinue(canContinue))
                {
                    _cardEventBinder.Unbind();
                    ClearExitedSides(left, right);
                    left.ReleaseNewBindingsOwnership();
                    right.ReleaseNewBindingsOwnership();
                    return new AdventureBoardPlacementResult(
                        Array.Empty<AdventureBoardCardPlacement>(),
                        Array.Empty<AdventureBoardCardPlacement>());
                }

                RefreshCardEventBindings();

                left.ReleaseNewBindingsOwnership();
                right.ReleaseNewBindingsOwnership();

                return new AdventureBoardPlacementResult(
                    leftPlacements,
                    rightPlacements);
            }
            catch
            {
                if (left.ShouldReplace)
                {
                    _layout.Clear(_widgets.GetArea(AdventureBoardSide.Left));
                    left.DisposeNewBindings();
                }

                if (right.ShouldReplace)
                {
                    _layout.Clear(_widgets.GetArea(AdventureBoardSide.Right));
                    right.DisposeNewBindings();
                }

                RefreshCardEventBindings();
                throw;
            }
        }

        private SideReplacement CreateSideReplacement(
            AdventureBoardSide side,
            IReadOnlyList<AdventureBoardCardViewModel> viewModels)
        {
            _layout.ValidateCardCount(viewModels.Count);

            if (viewModels.Count == 0)
            {
                return SideReplacement.Keep(side);
            }

            if (IsSideUnchanged(side, viewModels))
            {
                return SideReplacement.Keep(side);
            }

            List<AdventureBoardCardWidgetBinding> newBindings = CreateWidgets(viewModels);

            return SideReplacement.Replace(side, newBindings);
        }

        private IReadOnlyList<AdventureBoardCardPlacement> ApplySideReplacement(
            SideReplacement replacement)
        {
            if (!replacement.ShouldReplace)
                return Array.Empty<AdventureBoardCardPlacement>();

            AdventureBoardSide side = replacement.Side;
            VisualElement area = _widgets.GetArea(side);
            DisposeSide(side);

            if (replacement.NewBindings.Count == 0)
            {
                _layout.Clear(area);
                return Array.Empty<AdventureBoardCardPlacement>();
            }

            List<VisualElement> elements = new(replacement.NewBindings.Count);
            for (int i = 0; i < replacement.NewBindings.Count; i++)
            {
                elements.Add(replacement.NewBindings[i].Element);
            }

            IReadOnlyList<AdventureBoardCardPlacement> placements =
                _layout.ReplaceCards(area, elements, prepareEnter: true);

            for (int i = 0; i < placements.Count; i++)
            {
                replacement.NewBindings[i].BindPlacement(placements[i]);
            }

            _cardRegistry.SetBindings(side, replacement.NewBindings);
            return placements;
        }

        private void ClearExitedSides(
            SideReplacement left,
            SideReplacement right)
        {
            if (left?.ShouldReplace == true)
                ClearExitedSide(left.Side);

            if (right?.ShouldReplace == true)
                ClearExitedSide(right.Side);
        }

        private void ClearExitedSide(AdventureBoardSide side)
        {
            DisposeSide(side);

            if (!_widgets.IsInitialized)
                return;

            _layout.Clear(_widgets.GetArea(side));
        }

        private List<AdventureBoardCardWidgetBinding> CreateWidgets(
            IReadOnlyList<AdventureBoardCardViewModel> viewModels)
        {
            List<AdventureBoardCardWidgetBinding> widgets = new(viewModels.Count);

            try
            {
                foreach (AdventureBoardCardViewModel viewModel in viewModels)
                {
                    AdventureBoardCardWidgetBinding widget =
                        _cardWidgetFactory.Create(viewModel);

                    widget.Element.AddToClassList("card-board__card");
                    widgets.Add(widget);
                }
            }
            catch
            {
                DisposeBindings(widgets);
                widgets.Clear();
                throw;
            }

            return widgets;
        }

        private bool IsSideUnchanged(
            AdventureBoardSide side,
            IReadOnlyList<AdventureBoardCardViewModel> viewModels)
        {
            IReadOnlyList<AdventureBoardCardWidgetBinding> currentBindings = _cardRegistry.GetBindings(side);
            if (currentBindings.Count != viewModels.Count)
                return false;

            for (int i = 0; i < currentBindings.Count; i++)
            {
                if (!IsSameCardPresentation(currentBindings[i].ViewModel, viewModels[i]))
                    return false;
            }

            return true;
        }

        private static bool IsSameCardPresentation(
            AdventureBoardCardViewModel current,
            AdventureBoardCardViewModel next)
        {
            if (current == null || next == null)
                return false;

            if (current.GetType() != next.GetType())
                return false;

            if (current.Side != next.Side || current.CardId != next.CardId)
                return false;

            if (current is AdventureChoiceCardViewModel currentChoice &&
                next is AdventureChoiceCardViewModel nextChoice)
            {
                return currentChoice.ChoiceType == nextChoice.ChoiceType;
            }

            return true;
        }

        private void DisposeSide(AdventureBoardSide side)
        {
            _cardRegistry.DisposeSide(side);
        }

        private async Awaitable PlaySideExit(AdventureBoardSide side)
        {
            IReadOnlyList<AdventureBoardCardWidgetBinding> bindings = _cardRegistry.GetBindings(side);
            if (bindings.Count == 0)
            {
                await Awaitable.NextFrameAsync();
                return;
            }

            List<Awaitable> exits = new(bindings.Count);
            for (int i = 0; i < bindings.Count; i++)
            {
                exits.Add(_layout.PlayExit(bindings[i].Placement));
            }

            for (int i = 0; i < exits.Count; i++)
            {
                await exits[i];
            }
        }

        private async Awaitable<bool> RemoveBindingAfterExit(
            AdventureBoardSide side,
            int index,
            Func<bool> canContinue)
        {
            List<AdventureBoardCardWidgetBinding> bindings = _cardRegistry.GetMutableBindings(side);
            if (index < 0 || index >= bindings.Count)
                return false;

            AdventureBoardCardWidgetBinding binding = bindings[index];
            _cardEventBinder.Unbind();
            await _layout.PlayExit(binding.Placement);

            int currentIndex = bindings.IndexOf(binding);
            if (currentIndex < 0)
            {
                if (CanContinue(canContinue))
                    RefreshCardEventBindings();

                return false;
            }

            bindings.RemoveAt(currentIndex);
            DisposeRemovedBinding(binding);

            if (!CanContinue(canContinue))
                return false;

            RelayoutSide(side);
            RefreshCardEventBindings();
            return true;
        }

        private void RemoveBindingAt(
            AdventureBoardSide side,
            int index)
        {
            List<AdventureBoardCardWidgetBinding> bindings = _cardRegistry.GetMutableBindings(side);
            if (index < 0 || index >= bindings.Count)
                return;

            AdventureBoardCardWidgetBinding binding = bindings[index];
            bindings.RemoveAt(index);
            DisposeRemovedBinding(binding);
        }

        private static void DisposeRemovedBinding(
            AdventureBoardCardWidgetBinding binding)
        {
            binding?.Dispose();
            binding?.Placement?.Slot?.RemoveFromHierarchy();
        }

        private void RelayoutSide(AdventureBoardSide side)
        {
            IReadOnlyList<AdventureBoardCardWidgetBinding> bindings = _cardRegistry.GetBindings(side);
            if (bindings.Count == 0)
                return;

            List<AdventureBoardCardPlacement> placements = new(bindings.Count);
            for (int i = 0; i < bindings.Count; i++)
            {
                placements.Add(bindings[i].Placement);
            }

            _layout.RelayoutSlots(placements);
        }

        private static void DisposeBindings(
            List<AdventureBoardCardWidgetBinding> bindings)
        {
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                bindings[i].Dispose();
            }
        }

        private void RefreshCardEventBindings()
        {
            _cardEventBinder.Unbind();
            _cardEventBinder.Bind(_cardRegistry.GetBindings(AdventureBoardSide.Left));
            _cardEventBinder.Bind(_cardRegistry.GetBindings(AdventureBoardSide.Right));
        }

        public void Dispose()
        {
            _cardEventBinder.Unbind();
            DisposeSideForShutdown(AdventureBoardSide.Left);
            DisposeSideForShutdown(AdventureBoardSide.Right);
        }

        private void DisposeSideForShutdown(AdventureBoardSide side)
        {
            _cardRegistry.DisposeSide(side);

            if (!_widgets.IsInitialized)
                return;

            _layout.Clear(_widgets.GetArea(side));
        }

        private static bool CanContinue(Func<bool> canContinue)
        {
            return canContinue == null || canContinue.Invoke();
        }

        private sealed class SideReplacement
        {
            private readonly List<AdventureBoardCardWidgetBinding> _newBindings;
            private bool _ownsNewBindings;

            private SideReplacement(
                AdventureBoardSide side,
                bool shouldReplace,
                List<AdventureBoardCardWidgetBinding> newBindings,
                bool ownsNewBindings)
            {
                Side = side;
                ShouldReplace = shouldReplace;
                _newBindings = newBindings;
                _ownsNewBindings = ownsNewBindings;
            }

            public AdventureBoardSide Side { get; }
            public bool ShouldReplace { get; }
            public List<AdventureBoardCardWidgetBinding> NewBindings => _newBindings;

            public static SideReplacement Keep(AdventureBoardSide side)
            {
                return new SideReplacement(
                    side,
                    false,
                    new List<AdventureBoardCardWidgetBinding>(),
                    ownsNewBindings: false);
            }

            public static SideReplacement Replace(
                AdventureBoardSide side,
                List<AdventureBoardCardWidgetBinding> newBindings)
            {
                return new SideReplacement(
                    side,
                    true,
                    newBindings,
                    ownsNewBindings: true);
            }

            public void DisposeNewBindings()
            {
                if (!_ownsNewBindings || _newBindings == null)
                    return;

                DisposeBindings(_newBindings);
                _newBindings.Clear();
                _ownsNewBindings = false;
            }

            public void ReleaseNewBindingsOwnership()
            {
                _ownsNewBindings = false;
            }
        }
    }
}
