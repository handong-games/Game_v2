using System;
using System.Collections.Generic;
using Domains.View.Widgets;
using Game.AbilitySystem.Abilities;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private IReadOnlyList<AdventureSkillSlotViewModel> _skillSlots = Array.Empty<AdventureSkillSlotViewModel>();
        private AdventureSkillSlotGroup _skillSlotGroup;
        private VisualElement _targetingEventRoot;
        private GameplayAbilitySpecHandle _activeSkillHandle = GameplayAbilitySpecHandle.Invalid;
        private int _activeSkillIndex = -1;
        private bool IsSkillTargetingActive { get; set; }

        private void BindSkillSlots(IReadOnlyList<AdventureSkillSlotViewModel> skillSlots)
        {
            _skillSlots = skillSlots ?? Array.Empty<AdventureSkillSlotViewModel>();
            _targetingEventRoot = Root.Q<VisualElement>("adventure-root") ?? Root;
            _skillSlotGroup = Root.Q<AdventureSkillSlotGroup>("skill-slot-group");
            _skillSlotGroup.Bind(_skillSlots);

            _skillSlotGroup.SelectionChanged -= OnSkillSlotSelectionChanged;
            _skillSlotGroup.SelectionChanged += OnSkillSlotSelectionChanged;
        }

        private Awaitable ShowSkillSlots()
        {
            return _skillSlotGroup.Show();
        }

        private void OnSkillSlotSelectionChanged(int selectedIndex, SkillSlotWidget selectedButton)
        {
            if (selectedIndex < 0)
            {
                ClearSkillPreview();
                return;
            }

            if (!TryGetSkillSlot(selectedIndex, out AdventureSkillSlotViewModel skillSlot))
            {
                ClearSkillPreview();
                return;
            }

            _activeSkillIndex = selectedIndex;
            _activeSkillHandle = skillSlot.Handle;

            if (skillSlot.TargetType == ESkillTargetType.None)
            {
                BeginPreviewOnlySkill();
                return;
            }

            BeginTargetingSkill(selectedButton);
        }

        private void BeginPreviewOnlySkill()
        {
            IsSkillTargetingActive = false;
            _targetingEventRoot?.UnregisterCallback<PointerMoveEvent>(_arrowWidget.Update);
            _targetingEventRoot?.UnregisterCallback<PointerDownEvent>(OnTargetingPointerDown, TrickleDown.TrickleDown);
            _arrowWidget.Hide();
            SetHoveredCard(null);
        }

        private void BeginTargetingSkill(SkillSlotWidget selectedButton)
        {
            if (selectedButton == null)
            {
                ClearSkillPreview();
                return;
            }

            IsSkillTargetingActive = true;

            _targetingEventRoot.UnregisterCallback<PointerMoveEvent>(_arrowWidget.Update);
            _targetingEventRoot.RegisterCallback<PointerMoveEvent>(_arrowWidget.Update);

            _targetingEventRoot.UnregisterCallback<PointerDownEvent>(OnTargetingPointerDown, TrickleDown.TrickleDown);
            _targetingEventRoot.RegisterCallback<PointerDownEvent>(OnTargetingPointerDown, TrickleDown.TrickleDown);

            Vector2 origin = GetArrowOrigin(selectedButton);
            _arrowWidget.Show(origin);
        }

        private void OnTargetingPointerDown(PointerDownEvent evt)
        {
            if (evt.button == (int)MouseButton.RightMouse)
            {
                ClearSkillPreview();
                evt.StopPropagation();
                return;
            }

            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (IsSkillSlotEventTarget(evt.target))
                return;

            if (_hoveredCard != null)
                return;

            ClearSkillPreview();
            evt.StopPropagation();
        }

        private bool TryGetSkillSlot(int index, out AdventureSkillSlotViewModel skillSlot)
        {
            skillSlot = default;

            if (index < 0 || index >= _skillSlots.Count)
                return false;

            skillSlot = _skillSlots[index];
            return skillSlot.Handle != GameplayAbilitySpecHandle.Invalid;
        }

        private void ConfirmSkillTarget()
        {
            if (_hoveredCard == null)
                return;

            if (!_cardDealer.TryGetCardId(_hoveredCard, out uint targetCardId))
                return;

            if (!_controller.UseSkillOnTarget(_activeSkillHandle, targetCardId))
                return;

            ClearSkillPreview();
        }

        private void ClearSkillPreview()
        {
            _targetingEventRoot?.UnregisterCallback<PointerMoveEvent>(_arrowWidget.Update);
            _targetingEventRoot?.UnregisterCallback<PointerDownEvent>(OnTargetingPointerDown, TrickleDown.TrickleDown);

            _activeSkillHandle = GameplayAbilitySpecHandle.Invalid;
            _activeSkillIndex = -1;
            IsSkillTargetingActive = false;
            _arrowWidget.Hide();
            SetHoveredCard(null);

            if (_skillSlotGroup != null)
                _skillSlotGroup.SetValueWithoutNotify(new ToggleButtonGroupState(0ul, _skillSlotGroup.Slots.Count));
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

        private static Vector2 GetArrowOrigin(VisualElement element)
        {
            Rect bounds = element.worldBound;

            return new Vector2(
                bounds.center.x,
                bounds.yMin + bounds.height * 0.25f);
        }
    }
}
