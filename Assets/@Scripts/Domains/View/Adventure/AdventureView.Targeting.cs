using System.Collections.Generic;
using Domains.View.Targeting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private readonly List<VisualElement> _targetingSlots = new();
        private readonly List<SkillTargetingManipulator> _targetingManipulators = new();

        private SkillTargetingManipulator _activeTargetingManipulator;
        private int _targetingSlotIndex = -1;

        private bool IsTargetingActive => _activeTargetingManipulator != null && _activeTargetingManipulator.IsActive;

        private void RegisterTargetingEvents()
        {
            VisualElement eventRoot = GetTargetingEventRoot();
            eventRoot?.RegisterCallback<PointerMoveEvent>(OnTargetingRootPointerMove);
            eventRoot?.RegisterCallback<PointerDownEvent>(OnTargetingRootPointerDown, TrickleDown.TrickleDown);

            IReadOnlyList<VisualElement> slots = _skillSlotWidget.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                VisualElement slot = slots[i];
                SkillTargetingManipulator manipulator = new(
                    i,
                    OnSkillTargetingStarted,
                    OnSkillTargetingMoved,
                    OnSkillTargetingReleased,
                    OnSkillTargetingCanceled);

                slot.AddManipulator(manipulator);
                _targetingSlots.Add(slot);
                _targetingManipulators.Add(manipulator);
            }
        }

        private void UnregisterTargetingEvents()
        {
            VisualElement eventRoot = GetTargetingEventRoot();
            eventRoot?.UnregisterCallback<PointerMoveEvent>(OnTargetingRootPointerMove);
            eventRoot?.UnregisterCallback<PointerDownEvent>(OnTargetingRootPointerDown, TrickleDown.TrickleDown);

            CancelTargeting();

            for (int i = 0; i < _targetingManipulators.Count; i++)
            {
                _targetingSlots[i].RemoveManipulator(_targetingManipulators[i]);
            }

            _targetingSlots.Clear();
            _targetingManipulators.Clear();
        }

        private void OnSkillTargetingStarted(
            SkillTargetingManipulator manipulator,
            int slotIndex,
            Vector2 origin,
            Vector2 position)
        {
            if (IsTargetingActive)
            {
                bool restartTargeting = _targetingSlotIndex != slotIndex;
                CancelTargeting();

                if (!restartTargeting)
                    return;
            }

            _activeTargetingManipulator = manipulator;
            _targetingSlotIndex = slotIndex;
            _arrowWidget.Show(origin);
            _arrowWidget.Update(position);
        }

        private void OnSkillTargetingMoved(Vector2 position)
        {
            if (!IsTargetingActive)
                return;

            _arrowWidget.Update(position);
        }

        private void OnSkillTargetingReleased(Vector2 position)
        {
            if (!IsTargetingActive)
                return;

            _arrowWidget.Update(position);
        }

        private void OnSkillTargetingCanceled()
        {
            CancelTargeting();
        }

        private void OnTargetingRootPointerMove(PointerMoveEvent evt)
        {
            if (!IsTargetingActive)
                return;

            _arrowWidget.Update(evt.position);
        }

        private void OnTargetingRootPointerDown(PointerDownEvent evt)
        {
            if (!IsTargetingActive)
                return;

            if (evt.button == (int)MouseButton.RightMouse)
            {
                CancelTargeting();
                evt.StopPropagation();
                return;
            }

            if (_activeTargetingManipulator.State == ESkillTargetingState.Holding)
                return;

            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            CancelTargeting();
            evt.StopPropagation();
        }

        private void CancelTargeting()
        {
            _activeTargetingManipulator?.Cancel();
            _activeTargetingManipulator = null;
            _targetingSlotIndex = -1;
            _arrowWidget?.Hide();
        }

        private VisualElement GetTargetingEventRoot()
        {
            return Root.Q<VisualElement>("adventure-root") ?? Root;
        }
    }
}
