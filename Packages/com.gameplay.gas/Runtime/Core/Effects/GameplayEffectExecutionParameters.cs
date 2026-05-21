namespace Gameplay.GAS
{
    public sealed class GameplayEffectExecutionParameters
    {
        public GameplayEffectExecutionParameters(GameplayEffectSpec spec, int stackCount = 1)
        {
            Spec = spec;
            StackCount = stackCount;
        }

        public GameplayEffectSpec Spec { get; }
        public GameplayEffectContext Context => Spec.Context;
        public AbilitySystemComponent Source => Context?.Source;
        public AbilitySystemComponent Target => Context?.Target;
        public int StackCount { get; }

        public float GetSetByCallerMagnitude(GameplayTag tag, float defaultValue = 0f)
        {
            return Spec.GetSetByCallerMagnitude(tag, defaultValue);
        }

        public bool TryGetSourceAttribute(GameplayAttribute attribute, out GameplayAttributeData data)
        {
            if (Source != null)
                return Source.TryGetAttributeData(attribute, out data);

            data = null;
            return false;
        }

        public bool TryGetTargetAttribute(GameplayAttribute attribute, out GameplayAttributeData data)
        {
            if (Target != null)
                return Target.TryGetAttributeData(attribute, out data);

            data = null;
            return false;
        }

        public bool AttemptCalculateCapturedAttributeMagnitude(
            GameplayEffectAttributeCaptureDefinition definition,
            out float magnitude)
        {
            if (!definition.IsValid)
            {
                magnitude = 0f;
                return false;
            }

            if (definition.Snapshot)
                return Spec.TryGetCapturedAttributeMagnitude(definition, out magnitude);

            AbilitySystemComponent component =
                definition.Source == GameplayEffectAttributeCaptureSource.Source ? Source : Target;

            if (component != null &&
                component.TryGetAttributeData(definition.Attribute, out GameplayAttributeData data))
            {
                magnitude = data.CurrentValue;
                return true;
            }

            magnitude = 0f;
            return false;
        }
    }
}
