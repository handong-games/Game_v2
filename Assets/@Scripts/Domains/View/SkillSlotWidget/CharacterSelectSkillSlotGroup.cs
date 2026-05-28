using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class CharacterSelectSkillSlotGroup : VisualElement
    {
        private readonly List<SkillSlotWidget> _slots = new();

        public CharacterSelectSkillSlotGroup()
        {
            AddToClassList("skill-slot-group");
        }

        public void Bind(IReadOnlyList<Domains.CharacterSelect.CharacterSelectSkillSlotViewModel> skillSlots)
        {
            int count = skillSlots?.Count ?? 0;
            EnsureSlotCount(count);

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
    }
}
