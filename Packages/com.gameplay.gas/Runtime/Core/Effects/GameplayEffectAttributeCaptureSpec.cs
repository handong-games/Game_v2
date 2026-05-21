namespace Gameplay.GAS
{
    public readonly struct GameplayEffectAttributeCaptureSpec
    {
        public GameplayEffectAttributeCaptureSpec(
            GameplayEffectAttributeCaptureDefinition definition,
            float baseValue,
            float currentValue)
        {
            Definition = definition;
            BaseValue = baseValue;
            CurrentValue = currentValue;
        }

        public GameplayEffectAttributeCaptureDefinition Definition { get; }
        public float BaseValue { get; }
        public float CurrentValue { get; }
    }
}
