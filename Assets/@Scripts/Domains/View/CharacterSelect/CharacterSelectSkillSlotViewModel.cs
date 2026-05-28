using Domains.View.Widgets;
using UnityEngine;
using UnityEngine.Localization;

namespace Domains.CharacterSelect
{
    public sealed class CharacterSelectSkillSlotViewModel : IReadOnlySkillSlotViewModel
    {
        public CharacterSelectSkillSlotViewModel(
            LocalizedString name,
            Sprite icon)
        {
            Name = name;
            Icon = icon;
        }

        public LocalizedString Name { get; }
        public Sprite Icon { get; }
    }
}
