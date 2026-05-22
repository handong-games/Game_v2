using System.Collections.Generic;
using Domains.View.Targeting;
using Domains.View.Widgets;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private readonly List<SkillTargetingManipulator> _skillTargetingManipulators = new();
        private readonly List<VisualElement> _skillTargetingSlots = new();

        private SkillSlotWidget _skillSlotWidget;
        private SkillTargetingManipulator _activeSkillTargetingManipulator;
        private GameplayAbilitySpecHandle[] _skillSlotHandles;
        private int _activeSkillSlotIndex = -1;

        private bool IsSkillTargetingActive => _activeSkillTargetingManipulator != null
            && _activeSkillTargetingManipulator.IsActive;

        private void BindSkillSlots(IReadOnlyList<SkillSlotViewModelV2> skillSlots)
        {
            _skillSlotWidget = Root.Q<SkillSlotWidget>("skill-slot-widget");

            int count = skillSlots?.Count ?? 0;
            _skillSlotHandles = new GameplayAbilitySpecHandle[count];

            for (int i = 0; i < count; i++)
            {
                _skillSlotHandles[i] = skillSlots[i].Handle;
            }

            _skillSlotWidget.BindV2(skillSlots);
        }

        private bool TryGetSkillHandle(int slotIndex, out GameplayAbilitySpecHandle handle)
        {
            handle = GameplayAbilitySpecHandle.Invalid;

            if (_skillSlotHandles == null)
                return false;

            if (slotIndex < 0 || slotIndex >= _skillSlotHandles.Length)
                return false;

            handle = _skillSlotHandles[slotIndex];
            return handle != GameplayAbilitySpecHandle.Invalid;
        }

        private Awaitable ShowSkillSlots()
        {
            return _skillSlotWidget.Show();
        }

        private void RegisterTargetingEvents()
        {
            VisualElement eventRoot = GetTargetingEventRoot();
            eventRoot.RegisterCallback<PointerMoveEvent>(OnSkillTargetingRootPointerMove);
            eventRoot.RegisterCallback<PointerDownEvent>(OnSkillTargetingRootPointerDown, TrickleDown.TrickleDown);

            IReadOnlyList<VisualElement> slots = _skillSlotWidget.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                SkillTargetingManipulator manipulator = new(
                    i,
                    OnSkillTargetingStarted,
                    OnSkillTargetingMoved,
                    OnSkillTargetingReleased,
                    OnSkillTargetingCanceled);

                slots[i].AddManipulator(manipulator);
                _skillTargetingSlots.Add(slots[i]);
                _skillTargetingManipulators.Add(manipulator);
            }
        }

        private void UnregisterTargetingEvents()
        {
            VisualElement eventRoot = GetTargetingEventRoot();
            eventRoot.UnregisterCallback<PointerMoveEvent>(OnSkillTargetingRootPointerMove);
            eventRoot.UnregisterCallback<PointerDownEvent>(OnSkillTargetingRootPointerDown, TrickleDown.TrickleDown);

            CancelSkillTargeting();

            for (int i = 0; i < _skillTargetingManipulators.Count; i++)
            {
                _skillTargetingSlots[i].RemoveManipulator(_skillTargetingManipulators[i]);
            }

            _skillTargetingSlots.Clear();
            _skillTargetingManipulators.Clear();
        }

        private VisualElement GetTargetingEventRoot()
        {
            return Root.Q<VisualElement>("adventure-root") ?? Root;
        }

        private void OnSkillTargetingStarted(
            SkillTargetingManipulator manipulator,
            int selectedSkillSlotIndex,
            Vector2 origin,
            Vector2 position)
        {
            // Selecting the active slot again cancels targeting.
            // Selecting a different slot cancels the old targeting and evaluates the new skill.
            if (IsSkillTargetingActive)
            {
                bool restartTargeting = _activeSkillSlotIndex != selectedSkillSlotIndex;
                CancelSkillTargeting();

                if (!restartTargeting)
                    return;
            }

            SkillSelectionDto selection = _controller.TrySelectSkill(selectedSkillSlotIndex);

            // Empty, locked, or otherwise unusable slots should not start any interaction.
            if (!selection.CanUse)
            {
                manipulator.Cancel();
                return;
            }

            // Skills that do not require a target are executed immediately without showing the arrow.
            if (!selection.RequiresTarget)
            {
                manipulator.Cancel();
                _controller.UseSkill(selectedSkillSlotIndex);
                return;
            }

            // Targeted skills enter targeting mode and keep updating the arrow until confirm/cancel.
            _activeSkillTargetingManipulator = manipulator;
            _activeSkillSlotIndex = selectedSkillSlotIndex;
            _arrowWidget.Show(origin);
            UpdateSkillTargeting(position);
        }

        private void OnSkillTargetingMoved(Vector2 position)
        {
            UpdateSkillTargeting(position);
        }

        private void OnSkillTargetingReleased(Vector2 position)
        {
            UpdateSkillTargeting(position);
        }

        private void OnSkillTargetingCanceled()
        {
            CancelSkillTargeting();
        }

        private void OnSkillTargetingRootPointerMove(PointerMoveEvent evt)
        {
            UpdateSkillTargeting(evt.position);
        }

        private void OnSkillTargetingRootPointerDown(PointerDownEvent evt)
        {
            if (!IsSkillTargetingActive)
                return;

            if (evt.button == (int)MouseButton.RightMouse)
            {
                CancelSkillTargeting();
                evt.StopPropagation();
                return;
            }

            if (_activeSkillTargetingManipulator.State == ESkillTargetingState.Holding)
                return;

            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (IsSkillSlotEventTarget(evt.target))
                return;

            if (_hoveredCard != null)
                return;

            CancelSkillTargeting();
            evt.StopPropagation();
        }

        private void UpdateSkillTargeting(Vector2 pointerPosition)
        {
            if (!IsSkillTargetingActive)
                return;

            _arrowWidget.Update(pointerPosition);
        }

        private void ConfirmSkillTarget()
        {
            if (_hoveredCard == null)
                return;

            if (!_cardDealer.TryGetCardId(_hoveredCard, out uint cardId))
                return;

            if (!_controller.UseSkillOnTarget(_activeSkillSlotIndex, cardId))
                return;

            CancelSkillTargeting();
        }

        private void CancelSkillTargeting()
        {
            SetHoveredCard(null);

            _activeSkillTargetingManipulator?.Cancel();
            _activeSkillTargetingManipulator = null;
            _activeSkillSlotIndex = -1;

            _arrowWidget.Hide();
        }

        private bool IsSkillSlotEventTarget(IEventHandler eventTarget)
        {
            VisualElement element = eventTarget as VisualElement;
            while (element != null)
            {
                if (_skillTargetingSlots.Contains(element))
                    return true;

                element = element.parent;
            }

            return false;
        }
    }
}
