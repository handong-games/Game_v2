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
        }

        public GameplayModifier(GameplayAttribute attribute, GameplayModifierOperation operation, GameplayTag setByCallerTag)
        {
            Attribute = attribute;
            Operation = operation;
            Magnitude = 0f;
            MagnitudeType = GameplayModifierMagnitudeType.SetByCaller;
            SetByCallerTag = setByCallerTag;
            AttributeBasedMagnitude = default;
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
        }

        public GameplayAttribute Attribute { get; }
        public GameplayModifierOperation Operation { get; }
        public float Magnitude { get; }
        public GameplayModifierMagnitudeType MagnitudeType { get; }
        public GameplayTag SetByCallerTag { get; }
        public GameplayAttributeBasedFloat AttributeBasedMagnitude { get; }
    }
}
