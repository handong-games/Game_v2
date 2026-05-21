using Gameplay.GAS;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.AbilitySystem.Abilities
{
    public abstract class SkillGameplayAbility : GameplayAbility
    {
        [SerializeField]
        private LocalizedString _name;

        [SerializeField]
        private Sprite _icon;

        [SerializeField]
        private ESkillTargetType _targetType;

        public LocalizedString Name => _name;
        public Sprite Icon => _icon;
        public ESkillTargetType TargetType => _targetType;
    }
}
