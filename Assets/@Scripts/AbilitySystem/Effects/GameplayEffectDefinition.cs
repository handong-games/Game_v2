using System.Collections.Generic;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Effects
{
    [CreateAssetMenu(
        fileName = "GE_Definition",
        menuName = "Game/AbilitySystem/Effects/Gameplay Effect Definition")]
    public sealed class GameplayEffectDefinition : ScriptableObject
    {
        [SerializeField]
        private List<GameplayModifierDefinition> _modifiers = new();

        public IReadOnlyList<GameplayModifierDefinition> Modifiers => _modifiers;

        public GameplayEffect CreateRuntimeEffect()
        {
            GameplayEffect effect = GameplayEffect.Create();

            for (int i = 0; i < _modifiers.Count; i++)
            {
                GameplayModifierDefinition definition = _modifiers[i];
                if (!definition.TryBuild(out GameplayModifier modifier))
                    continue;

                effect.AddModifier(modifier);
            }

            return effect;
        }
    }
}
