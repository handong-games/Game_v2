using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayEffectExecutionDefinition
    {
        public GameplayEffectExecutionDefinition()
        {
        }

        public GameplayEffectExecutionDefinition(
            GameplayEffectExecutionCalculation calculation,
            GameplayTagContainer passedInTags = null)
        {
            _calculation = calculation;
            if (passedInTags != null)
                _passedInTags = passedInTags;
        }

        [SerializeField]
        private GameplayEffectExecutionCalculation _calculation;

        [SerializeField]
        private GameplayTagContainer _passedInTags = new();

        [SerializeField]
        private List<GameplayEffectExecutionScopedModifierInfo> _calculationModifiers = new();

        [SerializeField]
        private List<ConditionalGameplayEffect> _conditionalGameplayEffects = new();

        public GameplayEffectExecutionCalculation Calculation => _calculation;
        public GameplayTagContainer PassedInTags => _passedInTags;
        public IReadOnlyList<GameplayEffectExecutionScopedModifierInfo> CalculationModifiers =>
            _calculationModifiers != null
                ? _calculationModifiers
                : Array.Empty<GameplayEffectExecutionScopedModifierInfo>();
        public IReadOnlyList<ConditionalGameplayEffect> ConditionalGameplayEffects =>
            _conditionalGameplayEffects != null
                ? _conditionalGameplayEffects
                : Array.Empty<ConditionalGameplayEffect>();

        public static GameplayEffectExecutionDefinition Create(
            GameplayEffectExecutionCalculation calculation,
            GameplayTagContainer passedInTags = null)
        {
            return new GameplayEffectExecutionDefinition(calculation, passedInTags);
        }

        public void GetAttributeCaptureDefinitions(
            List<GameplayEffectAttributeCaptureDefinition> definitions)
        {
            _calculation?.GetAttributeCaptureDefinitions(definitions);

            if (_calculationModifiers == null)
                return;

            for (int i = 0; i < _calculationModifiers.Count; i++)
            {
                _calculationModifiers[i]?.GetAttributeCaptureDefinitions(definitions);
            }
        }
    }
}
