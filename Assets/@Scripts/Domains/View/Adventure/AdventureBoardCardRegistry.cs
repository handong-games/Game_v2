using System;
using System.Collections.Generic;
using Domains.Adventure;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Stores the board card UI bindings currently placed on the Adventure board.
    // It only answers lookup questions; layout and bounds calculation stay outside.
    public sealed class AdventureBoardCardRegistry : IDisposable
    {
        private readonly Dictionary<AdventureBoardSide, List<AdventureBoardCardWidgetBinding>> _bindingsBySide = new()
        {
            { AdventureBoardSide.Left, new List<AdventureBoardCardWidgetBinding>() },
            { AdventureBoardSide.Right, new List<AdventureBoardCardWidgetBinding>() },
        };

        public IReadOnlyList<AdventureBoardCardWidgetBinding> GetBindings(AdventureBoardSide side)
        {
            return _bindingsBySide[side];
        }

        public List<AdventureBoardCardWidgetBinding> GetMutableBindings(AdventureBoardSide side)
        {
            return _bindingsBySide[side];
        }

        public void SetBindings(
            AdventureBoardSide side,
            List<AdventureBoardCardWidgetBinding> bindings)
        {
            _bindingsBySide[side] = bindings ?? new List<AdventureBoardCardWidgetBinding>();
        }

        public bool TryGetBinding(
            uint cardId,
            out AdventureBoardCardWidgetBinding binding)
        {
            return TryGetBinding(AdventureBoardSide.Left, cardId, out binding) ||
                   TryGetBinding(AdventureBoardSide.Right, cardId, out binding);
        }

        public bool TryGetBinding(
            AdventureBoardSide side,
            uint cardId,
            out AdventureBoardCardWidgetBinding binding)
        {
            if (TryFindBindingIndex(side, cardId, out int index))
            {
                binding = _bindingsBySide[side][index];
                return true;
            }

            binding = null;
            return false;
        }

        public bool TryFindBindingIndex(
            AdventureBoardSide side,
            uint cardId,
            out int index)
        {
            List<AdventureBoardCardWidgetBinding> bindings = _bindingsBySide[side];
            for (int i = 0; i < bindings.Count; i++)
            {
                AdventureBoardCardWidgetBinding current = bindings[i];
                if (current.ViewModel.CardId != cardId)
                    continue;

                index = i;
                return true;
            }

            index = -1;
            return false;
        }

        public bool TryGetCardElement(
            uint cardId,
            out VisualElement element)
        {
            if (TryGetBinding(cardId, out AdventureBoardCardWidgetBinding binding))
            {
                element = binding.Element;
                return element != null;
            }

            element = null;
            return false;
        }

        public bool TryGetCardWidget<T>(
            AdventureBoardSide side,
            uint cardId,
            out T widget)
            where T : class
        {
            if (TryGetBinding(side, cardId, out AdventureBoardCardWidgetBinding binding))
            {
                widget = binding.Element as T;
                return widget != null;
            }

            widget = null;
            return false;
        }

        public void DisposeSide(AdventureBoardSide side)
        {
            List<AdventureBoardCardWidgetBinding> bindings = _bindingsBySide[side];
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                bindings[i].Dispose();
            }

            bindings.Clear();
        }

        public void Dispose()
        {
            DisposeSide(AdventureBoardSide.Left);
            DisposeSide(AdventureBoardSide.Right);
        }
    }
}
