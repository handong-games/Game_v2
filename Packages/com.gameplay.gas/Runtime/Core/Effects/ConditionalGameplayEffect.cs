using System;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class ConditionalGameplayEffect
    {
        [SerializeField]
        private GameplayEffect _effect;

        [SerializeField]
        private GameplayTagRequirements _sourceTagRequirements = new();

        [SerializeField]
        private GameplayTagRequirements _targetTagRequirements = new();

        public GameplayEffect Effect => _effect;
        public GameplayTagRequirements SourceTagRequirements => _sourceTagRequirements;
        public GameplayTagRequirements TargetTagRequirements => _targetTagRequirements;

        public bool RequirementsMet(
            AbilitySystemComponent source,
            AbilitySystemComponent target)
        {
            if (_effect == null)
                return false;

            bool sourceRequirementsMet = source == null ||
                                         _sourceTagRequirements == null ||
                                         _sourceTagRequirements.RequirementsMet(source.OwnedTags);

            bool targetRequirementsMet = target == null ||
                                         _targetTagRequirements == null ||
                                         _targetTagRequirements.RequirementsMet(target.OwnedTags);

            return sourceRequirementsMet && targetRequirementsMet;
        }
    }
}
