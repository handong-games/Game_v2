namespace Gameplay.GAS
{
    public readonly struct GameplayAttributeBasedFloat
    {
        public GameplayAttributeBasedFloat(
            GameplayEffectAttributeCaptureDefinition captureDefinition,
            float coefficient = 1f,
            float preMultiplyAdditiveValue = 0f,
            float postMultiplyAdditiveValue = 0f)
        {
            CaptureDefinition = captureDefinition;
            Coefficient = coefficient;
            PreMultiplyAdditiveValue = preMultiplyAdditiveValue;
            PostMultiplyAdditiveValue = postMultiplyAdditiveValue;
        }

        public GameplayEffectAttributeCaptureDefinition CaptureDefinition { get; }
        public float Coefficient { get; }
        public float PreMultiplyAdditiveValue { get; }
        public float PostMultiplyAdditiveValue { get; }
        public bool IsValid => CaptureDefinition.IsValid;

        public float Evaluate(float capturedMagnitude)
        {
            return Coefficient * (PreMultiplyAdditiveValue + capturedMagnitude) +
                   PostMultiplyAdditiveValue;
        }
    }
}
