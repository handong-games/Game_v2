using System.Collections.Generic;

namespace Gameplay.GAS
{
    public sealed class GameplayEffectExecutionOutput
    {
        private readonly List<GameplayModifierEvaluatedData> _modifiers = new();

        public IReadOnlyList<GameplayModifierEvaluatedData> Modifiers => _modifiers;
        public bool ShouldTriggerConditionalGameplayEffects { get; private set; }
        public bool IsStackCountHandledManually { get; private set; }
        public bool AreGameplayCuesHandledManually { get; private set; }

        public void AddOutputModifier(GameplayModifierEvaluatedData modifier)
        {
            if (modifier.Attribute.IsValid)
                _modifiers.Add(modifier);
        }

        public void MarkConditionalGameplayEffectsToTrigger()
        {
            ShouldTriggerConditionalGameplayEffects = true;
        }

        public void MarkStackCountHandledManually()
        {
            IsStackCountHandledManually = true;
        }

        public void MarkGameplayCuesHandledManually()
        {
            AreGameplayCuesHandledManually = true;
        }
    }
}
