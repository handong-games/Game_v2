using System.Collections.Generic;
using Domains.Adventure;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class AdventureSkillSlotGroup : ToggleButtonGroup
    {
        private const string HiddenClass = "ui-transition--hidden";
        private const string FromBottomClass = "ui-transition--from-bottom";
        private const string EnterClass = "ui-transition--enter";

        private readonly List<SkillSlotWidget> _slots = new();
        private AdventureSkillSlotWidgetEvents _events;
        private bool _isShown;

        public IReadOnlyList<SkillSlotWidget> Slots => _slots;

        public AdventureSkillSlotGroup()
        {
            focusable = false;
            isMultipleSelection = false;
            allowEmptySelection = true;
            RegisterCallback<ChangeEvent<ToggleButtonGroupState>>(OnSelectionChanged);

            AddToClassList("skill-slot-group");
        }

        public void Bind(
            IReadOnlyList<AdventureSkillSlotViewModel> skillSlots,
            AdventureSkillSlotWidgetEvents events)
        {
            _events = events;
            int count = skillSlots?.Count ?? 0;
            EnsureSlotCount(count);

            SetValueWithoutNotify(new ToggleButtonGroupState(0ul, _slots.Count));

            for (int i = 0; i < _slots.Count; i++)
            {
                bool visible = i < count;
                SkillSlotWidget slot = _slots[i];
                slot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

                if (!visible)
                {
                    slot.Unbind();
                    continue;
                }

                slot.Bind(skillSlots[i]);
            }
        }

        public void Unbind()
        {
            _events = null;
        }

        public async Awaitable Show()
        {
            if (_isShown)
                return;

            _isShown = true;
            RemoveFromClassList(HiddenClass);
            await ViewTransitionManager.Instance.Play(this, EnterClass);
        }

        public void Hide()
        {
            _isShown = false;
            SetHidden();
        }

        private void OnSelectionChanged(ChangeEvent<ToggleButtonGroupState> evt)
        {
            int selectedIndex = FindSelectedIndex(evt.newValue);
            SkillSlotWidget selectedButton = GetSelectedButton(selectedIndex);
            _events?.SelectionChanged?.Invoke(selectedIndex, selectedButton);
        }

        private SkillSlotWidget GetSelectedButton(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= _slots.Count)
                return null;

            return _slots[selectedIndex];
        }

        private void OnSlotPointerDown(PointerDownEvent evt)
        {
            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if (evt.currentTarget is not SkillSlotWidget selectedButton)
                return;

            int selectedIndex = _slots.IndexOf(selectedButton);
            if (selectedIndex < 0)
                return;

            if (!selectedButton.enabledInHierarchy)
            {
                evt.StopImmediatePropagation();
                return;
            }

            int currentIndex = FindSelectedIndex(value);
            int nextIndex = currentIndex == selectedIndex && allowEmptySelection ? -1 : selectedIndex;
            value = CreateSelectionState(nextIndex);

            evt.StopImmediatePropagation();
        }

        private static int FindSelectedIndex(ToggleButtonGroupState state)
        {
            for (int i = 0; i < state.length; i++)
            {
                if (state[i])
                    return i;
            }

            return -1;
        }

        private ToggleButtonGroupState CreateSelectionState(int selectedIndex)
        {
            ulong mask = selectedIndex < 0 ? 0ul : 1ul << selectedIndex;
            return new ToggleButtonGroupState(mask, _slots.Count);
        }

        private void EnsureSlotCount(int count)
        {
            while (_slots.Count < count)
            {
                SkillSlotWidget slot = new();
                slot.AddToClassList("skill-slot-group__slot");
                slot.RegisterCallback<PointerDownEvent>(OnSlotPointerDown, TrickleDown.TrickleDown);
                slot.AvailableChanged += OnSlotAvailableChanged;
                _slots.Add(slot);
                Add(slot);
            }
        }

        private void OnSlotAvailableChanged(SkillSlotWidget slot, bool isAvailable)
        {
            if (isAvailable)
                return;

            int slotIndex = _slots.IndexOf(slot);
            if (slotIndex < 0)
                return;

            int selectedIndex = FindSelectedIndex(value);
            if (selectedIndex != slotIndex)
                return;

            value = CreateSelectionState(-1);
        }

        private void SetHidden()
        {
            RemoveFromClassList(EnterClass);
            AddToClassList(HiddenClass);
            AddToClassList(FromBottomClass);
        }
    }
}
