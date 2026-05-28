using Game.AbilitySystem.Abilities;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.Localization;

namespace Domains.View.Widgets
{
    public readonly struct AdventureSkillSlotViewModel : IReadOnlySkillSlotViewModel
    {
        public AdventureSkillSlotViewModel(
            LocalizedString name,
            Sprite icon,
            GameplayAbilitySpecHandle handle,
            ESkillTargetType targetType)
        {
            Name = name;
            Icon = icon;
            Handle = handle;
            TargetType = targetType;
        }

        public LocalizedString Name { get; }
        public Sprite Icon { get; }
        public GameplayAbilitySpecHandle Handle { get; }
        public ESkillTargetType TargetType { get; }
    }
}
