using System;
using System.Collections.Generic;
using Domains.View.Widgets;
using Game.AbilitySystem.Abilities;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Owns the skill-targeting UI state for AdventureScreen.
    // It controls selected skill state, target hover styling, and arrow presentation.
    public sealed class AdventureSkillUIFlow
    {
        private const string TargetHoverClass = "card--target-hover";

        private IReadOnlyList<AdventureSkillSlotViewModel> _skillSlots = Array.Empty<AdventureSkillSlotViewModel>();
        private AdventureSkillSlotGroup _skillSlotGroup;
        private VisualElement _targetingEventRoot;
        private ArrowWidget _arrowWidget;
        private VisualElement _hoveredCard;

        public GameplayAbilitySpecHandle ActiveSkillHandle { get; private set; } = GameplayAbilitySpecHandle.Invalid;
        public bool IsTargetingActive { get; private set; }

        public void Bind(
            IReadOnlyList<AdventureSkillSlotViewModel> skillSlots,
            AdventureSkillSlotGroup skillSlotGroup,
            VisualElement targetingEventRoot,
            ArrowWidget arrowWidget)
        {
            if (skillSlots == null)
                throw new ArgumentNullException(nameof(skillSlots));

            if (skillSlotGroup == null)
                throw new ArgumentNullException(nameof(skillSlotGroup));

            if (targetingEventRoot == null)
                throw new ArgumentNullException(nameof(targetingEventRoot));

            if (arrowWidget == null)
                throw new ArgumentNullException(nameof(arrowWidget));

            _skillSlots = skillSlots;
            _skillSlotGroup = skillSlotGroup;
            _targetingEventRoot = targetingEventRoot;
            _arrowWidget = arrowWidget;
        }

        public AdventureSkillSelectionResult SelectSkill(int selectedIndex, SkillSlotWidget selectedButton)
        {
            if (!TryGetSkillSlot(selectedIndex, out AdventureSkillSlotViewModel skillSlot))
            {
                Clear();
                return AdventureSkillSelectionResult.None;
            }

            ActiveSkillHandle = skillSlot.Handle;

            if (skillSlot.TargetType == ESkillTargetType.None)
            {
                BeginPreviewOnlySkill();
                return AdventureSkillSelectionResult.ActivateImmediately(skillSlot.Handle);
            }

            BeginTargetingSkill(selectedButton);
            return AdventureSkillSelectionResult.WaitForTarget(skillSlot.Handle);
        }

        public void HoverCard(VisualElement card)
        {
            if (!IsTargetingActive)
                return;

            SetHoveredCard(card);
        }

        public void LeaveCard(VisualElement card)
        {
            if (_hoveredCard != card)
                return;

            SetHoveredCard(null);
        }

        public bool TryGetHoveredCard(out VisualElement card)
        {
            card = _hoveredCard;
            return card != null;
        }

        public void Clear()
        {
            UnregisterTargetingEvents();

            ActiveSkillHandle = GameplayAbilitySpecHandle.Invalid;
            IsTargetingActive = false;
            _arrowWidget?.Hide();
            SetHoveredCard(null);

            if (_skillSlotGroup != null)
                _skillSlotGroup.SetValueWithoutNotify(new ToggleButtonGroupState(0ul, _skillSlotGroup.Slots.Count));
        }

        public void Unbind()
        {
            Clear();

            _skillSlots = Array.Empty<AdventureSkillSlotViewModel>();
            _skillSlotGroup = null;
            _targetingEventRoot = null;
            _arrowWidget = null;
            _hoveredCard = null;
        }

        private void BeginPreviewOnlySkill()
        {
            IsTargetingActive = false;
            UnregisterTargetingEvents();
            _arrowWidget?.Hide();
            SetHoveredCard(null);
        }

        private void BeginTargetingSkill(SkillSlotWidget selectedButton)
        {
            if (selectedButton == null)
                throw new ArgumentNullException(nameof(selectedButton));

            if (_targetingEventRoot == null)
                throw new InvalidOperationException("Targeting event root is not bound.");

            if (_arrowWidget == null)
                throw new InvalidOperationException("Arrow widget is not bound.");

            IsTargetingActive = true;

            _targetingEventRoot.UnregisterCallback<PointerMoveEvent>(_arrowWidget.Update);
            _targetingEventRoot.RegisterCallback<PointerMoveEvent>(_arrowWidget.Update);

            _targetingEventRoot.UnregisterCallback<PointerDownEvent>(OnTargetingPointerDown, TrickleDown.TrickleDown);
            _targetingEventRoot.RegisterCallback<PointerDownEvent>(OnTargetingPointerDown, TrickleDown.TrickleDown);

            _arrowWidget.Show(GetArrowOrigin(selectedButton));
        }

        private void OnTargetingPointerDown(PointerDownEvent evt)
        {
            if (evt.button == (int)MouseButton.RightMouse)
            {
                Clear();
                evt.StopPropagation();
                return;
            }

            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (IsSkillSlotEventTarget(evt.target))
                return;

            if (_hoveredCard != null)
                return;

            Clear();
            evt.StopPropagation();
        }

        private void UnregisterTargetingEvents()
        {
            if (_targetingEventRoot == null || _arrowWidget == null)
                return;

            _targetingEventRoot.UnregisterCallback<PointerMoveEvent>(_arrowWidget.Update);
            _targetingEventRoot.UnregisterCallback<PointerDownEvent>(OnTargetingPointerDown, TrickleDown.TrickleDown);
        }

        private bool TryGetSkillSlot(int index, out AdventureSkillSlotViewModel skillSlot)
        {
            skillSlot = default;

            if (index < 0 || index >= _skillSlots.Count)
                return false;

            skillSlot = _skillSlots[index];
            return skillSlot.Handle != GameplayAbilitySpecHandle.Invalid;
        }

        private bool IsSkillSlotEventTarget(IEventHandler eventTarget)
        {
            VisualElement element = eventTarget as VisualElement;
            while (element != null)
            {
                if (element == _skillSlotGroup)
                    return true;

                element = element.parent;
            }

            return false;
        }

        private void SetHoveredCard(VisualElement card)
        {
            if (_hoveredCard == card)
                return;

            _hoveredCard?.RemoveFromClassList(TargetHoverClass);
            _hoveredCard = card;
            _hoveredCard?.AddToClassList(TargetHoverClass);
        }

        private static Vector2 GetArrowOrigin(VisualElement element)
        {
            Rect bounds = element.worldBound;

            return new Vector2(
                bounds.center.x,
                bounds.yMin + bounds.height * 0.25f);
        }
    }

    // Role:
    // Reports what AdventureSkillUIFlow decided after a skill slot selection.
    // It lets AdventureView keep gameplay execution responsibility while UIFlow owns UI state.
    public readonly struct AdventureSkillSelectionResult
    {
        private AdventureSkillSelectionResult(
            GameplayAbilitySpecHandle handle,
            bool shouldActivateImmediately)
        {
            Handle = handle;
            ShouldActivateImmediately = shouldActivateImmediately;
        }

        public GameplayAbilitySpecHandle Handle { get; }
        public bool ShouldActivateImmediately { get; }
        public bool HasSkill => Handle != GameplayAbilitySpecHandle.Invalid;

        public static AdventureSkillSelectionResult None =>
            new(GameplayAbilitySpecHandle.Invalid, false);

        public static AdventureSkillSelectionResult ActivateImmediately(
            GameplayAbilitySpecHandle handle)
        {
            return new AdventureSkillSelectionResult(handle, true);
        }

        public static AdventureSkillSelectionResult WaitForTarget(
            GameplayAbilitySpecHandle handle)
        {
            return new AdventureSkillSelectionResult(handle, false);
        }
    }
}
