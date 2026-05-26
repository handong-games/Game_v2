namespace Gameplay.GAS
{
    public sealed class GameplayEffectModCallbackData
    {
        public GameplayEffectModCallbackData(
            GameplayEffectSpec effectSpec,
            GameplayModifierEvaluatedData evaluatedData,
            AbilitySystemComponent target)
        {
            EffectSpec = effectSpec;
            EvaluatedData = evaluatedData;
            Target = target;
        }

        public GameplayEffectSpec EffectSpec { get; }
        public GameplayModifierEvaluatedData EvaluatedData { get; }
        public AbilitySystemComponent Target { get; }
    }
}
