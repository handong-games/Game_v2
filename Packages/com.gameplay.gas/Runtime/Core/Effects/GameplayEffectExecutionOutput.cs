using System.Collections.Generic;

namespace Gameplay.GAS
{
    public sealed class GameplayEffectExecutionOutput
    {
        private readonly List<GameplayModifierEvaluatedData> _modifiers = new();

        public IReadOnlyList<GameplayModifierEvaluatedData> Modifiers => _modifiers;

        public void AddOutputModifier(GameplayModifierEvaluatedData modifier)
        {
            if (modifier.Attribute.IsValid)
                _modifiers.Add(modifier);
        }
    }
}
