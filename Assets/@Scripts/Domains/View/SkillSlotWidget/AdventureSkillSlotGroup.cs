using System;
using System.Collections.Generic;
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
        private bool _isShown;

        public IReadOnlyList<SkillSlotWidget> Slots => _slots;
        public event Action<int, SkillSlotWidget> SelectionChanged;

        public AdventureSkillSlotGroup()
        {
            focusable = false;
            isMultipleSelection = false;
            allowEmptySelection = true;
            RegisterCallback<ChangeEvent<ToggleButtonGroupState>>(OnSelectionChanged);

            AddToClassList("skill-slot-group");
        }

        public void Bind(IReadOnlyList<AdventureSkillSlotViewModel> skillSlots)
        {
            int count = skillSlots?.Count ?? 0;
            EnsureSlotCount(count);

            SetValueWithoutNotify(new ToggleButtonGroupState(0ul, _slots.Count));

            for (int i = 0; i < _slots.Count; i++)
            {
                bool visible = i < count;
                SkillSlotWidget slot = _slots[i];
                slot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

                if (!visible)
                    continue;

                slot.Bind(skillSlots[i]);
            }
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
            SelectionChanged?.Invoke(selectedIndex, selectedButton);
        }

        private SkillSlotWidget GetSelectedButton(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= _slots.Count)
                return null;

            return _slots[selectedIndex];
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

        private void EnsureSlotCount(int count)
        {
            while (_slots.Count < count)
            {
                SkillSlotWidget slot = new();
                slot.AddToClassList("skill-slot-group__slot");
                _slots.Add(slot);
                Add(slot);
            }
        }

        private void SetHidden()
        {
            RemoveFromClassList(EnterClass);
            AddToClassList(HiddenClass);
            AddToClassList(FromBottomClass);
        }
    }
}
