using Game.Data;
using UnityEngine;

namespace Domains.View.Widgets
{
    public readonly struct SkillSlotViewModel
    {
        public SkillSlotViewModel(
            int slotIndex,
            string name,
            Sprite icon,
            SkillType skillType,
            bool isEmpty)
        {
            SlotIndex = slotIndex;
            Name = name;
            Icon = icon;
            SkillType = skillType;
            IsEmpty = isEmpty;
        }

        public int SlotIndex { get; }
        public string Name { get; }
        public Sprite Icon { get; }
        public SkillType SkillType { get; }
        public bool IsEmpty { get; }
    }
}
