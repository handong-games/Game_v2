namespace Gameplay.GAS
{
    public readonly struct GameplayModifier
    {
        public GameplayModifier(GameplayAttribute attribute, GameplayModifierOperation operation, float magnitude)
        {
            Attribute = attribute;
            Operation = operation;
            Magnitude = magnitude;
            MagnitudeType = GameplayModifierMagnitudeType.Fixed;
            SetByCallerTag = default;
            AttributeBasedMagnitude = default;
            ModifierMagnitude = GameplayEffectModifierMagnitude.Fixed(magnitude);
        }

        public GameplayModifier(GameplayAttribute attribute, GameplayModifierOperation operation, GameplayTag setByCallerTag)
        {
            Attribute = attribute;
            Operation = operation;
            Magnitude = 0f;
            MagnitudeType = GameplayModifierMagnitudeType.SetByCaller;
            SetByCallerTag = setByCallerTag;
            AttributeBasedMagnitude = default;
            ModifierMagnitude = GameplayEffectModifierMagnitude.SetByCaller(setByCallerTag);
        }

        public GameplayModifier(
            GameplayAttribute attribute,
            GameplayModifierOperation operation,
            GameplayAttributeBasedFloat attributeBasedMagnitude)
        {
            Attribute = attribute;
            Operation = operation;
            Magnitude = 0f;
            MagnitudeType = GameplayModifierMagnitudeType.AttributeBased;
            SetByCallerTag = default;
            AttributeBasedMagnitude = attributeBasedMagnitude;
            ModifierMagnitude = GameplayEffectModifierMagnitude.AttributeBased(attributeBasedMagnitude);
        }

        public GameplayModifier(
            GameplayAttribute attribute,
            GameplayModifierOperation operation,
            GameplayEffectModifierMagnitude modifierMagnitude)
        {
            Attribute = attribute;
            Operation = operation;

            ModifierMagnitude = modifierMagnitude ?? GameplayEffectModifierMagnitude.Fixed(0f);
            MagnitudeType = ModifierMagnitude.MagnitudeType;
            Magnitude = ModifierMagnitude.FixedMagnitude;
            SetByCallerTag = ModifierMagnitude.SetByCallerTag;
            AttributeBasedMagnitude = ModifierMagnitude.TryGetAttributeBasedMagnitude(
                out GameplayAttributeBasedFloat attributeBasedMagnitude)
                ? attributeBasedMagnitude
                : default;
        }

        public GameplayAttribute Attribute { get; }
        public GameplayModifierOperation Operation { get; }
        public float Magnitude { get; }
        public GameplayModifierMagnitudeType MagnitudeType { get; }
        public GameplayTag SetByCallerTag { get; }
        public GameplayAttributeBasedFloat AttributeBasedMagnitude { get; }
        public GameplayEffectModifierMagnitude ModifierMagnitude { get; }
    }
}
