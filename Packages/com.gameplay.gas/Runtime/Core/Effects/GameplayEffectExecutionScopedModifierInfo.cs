using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayEffectExecutionScopedModifierInfo
    {
        [SerializeField]
        private GameplayEffectScopedModifierAggregatorType _aggregatorType =
            GameplayEffectScopedModifierAggregatorType.CapturedAttributeBacked;

        [SerializeField]
        private GameplayEffectAttributeCaptureReference _capturedAttribute = new();

        [SerializeField]
        private GameplayTag _transientAggregatorIdentifier;

        [SerializeField]
        private GameplayModifierOperation _operation = GameplayModifierOperation.Add;

        [SerializeField]
        private GameplayEffectModifierMagnitude _modifierMagnitude =
            GameplayEffectModifierMagnitude.Fixed(0f);

        [SerializeField]
        private GameplayTagRequirements _sourceTagRequirements = new();

        [SerializeField]
        private GameplayTagRequirements _targetTagRequirements = new();

        public GameplayEffectScopedModifierAggregatorType AggregatorType => _aggregatorType;
        public GameplayTag TransientAggregatorIdentifier => _transientAggregatorIdentifier;
        public GameplayModifierOperation Operation => _operation;
        public GameplayEffectModifierMagnitude ModifierMagnitude => _modifierMagnitude;
        public GameplayTagRequirements SourceTagRequirements => _sourceTagRequirements;
        public GameplayTagRequirements TargetTagRequirements => _targetTagRequirements;

        public bool TryGetCapturedAttribute(out GameplayEffectAttributeCaptureDefinition definition)
        {
            definition = default;

            return _aggregatorType == GameplayEffectScopedModifierAggregatorType.CapturedAttributeBacked &&
                   _capturedAttribute != null &&
                   _capturedAttribute.TryBuild(out definition);
        }

        public void GetAttributeCaptureDefinitions(
            List<GameplayEffectAttributeCaptureDefinition> definitions)
        {
            if (TryGetCapturedAttribute(out GameplayEffectAttributeCaptureDefinition definition))
                definitions.Add(definition);

            _modifierMagnitude?.GetAttributeCaptureDefinitions(definitions);
        }

        public bool RequirementsMet(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            bool sourceRequirementsMet = source == null ||
                                         _sourceTagRequirements == null ||
                                         _sourceTagRequirements.RequirementsMet(source.OwnedTags);

            bool targetRequirementsMet = target == null ||
                                         _targetTagRequirements == null ||
                                         _targetTagRequirements.RequirementsMet(target.OwnedTags);

            return sourceRequirementsMet && targetRequirementsMet;
        }

        public bool TryCalculateModifierMagnitude(
            GameplayEffectExecutionParameters parameters,
            out float magnitude)
        {
            if (_modifierMagnitude == null)
            {
                magnitude = 0f;
                return false;
            }

            return _modifierMagnitude.TryCalculateMagnitude(parameters.Spec, out magnitude);
        }
    }
}
