using System;
using Domains.Adventure;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Owns one board card widget's UI/runtime bindings until the board side is replaced or cleared.
    public sealed class AdventureBoardCardWidgetBinding : IDisposable
    {
        private readonly Action<AdventureBoardCardPlacement> _onPlaced;
        private Action _dispose;

        public AdventureBoardCardWidgetBinding(
            AdventureBoardCardViewModel viewModel,
            VisualElement element,
            Action<AdventureBoardCardPlacement> onPlaced,
            Action dispose,
            bool isInteractive = true,
            VisualElement interactionTarget = null)
        {
            ViewModel = viewModel;
            Element = element;
            _onPlaced = onPlaced;
            _dispose = dispose;
            IsInteractive = isInteractive;
            InteractionTarget = interactionTarget ?? element;

            if (IsInteractive && InteractionTarget == null)
                throw new ArgumentNullException(nameof(interactionTarget));

            if (IsInteractive)
                InteractionTarget.pickingMode = PickingMode.Position;
        }

        public AdventureBoardCardViewModel ViewModel { get; }
        public VisualElement Element { get; }
        public VisualElement InteractionTarget { get; }
        public bool IsInteractive { get; }
        public AdventureBoardCardPlacement Placement { get; private set; }

        public void BindPlacement(AdventureBoardCardPlacement placement)
        {
            Placement = placement;
            _onPlaced?.Invoke(placement);
        }

        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}
