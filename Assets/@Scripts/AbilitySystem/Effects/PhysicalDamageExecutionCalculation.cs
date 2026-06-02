using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Effects
{
    [CreateAssetMenu(
        menuName = "Game/AbilitySystem/Effects/Executions/Physical Damage Execution")]
    public sealed class PhysicalDamageExecutionCalculation : GameplayEffectExecutionCalculation
    {
        private static readonly GameplayEffectAttributeCaptureDefinition SourcePhysicalAttackCapture =
            new(
                CombatAttributeSet.PhysicalAttackAttribute,
                GameplayEffectAttributeCaptureSource.Source,
                snapshot: false);

        [SerializeField]
        private float _coefficient = 1f;

        [SerializeField]
        private float _flatBonus;

        [SerializeField]
        private float _minimumDamage;

        public override void GetAttributeCaptureDefinitions(
            List<GameplayEffectAttributeCaptureDefinition> definitions)
        {
            definitions.Add(SourcePhysicalAttackCapture);
        }

        public override void Execute(
            GameplayEffectExecutionParameters parameters,
            GameplayEffectExecutionOutput output)
        {
            if (!parameters.AttemptCalculateCapturedAttributeMagnitude(
                    SourcePhysicalAttackCapture,
                    out float physicalAttack))
            {
                physicalAttack = 0f;
            }

            float damage = physicalAttack * _coefficient + _flatBonus;
            damage = Mathf.Max(_minimumDamage, damage);

            if (damage <= 0f)
                return;

            output.AddOutputModifier(
                new GameplayModifierEvaluatedData(
                    VitalAttributeSet.IncomingDamageAttribute,
                    GameplayModifierOperation.Add,
                    damage));
        }
    }
}
